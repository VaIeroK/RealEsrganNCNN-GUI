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
            this.ScaleComboBox = new System.Windows.Forms.ComboBox();
            this.ModelComboBox = new System.Windows.Forms.ComboBox();
            this.TTAModeCheckBox = new System.Windows.Forms.CheckBox();
            this.OutputGroupBox = new System.Windows.Forms.GroupBox();
            this.ConsoleOutputRichTextBox = new System.Windows.Forms.RichTextBox();
            this.RunButton = new System.Windows.Forms.Button();
            this.ScaleLabel = new System.Windows.Forms.Label();
            this.ModelLabel = new System.Windows.Forms.Label();
            this.InputPathCaptionLabel = new System.Windows.Forms.Label();
            this.TitleSizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.TitleSizeLabel = new System.Windows.Forms.Label();
            this.FileRadioButton = new System.Windows.Forms.RadioButton();
            this.SelectModeGroupBox = new System.Windows.Forms.GroupBox();
            this.FolderRadioButton = new System.Windows.Forms.RadioButton();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.inputFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.AutoTitleSizeCheckBox = new System.Windows.Forms.CheckBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.StopButton = new System.Windows.Forms.Button();
            this.InputLabel = new System.Windows.Forms.Label();
            this.InOutPanel = new System.Windows.Forms.Panel();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToContextMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeFromContextMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ClearVideoFoldersButton = new System.Windows.Forms.Button();
            this.OutputGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TitleSizeUpDown)).BeginInit();
            this.SelectModeGroupBox.SuspendLayout();
            this.InOutPanel.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // BrowseInputButton
            // 
            this.BrowseInputButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseInputButton.Location = new System.Drawing.Point(256, 33);
            this.BrowseInputButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.BrowseInputButton.Name = "BrowseInputButton";
            this.BrowseInputButton.Size = new System.Drawing.Size(61, 20);
            this.BrowseInputButton.TabIndex = 0;
            this.BrowseInputButton.Text = "Browse";
            this.BrowseInputButton.UseVisualStyleBackColor = true;
            this.BrowseInputButton.Click += new System.EventHandler(this.BrowseInputButton_Click);
            // 
            // ScaleComboBox
            // 
            this.ScaleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ScaleComboBox.FormattingEnabled = true;
            this.ScaleComboBox.Items.AddRange(new object[] {
            "x2",
            "x3",
            "x4"});
            this.ScaleComboBox.Location = new System.Drawing.Point(61, 58);
            this.ScaleComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ScaleComboBox.Name = "ScaleComboBox";
            this.ScaleComboBox.Size = new System.Drawing.Size(62, 21);
            this.ScaleComboBox.TabIndex = 2;
            // 
            // ModelComboBox
            // 
            this.ModelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ModelComboBox.FormattingEnabled = true;
            this.ModelComboBox.Location = new System.Drawing.Point(61, 84);
            this.ModelComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ModelComboBox.Name = "ModelComboBox";
            this.ModelComboBox.Size = new System.Drawing.Size(125, 21);
            this.ModelComboBox.TabIndex = 3;
            // 
            // TTAModeCheckBox
            // 
            this.TTAModeCheckBox.AutoSize = true;
            this.TTAModeCheckBox.Location = new System.Drawing.Point(10, 135);
            this.TTAModeCheckBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TTAModeCheckBox.Name = "TTAModeCheckBox";
            this.TTAModeCheckBox.Size = new System.Drawing.Size(76, 17);
            this.TTAModeCheckBox.TabIndex = 5;
            this.TTAModeCheckBox.Text = "TTA mode";
            this.TTAModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // OutputGroupBox
            // 
            this.OutputGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputGroupBox.Controls.Add(this.ConsoleOutputRichTextBox);
            this.OutputGroupBox.Location = new System.Drawing.Point(6, 161);
            this.OutputGroupBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.OutputGroupBox.Name = "OutputGroupBox";
            this.OutputGroupBox.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.OutputGroupBox.Size = new System.Drawing.Size(429, 195);
            this.OutputGroupBox.TabIndex = 7;
            this.OutputGroupBox.TabStop = false;
            this.OutputGroupBox.Text = "Output";
            // 
            // ConsoleOutputRichTextBox
            // 
            this.ConsoleOutputRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConsoleOutputRichTextBox.Location = new System.Drawing.Point(2, 15);
            this.ConsoleOutputRichTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ConsoleOutputRichTextBox.Name = "ConsoleOutputRichTextBox";
            this.ConsoleOutputRichTextBox.ReadOnly = true;
            this.ConsoleOutputRichTextBox.Size = new System.Drawing.Size(425, 178);
            this.ConsoleOutputRichTextBox.TabIndex = 0;
            this.ConsoleOutputRichTextBox.Text = "";
            // 
            // RunButton
            // 
            this.RunButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RunButton.Location = new System.Drawing.Point(364, 132);
            this.RunButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
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
            this.ScaleLabel.Location = new System.Drawing.Point(7, 61);
            this.ScaleLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ScaleLabel.Name = "ScaleLabel";
            this.ScaleLabel.Size = new System.Drawing.Size(34, 13);
            this.ScaleLabel.TabIndex = 9;
            this.ScaleLabel.Text = "Scale";
            // 
            // ModelLabel
            // 
            this.ModelLabel.AutoSize = true;
            this.ModelLabel.Location = new System.Drawing.Point(7, 87);
            this.ModelLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ModelLabel.Name = "ModelLabel";
            this.ModelLabel.Size = new System.Drawing.Size(36, 13);
            this.ModelLabel.TabIndex = 10;
            this.ModelLabel.Text = "Model";
            // 
            // InputPathCaptionLabel
            // 
            this.InputPathCaptionLabel.AutoSize = true;
            this.InputPathCaptionLabel.Location = new System.Drawing.Point(7, 36);
            this.InputPathCaptionLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.InputPathCaptionLabel.Name = "InputPathCaptionLabel";
            this.InputPathCaptionLabel.Size = new System.Drawing.Size(34, 13);
            this.InputPathCaptionLabel.TabIndex = 12;
            this.InputPathCaptionLabel.Text = "Input:";
            // 
            // TitleSizeUpDown
            // 
            this.TitleSizeUpDown.Location = new System.Drawing.Point(61, 110);
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
            this.TitleSizeLabel.Location = new System.Drawing.Point(7, 112);
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
            this.SelectModeGroupBox.Location = new System.Drawing.Point(322, 29);
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
            this.openFileDialog.Multiselect = true;
            // 
            // AutoTitleSizeCheckBox
            // 
            this.AutoTitleSizeCheckBox.AutoSize = true;
            this.AutoTitleSizeCheckBox.Checked = true;
            this.AutoTitleSizeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoTitleSizeCheckBox.Location = new System.Drawing.Point(129, 112);
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
            this.StopButton.Location = new System.Drawing.Point(289, 132);
            this.StopButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(71, 25);
            this.StopButton.TabIndex = 21;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // InputLabel
            // 
            this.InputLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InputLabel.AutoSize = true;
            this.InputLabel.Location = new System.Drawing.Point(2, 4);
            this.InputLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.InputLabel.Name = "InputLabel";
            this.InputLabel.Size = new System.Drawing.Size(10, 13);
            this.InputLabel.TabIndex = 24;
            this.InputLabel.Text = "-";
            // 
            // InOutPanel
            // 
            this.InOutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InOutPanel.Controls.Add(this.InputLabel);
            this.InOutPanel.Location = new System.Drawing.Point(49, 32);
            this.InOutPanel.Name = "InOutPanel";
            this.InOutPanel.Size = new System.Drawing.Size(205, 21);
            this.InOutPanel.TabIndex = 26;
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
            this.menuStrip.Size = new System.Drawing.Size(442, 24);
            this.menuStrip.TabIndex = 27;
            this.menuStrip.Text = "menuStrip";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextMenuToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(47, 22);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // contextMenuToolStripMenuItem
            // 
            this.contextMenuToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToContextMenuToolStripMenuItem,
            this.removeFromContextMenuToolStripMenuItem});
            this.contextMenuToolStripMenuItem.Name = "contextMenuToolStripMenuItem";
            this.contextMenuToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.contextMenuToolStripMenuItem.Text = "Context menu";
            // 
            // addToContextMenuToolStripMenuItem
            // 
            this.addToContextMenuToolStripMenuItem.Name = "addToContextMenuToolStripMenuItem";
            this.addToContextMenuToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.addToContextMenuToolStripMenuItem.Text = "Add to context menu";
            this.addToContextMenuToolStripMenuItem.Click += new System.EventHandler(this.addToContextMenuToolStripMenuItem_Click);
            // 
            // removeFromContextMenuToolStripMenuItem
            // 
            this.removeFromContextMenuToolStripMenuItem.Name = "removeFromContextMenuToolStripMenuItem";
            this.removeFromContextMenuToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.removeFromContextMenuToolStripMenuItem.Text = "Remove from context menu";
            this.removeFromContextMenuToolStripMenuItem.Click += new System.EventHandler(this.removeFromContextMenuToolStripMenuItem_Click);
            // 
            // ClearVideoFoldersButton
            // 
            this.ClearVideoFoldersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ClearVideoFoldersButton.Location = new System.Drawing.Point(180, 132);
            this.ClearVideoFoldersButton.Margin = new System.Windows.Forms.Padding(2);
            this.ClearVideoFoldersButton.Name = "ClearVideoFoldersButton";
            this.ClearVideoFoldersButton.Size = new System.Drawing.Size(105, 25);
            this.ClearVideoFoldersButton.TabIndex = 28;
            this.ClearVideoFoldersButton.Text = "Clear temp folders";
            this.ClearVideoFoldersButton.UseVisualStyleBackColor = true;
            this.ClearVideoFoldersButton.Visible = false;
            this.ClearVideoFoldersButton.Click += new System.EventHandler(this.ClearVideoFoldersButton_Click);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(442, 367);
            this.Controls.Add(this.ClearVideoFoldersButton);
            this.Controls.Add(this.InOutPanel);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.AutoTitleSizeCheckBox);
            this.Controls.Add(this.SelectModeGroupBox);
            this.Controls.Add(this.TitleSizeLabel);
            this.Controls.Add(this.TitleSizeUpDown);
            this.Controls.Add(this.InputPathCaptionLabel);
            this.Controls.Add(this.ModelLabel);
            this.Controls.Add(this.ScaleLabel);
            this.Controls.Add(this.RunButton);
            this.Controls.Add(this.OutputGroupBox);
            this.Controls.Add(this.TTAModeCheckBox);
            this.Controls.Add(this.ModelComboBox);
            this.Controls.Add(this.ScaleComboBox);
            this.Controls.Add(this.BrowseInputButton);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MinimumSize = new System.Drawing.Size(385, 355);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RealEsrgan GUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.OutputGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TitleSizeUpDown)).EndInit();
            this.SelectModeGroupBox.ResumeLayout(false);
            this.SelectModeGroupBox.PerformLayout();
            this.InOutPanel.ResumeLayout(false);
            this.InOutPanel.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BrowseInputButton;
        private System.Windows.Forms.ComboBox ScaleComboBox;
        private System.Windows.Forms.ComboBox ModelComboBox;
        private System.Windows.Forms.CheckBox TTAModeCheckBox;
        private System.Windows.Forms.GroupBox OutputGroupBox;
        private System.Windows.Forms.RichTextBox ConsoleOutputRichTextBox;
        private System.Windows.Forms.Button RunButton;
        private System.Windows.Forms.Label ScaleLabel;
        private System.Windows.Forms.Label ModelLabel;
        private System.Windows.Forms.Label InputPathCaptionLabel;
        private System.Windows.Forms.NumericUpDown TitleSizeUpDown;
        private System.Windows.Forms.Label TitleSizeLabel;
        private System.Windows.Forms.RadioButton FileRadioButton;
        private System.Windows.Forms.GroupBox SelectModeGroupBox;
        private System.Windows.Forms.RadioButton FolderRadioButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.FolderBrowserDialog inputFolderBrowserDialog;
        private System.Windows.Forms.CheckBox AutoTitleSizeCheckBox;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Label InputLabel;
        private System.Windows.Forms.Panel InOutPanel;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToContextMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeFromContextMenuToolStripMenuItem;
        private System.Windows.Forms.Button ClearVideoFoldersButton;
    }
}

