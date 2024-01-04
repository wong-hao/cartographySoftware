using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public partial class CheckFCFieldValueFrm : Form
    {

        public String LyrName;
        private GApplication _app;
        public string mdbPath;


        public CheckFCFieldValueFrm(GApplication app)
        {
            InitializeComponent();
            _app = app;
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void roadLyrNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LyrName = roadLyrNameComboBox.SelectedItem.ToString();
        }

        private void CheckFCFieldValueFrm_Load(object sender, EventArgs e)
        {
            // 在构造函数或 Load 事件中添加选项
            mdbPath = _app.Template.Root + "\\质检\\要素分层硬编码逻辑检查.mdb";
            List<string> tableNames = GetTableNamesFromMDB(mdbPath);

            // 使用 foreach 循环遍历列表中的元素
            foreach (string tableName in tableNames)
            {
                roadLyrNameComboBox.Items.Add(tableName);
            }

            LyrName = String.Empty;

            // 选择默认项（可选）
            roadLyrNameComboBox.SelectedIndex = 0; // 默认选中第一个选项，如果需要选择第二个选项，将索引设置为 1
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
    }
}
