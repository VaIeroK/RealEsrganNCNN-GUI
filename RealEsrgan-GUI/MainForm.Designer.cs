namespace RealEsrgan_GUI
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.BrowseInputButton = new System.Windows.Forms.Button();
            this.BrowseOutputButton = new System.Windows.Forms.Button();
            this.ScaleComboBox = new System.Windows.Forms.ComboBox();
            this.ModelComboBox = new System.Windows.Forms.ComboBox();
            this.TTAModeCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ConsoleOutputRichTextBox = new System.Windows.Forms.RichTextBox();
            this.RunButton = new System.Windows.Forms.Button();
            this.ScaleLabel = new System.Windows.Forms.Label();
            this.ModelLabel = new System.Windows.Forms.Label();
            this.InputPathLabel = new System.Windows.Forms.Label();
            this.OutputPathLabel = new System.Windows.Forms.Label();
            this.OutputTextBox = new System.Windows.Forms.TextBox();
            this.InputTextBox = new System.Windows.Forms.TextBox();
            this.TitleSizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.TitleSizeLabel = new System.Windows.Forms.Label();
            this.FileRadioButton = new System.Windows.Forms.RadioButton();
            this.SelectModeGroupBox = new System.Windows.Forms.GroupBox();
            this.FolderRadioButton = new System.Windows.Forms.RadioButton();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.inputFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.AutoTitleSizeCheckBox = new System.Windows.Forms.CheckBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.outputFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.StopButton = new System.Windows.Forms.Button();
            this.AddRegButton = new System.Windows.Forms.Button();
            this.RemoveRegButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TitleSizeUpDown)).BeginInit();
            this.SelectModeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BrowseInputButton
            // 
            this.BrowseInputButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseInputButton.Location = new System.Drawing.Point(451, 5);
            this.BrowseInputButton.Margin = new System.Windows.Forms.Padding(2);
            this.BrowseInputButton.Name = "BrowseInputButton";
            this.BrowseInputButton.Size = new System.Drawing.Size(61, 20);
            this.BrowseInputButton.TabIndex = 0;
            this.BrowseInputButton.Text = "Browse";
            this.BrowseInputButton.UseVisualStyleBackColor = true;
            this.BrowseInputButton.Click += new System.EventHandler(this.BrowseInputButton_Click);
            // 
            // BrowseOutputButton
            // 
            this.BrowseOutputButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseOutputButton.Location = new System.Drawing.Point(451, 29);
            this.BrowseOutputButton.Margin = new System.Windows.Forms.Padding(2);
            this.BrowseOutputButton.Name = "BrowseOutputButton";
            this.BrowseOutputButton.Size = new System.Drawing.Size(61, 20);
            this.BrowseOutputButton.TabIndex = 1;
            this.BrowseOutputButton.Text = "Browse";
            this.BrowseOutputButton.UseVisualStyleBackColor = true;
            this.BrowseOutputButton.Click += new System.EventHandler(this.BrowseOutputButton_Click);
            // 
            // ScaleComboBox
            // 
            this.ScaleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ScaleComboBox.FormattingEnabled = true;
            this.ScaleComboBox.Items.AddRange(new object[] {
            "x2",
            "x3",
            "x4"});
            this.ScaleComboBox.Location = new System.Drawing.Point(61, 53);
            this.ScaleComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.ScaleComboBox.Name = "ScaleComboBox";
            this.ScaleComboBox.Size = new System.Drawing.Size(62, 21);
            this.ScaleComboBox.TabIndex = 2;
            // 
            // ModelComboBox
            // 
            this.ModelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ModelComboBox.FormattingEnabled = true;
            this.ModelComboBox.Location = new System.Drawing.Point(61, 79);
            this.ModelComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.ModelComboBox.Name = "ModelComboBox";
            this.ModelComboBox.Size = new System.Drawing.Size(125, 21);
            this.ModelComboBox.TabIndex = 3;
            // 
            // TTAModeCheckBox
            // 
            this.TTAModeCheckBox.AutoSize = true;
            this.TTAModeCheckBox.Location = new System.Drawing.Point(10, 130);
            this.TTAModeCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.TTAModeCheckBox.Name = "TTAModeCheckBox";
            this.TTAModeCheckBox.Size = new System.Drawing.Size(76, 17);
            this.TTAModeCheckBox.TabIndex = 5;
            this.TTAModeCheckBox.Text = "TTA mode";
            this.TTAModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.ConsoleOutputRichTextBox);
            this.groupBox1.Location = new System.Drawing.Point(6, 151);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(624, 205);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Output";
            // 
            // ConsoleOutputRichTextBox
            // 
            this.ConsoleOutputRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConsoleOutputRichTextBox.Location = new System.Drawing.Point(2, 15);
            this.ConsoleOutputRichTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.ConsoleOutputRichTextBox.Name = "ConsoleOutputRichTextBox";
            this.ConsoleOutputRichTextBox.ReadOnly = true;
            this.ConsoleOutputRichTextBox.Size = new System.Drawing.Size(620, 188);
            this.ConsoleOutputRichTextBox.TabIndex = 0;
            this.ConsoleOutputRichTextBox.Text = "";
            // 
            // RunButton
            // 
            this.RunButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RunButton.Location = new System.Drawing.Point(559, 127);
            this.RunButton.Margin = new System.Windows.Forms.Padding(2);
            this.RunButton.Name = "RunButton";
            this.RunButton.Size = new System.Drawing.Size(71, 25);
            this.RunButton.TabIndex = 8;
            this.RunButton.Text = "Run";
            this.RunButton.UseVisualStyleBackColor = true;
            this.RunButton.Click += new System.EventHandler(this.RunButton_Click);
            // 
            // ScaleLabel
            // 
            this.ScaleLabel.AutoSize = true;
            this.ScaleLabel.Location = new System.Drawing.Point(7, 56);
            this.ScaleLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ScaleLabel.Name = "ScaleLabel";
            this.ScaleLabel.Size = new System.Drawing.Size(34, 13);
            this.ScaleLabel.TabIndex = 9;
            this.ScaleLabel.Text = "Scale";
            // 
            // ModelLabel
            // 
            this.ModelLabel.AutoSize = true;
            this.ModelLabel.Location = new System.Drawing.Point(7, 82);
            this.ModelLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ModelLabel.Name = "ModelLabel";
            this.ModelLabel.Size = new System.Drawing.Size(36, 13);
            this.ModelLabel.TabIndex = 10;
            this.ModelLabel.Text = "Model";
            // 
            // InputPathLabel
            // 
            this.InputPathLabel.AutoSize = true;
            this.InputPathLabel.Location = new System.Drawing.Point(7, 8);
            this.InputPathLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.InputPathLabel.Name = "InputPathLabel";
            this.InputPathLabel.Size = new System.Drawing.Size(31, 13);
            this.InputPathLabel.TabIndex = 12;
            this.InputPathLabel.Text = "Input";
            // 
            // OutputPathLabel
            // 
            this.OutputPathLabel.AutoSize = true;
            this.OutputPathLabel.Location = new System.Drawing.Point(7, 32);
            this.OutputPathLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.OutputPathLabel.Name = "OutputPathLabel";
            this.OutputPathLabel.Size = new System.Drawing.Size(39, 13);
            this.OutputPathLabel.TabIndex = 13;
            this.OutputPathLabel.Text = "Output";
            // 
            // OutputTextBox
            // 
            this.OutputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputTextBox.Location = new System.Drawing.Point(61, 29);
            this.OutputTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.OutputTextBox.Name = "OutputTextBox";
            this.OutputTextBox.Size = new System.Drawing.Size(386, 20);
            this.OutputTextBox.TabIndex = 14;
            // 
            // InputTextBox
            // 
            this.InputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InputTextBox.Location = new System.Drawing.Point(61, 5);
            this.InputTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.InputTextBox.Name = "InputTextBox";
            this.InputTextBox.Size = new System.Drawing.Size(386, 20);
            this.InputTextBox.TabIndex = 15;
            // 
            // TitleSizeUpDown
            // 
            this.TitleSizeUpDown.Location = new System.Drawing.Point(61, 105);
            this.TitleSizeUpDown.Maximum = new decimal(new int[] {
            512,
            0,
            0,
            0});
            this.TitleSizeUpDown.Minimum = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.TitleSizeUpDown.Name = "TitleSizeUpDown";
            this.TitleSizeUpDown.Size = new System.Drawing.Size(62, 20);
            this.TitleSizeUpDown.TabIndex = 16;
            this.TitleSizeUpDown.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
            // 
            // TitleSizeLabel
            // 
            this.TitleSizeLabel.AutoSize = true;
            this.TitleSizeLabel.Location = new System.Drawing.Point(7, 107);
            this.TitleSizeLabel.Name = "TitleSizeLabel";
            this.TitleSizeLabel.Size = new System.Drawing.Size(48, 13);
            this.TitleSizeLabel.TabIndex = 17;
            this.TitleSizeLabel.Text = "Title size";
            // 
            // FileRadioButton
            // 
            this.FileRadioButton.AutoSize = true;
            this.FileRadioButton.Checked = true;
            this.FileRadioButton.Location = new System.Drawing.Point(6, 14);
            this.FileRadioButton.Name = "FileRadioButton";
            this.FileRadioButton.Size = new System.Drawing.Size(41, 17);
            this.FileRadioButton.TabIndex = 18;
            this.FileRadioButton.TabStop = true;
            this.FileRadioButton.Text = "File";
            this.FileRadioButton.UseVisualStyleBackColor = true;
            // 
            // SelectModeGroupBox
            // 
            this.SelectModeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectModeGroupBox.Controls.Add(this.FolderRadioButton);
            this.SelectModeGroupBox.Controls.Add(this.FileRadioButton);
            this.SelectModeGroupBox.Location = new System.Drawing.Point(517, 5);
            this.SelectModeGroupBox.Name = "SelectModeGroupBox";
            this.SelectModeGroupBox.Size = new System.Drawing.Size(113, 39);
            this.SelectModeGroupBox.TabIndex = 19;
            this.SelectModeGroupBox.TabStop = false;
            this.SelectModeGroupBox.Text = "Select mode";
            // 
            // FolderRadioButton
            // 
            this.FolderRadioButton.AutoSize = true;
            this.FolderRadioButton.Location = new System.Drawing.Point(53, 14);
            this.FolderRadioButton.Name = "FolderRadioButton";
            this.FolderRadioButton.Size = new System.Drawing.Size(54, 17);
            this.FolderRadioButton.TabIndex = 19;
            this.FolderRadioButton.Text = "Folder";
            this.FolderRadioButton.UseVisualStyleBackColor = true;
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp;*.tga;*.ppm)|*.png;*.jpg;*.jpeg;*.bmp;*.tga" +
    ";*.ppm|Все файлы (*.*)|*.*";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "PNG изображение (*.png)|*.png|JPEG изображение (*.jpg;*.jpeg)|*.jpg;*.jpeg|WebP и" +
    "зображение (*.webp)|*.webp";
            // 
            // AutoTitleSizeCheckBox
            // 
            this.AutoTitleSizeCheckBox.AutoSize = true;
            this.AutoTitleSizeCheckBox.Checked = true;
            this.AutoTitleSizeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoTitleSizeCheckBox.Location = new System.Drawing.Point(129, 107);
            this.AutoTitleSizeCheckBox.Name = "AutoTitleSizeCheckBox";
            this.AutoTitleSizeCheckBox.Size = new System.Drawing.Size(47, 17);
            this.AutoTitleSizeCheckBox.TabIndex = 20;
            this.AutoTitleSizeCheckBox.Text = "auto";
            this.AutoTitleSizeCheckBox.UseVisualStyleBackColor = true;
            this.AutoTitleSizeCheckBox.CheckedChanged += new System.EventHandler(this.AutoTitleSizeCheckBox_CheckedChanged);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 30000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // StopButton
            // 
            this.StopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.StopButton.Location = new System.Drawing.Point(484, 127);
            this.StopButton.Margin = new System.Windows.Forms.Padding(2);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(71, 25);
            this.StopButton.TabIndex = 21;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // AddRegButton
            // 
            this.AddRegButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddRegButton.Location = new System.Drawing.Point(476, 64);
            this.AddRegButton.Margin = new System.Windows.Forms.Padding(2);
            this.AddRegButton.Name = "AddRegButton";
            this.AddRegButton.Size = new System.Drawing.Size(154, 22);
            this.AddRegButton.TabIndex = 22;
            this.AddRegButton.Text = "Add to context menu";
            this.AddRegButton.UseVisualStyleBackColor = true;
            this.AddRegButton.Click += new System.EventHandler(this.AddRegButton_Click);
            // 
            // RemoveRegButton
            // 
            this.RemoveRegButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RemoveRegButton.Location = new System.Drawing.Point(476, 90);
            this.RemoveRegButton.Margin = new System.Windows.Forms.Padding(2);
            this.RemoveRegButton.Name = "RemoveRegButton";
            this.RemoveRegButton.Size = new System.Drawing.Size(154, 22);
            this.RemoveRegButton.TabIndex = 23;
            this.RemoveRegButton.Text = "Remove from context menu";
            this.RemoveRegButton.UseVisualStyleBackColor = true;
            this.RemoveRegButton.Click += new System.EventHandler(this.RemoveRegButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(637, 367);
            this.Controls.Add(this.RemoveRegButton);
            this.Controls.Add(this.AddRegButton);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.AutoTitleSizeCheckBox);
            this.Controls.Add(this.SelectModeGroupBox);
            this.Controls.Add(this.TitleSizeLabel);
            this.Controls.Add(this.TitleSizeUpDown);
            this.Controls.Add(this.InputTextBox);
            this.Controls.Add(this.OutputTextBox);
            this.Controls.Add(this.OutputPathLabel);
            this.Controls.Add(this.InputPathLabel);
            this.Controls.Add(this.ModelLabel);
            this.Controls.Add(this.ScaleLabel);
            this.Controls.Add(this.RunButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.TTAModeCheckBox);
            this.Controls.Add(this.ModelComboBox);
            this.Controls.Add(this.ScaleComboBox);
            this.Controls.Add(this.BrowseOutputButton);
            this.Controls.Add(this.BrowseInputButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(395, 385);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RealEsrgan GUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TitleSizeUpDown)).EndInit();
            this.SelectModeGroupBox.ResumeLayout(false);
            this.SelectModeGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BrowseInputButton;
        private System.Windows.Forms.Button BrowseOutputButton;
        private System.Windows.Forms.ComboBox ScaleComboBox;
        private System.Windows.Forms.ComboBox ModelComboBox;
        private System.Windows.Forms.CheckBox TTAModeCheckBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RichTextBox ConsoleOutputRichTextBox;
        private System.Windows.Forms.Button RunButton;
        private System.Windows.Forms.Label ScaleLabel;
        private System.Windows.Forms.Label ModelLabel;
        private System.Windows.Forms.Label InputPathLabel;
        private System.Windows.Forms.Label OutputPathLabel;
        private System.Windows.Forms.TextBox OutputTextBox;
        private System.Windows.Forms.TextBox InputTextBox;
        private System.Windows.Forms.NumericUpDown TitleSizeUpDown;
        private System.Windows.Forms.Label TitleSizeLabel;
        private System.Windows.Forms.RadioButton FileRadioButton;
        private System.Windows.Forms.GroupBox SelectModeGroupBox;
        private System.Windows.Forms.RadioButton FolderRadioButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.FolderBrowserDialog inputFolderBrowserDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.CheckBox AutoTitleSizeCheckBox;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.FolderBrowserDialog outputFolderBrowserDialog;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Button AddRegButton;
        private System.Windows.Forms.Button RemoveRegButton;
    }
}

