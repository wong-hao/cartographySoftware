using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using System.Data;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using System.Runtime.InteropServices;
using System.IO;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using System.Xml.Linq;
using System.Diagnostics;
using System.Threading;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geoprocessing;


namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 探测线要素间的制图（线宽）冲突，返回有冲突的地方
    /// 详见需求文档《【20181225回复】追加需求（质检工具等）_董先敏.docx》
    /// 支持独立进程运行-20221011
    /// 是否再改进为后台GP运行-未改
    /// </summary>
    /// 
    [SMGIAutomaticCommand]
    public class CheckGraphicConflictCmd : SMGI.Common.SMGICommand
    {
        public CheckGraphicConflictCmd()
        {
            m_caption = "图形冲突检查";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null;
            }
        }

        public override void OnClick()
        {
            if (m_Application.MapControl.Map.ReferenceScale == 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;
            }

            double referScale = m_Application.MapControl.Map.ReferenceScale;
            string tabaleName = "要素制图冲突检查_";
            if (referScale >= 50000)
            {
                if (referScale == 1000000)
                {
                    tabaleName += "100W";
                }
                else if (referScale == 500000)
                {
                    tabaleName += "50W";
                }
                else if (referScale == 250000)
                {
                    tabaleName += "25W";
                }
                else if (referScale == 100000)
                {
                    tabaleName += "10W";
                }
                else if (referScale == 50000)
                {
                    tabaleName += "5W";
                }
                else
                {
                    int s = (int)referScale / 10000;
                    tabaleName += s.ToString() + "W";
                }
            }
            else
            {
                if (referScale == 25000)
                {
                    tabaleName += "25K";
                }
                else if (referScale == 10000)
                {
                    tabaleName += "10K";
                }
                else
                {
                    int s = (int)referScale / 1000;
                    tabaleName += s.ToString() + "K";
                }
            }

            string mdbPath = m_Application.Template.Root + "\\质检\\质检内容配置.mdb";
            if (!System.IO.File.Exists(mdbPath))
            {
                MessageBox.Show(string.Format("未找到配置文件:{0}!", mdbPath));
                return;
            }
            DataTable dataTable = DCDHelper.ReadMDBTable(mdbPath, tabaleName);
            if (dataTable == null)
            {
                MessageBox.Show(string.Format("配置文件【{0}】中未找到表【{1}】！", mdbPath, tabaleName));
                return;
            }
            if (dataTable.Rows.Count == 0)
            {
                MessageBox.Show(string.Format("规则表中的规则为空，没有有效的检查规则！"));
                return;
            }

            CheckGraphicConflictForm frm = new CheckGraphicConflictForm();
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string outputFileName = OutputSetup.GetDir() + string.Format("\\图形冲突检查_{0}_{1}.shp", m_Application.MapControl.Map.ReferenceScale, DateTime.Now.ToString("yyMMdd_HHmmss"));

            if (frm.EnableShell)//shell
            {
                #region 调用独立进程shell
                XDocument doc = new XDocument();
                XElement root = new XElement("Arg");
                root.Add(new XElement("Product", m_Application.Template.ClassName));
                root.Add(new XElement("Template", m_Application.Template.Caption));

                //命令组
                XElement cmds = new XElement("Commands");
                XElement cmd = null;
                cmd = new XElement("SMGI.Plugin.DCDProcess.CheckGraphicConflictCmd");//
                cmd.Add(new XElement("GDBPath", m_Application.Workspace.EsriWorkspace.PathName));
                cmd.Add(new XElement("ReferenceScale", referScale));
                cmd.Add(new XElement("RuleDBPath", mdbPath));
                cmd.Add(new XElement("RuleTableName", tabaleName));
                cmd.Add(new XElement("MinDistance", frm.GraphicMinDistance));
                cmd.Add(new XElement("resultShpFile", outputFileName));
                cmds.Add(cmd);

                //添加
                root.Add(cmds);
                doc.Add(root);

                //物理存储XML
                string dir = System.IO.Path.GetDirectoryName(m_Application.Workspace.EsriWorkspace.PathName);
                string name = System.IO.Path.GetFileNameWithoutExtension(m_Application.Workspace.EsriWorkspace.PathName);
                string tempxml = dir + "\\" + name + ".xml";
                if (File.Exists(tempxml))
                {
                    File.Delete(tempxml);
                }
                doc.Save(tempxml);
                int pid = -1;
                using (System.Diagnostics.Process p = new System.Diagnostics.Process())
                {
                    ProcessStartInfo si = new ProcessStartInfo();
                    si.FileName = GApplication.ExePath + "\\" + "SMGI.Shell.exe";
                    si.Arguments = string.Format("\"{0}\"", doc.ToString());
                    si.UseShellExecute = true;
                    si.CreateNoWindow = false;
                    p.StartInfo = si;
                    p.Start();
                    pid = p.Id;
                }
                WaitForTimerCheck(pid);
                #endregion


                try
                {
                    XDocument document = XDocument.Load(tempxml);
                    var allElements = document.Element("Arg").Element("Commands").Elements();
                    foreach (var oneElement in allElements)
                    {
                        if (oneElement.Name.LocalName == "savePath")
                        {
                            var savePath = oneElement.Value;
                            if (File.Exists(savePath))
                            {
                                IFeatureClass errFC = CheckHelper.OpenSHPFile(savePath);
                                if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                    CheckHelper.AddTempLayerToMap(GApplication.Application.Workspace.LayerManager.Map, errFC);
                            }
                            else
                            {
                                MessageBox.Show("检查完毕,未发现图形冲突！");
                            }

                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                string err = "";
                using (var wo = m_Application.SetBusy())
                {
                    CheckGraphicConflict checker = new CheckGraphicConflict();
                    err = checker.DoCheck(m_Application.Workspace.EsriWorkspace.PathName, outputFileName, mdbPath, tabaleName, referScale, frm.GraphicMinDistance);
                }

                if (err == "")
                {
                    if (File.Exists(outputFileName))
                    {
                        IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName);

                        if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                    }
                    else
                    {
                        MessageBox.Show("检查完毕,未发现图形冲突！");
                    }
                }
                else
                {
                    MessageBox.Show(err);
                }
            }
        }


        private void WaitForTimerCheck(int id)//等待计时器检查
        {
            try
            {
                while (true)
                {
                    using (var p = System.Diagnostics.Process.GetProcessById(id))
                    {
                        Thread.Sleep(200);
                    }
                }
            }
            catch
            {
                return;
            }
        }


        protected override bool DoCommand(XElement args, Action<string> messageRaisedAction)
        {
            bool res = false;

            try
            {
                messageRaisedAction("正在解析参数......");
                string gdbpath = args.Element("GDBPath").Value.ToString();
                double referScale = double.Parse(args.Element("ReferenceScale").Value.Trim());
                string mdbPath = args.Element("RuleDBPath").Value.ToString();
                string tabaleName = args.Element("RuleTableName").Value.ToString();
                double graphicMinDistance = double.Parse(args.Element("MinDistance").Value.Trim());
                string outputFileName = args.Element("resultShpFile").Value.ToString();


                messageRaisedAction("正在执行检查......");
                CheckGraphicConflict checker = new CheckGraphicConflict();
                string err = checker.DoCheck(gdbpath, outputFileName, mdbPath, tabaleName, referScale, graphicMinDistance);
                if (err == "")
                {
                    string xmlPath = gdbpath.Replace(".gdb", ".xml");
                    XDocument document = XDocument.Load(xmlPath);
                    var allElements = document.Element("Arg").Element("Commands");
                    allElements.Add(new XElement("savePath", outputFileName));
                    document.Save(xmlPath);

                    res = true;
                }
                else
                {
                    MessageBox.Show(err);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                return false;
            }

            return res;
        }

    }
}
