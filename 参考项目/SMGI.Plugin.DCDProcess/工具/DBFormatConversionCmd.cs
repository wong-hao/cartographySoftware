using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using System.IO;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 批量将指定格式的数据库转换为目标格式的数据库（LZ）
    /// </summary>
    public class DBFormatConversionCmd : SMGI.Common.SMGICommand
    {
        public DBFormatConversionCmd()
        {

            m_category = "DataManagement";
            m_caption = "数据库格式转换";
            m_message = "批量将指定格式的数据库转换为目标格式的数据库";
            m_toolTip = "数据库格式转换";

        }

        public override bool Enabled
        {
            get
            {
                return true;
            }
        }

        public override void OnClick()
        {
            var frm = new DBFormatConversionForm();
            frm.StartPosition = FormStartPosition.CenterParent;

            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string err = "";
            try
            {
               
                using (var wo = m_Application.SetBusy())
                {

                    FormatConversion(frm.SourceDBList, frm.SourceDBFormat, frm.OutDBFormat, frm.OutputPath, wo, ref err);
                }

                if (err == "")
                {
                    MessageBox.Show("数据库转换完成！");
                }
                else
                {
                    string resultInfoFileName = GApplication.AppDataPath + string.Format("\\log\\Result_{0}.txt", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));

                    #region 输出结果日志文件
                    var outpuFS = System.IO.File.Open(resultInfoFileName, System.IO.FileMode.Create);
                    StreamWriter outputSW = new StreamWriter(outpuFS, Encoding.Default);

                    //向日志文件写入内容
                    outputSW.WriteLine(string.Format("转换失败的数据库信息具体如下："));
                    outputSW.WriteLine(string.Format("{0}", err));

                    outputSW.Flush();
                    outpuFS.Close();
                    #endregion

                    MessageBox.Show(string.Format("处理结束，部分数据库转换失败，具体情况请查看结果日志信息！"), "完成", MessageBoxButtons.OK);

                    System.Diagnostics.Process.Start(resultInfoFileName);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                System.Diagnostics.Trace.WriteLine(ex.Source);

                MessageBox.Show(ex.Message);
            }

        }

        private static bool FormatConversion(List<string> sourceDBList, string sourceDBFormat, string outDBFormat, string outputPath, WaitOperation wo, ref string errInfo)
        {
            int numErr = 0;

            Geoprocessor gp = new Geoprocessor();
            gp.OverwriteOutput = true;

            int num = 0;
            IWorkspaceFactory wsFactory = null;
            foreach (var item in sourceDBList)
            {
                if(wo != null)
                    wo.SetText(string.Format("正在处理地理数据库【{0}】，第{1}个/共{2}个......", item, ++num, sourceDBList.Count));

                string xmlExportFile = "";
                try
                {
                    xmlExportFile = "";
                    wsFactory = null;

                    #region 1.数据库有效新判断
                    if (sourceDBFormat == "mdb")
                    {
                        wsFactory = new AccessWorkspaceFactoryClass();
                    }
                    else if (sourceDBFormat == "gdb")
                    {
                        wsFactory = new FileGDBWorkspaceFactoryClass();
                    }
                    if (!wsFactory.IsWorkspace(item))
                    {
                        throw new Exception(string.Format("【{0}】不是有效的文件地理数据库!", item));
                    }
                    #endregion

                    #region 2.导出目标数据库数据的xml文档
                    xmlExportFile = outputPath + "\\workspaceExport.xml";
                    int index = 0;
                    while (File.Exists(xmlExportFile))
                    {
                        xmlExportFile = outputPath + string.Format("\\workspaceExport_{0}.xml", ++index);
                    }
                    ExportXMLWorkspaceDocument export = new ExportXMLWorkspaceDocument();
                    export.in_data = item;
                    export.out_file = xmlExportFile;
                    export.export_type = "DATA";
                    SMGI.Common.Helper.ExecuteGPTool(gp, export, null);
                    #endregion

                    #region 3.新建数据库并导入数据
                    string outDBPathName = "";
                    if (outDBFormat == "gdb")
                    {
                        outDBPathName = string.Format("{0}\\{1}.gdb", outputPath, System.IO.Path.GetFileNameWithoutExtension(item));
                        if (Directory.Exists(outDBPathName))
                        {
                            Directory.Delete(outDBPathName, true);
                        }

                        Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
                        wsFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
                        IWorkspaceName workspaceName = wsFactory.Create(outputPath, System.IO.Path.GetFileName(outDBPathName), null, 0);
                    }
                    else if (outDBFormat == "mdb")
                    {
                        outDBPathName = string.Format("{0}\\{1}.mdb", outputPath, System.IO.Path.GetFileNameWithoutExtension(item));
                        if (File.Exists(outDBPathName))
                        {
                            File.Delete(outDBPathName);
                        }

                        Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.AccessWorkspaceFactory");
                        wsFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
                        IWorkspaceName workspaceName = wsFactory.Create(outputPath, System.IO.Path.GetFileName(outDBPathName), null, 0);
                    }

                    ImportXMLWorkspaceDocument import = new ImportXMLWorkspaceDocument();
                    import.target_geodatabase = outDBPathName;
                    import.in_file = xmlExportFile;
                    import.import_type = "DATA";
                    SMGI.Common.Helper.ExecuteGPTool(gp, import, null);
                    #endregion
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                    System.Diagnostics.Trace.WriteLine(ex.Source);

                    if (errInfo != "")
                        errInfo += "\r\n";

                    errInfo += string.Format("数据库【{0}】转换失败：{1}!", item, ex.Message);

                    numErr++;
                }
                finally
                {
                    if (xmlExportFile != "")
                    {
                        File.Delete(xmlExportFile);
                        xmlExportFile = "";
                    }

                    if (wsFactory != null)
                    {
                        Marshal.ReleaseComObject(wsFactory);
                        wsFactory = null;
                    }
                }


            }//foreach

            return (numErr == 0);
        }

    }
}
