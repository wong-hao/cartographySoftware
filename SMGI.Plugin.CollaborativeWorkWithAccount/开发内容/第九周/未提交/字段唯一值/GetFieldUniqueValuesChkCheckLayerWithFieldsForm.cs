﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public partial class GetFieldUniqueValuesChkCheckLayerWithFieldsForm : Form
    {
        #region 属性
        public IFeatureClass CheckFeatureClass
        {
            get;
            internal set;
        }

        public List<string> FieldNameList
        {
            get
            {
                return _fieldNameList;
            }
        }
        private List<string> _fieldNameList;

        #endregion

        private GApplication _app;
        private bool _bOnlyStringField;
        public GetFieldUniqueValuesChkCheckLayerWithFieldsForm(GApplication app, bool bOnlyStringField = true)
        {
            InitializeComponent();

            _app = app;
            _bOnlyStringField = bOnlyStringField;

        }

        private void CheckLayerWithFieldsForm_Load(object sender, EventArgs e)
        {
            cbLayerName.ValueMember = "Key";
            cbLayerName.DisplayMember = "Value";
            var layers = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer));
            foreach (var lyr in layers)
            {
                IFeatureLayer feLayer = lyr as IFeatureLayer;
                if (feLayer.FeatureClass == null)
                    continue;//空图层

                if ((feLayer.FeatureClass as IDataset).Workspace.PathName != _app.Workspace.EsriWorkspace.PathName)
                    continue;//临时数据

                cbLayerName.Items.Add(new KeyValuePair<IFeatureClass, string>(feLayer.FeatureClass, feLayer.Name));
            }
            if (cbLayerName.Items.Count > 0)
            {
                cbLayerName.SelectedIndex = 0;
            }
        }

        private void cbLayerName_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkFieldList.Items.Clear();

            var item = (KeyValuePair<IFeatureClass, string>)cbLayerName.SelectedItem;
            IFeatureClass fc = item.Key;
            if (fc != null)
            {
                for (var i = 0; i < fc.Fields.FieldCount; i++)
                {
                    var fd = fc.Fields.Field[i];

                    if (fd.Name == "SOURCE" || fd.Name == "REMARK" || fd.Name == "CHECK")
                    {
                        chkFieldList.Items.Add(fd.Name);

                    }
                }
            }
        }

        private void btnSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkFieldList.Items.Count; i++)
            {
                chkFieldList.SetItemChecked(i, true);
            }
        }

        private void btnUnselAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkFieldList.Items.Count; i++)
            {
                chkFieldList.SetItemChecked(i, false);
            }
            chkFieldList.ClearSelected();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (cbLayerName.SelectedIndex == -1)
            {
                MessageBox.Show("请指定待检查图层！");
                return;
            }
            CheckFeatureClass = ((KeyValuePair<IFeatureClass, string>)cbLayerName.SelectedItem).Key;

            if (chkFieldList.CheckedItems.Count == 0)
            {
                MessageBox.Show("请选择至少一个字段项！");
                return;
            }

            _fieldNameList = new List<string>();
            foreach (var item in chkFieldList.CheckedItems)
            {
                _fieldNameList.Add(item.ToString());
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
