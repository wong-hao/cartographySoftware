using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using SMGI.Common;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public partial class RoadConnChkFrm : Form
    {
        public String RoadLayerName
        {
            get
            {
                return cbLayerNames.SelectedItem.ToString();
            }
        }

        /// <summary>
        /// 输出路径
        /// </summary>
        public string OutFilePath
        {
            get
            {
                return tbOutFilePath.Text;
            }
        }

        public RoadConnChkFrm()
        {
            InitializeComponent();
        }

        private void RoadConnChkFrm_Load(object sender, EventArgs e)
        {
            cbLayerNames.Items.Add("其他道路");
            cbLayerNames.Items.Add("乡道");
            cbLayerNames.Items.Add("国省县道");

            cbLayerNames.SelectedIndex = 0;

            tbOutFilePath.Text = OutputSetup.GetDir();
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

            if (tbOutFilePath.Text == "")
            {
                MessageBox.Show("请指定输出路径！");
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
