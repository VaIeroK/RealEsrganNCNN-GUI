using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Pipes;

namespace RealEsrgan_GUI
{
    public partial class MainForm : Form
    {
        private static readonly string[] imageExtensions = { ".png", ".jpg", ".jpeg", ".webp" };
        private static readonly string[] videoExtensions = { ".mp4", ".mov", ".mkv", ".webm", ".avi" };
        private readonly Control[] controlsToSave;
        private readonly string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppSettings.config");
        private readonly Esrgan esrgan;
        private readonly FFmpeg fFmpeg;
        private bool esrganTerminated;
        private readonly List<string> inputPaths = new List<string>();

        private readonly SettingsManager settingsManager;
        private PipeServer pipeServer;

        public MainForm(string[] args)
        {
            InitializeComponent();

            settingsManager = new SettingsManager(configPath);

            // instantiate and start pipe server (callback will marshal to UI thread)
            pipeServer = new PipeServer("RealEsrganPipe", files => this.Invoke((Action)(() => AddPaths(files, true))));
            pipeServer.Start();

            controlsToSave = new Control[]
            {
                this,
                FileRadioButton,
                FolderRadioButton,
                ScaleComboBox,
                ModelComboBox,
                TitleSizeUpDown,
                TTAModeCheckBox,
                AutoTitleSizeCheckBox
            };

            // initialize external tools with error handling (show message and exit if missing)
            Esrgan tempEsrgan = null;
            FFmpeg tempFfmpeg = null;
            try
            {
                tempEsrgan = new Esrgan("realesrgan-ncnn-vulkan.exe", "models");
                tempFfmpeg = new FFmpeg("ffmpeg.exe");
            }
            catch (FileNotFoundException fnf)
            {
                // stop pipe server and dispose settings before exit
                try { pipeServer?.Stop(); pipeServer?.Dispose(); } catch { }
                try { settingsManager?.Dispose(); } catch { }

                MessageBox.Show("Required executable not found:\n\n" + fnf.Message + "\n\nPlace the executable or a .lnk shortcut to it in the application folder.",
                                "Missing dependency", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // assign readonly fields to satisfy compiler (constructor will not complete)
                esrgan = null;
                fFmpeg = null;

                Environment.Exit(1);
                return;
            }
            catch (Exception ex)
            {
                try { pipeServer?.Stop(); pipeServer?.Dispose(); } catch { }
                try { settingsManager?.Dispose(); } catch { }

                MessageBox.Show("Failed to initialize required tools:\n\n" + ex.Message,
                                "Initialization error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                esrgan = null;
                fFmpeg = null;

                Environment.Exit(1);
                return;
            }

            // assign readonly fields
            esrgan = tempEsrgan;
            fFmpeg = tempFfmpeg;

            foreach (var name in esrgan.GetModelNames())
                ModelComboBox.Items.Add(name);

            if (ModelComboBox.Items.Count > 0)
                ModelComboBox.SelectedIndex = 0;

            ScaleComboBox.SelectedIndex = 0;

            toolTip.SetToolTip(TTAModeCheckBox, "TTA (Test-Time Augmentation) improves image quality by processing multiple augmented versions and averaging the results.");
            toolTip.SetToolTip(TitleSizeLabel, "Tile size splits image into blocks of this size to reduce GPU memory usage.");
            toolTip.SetToolTip(TitleSizeUpDown, "Tile size splits image into blocks of this size to reduce GPU memory usage.");
            toolTip.SetToolTip(AutoTitleSizeCheckBox, "Tile size splits image into blocks of this size to reduce GPU memory usage.");
            toolTip.SetToolTip(ScaleLabel, "Output image image enlargement factor.");
            toolTip.SetToolTip(ScaleComboBox, "Output image image enlargement factor.");
            toolTip.SetToolTip(ModelLabel, "Choose the pre-trained upscaling model.");
            toolTip.SetToolTip(ModelComboBox, "Choose the pre-trained upscaling model.");

            // configure OpenFileDialog filters based on extension arrays
            openFileDialog.Filter = BuildOpenFileDialogFilter();
            openFileDialog.FilterIndex = 1;

            // load UI settings via unified manager
            settingsManager.LoadControls(controlsToSave);
            AutoTitleSizeCheckBox_CheckedChanged(null, null);

            // restore dialog paths
            openFileDialog.InitialDirectory = settingsManager.Get("OpenFileDialog");
            inputFolderBrowserDialog.SelectedPath = settingsManager.Get("InputFolderDialog");

            AddPaths(args);
        }

        private string BuildOpenFileDialogFilter()
        {
            // Build pattern lists like "*.png;*.jpg"
            string imagePattern = string.Join(";", imageExtensions.Select(ext => $"*{ext}"));
            string videoPattern = string.Join(";", videoExtensions.Select(ext => $"*{ext}"));
            string allSupportedPattern = string.Join(";", imageExtensions.Concat(videoExtensions).Select(ext => $"*{ext}"));

            // "Images (*.png;*.jpg)|*.png;*.jpg|Videos (*.mp4;*.mov)|*.mp4;*.mov|All supported (*.png;*.jpg;*.mp4)|*.png;*.jpg;*.mp4|All files (*.*)|*.*"
            var parts = new List<string>
            {
                $"Images ({imagePattern})|{imagePattern}",
                $"Videos ({videoPattern})|{videoPattern}",
                $"All supported ({allSupportedPattern})|{allSupportedPattern}",
                "All files (*.*)|*.*"
            };

            return string.Join("|", parts);
        }

        private void BrowseInputButton_Click(object sender, EventArgs e)
        {
            if (FileRadioButton.Checked)
            {
                // ensure filter is up-to-date (if arrays changed at runtime)
                openFileDialog.Filter = BuildOpenFileDialogFilter();
                openFileDialog.FilterIndex = 3;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    AddPaths(openFileDialog.FileNames);
            }
            else if (FolderRadioButton.Checked)
            {
                if (inputFolderBrowserDialog.ShowDialog() == DialogResult.OK)
                    AddPaths(new[] { inputFolderBrowserDialog.SelectedPath });
            }
        }

        private async void RunButton_Click(object sender, EventArgs e)
        {
            if (esrgan.IsRunning())
            {
                MessageBox.Show("Upscaling is already in progress", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int total = inputPaths.Count;
            int current = 0;

            ClearVideoFoldersButton.Visible = false;
            ClearVideoFoldersButton.Enabled = true;
            esrganTerminated = false;
            foreach (var input in inputPaths.ToArray())
            {
                if (esrganTerminated) break;

                OutputGroupBox.Text = $"Output    Processing: ({++current}/{total})";
                await Upscale(input);
            }
            OutputGroupBox.Text = "Output";
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            pipeServer.Stop();
            pipeServer.Dispose();

            // save UI controls and dialog paths via manager
            settingsManager.SaveControls(controlsToSave);
            if (!string.IsNullOrEmpty(openFileDialog.FileName)) settingsManager.Set("OpenFileDialog", Path.GetDirectoryName(openFileDialog.FileName));
            if (!string.IsNullOrEmpty(inputFolderBrowserDialog.SelectedPath)) settingsManager.Set("InputFolderDialog", inputFolderBrowserDialog.SelectedPath);
            settingsManager.Save();

            StopButton_Click(null, null);
            settingsManager.Dispose();
        }

        private void AutoTitleSizeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            TitleSizeUpDown.Enabled = !AutoTitleSizeCheckBox.Checked;
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (!esrgan.IsRunning()) return;

            ClearVideoFoldersButton.Visible = true;
            ClearVideoFoldersButton.Enabled = true;
            esrganTerminated = true;
            ConsoleOutputRichTextBox.AppendText(esrgan.Terminate() ? "ESRGAN stopped" + Environment.NewLine : "Failed to stop ESRGAN" + Environment.NewLine);
        }

        private void addToContextMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var appPath = Assembly.GetExecutingAssembly().Location;
            const string menuName = "RealEsrgan Upscale";

            try
            {
                foreach (var ext in imageExtensions.Concat(videoExtensions))
                {
                    using (var key = Registry.ClassesRoot.CreateSubKey($@"SystemFileAssociations\{ext}\shell\RealEsrgan-GUI"))
                    {
                        key.SetValue("", menuName);
                        key.SetValue("Icon", appPath);
                    }
                    using (var cmdKey = Registry.ClassesRoot.CreateSubKey($@"SystemFileAssociations\{ext}\shell\RealEsrgan-GUI\command"))
                    {
                        cmdKey.SetValue("", $"\"{appPath}\" \"%1\"");
                    }
                }
                MessageBox.Show("Success", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add context menu. Try running as administrator.\n\n" + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void removeFromContextMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var ext in imageExtensions.Concat(videoExtensions))
                    Registry.ClassesRoot.DeleteSubKeyTree($@"SystemFileAssociations\{ext}\shell\RealEsrgan-GUI", false);
                MessageBox.Show("Success", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove context menu. Try running as administrator.\n\n" + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            AddPaths(files);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private async void ClearVideoFoldersButton_Click(object sender, EventArgs e)
        {
            // Collect candidate temp folders for selected video inputs
            var candidates = new List<string>();

            foreach (var path in inputPaths.ToArray())
            {
                if (!System.IO.File.Exists(path)) continue;
                var ext = Path.GetExtension(path).ToLowerInvariant();
                if (!videoExtensions.Contains(ext)) continue;

                var parent = Path.GetDirectoryName(path);
                var baseName = Path.GetFileNameWithoutExtension(path);
                var inDir = Path.Combine(parent, baseName + "_input");
                var outDir = Path.Combine(parent, baseName + "_output");

                if (Directory.Exists(inDir)) candidates.Add(inDir);
                if (Directory.Exists(outDir)) candidates.Add(outDir);
            }

            if (candidates.Count == 0)
            {
                MessageBox.Show("No temporary video folders found for selected inputs.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show($"Found {candidates.Count} temporary folder(s). Delete them now? This will permanently remove extracted/upscaled frames.",
                                          "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var (deletedCount, errors) = await CleanupVideoTempFoldersAsync(candidates).ConfigureAwait(false);

            this.Invoke((Action)(() =>
            {
                var msg = $"Deleted {deletedCount} folder(s).";
                if (errors.Count > 0)
                {
                    msg += $"\n\nErrors:\n{string.Join("\n", errors)}";
                    MessageBox.Show(msg, "Completed with errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show(msg, "Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Refresh UI state
                UpdateInputPath();
                ClearVideoFoldersButton.Enabled = false;
                ClearVideoFoldersButton.Visible = false;
            }));
        }

        /// <summary>
        /// Reusable cleanup function: deletes provided temporary folders (distinct),
        /// retries on transient errors, runs on background thread.
        /// Returns number of successfully deleted folders and list of error messages.
        /// </summary>
        private async Task<(int deletedCount, List<string> errors)> CleanupVideoTempFoldersAsync(IEnumerable<string> dirs)
        {
            if (dirs == null) return (0, new List<string>());

            var distinct = dirs.Where(d => !string.IsNullOrWhiteSpace(d)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            int deletedCount = 0;
            var errors = new List<string>();

            await Task.Run(() =>
            {
                foreach (var dir in distinct)
                {
                    try
                    {
                        if (!Directory.Exists(dir)) continue;

                        bool ok = false;
                        Exception lastEx = null;
                        for (int attempt = 0; attempt < 3 && !ok; attempt++)
                        {
                            try
                            {
                                Directory.Delete(dir, true);
                                ok = true;
                            }
                            catch (Exception ex)
                            {
                                lastEx = ex;
                                Thread.Sleep(150);
                            }
                        }

                        if (ok) Interlocked.Increment(ref deletedCount);
                        else errors.Add($"{dir}: {lastEx?.Message ?? "Delete failed"}");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{dir}: {ex.Message}");
                    }
                }
            }).ConfigureAwait(false);

            return (deletedCount, errors);
        }

        private async Task Upscale(string inputPath)
        {
            if (esrganTerminated) return;

            if ((FileRadioButton.Checked && !System.IO.File.Exists(inputPath)) ||
                (FolderRadioButton.Checked && !Directory.Exists(inputPath)))
            {
                MessageBox.Show(FileRadioButton.Checked ? "Input file does not exist!" : "Input folder does not exist!",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // prepare common params
            int scale = int.Parse(ScaleComboBox.SelectedItem.ToString().Replace("x", ""));
            int titleSize = AutoTitleSizeCheckBox.Checked ? 0 : (int)TitleSizeUpDown.Value;
            if (ModelComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a model!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var model = esrgan.GetModel(ModelComboBox.SelectedItem.ToString(), scale);

            // wire ESRGAN output callbacks (UI thread)
            esrgan.OnOutput(data =>
            {
                if (!string.IsNullOrEmpty(data))
                    ConsoleOutputRichTextBox.Invoke((Action)(() => ConsoleOutputRichTextBox.AppendText(data + Environment.NewLine)));
            });

            esrgan.OnExit(code =>
            {
                ConsoleOutputRichTextBox.Invoke((Action)(() =>
                {
                    if (code == 0) ConsoleOutputRichTextBox.AppendText("Upscale Finished!" + Environment.NewLine);
                }));
            });

            ConsoleOutputRichTextBox.Clear();

            // If input is a single file and it's a video -> unpack -> ESRGAN folder -> pack back
            if (FileRadioButton.Checked && System.IO.File.Exists(inputPath) &&
                videoExtensions.Contains(Path.GetExtension(inputPath).ToLowerInvariant()))
            {
                var parentDir = Path.GetDirectoryName(inputPath);
                var baseName = Path.GetFileNameWithoutExtension(inputPath);

                var framesInputDir = Path.Combine(parentDir, baseName + "_input");
                var framesOutputDir = Path.Combine(parentDir, baseName + "_output");
                Directory.CreateDirectory(framesInputDir);
                Directory.CreateDirectory(framesOutputDir);

                int fps;
                try
                {
                    // If input frames folder already contains files, skip unpack (resume scenario).
                    var existingInputFiles = Directory.GetFiles(framesInputDir);
                    if (existingInputFiles.Length == 0)
                    {
                        long inputFileSize = new FileInfo(inputPath).Length;
                        long required = DiskHelpers.EstimateRequiredForUnpack(inputFileSize);
                        if (!DiskHelpers.HasEnoughDiskSpaceForPath(framesInputDir, required))
                        {
                            MessageBox.Show("Not enough disk space to unpack video. Operation cancelled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        ConsoleOutputRichTextBox.Invoke((Action)(() => ConsoleOutputRichTextBox.AppendText("Unpacking video to frames..." + Environment.NewLine)));
                        fps = await fFmpeg.UnpackAsync(inputPath, framesInputDir, FFmpegQuality.High).ConfigureAwait(false);
                        ConsoleOutputRichTextBox.Invoke((Action)(() => ConsoleOutputRichTextBox.AppendText($"Detected FPS: {fps}" + Environment.NewLine)));
                    }
                    else
                    {
                        // We still probe fps to use when packing back
                        fps = (int)Math.Round(await fFmpeg.ProbeFpsAsync(inputPath).ConfigureAwait(false));
                        ConsoleOutputRichTextBox.Invoke((Action)(() => ConsoleOutputRichTextBox.AppendText($"Found existing frames ({existingInputFiles.Length}), skipping unpack. Detected FPS: {fps}" + Environment.NewLine)));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to unpack video:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Before starting ESRGAN: resume logic.
                // If output folder contains already processed frames, remove those from input so ESRGAN will process remaining frames.
                try
                {
                    var outFiles = Directory.Exists(framesOutputDir) ? Directory.GetFiles(framesOutputDir) : new string[0];
                    if (outFiles.Length > 0)
                    {
                        var outNames = new HashSet<string>(outFiles.Select(Path.GetFileName), StringComparer.OrdinalIgnoreCase);
                        int removed = 0;
                        foreach (var inFile in Directory.GetFiles(framesInputDir))
                        {
                            if (outNames.Contains(Path.GetFileName(inFile)))
                            {
                                try
                                {
                                    System.IO.File.Delete(inFile);
                                    removed++;
                                }
                                catch
                                {
                                    // ignore individual delete errors
                                }
                            }
                        }
                        if (removed > 0)
                        {
                            ConsoleOutputRichTextBox.Invoke((Action)(() => ConsoleOutputRichTextBox.AppendText($"Resuming: removed {removed} already processed frames from input folder." + Environment.NewLine)));
                        }
                    }
                }
                catch
                {
                    // ignore resume cleanup errors
                }

                if (esrganTerminated) return;

                // Check free space for processing (esrgan output will increase size). Estimate: need at least input frames size * 2 + margin
                try
                {
                    long inputFramesSize = DiskHelpers.GetDirectorySizeBytes(framesInputDir);
                    long requiredForProcessing = DiskHelpers.EstimateRequiredForProcessing(inputFramesSize);
                    if (!DiskHelpers.HasEnoughDiskSpaceForPath(framesOutputDir, requiredForProcessing))
                    {
                        MessageBox.Show("Not enough disk space to run ESRGAN/upscale. Operation cancelled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                catch
                {
                    // ignore size calculation errors, proceed but be conservative
                }

                // start ESRGAN on folder
                bool started = esrgan.RunFolder(framesInputDir, framesOutputDir, model, scale, titleSize, TTAModeCheckBox.Checked);
                if (!started)
                {
                    MessageBox.Show("Failed to start ESRGAN for frames folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // await ESRGAN to finish
                await esrgan.WaitForExitAsync().ConfigureAwait(false);

                // If user stopped ESRGAN manually, do not create final video
                if (esrganTerminated)
                {
                    ConsoleOutputRichTextBox.Invoke((Action)(() => ConsoleOutputRichTextBox.AppendText("Upscale was stopped by user. Skipping video packing." + Environment.NewLine)));
                    // allow manual cleanup by user via button
                    this.Invoke((Action)(() =>
                    {
                        ClearVideoFoldersButton.Visible = true;
                        ClearVideoFoldersButton.Enabled = true;
                    }));
                    return;
                }

                // pack frames back to video
                var outVideo = Path.Combine(parentDir, baseName + "_upscaled" + Path.GetExtension(inputPath));
                try
                {
                    // check free space for packing: need free >= size of output frames + margin
                    long outFramesSize = DiskHelpers.GetDirectorySizeBytes(framesOutputDir);
                    long requiredForPack = Math.Max(outFramesSize + (100L * 1024 * 1024L), 200L * 1024 * 1024L);
                    if (!DiskHelpers.HasEnoughDiskSpaceForPath(framesOutputDir, requiredForPack))
                    {
                        MessageBox.Show("Not enough disk space to pack upscaled video. Operation cancelled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    ConsoleOutputRichTextBox.Invoke((Action)(() => ConsoleOutputRichTextBox.AppendText("Packing upscaled frames to video..." + Environment.NewLine)));
                    int code = await fFmpeg.PackAsync(framesOutputDir, outVideo, fps, FFmpegQuality.High).ConfigureAwait(false);
                    ConsoleOutputRichTextBox.Invoke((Action)(() =>
                    {
                        if (code == 0)
                            ConsoleOutputRichTextBox.AppendText($"Packed video: {outVideo}" + Environment.NewLine);
                        else
                            ConsoleOutputRichTextBox.AppendText($"ffmpeg packing failed (code {code})" + Environment.NewLine);
                    }));

                    // If packing succeeded, cleanup temp folders automatically
                    if (!esrganTerminated)
                    {
                        var dirsToDelete = new[] { framesInputDir, framesOutputDir };
                        var (deleted, errors) = await CleanupVideoTempFoldersAsync(dirsToDelete).ConfigureAwait(false);
                        this.Invoke((Action)(() =>
                        {
                            if (errors.Count == 0)
                            {
                                ConsoleOutputRichTextBox.AppendText($"Temporary folders removed ({deleted})." + Environment.NewLine);
                            }
                            else
                            {
                                ConsoleOutputRichTextBox.AppendText($"Temporary cleanup completed with {errors.Count} errors." + Environment.NewLine);
                                MessageBox.Show("Some temporary folders could not be deleted:\n\n" + string.Join("\n", errors), "Cleanup errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                ClearVideoFoldersButton.Visible = true;
                                ClearVideoFoldersButton.Enabled = true;
                            }
                            ClearVideoFoldersButton.Visible = false;
                        }));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to pack video:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Invoke((Action)(() =>
                    {
                        ClearVideoFoldersButton.Visible = true;
                        ClearVideoFoldersButton.Enabled = true;
                    }));
                    return;
                }

                return;
            }

            // Non-video file or folder path: existing behavior
            string outputPath = AddFilePrefix(inputPath);
            if (FolderRadioButton.Checked && !Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            // Before running ESRGAN on single file, check drive free space
            if (FileRadioButton.Checked)
            {
                try
                {
                    long inputSize = new FileInfo(inputPath).Length;
                    long required = Math.Max(inputSize * 2, 200L * 1024 * 1024);
                    if (!DiskHelpers.HasEnoughDiskSpaceForPath(inputPath, required))
                    {
                        MessageBox.Show("Not enough disk space to run ESRGAN on this file. Operation cancelled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                catch { /* ignore size check errors and proceed */ }
            }

            if (FileRadioButton.Checked)
                esrgan.RunFile(inputPath, outputPath, model, scale, titleSize, TTAModeCheckBox.Checked);
            else
                esrgan.RunFolder(inputPath, outputPath, model, scale, titleSize, TTAModeCheckBox.Checked);

            await esrgan.WaitForExitAsync().ConfigureAwait(false);
        }

        private void UpdateInputPath()
        {
            InputLabel.Text = inputPaths.Count == 0
                ? "-"
                : (Path.GetFileName(inputPaths[0]) + (inputPaths.Count > 1 ? $"    (+{inputPaths.Count - 1} more)" : ""));
        }

        private string AddFilePrefix(string path)
        {
            // handles existing file, directory or arbitrary path (with or without extension)
            if (System.IO.File.Exists(path))
            {
                var dir = Path.GetDirectoryName(path);
                var name = Path.GetFileNameWithoutExtension(path);
                var ext = Path.GetExtension(path);
                return Path.Combine(dir, $"{name}_upscaled{ext}");
            }
            if (Directory.Exists(path))
            {
                var parent = Path.GetDirectoryName(path);
                var folder = Path.GetFileName(path);
                return Path.Combine(parent, $"{folder}_upscaled");
            }
            // fallback: treat as path (may be relative or non-existing)
            var dirFallback = Path.GetDirectoryName(path);
            var nameFallback = Path.GetFileNameWithoutExtension(path);
            var extFallback = Path.GetExtension(path);
            return Path.Combine(dirFallback ?? "", $"{nameFallback}_upscaled{extFallback}");
        }

        private void AddPaths(string[] paths, bool append = false)
        {
            if (paths == null || paths.Length == 0) return;

            if (!append)
                inputPaths.Clear();

            var first = paths[0];
            if (System.IO.File.Exists(first))
            {
                foreach (var file in paths)
                {
                    var ext = Path.GetExtension(file).ToLowerInvariant();
                    if ((imageExtensions.Contains(ext) || videoExtensions.Contains(ext)) && System.IO.File.Exists(file))
                        inputPaths.Add(file);
                }
                FileRadioButton.Checked = true;
            }
            else if (Directory.Exists(first))
            {
                foreach (var dir in paths)
                {
                    if (Directory.Exists(dir))
                        inputPaths.Add(dir);
                }
                FolderRadioButton.Checked = true;
            }

            UpdateInputPath();
        }
    }
}