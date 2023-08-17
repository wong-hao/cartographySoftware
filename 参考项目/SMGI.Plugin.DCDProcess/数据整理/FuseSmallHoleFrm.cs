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

namespace SMGI.Plugin.DCDProcess
{
    public partial class FuseSmallHoleFrm : Form
    {
        private GApplication _app;
        private List<esriGeometryType> _layerTypeList;


        //融合孔洞的目标图层名
        public string TargetLayerName
        {
            get
            {
                return cbTargetLayerName.Text;
            }
        }

        //选择的相关图层名
        private List<string> _referLayerNames;
        public List<string> ReferLayerNames
        {
            get
            {
                return _referLayerNames;
            }
        }

        /// <summary>
        /// 需保留的最小孔洞大小
        /// </summary>
        public double MinHoleArea
        {
            get
            {
                return double.Parse(tbMinHoleArea.Text);
            }
        }

        public FuseSmallHoleFrm(GApplication app, List<esriGeometryType> layerTypeList = null)
        {
            InitializeComponent();

            _app = app;
            _layerTypeList = layerTypeList;
            _referLayerNames = new List<string>();
        }

        private void LayerSelectFrm_Load(object sender, EventArgs e)
        {
            List<string> lyNames = new List<string>();
            var pLayers = _app.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
                (l is IGeoFeatureLayer))).ToArray();
            for (int i = 0; i < pLayers.Length; i++)
            {
                IFeatureLayer featurelayer = pLayers[i] as IFeatureLayer;

                if (_layerTypeList != null && _layerTypeList.Count > 0)
                {
                    esriGeometryType type = featurelayer.FeatureClass.ShapeType;
                    if (!_layerTypeList.Contains(type))
                        continue;
                }

                lyNames.Add(featurelayer.Name);
            }

            //初始化
            cbTargetLayerName.Items.AddRange(lyNames.ToArray());
            int selIndex = cbTargetLayerName.Items.Count - 1;
            int vegaIndex = cbTargetLayerName.Items.IndexOf("VEGA");
            if (vegaIndex != -1)
            {
                selIndex = vegaIndex;
            }
            cbTargetLayerName.SelectedIndex = selIndex;
        }

        private void cbTargetLayerName_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCheckListBox();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (cbTargetLayerName.Text == "")
            {
                MessageBox.Show("请指定孔洞融合的目标图层！");
                return;
            }

            foreach (var ln in checkedListBox.CheckedItems)
            {
                _referLayerNames.Add(ln as string);
            }

            double area = 0;
            double.TryParse(tbMinHoleArea.Text, out area);
            if (area <= 0)
            {
                MessageBox.Show("请指定一个大于0的最小孔洞面积！");
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private void UpdateCheckListBox()
        {
            checkedListBox.Items.Clear();

            for (int i = 0; i < cbTargetLayerName.Items.Count; ++i)
            {
                if (i == cbTargetLayerName.SelectedIndex)
                    continue;

                checkedListBox.Items.Add(cbTargetLayerName.Items[i].ToString());
            }
        }

        
    }
}
