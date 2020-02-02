namespace HeadlessFormsExperiment
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectSimioModelFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textLogs = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.panel1 = new System.Windows.Forms.Panel();
            this.comboHeadlessExperiments = new System.Windows.Forms.ComboBox();
            this.labelExtensionsPath = new System.Windows.Forms.Label();
            this.comboHeadlessRunModels = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbHeadlessSaveModelAfterRun = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.buttonRunExperiment = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.textHeadlessProjectFile = new System.Windows.Forms.TextBox();
            this.buttonHeadlessSelectModel = new System.Windows.Forms.Button();
            this.timerLogs = new System.Windows.Forms.Timer(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.actionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1149, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // actionsToolStripMenuItem
            // 
            this.actionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectSimioModelFileToolStripMenuItem,
            this.runModelToolStripMenuItem});
            this.actionsToolStripMenuItem.Name = "actionsToolStripMenuItem";
            this.actionsToolStripMenuItem.Size = new System.Drawing.Size(70, 24);
            this.actionsToolStripMenuItem.Text = "&Actions";
            // 
            // selectSimioModelFileToolStripMenuItem
            // 
            this.selectSimioModelFileToolStripMenuItem.Name = "selectSimioModelFileToolStripMenuItem";
            this.selectSimioModelFileToolStripMenuItem.Size = new System.Drawing.Size(249, 26);
            this.selectSimioModelFileToolStripMenuItem.Text = "&Select Simio Model File...";
            // 
            // runModelToolStripMenuItem
            // 
            this.runModelToolStripMenuItem.Name = "runModelToolStripMenuItem";
            this.runModelToolStripMenuItem.Size = new System.Drawing.Size(249, 26);
            this.runModelToolStripMenuItem.Text = "&Run Model";
            // 
            // textLogs
            // 
            this.textLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textLogs.Location = new System.Drawing.Point(0, 277);
            this.textLogs.Multiline = true;
            this.textLogs.Name = "textLogs";
            this.textLogs.ReadOnly = true;
            this.textLogs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textLogs.Size = new System.Drawing.Size(1149, 325);
            this.textLogs.TabIndex = 1;
            this.textLogs.Text = "(No logs yet...)";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Location = new System.Drawing.Point(0, 602);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1149, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.comboHeadlessExperiments);
            this.panel1.Controls.Add(this.labelExtensionsPath);
            this.panel1.Controls.Add(this.comboHeadlessRunModels);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.buttonRunExperiment);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.textHeadlessProjectFile);
            this.panel1.Controls.Add(this.buttonHeadlessSelectModel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 28);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1149, 249);
            this.panel1.TabIndex = 3;
            // 
            // comboHeadlessExperiments
            // 
            this.comboHeadlessExperiments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboHeadlessExperiments.FormattingEnabled = true;
            this.comboHeadlessExperiments.Location = new System.Drawing.Point(156, 194);
            this.comboHeadlessExperiments.Name = "comboHeadlessExperiments";
            this.comboHeadlessExperiments.Size = new System.Drawing.Size(352, 24);
            this.comboHeadlessExperiments.TabIndex = 40;
            // 
            // labelExtensionsPath
            // 
            this.labelExtensionsPath.AutoSize = true;
            this.labelExtensionsPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelExtensionsPath.Location = new System.Drawing.Point(152, 113);
            this.labelExtensionsPath.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelExtensionsPath.Name = "labelExtensionsPath";
            this.labelExtensionsPath.Size = new System.Drawing.Size(154, 20);
            this.labelExtensionsPath.TabIndex = 39;
            this.labelExtensionsPath.Text = "(Extensions Path...)";
            // 
            // comboHeadlessRunModels
            // 
            this.comboHeadlessRunModels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboHeadlessRunModels.FormattingEnabled = true;
            this.comboHeadlessRunModels.Location = new System.Drawing.Point(156, 152);
            this.comboHeadlessRunModels.Name = "comboHeadlessRunModels";
            this.comboHeadlessRunModels.Size = new System.Drawing.Size(352, 24);
            this.comboHeadlessRunModels.TabIndex = 38;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(11, 18);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(854, 20);
            this.label1.TabIndex = 36;
            this.label1.Text = "This Sample Project assumes the DLL files are in the same folder as this EXE. It " +
    "is for running Experiments only.";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 152);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(95, 17);
            this.label7.TabIndex = 35;
            this.label7.Text = "Model Name: ";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbHeadlessSaveModelAfterRun);
            this.groupBox1.Location = new System.Drawing.Point(565, 152);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(307, 75);
            this.groupBox1.TabIndex = 33;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Post-Run Actions";
            // 
            // cbHeadlessSaveModelAfterRun
            // 
            this.cbHeadlessSaveModelAfterRun.AutoSize = true;
            this.cbHeadlessSaveModelAfterRun.Checked = true;
            this.cbHeadlessSaveModelAfterRun.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbHeadlessSaveModelAfterRun.Location = new System.Drawing.Point(19, 37);
            this.cbHeadlessSaveModelAfterRun.Name = "cbHeadlessSaveModelAfterRun";
            this.cbHeadlessSaveModelAfterRun.Size = new System.Drawing.Size(168, 21);
            this.cbHeadlessSaveModelAfterRun.TabIndex = 15;
            this.cbHeadlessSaveModelAfterRun.Text = "Save Model After Run";
            this.cbHeadlessSaveModelAfterRun.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 50);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(146, 17);
            this.label9.TabIndex = 32;
            this.label9.Text = "Project Name (.SPFX)";
            // 
            // buttonRunExperiment
            // 
            this.buttonRunExperiment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRunExperiment.Location = new System.Drawing.Point(950, 123);
            this.buttonRunExperiment.Name = "buttonRunExperiment";
            this.buttonRunExperiment.Size = new System.Drawing.Size(187, 40);
            this.buttonRunExperiment.TabIndex = 31;
            this.buttonRunExperiment.Text = "Run Experiment";
            this.buttonRunExperiment.UseVisualStyleBackColor = true;
            this.buttonRunExperiment.Click += new System.EventHandler(this.buttonRunExperiment_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 194);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(123, 17);
            this.label6.TabIndex = 30;
            this.label6.Text = "Experiment Name:";
            // 
            // textHeadlessProjectFile
            // 
            this.textHeadlessProjectFile.Location = new System.Drawing.Point(11, 73);
            this.textHeadlessProjectFile.Name = "textHeadlessProjectFile";
            this.textHeadlessProjectFile.Size = new System.Drawing.Size(861, 22);
            this.textHeadlessProjectFile.TabIndex = 28;
            // 
            // buttonHeadlessSelectModel
            // 
            this.buttonHeadlessSelectModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHeadlessSelectModel.Location = new System.Drawing.Point(950, 64);
            this.buttonHeadlessSelectModel.Name = "buttonHeadlessSelectModel";
            this.buttonHeadlessSelectModel.Size = new System.Drawing.Size(187, 40);
            this.buttonHeadlessSelectModel.TabIndex = 27;
            this.buttonHeadlessSelectModel.Text = "Select Simio Project";
            this.buttonHeadlessSelectModel.UseVisualStyleBackColor = true;
            this.buttonHeadlessSelectModel.Click += new System.EventHandler(this.buttonHeadlessSelectModel_Click);
            // 
            // timerLogs
            // 
            this.timerLogs.Interval = 1000;
            this.timerLogs.Tick += new System.EventHandler(this.timerLogs_Tick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 113);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 17);
            this.label2.TabIndex = 41;
            this.label2.Text = "Extensions Path:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1149, 624);
            this.Controls.Add(this.textLogs);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Run Experiments using WinForms";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem actionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectSimioModelFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runModelToolStripMenuItem;
        private System.Windows.Forms.TextBox textLogs;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button buttonRunExperiment;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textHeadlessProjectFile;
        private System.Windows.Forms.Button buttonHeadlessSelectModel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cbHeadlessSaveModelAfterRun;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timerLogs;
        private System.Windows.Forms.Label labelExtensionsPath;
        private System.Windows.Forms.ComboBox comboHeadlessRunModels;
        private System.Windows.Forms.ComboBox comboHeadlessExperiments;
        private System.Windows.Forms.Label label2;
    }
}

