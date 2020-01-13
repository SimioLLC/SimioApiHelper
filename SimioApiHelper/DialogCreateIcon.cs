using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.IconLib;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimioApiHelper
{
    public partial class DialogCreateIcon : Form
    {
        public DialogCreateIcon()
        {
            InitializeComponent();
        }

        private void buttonGetPng_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = false;

                DialogResult result = dialog.ShowDialog();

                if (result != DialogResult.OK)
                    return;

                string pngPath = dialog.FileName;

                string filename = Path.GetFileNameWithoutExtension(pngPath);
                string folder = Path.GetDirectoryName(pngPath);
                textPngPath.Text = pngPath;

                string icoPath = Path.Combine(folder, $"{filename}.ico");

                textIcoPath.Text = icoPath;



            }
            catch (Exception ex)
            {
                alert($"Err={ex}");
            }
        }

        private void alert(string msg)
        {
            MessageBox.Show(msg);
        }

        private void buttonConvert_Click(object sender, EventArgs e)
        {
            try
            {
                MultiIcon mIcon = new MultiIcon();
                SingleIcon sIcon = mIcon.Add("Icon1");

                string pngPath = textPngPath.Text;
                string icoPath = textIcoPath.Text;

                sIcon.CreateFrom(pngPath, IconOutputFormat.FromWinXP);
                mIcon.SelectedIndex = 0;
                mIcon.Save(icoPath, MultiIconFormat.ICO);

            }
            catch (Exception ex)
            {
                alert($"Err={ex}");
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DialogCreateIcon_Load(object sender, EventArgs e)
        {

        }
    }

}
