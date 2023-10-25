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
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.DCDProcess
{
    public partial class LayerSelectForm : Form
    {
        public List<IFeatureLayer> SelectFeatureLayerList
        {
            get
            {
                return _selectFeatureLayerList;
            }
        }
        private List<IFeatureLayer> _selectFeatureLayerList;


        private GApplication _app;
        private bool _bPointType;
        private bool _bPolylineType;
        private bool _bPolygonType;
        public LayerSelectForm(GApplication app, bool bPointType = true, bool bPolylineType = true, bool bPolygonType = true)
        {
            InitializeComponent();

            _app = app;
            _bPointType = bPointType;
            _bPolylineType = bPolylineType;
            _bPolygonType = bPolygonType;
        }

        private void LayerSelectForm_Load(object sender, EventArgs e)
        {
            //检索所有的图层名称
            LayerNameList.ValueMember = "Key";
            LayerNameList.DisplayMember = "Value";
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

                LayerNameList.Items.Add(new KeyValuePair<IFeatureLayer, string>(feLayer, feLayer.Name));
            }
        }

        private void btnSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < LayerNameList.Items.Count; i++)
            {
                LayerNameList.SetItemChecked(i, true);
            }
        }

        private void btnUnSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < LayerNameList.Items.Count; i++)
            {
                LayerNameList.SetItemChecked(i, false);
            }
            LayerNameList.ClearSelected();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (LayerNameList.CheckedItems.Count == 0)
            {
                MessageBox.Show("请选择至少一个待处理图层！");
                return;
            }

            _selectFeatureLayerList = new List<IFeatureLayer>();
            foreach (var lyr in LayerNameList.CheckedItems)
            {
                var kv = (KeyValuePair<IFeatureLayer, string>)lyr;
                _selectFeatureLayerList.Add(kv.Key);
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
