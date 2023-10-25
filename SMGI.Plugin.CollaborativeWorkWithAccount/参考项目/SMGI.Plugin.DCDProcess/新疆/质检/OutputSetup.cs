using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Linq;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using SMGI.Plugin.DCDProcess.DataProcess;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 设置统一输出目录
    /// </summary>
    public class OutputSetup
    {
         

        private static string _dir = "";
        private static string _tdir = "";
        private static readonly string ConfigPath = GApplication.AppDataPath + "\\datacheckdir.xml";

      
        public static string GetDir()
        {
            //if (_tdir != "")
            //    return _tdir;



            XDocument xdoc = null;
            xdoc = File.Exists(ConfigPath) ? XDocument.Load(ConfigPath) : new XDocument(new XElement("Config"));

        Open:
            var xElement = xdoc.Element("Config");
            if (xElement != null)
            {
                var val = xElement.Element("CheckDir");

                if (val != null && Directory.Exists(val.Value))
                {
                    _dir = val.Value;
                }
                else
                {
                    var frm = new FrmOutputDir();
                    frm.SetDir("");
                reOpen:
                    if (frm.ShowDialog() != DialogResult.OK || frm.GetDir().Length < 1)
                        goto reOpen;
                    _dir = frm.GetDir();

                    xElement.Add(new XElement("CheckDir", _dir));
                    xdoc.Save(ConfigPath);
                }
            }
            else
            {
                xdoc = new XDocument(new XElement("Config"));
                goto Open;
            }


            //if (null == GApplication.Application.AppConfig["DataCheckOutputDir"] ||
            //    !Directory.Exists(GApplication.Application.AppConfig["DataCheckOutputDir"].ToString()))
            //{
            //    // GApplication.Application.AppConfig["DataCheckOutputDir"] =Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            //    //提示设置路径
            //    var frm = new FrmOutputDir();
            //    frm.SetDir("");
            //    reOpen:
            //    if (frm.ShowDialog() != DialogResult.OK || frm.GetDir().Length < 1)
            //        goto reOpen;
            //    _dir = frm.GetDir();
            //}
            //else
            //{
            //    _dir = GApplication.Application.AppConfig["DataCheckOutputDir"].ToString();
            //}

            _tdir = _dir + "\\" + DateTime.Now.ToString("yyMMdd_HHmm");
            if (!Directory.Exists(_tdir))
                Directory.CreateDirectory(_tdir);
            return _tdir;

        }
        public static string _getdir()
        {
            if (_dir != "")
                return _dir;

            XDocument xdoc = null;
            xdoc = File.Exists(ConfigPath) ? XDocument.Load(ConfigPath) : new XDocument(new XElement("Config"));
            var xElement = xdoc.Element("Config");
            if (xElement != null)
            {
                var val = xElement.Element("CheckDir");

                if (val != null)
                {
                    if (Directory.Exists(val.Value))
                    {
                        return val.Value;
                    }
                }
                else
                {
                    xElement.Add(new XElement("CheckDir", ""));
                }

            }
            else
            {
                xdoc = new XDocument(new XElement("Config"));

            }
            xdoc.Save(ConfigPath);

            return "";
        }
        public static bool PutDir(string dir)
        {
            _dir = dir;

            var xdoc = XDocument.Load(ConfigPath);
            xdoc.Element("Config").Element("CheckDir").Value = _dir;
            xdoc.Save(ConfigPath);

            _tdir = _dir + "\\" + DateTime.Now.ToString("yyMMyy_hh");
            if (!Directory.Exists(_tdir))
                Directory.CreateDirectory(_tdir);

            return true;
        }

    }


    public class ResultGDBWriter
    {

        private static IWorkspace _pWorkspace = null;

        public static IWorkspace GetVacantGdbWorkspace()
        {
            if (_pWorkspace != null) return _pWorkspace;

            var gdbdir = OutputSetup.GetDir() + "\\Result.gdb";
            if (Directory.Exists(gdbdir))
                Directory.Delete(gdbdir, true);
            else
            {
                Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
                IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance
                    (factoryType);
                workspaceFactory.Create(OutputSetup.GetDir(), "Result.gdb", null, 0);
                _pWorkspace = workspaceFactory.OpenFromFile(gdbdir, 0);
                // InitFeatureClass();

                Marshal.ReleaseComObject(workspaceFactory);

            }

            return _pWorkspace;
        }

        public static IFeatureClass GetPointFC(ISpatialReference spatial)
        { 
            return InitFeatureClass("点", spatial, esriGeometryType.esriGeometryPoint);
        }
        public static IFeatureClass GetLineFC(ISpatialReference spatial)
        {
            return InitFeatureClass("线", spatial, esriGeometryType.esriGeometryPolyline);
        }
        public static IFeatureClass GetAreaFC(ISpatialReference spatial)
        {
            return InitFeatureClass("面", spatial, esriGeometryType.esriGeometryPolygon);
        }
        public static IFeatureClass GetMutiPointFC(ISpatialReference spatial)
        {
            return InitFeatureClass("多点", spatial, esriGeometryType.esriGeometryMultipoint);
        }
        public static IFeatureClass GetTextFC(ISpatialReference spatial)
        {
            return InitFeatureClass("文本", spatial, esriGeometryType.esriGeometryPoint);
        }
        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="errorGeo"></param>
        /// <param name="fieldName2FieldValue"></param>
        public static void addErrorGeometry(IFeatureClass fcls,IFeatureCursor fcursor,IGeometry errorGeo, Dictionary<string, string> fieldName2FieldValue = null)
        {
            IFeatureBuffer featureBuf = fcls.CreateFeatureBuffer();
            if (fieldName2FieldValue != null)
            {
                foreach (var kv in fieldName2FieldValue)
                {
                    string fieldName = kv.Key;
                    string fieldValue = kv.Value;

                    int index = fcls.FindField(fieldName);
                    if (index != -1)
                    {
                        featureBuf.set_Value(index, fieldValue);
                    }
                }
            }
            featureBuf.Shape = errorGeo;

            fcursor.InsertFeature(featureBuf);

        }

        private static IFeatureClass InitFeatureClass(string name, ISpatialReference spatial, esriGeometryType type)
        {

            if (_pWorkspace == null)
                GetVacantGdbWorkspace();
            var bex = (_pWorkspace as IWorkspace2).NameExists[esriDatasetType.esriDTFeatureClass, name];

            if (bex)
            {
              return (_pWorkspace as IFeatureWorkspace).OpenFeatureClass(name);
                
            }
             
            return  CreateCheckResultFeatureClass(_pWorkspace as IFeatureWorkspace, name, spatial, type,
               new Dictionary<string, int> { { "检查内容", 60 }, { "图层", 30 }, { "错误描述", 500 } });

        }

        private static IFeatureClass CreateCheckResultFeatureClass(IFeatureWorkspace featureWorkspace, String shpName, ISpatialReference spatialReference, esriGeometryType geoType, Dictionary<string, int> fieldName2Len)
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

            IFeatureClass featureClass = featureWorkspace.CreateFeatureClass(shpName, fields, ocDescription.InstanceCLSID,
                ocDescription.ClassExtensionCLSID, esriFeatureType.esriFTSimple, fcDescription.ShapeFieldName, "");

            return featureClass;
        }
   
    

    }
}
