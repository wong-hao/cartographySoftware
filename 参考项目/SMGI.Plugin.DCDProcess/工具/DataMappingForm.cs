using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using System.IO;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    public partial class DataMappingForm : Form
    {
        public class DataMapingRule
        {
            /// <summary>
            /// 要素类别名：如水系、交通等
            /// </summary>
            public string Category
            {
                set;
                get;
            }

            /// <summary>
            /// 源要素类名
            /// </summary>
            public string SourceFCName
            {
                set;
                get;
            }

            /// <summary>
            /// 过滤条件
            /// </summary>
            public string SQLFilter
            {
                set;
                get;
            }

            /// <summary>
            /// 目标要素类名
            /// </summary>
            public string ObjFCName
            {
                set;
                get;
            }

            /// <summary>
            /// 目标要素类字段名与源要素类字段名的对应关系
            /// </summary>
            public Dictionary<string, string> ObjFN2SourceFN
            {
                set;
                get;
            }
            /// <summary>
            /// 目标要素字段名-目标值的对应关系（第三组值）
            /// </summary>
            public Dictionary<string, string> objFN2Value
            {
                set;
                get;
            }
        }

        public string SourceDataBase
        {
            get
            {
                return tbSourceDB.Text;
            }
        }

        public string TemplateDataBase
        {
            get
            {
                return tbTemplateDB.Text;
            }
        }


        public List<DataMapingRule> DataMappingRuleList
        {
            get;
            private set;
        }

        public string OutputDataBase
        {
            get
            {
                return tbOutDB.Text;
            }
        }

        public DataMappingForm()
        {
            InitializeComponent();
        }

        private void DataMappingForm_Load(object sender, EventArgs e)
        {
            string templatePath = GApplication.Application.Template.Root + @"\多尺度数据建库模板.gdb";
            if (Directory.Exists(templatePath))
            {
                if (GApplication.GDBFactory.IsWorkspace(templatePath))
                {
                    tbTemplateDB.Text = templatePath;
                }
            }

            string mdbPath = GApplication.Application.Template.Root + "\\规则配置.mdb";
            if (!System.IO.File.Exists(mdbPath))
            {
                MessageBox.Show(string.Format("未找到配置文件:{0}!", mdbPath));
                return;
            }
            List<string> ruleTableNameList = new List<string>();
            string ruleTableNamePrefix = "数据映射";
            IWorkspaceFactory wsFactory = new AccessWorkspaceFactoryClass();
            IWorkspace ws = wsFactory.OpenFromFile(mdbPath, 0);
            IEnumDataset enumDataset = ws.get_Datasets(esriDatasetType.esriDTAny);
            enumDataset.Reset();
            IDataset dataset = null;
            while ((dataset = enumDataset.Next()) != null)
            {
                if (dataset.Name.StartsWith(ruleTableNamePrefix))
                {
                    ruleTableNameList.Add(dataset.Name);
                }
            }
            Marshal.ReleaseComObject(enumDataset);
            Marshal.ReleaseComObject(ws);
            Marshal.ReleaseComObject(wsFactory);
            if (ruleTableNameList.Count == 0)
            {
                MessageBox.Show(string.Format("配置文件【{0}】中未发现任何数据映射规则表!", mdbPath));
                return;
            }

            foreach (var item in ruleTableNameList)
            {
                cmbMappingRuleTable.Items.Add(item);
            }
        }

        private void btnSource_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择GDB数据库文件夹";
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!GApplication.GDBFactory.IsWorkspace(fbd.SelectedPath))
                {
                    MessageBox.Show("不是有效地GDB文件");
                    return;
                }

                tbSourceDB.Text = fbd.SelectedPath;
            }
        }

        private void btnTemplate_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择GDB数据库文件夹";
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!GApplication.GDBFactory.IsWorkspace(fbd.SelectedPath))
                {
                    MessageBox.Show("不是有效地GDB文件");
                    return;
                }

                tbTemplateDB.Text = fbd.SelectedPath;
            }
        }

        private void btnOutput_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog { FileName = tbOutDB.Text, Filter = @"GDB数据库|*.gdb" };
            sfd.Title = "导出的数据库路径";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                tbOutDB.Text = sfd.FileName;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (tbSourceDB.Text == "")
            {
                MessageBox.Show("未指定源数据库！");
                return;
            }

            if (tbTemplateDB.Text == "")
            {
                MessageBox.Show("未指定标准结构模板数据库！");
                return;
            }

            if (tbOutDB.Text == "")
            {
                MessageBox.Show("未指定输出数据库！");
                return;
            }

            if (cmbMappingRuleTable.Text == "")
            {
                MessageBox.Show("未指定数据映射规则表！");
                return;
            }
            string mdbPath = GApplication.Application.Template.Root + "\\规则配置.mdb";
            DataTable dataTable = DCDHelper.ReadToDataTable(mdbPath, cmbMappingRuleTable.Text);
            if (dataTable == null)
            {
                return;
            }
            DataMappingRuleList = new List<DataMapingRule>();
            foreach (DataRow dr in dataTable.Rows)
            {
                DataMapingRule rule = new DataMapingRule();
                rule.Category = dr["类别名称"].ToString().Trim();
                rule.SourceFCName = dr["源要素类名"].ToString().Trim();
                rule.SQLFilter = dr["过滤条件"].ToString().Trim();
                rule.ObjFCName = dr["目标要素类名"].ToString().Trim();
                rule.ObjFN2SourceFN = new Dictionary<string, string>();
                rule.objFN2Value = new Dictionary<string, string>(); //第3组，默认值

                string fnMappingList = dr["字段名映射列表"].ToString().Trim();
                if (fnMappingList.Count() > 0)
                {
                    string[] fnMappingArray = fnMappingList.Split('、');
                    foreach (var fnMapping in fnMappingArray)
                    {
                        string[] fnArray = fnMapping.Split('|');
                        if (fnArray.Count() < 2)
                            continue;

                        if (fnArray.Count() >= 2)
                        {
                            string sourceFN = fnArray[0].Trim().ToUpper();
                            string objFN = fnArray[1].Trim().ToUpper();
                            rule.ObjFN2SourceFN[objFN] = sourceFN;
                        }

                        if (fnArray.Count() == 3)  //第3组值，默认值，可指定更改
                        {
                            string sourceFN = fnArray[0].Trim().ToUpper();
                            string objFN = fnArray[1].Trim().ToUpper();
                            string value = fnArray[2].Trim();
                            rule.objFN2Value[objFN] = value;
                        }
                    }
                }

                DataMappingRuleList.Add(rule);
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;

        }
    }
}
