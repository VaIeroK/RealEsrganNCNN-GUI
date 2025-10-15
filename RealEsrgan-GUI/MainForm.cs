using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RealEsrgan_GUI
{
    public partial class MainForm : Form
    {
        private Control[] controlsToSave = null;
        private readonly string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppSettings.config");
        private Configuration config;
        private Process esrgan = null;

        public MainForm()
        {
            InitializeComponent();

            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = configPath };
            config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            controlsToSave = new Control[] 
            { 
                this,
                FileRadioButton, 
                FolderRadioButton,
                ScaleComboBox,
                OutputFormatComboBox,
                ModelComboBox,
                TitleSizeUpDown,
                TTAModeCheckBox,
                VerboseOutputCheckBox,
                AutoTitleSizeCheckBox,
                InputTextBox,
                OutputTextBox
            };

            ScaleComboBox.SelectedIndex = 0;
            OutputFormatComboBox.SelectedIndex = 0;

            string modelsPath = "models";
            var files = Directory.GetFiles(modelsPath);
            HashSet<string> uniqueNames = new HashSet<string>(
                files.Select(f => Path.GetFileNameWithoutExtension(f))
            );
            foreach (var name in uniqueNames)
                ModelComboBox.Items.Add(name);

            if (ModelComboBox.Items.Count > 0)
                ModelComboBox.SelectedIndex = 0;

            toolTip.SetToolTip(TTAModeCheckBox, "TTA (Test-Time Augmentation) improves image quality by processing multiple augmented versions and averaging the results.");
            toolTip.SetToolTip(VerboseOutputCheckBox, "Shows full paths of input and output images during processing.");
            toolTip.SetToolTip(TitleSizeLabel, "Tile size splits image into blocks of this size to reduce GPU memory usage.");
            toolTip.SetToolTip(TitleSizeUpDown, "Tile size splits image into blocks of this size to reduce GPU memory usage.");
            toolTip.SetToolTip(AutoTitleSizeCheckBox, "Tile size splits image into blocks of this size to reduce GPU memory usage.");
            toolTip.SetToolTip(FormatLabel, "Output image format.");
            toolTip.SetToolTip(OutputFormatComboBox, "Output image format.");
            toolTip.SetToolTip(ScaleLabel, "Output image image enlargement factor.");
            toolTip.SetToolTip(ScaleComboBox, "Output image image enlargement factor.");
            toolTip.SetToolTip(ModelLabel, "Choose the pre-trained upscaling model.");
            toolTip.SetToolTip(ModelComboBox, "Choose the pre-trained upscaling model.");

            LoadSettings();
            AutoTitleSizeCheckBox_CheckedChanged(null, null);
        }

        private void BrowseInputButton_Click(object sender, EventArgs e)
        {
            if (FileRadioButton.Checked)
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    InputTextBox.Text = openFileDialog.FileName;
                }
            }
            else if (FolderRadioButton.Checked)
            {
                if (inputFolderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    InputTextBox.Text = inputFolderBrowserDialog.SelectedPath;
                }
            }
        }

        private void BrowseOutputButton_Click(object sender, EventArgs e)
        {
            if (FileRadioButton.Checked)
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    OutputTextBox.Text = saveFileDialog.FileName;
                }
            }
            else if (FolderRadioButton.Checked)
            {
                if (outputFolderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    OutputTextBox.Text = outputFolderBrowserDialog.SelectedPath;
                }
            }
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            if (esrgan != null && !esrgan.HasExited)
            {
                MessageBox.Show(
                    "Upscaling is already in progress",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            if ((FileRadioButton.Checked && !File.Exists(InputTextBox.Text)) ||
                (FolderRadioButton.Checked && !Directory.Exists(InputTextBox.Text)))
            {
                MessageBox.Show(FileRadioButton.Checked ? "Input file does not exist!" : "Input folder does not exist!",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (FolderRadioButton.Checked && !Directory.Exists(OutputTextBox.Text))
            {
                MessageBox.Show("Output folder does not exist!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ModelComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a model!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = "realesrgan-ncnn-vulkan.exe",
                Arguments = BuildEsrganArgs(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            esrgan = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            esrgan.OutputDataReceived += (s, e1) =>
            {
                if (e1.Data != null)
                {
                    ConsoleOutputRichTextBox.Invoke((Action)(() =>
                    {
                        ConsoleOutputRichTextBox.AppendText(e1.Data + Environment.NewLine);
                    }));
                }
            };

            esrgan.ErrorDataReceived += (s, e1) =>
            {
                if (e1.Data != null)
                {
                    ConsoleOutputRichTextBox.Invoke((Action)(() =>
                    {
                        ConsoleOutputRichTextBox.AppendText(e1.Data + Environment.NewLine);
                    }));
                }
            };

            esrgan.Exited += (s, e1) =>
            {
                ConsoleOutputRichTextBox.Invoke((Action)(() =>
                {
                    if (esrgan.ExitCode == 0)
                        ConsoleOutputRichTextBox.AppendText("Upscale Finished!" + Environment.NewLine);
                }));
            };

            ConsoleOutputRichTextBox.Clear();
            if (Debugger.IsAttached)
                ConsoleOutputRichTextBox.AppendText(BuildEsrganArgs() + Environment.NewLine);
            esrgan.Start();

            esrgan.BeginOutputReadLine();
            esrgan.BeginErrorReadLine();
        }

        private string BuildEsrganArgs()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"-i \"{InputTextBox.Text}\" ");
            sb.Append($"-o \"{OutputTextBox.Text}\" ");
            sb.Append($"-s {ScaleComboBox.SelectedItem.ToString().Replace("x", "")} ");
            sb.Append($"-n \"{ModelComboBox.SelectedItem.ToString()}\" ");
            sb.Append($"-f {OutputFormatComboBox.SelectedItem.ToString().ToLower()} ");
            if (TitleSizeUpDown.Enabled)
                sb.Append($"-t {TitleSizeUpDown.Value} ");
            if (TTAModeCheckBox.Checked)
                sb.Append($"-x ");
            if (VerboseOutputCheckBox.Checked)
                sb.Append($"-v ");

            return sb.ToString();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
            StopButton_Click(null, null);
        }

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
                    case ComboBox cb: cb.SelectedIndex = int.TryParse(value, out int ci) ? ci : 0; break;
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
            config.Save(ConfigurationSaveMode.Modified);
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

        private String GetFileFormatFromTextBox(TextBox textBox)
        {
            if (textBox == null)
                throw new ArgumentNullException(nameof(textBox));

            string path = textBox.Text.Trim();
            if (string.IsNullOrEmpty(path))
                return null;

            if (Directory.Exists(path))
                return null;

            string ext = Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext))
                return null;

            return ext.TrimStart('.').ToLowerInvariant();
        }

        private void AutoTitleSizeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            TitleSizeUpDown.Enabled = !AutoTitleSizeCheckBox.Checked;
        }

        private void OutputTextBox_TextChanged(object sender, EventArgs e)
        {
            string format = GetFileFormatFromTextBox(OutputTextBox);
            if (format != null)
                OutputFormatComboBox.SelectedItem = format;
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (esrgan != null && !esrgan.HasExited)
            {
                try
                {
                    esrgan.Kill();
                    ConsoleOutputRichTextBox.AppendText("ESRGAN stopped" + Environment.NewLine);
                }
                catch 
                {
                    ConsoleOutputRichTextBox.AppendText("Failed to stop ESRGAN" + Environment.NewLine);
                }
            }
        }
    }
}
