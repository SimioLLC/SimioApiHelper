using HeadlessLibrary;
using LoggertonHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeadlessFormsExperiment
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonRunExperiment_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string experimentName = textExperimentName.Text;
            string modelName = textModelName.Text;
            try
            {
                if (!HeadlessHelpers.RunExperiment(textHeadlessModelFile.Text, modelName, experimentName, 
                    cbHeadlessSaveModelAfterRun.Checked,
                    out string explanation))
                {
                    Alert(explanation);
                }
                else
                {
                    Alert(EnumLogFlags.Information, $"Model={textModelName.Text} Experiment={textExperimentName} performed the actions successfully. Check the logs for more information.");
                }
            }
            catch (Exception ex)
            {
                Alert($"Err={ex.Message}");
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
            Loggerton.Instance.LogIt(EnumLogFlags.Error, msg);
        }

    }
}
