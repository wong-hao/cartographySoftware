using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.IO;
using SMGI.Common;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    public partial class DataBaseProjectForm : Form
    {
        
        public List<IWorkspace> SourceWSList
        {
            get;
            protected set;
        }

        public ISpatialReference OutputSpatialReference
        {
            get;
            protected set;
        }


        public string OutputPath
        {
            get
            {
                return tbOutputPath.Text;
            }
        }

        public string DBNameDuffix
        {
            get
            {
                if (cbAddDuffix.Checked)
                {
                    return tbDuffix.Text;
                }
                else
                {
                    return "";
                }
            }
        }

        public bool EnableTransParams
        {
            get
            {
                return cbGeoTrans.Checked;
            }
        }

        public double[] TransParams
        {
            get;
            protected set;
        }


        public DataBaseProjectForm()
        {
            InitializeComponent();
        }

        private void DataBaseProjectForm_Load(object sender, EventArgs e)
        {
            lvDataBase.Columns.Add("");
            lvDataBase.Columns[0].Width = 1024;
            lvDataBase.Scrollable = true;

            lvDataBase.Items.Clear();
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

        private void lvDataBase_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            btnDel.Enabled = lvDataBase.SelectedItems.Count > 0;
        }

        private void btnOutSR_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "坐标参考文件(*.prj)|*.prj";
            dlg.Title = "选择目标空间参考";
            dlg.Multiselect = false;
            dlg.InitialDirectory = GApplication.RootPath + @"\Projection";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tbOutputSpatialReference.Text = dlg.FileName;
            }
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

        private void cbAddDuffix_CheckedChanged(object sender, EventArgs e)
        {
            tbDuffix.Enabled = cbAddDuffix.Checked;
        }

        private void cbGeoTrans_CheckedChanged(object sender, EventArgs e)
        {
            gbGeoTrans.Enabled = cbGeoTrans.Checked;
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            var sourceDBFileNameList = new List<string>();
            foreach (ListViewItem lvi in lvDataBase.Items)
            {
                if (!sourceDBFileNameList.Contains(lvi.SubItems[0].Text))
                {
                    sourceDBFileNameList.Add(lvi.SubItems[0].Text);
                }
            }
            if (sourceDBFileNameList.Count == 0)
            {
                MessageBox.Show("请指定至少一个输入地理数据库！");
                return;
            }
            
            
            if (string.IsNullOrEmpty(tbOutputSpatialReference.Text))
            {
                MessageBox.Show("请指定输出坐标系！");
                return;
            }
            ISpatialReferenceFactory spatialRefFactory = new SpatialReferenceEnvironmentClass();
            OutputSpatialReference = spatialRefFactory.CreateESRISpatialReferenceFromPRJFile(tbOutputSpatialReference.Text);
            if (OutputSpatialReference == null)
            {
                MessageBox.Show("指定的输出坐标系无效！");
                return;
            }

            if (string.IsNullOrEmpty(tbOutputPath.Text))
            {
                MessageBox.Show("请指定输出地理数据库的存储位置！");
                return;
            }

            TransParams = null;
            if (cbGeoTrans.Checked)
            {
                double dx, dy, dz, rx, ry, rz, s;
                double.TryParse(tbXTrans.Text, out dx);
                double.TryParse(tbYTrans.Text, out dy);
                double.TryParse(tbZTrans.Text, out dz);
                double.TryParse(tbXRotate.Text, out rx);
                double.TryParse(tbYRotate.Text, out ry);
                double.TryParse(tbZRotate.Text, out rz);
                double.TryParse(tbScale.Text, out s);

                TransParams = new double[7] { dx, dy, dz, rx, ry, rz, s };
            }

            #region 判断输入的原始库数据是否为有效的地理数据库
            foreach (var sourceFileName in sourceDBFileNameList)
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
            
            SourceWSList = new List<IWorkspace>();
            List<string> noSRDBList = new List<string>();
            using (var wo = GApplication.Application.SetBusy())
            {
                foreach (var sourceFileName in sourceDBFileNameList)
                {
                    wo.SetText(string.Format("正在验证数据库【{0}】中的要素类是已设置坐标系......", sourceFileName));

                    IWorkspaceFactory srcWF = null;
                    if (sourceFileName.ToLower().EndsWith(".gdb"))
                    {
                        srcWF = new FileGDBWorkspaceFactoryClass();
                    }
                    else if (sourceFileName.ToLower().EndsWith(".mdb"))
                    {
                        srcWF = new AccessWorkspaceFactoryClass();
                    }
                    IWorkspace sourceWorkspace = srcWF.OpenFromFile(sourceFileName, 0);

                    #region 判断输入的数据库中的要素类或要素数据集是否都存在坐标系
                    IEnumDataset sourceEnumDataset = sourceWorkspace.get_Datasets(esriDatasetType.esriDTAny);
                    sourceEnumDataset.Reset();
                    IDataset sourceDataset = null;
                    while ((sourceDataset = sourceEnumDataset.Next()) != null)
                    {
                        if (sourceDataset is IFeatureDataset || sourceDataset is IFeatureClass)//要素数据集、要素类
                        {
                            var sr = (sourceDataset as IGeoDataset).SpatialReference;
                            if (sr == null || sr.Name.ToUpper() == "UNKNOWN")
                            {
                                noSRDBList.Add(sourceFileName);
                                break;
                            }
                        }
                    }
                    Marshal.ReleaseComObject(sourceEnumDataset);
                    #endregion

                    if(!noSRDBList.Contains(sourceFileName))
                        SourceWSList.Add(sourceWorkspace);
                }
            }
            if (noSRDBList.Count != 0)
            {
                string err = string.Format("输入的地理数据库中共存在【{0}】个地理数据库包含了未定义坐标系的要素类，分别如下：", noSRDBList.Count);
                foreach (var item in noSRDBList)
                {
                    err += "\n" + item;
                }
                MessageBox.Show(err);

                return;
            }
            

            DialogResult = System.Windows.Forms.DialogResult.OK;
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
