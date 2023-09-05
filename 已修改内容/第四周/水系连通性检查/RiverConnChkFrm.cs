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

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public partial class RiverConnChkFrm : Form
    {
        public List<string> RiverLayerNames
        {
            get;
            internal set;
        }

        public String RiverLayerName
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

        public RiverConnChkFrm()
        {
            InitializeComponent();
        }

        private void RiverConnChkFrm_Load(object sender, EventArgs e)
        {
            //将水系图层列表清空
            cbLayerNames.Items.Clear();

            foreach (string RiverLayer in RiverLayerNames)
            {
                cbLayerNames.Items.Add(RiverLayer);
            }

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
