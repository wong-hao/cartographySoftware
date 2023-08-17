using System;
using System.Windows.Forms;

namespace SMGI.Plugin.DCDProcess.DataProcess
{
    public partial class FrmOutputDir : Form
    {

        private string _dir="";
        public FrmOutputDir()
        {
            InitializeComponent();
        }

        public void SetDir(string dir)
        {
            lbl_dir.Text = dir;
            _dir = dir;
        }

        public string GetDir()
        {
            return _dir;
        }

   
        private void btn_change_Click(object sender, EventArgs e)
        {
           var fd=new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK && fd.SelectedPath.Length > 0)
            {
                lbl_dir.Text = fd.SelectedPath;
                _dir = fd.SelectedPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (GetDir() == "")
            {
                MessageBox.Show("请设置当前目录！");
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
      
    }
}
