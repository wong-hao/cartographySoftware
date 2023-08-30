using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
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

        public String roadLyrName;
        public String checkField;

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

        private void CheckRiverNameForm_Load(object sender, EventArgs e)
        {
            // 在构造函数或 Load 事件中添加选项
            roadLyrNameComboBox.Items.Add("水渠");
            roadLyrNameComboBox.Items.Add("河流");
            roadLyrName = String.Empty;

            checkFieldComboBox.Items.Add("NAME");
            checkFieldComboBox.Items.Add("LABELNAME");
            checkField = String.Empty;

            // 选择默认项（可选）
            roadLyrNameComboBox.SelectedIndex = 0; // 默认选中第一个选项，如果需要选择第二个选项，将索引设置为 1
            checkFieldComboBox.SelectedIndex = 0; // 默认选中第一个选项，如果需要选择第二个选项，将索引设置为 1

            tbOutFilePath.Text = OutputSetup.GetDir();
        }

        private void roadLyrNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            roadLyrName = roadLyrNameComboBox.SelectedItem.ToString();
        }

        private void checkFieldComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkField = checkFieldComboBox.SelectedItem.ToString();
        }
    }
}
