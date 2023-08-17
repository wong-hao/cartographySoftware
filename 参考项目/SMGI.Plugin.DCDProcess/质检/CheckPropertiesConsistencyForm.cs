using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Plugin.DCDProcess
{
    public partial class CheckPropertiesConsistencyForm : Form
    {
        #region 属性
        public string ReferGDB
        {
            get
            {
                return tbReferGDB.Text;
            }
        }

        public string OutputPath
        {
            get
            {
                return tbOutFilePath.Text;
            }
        }
        #endregion

        public CheckPropertiesConsistencyForm()
        {
            InitializeComponent();

            tbOutFilePath.Text = OutputSetup.GetDir();
        }

        private void btReferGDB_Click(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            fd.Description = "选择GDB数据库";
            fd.ShowNewFolderButton = false;
            if (fd.ShowDialog() != DialogResult.OK || !fd.SelectedPath.ToLower().Trim().EndsWith(".gdb"))
                return;

            tbReferGDB.Text = fd.SelectedPath;
        }

        private void btnOutputPath_Click(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK && fd.SelectedPath.Length > 0)
            {
                btnOutputPath.Text = fd.SelectedPath;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (tbReferGDB.Text == "")
            {
                MessageBox.Show("请指定参考数据库！");
                return;
            }

            if (tbOutFilePath.Text == "")
            {
                MessageBox.Show("请指定检查结果输出路径！");
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
