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
    public partial class LineFeatureMatchForm : Form
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

        public double BufferValue
        {
            get
            {
                return double.Parse(tbBufferValue.Text);
            }
        }

        private IFeatureLayer _referLayer;
        public IFeatureLayer ReferLayer
        {
            get
            {
                return _referLayer;
            }
        }

        private List<string> _matchFNList;
        public List<string> MatchFNList
        {
            get
            {
                return _matchFNList;
            }
        }


        public string CheckFN
        {
            get
            {
                return cbCheckFN.Text;
            }
        }

        public LineFeatureMatchForm()
        {
            InitializeComponent();

            clbObjLayerList.ValueMember = "Key";
            clbObjLayerList.DisplayMember = "Value";

            clbReferLayerList.ValueMember = "Key";
            clbReferLayerList.DisplayMember = "Value";
            

            var lineLayers = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline));
            foreach (var lyr in lineLayers)
            {
                IFeatureLayer feLayer = lyr as IFeatureLayer;
                if (feLayer.FeatureClass == null)
                    continue;//空图层

                clbObjLayerList.Items.Add(new KeyValuePair<IFeatureLayer, string>(feLayer, feLayer.Name));
                clbReferLayerList.Items.Add(new KeyValuePair<IFeatureLayer, string>(feLayer, feLayer.Name));
            }
            _objLayer = null;
            _referLayer = null;


        }

        private void clbObjLayerList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFNList();
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

        private void clbReferLayerList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFNList();
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
                MessageBox.Show("请输入一个合法的缓冲距离");
                return;
            }

            if (clbReferLayerList.SelectedItem == null)
            {
                MessageBox.Show("参考图层名不能为空");
                return;
            }
            _referLayer = ((KeyValuePair<IFeatureLayer, string>)clbReferLayerList.SelectedItem).Key;
            if (((_referLayer.FeatureClass as IGeoDataset).SpatialReference as IProjectedCoordinateSystem) == null)
            {
                MessageBox.Show("参考图层的空间参考不为投影坐标系！");
                return;
            }

            if (_referLayer == _objLayer)
            {
                MessageBox.Show("参考图层不能选择目标图层！");
                return;
            }

            _matchFNList = new List<string>();
            foreach (var item in chkMatchFieldList.CheckedItems)
            {
                _matchFNList.Add(item.ToString());
            }


            if (cbCheckFN.Text == "")
            {
                MessageBox.Show("请选择待核查字段！");
                return;
            }

            if (_matchFNList.Contains(cbCheckFN.Text))
            {
                MessageBox.Show("待核查字段不能作为匹配字段！");
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void UpdateFNList()
        {
            chkMatchFieldList.Items.Clear();
            cbCheckFN.Items.Clear();

            if (clbObjLayerList.SelectedItem == null)
            {
                return;
            }
            _objLayer = ((KeyValuePair<IFeatureLayer, string>)clbObjLayerList.SelectedItem).Key;

            if (clbReferLayerList.SelectedItem == null)
            {
                return;
            }
            _referLayer = ((KeyValuePair<IFeatureLayer, string>)clbReferLayerList.SelectedItem).Key;


            //获取图层字段名称
            List<string> objFNList = new List<string>();
            List<string> publicFNList = new List<string>();

            for (int i = 0; i < _objLayer.FeatureClass.Fields.FieldCount; ++i)
            {
                var fd = _objLayer.FeatureClass.Fields.Field[i];

                if (fd.Type == esriFieldType.esriFieldTypeGeometry ||
                    fd.Type == esriFieldType.esriFieldTypeOID)
                    continue;

                if (fd.Name.ToUpper() == "SHAPE_LENGTH" || fd.Name.ToUpper() == "SHAPE_AREA")
                    continue;

                objFNList.Add(fd.Name.ToUpper());
            }
            for (int i = 0; i < _referLayer.FeatureClass.Fields.FieldCount; ++i)
            {
                var fd = _referLayer.FeatureClass.Fields.Field[i];

                if (fd.Type == esriFieldType.esriFieldTypeGeometry ||
                    fd.Type == esriFieldType.esriFieldTypeOID)
                    continue;

                if (fd.Name.ToUpper() == "SHAPE_LENGTH" || fd.Name.ToUpper() == "SHAPE_AREA")
                    continue;

                if (objFNList.Contains(fd.Name.ToUpper()))
                {
                    publicFNList.Add(fd.Name.ToUpper());
                }
                
            }

            foreach (var item in publicFNList)
            {
                chkMatchFieldList.Items.Add(item);

                cbCheckFN.Items.Add(item);
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
    }
}
