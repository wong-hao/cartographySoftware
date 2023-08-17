using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using System.IO;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.DCDProcess
{
    public partial class CheckDataBaseStructForm : Form
    {
        /// <summary>
        /// 待检查的目标数据库路径
        /// </summary>
        public string ObjDataBase
        {
            get
            {
                return tbObjDB.Text;
            }
        }

        /// <summary>
        /// 参考的模板数据库路径
        /// </summary>
        public string TemplateDataBase
        {
            get
            {
                return tbTemplateDB.Text;
            }
        }

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


        public CheckDataBaseStructForm()
        {
            InitializeComponent();
        }

        private void CheckDataBaseStructForm_Load(object sender, EventArgs e)
        {
            if (GApplication.Application.Workspace != null)
            {
                tbObjDB.Text = GApplication.Application.Workspace.EsriWorkspace.PathName;
            }

            string templatePath = GApplication.Application.Template.Root + @"\多尺度数据建库模板.gdb";
            if (Directory.Exists(templatePath))
            {
                if (GApplication.GDBFactory.IsWorkspace(templatePath))
                {
                    tbTemplateDB.Text = templatePath;
                }
            }

            tbOutFilePath.Text = OutputSetup.GetDir();
        }

        private void btObj_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择GDB工程文件夹";
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!GApplication.GDBFactory.IsWorkspace(fbd.SelectedPath))
                {
                    MessageBox.Show("不是有效地GDB文件");
                    return;
                }

                tbObjDB.Text = fbd.SelectedPath;
            }
        }

        private void btTemplate_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择GDB工程文件夹";
            fbd.ShowNewFolderButton = false;
            fbd.SelectedPath = GApplication.Application.Template.Root;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!GApplication.GDBFactory.IsWorkspace(fbd.SelectedPath))
                {
                    MessageBox.Show("不是有效地GDB文件");
                    return;
                }

                tbTemplateDB.Text = fbd.SelectedPath;
            }
        }

        private void btOutputPath_Click(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK && fd.SelectedPath.Length > 0)
            {
                tbOutFilePath.Text = fd.SelectedPath;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (tbObjDB.Text == "")
            {
                MessageBox.Show("请指定需检查的目标数据库!");
                return;
            }

            if (tbTemplateDB.Text == "")
            {
                MessageBox.Show("请指定参考的模板数据库!");
                return;
            }

            if (tbOutFilePath.Text == "")
            {
                MessageBox.Show("请指定检查结果输出路径!");
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        
    }
}
