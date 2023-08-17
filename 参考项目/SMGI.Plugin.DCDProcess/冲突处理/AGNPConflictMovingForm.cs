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
using System.Xml.Linq;

namespace SMGI.Plugin.DCDProcess
{
    public partial class AGNPConflictMovingForm : Form
    {
        public Dictionary<string, Dictionary<double, string>> PtFCName2SizeAndSql
        {
            private set;
            get;
        }

        public Dictionary<string, Dictionary<double, string>> LineFCName2WidthAndSql
        {
            private set;
            get;
        }

        public double MinSpacing
        {
            private set;
            get;
        }


        private GApplication _app;
        private XDocument _ruleDoc;

        public AGNPConflictMovingForm(GApplication app, XDocument ruleDoc)
        {
            InitializeComponent();

            _app = app;
            _ruleDoc = ruleDoc;

            var contentItem = _ruleDoc.Element("POIMovingRule");
            if (contentItem != null)
            {
                clbAGNPClass.ValueMember = "Key";
                clbAGNPClass.DisplayMember = "Value";

                var ptItem = contentItem.Element("PointLayer");
                foreach (XElement ele in ptItem.Elements("Item"))
                {
                    string classFCName = ele.Attribute("calssname").Value.ToString();
                    string sql = ele.Attribute("sql").Value.ToString();
                    double size = double.Parse(ele.Attribute("size").Value.ToString());
                    bool ck = bool.Parse(ele.Attribute("checked").Value);

                    KeyValuePair<string, KeyValuePair<string, double>> key = new KeyValuePair<string, KeyValuePair<string, double>>(classFCName, new KeyValuePair<string, double>(sql, size));


                    clbAGNPClass.Items.Add(new KeyValuePair<KeyValuePair<string, KeyValuePair<string, double>>, string>(key, ele.Value), ck);
                }


                clbLRDLGB.ValueMember = "Key";
                clbLRDLGB.DisplayMember = "Value";

                var lineItem = contentItem.Element("LineLayer");
                foreach (XElement ele in lineItem.Elements("Item"))
                {
                    string classFCName = ele.Attribute("calssname").Value.ToString();
                    string sql = ele.Attribute("sql").Value.ToString();
                    double width = double.Parse(ele.Attribute("width").Value.ToString());
                    bool ck = bool.Parse(ele.Attribute("checked").Value);

                    KeyValuePair<string, KeyValuePair<string, double>> key = new KeyValuePair<string, KeyValuePair<string, double>>(classFCName, new KeyValuePair<string, double>(sql, width));

                    clbLRDLGB.Items.Add(new KeyValuePair<KeyValuePair<string, KeyValuePair<string, double>>, string>(key, ele.Value), ck);
                }
            }
        }

        public AGNPConflictMovingForm(GApplication app, XDocument ruleDoc,Dictionary<string, Dictionary<double, string>> ptFCName2SizeAndSql,Dictionary<string, Dictionary<double, string>> lineFCName2WidthAndSql,double minSpace)
        {
            InitializeComponent();

            _app = app;
            _ruleDoc = ruleDoc;

            var contentItem = _ruleDoc.Element("POIMovingRule");
            if (contentItem != null)
            {
                clbAGNPClass.ValueMember = "Key";
                clbAGNPClass.DisplayMember = "Value";

                var ptItem = contentItem.Element("PointLayer");
                foreach (XElement ele in ptItem.Elements("Item"))
                {
                    string classFCName = ele.Attribute("calssname").Value.ToString();
                    string sql = ele.Attribute("sql").Value.ToString();
                    double size = double.Parse(ele.Attribute("size").Value.ToString());
                    bool ck = false;
                    if (ptFCName2SizeAndSql.ContainsKey(classFCName.ToUpper().Trim()) && ptFCName2SizeAndSql[classFCName.ToUpper().Trim()].ContainsKey(size) && ptFCName2SizeAndSql[classFCName.ToUpper().Trim()][size].Contains(sql))
                    {
                        ck = true;
                    }

                    KeyValuePair<string, KeyValuePair<string, double>> key = new KeyValuePair<string, KeyValuePair<string, double>>(classFCName, new KeyValuePair<string, double>(sql, size));


                    clbAGNPClass.Items.Add(new KeyValuePair<KeyValuePair<string, KeyValuePair<string, double>>, string>(key, ele.Value), ck);
                }


                clbLRDLGB.ValueMember = "Key";
                clbLRDLGB.DisplayMember = "Value";

                var lineItem = contentItem.Element("LineLayer");
                foreach (XElement ele in lineItem.Elements("Item"))
                {
                    string classFCName = ele.Attribute("calssname").Value.ToString();
                    string sql = ele.Attribute("sql").Value.ToString();
                    double width = double.Parse(ele.Attribute("width").Value.ToString());
                    bool ck = false;
                    if (lineFCName2WidthAndSql.ContainsKey(classFCName.ToUpper().Trim()) && lineFCName2WidthAndSql[classFCName.ToUpper().Trim()].ContainsKey(width) && lineFCName2WidthAndSql[classFCName.ToUpper().Trim()][width].Contains(sql))
                    {
                        ck = true;
                    }

                    KeyValuePair<string, KeyValuePair<string, double>> key = new KeyValuePair<string, KeyValuePair<string, double>>(classFCName, new KeyValuePair<string, double>(sql, width));

                    clbLRDLGB.Items.Add(new KeyValuePair<KeyValuePair<string, KeyValuePair<string, double>>, string>(key, ele.Value), ck);
                }
            }

            minSapceEdit.Value = (decimal)minSpace;
        }


        private void btOK_Click(object sender, EventArgs e)
        {
            PtFCName2SizeAndSql = new Dictionary<string,Dictionary<double,string>>();
            LineFCName2WidthAndSql = new Dictionary<string, Dictionary<double, string>>();

            for (int i = 0; i < clbAGNPClass.Items.Count; ++i)
            {
                if(!clbAGNPClass.GetItemChecked(i))
                    continue;

                var kv = ((KeyValuePair<KeyValuePair<string, KeyValuePair<string, double>>, string>)clbAGNPClass.Items[i]).Key;

                string sql = kv.Value.Key;
                if (sql == "")
                {
                    sql = "1=1";//全部
                }

                if (PtFCName2SizeAndSql.ContainsKey(kv.Key.ToUpper().Trim()))
                {
                    if (PtFCName2SizeAndSql[kv.Key.ToUpper().Trim()].ContainsKey(kv.Value.Value))//点图层中该符号大小已存在一条记录，则合并sql
                    {
                        PtFCName2SizeAndSql[kv.Key.ToUpper().Trim()][kv.Value.Value] += string.Format(" or ({0})", sql);
                    }
                    else//新尺寸
                    {
                        PtFCName2SizeAndSql[kv.Key.ToUpper().Trim()].Add(kv.Value.Value, string.Format("({0})", sql));
                    }
                }
                else
                {
                    Dictionary<double, string> size2sql = new Dictionary<double, string>();
                    size2sql.Add(kv.Value.Value, string.Format("({0})", sql));
                    
                    PtFCName2SizeAndSql.Add(kv.Key.ToUpper().Trim(), size2sql);
                }
            }

            for (int i = 0; i < clbLRDLGB.Items.Count; ++i)
            {
                if (!clbLRDLGB.GetItemChecked(i))
                    continue;

                var kv = ((KeyValuePair<KeyValuePair<string, KeyValuePair<string, double>>, string>)clbLRDLGB.Items[i]).Key;

                string sql = kv.Value.Key;
                if (sql == "")
                {
                    sql = "1=1";//全部
                }

                if (LineFCName2WidthAndSql.ContainsKey(kv.Key.ToUpper().Trim()))
                {
                    if (LineFCName2WidthAndSql[kv.Key.ToUpper().Trim()].ContainsKey(kv.Value.Value))//线图层中该符号宽度已存在一条记录，则合并sql
                    {
                        LineFCName2WidthAndSql[kv.Key.ToUpper().Trim()][kv.Value.Value] += string.Format(" or ({0})", sql);
                    }
                    else//新尺寸
                    {
                        LineFCName2WidthAndSql[kv.Key.ToUpper().Trim()].Add(kv.Value.Value, string.Format("({0})", sql));
                    }
                }
                else
                {
                    Dictionary<double, string> width2sql = new Dictionary<double, string>();
                    width2sql.Add(kv.Value.Value, string.Format("({0})", sql));

                    LineFCName2WidthAndSql.Add(kv.Key.ToUpper().Trim(), width2sql);
                }
            }

            if (PtFCName2SizeAndSql.Count == 0)
            {
                MessageBox.Show("请选择至少一种点类型！");
                return;
            }

            if (LineFCName2WidthAndSql.Count == 0)
            {
                MessageBox.Show("请选择至少一种线类型！");
                return;
            }

            MinSpacing = (double)minSapceEdit.Value;

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
