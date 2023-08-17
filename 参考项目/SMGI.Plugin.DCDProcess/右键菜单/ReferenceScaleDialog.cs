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
    public partial class ReferenceScaleDialog : Form
    {
        private double _referScale;
        public double ReferScale
        {
            get
            {
                return _referScale;
            }
        }

        public ReferenceScaleDialog(double scale)
        {
            InitializeComponent();

            _referScale = scale;

            cmbReferScale.Text = _referScale.ToString();
        }

        

        private void ReferenceScaleDialog_Load(object sender, EventArgs e)
        {

        }

        private void btOK_Click(object sender, EventArgs e)
        {
            bool res = double.TryParse(cmbReferScale.Text, out _referScale);
            if (!res || ReferScale < 0)
            {
                MessageBox.Show("参考比例尺输入不正确，请重新输入！");
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
