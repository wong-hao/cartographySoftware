using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;

namespace SMGI.Plugin.DCDProcess
{
    public partial class BOULConstructBOUAFrm : Form
    {
        private GApplication _app;

        public string BOULFCName
        {
            get
            {
                return cbBOUL.Text;
            }
        }

        private IFeatureClass _rangeFC;
        public IFeatureClass RangeFC
        {
            get
            {
                return _rangeFC;
            }
        }

        private string _filterString;
        public string FilterString
        {
            get
            {
                return _filterString;
            }
        }

        public string BOUAFCName
        {
            get
            {
                return cbBOUA.Text;
            }
        }

        public bool NeedPropJoin
        {
            get
            {
                return cbPropJoin.Checked;
            }
        }

        public string PropJoinFCName
        {
            get
            {
                return cbProPlgFCName.Text;
            }
        }

        public bool UsingSpecialPath
        {
            get { return checkBox1.Checked; }
        }


        public BOULConstructBOUAFrm(GApplication app)
        {
            InitializeComponent();

            _app = app;
            _filterString = "";
            _rangeFC = null;
        }


        private void BOULConstructBOUAFrm_Load(object sender, EventArgs e)
        {
            //检索所有的线图层名称
            var lineLayers = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline));
            foreach (var lr in lineLayers)
            {
                cbBOUL.Items.Add(lr.Name);
            }

            int boulIndex = cbBOUL.Items.IndexOf("BOUL");
            if (boulIndex != -1)
            {
                cbBOUL.SelectedIndex = boulIndex;
            }

            //检索所有的面图层名称
            var polygonLayers = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon));
            foreach (var lr in polygonLayers)
            {
                cbBOUA.Items.Add(lr.Name);
                cbProPlgFCName.Items.Add(lr.Name);
            }

            int bouaIndex = cbBOUA.Items.IndexOf("BOUA");
            if (bouaIndex != -1)
            {
                cbBOUA.SelectedIndex = bouaIndex;
            }


            //境界类型
            cbTYPE.ValueMember = "Key";
            cbTYPE.DisplayMember = "Value";
            cbTYPE.Items.Add(new KeyValuePair<int, string>(660202, "乡级境界"));
            cbTYPE.Items.Add(new KeyValuePair<int, string>(650202, "县级境界"));
            cbTYPE.Items.Add(new KeyValuePair<int, string>(640202, "地级境界"));
            cbTYPE.Items.Add(new KeyValuePair<int, string>(630202, "省级境界"));

            cbTYPE.SelectedIndex = 1;

            cbPropJoin.Checked = false;
            cbProPlgFCName.Enabled = false;

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
            }
        }

        private void cbPropJoin_CheckedChanged(object sender, EventArgs e)
        {
            if (cbPropJoin.Checked)
            {
                cbProPlgFCName.Enabled = true;

                int index = cbProPlgFCName.Items.IndexOf(cbBOUA.Text);
                if (index != -1)
                {
                    cbProPlgFCName.SelectedIndex = index;
                }

            }
            else
            {
                cbProPlgFCName.Enabled = false;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (cbBOUL.Text == "")
            {
                MessageBox.Show("请指定境界线图层！");
                return;
            }

            if (cbBOUA.Text == "" && !checkBox1.Checked)
            {
                MessageBox.Show("请指定输出的面图层！");
                return;
            }

            if (tbShpFileName.Text != "")
            {
                _rangeFC = getRangeFeatureClass(tbShpFileName.Text);

                if ((_rangeFC as IGeoDataset).SpatialReference == null)
                {
                    MessageBox.Show("范围文件没有空间参考系！");
                    return;
                }
            }

            if (cbPropJoin.Checked)
            {
                if (cbProPlgFCName.Text == "")
                {
                    MessageBox.Show("请指定属性关联的面图层！");
                    return;
                }

            }

            var selectedItem = (KeyValuePair<int, string>)cbTYPE.SelectedItem;
            _filterString = string.Format("GB < {0}", selectedItem.Key);//参与构面的境界线条件

            DialogResult = DialogResult.OK;
        }

        

        //读取shp文件,获取范围几何体并返回空间参考名称
        private  IFeatureClass getRangeFeatureClass(string fileName)
        {
            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName(fileName), 0);
            IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;

            IFeatureClass fc = pFeatureWorkspace.OpenFeatureClass(System.IO.Path.GetFileName(fileName));

            //是否为多边形几何体
            if (fc.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                MessageBox.Show("范围文件应为多边形几何体，请重新指定范围文件！");
                return null;
            }

            return fc;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                cbBOUA.SelectedIndex = -1;                
            }           
        }

        private void cbBOUA_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cbBOUA.SelectedIndex>-1)
                checkBox1.Checked = false;
        }

        

    }
}
