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
        private static readonly string[] extensions = { ".png", ".jpg", ".jpeg", ".webp" };
        private readonly Control[] controlsToSave;
        private readonly string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppSettings.config");
        private readonly Configuration config;
        private static readonly Mutex configMutex = new Mutex(false, "RealEsrganUpscale_ConfigMutex");
        private readonly Esrgan esrgan;
        private bool esrganTerminated;
        private readonly List<string> inputPaths = new List<string>();
        private CancellationTokenSource pipeCts;

        public MainForm(string[] args)
        {
            InitializeComponent();

            StartPipeServer();

            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = configPath };
            config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

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

            ScaleComboBox.SelectedIndex = 0;

            esrgan = new Esrgan("realesrgan-ncnn-vulkan.exe", "models");
            foreach (var name in esrgan.GetModelNames())
                ModelComboBox.Items.Add(name);

            if (ModelComboBox.Items.Count > 0)
                ModelComboBox.SelectedIndex = 0;

            toolTip.SetToolTip(TTAModeCheckBox, "TTA (Test-Time Augmentation) improves image quality by processing multiple augmented versions and averaging the results.");
            toolTip.SetToolTip(TitleSizeLabel, "Tile size splits image into blocks of this size to reduce GPU memory usage.");
            toolTip.SetToolTip(TitleSizeUpDown, "Tile size splits image into blocks of this size to reduce GPU memory usage.");
            toolTip.SetToolTip(AutoTitleSizeCheckBox, "Tile size splits image into blocks of this size to reduce GPU memory usage.");
            toolTip.SetToolTip(ScaleLabel, "Output image image enlargement factor.");
            toolTip.SetToolTip(ScaleComboBox, "Output image image enlargement factor.");
            toolTip.SetToolTip(ModelLabel, "Choose the pre-trained upscaling model.");
            toolTip.SetToolTip(ModelComboBox, "Choose the pre-trained upscaling model.");

            LoadSettings();
            AutoTitleSizeCheckBox_CheckedChanged(null, null);

            AddPaths(args);
        }

        private void BrowseInputButton_Click(object sender, EventArgs e)
        {
            if (FileRadioButton.Checked)
            {
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
            StopPipeServer();

            SaveSettings();
            StopButton_Click(null, null);
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

            string outputPath = AddFilePrefix(inputPath);
            if (FolderRadioButton.Checked && !Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            if (ModelComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a model!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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

            int scale = int.Parse(ScaleComboBox.SelectedItem.ToString().Replace("x", ""));
            int titleSize = AutoTitleSizeCheckBox.Checked ? 0 : (int)TitleSizeUpDown.Value;
            var model = esrgan.GetModel(ModelComboBox.SelectedItem.ToString(), scale);

            if (FileRadioButton.Checked)
                esrgan.RunFile(inputPath, outputPath, model, scale, titleSize, TTAModeCheckBox.Checked);
            else
                esrgan.RunFolder(inputPath, outputPath, model, scale, titleSize, TTAModeCheckBox.Checked);

            await esrgan.WaitForExitAsync();
        }

        private void AutoTitleSizeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            TitleSizeUpDown.Enabled = !AutoTitleSizeCheckBox.Checked;
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (!esrgan.IsRunning()) return;

            esrganTerminated = true;
            ConsoleOutputRichTextBox.AppendText(esrgan.Terminate() ? "ESRGAN stopped" + Environment.NewLine : "Failed to stop ESRGAN" + Environment.NewLine);
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
                    if (extensions.Contains(ext) && System.IO.File.Exists(file))
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

        private void addToContextMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var appPath = Assembly.GetExecutingAssembly().Location;
            const string menuName = "RealEsrgan Upscale";

            try
            {
                foreach (var ext in extensions)
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
                foreach (var ext in extensions)
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

        private void StartPipeServer()
        {
            // create CTS and fire-and-forget the server loop
            StopPipeServer(); // ensure previous stopped if called twice
            pipeCts = new CancellationTokenSource();
            var token = pipeCts.Token;
            _ = RunPipeServerAsync(token);
        }

        private void StopPipeServer()
        {
            try
            {
                if (pipeCts != null && !pipeCts.IsCancellationRequested)
                {
                    pipeCts.Cancel();
                    pipeCts.Dispose();
                }
            }
            catch
            {
                // ignore
            }
            finally
            {
                pipeCts = null;
            }
        }

        private async Task RunPipeServerAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                using (var server = new NamedPipeServerStream("RealEsrganPipe", PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous))
                {
                    var connectTask = server.WaitForConnectionAsync();
                    var tcs = new TaskCompletionSource<object>();
                    using (token.Register(() => tcs.TrySetResult(null)))
                    {
                        var finished = await Task.WhenAny(connectTask, tcs.Task);
                        if (finished == tcs.Task)
                        {
                            // cancellation requested - try to close server and exit loop
                            try { server.Dispose(); } catch { }
                            break;
                        }
                        else
                        {
                            // connected - read once
                            try
                            {
                                using (var reader = new StreamReader(server))
                                {
                                    var line = await reader.ReadLineAsync();
                                    if (!string.IsNullOrEmpty(line))
                                    {
                                        var files = line.Split('|');
                                        this.Invoke((Action)(() => AddPaths(files, true)));
                                    }
                                }
                            }
                            catch
                            {
                                // ignore transient pipe errors
                            }
                        }
                    }
                }
            }
        }

        #region Settings
        private void LoadSettings()
        {
            foreach (var control in controlsToSave)
            {
                var setting = config.AppSettings.Settings[control.Name];
                if (setting == null) continue;

                var value = setting.Value;
                if (control is TextBox tb) tb.Text = value ?? "";
                else if (control is ComboBox cb)
                {
                    int ci;
                    cb.SelectedIndex = int.TryParse(value, out ci) && ci >= 0 && ci < cb.Items.Count ? ci : 0;
                }
                else if (control is NumericUpDown nud) nud.Value = int.TryParse(value, out int nv) ? nv : 0;
                else if (control is RadioButton rb) rb.Checked = bool.TryParse(value, out bool rbv) ? rbv : false;
                else if (control is CheckBox cbx) cbx.Checked = bool.TryParse(value, out bool cbv) ? cbv : false;
                else if (control is Form f)
                {
                    var parts = (value ?? "").Split(';');
                    if (parts.Length == 2)
                    {
                        var size = parts[0].Split(',');
                        var pos = parts[1].Split(',');
                        if (size.Length == 2 && int.TryParse(size[0], out int w) && int.TryParse(size[1], out int h))
                            f.Size = new System.Drawing.Size(w, h);
                        if (pos.Length == 2 && int.TryParse(pos[0], out int x) && int.TryParse(pos[1], out int y))
                        {
                            f.StartPosition = FormStartPosition.Manual;
                            f.Location = new System.Drawing.Point(x, y);
                        }
                    }
                }
            }

            openFileDialog.InitialDirectory = LoadSettings("OpenFileDialog");
            saveFileDialog.InitialDirectory = LoadSettings("SaveFileDialog");
            inputFolderBrowserDialog.SelectedPath = LoadSettings("InputFolderDialog");
            outputFolderBrowserDialog.SelectedPath = LoadSettings("OutputFolderDialog");
        }

        private void SaveSettings()
        {
            try
            {
                configMutex.WaitOne();
                foreach (var control in controlsToSave)
                {
                    string value = null;
                    if (control is TextBox tb) value = tb.Text;
                    else if (control is ComboBox cb) value = cb.SelectedIndex.ToString();
                    else if (control is NumericUpDown nud) value = nud.Value.ToString();
                    else if (control is RadioButton rb) value = rb.Checked.ToString();
                    else if (control is CheckBox cbx) value = cbx.Checked.ToString();
                    else if (control is Form f) value = $"{f.Size.Width},{f.Size.Height};{f.Location.X},{f.Location.Y}";

                    if (value != null) AddOrSet(control.Name, value);
                }

                if (!string.IsNullOrEmpty(openFileDialog.FileName)) AddOrSet("OpenFileDialog", Path.GetDirectoryName(openFileDialog.FileName));
                if (!string.IsNullOrEmpty(saveFileDialog.FileName)) AddOrSet("SaveFileDialog", Path.GetDirectoryName(saveFileDialog.FileName));
                if (!string.IsNullOrEmpty(inputFolderBrowserDialog.SelectedPath)) AddOrSet("InputFolderDialog", inputFolderBrowserDialog.SelectedPath);
                if (!string.IsNullOrEmpty(outputFolderBrowserDialog.SelectedPath)) AddOrSet("OutputFolderDialog", outputFolderBrowserDialog.SelectedPath);

                config.Save(ConfigurationSaveMode.Modified, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save settings.\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                configMutex.ReleaseMutex();
            }
        }

        private string LoadSettings(string key)
        {
            var s = config.AppSettings.Settings[key];
            return s == null ? "" : s.Value;
        }

        private void AddOrSet(string key, string value)
        {
            if (config.AppSettings.Settings[key] != null)
                config.AppSettings.Settings[key].Value = value;
            else
                config.AppSettings.Settings.Add(key, value);
        }
        #endregion
    }
}