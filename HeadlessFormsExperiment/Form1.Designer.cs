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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectSimioModelFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textOutput = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.buttonRunExperiment = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.textExperimentName = new System.Windows.Forms.TextBox();
            this.textHeadlessModelFile = new System.Windows.Forms.TextBox();
            this.buttonHeadlessSelectModel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbHeadlessSaveModelAfterRun = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textModelName = new System.Windows.Forms.TextBox();
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
            // textOutput
            // 
            this.textOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textOutput.Location = new System.Drawing.Point(0, 220);
            this.textOutput.Multiline = true;
            this.textOutput.Name = "textOutput";
            this.textOutput.ReadOnly = true;
            this.textOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textOutput.Size = new System.Drawing.Size(1149, 382);
            this.textOutput.TabIndex = 1;
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
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.textModelName);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.buttonRunExperiment);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.textExperimentName);
            this.panel1.Controls.Add(this.textHeadlessModelFile);
            this.panel1.Controls.Add(this.buttonHeadlessSelectModel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 28);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1149, 192);
            this.panel1.TabIndex = 3;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 10);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(146, 17);
            this.label9.TabIndex = 32;
            this.label9.Text = "Project Name (.SPFX)";
            // 
            // buttonRunExperiment
            // 
            this.buttonRunExperiment.Location = new System.Drawing.Point(972, 90);
            this.buttonRunExperiment.Name = "buttonRunExperiment";
            this.buttonRunExperiment.Size = new System.Drawing.Size(165, 46);
            this.buttonRunExperiment.TabIndex = 31;
            this.buttonRunExperiment.Text = "Run Experiment";
            this.buttonRunExperiment.UseVisualStyleBackColor = true;
            this.buttonRunExperiment.Click += new System.EventHandler(this.buttonRunExperiment_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 98);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(123, 17);
            this.label6.TabIndex = 30;
            this.label6.Text = "Experiment Name:";
            // 
            // textExperimentName
            // 
            this.textExperimentName.Location = new System.Drawing.Point(195, 98);
            this.textExperimentName.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textExperimentName.Name = "textExperimentName";
            this.textExperimentName.Size = new System.Drawing.Size(158, 22);
            this.textExperimentName.TabIndex = 29;
            this.textExperimentName.Text = "Experiment1";
            // 
            // textHeadlessModelFile
            // 
            this.textHeadlessModelFile.Location = new System.Drawing.Point(11, 33);
            this.textHeadlessModelFile.Name = "textHeadlessModelFile";
            this.textHeadlessModelFile.Size = new System.Drawing.Size(460, 22);
            this.textHeadlessModelFile.TabIndex = 28;
            // 
            // buttonHeadlessSelectModel
            // 
            this.buttonHeadlessSelectModel.Location = new System.Drawing.Point(972, 33);
            this.buttonHeadlessSelectModel.Name = "buttonHeadlessSelectModel";
            this.buttonHeadlessSelectModel.Size = new System.Drawing.Size(165, 40);
            this.buttonHeadlessSelectModel.TabIndex = 27;
            this.buttonHeadlessSelectModel.Text = "Select Project";
            this.buttonHeadlessSelectModel.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbHeadlessSaveModelAfterRun);
            this.groupBox1.Location = new System.Drawing.Point(503, 90);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(369, 75);
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
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 73);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(95, 17);
            this.label7.TabIndex = 35;
            this.label7.Text = "Model Name: ";
            // 
            // textModelName
            // 
            this.textModelName.Location = new System.Drawing.Point(195, 70);
            this.textModelName.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textModelName.Name = "textModelName";
            this.textModelName.Size = new System.Drawing.Size(210, 22);
            this.textModelName.TabIndex = 34;
            this.textModelName.Text = "Model";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1149, 624);
            this.Controls.Add(this.textOutput);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Run Experiments using WinForm";
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
        private System.Windows.Forms.TextBox textOutput;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button buttonRunExperiment;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textExperimentName;
        private System.Windows.Forms.TextBox textHeadlessModelFile;
        private System.Windows.Forms.Button buttonHeadlessSelectModel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cbHeadlessSaveModelAfterRun;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textModelName;
    }
}

