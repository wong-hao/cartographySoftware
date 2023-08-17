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
    public partial class CheckGraphicConflictFormJS : Form
    {
        /// <summary>
        /// 要素符号化后，符号之间的最小宽度（mm）
        /// </summary>
        public double GraphicMinDistance
        {
            get
            {
                return double.Parse(tbMinDistance.Text);
            }
        }

        public CheckGraphicConflictFormJS()
        {
            InitializeComponent();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            double val = 0;
            double.TryParse(tbMinDistance.Text, out val);
            if (val <= 0)
            {
                MessageBox.Show("请输入一个大于0的合法间距！");
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
