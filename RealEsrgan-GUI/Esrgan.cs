using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealEsrgan_GUI
{
    internal class Esrgan
    {
        private EsrganModelReader modelReader = null;
        private Process process = null;
        private string esrganPath = null;
        private string modelsPath = null;   
        private Action<string> onOutput = null;
        private Action<int> onExit = null;

        public Esrgan(string esrganPath, string modelsPath)
        {
            this.esrganPath = PathResolver.GetFilePath(esrganPath);
            this.modelsPath = PathResolver.GetDirectoryPath(modelsPath);
            this.modelReader = new EsrganModelReader(this.modelsPath);
        }

        public void OnOutput(Action<string> callback)
        {
            onOutput = callback;
        }

        public void OnExit(Action<int> callback)
        {
            onExit = callback;
        }

        public bool RunFile(string fileName, string outFileName, EsrganModel model, int scale, int titleSize, bool tta)
        {
            if (IsRunning())
                return false;

            if (!System.IO.File.Exists(fileName))
                return false;

            var psi = new ProcessStartInfo
            {
                FileName = esrganPath,
                Arguments = BuildEsrganArgs(fileName, outFileName, Path.GetExtension(outFileName), model.GetFullName(), scale, titleSize, tta),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    onOutput?.Invoke(e.Data);
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    onOutput?.Invoke(e.Data);
            };

            process.Exited += (s, e) =>
            {
                onExit?.Invoke(process.ExitCode);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return true;
        }

        public bool RunFolder(string inputFolder, string outFolder, EsrganModel model, int scale, int titleSize, bool tta)
        {
            if (IsRunning())
                return false;

            if (!System.IO.Directory.Exists(inputFolder))
                return false;

            var psi = new ProcessStartInfo
            {
                FileName = esrganPath,
                Arguments = BuildEsrganArgs(inputFolder, outFolder, "png", model.GetFullName(), scale, titleSize, tta),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    onOutput?.Invoke(e.Data);
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    onOutput?.Invoke(e.Data);
            };

            process.Exited += (s, e) =>
            {
                onExit?.Invoke(process.ExitCode);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return true;
        }

        public List<string> GetModelNames()
        {
            return modelReader.GetModelNames();
        }

        public EsrganModel GetModel(string shortName, int scale)
        {
            return modelReader.GetModel(shortName, scale);
        }

        public bool IsRunning()
        {
            return process != null && !process.HasExited;
        }

        public void WaitForExit()
        {
            if (IsRunning())
                process.WaitForExit();
        }

        public bool Terminate()
        {
            if (IsRunning())
            {
                try
                {
                    process.Kill();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
                return false;
        }

        private string BuildEsrganArgs(string input, string output,string format, string model, int scale, int titleSize, bool tta)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"-i \"{input}\" ");
            sb.Append($"-o \"{output}\" ");
            sb.Append($"-s {scale} ");
            sb.Append($"-m \"{modelsPath}\" ");
            sb.Append($"-n \"{model}\" ");
            sb.Append($"-f {format} ");
            if (titleSize >= 32)
                sb.Append($"-t {titleSize} ");
            if (tta)
                sb.Append($"-x ");
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

                var uniqueModels = new Dictionary<string, EsrganModel>(StringComparer.OrdinalIgnoreCase);

                foreach (var file in files)
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
                    var model = new EsrganModel(fileNameWithoutExt);

                    string key = $"{model.GetName()}|{model.GetScale()}";
                    if (!uniqueModels.ContainsKey(key))
                        uniqueModels[key] = model;
                }

                _models.AddRange(uniqueModels.Values);
            }

            public List<EsrganModel> GetModels() => _models;

            public List<string> GetModelNames()
            {
                return _models.Select(m => m.GetName()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }

            public EsrganModel GetModel(string shortName, int scale)
            {
                var model = _models.FirstOrDefault(m =>
                    string.Equals(m.GetName(), shortName, StringComparison.OrdinalIgnoreCase)
                    && m.GetScale() == scale);

                if (model != null) return model;

                return _models.FirstOrDefault(m =>
                    string.Equals(m.GetName(), shortName, StringComparison.OrdinalIgnoreCase)
                    && m.GetScale() == 0);
            }
        }
    }
}
