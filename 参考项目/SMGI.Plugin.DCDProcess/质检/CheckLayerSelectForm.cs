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
using SMGI.Common;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.DCDProcess
{
    public partial class CheckLayerSelectForm : Form
    {
        #region 属性
        public List<IFeatureLayer> CheckFeatureLayerList
        {
            get
            {
                return _checkFeatureLayerList;
            }
        }
        private List<IFeatureLayer> _checkFeatureLayerList;

        public bool EnableBetweenLayer
        {
            get
            {
                return cbBetweenLayers.Checked;
            }
        }

        public string OutputPath
        {
            get
            {
                return tbOutFilePath.Text;
            }
        }
        #endregion

        private GApplication _app;
        private bool _bPointType;
        private bool _bPolylineType;
        private bool _bPolygonType;
        private bool _setSavePath;
        public CheckLayerSelectForm(GApplication app, bool bPointType = true, bool bPolylineType = true, bool bPolygonType = true, bool enableBetweenLayerVisible = false)
        {
            InitializeComponent();

            _app = app;
            _bPointType = bPointType;
            _bPolylineType = bPolylineType;
            _bPolygonType = bPolygonType;

            cbBetweenLayers.Visible = enableBetweenLayerVisible;
            _setSavePath = true;
            tbOutFilePath.Text = OutputSetup.GetDir();
        }
        public CheckLayerSelectForm(GApplication app, bool bPointType , bool bPolylineType , bool bPolygonType , bool enableBetweenLayerVisible ,bool setSavePath)
        {
            InitializeComponent();

            _app = app;
            _bPointType = bPointType;
            _bPolylineType = bPolylineType;
            _bPolygonType = bPolygonType;
            _setSavePath = setSavePath;
            
            if (_setSavePath)
            {
                tbOutFilePath.Text = OutputSetup.GetDir(); 
            }
            else
            {
                label5.Visible = false;
                tbOutFilePath.Visible = false;
                btnFilePath.Visible = false;
                label2.Text = "待处理的图层列表";
                this.chkLayerList.Size = new System.Drawing.Size(470, 190);

            }
        }

        private void CheckLayerSelectForm_Load(object sender, EventArgs e)
        {
            //检索所有的点图层名称
            chkLayerList.ValueMember = "Key";
            chkLayerList.DisplayMember = "Value";
            var layers = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer));
            foreach (var lyr in layers)
            {
                IFeatureLayer feLayer = lyr as IFeatureLayer;
                if (feLayer.FeatureClass == null)
                    continue;//空图层

                if ((feLayer.FeatureClass as IDataset).Workspace.PathName != _app.Workspace.EsriWorkspace.PathName)
                    continue;//临时数据

                if (feLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint && !_bPointType)
                    continue;//点图层

                if (feLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline && !_bPolylineType)
                    continue;//线图层

                if (feLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon && !_bPolygonType)
                    continue;//面图层

                chkLayerList.Items.Add(new KeyValuePair<IFeatureLayer, string>(feLayer, feLayer.Name));
            }            
        }

        private void btnSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkLayerList.Items.Count; i++)
            {
                chkLayerList.SetItemChecked(i, true);
            }
        }

        private void btnUnSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkLayerList.Items.Count; i++)
            {
                chkLayerList.SetItemChecked(i, false);
            }
            chkLayerList.ClearSelected();
        }

        private void btnFilePath_Click(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK && fd.SelectedPath.Length > 0)
            {
                tbOutFilePath.Text = fd.SelectedPath;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (chkLayerList.CheckedItems.Count == 0)
            {
                MessageBox.Show("请选择至少一个图层进行检查！");
                return;
            }

            if (tbOutFilePath.Text == "" && _setSavePath)
            {
                MessageBox.Show("请指定检查结果输出路径！");
                return;
            }

            _checkFeatureLayerList = new List<IFeatureLayer>();
            foreach (var lyr in chkLayerList.CheckedItems)
            {
                var kv = (KeyValuePair<IFeatureLayer, string>)lyr;
                _checkFeatureLayerList.Add(kv.Key);
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }        
    }
}
