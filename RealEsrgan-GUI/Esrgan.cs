using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealEsrgan_GUI
{
    internal class Esrgan
    {
        private readonly EsrganModelReader modelReader;
        private Process process;
        private readonly string esrganPath;
        private readonly string modelsPath;
        private Action<string> onOutput;
        private Action<int> onExit;

        public Esrgan(string esrganPath, string modelsPath)
        {
            this.esrganPath = PathResolver.GetFilePath(esrganPath);
            this.modelsPath = PathResolver.GetDirectoryPath(modelsPath);
            this.modelReader = new EsrganModelReader(this.modelsPath);
        }

        public void OnOutput(Action<string> callback) => onOutput = callback;
        public void OnExit(Action<int> callback) => onExit = callback;

        public bool RunFile(string fileName, string outFileName, EsrganModel model, int scale, int titleSize, bool tta)
        {
            if (IsRunning()) return false;
            if (!System.IO.File.Exists(fileName)) return false;

            var format = Path.GetExtension(outFileName);
            var args = BuildEsrganArgs(fileName, outFileName, format, model.GetFullName(), scale, titleSize, tta);
            var psi = CreateStartInfo(args);
            return StartProcess(psi);
        }

        public bool RunFolder(string inputFolder, string outFolder, EsrganModel model, int scale, int titleSize, bool tta)
        {
            if (IsRunning()) return false;
            if (!Directory.Exists(inputFolder)) return false;

            var args = BuildEsrganArgs(inputFolder, outFolder, "png", model.GetFullName(), scale, titleSize, tta);
            var psi = CreateStartInfo(args);
            return StartProcess(psi);
        }

        private ProcessStartInfo CreateStartInfo(string args) => new ProcessStartInfo
        {
            FileName = esrganPath,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        private bool StartProcess(ProcessStartInfo psi)
        {
            process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) onOutput?.Invoke(e.Data); };
            process.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) onOutput?.Invoke(e.Data); };
            process.Exited += (s, e) => onExit?.Invoke(process.ExitCode);

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetModelNames() => modelReader.GetModelNames();
        public EsrganModel GetModel(string shortName, int scale) => modelReader.GetModel(shortName, scale);
        public bool IsRunning() => process != null && !process.HasExited;

        // added: async wait for process exit
        public Task<int> WaitForExitAsync()
        {
            if (process == null) return Task.FromResult(0);
            try
            {
                if (process.HasExited) return Task.FromResult(process.ExitCode);
            }
            catch
            {
                // reading HasExited might throw if process handle invalidated; fall through to safe behavior
            }

            var tcs = new TaskCompletionSource<int>();
            EventHandler handler = null;
            handler = (s, e) =>
            {
                try { tcs.TrySetResult(process.ExitCode); }
                catch { tcs.TrySetResult(-1); }
                finally
                {
                    process.Exited -= handler;
                }
            };

            // Ensure events are enabled
            process.EnableRaisingEvents = true;
            process.Exited += handler;
            return tcs.Task;
        }

        public bool Terminate()
        {
            if (!IsRunning()) return false;
            try { process.Kill(); return true; }
            catch { return false; }
        }

        private string BuildEsrganArgs(string input, string output, string format, string model, int scale, int titleSize, bool tta)
        {
            // ensure format without leading dot
            if (!string.IsNullOrEmpty(format) && format.StartsWith(".")) format = format.TrimStart('.');
            var sb = new StringBuilder();
            sb.Append($"-i \"{input}\" ");
            sb.Append($"-o \"{output}\" ");
            sb.Append($"-s {scale} ");
            sb.Append($"-m \"{modelsPath}\" ");
            sb.Append($"-n \"{model}\" ");
            sb.Append($"-f {format} ");
            if (titleSize >= 32) sb.Append($"-t {titleSize} ");
            if (tta) sb.Append("-x ");
            return sb.ToString();
        }

        public class EsrganModel
        {
            private readonly string _fullName;
            private readonly string _cleanName;
            private readonly int _scale;

            public EsrganModel(string fileName)
            {
                _fullName = fileName;
                var name = fileName;
                var scale = 0;

                var match = Regex.Match(fileName, @"-x(\d+)(?:plus)?", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    scale = int.Parse(match.Groups[1].Value);
                    name = Regex.Replace(fileName, @"-x\d+(?:plus)?", "", RegexOptions.IgnoreCase);
                }

                _cleanName = name;
                _scale = scale;
            }

            public string GetFullName() => _fullName;
            public string GetName() => _cleanName;
            public int GetScale() => _scale;
        }

        private class EsrganModelReader
        {
            private readonly List<EsrganModel> _models = new List<EsrganModel>();

            public EsrganModelReader(string folderPath)
            {
                if (!Directory.Exists(folderPath))
                    throw new DirectoryNotFoundException($"Folder not found: {folderPath}");

                var files = Directory.GetFiles(folderPath);
                var unique = new Dictionary<string, EsrganModel>(StringComparer.OrdinalIgnoreCase);

                foreach (var file in files)
                {
                    var nameNoExt = Path.GetFileNameWithoutExtension(file);
                    var model = new EsrganModel(nameNoExt);
                    var key = $"{model.GetName()}|{model.GetScale()}";
                    if (!unique.ContainsKey(key)) unique[key] = model;
                }

                _models.AddRange(unique.Values);
            }

            public List<EsrganModel> GetModels() => _models;

            public List<string> GetModelNames()
            {
                return _models.Select(m => m.GetName()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }

            public EsrganModel GetModel(string shortName, int scale)
            {
                var model = _models.FirstOrDefault(m => string.Equals(m.GetName(), shortName, StringComparison.OrdinalIgnoreCase) && m.GetScale() == scale);
                if (model != null) return model;
                return _models.FirstOrDefault(m => string.Equals(m.GetName(), shortName, StringComparison.OrdinalIgnoreCase) && m.GetScale() == 0);
            }
        }
    }
}