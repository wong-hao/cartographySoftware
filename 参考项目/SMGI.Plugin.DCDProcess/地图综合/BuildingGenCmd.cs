using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using System.Drawing;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Xml.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Data;
using ESRI.ArcGIS.esriSystem;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SMGI.Plugin.DCDProcess
{
    public class BuildingGenCmd : SMGICommand
    {
        public BuildingGenCmd()
        {
            m_caption = "居民地综合";
            m_toolTip = "居民地综合";
            m_category = "居民地综合";

        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing;
            }
        }

      
       
        public override void OnClick()
        {
            FrmBuildingGen frm = new FrmBuildingGen();
            if (DialogResult.OK != frm.ShowDialog())
            {
                return;
            }
            
            try
            {
               
                Excute(frm.GenXml);
                 
            }
            catch
            {
            }
            finally
            {
                MessageBox.Show("处理完成!");
            }
       
        }
        public static void Excute(string tempXml)
        {
            #region
            XDocument doc = XDocument.Load(tempXml);
            XElement cmd = null;
            var root = doc.Element("Arg");
            cmd = root.Elements().FirstOrDefault();
            string gdbPath = cmd.Element("GDB").Value;
            string fclName = cmd.Element("FclName").Value;
            string field = cmd.Element("Field").Value;
            string exePath =TBGenHelper.GenExePath;
            #endregion
            

            try
            {
                using (var wo = GApplication.Application.SetBusy())
                {
                    wo.SetText("正在房屋综合..");

                    //Process p = null;
                    //ProcessStartInfo si = null;
                    int mPid = TBGenHelper.ExcuteGenShell(tempXml);


                    TBGenHelper.WaitForTimerCheck(mPid);
                }
            }
            catch
            {
            }

            try
            {
                using (var wo = GApplication.Application.SetBusy())
                {
                    #region
                    wo.SetText("正在处理..");
                    System.Diagnostics.Process p = null;
                    ProcessStartInfo si = null;

                    doc = new XDocument();

                    root = new XElement("Arg");
                    root.SetAttributeValue("JopName", "房屋综合");
                    cmd = new XElement("TBBuildingAfter");
                    cmd.Add(new XElement("GDB", gdbPath));//
                    cmd.Add(new XElement("Field", field));//
                    cmd.Add(new XElement("FclName", fclName));
                    root.Add(cmd);
                    doc.Add(root);
                    string tempxml = GApplication.ExePath + "\\TBBuilding1.xml";
                    if (File.Exists(tempxml))
                    {
                        File.Delete(tempxml);
                    }
                    doc.Save(tempxml);
                    int pid = -1;
                    using (p = new System.Diagnostics.Process())
                    {
                        si = new ProcessStartInfo();
                        si.FileName = exePath;
                        si.Arguments = string.Format("\"{0}\"", tempxml);

                        si.UseShellExecute = true;
                        si.CreateNoWindow = false;
                        p.StartInfo = si;
                        p.Start();
                        pid = p.Id;
                    }
                    TBGenHelper.WaitForTimerCheck(pid);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("综合错误:" + ex.Message);
            }
        }
       

    }
}
