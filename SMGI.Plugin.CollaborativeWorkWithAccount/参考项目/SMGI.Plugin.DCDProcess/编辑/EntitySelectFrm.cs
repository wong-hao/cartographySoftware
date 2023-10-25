using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.DCDProcess
{
    public partial class EntitySelectFrm : Form
    {

        private readonly IMap _map;
        private readonly Dictionary<string, IFeatureLayer> _layerDictionary = new Dictionary<string, IFeatureLayer>();
        //记录上次设置的记录
        private string _curLayerName = "";
        List<string> _curFdlist = new List<string>();
        public IFeatureLayer SelectFeatureLayer
        {
            get
            {
                if (_curLayerName == "" || !_layerDictionary.ContainsKey(_curLayerName))
                    return null;
                return _layerDictionary[_curLayerName];
            }
        }

        public List<string> SelectFields
        {
            get
            {
                return _curFdlist;
            }
        }

        public EntitySelectFrm(IMap map)
        {

            InitializeComponent();
            _map = map;


        }

        private void EntitySelectFrm_Load(object sender, EventArgs e)
        {
            //if (_layerDictionary.Count > 0) return;

            _layerDictionary.Clear();


            for (int i = 0; i < _map.LayerCount; i++)
            {
                ILayer pLayer = _map.Layer[i];
                GetLayers(pLayer);
            }

            foreach (var layerName in _layerDictionary.Keys)
            {
                cmbLayers.Items.Add(layerName);
            }
            if (_curLayerName == "" || !_layerDictionary.ContainsKey(_curLayerName))
            {
                cmbLayers.SelectedIndex = 0;
                _curFdlist.Clear();
            }
            else
            {
                var idx = cmbLayers.Items.IndexOf(_curLayerName);
                cmbLayers.SelectedIndex = idx;
            }
            _curLayerName = cmbLayers.SelectedItem.ToString();
            SetFields(SelectFeatureLayer);
            if (_curFdlist != null && _curFdlist.Count > 0)
            {
                for (var i = 0; i < chkFields.Items.Count; i++)
                {
                    foreach (var fd in _curFdlist)
                    {
                        if (chkFields.Items[i] == fd)
                        {
                            chkFields.SetItemChecked(i, true);
                            break;
                        }
                    }
                }
            }
        }


        private void SetFields(IFeatureLayer fl)
        {
            var fc = fl.FeatureClass;
            var ruleNames = new List<string>();
            for (int i = 0; i < fc.Fields.FieldCount; i++)
            {
                var field = fc.Fields.Field[i];
                if (field.Type == esriFieldType.esriFieldTypeInteger || field.Type == esriFieldType.esriFieldTypeSmallInteger || field.Type == esriFieldType.esriFieldTypeString)
                {
                    if (!field.Name.Contains("smgi"))
                        ruleNames.Add(field.Name);
                }
            }

            chkFields.Items.Clear();
            chkFields.Items.AddRange(ruleNames.ToArray());


        }
        private void GetLayers(ILayer pLayer)
        {
            if (pLayer is IGroupLayer)
            {
                var pGroupLayer = (ICompositeLayer)pLayer;
                for (int i = 0; i < pGroupLayer.Count; i++)
                {
                    ILayer subLayer = pGroupLayer.Layer[i];
                    GetLayers(subLayer);
                }
            }
            else
            {
                if (pLayer is IFeatureLayer)
                {
                    var pFeatLayer = (IFeatureLayer)pLayer;

                    if (null == pFeatLayer.FeatureClass) return;

                    _layerDictionary.Add(pLayer.Name, pFeatLayer);
                }
            }
        }

        private void cmbLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            _curLayerName = cmbLayers.SelectedItem.ToString();
            _curFdlist.Clear();
            SetFields(SelectFeatureLayer);
        }

        private void chkFields_SelectedValueChanged(object sender, EventArgs e)
        {
            _curFdlist = chkFields.CheckedItems.Cast<string>().ToList();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
