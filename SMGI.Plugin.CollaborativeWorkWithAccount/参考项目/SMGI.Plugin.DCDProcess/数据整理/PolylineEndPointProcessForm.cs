using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.DCDProcess
{
    public partial class PolylineEndPointProcessForm : Form
    {
        private IFeatureLayer _objLayer;
        public IFeatureLayer ObjLayer
        {
            get
            {
                return _objLayer;
            }
        }

        public string FilterText
        {
            get
            {
                return rtbSQLText.Text.Trim();
            }
        }

        public double ThresholdValue
        {
            get
            {
                return double.Parse(tbBufferValue.Text);
            }
        }

        public PolylineEndPointProcessForm()
        {
            InitializeComponent();
        }

        private void PolylineEndPointProcessForm_Load(object sender, EventArgs e)
        {
            clbObjLayerList.ValueMember = "Key";
            clbObjLayerList.DisplayMember = "Value";

            var lineLayers = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline));
            foreach (var lyr in lineLayers)
            {
                IFeatureLayer feLayer = lyr as IFeatureLayer;
                if (feLayer.FeatureClass == null)
                    continue;//空图层

                clbObjLayerList.Items.Add(new KeyValuePair<IFeatureLayer, string>(feLayer, feLayer.Name));
            }
            _objLayer = null;
        }

        private void btnVerify_Click(object sender, EventArgs e)
        {
            if (clbObjLayerList.SelectedItem == null)
            {
                MessageBox.Show("请先选择目标要素类");
                return;
            }
            _objLayer = ((KeyValuePair<IFeatureLayer, string>)clbObjLayerList.SelectedItem).Key;

            string err = VerifySQL((_objLayer as IFeatureLayer).FeatureClass, rtbSQLText.Text.Trim());
            if (err != "")
            {
                MessageBox.Show(err);
            }
            else
            {
                MessageBox.Show("语法正确！");
            }
        }

        public static string VerifySQL(IFeatureClass fc, string filter)
        {
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = filter;
            try
            {
                int count = fc.FeatureCount(qf);
            }
            catch (Exception ex1)
            {
                System.Diagnostics.Trace.WriteLine(ex1.Message);
                System.Diagnostics.Trace.WriteLine(ex1.StackTrace);
                System.Diagnostics.Trace.WriteLine(ex1.Source);

                return string.Format("定义查询语句【{0}】错误:{1}!", filter, ex1.Message);
            }

            return "";

        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (clbObjLayerList.SelectedItem == null)
            {
                MessageBox.Show("目标图层名不能为空");
                return;
            }
            _objLayer = ((KeyValuePair<IFeatureLayer, string>)clbObjLayerList.SelectedItem).Key;
            if (((_objLayer.FeatureClass as IGeoDataset).SpatialReference as IProjectedCoordinateSystem) == null)
            {
                MessageBox.Show("目标图层的空间参考不为投影坐标系！");
                return;
            }

            if (FilterText != "")
            {
                string err = VerifySQL((_objLayer as IFeatureLayer).FeatureClass, rtbSQLText.Text.Trim());
                if (err != "")
                {
                    MessageBox.Show(err);
                    return;
                }
            }

            double bufferVal = 0;
            bool b = double.TryParse(tbBufferValue.Text, out bufferVal);
            if (!b || bufferVal <= 0)
            {
                MessageBox.Show("请输入一个合法的距离阈值");
                return;
            }

            

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
