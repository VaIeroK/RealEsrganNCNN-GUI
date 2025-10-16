using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealEsrgan_GUI
{
    public partial class MainForm : Form
    {
        string[] extensions = { ".png", ".jpg", ".jpeg", ".webp" };
        private Control[] controlsToSave = null;
        private readonly string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppSettings.config");
        private Configuration config;
        private static readonly Mutex configMutex = new Mutex(false, "RealEsrganUpscale_ConfigMutex");
        private Esrgan esrgan = null;
        private bool esrganTerminated = false;
        private List<string> inputPaths = null;

        public MainForm(string[] args)
        {
            InitializeComponent();

            var unused = this.Handle;

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

            inputPaths = new List<string>();
            esrgan = new Esrgan("realesrgan-ncnn-vulkan.exe", "models");
            var models = esrgan.GetModelNames();
            foreach (var name in models)
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
                {
                    AddPaths(openFileDialog.FileNames);
                }
            }
            else if (FolderRadioButton.Checked)
            {
                if (inputFolderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    AddPaths(new string[] { inputFolderBrowserDialog.SelectedPath  });
                }
            }
        }

        private async void RunButton_Click(object sender, EventArgs e)
        {
            if (esrgan.IsRunning())
            {
                MessageBox.Show(
                    "Upscaling is already in progress",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            int total = inputPaths.Count;
            int current = 0;

            esrganTerminated = false;
            foreach (string input in inputPaths)
            {
                if (esrganTerminated)
                    break;

                OutputGroupBox.Text = "Output    Processing: (" + (++current) + "/" + total + ")";

                await Upscale(input);
            }
            OutputGroupBox.Text = "Output";
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
            StopButton_Click(null, null);
        }

        private async Task Upscale(string inputPath)
        {
            if (esrganTerminated)
                return;

            string outputPath = AddFilePrefix(inputPath);

            if ((FileRadioButton.Checked && !System.IO.File.Exists(inputPath)) ||
                (FolderRadioButton.Checked && !Directory.Exists(inputPath)))
            {
                MessageBox.Show(FileRadioButton.Checked ? "Input file does not exist!" : "Input folder does not exist!",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (FolderRadioButton.Checked && !Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            if (ModelComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a model!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            esrgan.OnOutput(data =>
            {
                if (data != null)
                {
                    ConsoleOutputRichTextBox.Invoke((Action)(() =>
                    {
                        ConsoleOutputRichTextBox.AppendText(data + Environment.NewLine);
                    }));
                }
            });

            esrgan.OnExit(code =>
            {
                ConsoleOutputRichTextBox.Invoke((Action)(() =>
                {
                    if (code == 0)
                        ConsoleOutputRichTextBox.AppendText("Upscale Finished!" + Environment.NewLine);
                }));
            });

            ConsoleOutputRichTextBox.Clear();
            int scale = int.Parse(ScaleComboBox.SelectedItem.ToString().Replace("x", ""));
            int titleSize = AutoTitleSizeCheckBox.Checked ? 0 : (int)TitleSizeUpDown.Value;
            Esrgan.EsrganModel model = esrgan.GetModel(ModelComboBox.SelectedItem.ToString(), scale);
            if (FileRadioButton.Checked)
                esrgan.RunFile(inputPath, outputPath, model, scale, titleSize, TTAModeCheckBox.Checked);
            else if (FolderRadioButton.Checked)
                esrgan.RunFolder(inputPath, outputPath, model, scale, titleSize, TTAModeCheckBox.Checked);

            await Task.Run(() => esrgan.WaitForExit());
        }

        private void AutoTitleSizeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            TitleSizeUpDown.Enabled = !AutoTitleSizeCheckBox.Checked;
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (esrgan.IsRunning())
            {
                esrganTerminated = true;
                if (esrgan.Terminate())
                    ConsoleOutputRichTextBox.AppendText("ESRGAN stopped" + Environment.NewLine);
                else
                    ConsoleOutputRichTextBox.AppendText("Failed to stop ESRGAN" + Environment.NewLine);
            }
        }
        private void UpdateInputPath()
        {
            if (inputPaths.Count == 0)
            {
                InputLabel.Text = "-";
            }
            else
            {
                InputLabel.Text = Path.GetFileName(inputPaths[0]);
                if (inputPaths.Count > 1)
                    InputLabel.Text += $"    (+{inputPaths.Count - 1} more)";
            }
        }

        private string AddFilePrefix(string path)
        {
            if (System.IO.File.Exists(path))
            {
                string dir = Path.GetDirectoryName(path);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(path);
                string ext = Path.GetExtension(path);
                return Path.Combine(dir, $"{fileNameWithoutExt}_upscaled{ext}");
            }
            else if (Directory.Exists(path))
            {
                string parentDir = Path.GetDirectoryName(path);
                string folderName = Path.GetFileName(path);
                return Path.Combine(parentDir, $"{folderName}_upscaled");
            }
            else
            {
                string dir = Path.GetDirectoryName(path);
                string name = Path.GetFileName(path);
                if (!string.IsNullOrEmpty(Path.GetExtension(name)))
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(name);
                    string ext = Path.GetExtension(name);
                    return Path.Combine(dir, $"{fileNameWithoutExt}_upscaled{ext}");
                }
                else
                {
                    return Path.Combine(dir, $"{name}_upscaled");
                }
            }
        }

        private void AddPaths(string[] paths, bool append = false)
        {
            if (paths.Length > 0 && System.IO.File.Exists(paths[0]))
            {
                if (!append)
                    inputPaths.Clear();
                foreach (string file in paths)
                {
                    string ext = Path.GetExtension(file).ToLower();
                    if (extensions.Contains(ext) && System.IO.File.Exists(file))
                    {
                        inputPaths.Add(file);
                    }
                }
                FileRadioButton.Checked = true;
                UpdateInputPath();
            }
            else if (paths.Length > 0 && Directory.Exists(paths[0]))
            {
                if (!append)
                    inputPaths.Clear();
                foreach (string dir in paths)
                {
                    if (Directory.Exists(dir))
                    {
                        inputPaths.Add(dir);
                    }
                }
                FolderRadioButton.Checked = true;
                UpdateInputPath();
            }
        }

        private void addToContextMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string appPath = Assembly.GetExecutingAssembly().Location;
            string menuName = "RealEsrgan Upscale";

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
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            AddPaths(files);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Program.WM_COPYDATA)
            {
                Program.COPYDATASTRUCT cds = (Program.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(Program.COPYDATASTRUCT));
                string argString = Marshal.PtrToStringUni(cds.lpData);
                string[] files = argString.Split('|');
                MessageBox.Show(string.Join(", ", files), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AddPaths(files, true);
            }
            base.WndProc(ref m);
        }

        #region Settings
        private void LoadSettings()
        {
            foreach (var control in controlsToSave)
            {
                var setting = config.AppSettings.Settings[control.Name];
                if (setting == null)
                    continue;

                string value = config.AppSettings.Settings[control.Name]?.Value;
                switch (control)
                {
                    case TextBox tb: tb.Text = value ?? ""; break;
                    case ComboBox cb:
                        if (int.TryParse(value, out int ci) && ci >= 0 && ci < cb.Items.Count)
                            cb.SelectedIndex = ci;
                        else
                            cb.SelectedIndex = 0;
                        break;
                    case NumericUpDown nud: nud.Value = int.TryParse(value, out int nv) ? nv : 0; break;
                    case RadioButton rb: rb.Checked = bool.TryParse(value, out bool b) ? b : false; break;
                    case CheckBox cbx: cbx.Checked = bool.TryParse(value, out bool b2) ? b2 : false; break;
                    case Form f:
                        // "W,H;X,Y"
                        var parts = value.Split(';');
                        if (parts.Length == 2)
                        {
                            var size = parts[0].Split(',');
                            var pos = parts[1].Split(',');
                            if (size.Length == 2 &&
                                int.TryParse(size[0], out int w) &&
                                int.TryParse(size[1], out int h))
                                f.Size = new System.Drawing.Size(w, h);

                            if (pos.Length == 2 &&
                                int.TryParse(pos[0], out int x) &&
                                int.TryParse(pos[1], out int y))
                            {
                                f.StartPosition = FormStartPosition.Manual;
                                f.Location = new System.Drawing.Point(x, y);
                            }
                        }
                        break;
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

                    if (control is TextBox tb)
                        value = tb.Text;
                    else if (control is ComboBox cb)
                        value = cb.SelectedIndex.ToString();
                    else if (control is NumericUpDown nud)
                        value = nud.Value.ToString();
                    else if (control is RadioButton rb)
                        value = rb.Checked.ToString();
                    else if (control is CheckBox cbx)
                        value = cbx.Checked.ToString();
                    else if (control is Form f)
                        value = $"{f.Size.Width},{f.Size.Height};{f.Location.X},{f.Location.Y}";

                    if (value == null)
                        continue;

                    AddOrSet(control.Name, value);
                }

                if (openFileDialog.FileName != "")
                    AddOrSet("OpenFileDialog", Path.GetDirectoryName(openFileDialog.FileName));
                if (saveFileDialog.FileName != "")
                    AddOrSet("SaveFileDialog", Path.GetDirectoryName(saveFileDialog.FileName));
                if (inputFolderBrowserDialog.SelectedPath != "")
                    AddOrSet("InputFolderDialog", inputFolderBrowserDialog.SelectedPath);
                if (outputFolderBrowserDialog.SelectedPath != "")
                    AddOrSet("OutputFolderDialog", outputFolderBrowserDialog.SelectedPath);
                config.Save(ConfigurationSaveMode.Modified, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save settings.\n\n" + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                configMutex.ReleaseMutex();
            }
        }

        String LoadSettings(String key)
        {
            var setting = config.AppSettings.Settings[key];
            if (setting == null)
                return "";
            return config.AppSettings.Settings[key]?.Value;
        }

        void AddOrSet(String key, String value)
        {
            if (config.AppSettings.Settings[key] != null)
                config.AppSettings.Settings[key].Value = value;
            else
                config.AppSettings.Settings.Add(key, value);
        }
        #endregion
    }
}
