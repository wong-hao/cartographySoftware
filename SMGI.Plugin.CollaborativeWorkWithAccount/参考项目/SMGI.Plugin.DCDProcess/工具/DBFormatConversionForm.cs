using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geoprocessor;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    public partial class DBFormatConversionForm : Form
    {
        private string _sourceDBFormat = "";
        public string SourceDBFormat
        {
            get
            {
                return _sourceDBFormat;
            }
        }

        private string _outDBFormat = "";
        public string OutDBFormat
        {
            get
            {
                return _outDBFormat;
            }
        }

        private List<string> _sourceDBList = null;
        public List<string> SourceDBList
        {
            get
            {
                return _sourceDBList;
            }
        }

        public string OutputPath
        {
            get
            {
                return tbOutputPath.Text;
            }
        }

        public DBFormatConversionForm()
        {
            InitializeComponent();
        }

        private void DBFormatConversionForm_Load(object sender, EventArgs e)
        {
            if (CBDataBaseConvType.Items.Count > 0)
            {
                CBDataBaseConvType.SelectedIndex = 0;
            }
            _sourceDBList = new List<string>();

            lvDataBase.Columns.Add("");
            lvDataBase.Columns[0].Width = 1024;
            lvDataBase.Scrollable = true;

            lvDataBase.Items.Clear();
        }

        private void CBDataBaseConvType_SelectedIndexChanged(object sender, EventArgs e)
        {
            lvDataBase.Items.Clear();

            _sourceDBList = new List<string>();
        }

        private void lvDataBase_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            btnDel.Enabled = lvDataBase.SelectedItems.Count > 0;
        }

        private void btnSourceDB_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog mFolderBrowserDialog = new FolderBrowserDialog();
            // mFolderBrowserDialog.ShowNewFolderButton = true;
            if (mFolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string path = mFolderBrowserDialog.SelectedPath;

                _sourceDBFormat = "";
                _outDBFormat = "";
                if (CBDataBaseConvType.Text.ToLower().StartsWith("gdb"))
                {
                    _sourceDBFormat = "gdb";
                    _outDBFormat = "mdb";

                    //GDB数据库
                    findGDBfiles(path);
                }
                else
                {
                    _sourceDBFormat = "mdb";
                    _outDBFormat = "gdb";

                    //MDB数据库
                    findMDBfiles(path);
                }

                
            }
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lvDataBase.SelectedItems)  //选中项遍历  
            {
                lvDataBase.Items.RemoveAt(lvi.Index); // 按索引移除  
                _sourceDBList.Remove(lvi.Text);
            }
        }

        private void btnOutPutPath_Click(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK && fd.SelectedPath.Length > 0)
            {
                tbOutputPath.Text = fd.SelectedPath;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            #region 判断必填参数是否已经填写
            if (_sourceDBFormat == "")
            {
                MessageBox.Show("请指定目标地理数据库！");
                return;
            }
            if (_sourceDBList.Count == 0)
            {
                MessageBox.Show("请指定需转换的地理数据库！");
                return;
            }
            if (tbOutputPath.Text == "")
            {
                MessageBox.Show("请指定输出路径！");
                return;
            }
            #endregion

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void findMDBfiles(string path)
        {
            string[] dataFiles = Directory.GetFiles(path);
            for (int i = 0; i < dataFiles.Length; i++)
            {
                if (dataFiles[i].ToLower().EndsWith(".mdb"))
                {
                    if (_sourceDBList.Contains(dataFiles[i]))
                        continue;

                    lvDataBase.BeginUpdate();

                    ListViewItem item = new ListViewItem(dataFiles[i], 0);
                    lvDataBase.Items.Add(item);

                    lvDataBase.EndUpdate();

                    _sourceDBList.Add(dataFiles[i]);
                }


            }
            string[] dataDirectories = Directory.GetDirectories(path);
            for (int j = 0; j < dataDirectories.Length; j++)
            {
                findMDBfiles(dataDirectories[j]);
            }
        }

        private void findGDBfiles(string path)
        {
            string[] dataDirectories = Directory.GetDirectories(path);
            for (int i = 0; i < dataDirectories.Length; i++)
            {
                if (dataDirectories[i].ToLower().EndsWith(".gdb"))
                {
                    if (_sourceDBList.Contains(dataDirectories[i]))
                        continue;

                    lvDataBase.BeginUpdate();

                    ListViewItem item = new ListViewItem(dataDirectories[i], 0);
                    lvDataBase.Items.Add(item);

                    lvDataBase.EndUpdate();

                    _sourceDBList.Add(dataDirectories[i]);
                }
                else
                {
                    findGDBfiles(dataDirectories[i]);
                }
            }
        }
        
    }
}
