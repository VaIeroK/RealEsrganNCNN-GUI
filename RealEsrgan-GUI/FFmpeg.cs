using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RealEsrgan_GUI
{
    internal enum FFmpegQuality
    {
        Low,
        Medium,
        High
    }

    /// <summary>
    /// Utility class for working with ffmpeg (unpack video to PNG frames and pack PNG frames to video).
    /// Requires ffmpeg.exe available at the provided path.
    /// </summary>
    internal class FFmpeg
    {
        private readonly string _ffmpegPath;
        private const long DefaultMinDiskBytes = 512L * 1024L * 1024L; // 512 MB

        public FFmpeg(string ffmpegPath)
        {
            if (string.IsNullOrWhiteSpace(ffmpegPath))
                throw new ArgumentException("FFmpeg path is not specified.", nameof(ffmpegPath));

            _ffmpegPath = Path.GetFullPath(PathResolver.GetFilePath(ffmpegPath));
            if (!File.Exists(_ffmpegPath))
                throw new FileNotFoundException("ffmpeg executable not found.", _ffmpegPath);
        }

        /// <summary>
        /// Unpacks a video into a sequence of PNG files in the outputFolder.
        /// Returns detected FPS (rounded to nearest integer).
        /// Frames are written as frame_00000001.png, frame_00000002.png, ...
        /// Quality controls PNG compression level (0..9): High -> 0 (less compression), Low -> 9 (max compression).
        /// This method checks available disk space before starting and will throw InvalidOperationException if insufficient.
        /// </summary>
        public async Task<int> UnpackAsync(string inputFile, string outputFolder, FFmpegQuality quality = FFmpegQuality.Medium)
        {
            if (string.IsNullOrWhiteSpace(inputFile)) throw new ArgumentException(nameof(inputFile));
            if (!File.Exists(inputFile)) throw new FileNotFoundException("Input file not found.", inputFile);
            if (string.IsNullOrWhiteSpace(outputFolder)) throw new ArgumentException(nameof(outputFolder));

            Directory.CreateDirectory(outputFolder);

            // Basic disk-space check: require at least DefaultMinDiskBytes free on drive containing outputFolder
            long required = DiskHelpers.EstimateRequiredForUnpack(new FileInfo(inputFile).Length);
            if (!DiskHelpers.HasEnoughDiskSpaceForPath(outputFolder, required))
                throw new InvalidOperationException($"Insufficient disk space to unpack video. Required approx: {required} bytes.");

            double fps = await ProbeFpsAsync(inputFile).ConfigureAwait(false);

            // output pattern with 8 digits to match project conventions
            string outPattern = Path.Combine(outputFolder, "frame_%08d.png");

            // Map quality to png compression level (0-9). High->0, Medium->6, Low->9
            int pngCompression = MapPngCompression(quality);

            // -vsync 0 preserves original timestamps (no frame duplication/drop)
            // -compression_level sets PNG encoder compression (applies to png output)
            string args = $"-hide_banner -loglevel error -i \"{inputFile}\" -vsync 0 -compression_level {pngCompression} \"{outPattern}\"";

            var result = await RunProcessAsync(args).ConfigureAwait(false);
            int code = result.Item1;
            string stderr = result.Item3;

            if (code != 0)
                throw new InvalidOperationException($"ffmpeg failed to unpack. ExitCode={code}. Error: {stderr}");

            return (int)Math.Round(fps);
        }

        /// <summary>
        /// Packs PNG files (expected names frame_XXXXXXXX.png) from inputFolder into outputFile with specified fps.
        /// Returns ffmpeg exit code (0 = success).
        /// Quality controls encoder parameters (CRF/bitrate).
        /// This method checks available disk space before starting and will throw InvalidOperationException if insufficient.
        /// </summary>
        public async Task<int> PackAsync(string inputFolder, string outputFile, int fps, FFmpegQuality quality = FFmpegQuality.Medium)
        {
            if (string.IsNullOrWhiteSpace(inputFolder)) throw new ArgumentException(nameof(inputFolder));
            if (!Directory.Exists(inputFolder)) throw new DirectoryNotFoundException(inputFolder);
            if (string.IsNullOrWhiteSpace(outputFile)) throw new ArgumentException(nameof(outputFile));
            if (fps <= 0) throw new ArgumentOutOfRangeException(nameof(fps));

            // Basic disk-space check: ensure target drive has room for output (estimate)
            string outputDir = Path.GetDirectoryName(Path.GetFullPath(outputFile));
            if (string.IsNullOrEmpty(outputDir)) outputDir = Directory.GetCurrentDirectory();

            long inputFramesSize = DiskHelpers.GetDirectorySizeBytes(inputFolder);
            long required = Math.Max(inputFramesSize + (100L * 1024L * 1024L), DefaultMinDiskBytes);
            if (!DiskHelpers.HasEnoughDiskSpaceForPath(outputDir, required))
                throw new InvalidOperationException($"Insufficient disk space to pack video. Required approx: {required} bytes.");

            // choose codec/settings based on output extension and quality
            string ext = Path.GetExtension(outputFile).ToLowerInvariant();
            string codecArgs = GetCodecArgsForExtension(ext, quality);

            // input pattern expects frame_00000001.png etc.
            string inputPattern = Path.Combine(inputFolder, "frame_%08d.png");
            string args = $"-hide_banner -loglevel error -y -framerate {fps} -i \"{inputPattern}\" {codecArgs} \"{outputFile}\"";

            var result = await RunProcessAsync(args).ConfigureAwait(false);
            return result.Item1;
        }

        private int MapPngCompression(FFmpegQuality quality)
        {
            switch (quality)
            {
                case FFmpegQuality.High: return 0; // minimal compression / best quality
                case FFmpegQuality.Medium: return 6;
                case FFmpegQuality.Low: return 9; // more compression / smaller files
                default: return 6;
            }
        }

        private string GetCodecArgsForExtension(string ext, FFmpegQuality quality)
        {
            // Map quality to CRF / bitrate values
            int crfHigh = 18, crfMed = 23, crfLow = 28;
            int crf = quality == FFmpegQuality.High ? crfHigh : (quality == FFmpegQuality.Low ? crfLow : crfMed);

            switch (ext)
            {
                case ".mp4":
                case ".mov":
                case ".mkv":
                    // libx264 — broad compatibility
                    return $"-c:v libx264 -preset veryfast -crf {crf} -pix_fmt yuv420p";
                case ".webm":
                    // libvpx quality mapping via bitrate
                    // High -> 5M, Med -> 2M, Low -> 800k
                    string bitrate = quality == FFmpegQuality.High ? "5M" : (quality == FFmpegQuality.Low ? "800k" : "2M");
                    return $"-c:v libvpx -b:v {bitrate} -pix_fmt yuv420p";
                case ".avi":
                    // use libx264 for avi with different crf
                    return $"-c:v libx264 -preset veryfast -crf {Math.Min(crf + 2, 35)} -pix_fmt yuv420p";
                default:
                    // default to libx264
                    return $"-c:v libx264 -preset veryfast -crf {crf} -pix_fmt yuv420p";
            }
        }

        /// <summary>
        /// Attempts to detect video FPS by parsing ffmpeg -i output.
        /// Returns double fps (not rounded). If detection fails returns 25.0.
        /// </summary>
        public async Task<double> ProbeFpsAsync(string inputFile)
        {
            // call ffmpeg for probing; ffmpeg writes stream info to stderr
            string args = $"-hide_banner -i \"{inputFile}\"";
            var result = await RunProcessAsync(args).ConfigureAwait(false);
            string stderr = result.Item3 ?? string.Empty;

            // priority 1: explicit "XX fps"
            var m = Regex.Match(stderr, @"(?<fps>\d+(?:\.\d+)?)\s+fps", RegexOptions.IgnoreCase);
            if (m.Success && double.TryParse(m.Groups["fps"].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double fps))
                return fps;

            // fallback: "tbr"
            m = Regex.Match(stderr, @"(?<fps>\d+(?:\.\d+)?)\s+tbr", RegexOptions.IgnoreCase);
            if (m.Success && double.TryParse(m.Groups["fps"].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out fps))
                return fps;

            // fallback: r_frame_rate or avg_frame_rate like "24000/1001"
            m = Regex.Match(stderr, @"(r_frame_rate|avg_frame_rate)\s*[:=]?\s*(?<num>\d+)\s*/\s*(?<den>\d+)", RegexOptions.IgnoreCase);
            if (m.Success && int.TryParse(m.Groups["num"].Value, out int num) && int.TryParse(m.Groups["den"].Value, out int den) && den != 0)
                return (double)num / den;

            // default fallback
            return 25.0;
        }

        /// <summary>
        /// Runs ffmpeg with provided arguments and returns Tuple(exitCode, stdout, stderr).
        /// </summary>
        private async Task<Tuple<int, string, string>> RunProcessAsync(string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (var proc = new Process { StartInfo = psi, EnableRaisingEvents = true })
            {
                var sbOut = new StringBuilder();
                var sbErr = new StringBuilder();

                var stdoutTcs = new TaskCompletionSource<bool>();
                var stderrTcs = new TaskCompletionSource<bool>();
                var exitTcs = new TaskCompletionSource<int>();

                proc.OutputDataReceived += (s, e) =>
                {
                    if (e.Data == null) stdoutTcs.TrySetResult(true);
                    else sbOut.AppendLine(e.Data);
                };
                proc.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data == null) stderrTcs.TrySetResult(true);
                    else sbErr.AppendLine(e.Data);
                };
                proc.Exited += (s, e) =>
                {
                    try
                    {
                        exitTcs.TrySetResult(proc.ExitCode);
                    }
                    catch (Exception ex)
                    {
                        exitTcs.TrySetException(ex);
                    }
                };

                try
                {
                    if (!proc.Start())
                        throw new InvalidOperationException("Failed to start ffmpeg process.");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to start ffmpeg: " + ex.Message, ex);
                }

                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                // await process exit
                var exitCode = await exitTcs.Task.ConfigureAwait(false);

                // ensure stdout/stderr drained (wait for null-data events)
                try { await Task.WhenAll(stdoutTcs.Task, stderrTcs.Task).ConfigureAwait(false); } catch { /* ignore */ }

                return Tuple.Create(exitCode, sbOut.ToString(), sbErr.ToString());
            }
        }
    }
}