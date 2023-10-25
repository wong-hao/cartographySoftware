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
    public partial class FrmMeasureResult : Form
    {
        public enum MeasureType
        {
            LINE,//测量距离
            AREA//测量面积
        }

        public MeasureType CurMeasureType
        {
            get
            {
                if (rbLen.Checked)
                    return MeasureType.LINE;
                else
                    return MeasureType.AREA;
            }
        }

        public string ResultText
        {
            set
            {
                this.rtbResult.Text = value;
            }
        }

        public delegate void FrmClosedEventHandler();
        public event FrmClosedEventHandler frmClosed = null;


        public FrmMeasureResult()
        {
            InitializeComponent();
        }

        //窗口关闭后引发委托事件
        private void FrmMeasureResultcs_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (frmClosed != null)
            {
                frmClosed();
            }
        }
    }
}
