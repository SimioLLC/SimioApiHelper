using HeadlessLibrary;
using LoggertonHelpers;
using SimioAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeadlessFormsPlan
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            timerLogs.Enabled = true;
        }

        private bool LoadProject(string projectPath, out string explanation)
        {
            explanation = "";

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                string extensionsPath = System.AppDomain.CurrentDomain.BaseDirectory;
                labelExtensionsPath.Text = extensionsPath;

                ISimioProject project = HeadlessHelpers.LoadProject(extensionsPath, projectPath, out explanation);
                if (project == null)
                {
                    explanation = $"Cannot load project={projectPath}. Reason={explanation}";
                    return false;
                }

                comboHeadlessRunModels.Items.Clear();
                comboHeadlessRunModels.DisplayMember = "Name";
                foreach (var model in project.Models)
                {
                    comboHeadlessRunModels.Items.Add(model);
                }

                IModel defaultModel = comboHeadlessRunModels.Items[1] as IModel;

                return true;
            }
            catch (Exception ex)
            {
                explanation = $"Project={projectPath} Err={ex.Message}";
                return false;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }

        private void Alert(string msg)
        {
            Loggerton.Instance.LogIt(EnumLogFlags.Error, msg);
            MessageBox.Show(msg);
        }

        private void Alert(EnumLogFlags flags, string msg)
        {
            Loggerton.Instance.LogIt(flags, msg);
            MessageBox.Show(msg);
        }

        private void Logit(string msg)
        {
            if (msg.ToLower().StartsWith("info:"))
                Loggerton.Instance.LogIt(EnumLogFlags.Information, msg);
            else
                Loggerton.Instance.LogIt(EnumLogFlags.Error, msg);
        }

        private void buttonHeadlessSelectModel_Click(object sender, EventArgs e)
        {
            textHeadlessProjectFile.Text = HeadlessHelpers.GetProjectFile();
            string projectPath = textHeadlessProjectFile.Text;

            if (!LoadProject(projectPath, out string explanation))
                Alert($"{explanation}");

        }

        private void timerLogs_Tick(object sender, EventArgs e)
        {
            textLogs.Text = Loggerton.Instance.ShowLogs();
        }

        private void buttonRunPlan_Click_1(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                string modelName = comboHeadlessRunModels.Text;

                string projectPath = textHeadlessProjectFile.Text;
                string extensionsPath = labelExtensionsPath.Text;
                Logit($"Info: Running Model={modelName} Plan. ExtensionsPath={extensionsPath}");

                if (!HeadlessHelpers.RunModelPlan(extensionsPath, projectPath, modelName, 
                    cbHeadlessRunRiskAnalysis.Checked, 
                    cbHeadlessSaveModelAfterRun.Checked,
                    cbHeadlessRunPublishPlanAfterRun.Checked, out string explanation))
                {
                    Alert(explanation);
                }
                else
                {
                    Alert(EnumLogFlags.Information, $"Success! \nProject={projectPath} Model={modelName} performed the actions successfully. Check the logs for more information.");
                }
            }
            catch (Exception ex)
            {
                Alert($"RunPlan: Err={ex.Message}");
                throw;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }

    }
}
