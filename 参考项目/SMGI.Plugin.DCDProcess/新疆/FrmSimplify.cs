using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace SMGI.Plugin.DCDProcess
{
    public partial class FrmSimplify : DevExpress.XtraEditors.XtraForm
    {
        public double width=100;
        public double height=100;

        public FrmSimplify()
        {
            InitializeComponent();
            txtBendWidth.Text = width.ToString();
            txtBendDeepth.Text = height.ToString();  
        }        

        public FrmSimplify(double _width,double _height)
        {
            InitializeComponent();

            width = _width;
            height = _height;

            txtBendWidth.Text = width.ToString();
            txtBendDeepth.Text = height.ToString();
        }


        private void FrmSimplify_Load(object sender, EventArgs e)
        {
            this.AcceptButton = btOK;
            this.CancelButton = btCancel;
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            double.TryParse(txtBendDeepth.Text, out height);
            double.TryParse(txtBendWidth.Text, out width);
            if (width == 0)
            {
                MessageBox.Show("最小开口宽度设置错误！");
                return;
            }
            if (height == 0)
            {
                MessageBox.Show("最小弯曲深度设置错误！");
                return;
            }            
            this.DialogResult = DialogResult.OK;
            return;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {   
            this.Close();
        }

        private void txtBendDeepth_TextChanged(object sender, EventArgs e)
        {
            double.TryParse(txtBendDeepth.Text, out height); 
            if (height == 0)
            {
                MessageBox.Show("最小弯曲深度设置错误！");
                return;
            } 
        }

        private void txtBendWidth_TextChanged(object sender, EventArgs e)
        {           
            double.TryParse(txtBendWidth.Text, out width);
            if (width == 0)
            {
                MessageBox.Show("最小开口宽度设置错误！");
                return;
            }            
        }
    }
}