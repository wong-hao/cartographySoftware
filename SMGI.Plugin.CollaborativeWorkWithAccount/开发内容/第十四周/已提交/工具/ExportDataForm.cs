using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesFile;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public partial class ExportDataForm : Form
    {
        public string IPAdress
        {
            get
            {
                return tbIPAdress.Text.Trim();
            }
        }
        public string UserName
        {
            get
            {
                return tbUserName.Text.Trim();
            }
        }
        public string Password
        {
            get
            {
                return tbPassword.Text.Trim();
            }
        }
        public string DataBase
        {
            get
            {
                return tbDataBase.Text.Trim();
            }
        }
        public bool Range
        {
            get
            {
                return cbRange.Checked;
            }
        }
        private IGeometry _rangeGeometry;
        public IGeometry RangeGeometry
        {
            get
            {
                return _rangeGeometry;
            }
        }
        public string OutputGDB
        {
            get
            {
                return tboutputGDB.Text;
            }
        }

        public bool DelCollaField
        {
            get
            {
                return cbDelCollaField.Checked;
            }
        }

        public bool FieldNameUpper
        {
            get
            {
                return cbFieldNameUpper.Checked;
            }
        }

        public bool DelEmptyLyr
        {
            get
            {
                return cbDelEmptyLyr.Checked;
            }
        }

        private List<string> _featureClassNameList = new List<string>();
        public List<string> lyrs
        {
            get
            {
                return _featureClassNameList;
            }
        }

        GApplication _app;

        public ExportDataForm(GApplication app)
        {
            InitializeComponent();

            _app = app;
            _rangeGeometry = null;

            if (RegistryHelper.IsRegistryExist(Registry.LocalMachine, "SOFTWARE", "SMGI"))
            {
                try
                {
                    string info = RegistryHelper.GetRegistryData(Registry.LocalMachine, "SOFTWARE\\SMGI", "DownLoad");
                    string[] Params = info.Split(',');
                    if (Params.Count() < 4)
                        return;

                    tbIPAdress.Text = Params[0];
                    tbDataBase.Text = Params[1];
                    tbPassword.Select();
                }
                catch
                {

                }
            }
        }

        private void cbRange_CheckedChanged(object sender, EventArgs e)
        {
            btnShpFile.Enabled = cbRange.Checked;
        }

        private void btnShpFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "选择一个范围文件";
            dlg.AddExtension = true;
            dlg.DefaultExt = "shp";
            dlg.Filter = "选择文件|*.shp";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tbShpFileName.Text = dlg.FileName;
                cbRange.Checked = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog pDialog = new SaveFileDialog();
            pDialog.AddExtension = true;
            pDialog.DefaultExt = "gdb";
            pDialog.Filter = "文件地理数据库|*.gdb";
            pDialog.FilterIndex = 0;
            if (pDialog.ShowDialog() == DialogResult.OK)
            {
                tboutputGDB.Text = pDialog.FileName;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbIPAdress.Text) || string.IsNullOrEmpty(tbDataBase.Text) || string.IsNullOrEmpty(tbUserName.Text) || string.IsNullOrEmpty(tbPassword.Text))
            {
                MessageBox.Show("请输入数据库联接信息！");
                return;
            }

            if (cbRange.Checked && string.IsNullOrEmpty(tbShpFileName.Text))
            {
                MessageBox.Show("请指定提取范围文件！");
                return;
            }

            if (string.IsNullOrEmpty(tboutputGDB.Text))
            {
                MessageBox.Show("请指定输出路径！");
                return;
            }

            if (cbRange.Checked)
            {
                string refName = getRangeGeometryReference(tbShpFileName.Text);
                if (string.IsNullOrEmpty(refName))
                {
                    MessageBox.Show("范围文件没有空间参考！");
                    return;
                }
            }

            _featureClassNameList.Clear();
            for (int i = 0; i < this.listView1.CheckedItems.Count; i++)
            {
                _featureClassNameList.Add(this.listView1.CheckedItems[i].Text);
            }

            DialogResult = DialogResult.OK;
        }


        //读取shp文件,获取范围几何体并返回空间参考名称
        private string getRangeGeometryReference(string fileName)
        {
            string refName = "";

            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName(fileName), 0);
            IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
            IFeatureClass shapeFC = pFeatureWorkspace.OpenFeatureClass(System.IO.Path.GetFileName(fileName));

            //是否为多边形几何体
            if (shapeFC.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                MessageBox.Show("范围文件应为多边形几何体，请重新指定范围文件！");
                tbShpFileName.Text = "";
                _rangeGeometry = null;
                return refName;
            }



            IPolygon geometry = new PolygonClass();

            IFeatureCursor featureCursor = shapeFC.Search(null, false);
            IFeature pFeature = null;
            int geoCount = 0;
            while ((pFeature = featureCursor.NextFeature()) != null)
            {
                if (pFeature != null && pFeature.Shape is IPolygon)
                {
                    geoCount++;
                    ITopologicalOperator2 rangeGeometryTopo = geometry as ITopologicalOperator2;
                    geometry = rangeGeometryTopo.Union(pFeature.Shape) as IPolygon;
                    refName = pFeature.Shape.SpatialReference.Name;
                    Marshal.ReleaseComObject(rangeGeometryTopo);
                }
            }

            _rangeGeometry = geometry;
            Marshal.ReleaseComObject(featureCursor);

            if (_rangeGeometry.IsEmpty)
            {
                MessageBox.Show("范围文件要素为空，请重新指定范围文件！");
                tbShpFileName.Text = "";
                _rangeGeometry = null;
                return refName;
            }
            else
            {
                IArea area = _rangeGeometry as IArea;
                MessageBox.Show(string.Format("已加载范围文件，共计{0}个几何要素，面积{1}", geoCount, area.Area));
                return refName;
            }
        }

        private void btn_Refresh_Click(object sender, EventArgs e)
        {
            refreshLayerList();
        }

        //获取图层名
        public List<string> getFeatureClassNames(GApplication app, string ipAddress, string userName, string passWord, string databaseName)
        {
            List<string> fcNames = new List<string>();

            IWorkspace pWorkspace = app.GetWorkspacWithSDEConnection(ipAddress, userName, passWord, databaseName);
            if (null == pWorkspace)
            {
                MessageBox.Show("无法访问服务器！");
                return fcNames;
            }

            IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspace;
            IEnumDataset pEnumDataset = pWorkspace.get_Datasets(esriDatasetType.esriDTAny);
            pEnumDataset.Reset();
            IDataset pDataset = null;

            while ((pDataset = pEnumDataset.Next()) != null)
            {
                if (pDataset.Name == string.Format("{0}.{1}.{2}", pWorkspace.ConnectionProperties.GetProperty("DATABASE"), "sde", "SMGIRangeGeometry"))
                {
                    continue;
                }
                if (pDataset is IFeatureDataset)//要素数据集
                {
                    IFeatureDataset pFeatureDataset = pFeatureWorkspace.OpenFeatureDataset(pDataset.Name);
                    IEnumDataset pEnumDatasetF = pFeatureDataset.Subsets;
                    pEnumDatasetF.Reset();
                    IDataset pDatasetF = pEnumDatasetF.Next();
                    while (pDatasetF != null)
                    {
                        if (pDatasetF is IFeatureClass)//要素类
                        {
                            IFeatureClass fc = pFeatureWorkspace.OpenFeatureClass(pDatasetF.Name);
                            if (fc != null)
                            {
                                fcNames.Add(fc.AliasName.Split('.').Last().ToUpper());
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(fc);
                            }
                        }

                        pDatasetF = pEnumDatasetF.Next();
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDatasetF);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureDataset);
                }
                else if (pDataset is IFeatureClass)//要素类
                {
                    IFeatureClass fc = pFeatureWorkspace.OpenFeatureClass(pDataset.Name);
                    if (fc != null)
                        fcNames.Add(fc.AliasName.Split('.').Last().ToUpper());
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fc);
                }
                else
                {
                    //table等类型的图层不予以处理
                }
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDataset);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspace);
            return fcNames;
        }

        private void refreshLayerList()
        {
            if (string.IsNullOrEmpty(tbIPAdress.Text) || string.IsNullOrEmpty(tbDataBase.Text) || string.IsNullOrEmpty(tbUserName.Text) || string.IsNullOrEmpty(tbPassword.Text))
            {
                MessageBox.Show("请输入数据库联接信息！");
                return;
            }
            this.listView1.Items.Clear();
            //获取图层名列表，填充选项
            using (var wo = _app.SetBusy())
            {
                wo.SetText("正在连接数据库...");
                List<string> fcNames = getFeatureClassNames(_app, tbIPAdress.Text, tbUserName.Text, tbPassword.Text, tbDataBase.Text);

                this.listView1.BeginUpdate();
                foreach (string fcName in fcNames)
                {
                    ListViewItem item = new ListViewItem();
                    item.SubItems.Clear();
                    item.SubItems[0].Text = fcName;
                    this.listView1.Items.Add(item);
                }
                this.listView1.EndUpdate();
            }
        }

        private void btn_All_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < this.listView1.Items.Count; i++)
            {
                this.listView1.Items[i].Checked = true;
            }
        }

        private void btn_Clear_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < this.listView1.Items.Count; i++)
            {
                this.listView1.Items[i].Checked = false;
            }
        }
    }
}
