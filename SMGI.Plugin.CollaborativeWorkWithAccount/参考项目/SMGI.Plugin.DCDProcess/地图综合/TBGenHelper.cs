using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Data;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.Carto;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using System.Diagnostics;
using System.Threading;
namespace SMGI.Plugin.DCDProcess
{

        /// <summary>
        /// 创建临时数据库、打开数据库等操作
        /// </summary>
        public static class TBGenHelper
        {
            public static IWorkspace CreateTempWorkspace(string fullPath)
            {
                IWorkspace pWorkspace = null;
                IWorkspaceFactory2 wsFactory = new FileGDBWorkspaceFactoryClass();

                if (!Directory.Exists(fullPath))
                {

                    IWorkspaceName pWorkspaceName = wsFactory.Create(System.IO.Path.GetDirectoryName(fullPath),
                        System.IO.Path.GetFileName(fullPath), null, 0);
                    IName pName = (IName)pWorkspaceName;
                    pWorkspace = (IWorkspace)pName.Open();
                }
                else
                {
                    pWorkspace = wsFactory.OpenFromFile(fullPath, 0);
                }
                return pWorkspace;
            }
            private static IFields CreatLayerAttribute(Dictionary<string, esriFieldType> fieldsDic, ISpatialReference sr, esriGeometryType geometryType = esriGeometryType.esriGeometryPolygon)
            {
                //设置字段集
                IFields fields = new FieldsClass();
                var fieldsEdit = (IFieldsEdit)fields;
                IField field = new FieldClass();
                var fieldEdit = (IFieldEdit)field;

                //创建主键
                fieldEdit.Name_2 = "OBJECTID";
                fieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
                fieldsEdit.AddField(field);

                foreach (var kv in fieldsDic)
                {
                    field = new FieldClass();
                    fieldEdit = (IFieldEdit)field;
                    fieldEdit.Name_2 = kv.Key;
                    fieldEdit.Type_2 = kv.Value;
                    fieldsEdit.AddField(field);
                }


                //创建图形字段
                IGeometryDef geometryDef = new GeometryDefClass();
                var geometryDefEdit = (IGeometryDefEdit)geometryDef;
                geometryDefEdit.GeometryType_2 = geometryType;
                geometryDefEdit.SpatialReference_2 = sr;

                field = new FieldClass();
                fieldEdit = (IFieldEdit)field;
                fieldEdit.Name_2 = "SHAPE";
                fieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
                fieldEdit.GeometryDef_2 = geometryDef;
                fieldsEdit.AddField(field);
                return fields;
            }

            //创建要素类
            public static IFeatureClass CreateFeatureClass(string name, ISpatialReference sr, Dictionary<string, esriFieldType> fieldsDic, esriGeometryType geometryType = esriGeometryType.esriGeometryPolygon)
            {
                try
                {

                    string fullPath = AppDataPath + "\\TBGenWorkspace.gdb";
                    IWorkspace ws = CreateTempWorkspace(fullPath);
                    IFeatureWorkspace fws = ws as IFeatureWorkspace;
                    if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, name))
                    {
                        (fws.OpenTable(name) as IDataset).Delete();
                    }
                    if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, name))
                    {
                        IFeatureClass fcl = fws.OpenFeatureClass(name);
                        (fcl as IDataset).Delete();
                       
                    }
                    IFields org_fields = CreatLayerAttribute(fieldsDic, sr, geometryType);

                    return fws.CreateFeatureClass(name, org_fields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");
                }
                catch
                {
                    return null;
                }
            }

            public static string AppDataPath
            {
                get
                {
                    if (System.Environment.OSVersion.Version.Major <= 5)
                    {
                        return System.IO.Path.GetFullPath(
                            System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + @"\..");
                    }

                    var dp = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    var di = new System.IO.DirectoryInfo(dp);
                    var ds = di.GetDirectories("SMGI");
                    if (ds == null || ds.Length == 0)
                    {
                        var sdi = di.CreateSubdirectory("SMGI");
                        return sdi.FullName;
                    }
                    else
                    {
                        return ds[0].FullName;
                    }
                }
            }

            public static void AddField0(string gdbPath, string fclName, string fieldName)
            {
                //IFeatureClass fCls = null;
                //IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
                //IWorkspace workspace = wsFactory.OpenFromFile(gdbPath, 0);
                //IWorkspaceFactoryLockControl ipWsFactoryLock = (IWorkspaceFactoryLockControl)wsFactory;//注意在java api中不能强转切记需要IWorkspaceFactoryLockControl ipWsFactoryLock = new IWorkspaceFactoryLockControlProxy(pwf);
                //if (ipWsFactoryLock.SchemaLockingEnabled)
                //{
                //    ipWsFactoryLock.DisableSchemaLocking();
                //}
                //if ((workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fclName))
                //{
                //    fCls=(workspace as IFeatureWorkspace).OpenFeatureClass(fclName);
                //}
                //if (fCls == null)
                //    return;
                //int index = fCls.FindField(fieldName);
                //if (index != -1)
                //{
                //    return;
                //}
                //IFields pFields = fCls.Fields;
                //IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
                //IField pField = new FieldClass();
                //IFieldEdit pFieldEdit = pField as IFieldEdit;
                //pFieldEdit.Name_2 = fieldName;
                //pFieldEdit.AliasName_2 = fieldName;
                //pFieldEdit.Length_2 = 1;
                //pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
                //IClass pTable = fCls as IClass;
                //pTable.AddField(pField);
                //pFieldsEdit = null;
                //pField = null;

            }

            /// <summary>
            /// 选择要素类
            /// </summary>
            /// <param name="tempDic"></param>
            /// <param name="title"></param>
            /// <param name="multiSelect"></param>
            public static void AddTempDatas(ref Dictionary<IFeatureClass, string> tempDic, string title, bool multiSelect = false)
            {
                tempDic = new Dictionary<IFeatureClass, string>();
                IGxDialog dlg = new GxDialog();
                IEnumGxObject enumObj;
                IGxObjectFilterCollection filterCollection = dlg as IGxObjectFilterCollection;
                IGxObjectFilter ipFilter = new GxFilterFeatureClassesClass();
                filterCollection.AddFilter(ipFilter, true);
                dlg.AllowMultiSelect = multiSelect;
                dlg.Title = title;
                bool result = dlg.DoModalOpen(0, out enumObj);
                if (!result)
                    return;
                if (enumObj != null)
                {
                    enumObj.Reset();
                    IGxObject gxObj = null;

                    while ((gxObj = enumObj.Next()) != null)
                    {
                        if (gxObj is IGxDataset)
                        {
                            #region
                            IGxDataset gxDataset = gxObj as IGxDataset;
                            IDataset pDataset = gxDataset.Dataset;
                            switch (pDataset.Type)
                            {
                                case esriDatasetType.esriDTFeatureClass:
                                    IFeatureClass pFc = pDataset as IFeatureClass;
                                    tempDic[pFc] = gxObj.FullName;
                                    break;
                                case esriDatasetType.esriDTFeatureDataset:
                                    IFeatureDataset pFeatureDs = pDataset as IFeatureDataset;
                                    //do anyting you like
                                    break;
                                case esriDatasetType.esriDTRasterDataset:
                                    IRasterDataset rasterDs = pDataset as IRasterDataset;
                                    //do anyting you like
                                    break;
                                case esriDatasetType.esriDTTable:
                                    ITable pTable = pDataset as ITable;
                                    //do anyting you like
                                    break;
                                case esriDatasetType.esriDTTin:
                                    ITin pTin = pDataset as ITin;
                                    //do anyting you like
                                    break;
                                case esriDatasetType.esriDTRasterCatalog:
                                    IRasterCatalog pCatalog = pDataset as IRasterCatalog;
                                    //do anyting you like
                                    break;
                                default:
                                    break;

                            }
                            #endregion
                        }
                        else if (gxObj is IGxLayer)
                        {
                            IGxLayer gxLayer = gxObj as IGxLayer;
                            ILayer pLayer = gxLayer.Layer;

                            //do anything you like
                        }
                    }
                }
            }

            /// <summary>
            /// 选择数据库
            /// </summary>
            /// <param name="multiSelect"></param>
            /// <returns></returns>
            public static IWorkspace AddTempDataBase(bool multiSelect = false)
            {
                IWorkspace ws = null;
                IGxDialog dlg = new GxDialog();
                IEnumGxObject enumObj;
                IGxObjectFilterCollection filterCollection = dlg as IGxObjectFilterCollection;
                // filterCollection.AddFilter(new GxFilterPersonalGeodatabases(), true);
                filterCollection.AddFilter(new GxFilterFileGeodatabases(), true);
                dlg.AllowMultiSelect = multiSelect;
                dlg.Title = "选择数据库";
                bool result = dlg.DoModalOpen(0, out enumObj);
                if (!result)
                    return ws;
                if (enumObj != null)
                {
                    enumObj.Reset();
                    IGxObject gxObj = null;
                    while ((gxObj = enumObj.Next()) != null)
                    {
                        if (gxObj is IGxDatabase)
                        {

                            IGxDatabase gxDataset = gxObj as IGxDatabase;
                            ws = gxDataset.Workspace;
                            break;

                        }

                    }
                }
                return ws;
            }
            public static string SaveTempDataBase()
            {
                
                IGxDialog dlg = new GxDialog();
                IGxObjectFilterCollection filterCollection = dlg as IGxObjectFilterCollection;
                //filterCollection.AddFilter(new GxFilterPersonalGeodatabases(), true);
                filterCollection.AddFilter(new GxFilterFileGeodatabases(), true);


                dlg.AllowMultiSelect = false;
                dlg.Title = "保存数据库";
                bool result = dlg.DoModalSave(0);
                if (!result)
                    return string.Empty;
                return dlg.FinalLocation.FullName + "\\" + (dlg.Name.ToLower().Contains(".gdb")==true?dlg.Name: dlg.Name+".gdb");
                
            }

            public static IWorkspace AddTempGISDataBase(bool multiSelect = false)
            {
                IWorkspace ws = null;
                IGxDialog dlg = new GxDialog();
                IEnumGxObject enumObj;
                IGxObjectFilterCollection filterCollection = dlg as IGxObjectFilterCollection;
                filterCollection.AddFilter(new GxFilterPersonalGeodatabases(), true);
                filterCollection.AddFilter(new GxFilterFileGeodatabases(), true);
                dlg.AllowMultiSelect = multiSelect;
                dlg.Title = "选择数据库";
                bool result = dlg.DoModalOpen(0, out enumObj);
                if (!result)
                    return ws;
                if (enumObj != null)
                {
                    enumObj.Reset();
                    IGxObject gxObj = null;
                    while ((gxObj = enumObj.Next()) != null)
                    {
                        if (gxObj is IGxDatabase)
                        {

                            IGxDatabase gxDataset = gxObj as IGxDatabase;
                            ws = gxDataset.Workspace;
                            break;

                        }

                    }
                }
                return ws;
            }
            public static void WaitForTimerCheck(int id,int timeOut=-1)
            {
                int tempTime = 0;
           
                try
                {
                    while (true)
                    {
                        using (var p = System.Diagnostics.Process.GetProcessById(id))
                        {
                            Thread.Sleep(300);
                            tempTime += 300;
                        }
                        if (timeOut > 0)
                        {
                            if(tempTime>timeOut)
                            {
                                break;
                            }
                        }
                  
                    }
                }
                catch
                {
                    return;
                }
            }

            public static bool UseShellExecute = true;
            public static bool CreateNoWindow = false;
            public static string GenExePath = SMGI.Common.GApplication.ExePath + @"\TBGeneralize.exe";
            public static int  ExcuteGenShell(string xml)
            {
                string exePath = SMGI.Common.GApplication.ExePath + @"\TBGeneralize.exe";
                System.Diagnostics.Process p = null;
                ProcessStartInfo si = null;
                int pid = -1;
                using (p = new System.Diagnostics.Process())
                {
                    si = new ProcessStartInfo();
                    si.FileName = exePath;
                    si.Arguments = string.Format("\"{0}\"", xml);
                    si.UseShellExecute = UseShellExecute;
                    si.CreateNoWindow = CreateNoWindow;
                    p.StartInfo = si;
                    p.Start();
                    pid = p.Id;
                }
                return pid;
            }
            public static IFeatureClass QueryFeatureClass(string gdbPath, string fclName)
            {
                IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
                IWorkspace workspace = wsFactory.OpenFromFile(gdbPath, 0);
                if ((workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fclName))
                {
                    return (workspace as IFeatureWorkspace).OpenFeatureClass(fclName);
                }
                return null;

            }
            public static bool ExistFeatureClass(string gdbPath, string fclName)
            {
                IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
                IWorkspace workspace = wsFactory.OpenFromFile(gdbPath, 0);
                if ((workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fclName))
                {
                    return true;
                }
                return false;

            }

        }

        /// <summary>
        /// 自动综合相关信息
        /// </summary>
        public class TBAutoGenInfo
        {
            public string ClassName;
            public string XmlPath;
            public string ChsName;
        }
}
