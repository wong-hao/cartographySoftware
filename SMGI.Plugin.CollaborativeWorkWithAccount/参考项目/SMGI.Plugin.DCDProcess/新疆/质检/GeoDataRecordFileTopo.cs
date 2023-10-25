using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesFile;
using System.IO;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.DCDProcess
{
    public class GeoDataRecordFileTopo
    {
        IFeatureClass errorFeatureClass = null;
        IFeatureCursor errorFeatureCursor = null;

        public IWorkspace errorWorkspace = null;

        public GeoDataRecordFileTopo()
        {
            // CreateErrorRecordDataFile(dataName, folderPath, spatialReference);
        }
        #region 专用函数
        // 创建记录质检结果的数据库文件
        public IWorkspace CreateErrorRecordDataFileTOPO( string folderPath)
        {
            IWorkspace workspace;
            IWorkspaceFactory pWorkspaceFac = new ESRI.ArcGIS.DataSourcesGDB.FileGDBWorkspaceFactoryClass();
            string mdbName = "\\质检结果.gdb";
            string mdbFullFileName = folderPath + mdbName;
            if (Directory.Exists(mdbFullFileName))
            {
                try
                {
                    File.Delete(mdbFullFileName);
                    workspace = GeoDatabaseHelper.CreateWorkspace(pWorkspaceFac, folderPath, mdbName);
                    errorWorkspace = workspace;
                    return workspace;
                }
                catch
                {
                    workspace = pWorkspaceFac.OpenFromFile(mdbFullFileName, 0);
                    GeoDatabaseHelper.ClearWorkspace(workspace);
                    errorWorkspace = workspace;
                    return workspace;
                }
            }
            else
            {
                workspace = GeoDatabaseHelper.CreateWorkspace(pWorkspaceFac, folderPath, mdbName);
                errorWorkspace = workspace;
                return workspace;
            }
        }
        /// <summary>
        /// 创建dataset
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="dataName"></param>
        /// <param name="spatialReference"></param>
        public void CreateErrorRecordDatasetTOPO(IWorkspace workspace, string dataName, ISpatialReference spatialReference)
        {
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
            try
            {
                featureWorkspace.CreateFeatureDataset(dataName, spatialReference);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                System.Diagnostics.Trace.WriteLine(ex.Message);

            }

        }
        public void CreateErrorRecordDataFile(string dataName, string folderPath, ISpatialReference spatialReference)
        {
            //   spatialReference.SetDomain(-450359962737.05, 990359962737.05, -450359962737.05, 990359962737.05);
            IWorkspace workspace;
            IWorkspaceFactory pWorkspaceFac = new ESRI.ArcGIS.DataSourcesGDB.FileGDBWorkspaceFactoryClass();
            string mdbName = "质检结果.gdb";
            string mdbFullFileName = folderPath + dataName + ".gdb";
            if (Directory.Exists(mdbFullFileName))
            {
                try
                {
                    File.Delete(mdbFullFileName);
                    workspace = GeoDatabaseHelper.CreateWorkspace(pWorkspaceFac, folderPath, mdbName);
                    IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
                    featureWorkspace.CreateFeatureDataset(dataName, spatialReference);
                    errorFeatureClass = CreateErrorRecordFeatureClass("其他检查结果", workspace, spatialReference);

                    errorWorkspace = workspace;
                }
                catch
                {
                    workspace = pWorkspaceFac.OpenFromFile(mdbFullFileName, 0);
                    GeoDatabaseHelper.ClearWorkspace(workspace);
                    IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
                    featureWorkspace.CreateFeatureDataset(dataName, spatialReference);
                    errorFeatureClass = CreateErrorRecordFeatureClass("其他检查结果", workspace, spatialReference);
                    errorWorkspace = workspace;
                }
            }
            else
            {
                workspace = GeoDatabaseHelper.CreateWorkspace(pWorkspaceFac, folderPath, mdbName);
                errorWorkspace = workspace;
                errorFeatureClass = CreateErrorRecordFeatureClass("其他检查结果", workspace, spatialReference);
                IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
                featureWorkspace.CreateFeatureDataset(dataName, spatialReference);
            }
        }

        public IFeatureClass CreateErrorRecordFeatureClass(String shpName, IWorkspace workspace, ISpatialReference spatialReference)
        {
            spatialReference.SetDomain(-450359962737.05, 990359962737.05, -450359962737.05, 990359962737.05);
            //  spatialReference.SetDomain(-990359962737000000000000.05, 990359962737000000000000.05, -990359962737000000000000.05, 990359962737000000000000.05);
            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
            IFields fields = ocDescription.RequiredFields;
            IFieldsEdit pFieldsEdit = (IFieldsEdit)fields;

            IField ErrTypeField = new FieldClass();
            IFieldEdit ErrFieldEdit = (IFieldEdit)ErrTypeField;
            ErrFieldEdit.Name_2 = "Layer";
            ErrFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            ErrFieldEdit.Length_2 = 20;
            pFieldsEdit.AddField(ErrTypeField);

            ErrTypeField = new FieldClass();
            ErrFieldEdit = (IFieldEdit)ErrTypeField;
            ErrFieldEdit.Name_2 = "FeatureOID";
            ErrFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            ErrFieldEdit.Length_2 = 50;
            pFieldsEdit.AddField(ErrTypeField);

            ErrTypeField = new FieldClass();
            ErrFieldEdit = (IFieldEdit)ErrTypeField;
            ErrFieldEdit.Name_2 = cmdUpdateRecord.CollabVERSION;
            ErrFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            ErrFieldEdit.Length_2 = 50;
            pFieldsEdit.AddField(ErrTypeField);

            ErrTypeField = new FieldClass();
            ErrFieldEdit = (IFieldEdit)ErrTypeField;
            ErrFieldEdit.Name_2 = cmdUpdateRecord.CollabOPUSER;
            ErrFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            ErrFieldEdit.Length_2 = 50;
            pFieldsEdit.AddField(ErrTypeField);

            ErrTypeField = new FieldClass();
            ErrFieldEdit = (IFieldEdit)ErrTypeField;
            ErrFieldEdit.Name_2 = "ErrorType";
            ErrFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            ErrFieldEdit.Length_2 = 50;
            pFieldsEdit.AddField(ErrTypeField);

            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = workspace;
            fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

            int shapeFieldIndex = fields.FindField(fcDescription.ShapeFieldName);
            IField Shapefield = fields.get_Field(shapeFieldIndex);
            IGeometryDef geometryDef = Shapefield.GeometryDef;
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;

            //修改XY分辨率与元数据一致
            //ISpatialReferenceResolution spResolution = spatialReference as ISpatialReferenceResolution;
            //   spResolution.set_XYResolution(false, 0.000000001);
            geometryDefEdit.SpatialReference_2 = spatialReference;

            IFeatureClass featureClass = GeoDatabaseHelper.CreateFeatureClass(workspace, shpName, fields);
            return featureClass;
        }


        /// <summary>
        /// 向记录质检结果的要素类中添加质检结果
        /// </summary>
        /// <param name="ErrorPoint">问题点</param>
        /// <param name="LayerName">对应图层名</param>
        /// <param name="OID">对应要素的OID</param>
        /// <param name="ErrorType">错误类型描述</param>
        public void AdderrorPoint(ESRI.ArcGIS.Geometry.IPoint ErrorPoint, string LayerName, string OID, string ErrorType)
        {

            string[] FieldNames = { "Layer", "OID", "ErrorType" };
            int FieldIndex1 = errorFeatureClass.FindField("Layer");
            int FieldIndex2 = errorFeatureClass.FindField("FeatureOID");
            int FieldIndex3 = errorFeatureClass.FindField("ErrorType");

            IFeatureBuffer featureBuf = errorFeatureClass.CreateFeatureBuffer();
            //   SetZValue(featureBuf, ErrorPoint); 
            //  errorFeatureClass.get;

            featureBuf.Shape = ErrorPoint;
            featureBuf.set_Value(FieldIndex1, LayerName);
            featureBuf.set_Value(FieldIndex2, OID);
            featureBuf.set_Value(FieldIndex3, ErrorType);
            errorFeatureCursor.InsertFeature(featureBuf);

        }
        public void AdderrorPoint1(ESRI.ArcGIS.Geometry.IPoint ErrorPoint, string LayerName, string OID, string ErrorType)
        {

            string[] FieldNames = { "Layer", "OID", cmdUpdateRecord.CollabVERSION, cmdUpdateRecord.CollabOPUSER, "ErrorType" };
            int FieldIndex1 = errorFeatureClass.FindField("Layer");
            int FieldIndex2 = errorFeatureClass.FindField("FeatureOID");
            int FieldIndex3 = errorFeatureClass.FindField(cmdUpdateRecord.CollabVERSION);
            int FieldIndex4 = errorFeatureClass.FindField(cmdUpdateRecord.CollabOPUSER);
            int FieldIndex5 = errorFeatureClass.FindField("ErrorType");

            IFeatureBuffer featureBuf = errorFeatureClass.CreateFeatureBuffer();
            //   SetZValue(featureBuf, ErrorPoint); 
            //  errorFeatureClass.get;

            featureBuf.Shape = ErrorPoint;
            featureBuf.set_Value(FieldIndex1, LayerName);
            featureBuf.set_Value(FieldIndex2, OID);
            featureBuf.set_Value(FieldIndex5, ErrorType);
            errorFeatureCursor.InsertFeature(featureBuf);

        }




        /// <summary>
        /// 初始化记录质检结果的要素类的动态游标
        /// </summary>
        public void CreateInsertFeatureCursor()
        {
            if (errorFeatureClass == null) return;
            errorFeatureCursor = errorFeatureClass.Insert(true);
        }

        /// <summary>
        /// 释放记录质检结果的要素类及其动态游标
        /// </summary>
        public void ReleaseInsertFeatureCursor()
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(errorFeatureCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(errorFeatureClass);
        }

        /// <summary>
        /// 清空记录质检结果的要素类，并删除其他所有要素类
        /// </summary>
        public void DeleteExistData()
        {
            IList<string> featureclassNameList = new List<string>();
            featureclassNameList = GeoDatabaseHelper.GetFeatureClassNameListFromWorkspace(errorWorkspace);
            IFeatureWorkspace m_featureworkspace = errorWorkspace as IFeatureWorkspace;
            for (int i = 0; i < featureclassNameList.Count; i++)
            {
                if ((errorFeatureClass as IDataset).Name == featureclassNameList[i])
                {
                    IFeatureCursor pCursor = errorFeatureClass.Search(null, false);
                    IFeature pFeature = pCursor.NextFeature();
                    while (pFeature != null)
                    {
                        pFeature.Delete();
                        pFeature = pCursor.NextFeature();
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);
                }
                else
                {
                    IFeatureClass fc = m_featureworkspace.OpenFeatureClass(featureclassNameList[i]);
                    IDataset dt = fc as IDataset;
                    dt.Delete();
                }
            }
        }
        #endregion

        #region 创建、初始化保存问题结果的SHP文件
        public void CreateErrorRecordSHPFile(string dataName, string folderPath)
        {
            IWorkspaceFactory pWorkspaceFac = new ShapefileWorkspaceFactoryClass();
            IFeatureWorkspace pSHPFeatWorkspace = (IFeatureWorkspace)pWorkspaceFac.OpenFromFile(folderPath, 0);
            string ShapeFilePath = folderPath + dataName + "-检查结果.shp";
            if (File.Exists(ShapeFilePath))
            {
                try
                {
                    errorFeatureClass = pSHPFeatWorkspace.OpenFeatureClass(dataName + "-检查结果");
                }
                catch
                {

                }
                if (errorFeatureClass != null)
                {
                    DeleteExistData();
                }
            }
            else
            {
                errorFeatureClass = CreateSHPFeatureClass(dataName + "-检查结果", pSHPFeatWorkspace, new UnknownCoordinateSystemClass());
            }
        }


        public IFeatureClass CreateSHPFeatureClass(String shpName, IFeatureWorkspace featureWorkspace, ISpatialReference spatialReference)
        {

            //spatialReference.SetDomain(-450359962737.05, 450359962737.05, -450359962737.05, 450359962737.05);

            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
            IFields fields = ocDescription.RequiredFields;
            IFieldsEdit pFieldsEdit = (IFieldsEdit)fields;

            IField ErrTypeField = new FieldClass();
            IFieldEdit ErrFieldEdit = (IFieldEdit)ErrTypeField;
            ErrFieldEdit.Name_2 = "Layer";
            ErrFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            ErrFieldEdit.Length_2 = 20;
            pFieldsEdit.AddField(ErrTypeField);

            ErrTypeField = new FieldClass();
            ErrFieldEdit = (IFieldEdit)ErrTypeField;
            ErrFieldEdit.Name_2 = "FeatureOID";
            ErrFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            ErrFieldEdit.Length_2 = 20;
            pFieldsEdit.AddField(ErrTypeField);

            ErrTypeField = new FieldClass();
            ErrFieldEdit = (IFieldEdit)ErrTypeField;
            ErrFieldEdit.Name_2 = "ErrorType";
            ErrFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            ErrFieldEdit.Length_2 = 40;
            pFieldsEdit.AddField(ErrTypeField);

            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = (IWorkspace)featureWorkspace;
            fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

            int shapeFieldIndex = fields.FindField(fcDescription.ShapeFieldName);
            IField Shapefield = fields.get_Field(shapeFieldIndex);
            IGeometryDef geometryDef = Shapefield.GeometryDef;
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
            geometryDefEdit.SpatialReference_2 = spatialReference;

            IFeatureClass featureClass = featureWorkspace.CreateFeatureClass(shpName, fields,
            ocDescription.InstanceCLSID, ocDescription.ClassExtensionCLSID, esriFeatureType.esriFTSimple, fcDescription.ShapeFieldName, "");
            return featureClass;
        }
        #endregion
    }
}
