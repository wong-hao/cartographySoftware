using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public partial class CheckReservoirNameCheckForm : Form
    {
        #region 属性

        public String checkField
        {
            get
            {
                return checkFieldComboBox.SelectedItem.ToString();
            }
        }

        public string ResultOutputFilePath
        {
            get
            {
                return tbOutFilePath.Text;
            }
        }

        #endregion

        public CheckReservoirNameCheckForm()
        {
            InitializeComponent();
        }

        private void CheckReservoirNameCheckForm_Load(object sender, EventArgs e)
        {
            // 在构造函数或 Load 事件中添加选项
            checkFieldComboBox.Items.Add("NAME");
            checkFieldComboBox.Items.Add("LABELNAME");
            checkFieldComboBox.Items.Add("TYPE");
            checkFieldComboBox.Items.Add("DJ");

            // 选择默认项（可选）
            checkFieldComboBox.SelectedIndex = 0; // 默认选中第一个选项，如果需要选择第二个选项，将索引设置为 1

            tbOutFilePath.Text = OutputSetup.GetDir();
        }

        private void btnOutputPath_Click(object sender, EventArgs e)
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
                MessageBox.Show("请指定检查结果输出路径！");
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
