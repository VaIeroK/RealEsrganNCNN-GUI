using System;
using System.Configuration;
using System.Windows.Forms;
using System.Threading;

namespace RealEsrgan_GUI
{
    internal class SettingsManager : IDisposable
    {
        private readonly Configuration config;
        private readonly string configPath;
        private static readonly Mutex configMutex = new Mutex(false, "RealEsrganUpscale_ConfigMutex");
        private bool disposed;

        public SettingsManager(string configPath)
        {
            this.configPath = configPath;
            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = configPath };
            config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }

        public string Get(string key) => config.AppSettings.Settings[key]?.Value ?? "";

        public void Set(string key, string value)
        {
            try
            {
                configMutex.WaitOne();
                AddOrSet(key, value);
            }
            finally
            {
                configMutex.ReleaseMutex();
            }
        }

        public void Save()
        {
            try
            {
                configMutex.WaitOne();
                config.Save(ConfigurationSaveMode.Modified, false);
            }
            catch
            {
                // swallow - callers handle UI errors if needed
            }
            finally
            {
                configMutex.ReleaseMutex();
            }
        }

        public void LoadControls(Control[] controls)
        {
            if (controls == null) return;

            foreach (var control in controls)
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
        }

        public void SaveControls(Control[] controls)
        {
            if (controls == null) return;

            try
            {
                configMutex.WaitOne();

                foreach (var control in controls)
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
            }
            finally
            {
                configMutex.ReleaseMutex();
            }
        }

        private void AddOrSet(string key, string value)
        {
            if (config.AppSettings.Settings[key] != null)
                config.AppSettings.Settings[key].Value = value;
            else
                config.AppSettings.Settings.Add(key, value);
        }

        public void Dispose()
        {
            if (disposed) return;
            try
            {
                Save();
            }
            catch { }
            disposed = true;
        }
    }
}