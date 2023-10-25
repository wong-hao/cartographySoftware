using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using System.Xml;
using System.Xml.Linq;

namespace SMGI.Plugin.DCDProcess
{
    public partial class AttributeBrushToolForm : Form
    {
        /// <summary>
        /// 选择要素的属性信息
        /// </summary>
        public FeatureStruct SelFeStruct
        {
            get;
            private set;
        }

        /// <summary>
        /// 被选择的属性字段
        /// </summary>
        public List<string> SelFNList
        {
            get
            {
                var selFNList = new List<string>();
                foreach (var item in chkFieldList.CheckedItems)
                {
                    selFNList.Add(item.ToString().ToUpper());
                }

                return selFNList;
            }
        }

        private bool _bSelAllFN;//默认字段全选与否
        private List<string> _exlucdeFNList;//默认字段全选为真时，排除的属性字段名称列表
        List<string> _defalutSelFNList;//默认字段全选为假时，默认选择的字段名称列表


        public delegate void FrmClosedEventHandler();
        public event FrmClosedEventHandler frmClosed = null;

        public AttributeBrushToolForm(IFeature selFe)
        {
            InitializeComponent();

            TopMost = true;//总是最上面
            _bSelAllFN = true;
            _exlucdeFNList = new List<string>();
            _defalutSelFNList = new List<string>();                     
            
            #region 读取属性刷配置表 
            string cfgFileName = GApplication.Application.Template.Root + "\\AttributeBrushConfig.xml";
            Dictionary<string, List<string>> fc_fieldsD = LoadAttributeBrushXML(cfgFileName);  
            #endregion

            #region 收集被选要素的属性值信息
            SelFeStruct = new FeatureStruct();
            SelFeStruct.FCName = selFe.Class.AliasName;
            for (int i = 0; i < selFe.Fields.FieldCount; i++)
            {
                IField field = selFe.Fields.get_Field(i);

                if (!field.Editable)
                    continue;

                if (field.Type == esriFieldType.esriFieldTypeOID || field.Type == esriFieldType.esriFieldTypeGeometry)
                    continue;//排除oid和Geometry

                if (cmdUpdateRecord.CollabGUID == field.Name.ToUpper() || cmdUpdateRecord.CollabVERSION == field.Name.ToUpper() || cmdUpdateRecord.CollabDELSTATE == field.Name.ToUpper() || cmdUpdateRecord.CollabOPUSER == field.Name.ToUpper())
                    continue;//协同字段排除

                SelFeStruct.FieldInfo.Add(field.Name, field.Type);
                SelFeStruct.FieldValue.Add(field.Name, selFe.get_Value(i));
            }
            #endregion
            
            #region 更新界面
            label2.Text = String.Format("要素类名称:{0}", SelFeStruct.FCName);
            chkFieldList.Items.Clear();
            foreach (var kv in SelFeStruct.FieldInfo)
            {
                if (fc_fieldsD.ContainsKey(SelFeStruct.FCName))
                {
                    if (fc_fieldsD[SelFeStruct.FCName].Contains(kv.Key.ToUpper()))
                    {
                        chkFieldList.Items.Add(kv.Key, true);
                    }
                    else
                    {
                        chkFieldList.Items.Add(kv.Key, false);
                    }
                }
                else
                {
                    chkFieldList.Items.Add(kv.Key, false); 
                }
            }            
            #endregion


        }

        private void btnOptionSet_Click(object sender, EventArgs e)
        {
            List<string> DoFields =new List<string>();
            foreach (var cBox in chkFieldList.CheckedItems)
            {
                DoFields.Add(cBox.ToString().ToUpper());
            }
            #region 将配置写入到配置文件中
            string cfgFileName = GApplication.Application.Template.Root + "\\AttributeBrushConfig.xml";
            if (System.IO.File.Exists(cfgFileName))
            {
                try
                {
                    XDocument xmlDoc = XDocument.Load(cfgFileName);
                    var AttributeBrushItem = xmlDoc.Element("Option").Element("AttributeBrush");

                    var fcItems = AttributeBrushItem.Element("FCS").Elements("FC");
                    bool fcNameExist = false;
                     foreach (XElement fcItem in fcItems)
                     {
                         string fcName = fcItem.Element("NAME").Value;
                         
                         if (fcName != SelFeStruct.FCName)
                             continue;
                         else
                         {
                             var fieldsItems = fcItem.Element("Fields");
                             fieldsItems.RemoveNodes();
                             foreach (string fieldName in DoFields)
                             {
                                 fieldsItems.Add(new XElement("FN", fieldName));
                             }
                             fcNameExist = true;
                             break;
                         }                         
                     }
                     if (!fcNameExist)
                     {
                         XElement xeFC = new XElement("FC");
                         XElement xeNAME = new XElement("NAME", SelFeStruct.FCName);
                         xeFC.Add(xeNAME);
                         XElement xeFields = new XElement("Fields");
                         foreach (string fieldName in DoFields)
                         {
                             xeFields.Add(new XElement("FN", fieldName)); 
                         }
                         xeFC.Add(xeFields);                         
                         AttributeBrushItem.Element("FCS").Add(xeFC);                         
                     }
                     xmlDoc.Save(cfgFileName);
                }
                catch
                {
                }
            }
            #endregion
            
        }

        //窗口关闭后引发委托事件
        private void AttributeBrushToolForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (frmClosed != null)
            {
                frmClosed();
                SMGI.Plugin.DCDProcess.DataBindings.abtool2.FrmLocation = this.Location;
            }
        }

        private Dictionary<string, List<string>> LoadAttributeBrushXML(string cfgFileName)
        {
            List<string> ExlucdeFields = new List<string>();
            Dictionary<string, List<string>> fc_fieldsD = new Dictionary<string, List<string>>();
            #region 读取属性刷配置表            
            if (System.IO.File.Exists(cfgFileName))
            {
                try
                {
                    XDocument xmlDoc = XDocument.Load(cfgFileName);
                    var AttributeBrushItem = xmlDoc.Element("Option").Element("AttributeBrush");
                    if (AttributeBrushItem != null)
                    {
                        var exlucdeFieldItems = AttributeBrushItem.Element("ExlucdeFields").Elements("FN");
                        foreach (XElement exlucdeFieldItem in exlucdeFieldItems)
                        {
                            string fieldName = exlucdeFieldItem.Value.ToUpper();
                            ExlucdeFields.Add(fieldName);
                        }

                        var fcItems = AttributeBrushItem.Elements("FCS").Elements("FC");
                        foreach (XElement fcItem in fcItems)
                        {
                            string fcName = fcItem.Element("NAME").Value;
                            var fieldNameItems = fcItem.Element("Fields").Elements("FN");
                            foreach (XElement fieldNameItem in fieldNameItems)
                            {
                                string fieldName = fieldNameItem.Value.ToUpper();
                                if (fc_fieldsD.ContainsKey(fcName))
                                {
                                    fc_fieldsD[fcName].Add(fieldName);
                                }
                                else
                                {
                                    fc_fieldsD.Add(fcName, new List<string> { fieldName }); 
                                }
                            }
                        }                        
                    }
                    btnOptionSet.Enabled = true;
                }
                catch (Exception ex)
                {
                    btnOptionSet.Enabled = false;
                }
            }
            #endregion 
            return fc_fieldsD;
        }

        private void AttributeBrushToolForm2_Load(object sender, EventArgs e)
        {
            this.DataBindings.Add("Location", SMGI.Plugin.DCDProcess.DataBindings.abtool2, "FrmLocation", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        

    }

    /// <summary>
    /// 要素集的数据结构
    /// </summary>
    public class FeatureStruct
    {
        /// <summary>
        /// 要素名称
        /// </summary>
        public string FCName;
        /// <summary>
        /// 字段信息
        /// </summary>
        public Dictionary<string, esriFieldType> FieldInfo = new Dictionary<string, esriFieldType>();
        /// <summary>
        /// 字段值
        /// </summary>
        public Dictionary<string, object> FieldValue = new Dictionary<string, object>();
    }
}
