using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesFile;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 输出一个点shape文件
    /// </summary>
    public class ShapeFileWriter
    {
        public ShapeFileWriter()
        {
            _errorWorkspace = null;
            _errorFeatureClass = null;
            _errorFeatureCursor = null;
            _errNum = 0;
        }

        #region 成员、属性

        //错误工作空间
        IWorkspace _errorWorkspace;

        //错误点集要素类
        IFeatureClass _errorFeatureClass;

        //错误要素指针
        IFeatureCursor _errorFeatureCursor;

        //错误点数
        int _errNum;
        public int ErrNum
        {
            get
            {
                return _errNum;
            }
        }

        #endregion



        /// <summary>
        /// 创建一个shape文件
        /// </summary>
        /// <param name="fullFileName">D:\\Test\\居民地_点重叠检查.shp</param>
        /// <param name="sr"></param>
        /// <param name="geoType"></param>
        /// <param name="fieldName2Len"></param>
        /// <param name="bOverride"></param>
        /// <returns></returns>
        public bool createErrorResutSHPFile(string fullFileName, ISpatialReference sr, esriGeometryType geoType, Dictionary<string, int> fieldName2Len = null, bool bOverride = true)
        {
            var exteion = System.IO.Path.GetExtension(fullFileName);            
            if (exteion == "")
            {
                fullFileName = fullFileName + ".shp";
            }
            if (System.IO.File.Exists(fullFileName))
            {
                if (!bOverride)
                {
                    if (MessageBox.Show(string.Format("文件【{0}】已经存在,是否直接覆盖？", fullFileName), "提示", MessageBoxButtons.YesNo) == DialogResult.No)
                        return false;
                }
                string path = fullFileName.Substring(0, fullFileName.LastIndexOf("\\"));
                string shpName = System.IO.Path.GetFileNameWithoutExtension(fullFileName);//fullFileName.Substring(fullFileName.LastIndexOf("\\"));
                IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactoryClass();
                IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(path, 0);
                IFeatureClass pFeatureClass;
                string strShapeFile = shpName;
                if (File.Exists(path + "\\" + shpName + ".shp"))
                {
                    pFeatureClass = pFeatureWorkspace.OpenFeatureClass(strShapeFile);
                    IDataset pDataset = (IDataset)pFeatureClass;
                    pDataset.Delete();
                }

                Marshal.ReleaseComObject(pFeatureWorkspace);
                Marshal.ReleaseComObject(pWorkspaceFactory);
                //System.IO.File.Delete(fullFileName);
            }
            
            IWorkspaceFactory pWorkspaceFac = new ShapefileWorkspaceFactoryClass();
            _errorWorkspace = pWorkspaceFac.OpenFromFile(System.IO.Path.GetDirectoryName(fullFileName), 0);
            IFeatureWorkspace pSHPFeatWorkspace = (IFeatureWorkspace)_errorWorkspace;

            if (sr == null)
            {
                sr = new UnknownCoordinateSystemClass();
            }

            _errorFeatureClass = CreateCheckResultFeatureClass(pSHPFeatWorkspace, System.IO.Path.GetFileNameWithoutExtension(fullFileName), sr, geoType, fieldName2Len);


            Marshal.ReleaseComObject(pSHPFeatWorkspace);
            Marshal.ReleaseComObject(pWorkspaceFac);
            return true;
        }


        /// <summary>
        /// 插入错误点
        /// </summary>
        /// <param name="errorGeo"></param>
        /// <param name="fieldName2FieldValue"></param>
        public void addErrorGeometry(IGeometry errorGeo, Dictionary<string,string> fieldName2FieldValue=null)
        {
            if (_errorFeatureClass == null)
                return ;

            if (_errorFeatureCursor == null)
                _errorFeatureCursor = _errorFeatureClass.Insert(true);

            IFeatureBuffer featureBuf = _errorFeatureClass.CreateFeatureBuffer();
            if (fieldName2FieldValue != null)
            {
                foreach (var kv in fieldName2FieldValue)
                {
                    string fieldName = kv.Key;
                    string fieldValue = kv.Value;

                    int index = _errorFeatureClass.FindField(fieldName);
                    if (index != -1)
                    {
                        featureBuf.set_Value(index, fieldValue);
                    }
                }
            }
            featureBuf.Shape = errorGeo;

            _errorFeatureCursor.InsertFeature(featureBuf);

            _errNum++;

            if (0 == _errNum % 5000)
            {
                _errorFeatureCursor.Flush();
            }
        }


        /// <summary>
        /// 插入错误点
        /// </summary>
        /// <param name="errorGeo"></param>
        /// <param name="fieldName2FieldValue"></param>
        public void addCheckWorkbenchErrorGeometry(IGeometry errorGeo, Dictionary<string, string> fieldName2FieldValue = null)
        {
            if (_errorFeatureClass == null)
                return;

            if (_errorFeatureCursor == null)
                _errorFeatureCursor = _errorFeatureClass.Insert(true);

            IFeatureBuffer featureBuf = _errorFeatureClass.CreateFeatureBuffer();
            if (fieldName2FieldValue != null)
            {
                foreach (var kv in fieldName2FieldValue)
                {
                    string fieldName = kv.Key;
                    string fieldValue = kv.Value;

                    int index = _errorFeatureClass.FindField(fieldName);
                    if (index != -1)
                    {
                        featureBuf.set_Value(index, fieldValue);
                    }
                }
            }
            featureBuf.Shape = errorGeo;

            _errorFeatureCursor.InsertFeature(featureBuf);

            _errNum++;

            _errorFeatureCursor.Flush();
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        public void saveErrorResutSHPFile()
        {
            if (_errorFeatureCursor != null && _errNum % 10000 != 0)
                _errorFeatureCursor.Flush();

            if(_errorFeatureCursor != null)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_errorFeatureCursor);
            if (_errorFeatureClass != null)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_errorFeatureClass);
            if (_errorWorkspace != null)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_errorWorkspace);
        }

        /// <summary>
        /// 创建一个检查结果要素类
        /// </summary>
        /// <param name="featureWorkspace"></param>
        /// <param name="shpName"></param>
        /// <param name="spatialReference"></param>
        /// <param name="geoType"></param>
        /// <param name="fieldName2Len"></param>
        /// <returns></returns>
        private IFeatureClass CreateCheckResultFeatureClass(IFeatureWorkspace featureWorkspace, String shpName, ISpatialReference spatialReference, esriGeometryType geoType, Dictionary<string, int> fieldName2Len)
        {
            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
            IFields fields = ocDescription.RequiredFields;
            IFieldsEdit pFieldsEdit = (IFieldsEdit)fields;

            if (fieldName2Len != null)
            {
                foreach (var kv in fieldName2Len)
                {
                    IField ErrTypeField = new FieldClass();
                    IFieldEdit ErrFieldEdit = (IFieldEdit)ErrTypeField;
                    ErrFieldEdit.Name_2 = kv.Key;
                    ErrFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                    ErrFieldEdit.Length_2 = kv.Value;
                    pFieldsEdit.AddField(ErrTypeField);
                }
            }

            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = (IWorkspace)featureWorkspace;
            fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

            int shapeFieldIndex = fields.FindField(fcDescription.ShapeFieldName);
            IField Shapefield = fields.get_Field(shapeFieldIndex);
            IGeometryDef geometryDef = Shapefield.GeometryDef;
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = geoType;
            geometryDefEdit.SpatialReference_2 = spatialReference;

            IFeatureClass featureClass = featureWorkspace.CreateFeatureClass(shpName, fields,ocDescription.InstanceCLSID, 
                ocDescription.ClassExtensionCLSID, esriFeatureType.esriFTSimple, fcDescription.ShapeFieldName, "");

            return featureClass;
        }
   
    
    }
}
