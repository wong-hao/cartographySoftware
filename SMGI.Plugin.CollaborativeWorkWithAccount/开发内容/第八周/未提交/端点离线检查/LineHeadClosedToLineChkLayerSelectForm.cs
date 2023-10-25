using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using SMGI.Common;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public partial class LineHeadClosedToLineChkLayerSelectForm : Form
    {
        private GApplication _app;
        private Dictionary<string, ILayer> _layerDictionary = new Dictionary<string, ILayer>();
        public ILayer pSelectLayer;
        public esriGeometryType GeoTypeFilter { get; set; }

        public LineHeadClosedToLineChkLayerSelectForm(GApplication app)
        {
            InitializeComponent();
            _app = app;
        }

        private void GetLayers(ILayer pLayer, esriGeometryType pGeoType, Dictionary<string, ILayer> LayerList)
        {
            if (pLayer is IGroupLayer)
            {
                ICompositeLayer pGroupLayer = (ICompositeLayer)pLayer;
                for (int i = 0; i < pGroupLayer.Count; i++)
                {
                    ILayer SubLayer = pGroupLayer.get_Layer(i);
                    GetLayers(SubLayer, pGeoType, LayerList);
                }
            }
            else
            {
                if (pLayer is IFeatureLayer)
                {
                    IFeatureLayer pFeatLayer = (IFeatureLayer)pLayer;
                    IFeatureClass pFeatClass = pFeatLayer.FeatureClass;
                    if (null == pFeatClass) return;
                    if (pFeatClass.ShapeType == pGeoType)
                    {
                        LayerList.Add(pLayer.Name, pLayer);
                    }
                }
            }
        }

        private void LayerSelectForm_Load(object sender, EventArgs e)
        {
            var acv = _app.ActiveView;
            var map = acv.FocusMap;

            for (int i = 0; i < map.LayerCount; i++)
            {
                ILayer pLayer = map.get_Layer(i);
                GetLayers(pLayer, GeoTypeFilter, _layerDictionary);
            }

            foreach (var layerName in _layerDictionary.Keys)
            {
                if (layerName != "高速公路" && layerName != "国省县道" && layerName != "乡道" && layerName != "其他道路" && layerName != "城市道路" && layerName != "河流" && layerName != "水渠")
                {
                    this.clbLayerList.Items.Add(layerName);
                }
            }
            this.clbLayerList.SelectedIndex = 0;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (tbValue.Text.Trim() == "")
            {
                MessageBox.Show("字段名不能为空");
                return;
            }

            if (clbLayerList.SelectedItem.ToString().Trim() == "")
            {
                MessageBox.Show("图层名不能为空");
                return;
            }

            pSelectLayer = _layerDictionary[clbLayerList.SelectedItem.ToString()];
            this.Close();
        }
        //图层变化
        private void clbLayerList_SelectedIndexChanged(object sender, EventArgs e)
        {
            string layerName = clbLayerList.SelectedItem.ToString();
        }

    }
}
