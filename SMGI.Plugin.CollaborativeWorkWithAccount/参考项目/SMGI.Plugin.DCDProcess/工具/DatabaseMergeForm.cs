using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SMGI.Common;

namespace SMGI.Plugin.DCDProcess
{
    public partial class DatabaseMergeForm : Form
    {
        private List<string> _sourceDBFileNameList;
        public List<string> SourceDBFileNameList
        {
            get
            {
                return _sourceDBFileNameList;
            }
        }

        public string DataBaseTemplate
        {
            get
            {
                return tbReferGDBTemplate.Text;
            }
        }

        public string OutputPath
        {
            get
            {
                return tbOutputPath.Text;
            }
        }

        public string OutputGDBName
        {
            get
            {
                string dbName = tbDBName.Text;
                if (!dbName.ToLower().EndsWith(".gdb"))
                    dbName += ".gdb";

                return dbName;
            }
        }

        private GApplication _app;
        public DatabaseMergeForm(GApplication app)
        {
            InitializeComponent();
            _app = app;

            _sourceDBFileNameList = new List<string>();
        }

        private void DatabaseMergeForm_Load(object sender, EventArgs e)
        {
            lvDataBase.Columns.Add("");
            lvDataBase.Columns[0].Width = 1024;
            lvDataBase.Scrollable = true;

            lvDataBase.Items.Clear();
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

                int count = lvDataBase.Items.Count;

                //MDB数据库
                findMDBfiles(path);

                //GDB数据库
                findGDBfiles(path);

                if (lvDataBase.Items.Count == count)
                {
                    MessageBox.Show("指定的文件夹内没有检索到有效的地理数据库！");
                }
            }
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lvDataBase.SelectedItems)  //选中项遍历  
            {
                lvDataBase.Items.RemoveAt(lvi.Index); // 按索引移除  
            }
        }

        private void btnReferGDBTemplate_Click(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            fd.Description = "选择参考文件地理数据库模板（GDB）";
            fd.ShowNewFolderButton = false;
            if (fd.ShowDialog() != DialogResult.OK || !fd.SelectedPath.ToLower().Trim().EndsWith(".gdb"))
            {
                MessageBox.Show("请指定一个有效的文件地理数据库文件！");
                return;
            }

            tbReferGDBTemplate.Text = fd.SelectedPath;
        }

        private void btnOutPutPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog pDialog = new FolderBrowserDialog();
            pDialog.Description = "选择输出路径";
            pDialog.ShowNewFolderButton = true;

            if (DialogResult.OK == pDialog.ShowDialog())
            {
                //更新
                tbOutputPath.Text = pDialog.SelectedPath;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            #region 验证是否已经输入参数
            //待合并的数据库
            _sourceDBFileNameList.Clear();
            foreach (ListViewItem lvi in lvDataBase.Items)
            {
                if (!_sourceDBFileNameList.Contains(lvi.SubItems[0].Text))
                {
                    _sourceDBFileNameList.Add(lvi.SubItems[0].Text);
                }
            }

            if (_sourceDBFileNameList.Count == 0)
            {
                MessageBox.Show("请指定至少一个待合并的地理库数据！");
                return;
            }

            if (string.IsNullOrEmpty(tbReferGDBTemplate.Text))
            {
                MessageBox.Show("请指定输出文件地理数据库参考模板！");
                return;
            }
            if (string.IsNullOrEmpty(tbOutputPath.Text))
            {
                MessageBox.Show("请指定输出文件地理数据库的位置！");
                return;
            }
            if (string.IsNullOrEmpty(tbDBName.Text))
            {
                MessageBox.Show("请指定输出文件地理数据库的名称！");
                return;
            }
            #endregion

            #region 判断待合并的原始库数据是否为有效的地理数据库
            foreach (var sourceFileName in _sourceDBFileNameList)
            {
                //数据库有效性
                if (sourceFileName.ToLower().EndsWith(".gdb"))
                {
                    if (!Directory.Exists(sourceFileName))
                    {
                        MessageBox.Show(string.Format("原始数据文件【{0}】不存在！", sourceFileName));
                        return;
                    }

                    if (!GApplication.GDBFactory.IsWorkspace(sourceFileName))
                    {
                        MessageBox.Show(string.Format("原始数据文件【{0}】不是有效地GDB文件！", sourceFileName));
                        return;
                    }
                }
                else if (sourceFileName.ToLower().EndsWith(".mdb"))
                {
                    if (!File.Exists(sourceFileName))
                    {
                        MessageBox.Show(string.Format("原始数据文件【{0}】不存在！", sourceFileName));
                        return;
                    }

                    if (!GApplication.MDBFactory.IsWorkspace(sourceFileName))
                    {
                        MessageBox.Show(string.Format("原始数据文件【{0}】不是有效地MDB文件！", sourceFileName));
                        return;
                    }
                }
                else
                {
                    MessageBox.Show(string.Format("原始数据文件【{0}】为无效的地理数据库文件！", sourceFileName));
                    return;
                }
            }
            #endregion
            
            #region 判断输出数据库模板是否有效
            if (!tbReferGDBTemplate.Text.ToLower().EndsWith(".gdb"))
            {
                MessageBox.Show(string.Format("输出文件地理数据库参考模板为无效的地理数据库文件！"));
                return;
            }

            if (!GApplication.GDBFactory.IsWorkspace(tbReferGDBTemplate.Text))
            {
                MessageBox.Show(string.Format("输出文件地理数据库参考模板不是有效地GDB文件！"));
                return;
            }
            #endregion

            #region 判断待合并的数据库是否可以参考输出数据库模板进行合并
            //空间参考、要素类类型等
            #endregion

            #region 判断输出后的数据库文件是否已经存在
            string fileName = System.IO.Path.Combine(OutputPath, OutputGDBName);
            if (Directory.Exists(fileName))
            {
                MessageBox.Show("输出数据库文件({0})已存在！", "提示", MessageBoxButtons.OK);

                return;
            }
            #endregion
            

            DialogResult = DialogResult.OK;

            
        }

        public void findMDBfiles(string path)
        {
            string[] dataFiles = Directory.GetFiles(path);
            for (int i = 0; i < dataFiles.Length; i++)
            {
                if (dataFiles[i].ToLower().EndsWith(".mdb"))
                {
                    lvDataBase.BeginUpdate();

                    ListViewItem item = new ListViewItem(dataFiles[i], 0);
                    lvDataBase.Items.Add(item);


                    lvDataBase.EndUpdate();
                }


            }
            string[] dataDirectories = Directory.GetDirectories(path);
            for (int j = 0; j < dataDirectories.Length; j++)
            {
                findMDBfiles(dataDirectories[j]);
            }
        }

        public void findGDBfiles(string path)
        {
            string[] dataDirectories = Directory.GetDirectories(path);
            for (int i = 0; i < dataDirectories.Length; i++)
            {
                if (dataDirectories[i].ToLower().EndsWith(".gdb"))
                {
                    lvDataBase.BeginUpdate();

                    ListViewItem item = new ListViewItem(dataDirectories[i], 0);
                    lvDataBase.Items.Add(item);

                    lvDataBase.EndUpdate();
                }
                else
                {
                    findGDBfiles(dataDirectories[i]);
                }
            }
        }

        
    }
}
