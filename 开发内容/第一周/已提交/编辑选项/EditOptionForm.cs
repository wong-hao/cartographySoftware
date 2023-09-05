using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using SMGI.Common;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.GeneralEdit
{
    public partial class EditOptionForm : Form
    {
        public int StickyMoveTolerance { get; set; }

        private GApplication _app;
        private IEngineEditProperties2 _editorProp;

        private string autoFilePath = "AutoFill.xml";

        public string tolerance = "";
        
        public EditOptionForm()
        {
            InitializeComponent();

            _app = GApplication.Application;
        }

        private void EditOptionForm_Load(object sender, EventArgs e)
        {
            _editorProp = _app.EngineEditor as IEngineEditProperties2;

            //更新控件
            tbStickyMoveTolerance.Text = _editorProp.StickyMoveTolerance.ToString();

            //重新打开工程后读取自动填充的移动容差
            GetAutoSavedTolerance();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            int tol = 0;
            bool b = int.TryParse(tbStickyMoveTolerance.Text, out tol);
            if (!b || tol < 0)
            {
                MessageBox.Show("粘滞容差输入不合法!");
                return;
            }

            //更新容差
            _editorProp.StickyMoveTolerance = tol;

            tolerance = tbStickyMoveTolerance.Text;

            //保存移动容差以方便下次打开工程自动填充
            SetAutoSavedTolerance();

            DialogResult = DialogResult.OK;
        }

        public void GetAutoSavedTolerance(){
            string cfgFileName = _app.Template.Root + "\\" + autoFilePath;
            if (!System.IO.File.Exists(cfgFileName))
                return;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(cfgFileName);
            XmlNodeList nodes = xmlDoc.SelectNodes("/AutoFill/Field");

            foreach (XmlNode xmlnode in nodes)
            {
                if (xmlnode.NodeType != XmlNodeType.Element)
                    continue;
                string name = (xmlnode as XmlElement).GetAttribute("name");
                string content = (xmlnode as XmlElement).GetAttribute("content");
                if (name == "tolerance") tbStickyMoveTolerance.Text = content;
            }
        }

        public void SetAutoSavedTolerance(){
            string cfgFileName = _app.Template.Root + "\\" + autoFilePath;
            if (System.IO.File.Exists(cfgFileName))
            {
                bool toSave = false;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(cfgFileName);
                XmlNodeList nodes = xmlDoc.SelectNodes("/AutoFill/Field");
                foreach (XmlNode xmlnode in nodes)
                {
                    if (xmlnode.NodeType != XmlNodeType.Element)
                        continue;
                    string name = (xmlnode as XmlElement).GetAttribute("name");
                    if (name == "tolerance") 
                    {
                        if ((xmlnode as XmlElement).GetAttribute("content") != tolerance)
                        {
                            (xmlnode as XmlElement).SetAttribute("content", tolerance);
                            toSave = true;
                        }
                    }
                }
                if (toSave)
                {
                    xmlDoc.Save(cfgFileName);
                }
            }
        }
    }
}
