using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public partial class CheckFCFieldValueFrm : Form
    {
        public List<IFeatureLayer> CheckFeatureLayerList
        {
            get
            {
                return _checkFeatureLayerList;
            }
        }
        private List<IFeatureLayer> _checkFeatureLayerList;

        public List<string> CheckFeatureLayerNameList
        {
            get
            {
                return _checkFeatureLayerNameList;
            }
        }
        private List<string> _checkFeatureLayerNameList;

        private GApplication _app;
        public string mdbPath;


        public CheckFCFieldValueFrm(GApplication app)
        {
            InitializeComponent();
            _app = app;
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            _checkFeatureLayerList = new List<IFeatureLayer>();
            _checkFeatureLayerNameList = new List<string>();

            foreach (var lyr in chkLayerList.CheckedItems)
            {
                var kv = (KeyValuePair<IFeatureLayer, string>)lyr;
                _checkFeatureLayerList.Add(kv.Key);
                _checkFeatureLayerNameList.Add(kv.Value);
            }

            if (CheckFeatureLayerNameList.Count == 0)
            {
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void CheckFCFieldValueFrm_Load(object sender, EventArgs e)
        {
            // 在构造函数或 Load 事件中添加选项
            mdbPath = _app.Template.Root + "\\质检\\属性逻辑关系硬编码检查.mdb";
            List<string> tableNames = GetTableNamesFromMDB(mdbPath);

            // 使用 foreach 循环遍历列表中的元素
            chkLayerList.ValueMember = "Key";
            chkLayerList.DisplayMember = "Value";

            foreach (string tableName in tableNames)
            {
                IFeatureLayer tableNameLayer = (_app.Workspace.LayerManager.GetLayer(
                    l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == tableName)).FirstOrDefault() as IFeatureLayer);
                chkLayerList.Items.Add(new KeyValuePair<IFeatureLayer, string>(tableNameLayer, tableName));
            }
        }

        public static List<string> GetTableNamesFromMDB(string mdbFilePath)
        {
            List<string> tableNames = new List<string>();

            string connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + mdbFilePath + ";";
            using (OleDbConnection conn = new OleDbConnection(connString))
            {
                conn.Open();

                // 获取表格信息
                var schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

                if (schema != null)
                {
                    foreach (DataRow row in schema.Rows)
                    {
                        string tableName = row["TABLE_NAME"].ToString();
                        tableNames.Add(tableName);
                    }
                }
            }

            return tableNames;
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
    }
}
