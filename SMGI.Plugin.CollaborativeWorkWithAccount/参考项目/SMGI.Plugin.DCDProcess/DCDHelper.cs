using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;

using System.Data;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesFile;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using System.Windows.Forms;
using System.Diagnostics;
using ESRI.ArcGIS.Geoprocessor;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessing;
using SMGI.Plugin.GeneralEdit;
namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 辅助类，公共方法
    /// </summary>
    public class DCDHelper
    {
        public static long MaxMem
        {
            get
            {
                return 1024 * 1024 * 700;//字节
            }
        }

        public static long MaxMem2
        {
            get
            {
                return 1024 * 1024 * 500;//字节
            }
        }

        /// <summary>
        ///  从mdb提取表
        /// </summary>
        /// <param name="mdbFilePath">mdb路径</param>
        /// <param name="tableName">表名称</param>
        /// <returns></returns>
        public static DataTable ReadToDataTable(string mdbFilePath, string tableName)
        {
            
            if (!System.IO.File.Exists(mdbFilePath))
            {
                MessageBox.Show(string.Format("没有找到文件：【{0}】！",mdbFilePath));
                return null;
            }

            DataTable pDataTable = new DataTable();
            IWorkspaceFactory pWorkspaceFactory = new AccessWorkspaceFactoryClass();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(mdbFilePath, 0);
            IEnumDataset pEnumDataset = pWorkspace.get_Datasets(esriDatasetType.esriDTAny);
            pEnumDataset.Reset();
            IDataset pDataset = pEnumDataset.Next();
            ITable pTable = null;
            while (pDataset != null)
            {
                if (pDataset.Name == tableName)
                {
                    pTable = pDataset as ITable;
                    break;
                }
                pDataset = pEnumDataset.Next();
            }
            Marshal.ReleaseComObject(pEnumDataset);


            if (pTable == null)
            {
                MessageBox.Show(string.Format("文件【{0}】中没有找到表：【{1}】！", mdbFilePath,tableName));
                return null;
            }

            ICursor pCursor = pTable.Search(null, false);
            IRow pRow = null;
            //添加表的字段信息
            for (int i = 0; i < pTable.Fields.FieldCount; i++)
            {
                pDataTable.Columns.Add(pTable.Fields.Field[i].Name);
            }
            //添加数据
            while ((pRow = pCursor.NextRow()) != null)
            {
                DataRow dr = pDataTable.NewRow();
                for (int i = 0; i < pRow.Fields.FieldCount; i++)
                {
                    object obValue = pRow.get_Value(i);
                    if (obValue != null && !Convert.IsDBNull(obValue))
                    {
                        dr[i] = pRow.get_Value(i);
                    }
                    else
                    {
                        dr[i] = "";
                    }
                }
                pDataTable.Rows.Add(dr);
            }
            Marshal.ReleaseComObject(pCursor);

            Marshal.ReleaseComObject(pWorkspace);
            Marshal.ReleaseComObject(pWorkspaceFactory);

            return pDataTable;
        }

        /// <summary>
        /// 获取要素类
        /// </summary>
        /// <param name="pws"></param>
        /// <param name="fclName"></param>
        /// <returns></returns>
        public static IFeatureClass GetFclViaWs(IWorkspace pws, string fclName)
        {
            try
            {
                IFeatureClass fcl = null;
                fcl = (pws as IFeatureWorkspace).OpenFeatureClass(fclName);
                return fcl;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return null; 
            }
        }

        /// <summary>
        /// 创建临时工作空间
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static IWorkspace createTempWorkspace(string fullPath)
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

        /// <summary>
        /// 床架内存工作空间
        /// </summary>
        /// <returns></returns>
        public static IWorkspace CreateInMemoryWorkspace()
        {
            // Create an in-memory workspace factory.
            IWorkspaceFactory pWorkspaceFactory = new InMemoryWorkspaceFactoryClass();

            // Create a new in-memory workspace. This returns a name object.
            IWorkspaceName pWorkspaceName = pWorkspaceFactory.Create(null, "TempWorkspace", null, 0);
            IName pName = (IName)pWorkspaceName;

            // Open the workspace through the name object.
            IWorkspace pWorkspace = (IWorkspace)pName.Open();


            //Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.InMemoryWorkspaceFactory");
            //IWorkspaceFactory pWorkspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);

            //IWorkspaceName pWorkspaceName = pWorkspaceFactory.Create("", "TempWorkspace", null, 0);

            //IName pName = (IName)pWorkspaceName;
            //IWorkspace pWorkspace = (IWorkspace)pName.Open();

            return pWorkspace;
        }

        /// <summary>
        /// 获取应用程序默认路径
        /// </summary>
        public static string GetAppDataPath()
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

        /// <summary>
        /// 从工作空间中获取栅格数据集
        /// </summary>
        /// <returns></returns>
        public static IRasterDataset getRasterDatasetFromWorkspace(IWorkspace ws, string rasterDatasetName)
        {
            IRasterWorkspaceEx rasterWorkspace = ws as IRasterWorkspaceEx;
            if (null == rasterWorkspace)
                return null;

            if (!(ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTRasterDataset, rasterDatasetName))
                return null;

            return rasterWorkspace.OpenRasterDataset(rasterDatasetName);
        }

        /// <summary>
        /// 以要素类org_fc为模板，创建一个空要素类
        /// </summary>
        /// <param name="fWS"></param>
        /// <param name="org_fc"></param>
        /// <param name="FeatureClassName"></param>
        /// <param name="bOverwriteOutput">是否直接覆盖</param>
        /// <returns></returns>
        public static IFeatureClass CreateFeatureClassStructToWorkspace(IFeatureWorkspace fWS, IFeatureClass org_fc, string FeatureClassName, bool bOverwriteOutput = true)
        {
            FeatureClassName = FeatureClassName.Split('.').Last();

            if ((fWS as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, FeatureClassName))
            {
                if (!bOverwriteOutput)
                {
                    if (MessageBox.Show(string.Format("要素类【{0}】已经存在，是否确定直接覆盖,或退出？", FeatureClassName), "警告", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        bOverwriteOutput = true;
                    }
                }

                if (bOverwriteOutput)
                {
                    IFeatureClass fc = fWS.OpenFeatureClass(FeatureClassName);
                    (fc as IDataset).Delete();
                }
                else
                {
                    return null;
                }
            }

            IFields org_fields;
            org_fields = (org_fc.Fields as IClone).Clone() as IFields;
            for (int i = 0; i < org_fields.FieldCount; i++)
            {
                IField field = org_fields.get_Field(i);
                IFieldEdit fieldEdit = field as IFieldEdit;
                fieldEdit.Name_2 = field.Name.ToUpper();
            }


            IObjectClassDescription featureDescription = new FeatureClassDescriptionClass();
            IFieldsEdit target_fields = featureDescription.RequiredFields as IFieldsEdit;
            for (int i = 0; i < org_fields.FieldCount; i++)
            {
                IField field = org_fields.get_Field(i);

                if (!(field as IFieldEdit).Editable)
                {
                    continue;
                }

                if (field.Type == esriFieldType.esriFieldTypeGeometry)
                {
                    (target_fields as IFieldsEdit).set_Field(target_fields.FindFieldByAliasName((featureDescription as IFeatureClassDescription).ShapeFieldName),
                        (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField);

                    continue;
                }

                if (target_fields.FindField(field.Name) != -1)
                {
                    continue;
                }

                IField field_new = (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField;
                (target_fields as IFieldsEdit).AddField(field_new);
            }
            return fWS.CreateFeatureClass(FeatureClassName, target_fields, featureDescription.InstanceCLSID,
                featureDescription.ClassExtensionCLSID, esriFeatureType.esriFTSimple,
                (featureDescription as IFeatureClassDescription).ShapeFieldName, string.Empty);
        }

        /// <summary>
        /// 将要素类inFeatureClss中指定的要素复制到目标要素类中targetFeatureClss
        /// </summary>
        /// <param name="inFeatureClss"></param>
        /// <param name="qf"></param>
        /// <param name="targetFeatureClss"></param>
        /// <param name="bRemoveCollaValue"></param>
        public static void CopyFeaturesToFeatureClass(IFeatureClass inFeatureClss, IQueryFilter qf, IFeatureClass targetFeatureClss, bool bRemoveCollaValue = true)
        {
            if (inFeatureClss.ShapeType != targetFeatureClss.ShapeType)
            {
                return;
            }

            bool bProjection = false;
            ISpatialReference in_sr = (inFeatureClss as IGeoDataset).SpatialReference;
            ISpatialReference target_sr = (targetFeatureClss as IGeoDataset).SpatialReference;
            if (in_sr.Name != target_sr.Name)
            {
                bProjection = true;
            }

            IFeatureClassLoad pFCLoad = targetFeatureClss as IFeatureClassLoad;
            pFCLoad.LoadOnlyMode = true;

            IFeatureCursor pTargetFeatureCursor = targetFeatureClss.Insert(true);
            

            IFeatureCursor pInFeatureCursor = inFeatureClss.Search(qf, true);
            IFeature pInFeature = null;

            try
            {
                while ((pInFeature = pInFeatureCursor.NextFeature()) != null)
                {
                    IFeatureBuffer pFeatureBuffer = targetFeatureClss.CreateFeatureBuffer();

                    IGeometry shape = pInFeature.ShapeCopy;
                    if (bProjection)
                        shape.Project(target_sr);//投影变换

                    pFeatureBuffer.Shape = shape;
                    for (int i = 0; i < pFeatureBuffer.Fields.FieldCount; i++)
                    {
                        IField pfield = pFeatureBuffer.Fields.get_Field(i);
                        if (pfield.Type == esriFieldType.esriFieldTypeGeometry || pfield.Type == esriFieldType.esriFieldTypeOID)
                            continue;

                        if (pfield.Name.ToUpper() == "SHAPE_LENGTH" || pfield.Name.ToUpper() == "SHAPE_AREA")
                            continue;

                        if (bRemoveCollaValue)
                        {
                            if (pfield.Name.ToUpper() == cmdUpdateRecord.CollabGUID || pfield.Name.ToUpper() == cmdUpdateRecord.CollabVERSION ||
                                pfield.Name.ToUpper() == cmdUpdateRecord.CollabDELSTATE || pfield.Name.ToUpper() == cmdUpdateRecord.CollabOPUSER)
                                continue;
                        }

                        //复制属性值
                        int index = pInFeature.Fields.FindField(pfield.Name);
                        if (index != -1 && pfield.Editable)
                        {
                            pFeatureBuffer.set_Value(i, pInFeature.get_Value(index));
                        }

                    }
                    pTargetFeatureCursor.InsertFeature(pFeatureBuffer);

                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
            }
            pTargetFeatureCursor.Flush();

            System.Runtime.InteropServices.Marshal.ReleaseComObject(pTargetFeatureCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pInFeatureCursor);

            pFCLoad.LoadOnlyMode = false;
        }

        /// <summary>
        /// 打散要素
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="qf"></param>
        public static void MultiToSingle(IFeatureClass fc, IQueryFilter qf)
        {
            
            List<int> oidList = new List<int>();
            IFeatureCursor pCur = fc.Search(qf, true);
            IFeature f = null;
            while ((f = pCur.NextFeature()) != null)
            {
                var po = (IPolygon4)f.Shape;
                var gc = (IGeometryCollection)po.ConnectedComponentBag;
                if (gc.GeometryCount > 1)
                {
                    oidList.Add(f.OID);
                }
            }
            Marshal.ReleaseComObject(pCur);

            if (oidList.Count == 0)
            {
                return;
            }

            IFeatureClassLoad pFCLoad = fc as IFeatureClassLoad;
            pFCLoad.LoadOnlyMode = true;

            string oidSet = "";
            foreach (var oid in oidList)
            {
                if (oidSet != "")
                    oidSet += string.Format(",{0}", oid);
                else
                    oidSet = string.Format("{0}", oid);
            }

            string filter = string.Format("OBJECTID in ({0})", oidSet);
            IFeatureCursor pCursor = fc.Search(new QueryFilterClass() { WhereClause = filter }, false);
            IFeature fe = null;
            while ((fe = pCursor.NextFeature()) != null)
            {
                var po = (IPolygon4)fe.ShapeCopy;
                var gc = (IGeometryCollection)po.ConnectedComponentBag;
                if (gc.GeometryCount <= 1) continue;

                //内存监控
                if (Environment.WorkingSet > DCDHelper.MaxMem)
                {
                    GC.Collect();
                }

                //打散要素
                try
                {
                    var fci = fc.Insert(true);
                    for (var i = 1; i < gc.GeometryCount; i++)
                    {
                        var fb = fc.CreateFeatureBuffer();

                        //几何赋值
                        fb.Shape = gc.Geometry[i];

                        //属性赋值
                        for (int j = 0; j < fb.Fields.FieldCount; j++)
                        {
                            IField pfield = fb.Fields.get_Field(j);
                            if (pfield.Type == esriFieldType.esriFieldTypeGeometry || pfield.Type == esriFieldType.esriFieldTypeOID)
                            {
                                continue;
                            }

                            if (pfield.Name.ToUpper() == "SHAPE_LENGTH" || pfield.Name.ToUpper() == "SHAPE_AREA")
                            {
                                continue;
                            }

                            int index = fe.Fields.FindField(pfield.Name);
                            if (index != -1 && pfield.Editable)
                            {
                                fb.set_Value(j, fe.get_Value(index));
                            }

                        }
                        fci.InsertFeature(fb);
                    }
                    fci.Flush();
                    
                    //修改fe的几何为 gc.Geometry[0]
                    fe.Shape = gc.Geometry[0];
                    fe.Store();
                }
                catch (COMException ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    string err = ex.Message;
                }
            }

            Marshal.ReleaseComObject(pCursor);
            pFCLoad.LoadOnlyMode = false;
        }
        // <summary>
        /// 截取字符串到Dictionary中
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Dictionary<string, string> mdbreaderF(string str)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            if (str.Contains('|'))
            {
                string[] temp11 = str.Split('|');
                foreach (var item in temp11)
                {

                    if (item.Contains('['))
                    {
                        string[] temp1 = item.Split('[');//LRDL[gb:410100,410100]
                        string key = temp1[0];//LRDL
                        string temp2 = temp1[1].Substring(0, temp1[1].Length - 1);//gb:410100,410100

                        result.Add(key, temp2);
                    }
                    else { result.Add(item, ""); }

                }
            }
            else
            {

                if (str.Contains('['))
                {
                    string[] temp1 = str.Split('[');//LRDL[,410100,410100]
                    string key = temp1[0];
                    string temp2 = temp1[1].Substring(0, temp1[1].Length - 1);//410100,410100                   
                    result.Add(key, temp2);
                }
                else { result.Add(str, ""); }
            }
            return result;
        }

        /// <summary>
        /// 根据几何几何，增加一个新要素类
        /// </summary>
        /// <param name="fWS"></param>
        /// <param name="fdt"></param>
        /// <param name="newFCName"></param>
        /// <param name="geoType"></param>
        /// <param name="spatialReference"></param>
        /// <param name="geoList"></param>
        /// <param name="bOverwriteOutput"></param>
        /// <returns></returns>
        public static IFeatureClass CreateFeatureClassStructToWorkspace(IFeatureWorkspace fWS, IFeatureDataset fdt, string newFCName, 
            esriGeometryType geoType, ISpatialReference spatialReference, List<IGeometry> geoList, bool bOverwriteOutput = true)
        {
            IFeatureClass result = null;

            if ((fWS as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, newFCName))
            {
                if (!bOverwriteOutput)
                {
                    if (MessageBox.Show(string.Format("要素类【{0}】已经存在，是否确定直接覆盖,或退出？", newFCName), "警告", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        bOverwriteOutput = true;
                    }
                }

                if (bOverwriteOutput)
                {
                    IFeatureClass fc = fWS.OpenFeatureClass(newFCName);
                    (fc as IDataset).Delete();
                }
                else
                {
                    return null;
                }
            }

            #region 创建临时点要素类
            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
            IFields fields = ocDescription.RequiredFields;
            IFieldsEdit pFieldsEdit = (IFieldsEdit)fields;

            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = (IWorkspace)fWS;
            fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

            int shapeFieldIndex = fields.FindField(fcDescription.ShapeFieldName);
            IField Shapefield = fields.get_Field(shapeFieldIndex);
            IGeometryDef geometryDef = Shapefield.GeometryDef;
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = geoType;
            geometryDefEdit.SpatialReference_2 = spatialReference;

            if (fdt == null)
            {
                result = fWS.CreateFeatureClass(newFCName, fields, ocDescription.InstanceCLSID,
                    ocDescription.ClassExtensionCLSID, esriFeatureType.esriFTSimple, fcDescription.ShapeFieldName, "");
            }
            else
            {
                result = fdt.CreateFeatureClass(newFCName, fields, ocDescription.InstanceCLSID, 
                    ocDescription.ClassExtensionCLSID, esriFeatureType.esriFTSimple, fcDescription.ShapeFieldName, "");
            }
            #endregion

            #region 复制几何，插入新要素
            IFeatureCursor feCursor = result.Insert(true);
            foreach (var geo in geoList)
            {
                IFeatureBuffer featureBuf = result.CreateFeatureBuffer();
                featureBuf.Shape = geo;

                feCursor.InsertFeature(featureBuf);
            }
            feCursor.Flush();

            System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);
            #endregion

            return result;

        }

        /// <summary>
        /// 增加一个整数字段，并将字段值根据某一整数字段值进行初始化
        /// </summary>
        /// <param name="fCls"></param>
        /// <param name="newFieldName"></param>
        /// <param name="valFieldName"></param>
        public static void AddIntegerField(IFeatureClass fCls, string newFieldName, string valFieldName)
        {
            //新增字段
            IFields pFields = fCls.Fields;
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = newFieldName;
            pFieldEdit.AliasName_2 = newFieldName;
            pFieldEdit.Length_2 = 1;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            IClass pTable = fCls as IClass;
            pTable.AddField(pField);
            pFieldsEdit = null;
            pField = null;

            //赋值
            int index = fCls.FindField(newFieldName);
            int valIndex = fCls.FindField(valFieldName);
            if (index != -1 && valIndex != -1)
            {
                IFeatureCursor fCursor = fCls.Update(null, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    try
                    {
                        int val = int.Parse(f.get_Value(valIndex).ToString());

                        f.set_Value(index, val);
                        fCursor.UpdateFeature(f);
                    }
                    catch
                    {
                        continue;
                    }
                    
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
            }
        }

        /// <summary>
        /// 通过命令行方式调用py文件(需安装python 32位)
        /// </summary>
        /// <param name="pyFilePath">py文件全路径</param>
        /// <param name="paramList">参数列表</param>
        /// <returns></returns>
        public static bool RunPython(string pyFilePath, params string[] paramList)
        {
            try
            {
                string arguments = "/c " + pyFilePath;
                foreach (var item in paramList)
                {
                    arguments += " " + item;
                }

                ProcessStartInfo si = new ProcessStartInfo("cmd", arguments);
                si.RedirectStandardOutput = true;
                si.RedirectStandardError = true;
                si.RedirectStandardInput = true;
                si.UseShellExecute = false;
                si.CreateNoWindow = true;


                string outStr = "";
                string errStr = "";
                using (System.Diagnostics.Process p = new System.Diagnostics.Process())
                {
                    p.StartInfo = si;
                    p.Start();

                    outStr = p.StandardOutput.ReadToEnd();
                    errStr = p.StandardError.ReadToEnd();

                    p.WaitForExit();
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();

                bool res = outStr.Length == 0 && errStr.Length == 0;
                if(!res)
                {
                    MessageBox.Show(errStr + "\n" + outStr);
                }

                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 返回与直线成degree夹角的角度指
        /// </summary>
        /// <param name="pPolyline"></param>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static double GetAngle(ILine pPolyline, double degree = 0)
        {
            Double radian = pPolyline.Angle;
            Double angle = radian * 180 / Math.PI;
            angle = angle + degree;
            while (angle < 0)
            {
                angle = angle + 360;
            }

            angle = angle % 360;

            return angle;
        }


        /// <summary>
        /// 相交分析
        /// </summary>
        /// <param name="fc1"></param>
        /// <param name="fc2"></param>
        /// <param name="outputType">INPUT、LINE、POINT</param>
        /// <returns></returns>
        public static IFeatureClass IntersectAnalysis(IFeatureClass fc1, IQueryFilter qf1, IFeatureClass fc2, IQueryFilter qf2, string outputType = "INPUT")
        {
            IFeatureClass result = null;

            if (fc1 == null || fc2 == null)
                return null;

            try
            {
                //临时数据库
                string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
                IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);
                IFeatureWorkspace fws = ws as IFeatureWorkspace;

                Geoprocessor gp = new Geoprocessor();
                gp.OverwriteOutput = true;
                gp.SetEnvironmentValue("workspace", ws.PathName);


                ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer makeFeatureLayer = new ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer();
                ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute selectLayerByAttribute = new ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute();

                makeFeatureLayer.in_features = fc1;
                makeFeatureLayer.out_layer = fc1.AliasName + "_Layer";
                SMGI.Common.Helper.ExecuteGPTool(gp, makeFeatureLayer, null);
                selectLayerByAttribute.in_layer_or_view = fc1.AliasName + "_Layer";
                selectLayerByAttribute.where_clause = qf1.WhereClause;
                SMGI.Common.Helper.ExecuteGPTool(gp, selectLayerByAttribute, null);

                makeFeatureLayer.in_features = fc2;
                makeFeatureLayer.out_layer = fc2.AliasName + "_Layer";
                SMGI.Common.Helper.ExecuteGPTool(gp, makeFeatureLayer, null);
                selectLayerByAttribute.in_layer_or_view = fc2.AliasName + "_Layer";
                selectLayerByAttribute.where_clause = qf2.WhereClause;
                SMGI.Common.Helper.ExecuteGPTool(gp, selectLayerByAttribute, null);


                ESRI.ArcGIS.AnalysisTools.Intersect intersect = new ESRI.ArcGIS.AnalysisTools.Intersect();
                intersect.in_features = fc1.AliasName + "_Layer;" + fc2.AliasName + "_Layer";
                intersect.out_feature_class = fc1.AliasName + "_" + fc2.AliasName + "_intersect";
                intersect.join_attributes = "ONLY_FID";
                intersect.output_type = outputType;

                SMGI.Common.Helper.ExecuteGPTool(gp, intersect, null);

                result = fws.OpenFeatureClass(fc1.AliasName + "_" + fc2.AliasName + "_intersect");
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;

        }

        /// <summary>
        /// 读取MDB表
        /// </summary>
        /// <param name="mdbFilePath"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static DataTable ReadMDBTable(string mdbFilePath, string tableName)
        {
            DataTable dt = null;
            if (!System.IO.File.Exists(mdbFilePath))
                return dt;

            IWorkspaceFactory wsFactory = new AccessWorkspaceFactoryClass();
            IWorkspace ws = wsFactory.OpenFromFile(mdbFilePath, 0);
            IEnumDataset enumDataset = ws.get_Datasets(esriDatasetType.esriDTAny);
            enumDataset.Reset();
            IDataset dataset = null;
            ITable objTable = null;
            while ((dataset = enumDataset.Next()) != null)
            {
                if (dataset.Name == tableName)
                {
                    objTable = dataset as ITable;
                    break;
                }
            }
            Marshal.ReleaseComObject(enumDataset);
            if (objTable == null)
            {
                return dt;
            }

            //添加表的字段信息
            dt = new DataTable();
            for (int i = 0; i < objTable.Fields.FieldCount; i++)
            {
                dt.Columns.Add(objTable.Fields.Field[i].Name);
            }

            //添加数据
            ICursor rowCursor = objTable.Search(null, false);
            IRow row = null;
            while ((row = rowCursor.NextRow()) != null)
            {
                DataRow dr = dt.NewRow();
                for (int i = 0; i < row.Fields.FieldCount; i++)
                {
                    object obValue = row.get_Value(i);
                    if (obValue != null && !Convert.IsDBNull(obValue))
                    {
                        dr[i] = row.get_Value(i);
                    }
                    else
                    {
                        dr[i] = "";
                    }
                }
                dt.Rows.Add(dr);
            }
            Marshal.ReleaseComObject(rowCursor);

            Marshal.ReleaseComObject(ws);
            Marshal.ReleaseComObject(wsFactory);

            return dt;
        }

        #region 追踪相关函数
        /// <summary>
        /// 根据点选取需要追踪的要素和追踪线
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static IPolyline SelectTraceLineAndPolygonByPoint(IPoint pt, out IFeature fe)
        {
            IPolyline result = null;
            IGeometry bufferGeo = null;
            fe = null;
            var topoOp = (pt as ITopologicalOperator);
            if (GApplication.Application.Workspace.Map.SpatialReference is IGeographicCoordinateSystem)
            {
                bufferGeo = topoOp.Buffer(5 * 0.000009).Envelope;
            }
            else
            {
                double dis = GApplication.Application.ActiveView.ScreenDisplay.DisplayTransformation.FromPoints(3);
                bufferGeo = topoOp.Buffer(dis).Envelope;
            }


            ISpatialFilter sf = new SpatialFilter();
            sf.Geometry = bufferGeo;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l.Visible) && ((l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline || (l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon);

            })).ToArray();

            try
            {
                IEngineEditLayers editLayer = GApplication.Application.EngineEditor as IEngineEditLayers;
                foreach (var item in lyrs)
                {
                    if (!editLayer.IsEditable(item as IFeatureLayer))
                        continue;

                    IFeatureClass fc = (item as IGeoFeatureLayer).FeatureClass;
                    IFeatureCursor feCursor = fc.Search(sf, true);
                    //IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        if (fe.Shape == null || fe.Shape.IsEmpty)
                            continue;

                        if (fe.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                        {
                            result = (fe.ShapeCopy as ITopologicalOperator).Boundary as IPolyline;
                        }
                        else
                        {
                            result = fe.ShapeCopy as IPolyline;
                        }

                        break;
                    }
                    Marshal.ReleaseComObject(feCursor);

                    if (result != null)
                    {
                        return result;
                    }
                }
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }

            return null;
        }

        /// <summary>
        /// 根据点选取需要追踪的要素的追踪线
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static IPolyline SelectTraceLineByPoint(IPoint pt)
        {

            IFeature fe = null;
            return SelectTraceLineAndPolygonByPoint(pt, out fe);

        }
        /// <summary>
        /// 清除追踪线元素
        /// </summary>
        public static void DeleteTraceLineElement()
        {
            IGraphicsContainer gc = GApplication.Application.ActiveView.GraphicsContainer;
            gc.Reset();
            IElement ele = null;
            while ((ele = gc.Next()) != null)
            {
                IElementProperties ep1 = ele as IElementProperties;
                if (ep1.Name == "TraceSelLine")
                    GApplication.Application.ActiveView.GraphicsContainer.DeleteElement(ele);
            }
            GApplication.Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewBackground, null, GApplication.Application.ActiveView.Extent);
        }

        /// <summary>
        /// 将追踪线元素画出来
        /// </summary>
        /// <param name="tracePolyline"></param>
        public static void DrawTraceLineElement(IPolyline tracePolyline)
        {
            //清除原先的追踪线元素
            DeleteTraceLineElement();

            IElement element = null;
            ILineElement lineElement = null;
            ISimpleLineSymbol lineSymbol = null;
            IElementProperties ep = null;

            //绘制选中的线
            if (tracePolyline != null)
            {
                lineElement = new LineElementClass();
                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                lineSymbol.Color = new RgbColorClass { Red = 255 };
                lineSymbol.Width = 2;
                lineElement.Symbol = lineSymbol;

                //
                element = lineElement as IElement;
                element.Geometry = tracePolyline;
                ep = element as IElementProperties;
                ep.Name = "TraceSelLine";
                GApplication.Application.ActiveView.GraphicsContainer.AddElement(element, 0);
            }

            GApplication.Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewBackground, null, GApplication.Application.ActiveView.Extent);
        }

        public static void DrawPolygonElement(IPolygon polygon)
        {
            //清除原先的追踪线元素
            //DeleteTraceLineElement();

            IElement element = null;
            IPolygonElement polygonElement = null;
            IElementProperties ep = null;

            //绘制
            if (polygon != null)
            {
                polygonElement = new PolygonElementClass();
                element = polygonElement as IElement;
                element.Geometry = polygon;
                ep = element as IElementProperties;
                ep.Name = "TraceSelLine";
                GApplication.Application.ActiveView.GraphicsContainer.AddElement(element, 0);
            }

            GApplication.Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewBackground, null, GApplication.Application.ActiveView.Extent);
        }

        /// <summary>
        /// 获取节点组
        /// </summary>
        /// <param name="startPt"></param>
        /// <param name="endPt"></param>
        /// <param name="pLine"></param>
        /// <returns></returns>
        public static IPointCollection getColFromTwoPoint(IPolyline polyline, IPoint startPt, IPoint endPt)
        {
            IPointCollection ptColl = new PolylineClass();

            bool isSpilt;
            int startPart1Index, seg1Index;
            int startPart2Index, seg2Index;
            //起始点不在顶点上时，seg1Index为与起始点相邻的两个顶点中索引较大者
            polyline.SplitAtPoint(startPt, true, false, out isSpilt, out startPart1Index, out seg1Index);
            polyline.SplitAtPoint(endPt, true, false, out isSpilt, out startPart2Index, out seg2Index);
            List<int> oid_list_1 = new List<int>();
            List<int> oid_list_2 = new List<int>();
            if (startPart1Index != startPart2Index)//跨部件时，不添加中间节点
            {
                ptColl.AddPoint(startPt);
                ptColl.AddPoint(endPt);

                return ptColl;
            }

            IGeometryCollection gc = polyline as IGeometryCollection;
            if (polyline.IsClosed)//取较短线
            {
                int ptCount = (gc.Geometry[startPart1Index] as IPointCollection).PointCount;

                //起始点到结束点为索引顺序
                if (seg1Index < seg2Index)
                {
                    Console.WriteLine("起始点到结束点为索引顺序({0},{1})", seg1Index, seg2Index);
                    //线段1（线段结点索引顺序）
                    IPointCollection ptColl_1 = new PolylineClass();
                    for (int i = seg1Index; i <= seg2Index; i++)
                    {
                        ptColl_1.AddPoint((gc.Geometry[startPart1Index] as IPointCollection).get_Point(i));
                        oid_list_1.Add(i);
                    }

                    //线段2（线段结点索引逆序）
                    IPointCollection ptColl_2 = new PolylineClass();
                    for (int i = seg1Index; i >= 0; i--)
                    {
                        ptColl_2.AddPoint((gc.Geometry[startPart1Index] as IPointCollection).get_Point(i));
                        oid_list_2.Add(i);
                    }
                    for (int i = ptCount - 2; i >= seg2Index; i--)
                    {
                        ptColl_2.AddPoint((gc.Geometry[startPart1Index] as IPointCollection).get_Point(i));
                        oid_list_2.Add(i);
                    }

                    ptColl = (ptColl_1 as IPolyline).Length < (ptColl_2 as IPolyline).Length ? ptColl_1 : ptColl_2;
                    if ((ptColl_1 as IPolyline).Length < (ptColl_2 as IPolyline).Length)
                    {
                        Console.WriteLine("ptColl_1:{0}", string.Join(" ", oid_list_1));
                    }
                    else {
                        Console.WriteLine("ptColl_2:{0}", string.Join(" ", oid_list_2));
                    }
                }
                else
                //起始点到结束点为索引逆序
                {
                    Console.WriteLine("起始点到结束点为索引逆序({0},{1})", seg1Index, seg2Index);
                    //线段1（线段结点索引逆序）
                    IPointCollection ptColl_1 = new PolylineClass();
                    ptColl_1.AddPoint((gc.Geometry[startPart1Index] as IPointCollection).get_Point(seg1Index));
                    for (int i = seg1Index; i >= seg2Index; i--)
                    {
                        ptColl_1.AddPoint((gc.Geometry[startPart1Index] as IPointCollection).get_Point(i));
                        oid_list_1.Add(i);
                    }

                    //线段2（线段结点索引顺序）
                    IPointCollection ptColl_2 = new PolylineClass();
                    for (int i = seg1Index; i < ptCount - 1; i++)
                    {
                        ptColl_2.AddPoint((gc.Geometry[startPart1Index] as IPointCollection).get_Point(i));
                        oid_list_2.Add(i);
                    }
                    for (int i = 0; i <= seg2Index; i++)
                    {
                        ptColl_2.AddPoint((gc.Geometry[startPart1Index] as IPointCollection).get_Point(i));
                        oid_list_2.Add(i);
                    }

                    ptColl = (ptColl_1 as IPolyline).Length < (ptColl_2 as IPolyline).Length ? ptColl_1 : ptColl_2;
                    if ((ptColl_1 as IPolyline).Length < (ptColl_2 as IPolyline).Length)
                    {
                        Console.WriteLine("ptColl_1:{0}", string.Join(" ", oid_list_1));
                        for (int i = 0; i < ptColl_1.PointCount; i++) {
                            Console.WriteLine("返回节点坐标({0},{1})", ptColl_1.get_Point(i).X, ptColl_1.get_Point(i).Y);
                        }
                    }
                    else
                    {
                        Console.WriteLine("ptColl_2:{0}", string.Join(" ", oid_list_2));
                        for (int i = 0; i < ptColl_2.PointCount; i++)
                        {
                            Console.WriteLine("返回节点坐标({0},{1})", ptColl_2.get_Point(i).X, ptColl_2.get_Point(i).Y);
                        }
                    }
                }
            }
            else
            {
                if (seg1Index < seg2Index)
                {
                    for (int i = seg1Index; i <= seg2Index; i++)
                    {
                        ptColl.AddPoint((gc.Geometry[startPart1Index] as IPointCollection).get_Point(i));
                    }
                }
                else
                {
                    for (int i = seg1Index; i >= seg2Index; i--)
                    {
                        ptColl.AddPoint((gc.Geometry[startPart1Index] as IPointCollection).get_Point(i));
                    }
                }
            }

            return ptColl;
        }


        /// <summary>
        /// 获取最近节点
        /// </summary>
        /// <param name="startPt"></param>
        /// <param name="endPt"></param>
        /// <param name="pLine"></param>
        /// <returns></returns>
        public static IPoint getVertexFromPolyline(IPolyline polyline, IPoint mousePt)
        {
            IPoint vertex = new Point();
            IPointCollection ptColl = new PolylineClass();
            bool isSpilt;
            int startPartIndex, segIndex;
            polyline.SplitAtPoint(mousePt, true, false, out isSpilt, out startPartIndex, out segIndex);
            IGeometryCollection gc = polyline as IGeometryCollection;
            vertex = (gc.Geometry[startPartIndex] as IPointCollection).get_Point(segIndex);

            return vertex;
        }

        #endregion

        /// <summary>
        /// 拓扑纠正
        /// </summary>
        /// <param name="fc"></param>
        public static void featureSimplify(IFeatureClass fc)
        {
            IFeatureCursor pCursor = fc.Search(null, true);
            IFeature fe = null;
            while ((fe = pCursor.NextFeature()) != null)
            {
                //内存监控
                if (Environment.WorkingSet > DCDHelper.MaxMem)
                {
                    GC.Collect();
                }

                try
                {
                    IGeometry geo = fe.ShapeCopy;
                    ITopologicalOperator2 feaTopo = geo as ITopologicalOperator2;
                    feaTopo.IsKnownSimple_2 = false;
                    feaTopo.Simplify();
                    fe.Shape = geo;
                    fe.Store();
                }
                catch (Exception ex)
                {
                    continue;
                }

            }
            Marshal.ReleaseComObject(pCursor);
        }

        /// <summary>
        /// 将要素类fc中的非法面合并到融合为其临近的合法面(已被EliminateIllegalFeature替代)
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="illegalFeQF"></param>
        /// <param name="fws"></param>
        /// <param name="wo"></param>
        public static void UnionIllegalFeature(IFeatureClass fc, QueryFilterClass illegalFeQF, IFeatureWorkspace fws, WaitOperation wo = null)
        {
            int gbIndex = fc.Fields.FindField("GB");
            if (gbIndex == -1)
                return;

            //将非法面复制到一个新的临时图层
            IFeatureClass temp_fc = DCDHelper.CreateFeatureClassStructToWorkspace(fws, fc, "temp_illegal");
            DCDHelper.CopyFeaturesToFeatureClass(fc, illegalFeQF, temp_fc, false);
            //拓扑纠正
            DCDHelper.featureSimplify(temp_fc);


            //删除非非法面
            ITable pTable = fc as ITable;
            pTable.DeleteSearchedRows(illegalFeQF);
            //拓扑纠正
            DCDHelper.featureSimplify(fc);

            //
            Dictionary<int, IGeometry> oid2Geo = new Dictionary<int, IGeometry>();//植被面OID，非植被面几何


            //遍历临时非植被面
            IFeatureCursor pTempCursor = temp_fc.Search(null, false);
            IFeature tempFe = null;
            int cout = temp_fc.FeatureCount(null);
            int processedCount = 1;
            while ((tempFe = pTempCursor.NextFeature()) != null)
            {
                //内存监控
                if (Environment.WorkingSet > DCDHelper.MaxMem)
                {
                    GC.Collect();
                }

                if (wo != null)
                    wo.SetText(string.Format("正在融合非法面【{0}】/【{1}】......", processedCount++, cout));

                IGeometry tempShape = tempFe.Shape;

                if (tempShape.IsEmpty || (tempShape as IArea).Area == 0)
                    continue;

                ISpatialFilter sf = new SpatialFilter();
                sf.WhereClause = "";
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                sf.Geometry = tempShape;
                sf.GeometryField = "SHAPE";
                IFeatureCursor pCursor2 = fc.Search(sf, true);

                int maxOID = -1;
                double maxLen = 0;
                IFeature fe2 = null;
                while ((fe2 = pCursor2.NextFeature()) != null)
                {
                    //内存监控
                    if (Environment.WorkingSet > DCDHelper.MaxMem)
                    {
                        GC.Collect();
                    }

                    try
                    {
                        double len = 0;
                        var pGeometryResult = (tempShape as ITopologicalOperator).Intersect(fe2.Shape, esriGeometryDimension.esriGeometry1Dimension);
                        if (pGeometryResult != null)
                        {
                            IGeometryCollection Geos = pGeometryResult as IGeometryCollection;
                            for (int i = 0; i < Geos.GeometryCount; i++)
                            {
                                var path = Geos.get_Geometry(i) as IPath;
                                len += path.Length;
                            }
                        }

                        if (len > maxLen)
                        {
                            maxOID = fe2.OID;
                            maxLen = len;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.Message);
                        System.Diagnostics.Trace.WriteLine(ex.Source);
                        System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                        string err = ex.Message;
                    }

                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor2);

                if (maxOID != -1)
                {
                    if (oid2Geo.ContainsKey(maxOID))//已包含,合并非植被面
                    {
                        IGeometry geo = (tempShape as ITopologicalOperator2).Union(oid2Geo[maxOID]);

                        oid2Geo[maxOID] = geo;
                    }
                    else
                    {
                        oid2Geo.Add(maxOID, tempShape);
                    }
                }

            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pTempCursor);


            IFeatureCursor pCursor = fc.Search(null, false);
            IFeature fe = null;
            while ((fe = pCursor.NextFeature()) != null)
            {
                if (!oid2Geo.ContainsKey(fe.OID))
                    continue;

                //内存监控
                if (Environment.WorkingSet > DCDHelper.MaxMem)
                {
                    GC.Collect();
                }

                if (wo != null)
                    wo.SetText(string.Format("正在处理植被面【{0}】......", fe.OID));

                fe.Shape = (fe.Shape as ITopologicalOperator2).Union(oid2Geo[fe.OID]);
                fe.Store();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);

            //删除临时要素类
            if (temp_fc != null)
            {
                (temp_fc as IDataset).Delete();
            }
        }

        /// <summary>
        /// 获取范围
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        public static IEnvelope CalFeatureClassExtent(IFeatureClass fc)
        {
            IEnvelope env = null;

            IFeatureCursor pCursor = fc.Search(null, true);
            IFeature fe = null;
            while ((fe = pCursor.NextFeature()) != null)
            {
                if (env == null)
                {
                    env = fe.Shape.Envelope;
                }
                else
                {
                    env.Union(fe.Shape.Envelope);
                }
            }
            Marshal.ReleaseComObject(pCursor);

            return env;
        }

        /// <summary>
        /// 获取子块范围
        /// </summary>
        /// <param name="totalEnv"></param>
        /// <param name="tileRowNum"></param>
        /// <param name="tileColumnNum"></param>
        /// <param name="index">从0开始</param>
        /// <returns></returns>
        public static IEnvelope getSubEnvelope(IEnvelope totalEnv, int tileRowNum, int tileColumnNum, int index)
        {
            IEnvelope result = (totalEnv as IClone).Clone() as IEnvelope;

            double width = totalEnv.Width / tileColumnNum;
            double height = totalEnv.Height / tileRowNum;

            int rowIndex = 1 + (index) / tileColumnNum;//从1开始
            int columnIndex = 1 + (index) % tileColumnNum;//从1开始


            IPoint llPoint = totalEnv.LowerLeft;

            result.XMin = llPoint.X + width * (columnIndex - 1);
            result.XMax = llPoint.X + width * columnIndex;

            result.YMin = llPoint.Y + height * (rowIndex - 1);
            result.YMax = llPoint.Y + height * rowIndex;


            return result;
        }

        /// <summary>
        /// 复制与targetFeatureClss要素相邻的inFeatureClss要素到targetFeatureClss
        /// </summary>
        /// <param name="inFeatureClss"></param>
        /// <param name="targetFeatureClss"></param>
        /// <returns></returns>
        public static List<int> CopyFeatureOfInter(IFeatureClass inFeatureClss, IFeatureClass targetFeatureClss)
        {
            //找出inFeatureClss中所有与targetFeatureClss要素相交的要素OID集合
            List<int> oidList = new List<int>();

            int guidIndex = inFeatureClss.FindField(cmdUpdateRecord.CollabGUID);

            IFeatureCursor pTargetCursor = targetFeatureClss.Search(null, true);
            IFeature targetFe = null;
            while ((targetFe = pTargetCursor.NextFeature()) != null)
            {
                //内存监控
                if (Environment.WorkingSet > DCDHelper.MaxMem)
                {
                    GC.Collect();
                }

                ISpatialFilter sf = new SpatialFilter();
                if (oidList.Count > 0)
                {
                    string oidSet = "";
                    foreach (var oid in oidList)
                    {
                        if (oidSet != "")
                            oidSet += string.Format(",{0}", oid);
                        else
                            oidSet = string.Format("{0}", oid);
                    }

                    sf.WhereClause = string.Format("OBJECTID not in ({0})", oidSet);
                }
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                sf.Geometry = targetFe.Shape.Envelope;

                IFeatureCursor pCusor = inFeatureClss.Search(sf, true);
                IFeature fe = null;
                while ((fe = pCusor.NextFeature()) != null)
                {
                    if (!oidList.Contains(fe.OID))
                    {
                        oidList.Add(fe.OID);
                    }
                }
                Marshal.ReleaseComObject(pCusor);

            }
            Marshal.ReleaseComObject(pTargetCursor);

            //复制要素
            if (oidList.Count > 0)
            {
                string oidSet = "";
                foreach (var oid in oidList)
                {
                    if (oidSet != "")
                        oidSet += string.Format(",{0}", oid);
                    else
                        oidSet = string.Format("{0}", oid);
                }

                string filter = string.Format("OBJECTID in ({0})", oidSet);

                CopyFeaturesToFeatureClass(inFeatureClss, new QueryFilterClass() { WhereClause = filter }, targetFeatureClss, false);
            }

            return oidList;

        }

        /// <summary>
        /// 将要素类fc中的非法面合并到融合为其临近的合法面
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="illegalFeQF"></param>
        /// <param name="ws"></param>
        /// <param name="newFCName"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public static IFeatureClass EliminateFeature(IFeatureClass fc, IQueryFilter illegalFeQF, IWorkspace ws, string newFCName, WaitOperation wo = null)
        {
            Geoprocessor gp = new Geoprocessor();
            gp.OverwriteOutput = true;
            gp.SetEnvironmentValue("workspace", ws.PathName);

            ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer pMakeFeatureLayer = new ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer();
            pMakeFeatureLayer.in_features = fc;
            pMakeFeatureLayer.out_layer = fc.AliasName + "_Layer";
            SMGI.Common.Helper.ExecuteGPTool(gp, pMakeFeatureLayer, null);

            ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute pSelectLayerByAttribute = new ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute();
            pSelectLayerByAttribute.in_layer_or_view = fc.AliasName + "_Layer";
            pSelectLayerByAttribute.where_clause = illegalFeQF.WhereClause;
            SMGI.Common.Helper.ExecuteGPTool(gp, pSelectLayerByAttribute, null);


            Geoprocessor gp2 = new Geoprocessor();
            gp2.OverwriteOutput = true;

            ESRI.ArcGIS.DataManagementTools.Eliminate eli = new ESRI.ArcGIS.DataManagementTools.Eliminate();
            eli.in_features = fc.AliasName + "_Layer";
            eli.out_feature_class = ws.PathName + "\\" + newFCName;

            SMGI.Common.Helper.ExecuteGPTool(gp2, eli, null);

            IFeatureWorkspace fws = ws as IFeatureWorkspace;
            IFeatureClass result = fws.OpenFeatureClass(newFCName);

            //删除非法面(没有和任何面邻近)
            ITable pTable = result as ITable;
            pTable.DeleteSearchedRows(illegalFeQF);

            return result;
        }

        /// <summary>
        /// 将要素类inFeatureClss中指定的要素复制到目标要素类中targetFeatureClss,并删除原要素类中复制了的要素
        /// </summary>
        /// <param name="inFeatureClss"></param>
        /// <param name="targetFeatureClss"></param>
        /// <param name="env"></param>
        /// <param name="bRemoveCollaValue"></param>
        public static void CopyFeaturesToFeatureClass(IFeatureClass inFeatureClss, IFeatureClass targetFeatureClss, IEnvelope env, bool bRemoveCollaValue = true)
        {
            if (inFeatureClss.ShapeType != targetFeatureClss.ShapeType)
            {
                return;
            }

            bool bProjection = false;
            ISpatialReference in_sr = (inFeatureClss as IGeoDataset).SpatialReference;
            ISpatialReference target_sr = (targetFeatureClss as IGeoDataset).SpatialReference;
            if (in_sr.Name != target_sr.Name)
            {
                bProjection = true;
            }

            List<int> delOIDList = new List<int>();

            IFeatureClassLoad pFCLoad = targetFeatureClss as IFeatureClassLoad;
            pFCLoad.LoadOnlyMode = true;

            IFeatureCursor pTargetFeatureCursor = targetFeatureClss.Insert(true);



            ISpatialFilter sf = new SpatialFilter();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            sf.Geometry = env;
            IFeatureCursor pInFeatureCursor = inFeatureClss.Search(sf, true);
            IFeature pInFeature = null;

            try
            {
                while ((pInFeature = pInFeatureCursor.NextFeature()) != null)
                {
                    IFeatureBuffer pFeatureBuffer = targetFeatureClss.CreateFeatureBuffer();

                    IGeometry shape = pInFeature.ShapeCopy;
                    if (bProjection)
                        shape.Project(target_sr);//投影变换

                    pFeatureBuffer.Shape = shape;
                    for (int i = 0; i < pFeatureBuffer.Fields.FieldCount; i++)
                    {
                        IField pfield = pFeatureBuffer.Fields.get_Field(i);
                        if (pfield.Type == esriFieldType.esriFieldTypeGeometry || pfield.Type == esriFieldType.esriFieldTypeOID)
                            continue;

                        if (pfield.Name.ToUpper() == "SHAPE_LENGTH" || pfield.Name.ToUpper() == "SHAPE_AREA")
                            continue;

                        if (bRemoveCollaValue)
                        {
                            if (pfield.Name.ToUpper() == cmdUpdateRecord.CollabGUID || pfield.Name.ToUpper() == cmdUpdateRecord.CollabVERSION ||
                                pfield.Name.ToUpper() == cmdUpdateRecord.CollabDELSTATE || pfield.Name.ToUpper() == cmdUpdateRecord.CollabOPUSER)
                                continue;
                        }

                        //复制属性值
                        int index = pInFeature.Fields.FindField(pfield.Name);
                        if (index != -1 && pfield.Editable)
                        {
                            pFeatureBuffer.set_Value(i, pInFeature.get_Value(index));
                        }

                    }
                    pTargetFeatureCursor.InsertFeature(pFeatureBuffer);

                    delOIDList.Add(pInFeature.OID);

                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
            }
            pTargetFeatureCursor.Flush();

            System.Runtime.InteropServices.Marshal.ReleaseComObject(pTargetFeatureCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pInFeatureCursor);

            pFCLoad.LoadOnlyMode = false;

            //删除已复制过的要素
            if (delOIDList.Count > 0)
            {
                string oidSet = "";
                foreach (var oid in delOIDList)
                {
                    if (oidSet != "")
                        oidSet += string.Format(",{0}", oid);
                    else
                        oidSet = string.Format("{0}", oid);
                }

                string filter = string.Format("OBJECTID in ({0})", oidSet);

                (inFeatureClss as ITable).DeleteSearchedRows(new QueryFilterClass() { WhereClause = filter });
            }
        }

        /// <summary>
        /// 将要素类fc中的非法面合并到融合为其临近的合法面
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="illegalQF"></param>
        /// <param name="ws"></param>
        /// <param name="bSpit">是否分割为多个子块执行</param>
        /// <param name="splitSpace">子块宽度</param>
        /// <param name="wo"></param>
        public static void EliminateIllegalFeature(IFeatureClass fc, IQueryFilter illegalQF, IWorkspace ws, bool bSpit = false, double splitSpace = 0)
        {
            //将非法面复制到一个新的临时图层
            IFeatureClass temp_fc = DCDHelper.CreateFeatureClassStructToWorkspace(ws as IFeatureWorkspace, fc, "temp_illegal");
            DCDHelper.CopyFeaturesToFeatureClass(fc, illegalQF, temp_fc, false);


            //删除非非法面
            ITable pTable = fc as ITable;
            pTable.DeleteSearchedRows(illegalQF);

            int tileRowNum = 1;
            int tileColNum = 1;
            //计算图层范围
            IEnvelope illegalEnv = CalFeatureClassExtent(temp_fc);
            if (bSpit)
            {
                //分块融合
                tileRowNum = (int)Math.Ceiling(illegalEnv.Height / splitSpace);
                tileColNum = (int)Math.Ceiling(illegalEnv.Width / splitSpace);
            }

            int index = 0;
            while (0 != temp_fc.FeatureCount(null))
            {
                //获取当前子块的范围
                IEnvelope subEnv = getSubEnvelope(illegalEnv, tileRowNum, tileColNum, index);

                //创建新的临时要素类
                IFeatureClass newfc = DCDHelper.CreateFeatureClassStructToWorkspace(ws as IFeatureWorkspace, fc, fc.AliasName + "_EliminateTemp" + index.ToString());

                //复制非法要素类到新要素类中
                CopyFeaturesToFeatureClass(temp_fc, newfc, subEnv, false);

                //复制植被面中与新要素类中要素相交的所有要素
                List<int> oidList = CopyFeatureOfInter(fc, newfc);
                if (oidList.Count > 0)
                {
                    //融合
                    IFeatureClass tempFC = EliminateFeature(newfc, illegalQF, ws, fc.AliasName + "_Eliminate" + index.ToString());

                    #region 更新要素类fc
                    string oidSet = "";
                    foreach (var oid in oidList)
                    {
                        if (oidSet != "")
                            oidSet += string.Format(",{0}", oid);
                        else
                            oidSet = string.Format("{0}", oid);
                    }

                    IQueryFilter tempQF = new QueryFilterClass() { WhereClause = string.Format("OBJECTID in ({0})", oidSet) };

                    //删除要素
                    (fc as ITable).DeleteSearchedRows(tempQF);

                    //复制要素
                    CopyFeaturesToFeatureClass(tempFC, null, fc, false);
                    #endregion

                    //删除临时要素类
                    if (tempFC != null)
                    {
                        (tempFC as IDataset).Delete();
                    }
                }

                //删除临时要素类
                if (newfc != null)
                {
                    (newfc as IDataset).Delete();
                }


                index++;

                //释放内存
                GC.Collect();
            }

            if (temp_fc != null)
            {
                (temp_fc as IDataset).Delete();
            }
        }

        /// <summary>
        /// 基于数据库模板创建数据库（结构:要素数据集、简单要素类、注记要素类）
        /// </summary>
        /// <param name="sourceWorkspace"></param>
        /// <param name="gdbFullFileName"></param>
        /// <returns></returns>
        public static Dictionary<string, IFeatureClass> CreateGDBStruct(IWorkspace sourceWorkspace, string gdbFullFileName)
        {
            Dictionary<string, IFeatureClass> result = new Dictionary<string, IFeatureClass>();

            //创建输出工作空间
            if (System.IO.Directory.Exists(gdbFullFileName))
            {
                System.IO.Directory.Delete(gdbFullFileName, true);
            }
            IWorkspace outputWorkspace = createGDB(gdbFullFileName);

            //创建数据库结构
            IEnumDataset sourceEnumDataset = sourceWorkspace.get_Datasets(esriDatasetType.esriDTAny);
            sourceEnumDataset.Reset();
            IDataset sourceDataset = null;
            while ((sourceDataset = sourceEnumDataset.Next()) != null)
            {
                if (sourceDataset is IFeatureDataset)//要素数据集
                {
                    //创建新数据集
                    IFeatureDataset outputFeatureDataset = (outputWorkspace as IFeatureWorkspace).CreateFeatureDataset(sourceDataset.Name, (sourceDataset as IGeoDataset).SpatialReference);

                    //遍历子要素类
                    IFeatureDataset sourceFeatureDataset = sourceDataset as IFeatureDataset;
                    IEnumDataset subSourceEnumDataset = sourceFeatureDataset.Subsets;
                    subSourceEnumDataset.Reset();
                    IDataset subSourceDataset = null;
                    while ((subSourceDataset = subSourceEnumDataset.Next()) != null)
                    {
                        if (subSourceDataset is IFeatureClass)//要素类
                        {
                            #region 创建要素类
                            IFeatureClass fc = subSourceDataset as IFeatureClass;

                            IFields fields = getFeatureClassFields(fc);

                            if (fc.FeatureType == esriFeatureType.esriFTSimple)
                            {
                                IFeatureClass newFC = outputFeatureDataset.CreateFeatureClass(subSourceDataset.Name,
                                    fields, null, null, fc.FeatureType, fc.ShapeFieldName, "");

                                result.Add(subSourceDataset.Name, newFC);
                            }
                            else if (fc.FeatureType == esriFeatureType.esriFTAnnotation)
                            {
                                IAnnoClass annoClass = fc.Extension as IAnnoClass;

                                IObjectClassDescription objClassDesc = new AnnotationFeatureClassDescriptionClass();

                                IGraphicsLayerScale graphicLayerScale = new GraphicsLayerScaleClass();
                                graphicLayerScale.Units = annoClass.ReferenceScaleUnits;
                                graphicLayerScale.ReferenceScale = annoClass.ReferenceScale;

                                IFeatureClass newFC = (outputWorkspace as IFeatureWorkspaceAnno).CreateAnnotationClass(
                                    subSourceDataset.Name, fields, objClassDesc.InstanceCLSID, objClassDesc.ClassExtensionCLSID,
                                    fc.ShapeFieldName, "", outputFeatureDataset, null, annoClass.AnnoProperties, graphicLayerScale,
                                    annoClass.SymbolCollection, false);

                                result.Add(subSourceDataset.Name, newFC);
                            }
                            #endregion
                        }
                    }
                    Marshal.ReleaseComObject(subSourceEnumDataset);
                }
                else if (sourceDataset is IFeatureClass)//要素类
                {
                    #region 创建要素类
                    IFeatureClass fc = sourceDataset as IFeatureClass;

                    IFields fields = getFeatureClassFields(fc);

                    if (fc.FeatureType == esriFeatureType.esriFTSimple)
                    {
                        IFeatureClass newFC = (outputWorkspace as IFeatureWorkspace).CreateFeatureClass(sourceDataset.Name,
                            fields, null, null, fc.FeatureType, fc.ShapeFieldName, "");

                        result.Add(sourceDataset.Name, newFC);
                    }
                    else if (fc.FeatureType == esriFeatureType.esriFTAnnotation)
                    {
                        IAnnoClass annoClass = fc.Extension as IAnnoClass;

                        IObjectClassDescription objClassDesc = new AnnotationFeatureClassDescriptionClass();

                        IGraphicsLayerScale graphicLayerScale = new GraphicsLayerScaleClass();
                        graphicLayerScale.Units = annoClass.ReferenceScaleUnits;
                        graphicLayerScale.ReferenceScale = annoClass.ReferenceScale;

                        IFeatureClass newFC = (outputWorkspace as IFeatureWorkspaceAnno).CreateAnnotationClass(
                            sourceDataset.Name, fields, objClassDesc.InstanceCLSID, objClassDesc.ClassExtensionCLSID,
                            fc.ShapeFieldName, "", null, null, annoClass.AnnoProperties, graphicLayerScale,
                            annoClass.SymbolCollection, false);

                        result.Add(sourceDataset.Name, newFC);
                    }

                    #endregion
                }
            }
            Marshal.ReleaseComObject(sourceEnumDataset);

            return result;
        }

        /// <summary>
        /// 复制模板数据库(结构+数据)
        /// </summary>
        /// <param name="originWorkspace"></param>
        /// <param name="gdbFullFileName"></param>
        /// <returns></returns>
        public static IWorkspace CopyDatabaseStruct(IWorkspace originWorkspace, string gdbFullFileName)
        {
            Copy copy = new Copy();//复制数据库（包括了数据）
            copy.in_data = originWorkspace.PathName;
            copy.out_data = gdbFullFileName;

            IWorkspace ws = null;
            try
            {
                SMGI.Common.Helper.ExecuteGPTool(GApplication.Application.GPTool, copy, null);

                IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactoryClass();
                ws = workspaceFactory.OpenFromFile(gdbFullFileName, 0);

                Marshal.ReleaseComObject(workspaceFactory);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
            }


            return ws;
        }

        /// <summary>
        /// 创建GDB数据库
        /// </summary>
        /// <param name="gdbFullFileName"></param>
        /// <returns></returns>
        public static IWorkspace createGDB(string gdbFullFileName)
        {
            int lastSlashIndex = gdbFullFileName.LastIndexOf("\\");
            int lastDotIndex = gdbFullFileName.LastIndexOf(".");

            string path = gdbFullFileName.Substring(0, lastSlashIndex);
            string databasename = gdbFullFileName.Substring(lastSlashIndex + 1, lastDotIndex - lastSlashIndex - 1);

            IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactoryClass();
            IWorkspaceName workspaceName = workspaceFactory.Create(path, databasename, null, 0);
            IName name = workspaceName as IName;
            IWorkspace workspace = name.Open() as IWorkspace;

            Marshal.ReleaseComObject(workspaceFactory);

            return workspace;
        }

        /// <summary>
        /// 获取要素类的字段结构信息
        /// </summary>
        /// <param name="sourceFC"></param>
        /// <param name="bFieldNameUpper"></param>
        /// <returns></returns>
        public static IFields getFeatureClassFields(IFeatureClass sourceFC, bool bFieldNameUpper = false)
        {
            //获取源要素类的字段结构信息
            IFields targetFields = null;
            IObjectClassDescription featureDescription = new FeatureClassDescriptionClass();
            targetFields = featureDescription.RequiredFields; //要素类自带字段

            for (int i = 0; i < sourceFC.Fields.FieldCount; ++i)
            {
                IField field = sourceFC.Fields.get_Field(i);

                if (field.Type == esriFieldType.esriFieldTypeGeometry)
                {
                    (targetFields as IFieldsEdit).set_Field(targetFields.FindFieldByAliasName((featureDescription as IFeatureClassDescription).ShapeFieldName),
                        (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField);

                    continue;
                }

                //剔除面积、长度字段
                if (field == sourceFC.AreaField || field == sourceFC.LengthField)
                {
                    continue;
                }

                if (targetFields.FindField(field.Name) != -1)//已包含该字段（要素类自带字段）
                {
                    continue;
                }

                IField newField = (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField;
                (targetFields as IFieldsEdit).AddField(newField);
            }

            IGeometryDef geometryDef = new GeometryDefClass();
            IGeometryDefEdit geometryDefEdit = geometryDef as IGeometryDefEdit;
            ISpatialReference sr = (sourceFC as IGeoDataset).SpatialReference;
            sr.SetDomain(-450359962737.05, 990359962737.05, -450359962737.05, 990359962737.05);
            geometryDefEdit.SpatialReference_2 = sr;
            for (int i = 0; i < targetFields.FieldCount; i++)
            {
                IField field = targetFields.get_Field(i);
                if (field.Type == esriFieldType.esriFieldTypeOID)
                {
                    IFieldEdit fieldEdit = (IFieldEdit)field;
                    fieldEdit.Name_2 = field.AliasName;
                }

                if (field.Type == esriFieldType.esriFieldTypeGeometry)
                {
                    geometryDefEdit.GeometryType_2 = field.GeometryDef.GeometryType;
                    IFieldEdit fieldEdit = (IFieldEdit)field;
                    fieldEdit.Name_2 = field.AliasName;
                    fieldEdit.GeometryDef_2 = geometryDef;
                    break;
                }
            }

            if (bFieldNameUpper)//转换为大写
            {
                for (int i = 0; i < targetFields.FieldCount; i++)
                {
                    IField field = targetFields.get_Field(i);

                    IFieldEdit2 fieldEdit = field as IFieldEdit2;
                    fieldEdit.Name_2 = field.Name.ToUpper();
                    fieldEdit.AliasName_2 = field.AliasName.ToUpper();
                }

            }

            return targetFields;
        }

        /// <summary>
        /// 导入数据
        /// </summary>
        /// <param name="inFC"></param>
        /// <param name="qf"></param>
        /// <param name="outFC"></param>
        /// <param name="outFN2InFN">目标要素类字段名与源要素类字段名的对应关系</param>
        /// <param name="needConver"></param>
        /// <returns></returns>
        public static bool AppendToFeatureClass(IFeatureClass inFC, IQueryFilter qf, IFeatureClass outFC, Dictionary<string, string> outFN2InFN, bool needConver = false)
        {
            bool bProjection = false;
            ISpatialReference in_sr = (inFC as IGeoDataset).SpatialReference;
            ISpatialReference out_sr = (outFC as IGeoDataset).SpatialReference;
            if (in_sr != null && out_sr != null && in_sr.Name != out_sr.Name)
            {
                bProjection = true;
            }

            IFeatureClassLoad fcLoad = outFC as IFeatureClassLoad;
            fcLoad.LoadOnlyMode = true;

            IFeatureCursor outFeatureCursor = null;
            IFeatureCursor inFeatureCursor = null;
            IFeature inFeature = null;
            try
            {
                //输出要素类
                outFeatureCursor = outFC.Insert(true);


                inFeatureCursor = inFC.Search(qf, true);
                while ((inFeature = inFeatureCursor.NextFeature()) != null)
                {
                    IFeatureBuffer outFeatureBuffer = outFC.CreateFeatureBuffer();

                    //几何赋值
                    IGeometry geo = inFeature.Shape;
                    if (bProjection)//投影变换
                        geo.Project(out_sr);

                    if (needConver)//需要进行几何的转换
                    {
                        if (inFC.ShapeType == esriGeometryType.esriGeometryPolygon && outFC.ShapeType == esriGeometryType.esriGeometryPolyline)//面转线
                        {
                            outFeatureBuffer.Shape = (geo as ITopologicalOperator).Boundary as IPolyline;
                        }
                        else if (inFC.ShapeType == esriGeometryType.esriGeometryPolygon && outFC.ShapeType == esriGeometryType.esriGeometryPoint)//面转点
                        {
                            outFeatureBuffer.Shape = (geo as IArea).LabelPoint;
                        }
                        else if (inFC.ShapeType == esriGeometryType.esriGeometryPolyline && outFC.ShapeType == esriGeometryType.esriGeometryPoint)//线转点
                        {
                            var pt = new PointClass();
                            (geo as ICurve).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, pt);
                            outFeatureBuffer.Shape = pt;
                        }
                        else
                        {
                            //不支持转换，默认直接赋值
                            outFeatureBuffer.Shape = geo;
                        }
                    }
                    else
                    {
                        outFeatureBuffer.Shape = geo;
                    }
                    

                    //属性赋值
                    for (int i = 0; i < outFeatureBuffer.Fields.FieldCount; i++)
                    {
                        IField field = outFeatureBuffer.Fields.get_Field(i);
                        if (!field.Editable)
                            continue;
                        if (field.Type == esriFieldType.esriFieldTypeGeometry || field.Type == esriFieldType.esriFieldTypeOID)
                            continue;
                        if (field.Name.ToUpper() == "SHAPE_LENGTH" || field.Name.ToUpper() == "SHAPE_AREA")
                            continue;

                        string inFN = field.Name;//初始赋值为目标字段名
                        if (outFN2InFN != null && outFN2InFN.ContainsKey(field.Name.ToUpper()))
                        {
                            inFN = outFN2InFN[field.Name.ToUpper()];
                        }
                        int index = inFeature.Fields.FindField(inFN);
                        if (index != -1)
                        {
                            outFeatureBuffer.set_Value(i, inFeature.get_Value(index));
                        }

                    }

                    //注记要素类处理
                    if (inFC.FeatureType == esriFeatureType.esriFTAnnotation)
                    {
                        IAnnotationFeature inAnnoFe = inFeature as IAnnotationFeature;
                        IAnnotationFeature outAnnoFe = outFeatureBuffer as IAnnotationFeature;
                        if (inAnnoFe != null && outAnnoFe != null)
                            outAnnoFe.Annotation = inAnnoFe.Annotation;
                    }

                    outFeatureCursor.InsertFeature(outFeatureBuffer);
                }

                outFeatureCursor.Flush();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                string err = ex.Message;
                if (inFeature != null)
                {
                    err = string.Format("在导入要素类【{0}】中的要素【{1}】时出现错误:", inFC.AliasName, inFeature.OID) + err;
                }
                MessageBox.Show(err);

                return false;
            }
            finally
            {
                if (outFeatureCursor != null)
                    Marshal.ReleaseComObject(outFeatureCursor);

                if (inFeatureCursor != null)
                    Marshal.ReleaseComObject(inFeatureCursor);

                if (fcLoad != null)
                    fcLoad.LoadOnlyMode = false;
            }

            return true;
        }

        /// <summary>
        /// 导入数据
        /// </summary>
        /// <param name="inFC"></param>
        /// <param name="qf"></param>
        /// <param name="outFC"></param>
        /// <param name="outFN2InFN">目标要素类字段名与源要素类字段名的对应关系</param>
        /// <param name="outFN2Value">目标要素类字段名与修改后值的对应关系</param>
        /// <param name="needConver"></param>
        /// <returns></returns>
        public static bool AppendToFeatureClass2(IFeatureClass inFC, IQueryFilter qf, IFeatureClass outFC, Dictionary<string, string> outFN2InFN, Dictionary<string, string> outFN2Value, bool needConver = false)
        {
            bool bProjection = false;
            ISpatialReference in_sr = (inFC as IGeoDataset).SpatialReference;
            ISpatialReference out_sr = (outFC as IGeoDataset).SpatialReference;
            if (in_sr != null && out_sr != null && in_sr.Name != out_sr.Name)
            {
                bProjection = true;
            }

            IFeatureClassLoad fcLoad = outFC as IFeatureClassLoad;
            fcLoad.LoadOnlyMode = true;

            IFeatureCursor outFeatureCursor = null;
            IFeatureCursor inFeatureCursor = null;
            IFeature inFeature = null;
            try
            {
                //输出要素类
                outFeatureCursor = outFC.Insert(true);
                inFeatureCursor = inFC.Search(qf, true);
                while ((inFeature = inFeatureCursor.NextFeature()) != null)
                {
                    IFeatureBuffer outFeatureBuffer = outFC.CreateFeatureBuffer();

                    //几何赋值
                    IGeometry geo = inFeature.Shape;
                    if (bProjection)//投影变换
                        geo.Project(out_sr);

                    if (needConver)//需要进行几何的转换
                    {
                        if (inFC.ShapeType == esriGeometryType.esriGeometryPolygon && outFC.ShapeType == esriGeometryType.esriGeometryPolyline)//面转线
                        {
                            outFeatureBuffer.Shape = (geo as ITopologicalOperator).Boundary as IPolyline;
                        }
                        else if (inFC.ShapeType == esriGeometryType.esriGeometryPolygon && outFC.ShapeType == esriGeometryType.esriGeometryPoint)//面转点
                        {
                            outFeatureBuffer.Shape = (geo as IArea).LabelPoint;
                        }
                        else if (inFC.ShapeType == esriGeometryType.esriGeometryPolyline && outFC.ShapeType == esriGeometryType.esriGeometryPoint)//线转点
                        {
                            var pt = new PointClass();
                            (geo as ICurve).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, pt);
                            outFeatureBuffer.Shape = pt;
                        }
                        else
                        {
                            //不支持转换，默认直接赋值
                            outFeatureBuffer.Shape = geo;
                        }
                    }
                    else
                    {
                        outFeatureBuffer.Shape = geo;
                    }


                    //属性赋值
                    for (int i = 0; i < outFeatureBuffer.Fields.FieldCount; i++)
                    {
                        IField field = outFeatureBuffer.Fields.get_Field(i);
                        if (!field.Editable)
                            continue;
                        if (field.Type == esriFieldType.esriFieldTypeGeometry || field.Type == esriFieldType.esriFieldTypeOID)
                            continue;
                        if (field.Name.ToUpper() == "SHAPE_LENGTH" || field.Name.ToUpper() == "SHAPE_AREA")
                            continue;

                        string inFN = field.Name;//初始赋值为目标字段名
                        if (outFN2InFN != null && outFN2InFN.ContainsKey(field.Name.ToUpper()))
                        {
                            inFN = outFN2InFN[field.Name.ToUpper()];
                        }
                        int index = inFeature.Fields.FindField(inFN);
                        if (index != -1)
                        {
                            if (outFN2Value != null && outFN2Value.ContainsKey(field.Name.ToUpper()))
                            {
                                string outString = outFN2Value[field.Name.ToUpper()];
                                if (field.Type == esriFieldType.esriFieldTypeString)
                                {
                                    outFeatureBuffer.set_Value(i, outString);
                                }
                                else if (field.Type == esriFieldType.esriFieldTypeInteger)
                                {
                                    var value = Int32.Parse(outString);
                                    outFeatureBuffer.set_Value(i, value);
                                }
                                else if (field.Type == esriFieldType.esriFieldTypeSmallInteger)
                                {
                                    var value = Int16.Parse(outString);
                                    outFeatureBuffer.set_Value(i, value);
                                }
                                else if (field.Type == esriFieldType.esriFieldTypeDouble)
                                {
                                    var value = Double.Parse(outString);
                                    outFeatureBuffer.set_Value(i, value);
                                }
                                else if (field.Type == esriFieldType.esriFieldTypeSingle)
                                {
                                    var value = Single.Parse(outString);
                                    outFeatureBuffer.set_Value(i, value);
                                }
                            }
                            else
                            {
                                outFeatureBuffer.set_Value(i, inFeature.get_Value(index));
                            }
                        }

                    }

                    //注记要素类处理
                    if (inFC.FeatureType == esriFeatureType.esriFTAnnotation)
                    {
                        IAnnotationFeature inAnnoFe = inFeature as IAnnotationFeature;
                        IAnnotationFeature outAnnoFe = outFeatureBuffer as IAnnotationFeature;
                        if (inAnnoFe != null && outAnnoFe != null)
                            outAnnoFe.Annotation = inAnnoFe.Annotation;
                    }

                    outFeatureCursor.InsertFeature(outFeatureBuffer);
                }

                outFeatureCursor.Flush();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                string err = ex.Message;
                if (inFeature != null)
                {
                    err = string.Format("在导入要素类【{0}】中的要素【{1}】时出现错误:", inFC.AliasName, inFeature.OID) + err;
                }
                MessageBox.Show(err);

                return false;
            }
            finally
            {
                if (outFeatureCursor != null)
                    Marshal.ReleaseComObject(outFeatureCursor);

                if (inFeatureCursor != null)
                    Marshal.ReleaseComObject(inFeatureCursor);

                if (fcLoad != null)
                    fcLoad.LoadOnlyMode = false;
            }

            return true;
        }

        public static Dictionary<string, IFeatureClass> GetAllFeatureClassFromWorkspace(IFeatureWorkspace ws)
        {
            Dictionary<string, IFeatureClass> result = new Dictionary<string, IFeatureClass>();

            if (null == ws)
                return result;

            IEnumDataset enumDataset = (ws as IWorkspace).get_Datasets(esriDatasetType.esriDTAny);
            enumDataset.Reset();
            IDataset dataset = null;
            while ((dataset = enumDataset.Next()) != null)
            {
                if (dataset is IFeatureDataset)//要素数据集
                {
                    IFeatureDataset feDataset = dataset as IFeatureDataset;
                    IEnumDataset subEnumDataset = feDataset.Subsets;
                    subEnumDataset.Reset();
                    IDataset subDataset = null;
                    while ((subDataset = subEnumDataset.Next()) != null)
                    {
                        if (subDataset is IFeatureClass)//要素类
                        {
                            IFeatureClass fc = subDataset as IFeatureClass;
                            if (fc != null)
                                result.Add(subDataset.Name, fc);
                        }
                    }
                    Marshal.ReleaseComObject(subEnumDataset);
                }
                else if (dataset is IFeatureClass)//要素类
                {
                    IFeatureClass fc = dataset as IFeatureClass;
                    if (fc != null)
                        result.Add(dataset.Name, fc);
                }
                else
                {

                }

            }
            Marshal.ReleaseComObject(enumDataset);


            return result;
        }

        /// <summary>
        /// 裁切数据库
        /// </summary>
        /// <param name="sourceWS"></param>
        /// <param name="outWS">与sourceWS拥有相同结构</param>
        /// <param name="extentGeo"></param>
        public static bool ClipDataBase(IFeatureWorkspace sourceWS, IFeatureWorkspace outWS, IPolygon extentGeo)
        {
            IEnumDataset enumDataset = (sourceWS as IWorkspace).get_Datasets(esriDatasetType.esriDTAny);
            enumDataset.Reset();
            IDataset dataset = null;
            while ((dataset = enumDataset.Next()) != null)
            {
                if (dataset is IFeatureDataset)//要素数据集
                {
                    IFeatureDataset featureDataset = dataset as IFeatureDataset;
                    IEnumDataset subEnumDataset = featureDataset.Subsets;
                    subEnumDataset.Reset();
                    IDataset subDataset = null;
                    while ((subDataset = subEnumDataset.Next()) != null)
                    {
                        if (subDataset is IFeatureClass)//要素类
                        {
                            IFeatureClass fc = subDataset as IFeatureClass;
                            IFeatureClass outFC = outWS.OpenFeatureClass(subDataset.Name);

                            //裁切数据
                            bool res = CopyIntersectFeatures(fc, outFC, extentGeo);
                            if (!res)
                                return res;
                        }
                    }
                    Marshal.ReleaseComObject(subEnumDataset);
                }
                else if (dataset is IFeatureClass)//要素类
                {
                    IFeatureClass fc = dataset as IFeatureClass;
                    IFeatureClass outFC = outWS.OpenFeatureClass(dataset.Name);

                    //裁切数据
                    bool res = CopyIntersectFeatures(fc, outFC, extentGeo);
                    if (!res)
                        return res;
                }
            }
            Marshal.ReleaseComObject(enumDataset);

            return true;
        }

        /// <summary>
        /// 裁切要素类
        /// </summary>
        /// <param name="sourceFC"></param>
        /// <param name="outFC"></param>
        /// <param name="extentGeo"></param>
        /// <returns></returns>
        public static bool CopyIntersectFeatures(IFeatureClass sourceFC, IFeatureClass outFC, IPolygon extentGeo)
        {
            IGeometry clipGeometry_out = (extentGeo as IClone).Clone() as IGeometry;

            bool bProjection = false;
            ISpatialReference in_sr = (sourceFC as IGeoDataset).SpatialReference;
            ISpatialReference out_sr = (outFC as IGeoDataset).SpatialReference;
            if (in_sr != null && out_sr != null && in_sr.Name != out_sr.Name)
            {
                bProjection = true;
            }
            if (clipGeometry_out.SpatialReference != null && out_sr != null &&
                clipGeometry_out.SpatialReference.Name != out_sr.Name)
            {
                clipGeometry_out.Project(out_sr);
            }

            IFeatureCursor outFeCursor = outFC.Insert(true);
            IFeatureBuffer outFeBuffer = outFC.CreateFeatureBuffer();
            esriGeometryDimension dim = esriGeometryDimension.esriGeometryNoDimension;
            switch (sourceFC.ShapeType)
            {
                case esriGeometryType.esriGeometryEnvelope:
                    dim = esriGeometryDimension.esriGeometry25Dimension;
                    break;
                case esriGeometryType.esriGeometryMultipoint:
                case esriGeometryType.esriGeometryPoint:
                    dim = esriGeometryDimension.esriGeometry0Dimension;
                    break;
                case esriGeometryType.esriGeometryPolygon:
                    dim = esriGeometryDimension.esriGeometry2Dimension;
                    break;
                case esriGeometryType.esriGeometryPolyline:
                    dim = esriGeometryDimension.esriGeometry1Dimension;
                    break;
                default:
                    break;
            }


            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = clipGeometry_out;
            sf.GeometryField = sourceFC.ShapeFieldName;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            IFeatureCursor inFeCursor = sourceFC.Search(sf, true);
            IFeature inFe = null;
            try
            {

                while ((inFe = inFeCursor.NextFeature()) != null)
                {
                    IGeometry shape = inFe.Shape;
                    if (bProjection)
                        shape.Project(out_sr);//投影变换

                    ITopologicalOperator topoOperator = clipGeometry_out as ITopologicalOperator;
                    IGeometry geoResult = null;
                    IRelationalOperator RelOperator = clipGeometry_out as IRelationalOperator;
                    if (RelOperator.Contains(shape))//Contains预处理可优化Intersect的速度（Disjoint预处理对Intersect没什么效果）
                    {
                        geoResult = shape;
                    }
                    else
                    {
                        #region 求交
                        geoResult = topoOperator.Intersect(shape, dim);
                        ITopologicalOperator topo = geoResult as ITopologicalOperator;
                        topo.Simplify();
                        if (geoResult == null || geoResult.IsEmpty)
                            continue;//空几何直接跳过

                        switch (sourceFC.ShapeType)
                        {
                            case esriGeometryType.esriGeometryPolygon:
                                {
                                    if ((geoResult as IArea).Area == 0)
                                    {
                                        continue;//面积为0的要素，直接跳过
                                    }

                                    #region 多部件
                                    var po = (IPolygon4)geoResult;
                                    var gc = (IGeometryCollection)po.ConnectedComponentBag;
                                    if (gc.GeometryCount > 1)//多部件?
                                    {

                                    }
                                    #endregion

                                    break;
                                }
                            case esriGeometryType.esriGeometryPolyline:
                                {
                                    if ((geoResult as IPolyline).Length == 0)
                                    {
                                        continue;//长度为0的要素，直接跳过
                                    }

                                    #region 多部件
                                    var gc = (IGeometryCollection)geoResult;
                                    if (gc.GeometryCount > 1)//多部件
                                    {

                                    }
                                    #endregion

                                    break;
                                }
                        }
                        #endregion
                    }

                    outFeBuffer.Shape = geoResult;
                    for (int i = 0; i < outFeBuffer.Fields.FieldCount; i++)
                    {
                        IField field = outFeBuffer.Fields.get_Field(i);
                        if (field.Type == esriFieldType.esriFieldTypeGeometry ||
                            field.Type == esriFieldType.esriFieldTypeOID)
                            continue;

                        if ((outFC.LengthField != null && field.Name == outFC.LengthField.Name) ||
                            (outFC.AreaField != null && field.Name == outFC.AreaField.Name))
                            continue;


                        int index = inFe.Fields.FindField(field.Name);
                        if (index != -1 && field.Editable)
                        {
                            outFeBuffer.set_Value(i, inFe.get_Value(index));
                        }

                    }
                    outFeCursor.InsertFeature(outFeBuffer);
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
            outFeCursor.Flush();

            System.Runtime.InteropServices.Marshal.ReleaseComObject(outFeCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(inFeCursor);

            return true;

        }

        /// <summary>
        /// 删除数据库中的空要素类
        /// </summary>
        /// <param name="fws"></param>
        /// <returns>若所有要素类都被删除，则返回true</returns>
        public static bool DeleteNullFeatureClass(IFeatureWorkspace fws)
        {
            bool result = true;

            IEnumDataset enumDataset = (fws as IWorkspace).get_Datasets(esriDatasetType.esriDTAny);
            enumDataset.Reset();
            IDataset dataset = null;
            while ((dataset = enumDataset.Next()) != null)
            {
                if (dataset is IFeatureDataset)//要素数据集
                {
                    IFeatureDataset featureDataset = dataset as IFeatureDataset;
                    IEnumDataset subEnumDataset = featureDataset.Subsets;
                    subEnumDataset.Reset();
                    IDataset subDataset = null;
                    while ((subDataset = subEnumDataset.Next()) != null)
                    {
                        if (subDataset is IFeatureClass)//要素类
                        {
                            IFeatureClass fc = subDataset as IFeatureClass;
                            if (fc.FeatureCount(null) == 0)
                            {
                                subDataset.Delete();
                            }
                            else
                            {
                                result = false;
                            }

                        }
                    }
                    Marshal.ReleaseComObject(subEnumDataset);
                }
                else if (dataset is IFeatureClass)//要素类
                {
                    IFeatureClass fc = dataset as IFeatureClass;
                    if (fc.FeatureCount(null) == 0)
                    {
                        dataset.Delete();
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            Marshal.ReleaseComObject(enumDataset);


            return result;
        }

        /// <summary>
        /// 创建临时要素类
        /// </summary>
        /// <param name="fws"></param>
        /// <param name="fcName"></param>
        /// <param name="sr"></param>
        /// <param name="geoType"></param>
        /// <param name="orgFCNameIndex"></param>
        /// <param name="orgfidIndx"></param>
        /// <returns></returns>
        public static IFeatureClass CreateFeatureClass(IFeatureWorkspace fws, string fcName, ISpatialReference sr, esriGeometryType geoType, ref int orgFCNameIndex, ref int orgfidIndx)
        {
            if ((fws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fcName))
            {
                IFeatureClass fc = fws.OpenFeatureClass(fcName);
                (fc as IDataset).Delete();
            }

            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
            IFieldsEdit target_fields = ocDescription.RequiredFields as IFieldsEdit;

            //新增几何所属原要素类名称字段
            IField newField = new FieldClass();
            IFieldEdit newFieldEdit = (IFieldEdit)newField;
            newFieldEdit.Name_2 = "org_fcname";
            newFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            newFieldEdit.Length_2 = 16;
            target_fields.AddField(newField);
            //新增几何所属原要素的OID
            newField = new FieldClass();
            newFieldEdit = (IFieldEdit)newField;
            newFieldEdit.Name_2 = "org_fid";
            newFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            target_fields.AddField(newField);


            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = (IWorkspace)fws;
            fieldChecker.Validate(target_fields, out enumFieldError, out validatedFields);

            int shapeFieldIndex = target_fields.FindField(fcDescription.ShapeFieldName);
            IField Shapefield = target_fields.get_Field(shapeFieldIndex);
            IGeometryDef geometryDef = Shapefield.GeometryDef;
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = geoType;
            geometryDefEdit.SpatialReference_2 = sr;

            IFeatureClass newFC = fws.CreateFeatureClass(fcName, target_fields, ocDescription.InstanceCLSID,
                ocDescription.ClassExtensionCLSID, esriFeatureType.esriFTSimple, fcDescription.ShapeFieldName, "");

            orgFCNameIndex = newFC.FindField("org_fcname");
            orgfidIndx = newFC.FindField("org_fid");

            return newFC;

        }

        /// <summary>
        /// 获取几何面的最长中轴线长度
        /// </summary>
        /// <param name="plgShape"></param>
        /// <returns></returns>
        public static double getPolygonCenterLineLen(IPolygon plgShape)
        {
            double len = 0;
            if (plgShape == null || plgShape.IsEmpty)
                return len;

            var clh = new CenterLineHelper();
            var cl = clh.Create(plgShape).Line;
            var gc = (IGeometryCollection)cl;
            for (var i = 0; i < gc.GeometryCount; i++)
            {
                var pl = new PolylineClass();
                pl.AddGeometry(gc.Geometry[i]);

                len += pl.Length;
            }

            return len;
        }
    }
}
