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

namespace SMGI.Plugin.DCDProcess
{
    public partial class CheckPropertiesConsistencyOfSameFeatureForm : Form
    {
        #region 属性
        public IFeatureLayer CheckFeatureLayer
        {
            get;
            internal set;
        }

        public string ObjFieldName
        {
            get
            {
                return cbObjFieldName.Text;
            }
        }

        public List<string> ReferFieldNameList
        {
            get
            {
                return _referFieldNameList;
            }
        }
        private List<string> _referFieldNameList;

        public bool BEliminateNullValue
        {
            get
            {
                return cbExcept.Checked;
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
        private List<string> _fdNameList;//只存储文本型和数字型
        public CheckPropertiesConsistencyOfSameFeatureForm(GApplication app)
        {
            InitializeComponent();

            _app = app;
            _fdNameList = new List<string>();
        }

        private void PropertiesConsistencyOfSameFeatureForm_Load(object sender, EventArgs e)
        {
            cbLayerName.ValueMember = "Key";
            cbLayerName.DisplayMember = "Value";
            var layers = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer));
            int hydlIndex = -1;
            foreach (var lyr in layers)
            {
                IFeatureLayer feLayer = lyr as IFeatureLayer;
                if (feLayer.FeatureClass == null)
                    continue;//空图层


                int index = cbLayerName.Items.Add(new KeyValuePair<IFeatureLayer, string>(feLayer, feLayer.Name));
                if(feLayer.Name == "HYDL")
                {
                    hydlIndex = index;
                }
            }

            if (cbLayerName.Items.Count > 0)
            {
                cbLayerName.SelectedIndex = 0;
                if (hydlIndex > 0)
                    cbLayerName.SelectedIndex = hydlIndex;
            }

            tbOutFilePath.Text = OutputSetup.GetDir();
        }

        private void cbLayerName_SelectedIndexChanged(object sender, EventArgs e)
        {
            _fdNameList = new List<string>();

            var item = (KeyValuePair<IFeatureLayer, string>)cbLayerName.SelectedItem;
            IFeatureClass fc = item.Key.FeatureClass;
            if (fc != null)
            {
                for (var i = 0; i < fc.Fields.FieldCount; i++)
                {
                    var fd = fc.Fields.Field[i];

                    if (fd.Name.ToUpper() == "SHAPE_LENGTH" || fd.Name.ToUpper() == "SHAPE_AREA")
                        continue;

                    if (fd.Type == esriFieldType.esriFieldTypeString ||
                        fd.Type == esriFieldType.esriFieldTypeSmallInteger ||
                        fd.Type == esriFieldType.esriFieldTypeInteger ||
                        fd.Type == esriFieldType.esriFieldTypeSingle || 
                        fd.Type == esriFieldType.esriFieldTypeDouble)//文本、数字型字段
                    {
                        _fdNameList.Add(fd.Name);
                    }
                }
            }

            //更新待检查字段组合框
            cbObjFieldName.Items.Clear();
            if (_fdNameList.Count > 0)
            {
                cbObjFieldName.Items.AddRange(_fdNameList.ToArray());
            }

            //清空参考字段表
            chkReferFieldList.Items.Clear();
        }

        private void cbObjFieldName_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkReferFieldList.Items.Clear();

            foreach (var fn in _fdNameList)
            {
                if (fn == cbObjFieldName.Text)
                    continue;

                chkReferFieldList.Items.Add(fn);
            }
        }

        private void btnOutputPath_Click(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK && fd.SelectedPath.Length > 0)
            {
                btnOutputPath.Text = fd.SelectedPath;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (cbLayerName.SelectedIndex == -1)
            {
                MessageBox.Show("请指定待检查图层！");
                return;
            }
            CheckFeatureLayer = ((KeyValuePair<IFeatureLayer, string>)cbLayerName.SelectedItem).Key;

            if (cbObjFieldName.Text == "")
            {
                MessageBox.Show("请指定待检查字段！");
                return;
            }

            if (chkReferFieldList.CheckedItems.Count == 0)
            {
                MessageBox.Show("请选择至少一个参考字段项！");
                return;
            }

            if (tbOutFilePath.Text == "")
            {
                MessageBox.Show("请指定检查结果输出路径！");
                return;
            }

            _referFieldNameList = new List<string>();
            foreach (var item in chkReferFieldList.CheckedItems)
            {
                _referFieldNameList.Add(item.ToString());
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
