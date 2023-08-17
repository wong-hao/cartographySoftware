using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;

using SMGI.Common;
namespace SMGI.Plugin.DCDProcess
{
    public partial class AddCalcFieldForm : Form
    {
        private List<string> _sourceDBFileNameList;
        public List<string> SourceDBFileNameList
        {
            get
            {
                return _sourceDBFileNameList;
            }
        }

        public string ruleTableName
        {
            get
            {
                return comboBox1.SelectedItem.ToString();               
            }
        }

        private GApplication _app;
        public AddCalcFieldForm(GApplication app)
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

            string mdbPath = GApplication.Application.Template.Root + "\\规则配置.mdb";
            if (!System.IO.File.Exists(mdbPath))
            {
                MessageBox.Show(string.Format("未找到配置文件:{0}!", mdbPath));
                return;
            }
            List<string> ruleTableNameList = new List<string>();

            string ruleTableNamePrefix = "分类代码";
            IWorkspaceFactory wsFactory = new AccessWorkspaceFactoryClass();
            IWorkspace ws = wsFactory.OpenFromFile(mdbPath, 0);
            IEnumDataset enumDataset = ws.get_Datasets(esriDatasetType.esriDTAny);
            enumDataset.Reset();
            IDataset dataset = null;
            while ((dataset = enumDataset.Next()) != null)
            {
                if (dataset.Name.StartsWith(ruleTableNamePrefix))
                {
                    ruleTableNameList.Add(dataset.Name);
                }
            }
            Marshal.ReleaseComObject(enumDataset);
            Marshal.ReleaseComObject(ws);
            Marshal.ReleaseComObject(wsFactory);
            if (ruleTableNameList.Count == 0)
            {
                MessageBox.Show(string.Format("配置文件【{0}】中未发现任何数据映射规则表!", mdbPath));
                return;
            }

            foreach (var item in ruleTableNameList)
            {
                comboBox1.Items.Add(item);
                comboBox1.SelectedIndex = 0;
            }
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

            if (string.IsNullOrEmpty(comboBox1.SelectedItem.ToString()))
            {
                MessageBox.Show("请指定分类代码赋值表！");
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

            #region 判断待合并的数据库是否可以参考输出数据库模板进行合并
            //空间参考、要素类类型等
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
