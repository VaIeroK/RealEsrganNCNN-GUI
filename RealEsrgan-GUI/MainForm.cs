using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace RealEsrgan_GUI
{
    public partial class MainForm : Form
    {
        private Control[] controlsToSave = null;
        private readonly string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppSettings.config");
        private Configuration config;
        private Esrgan esrgan = null;

        public MainForm(string[] args)
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
                ModelComboBox,
                TitleSizeUpDown,
                TTAModeCheckBox,
                AutoTitleSizeCheckBox,
                InputTextBox,
                OutputTextBox
            };

            ScaleComboBox.SelectedIndex = 0;

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

            if (args.Length > 0 && System.IO.File.Exists(args[0]))
            {
                InputTextBox.Text = args[0];
                FileRadioButton.Checked = true;
                string inputDir = Path.GetDirectoryName(InputTextBox.Text);
                string inputFileNameWithoutExt = Path.GetFileNameWithoutExtension(InputTextBox.Text);
                string outputFilePath = Path.Combine(inputDir, $"{inputFileNameWithoutExt}_upscaled.png");
                OutputTextBox.Text = outputFilePath;
            }
            else if (args.Length > 0 && Directory.Exists(args[0]))
            {
                InputTextBox.Text = args[0];
                FolderRadioButton.Checked = true;
                OutputTextBox.Text = args[0];
            }
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

            if ((FileRadioButton.Checked && !System.IO.File.Exists(InputTextBox.Text)) ||
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
                esrgan.RunFile(InputTextBox.Text, OutputTextBox.Text, model, scale, titleSize, TTAModeCheckBox.Checked);
            else if (FolderRadioButton.Checked)
                esrgan.RunFolder(InputTextBox.Text, OutputTextBox.Text, model, scale, titleSize, TTAModeCheckBox.Checked);
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

        private void AutoTitleSizeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            TitleSizeUpDown.Enabled = !AutoTitleSizeCheckBox.Checked;
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (esrgan.IsRunning())
                ConsoleOutputRichTextBox.AppendText("ESRGAN stopped" + Environment.NewLine);
            else
                ConsoleOutputRichTextBox.AppendText("Failed to stop ESRGAN" + Environment.NewLine);
        }

        private void AddRegButton_Click(object sender, EventArgs e)
        {
            string appPath = Assembly.GetExecutingAssembly().Location;
            string menuName = "RealEsrgan Upscale";
            string[] extensions = { ".png", ".jpg", ".jpeg", ".webp" };

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

        private void RemoveRegButton_Click(object sender, EventArgs e)
        {
            string[] extensions = { ".png", ".jpg", ".jpeg", ".webp" };

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
    }
}
