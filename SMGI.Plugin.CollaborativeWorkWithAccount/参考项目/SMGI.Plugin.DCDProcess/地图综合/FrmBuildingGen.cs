using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.CartographyTools;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;
using System.Threading;
using SMGI.Common;

namespace SMGI.Plugin.DCDProcess
{
    public partial class FrmBuildingGen : Form
    {
        private string mSql = "GB is not null'";
        private GApplication mApplication = null;
        private string mField = "GB";
        private string mFclName = "RESA";
        /// <summary>
        /// 外部调用脚本文件
        /// </summary>
        public string GenXml = string.Empty;
        private string mGDBPath = string.Empty;
       

        public FrmBuildingGen()
        {
            InitializeComponent();
            mApplication = GApplication.Application;
            btProcess.Enabled = false;
        }
        public FrmBuildingGen(string sql)
        {
            InitializeComponent();
            mApplication = GApplication.Application;
            this.txtSQL.Text = sql;
            btProcess.Enabled = false;
        }
        private void btlastprj_Click(object sender, EventArgs e)
        {
            txtGdbPath.Text = mApplication.AppConfig["LastGDBPath"].ToString();
            txtScale.Text = mApplication.AppConfig["MapScale"].ToString();
           
        }

        private void btSelectGdb_Click(object sender, EventArgs e)
        {
            IWorkspace ws = TBGenHelper.AddTempDataBase();
            if (ws != null)
            {
                if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, mFclName))
                {
                    txtGdbPath.Text = ws.PathName;        
                }
                else
                {
                    MessageBox.Show("数据库中不存在图层：" + mFclName);
                    return;
                }

            }
        }

        private void txt_scale_TextChanged(object sender, EventArgs e)
        {
            btProcess.Enabled = false;
            int scale;
            if (!System.IO.Directory.Exists(txtGdbPath.Text) || !txtGdbPath.Text.Contains(".gdb") ||  !int.TryParse(txtScale.Text, out scale)) 
                return;
            mApplication.AppConfig["LastGDBPath"] = txtGdbPath.Text;
            mApplication.AppConfig["MapScale"] = Convert.ToInt32(txtScale.Text);
            btProcess.Enabled = true;
        }
        public string  SetXML(double scale,string gdbpath)
        {
            XDocument doc = new XDocument();
            
            XElement root = new XElement("Arg");
            root.SetAttributeValue("JopName", "房屋综合");
            XElement cmd = new XElement("Command");

            cmd = new XElement("TBBuilding");
            cmd.Add(new XElement("GDB", gdbpath));//
            cmd.Add(new XElement("Scale", scale.ToString()));
            cmd.Add(new XElement("FclName", mFclName));
            cmd.Add(new XElement("AggregateDis", tbDis.Text));
            cmd.Add(new XElement("Conflict", cbConflict.Checked));
            cmd.Add(new XElement("Field", mField));
            cmd.Add(new XElement("HoleArea", tbHole.Text));
            cmd.Add(new XElement("MinArea", tbArea.Text));
            cmd.Add(new XElement("MinAreaRESA", tbArea2.Text));
            cmd.Add(new XElement("Orthogonality", cbOrthogonal.Checked));
            cmd.Add(new XElement("SimplifyDis", tbTolerance.Text));
            cmd.Add(new XElement("SQL", this.txtSQL.Text));

            root.Add(cmd);
            doc.Add(root);
            string tempxml = GApplication.ExePath + "\\autoTBRESA.xml";
            if (File.Exists(tempxml))
            {
                File.Delete(tempxml);
            }
            doc.Save(tempxml);
            return tempxml;
        }
        private void btProcess_Click(object sender, EventArgs e)
        {

            try
            {
                mGDBPath = txtGdbPath.Text;
                int scale;
                if (!System.IO.Directory.Exists(txtGdbPath.Text) || !txtGdbPath.Text.Contains(".gdb") || !int.TryParse(txtScale.Text, out scale))
                {
                    MessageBox.Show("请设置相关参数！" );
                    return;
                }
                IWorkspaceFactory wsf = new FileGDBWorkspaceFactoryClass();
                var ws = (IFeatureWorkspace)wsf.OpenFromFile(mGDBPath, 0);
                if (ws != null)
                {
                    if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, mFclName))
                    {
                         
                    }
                    else
                    {
                        MessageBox.Show("数据库中不存在图层：" + mFclName);
                        return;
                    }

                }
                XDocument doc = new XDocument();
                
                XElement root = new XElement("Arg");
                root.SetAttributeValue("JopName", "房屋综合");
                XElement cmd = new XElement("Command");
             
                cmd = new XElement("TBBuilding");
                cmd.Add(new XElement("GDB", txtGdbPath.Text));//
                cmd.Add(new XElement("Scale", Convert.ToDouble(txtScale.Text)));
                cmd.Add(new XElement("FclName", mFclName));
                cmd.Add(new XElement("AggregateDis", tbDis.Text));
                cmd.Add(new XElement("Conflict", cbConflict.Checked));
                cmd.Add(new XElement("Field", mField));
                cmd.Add(new XElement("HoleArea", tbHole.Text));
                cmd.Add(new XElement("MinArea", tbArea.Text));
                cmd.Add(new XElement("MinAreaRESA", tbArea2.Text));
                cmd.Add(new XElement("Orthogonality", cbOrthogonal.Checked));
                cmd.Add(new XElement("SimplifyDis", tbTolerance.Text));
                cmd.Add(new XElement("SQL", this.txtSQL.Text));

                root.Add(cmd);
                doc.Add(root);
                string tempxml = GApplication.ExePath + "\\autoTBRESA.xml";
                if (File.Exists(tempxml))
                {
                    File.Delete(tempxml);
                }
                doc.Save(tempxml);
                GenXml = tempxml;
            }
            catch(Exception ex)
            {
                MessageBox.Show("保存处理脚本错误：" + ex.Message);
                return;
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
           
             
        }
      
         

        private void FrmLCAHouseProcess_Load(object sender, EventArgs e)
        {


        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void txtSQL_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
