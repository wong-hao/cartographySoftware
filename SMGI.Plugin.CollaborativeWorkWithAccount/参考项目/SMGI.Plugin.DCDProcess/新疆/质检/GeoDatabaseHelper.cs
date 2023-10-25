using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.DCDProcess
{
    class GeoDatabaseHelper
    { 
        #region 基础函数
        //清空Workspace,删除所有数据集
        public static void ClearWorkspace(IWorkspace m_workspace)
        {
            IEnumDataset enumDt = m_workspace.get_Datasets(esriDatasetType.esriDTAny);
            IDataset dt = null;

            while ((dt = enumDt.Next()) != null)
            {
                ISchemaLock schemaLock = (ISchemaLock)dt;
                schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);
                dt.Delete();
                schemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);
            }
        }
        /// <summary>
        /// 创建工作空间
        /// </summary>
        /// <param name="fac">Workspace工厂类</param>
        /// <param name="dir">数据保存路径</param>
        /// <param name="name">数据文件名称</param>
        /// <returns>工作空间变量</returns>
        public static IWorkspace CreateWorkspace(IWorkspaceFactory fac, string dir, string name)
        {
            IWorkspaceName wname = fac.Create(dir, name, null, 0);
            return (wname as ESRI.ArcGIS.esriSystem.IName).Open() as IWorkspace;
        }

        /// <summary>
        /// 基于已有字段在工作空间下创建要素类
        /// </summary>
        /// <param name="ws">目标工作空间</param>
        /// <param name="name">要素类名称</param>
        /// <param name="org_fields">字段几何</param>
        /// <returns></returns>
        public static IFeatureClass CreateFeatureClass(IWorkspace ws, string name, IFields org_fields)
        {
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
                if (target_fields.FindField(field.Name) >= 0)
                {
                    continue;
                }
                IField field_new = (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField;
                (target_fields as IFieldsEdit).AddField(field_new);
            }

            IFeatureWorkspace fws = ws as IFeatureWorkspace;

            System.String strShapeField = string.Empty;

            return fws.CreateFeatureClass(name, target_fields,
                  featureDescription.InstanceCLSID, featureDescription.ClassExtensionCLSID,
                  esriFeatureType.esriFTSimple,
                  (featureDescription as IFeatureClassDescription).ShapeFieldName,
                  string.Empty);
        }


        /// 设置Z值和M值，解决The Geometry has no Z values错误
        /// </summary>
        /// <param name="pF">要素</param>
        /// <param name="pGeo">几何</param>
        public static void SetZValue(IFeatureBuffer pF, IGeometry pGeo)
        {
            int index;
            index = pF.Fields.FindField("Shape");
            IGeometryDef pGeometryDef;
            pGeometryDef = pF.Fields.get_Field(index).GeometryDef as IGeometryDef;
            if (pGeometryDef.HasZ)
            {
                IZAware pZAware = (IZAware)pGeo;
                pZAware.ZAware = true;
                //IZ iz1 = (IZ)pGeo;
                //iz1.SetConstantZ(0);  //将Z值设置为0
                ESRI.ArcGIS.Geometry.IPoint point = (ESRI.ArcGIS.Geometry.IPoint)pGeo;
                point.Z = 0;
            }
            else
            {
                IZAware pZAware = (IZAware)pGeo;
                pZAware.ZAware = false;
            }

            //M值
            if (pGeometryDef.HasM)
            {
                IMAware pMAware = (IMAware)pGeo;
                pMAware.MAware = true;
            }
            else
            {
                IMAware pMAware = (IMAware)pGeo;
                pMAware.MAware = false;

            }
        }


        /// <summary>
        /// 得到一个数据库里包含的featureclass的名称列表
        /// </summary>
        /// <param name="workspace">工作空间</param>
        /// <returns>要素类名称列表</returns>
        public static IList<string> GetFeatureClassNameListFromWorkspace(IWorkspace workspace)
        {
            IList<string> namelist = new List<string>();

            IEnumDataset aEnumDataset = workspace.get_Datasets(esriDatasetType.esriDTFeatureClass);
            IDataset aDataset = aEnumDataset.Next();
            while (aDataset != null)
            {
                IFeatureClass m_featureclass = aDataset as IFeatureClass;
                if (m_featureclass != null)
                {
                    if (m_featureclass.FeatureType != esriFeatureType.esriFTAnnotation)
                    {
                        namelist.Add(aDataset.Name);
                    }
                }

                aDataset = aEnumDataset.Next();
            }

            IEnumDataset bEnumDataset = workspace.get_Datasets(esriDatasetType.esriDTFeatureDataset);
            IDataset bDataset = bEnumDataset.Next();
            while (bDataset != null)
            {
                IEnumDataset cEnumDataset = bDataset.Subsets;
                IDataset cDataset = cEnumDataset.Next();
                while (cDataset != null)
                {
                    IFeatureClass m_featureclass = cDataset as IFeatureClass;
                    if (m_featureclass != null)
                    {
                        if (m_featureclass.FeatureType != esriFeatureType.esriFTAnnotation)
                        {
                            namelist.Add(cDataset.Name);
                        }
                    }
                    cDataset = cEnumDataset.Next();
                }
                bDataset = bEnumDataset.Next();
            }

            return namelist;
        }

        /// <summary>
        /// 创建字段
        /// </summary>
        /// <param name="fCls">要素类</param>
        /// <param name="fieldName">字段名</param>
        public static void AddField(IFeatureClass fCls, string fieldName)
        {
            if (fCls.FindField(fieldName) != -1) { return; }

            IFields pFields = fCls.Fields;
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = fieldName;
            pFieldEdit.AliasName_2 = fieldName;
            pFieldEdit.Length_2 = 50;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fCls.AddField(pField);
        }
        #endregion

        #region 扩展函数
        /// <summary>
        /// 在FeatureDataset下创建要素类
        /// </summary>
        /// <param name="fdt">要素数据集变量</param>
        /// <param name="name">要素类名称</param>
        /// <param name="org_fields">字段集合</param>
        /// <returns>新的要素类</returns>
        public static IFeatureClass CreateFeatureClassWithFeatureDataset(IFeatureDataset fdt, string name, IFields org_fields)
        {
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
                if (target_fields.FindField(field.Name) >= 0)
                {
                    continue;
                }
                IField field_new = (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField;
                (target_fields as IFieldsEdit).AddField(field_new);
            }

            System.String strShapeField = string.Empty;

            return fdt.CreateFeatureClass(name, target_fields,
                  featureDescription.InstanceCLSID, featureDescription.ClassExtensionCLSID,
                  esriFeatureType.esriFTSimple,
                  (featureDescription as IFeatureClassDescription).ShapeFieldName,
                  string.Empty);

        }

        /// <summary>
        /// 向FeatureDataset中添加要素类
        /// </summary>
        /// <param name="fdt">目标要素数据集</param>
        /// <param name="org_fc"></param>
        /// <param name="FeatureClassName"></param>
        /// <param name="qf"></param>
        public static void AddFeatureclassToFeatureDataset(IFeatureDataset fdt, IFeatureClass org_fc, string FeatureClassName, IQueryFilter qf = null)
        {
           // IGeoDataset geoDataset = org_fc as IGeoDataset;
          //  ISpatialReference spatialReference = geoDataset.SpatialReference;
           // spatialReference.SetDomain(-450359962737.05, 990359962737.05, -450359962737.05, 990359962737.05);
                     
        
            IFeatureClassLoad pload = null;
            IFeatureClass featureClass = null;
            IFeatureCursor writeCursor = null;
            string name = FeatureClassName;
            var idx = name.LastIndexOf('.');
            if (idx != -1)
            {
                name = name.Substring(idx + 1);
            }
            else
            {
                name = name.Replace('.', '_');
            }
            try
            {
                IFields expFields;
                expFields = (org_fc.Fields as IClone).Clone() as IFields;
                for (int i = 0; i < expFields.FieldCount; i++)
                {
                    IField field = expFields.get_Field(i);
                    IFieldEdit fieldEdit = field as IFieldEdit;
                    fieldEdit.Name_2 = field.Name.ToUpper();
                }
                featureClass = CreateFeatureClassWithFeatureDataset(fdt, name, expFields);

                pload = featureClass as IFeatureClassLoad;
                if (pload != null)
                    pload.LoadOnlyMode = true;

                if (featureClass != null)
                {
                    (featureClass as IFeatureClassManage).UpdateExtent();
                }
            }
            catch
            {

            }
            finally
            {
                if (writeCursor != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(writeCursor);
                }
                if (pload != null)
                {
                    pload.LoadOnlyMode = false;
                }
            }
        }
        /// <summary>
        /// 向指定工作空间导入要素类的全部数据
        /// </summary>
        /// <param name="ws">目标工作空间</param>
        /// <param name="org_fc">需要导入的要素类</param>
        /// <param name="name">要素类命名</param>
        /// <param name="qf">过滤条件</param>
        public static void ImportFeatureClassToWorkspace(IWorkspace ws, IFeatureClass org_fc, string name, IQueryFilter qf = null)
        {
            IFeatureClassLoad pload = null;
            IFeatureClass target_fc = null;
            IFeatureCursor writeCursor = null;
            string fc_name = name;
            var idx = fc_name.LastIndexOf('.');
            if (idx != -1)
            {
                fc_name = fc_name.Substring(idx + 1);
            }
            else
            {
                fc_name = fc_name.Replace('.', '_');
            }
            try
            {
                IFields expFields;
                expFields = (org_fc.Fields as IClone).Clone() as IFields;
                for (int i = 0; i < expFields.FieldCount; i++)
                {
                    IField field = expFields.get_Field(i);
                    IFieldEdit fieldEdit = field as IFieldEdit;
                    fieldEdit.Name_2 = field.Name.ToUpper();
                }
                target_fc = CreateFeatureClass(ws, fc_name, expFields);

                pload = target_fc as IFeatureClassLoad;
                if (pload != null)
                    pload.LoadOnlyMode = true;

                IFeatureCursor readCursor = org_fc.Search(qf, true);
                writeCursor = target_fc.Insert(true);
                IFeature feature = null;
                while ((feature = readCursor.NextFeature()) != null)
                {
                    IFeatureBuffer fb = target_fc.CreateFeatureBuffer();
                    for (int i = 0; i < fb.Fields.FieldCount; i++)
                    {
                        IField field = fb.Fields.get_Field(i);
                        if (!(field as IFieldEdit).Editable)
                        {
                            continue;
                        }
                        if (field.Type == esriFieldType.esriFieldTypeGeometry)
                        {
                            fb.Shape = feature.ShapeCopy;
                            continue;
                        }
                        fb.set_Value(i, feature.get_Value(feature.Fields.FindField(field.Name)));
                    }
                    writeCursor.InsertFeature(fb);
                }
                writeCursor.Flush();

                if (target_fc != null)
                {
                    (target_fc as IFeatureClassManage).UpdateExtent();
                }
            }
            catch
            {

            }
            finally
            {
                if (writeCursor != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(writeCursor);
                }
                if (pload != null)
                {
                    pload.LoadOnlyMode = false;
                }
            }
        }
        /// <summary>
        /// 向指定工作空间导入要素类模板（无数据，只有表结构）
        /// </summary>
        /// <param name="ws">目标工作空间</param>
        /// <param name="org_fc">原始要素类</param>
        /// <param name="name">目标要素类名称</param>
        /// <param name="qf">过滤条件</param>
        public static void ImportFeatureClassTempete(IWorkspace ws, IFeatureClass org_fc, string name, IQueryFilter qf = null)
        {
            IFeatureClassLoad pload = null;
            IFeatureClass target_fc = null;
            IFeatureCursor writeCursor = null;
            
            //整理要素类命名
            string fc_name = name;
            var idx = fc_name.LastIndexOf('.');
            if (idx != -1)
            {
                fc_name = fc_name.Substring(idx + 1);
            }
            else
            {
                fc_name = fc_name.Replace('.', '_');
            }

            try
            {
                IFields expFields;
                expFields = (org_fc.Fields as IClone).Clone() as IFields;
                for (int i = 0; i < expFields.FieldCount; i++)
                {
                    IField field = expFields.get_Field(i);
                    IFieldEdit fieldEdit = field as IFieldEdit;
                    fieldEdit.Name_2 = field.Name.ToUpper();
                }
                target_fc = CreateFeatureClass(ws, fc_name, expFields);

                pload = target_fc as IFeatureClassLoad;
                if (pload != null)
                    pload.LoadOnlyMode = true;

                if (target_fc != null)
                {
                    (target_fc as IFeatureClassManage).UpdateExtent();
                }
            }
            catch
            {

            }
            finally
            {
                if (writeCursor != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(writeCursor);
                }
                if (pload != null)
                {
                    pload.LoadOnlyMode = false;
                }
            }
        }
        #endregion
    }
}
