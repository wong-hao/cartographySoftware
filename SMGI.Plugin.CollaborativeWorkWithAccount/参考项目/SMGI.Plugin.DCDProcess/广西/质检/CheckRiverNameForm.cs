using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Plugin.DCDProcess.GX
{
    public partial class CheckRiverNameForm : Form
    {
        /// <summary>
        /// 检查结果输出路径
        /// </summary>
        public string OutputPath
        {
            get
            {
                return tbOutFilePath.Text;
            }
        }
        public double len
        {
            get
            {
                return (double)tblen.Value;
            }
        }
        public CheckRiverNameForm()
        {
            InitializeComponent();
        }

        private void btnFilePath_Click(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK && fd.SelectedPath.Length > 0)
            {
                tbOutFilePath.Text = fd.SelectedPath;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
