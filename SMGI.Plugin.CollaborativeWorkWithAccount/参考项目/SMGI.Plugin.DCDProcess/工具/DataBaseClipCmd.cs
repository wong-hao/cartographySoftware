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
    /// 该工具用于通过指定的分割面要素类及分割字段将目标地理数据库分割为若干个子数据库：
    /// （1）目标数据库类型：待分割的目标地理数据库类型，支持文件地理数据库、个人地理数据库；
    /// （2）目标数据库：待分割的目标地理数据库；
    /// （3）分割面要素类：用于分割目标数据库的面要素类；
    /// （4）分割字段：按分割字段的属性值进行分组，相同属性值的面要素共同决定子数据库的裁切范围，该子库的命名规则为：目标数据库名_(分割字段属性值)；
    /// （5）保留空要素类：是否保留空要素类，若不勾选，则将删除子数据库中的空要素类；
    /// （6）输出路径：子数据库的存储路径。

    /// 注意：
    /// 1.用于分割目标数据库的面要素类的空间参考建议与目标数据库保持一致；
    /// 2.用于分割目标数据库的面要素类名称不能以数字开头；
    /// 3.分割字段仅支持文本和数字字段。
    /// @LZ：2021
    /// 
    /// @zhx:2022.10.14 将本代码文件中的函数名由DataBaseClipCmd改为了DataBaseClipCmdV0
    ///                 使用DataBaseClipCmd2.cs代码里的DataBaseClipCmd，外部commands.xml可不用修改
    /// </summary>
    public class DataBaseClipCmdV0 : SMGI.Common.SMGICommand
    {
        public DataBaseClipCmdV0()
        {

            m_caption = "数据库分割";
            m_message = "通过指定的分割面要素类将目标地理数据库分割为若干个子数据库";

        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null;
            }
        }

        public override void OnClick()
        {
            var frm = new DataBaseClipForm();
            frm.StartPosition = FormStartPosition.CenterParent;

            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string err = "";
            using (var wo = m_Application.SetBusy())
            {

                err = DatabaseClip(frm.SourceWS, frm.DBDuffix, frm.ClipFC, frm.ClipFN, frm.NeedSaveNullFC, frm.OutputPath, wo);
            }

            if (err == "")
            {
                MessageBox.Show("数据库分割完成！");
            }
            else
            {
                MessageBox.Show(err);
            }

        }

        private static string DatabaseClip(IFeatureWorkspace sourceWS, string dbDuffix, IFeatureClass clipFC, string clipFN, bool needSaveNullFC, string outputPath, WaitOperation wo)
        {
            string err = "";

            ITable temp_table = null;
            string xmlExportFile = outputPath + "\\workspaceExport.xml";
            List<string> delGDBWorkspaceList = new List<string>();
            List<string> delMDBWorkspaceList = new List<string>();
            try
            {
                if(wo != null)
                    wo.SetText("正在创建临时数据库......");

                string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
                IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);
                Geoprocessor gp = new Geoprocessor();
                gp.OverwriteOutput = true;
                gp.SetEnvironmentValue("workspace", ws.PathName);

                if (wo != null)
                    wo.SetText("正在获取分割要素面中指定字段的属性值取值集合......");

                List<object> valueList = new List<object>();
                #region 1.获取裁切要素面中指定字段的属性值取值集合
                ESRI.ArcGIS.AnalysisTools.Frequency freque = new ESRI.ArcGIS.AnalysisTools.Frequency();
                freque.in_table = clipFC;
                freque.out_table = clipFC.AliasName + "_Frequency";
                freque.frequency_fields = clipFN;
                SMGI.Common.Helper.ExecuteGPTool(gp, freque, null);

                temp_table = (ws as IFeatureWorkspace).OpenTable(clipFC.AliasName + "_Frequency");


                ICursor cur = temp_table.Search(null, true);
                IRow row = null;
                while ((row = cur.NextRow()) != null)
                {
                    valueList.Add(row.get_Value(row.Fields.FindField(clipFN)));
                }
                Marshal.ReleaseComObject(cur);
                #endregion

                if (wo != null)
                    wo.SetText("正在导出数据库结构......");

                #region 2.导出目标数据库结构的xml文档
                int index = 0;
                while (File.Exists(xmlExportFile))
                {
                    xmlExportFile = outputPath + string.Format("\\workspaceExport_{0}.xml", ++index);
                }
                ExportXMLWorkspaceDocument export = new ExportXMLWorkspaceDocument();
                export.in_data = (sourceWS as IWorkspace).PathName;
                export.out_file = xmlExportFile;
                export.export_type = "SCHEMA_ONLY";
                SMGI.Common.Helper.ExecuteGPTool(gp, export, null);
                #endregion

                #region 3.裁切数据库
                int fnIndex = clipFC.FindField(clipFN);
                int num = 0;
                IWorkspaceFactory workspaceFactory = null;
                foreach (var item in valueList)
                {
                    string value = item.ToString();
                    if (DBNull.Value == item)
                        value = "<空>";
                    if (wo != null)
                        wo.SetText(string.Format("正在利用分割字段属性值（{0}）裁切数据库，第{1}个/共{2}个......", value, ++num, valueList.Count));

                    IPolygon extentGeo = null;
                    #region 4.1 获取指定属性值的裁切几何
                    string filter = "";
                    if (DBNull.Value == item)
                    {
                        filter = string.Format("{0} is null", clipFN);
                    }
                    else
                    {
                        IField field = clipFC.Fields.get_Field(fnIndex);
                        if (field.Type == esriFieldType.esriFieldTypeString)
                        {
                            filter = string.Format("{0} = '{1}'", clipFN, item.ToString());
                        }
                        else
                        {
                            filter = string.Format("{0} = {1}", clipFN, item.ToString());
                        }
                    }
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = filter;
                    IFeatureCursor clipFeCursor = clipFC.Search(qf, true);
                    IFeature clipFe = null;
                    while ((clipFe = clipFeCursor.NextFeature()) != null)
                    {
                        if (clipFe.Shape == null || clipFe.Shape.IsEmpty)
                            continue;

                        if (extentGeo == null)
                        {
                            extentGeo = clipFe.ShapeCopy as IPolygon;
                        }
                        else
                        {
                            extentGeo = (extentGeo as ITopologicalOperator).Union(clipFe.Shape) as IPolygon;
                        }
                    }
                    Marshal.ReleaseComObject(clipFeCursor);
                    #endregion

                    if (extentGeo == null)
                        continue;

                    IFeatureWorkspace subFWS = null;
                    #region 4.2 复制空数据库
                    string dbNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension((sourceWS as IWorkspace).PathName);
                    string subDBPath = "";
                    if (dbDuffix == "gdb")
                    {
                        subDBPath = string.Format("{0}\\{1}_({2}).gdb", outputPath, dbNameWithoutExtension, value);
                        if (Directory.Exists(subDBPath))
                        {
                            Directory.Delete(subDBPath, true);
                        }

                        Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
                        workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
                        IWorkspaceName workspaceName = workspaceFactory.Create(outputPath, System.IO.Path.GetFileName(subDBPath), null, 0);

                        subFWS = workspaceFactory.OpenFromFile(subDBPath, 0) as IFeatureWorkspace;
                    }
                    else if (dbDuffix == "mdb")
                    {
                        subDBPath = string.Format("{0}\\{1}_({2}).mdb", outputPath, dbNameWithoutExtension, value);
                        if (File.Exists(subDBPath))
                        {
                            File.Delete(subDBPath);
                        }

                        Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.AccessWorkspaceFactory");
                        workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
                        IWorkspaceName workspaceName = workspaceFactory.Create(outputPath, System.IO.Path.GetFileName(subDBPath), null, 0);

                        subFWS = workspaceFactory.OpenFromFile(subDBPath, 0) as IFeatureWorkspace;
                    }

                    ImportXMLWorkspaceDocument import = new ImportXMLWorkspaceDocument();
                    import.target_geodatabase = subDBPath;
                    import.in_file = xmlExportFile;
                    import.import_type = "SCHEMA_ONLY";
                    SMGI.Common.Helper.ExecuteGPTool(gp, import, null);
                    #endregion

                    if (subFWS == null)
                        continue;

                    #region 4.3 依次裁切目标数据库数据至空的子数据库
                    if (!DCDHelper.ClipDataBase(sourceWS, subFWS, extentGeo))
                    {
                        throw new Exception(string.Format("裁切数据库【{0}】时失败！", subDBPath));
                    }
                    #endregion

                    #region 4.4 剔除数据库中的空要素类
                    if (!needSaveNullFC)
                    {
                        bool res = DCDHelper.DeleteNullFeatureClass(subFWS);
                        if (res)
                        {
                            //收集空数据库
                            if (dbDuffix == "gdb")
                            {
                                string wsPathName = (subFWS as IWorkspace).PathName;
                                delGDBWorkspaceList.Add(wsPathName);
                            }
                            else if (dbDuffix == "mdb")
                            {
                                string wsPathName = (subFWS as IWorkspace).PathName;
                                delMDBWorkspaceList.Add(wsPathName);
                            }

                            IWorkspaceFactoryLockControl lockControl = workspaceFactory as IWorkspaceFactoryLockControl;
                            if (lockControl.SchemaLockingEnabled)
                                lockControl.DisableSchemaLocking();//关闭资源锁

                        }
                    }
                    #endregion

                    Marshal.ReleaseComObject(subFWS);
                    Marshal.ReleaseComObject(workspaceFactory);
                }
                #endregion

                foreach (var item in delGDBWorkspaceList)
                {
                    //删除空数据库
                    //Directory.Delete(item, true);//提示另一进程在用
                }
                foreach (var item in delMDBWorkspaceList)
                {
                    //删除空数据库
                    //File.Delete(item);//提示另一进程在用
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                System.Diagnostics.Trace.WriteLine(ex.Source);

                err = string.Format("分割数据库【{0}】时失败：{1}!", (sourceWS as IWorkspace).PathName, ex.Message);
            }
            finally
            {
                if (temp_table != null)
                    (temp_table as IDataset).Delete();

                if (File.Exists(xmlExportFile))
                    File.Delete(xmlExportFile);
            }

            return err;
        }



    }
}
