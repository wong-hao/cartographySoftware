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
    public partial class CheckRiverStructFrm : Form
    {

        public bool BIgnoreSmall
        {
            get
            {
                return cbCheckIgnoreSmall.Checked;
            }
        }

        public String roadLyrName;

        public CheckRiverStructFrm()
        {
            InitializeComponent();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void roadLyrNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            roadLyrName = roadLyrNameComboBox.SelectedItem.ToString();
        }

        private void CheckRiverStructFrm_Load(object sender, EventArgs e)
        {
            // 在构造函数或 Load 事件中添加选项
            roadLyrNameComboBox.Items.Add("水渠");
            roadLyrNameComboBox.Items.Add("河流");

            // 选择默认项（可选）
            roadLyrNameComboBox.SelectedIndex = 0; // 默认选中第一个选项，如果需要选择第二个选项，将索引设置为 1
        }
    }
}
