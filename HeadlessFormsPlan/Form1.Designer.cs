namespace HeadlessFormsPlan
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonLoadSimioProject = new System.Windows.Forms.Button();
            this.buttonSelectExtensionsPath = new System.Windows.Forms.Button();
            this.textExtensionsPath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboHeadlessRunModels = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbHeadlessRunPublishPlanAfterRun = new System.Windows.Forms.CheckBox();
            this.cbHeadlessRunRiskAnalysis = new System.Windows.Forms.CheckBox();
            this.cbHeadlessSaveModelAfterRun = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.buttonRunPlan = new System.Windows.Forms.Button();
            this.textHeadlessProjectFile = new System.Windows.Forms.TextBox();
            this.buttonHeadlessSelectModel = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.textLogs = new System.Windows.Forms.TextBox();
            this.timerLogs = new System.Windows.Forms.Timer(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.buttonLoadSimioProject);
            this.panel1.Controls.Add(this.buttonSelectExtensionsPath);
            this.panel1.Controls.Add(this.textExtensionsPath);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.comboHeadlessRunModels);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.buttonRunPlan);
            this.panel1.Controls.Add(this.textHeadlessProjectFile);
            this.panel1.Controls.Add(this.buttonHeadlessSelectModel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 28);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1152, 299);
            this.panel1.TabIndex = 4;
            // 
            // buttonLoadSimioProject
            // 
            this.buttonLoadSimioProject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLoadSimioProject.Location = new System.Drawing.Point(950, 138);
            this.buttonLoadSimioProject.Name = "buttonLoadSimioProject";
            this.buttonLoadSimioProject.Size = new System.Drawing.Size(190, 40);
            this.buttonLoadSimioProject.TabIndex = 43;
            this.buttonLoadSimioProject.Text = "Load Simio Project";
            this.buttonLoadSimioProject.UseVisualStyleBackColor = true;
            this.buttonLoadSimioProject.Click += new System.EventHandler(this.buttonLoadSimioProject_Click);
            // 
            // buttonSelectExtensionsPath
            // 
            this.buttonSelectExtensionsPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectExtensionsPath.Location = new System.Drawing.Point(950, 92);
            this.buttonSelectExtensionsPath.Name = "buttonSelectExtensionsPath";
            this.buttonSelectExtensionsPath.Size = new System.Drawing.Size(190, 40);
            this.buttonSelectExtensionsPath.TabIndex = 42;
            this.buttonSelectExtensionsPath.Text = "Select Extensions Path...";
            this.buttonSelectExtensionsPath.UseVisualStyleBackColor = true;
            this.buttonSelectExtensionsPath.Click += new System.EventHandler(this.buttonSelectExtensionsPath_Click);
            // 
            // textExtensionsPath
            // 
            this.textExtensionsPath.Location = new System.Drawing.Point(169, 97);
            this.textExtensionsPath.Name = "textExtensionsPath";
            this.textExtensionsPath.ReadOnly = true;
            this.textExtensionsPath.Size = new System.Drawing.Size(766, 22);
            this.textExtensionsPath.TabIndex = 41;
            this.toolTip1.SetToolTip(this.textExtensionsPath, "Folder holding the assemblies (binaries) necessary to run the Simio project");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 100);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 17);
            this.label2.TabIndex = 40;
            this.label2.Text = "Extensions Path:";
            // 
            // comboHeadlessRunModels
            // 
            this.comboHeadlessRunModels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboHeadlessRunModels.FormattingEnabled = true;
            this.comboHeadlessRunModels.Location = new System.Drawing.Point(169, 202);
            this.comboHeadlessRunModels.Name = "comboHeadlessRunModels";
            this.comboHeadlessRunModels.Size = new System.Drawing.Size(346, 24);
            this.comboHeadlessRunModels.TabIndex = 38;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(10, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1064, 20);
            this.label1.TabIndex = 36;
            this.label1.Text = "This Sample Project assumes the DLL files are in the same folder as this EXE. It " +
    "is for running Model Plans only (Simio RPS license needed)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 205);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(95, 17);
            this.label7.TabIndex = 35;
            this.label7.Text = "Model Name: ";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.cbHeadlessRunPublishPlanAfterRun);
            this.groupBox1.Controls.Add(this.cbHeadlessRunRiskAnalysis);
            this.groupBox1.Controls.Add(this.cbHeadlessSaveModelAfterRun);
            this.groupBox1.Location = new System.Drawing.Point(546, 193);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(375, 100);
            this.groupBox1.TabIndex = 33;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Post-Run Actions";
            // 
            // cbHeadlessRunPublishPlanAfterRun
            // 
            this.cbHeadlessRunPublishPlanAfterRun.AutoSize = true;
            this.cbHeadlessRunPublishPlanAfterRun.Location = new System.Drawing.Point(197, 33);
            this.cbHeadlessRunPublishPlanAfterRun.Name = "cbHeadlessRunPublishPlanAfterRun";
            this.cbHeadlessRunPublishPlanAfterRun.Size = new System.Drawing.Size(172, 21);
            this.cbHeadlessRunPublishPlanAfterRun.TabIndex = 19;
            this.cbHeadlessRunPublishPlanAfterRun.Text = "Publish Plan After Run";
            this.cbHeadlessRunPublishPlanAfterRun.UseVisualStyleBackColor = true;
            this.cbHeadlessRunPublishPlanAfterRun.Visible = false;
            // 
            // cbHeadlessRunRiskAnalysis
            // 
            this.cbHeadlessRunRiskAnalysis.AutoSize = true;
            this.cbHeadlessRunRiskAnalysis.Location = new System.Drawing.Point(6, 33);
            this.cbHeadlessRunRiskAnalysis.Name = "cbHeadlessRunRiskAnalysis";
            this.cbHeadlessRunRiskAnalysis.Size = new System.Drawing.Size(143, 21);
            this.cbHeadlessRunRiskAnalysis.TabIndex = 18;
            this.cbHeadlessRunRiskAnalysis.Text = "Run Risk Analysis";
            this.cbHeadlessRunRiskAnalysis.UseVisualStyleBackColor = true;
            // 
            // cbHeadlessSaveModelAfterRun
            // 
            this.cbHeadlessSaveModelAfterRun.AutoSize = true;
            this.cbHeadlessSaveModelAfterRun.Checked = true;
            this.cbHeadlessSaveModelAfterRun.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbHeadlessSaveModelAfterRun.Location = new System.Drawing.Point(6, 60);
            this.cbHeadlessSaveModelAfterRun.Name = "cbHeadlessSaveModelAfterRun";
            this.cbHeadlessSaveModelAfterRun.Size = new System.Drawing.Size(168, 21);
            this.cbHeadlessSaveModelAfterRun.TabIndex = 15;
            this.cbHeadlessSaveModelAfterRun.Text = "Save Model After Run";
            this.cbHeadlessSaveModelAfterRun.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 57);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(146, 17);
            this.label9.TabIndex = 32;
            this.label9.Text = "Project Name (.SPFX)";
            // 
            // buttonRunPlan
            // 
            this.buttonRunPlan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRunPlan.Location = new System.Drawing.Point(950, 202);
            this.buttonRunPlan.Name = "buttonRunPlan";
            this.buttonRunPlan.Size = new System.Drawing.Size(190, 40);
            this.buttonRunPlan.TabIndex = 31;
            this.buttonRunPlan.Text = "Run Plan";
            this.buttonRunPlan.UseVisualStyleBackColor = true;
            this.buttonRunPlan.Click += new System.EventHandler(this.buttonRunPlan_Click_1);
            // 
            // textHeadlessProjectFile
            // 
            this.textHeadlessProjectFile.Location = new System.Drawing.Point(169, 59);
            this.textHeadlessProjectFile.Name = "textHeadlessProjectFile";
            this.textHeadlessProjectFile.ReadOnly = true;
            this.textHeadlessProjectFile.Size = new System.Drawing.Size(766, 22);
            this.textHeadlessProjectFile.TabIndex = 28;
            this.toolTip1.SetToolTip(this.textHeadlessProjectFile, "The Simio project file");
            // 
            // buttonHeadlessSelectModel
            // 
            this.buttonHeadlessSelectModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHeadlessSelectModel.Location = new System.Drawing.Point(950, 46);
            this.buttonHeadlessSelectModel.Name = "buttonHeadlessSelectModel";
            this.buttonHeadlessSelectModel.Size = new System.Drawing.Size(190, 40);
            this.buttonHeadlessSelectModel.TabIndex = 27;
            this.buttonHeadlessSelectModel.Text = "Select Simio Project...";
            this.buttonHeadlessSelectModel.UseVisualStyleBackColor = true;
            this.buttonHeadlessSelectModel.Click += new System.EventHandler(this.buttonHeadlessSelectModel_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1152, 28);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(45, 24);
            this.fileToolStripMenuItem.Text = "E&xit";
            this.fileToolStripMenuItem.Click += new System.EventHandler(this.fileToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Location = new System.Drawing.Point(0, 650);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1152, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // textLogs
            // 
            this.textLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textLogs.Location = new System.Drawing.Point(0, 327);
            this.textLogs.Multiline = true;
            this.textLogs.Name = "textLogs";
            this.textLogs.ReadOnly = true;
            this.textLogs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textLogs.Size = new System.Drawing.Size(1152, 323);
            this.textLogs.TabIndex = 7;
            this.textLogs.Text = "(No logs yet...)";
            // 
            // timerLogs
            // 
            this.timerLogs.Tick += new System.EventHandler(this.timerLogs_Tick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(8, 158);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(323, 20);
            this.label3.TabIndex = 44;
            this.label3.Text = "After loading, select Actions and Run Plan";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1152, 672);
            this.Controls.Add(this.textLogs);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "Form1";
            this.Text = "Run Model Plans using WinForms";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox comboHeadlessRunModels;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cbHeadlessSaveModelAfterRun;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button buttonRunPlan;
        private System.Windows.Forms.TextBox textHeadlessProjectFile;
        private System.Windows.Forms.Button buttonHeadlessSelectModel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TextBox textLogs;
        private System.Windows.Forms.Timer timerLogs;
        private System.Windows.Forms.CheckBox cbHeadlessRunPublishPlanAfterRun;
        private System.Windows.Forms.CheckBox cbHeadlessRunRiskAnalysis;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonSelectExtensionsPath;
        private System.Windows.Forms.TextBox textExtensionsPath;
        private System.Windows.Forms.Button buttonLoadSimioProject;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label3;
    }
}

