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
    public partial class AddFNForm : Form
    {
        public string FN
        {
            get
            {
                return tbFN.Text.Trim();
            }
        }

        public AddFNForm()
        {
            InitializeComponent();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (tbFN.Text.Trim() == "")
            {
                MessageBox.Show("请输入字段名！");
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
