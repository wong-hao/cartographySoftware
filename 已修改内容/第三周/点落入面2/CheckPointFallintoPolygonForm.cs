using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public partial class CheckPointFallintoPolygonForm : Form
    {
        #region 属性
        public IFeatureClass PointFeatureClass
        {
            get
            {
                return _pointFeatureClass;
            }
        }
        private IFeatureClass _pointFeatureClass;

        public string PointFilterString
        {
            get
            {
                return tbPointFilterString.Text;
            }
        }

        public IFeatureClass AreaFeatureClass
        {
            get
            {
                return _areaFeatureClass;
            }
        }
        private IFeatureClass _areaFeatureClass;

        public string AreaFilterString
        {
            get
            {
                return tbAreaFilterString.Text;
            }
        }

        public string ResultOutputFilePath
        {
            get
            {
                return tbOutFilePath.Text;
            }
        }
        #endregion

        private GApplication _app;
        public CheckPointFallintoPolygonForm(GApplication app)
        {
            InitializeComponent();

            _app = app;
        }

        private void CheckPointFallintoPolygonForm_Load(object sender, EventArgs e)
        {
            //初始化点图层
            cbPointLayerName.ValueMember = "Key";
            cbPointLayerName.DisplayMember = "Value";
            var pointLayers = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint));
            foreach (var lyr in pointLayers)
            {
                IFeatureLayer feLayer = lyr as IFeatureLayer;
                if (feLayer.FeatureClass == null)
                    continue;//空图层

                if ((feLayer.FeatureClass as IDataset).Workspace.PathName != _app.Workspace.EsriWorkspace.PathName)
                    continue;//临时数据

                cbPointLayerName.Items.Add(new KeyValuePair<IFeatureClass, string>(feLayer.FeatureClass, feLayer.Name));
            }
            if (cbPointLayerName.Items.Count > 0)
            {
                cbPointLayerName.SelectedIndex = 0;
            }


            //初始化面图层
            cbAreaLayerName.ValueMember = "Key";
            cbAreaLayerName.DisplayMember = "Value";
            var areaLayers = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon));
            foreach (var lyr in areaLayers)
            {
                IFeatureLayer feLayer = lyr as IFeatureLayer;
                if (feLayer.FeatureClass == null)
                    continue;//空图层

                if ((feLayer.FeatureClass as IDataset).Workspace.PathName != _app.Workspace.EsriWorkspace.PathName)
                    continue;//临时数据

                cbAreaLayerName.Items.Add(new KeyValuePair<IFeatureClass, string>(feLayer.FeatureClass, feLayer.Name));
            }
            if (cbAreaLayerName.Items.Count > 0)
            {
                cbAreaLayerName.SelectedIndex = 0;
            }

            tbOutFilePath.Text = OutputSetup.GetDir();
        }

        private void btnOutputPath_Click(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK && fd.SelectedPath.Length > 0)
            {
                tbOutFilePath.Text = fd.SelectedPath;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (cbPointLayerName.SelectedIndex == -1)
            {
                MessageBox.Show("请指定点图层！");
                return;
            }
            var item = (KeyValuePair<IFeatureClass, string>)cbPointLayerName.SelectedItem;
            _pointFeatureClass = item.Key;

            if (cbAreaLayerName.SelectedIndex == -1)
            {
                MessageBox.Show("请指定面图层！");
                return;
            }
            item = (KeyValuePair<IFeatureClass, string>)cbAreaLayerName.SelectedItem;
            _areaFeatureClass = item.Key;

            //验证过滤条件是否合法
            if (tbPointFilterString.Text != "")
            {
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = tbPointFilterString.Text;

                try
                {
                    int feCount = _pointFeatureClass.FeatureCount(qf);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    MessageBox.Show(string.Format("点图层的过滤条件输入不合法：{0}", ex.Message));
                    return;
                }
            }
            if (tbAreaFilterString.Text != "")
            {
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = tbAreaFilterString.Text;

                try
                {
                    int feCount = _areaFeatureClass.FeatureCount(qf);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    MessageBox.Show(string.Format("面图层的过滤条件输入不合法：{0}", ex.Message));
                    return;
                }
            }

            if (tbOutFilePath.Text == "")
            {
                MessageBox.Show("请指定检查结果输出路径！");
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
