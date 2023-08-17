using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using Microsoft.Win32;
using SMGI.Common;

namespace SMGI.Plugin.DCDProcess
{
    public partial class LayerSelectWithFiedsForm : Form
    {
        private GApplication _app;
        private Dictionary<string, ILayer> _layerDictionary = new Dictionary<string, ILayer>();
        public ILayer pSelectLayer;
        public esriGeometryType GeoTypeFilter{get;set;}

        private Dictionary<string, List<string>> fieldsDic = new Dictionary<string, List<string>>();
        public List<string> FieldArray { get; set; }
        public string Shapetxt { get; set; }

        public LayerSelectWithFiedsForm(GApplication app)
        {
            InitializeComponent();
            _app = app;
            FieldArray =new List<string>();
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
                    if(null==pFeatClass)return;
                    
                    if (pFeatClass.ShapeType == pGeoType)
                    {
                        fieldsDic.Add(pLayer.Name, GetFields(pFeatClass));
                        LayerList.Add(pLayer.Name, pLayer);
                    }
                }
            }
        }

        private List<string> GetFields(IFeatureClass fc)
        {
            List<string> ruleNames = new List<string>();
            for (int i = 0; i < fc.Fields.FieldCount; i++)
            {
                var field = fc.Fields.get_Field(i);
                if (field.Type!= esriFieldType.esriFieldTypeGeometry && field.Type!= esriFieldType.esriFieldTypeOID)
                {
                    ruleNames.Add(field.Name);
                }
            }
            return ruleNames;
        }
        private void LayerSelectForm_Load(object sender, EventArgs e)
        {
            if(_layerDictionary.Count>0)return;
            var acv = _app.ActiveView;
            var map = acv.FocusMap;

            for (int i = 0; i < map.LayerCount; i++)
            {
                ILayer pLayer = map.get_Layer(i);
                GetLayers(pLayer, GeoTypeFilter, _layerDictionary);
            }

            foreach (var layerName in _layerDictionary.Keys)
            {
                this.clbLayerList.Items.Add(layerName);
            }

            this.clbLayerList.SelectedIndex = 0;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (clbLayerList.SelectedItem.ToString().Trim() == "")
            {
                MessageBox.Show("图层名不能为空");
                return;
            }

            if (chkShp.Checked&& shapetxt.Text.ToString().Trim() == "")
            {
                MessageBox.Show("作业范围文件不能为空");
                return;
            }
            Shapetxt = shapetxt.Text.ToString().Trim();
            pSelectLayer = _layerDictionary[clbLayerList.SelectedItem.ToString()];

            for (int i = 0; i < checkedFieldNames.CheckedItems.Count; i++)
            {
                var item = checkedFieldNames.CheckedItems[i];
                FieldArray.Add(item.ToString());
            }

            this.Close();
        }
        //图层变化
        private void clbLayerList_SelectedIndexChanged(object sender, EventArgs e)
        {
            string layerName=clbLayerList.SelectedItem.ToString();
            checkedFieldNames.Items.Clear();
            //checkedFieldNames.Items.AddRange(fieldsDic[layerName].ToArray());
            foreach (var item in fieldsDic[layerName].ToArray())
            {
                if (item.ToUpper() != "SHAPE_LENGTH")
                {
                    checkedFieldNames.Items.Add(item);
                }
            }
        }

        private void btnShape_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            try
            {
                ITable localDT = (_app.Workspace as IFeatureWorkspace).OpenTable("SMGILocalState");
                ICursor rowCursor = localDT.Search(null, false);
                IRow row = rowCursor.NextRow();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rowCursor);
                if (row != null)
                {
                    int index = localDT.Fields.FindField("EXTENTNAME");
                    if (index != -1)
                    {
                        string extentFileName = row.get_Value(index).ToString();
                        if (System.IO.File.Exists(extentFileName))
                            dlg.InitialDirectory = System.IO.Path.GetDirectoryName(extentFileName);                        
                    }
                }
            }
            catch { }
            
            dlg.Title = "选择一个范围文件";
            dlg.AddExtension = true;
            dlg.DefaultExt = "shp";
            dlg.Filter = "选择文件|*.shp";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                shapetxt.Text = dlg.FileName;
            }

        }

        private void chkShp_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShp.Checked)
            {
                label1.Enabled = true;
                shapetxt.Enabled = true;
                btnShape.Enabled = true;
            }
            else {
                label1.Enabled = false;
                shapetxt.Enabled = false;
                btnShape.Enabled = false;

                Shapetxt = "";
                shapetxt.Text = "";
            
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (var i=0;i< checkedFieldNames.Items.Count;i++)
            {
                 checkedFieldNames.SetItemChecked(i,true);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < checkedFieldNames.Items.Count; i++)
            {
                checkedFieldNames.SetItemChecked(i, false);
            }
        } 
    }
}
