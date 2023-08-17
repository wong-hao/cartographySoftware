using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesFile;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using System.IO;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 面重叠检查
    /// </summary>
    public class CheckNoAreaOverlapCmd : SMGICommand
    {
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
            var frm = new CheckLayerSelectForm(m_Application, false, false, true,true);
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Text = "面重叠检查";

            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string outputFileName;
            if (frm.CheckFeatureLayerList.Count > 1)
            {
                outputFileName = OutputSetup.GetDir() + string.Format("\\{0}.shp", frm.Text);
            }
            else
            {
                outputFileName = OutputSetup.GetDir() + string.Format("\\{0}_{1}.shp", frm.Text, frm.CheckFeatureLayerList.First().Name);
            }


            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                List<IFeatureClass> fcList = new List<IFeatureClass>();
                foreach (var layer in frm.CheckFeatureLayerList)
                {
                    IFeatureClass fc = layer.FeatureClass;
                    if(!fcList.Contains(fc))
                        fcList.Add(fc);
                }

                err = DoCheck(outputFileName, fcList, frm.EnableBetweenLayer, wo);
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
                    MessageBox.Show("检查完毕！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }

        }
        
        /// <summary>
        /// 面重叠检查
        /// </summary>
        /// <param name="resultSHPFileName"></param>
        /// <param name="fcList"></param>
        /// <param name="EnableMutiLayer">是否开启跨图层检查</param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public static string DoCheck(string resultSHPFileName, List<IFeatureClass> fcList, bool EnableMutiLayer = false, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                ShapeFileWriter resultFile = null;

                if (EnableMutiLayer && fcList.Count > 1)
                {
                    #region 跨图层重叠检查
                    string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
                    IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);
                    IFeatureWorkspace fws = ws as IFeatureWorkspace;
                    IFeatureClass temp_fc = null;
                    int orgFCNameIndex = -1;
                    int orgfidIndx = -1;
                    IFeatureClassLoad fcload = null;
                    ISpatialReference sr = null;
                    try
                    {
                        foreach (var fc in fcList)
                        {
                            if (fc.ShapeType != esriGeometryType.esriGeometryPolygon)
                                continue;

                            if (sr == null)
                            {
                                if (wo != null)
                                    wo.SetText(string.Format("正在创建临时要素类......"));

                                //创建一个临时面要素类
                                sr = (fc as IGeoDataset).SpatialReference;
                                temp_fc = CreateFeatureClass(fws, "temp_GraphicConflict", sr, esriGeometryType.esriGeometryPolygon, ref orgFCNameIndex, ref orgfidIndx);
                                fcload = temp_fc as IFeatureClassLoad;
                                fcload.LoadOnlyMode = true;
                            }

                            if (wo != null)
                                wo.SetText(string.Format("正在导入要素类【{0}】......", fc.AliasName));

                            #region 插入要素
                            IFeatureCursor newFeCursor = temp_fc.Insert(true);

                            IQueryFilter qf = new QueryFilterClass();
                            if (fc.HasCollabField())
                                qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                            IFeatureCursor feCursor = fc.Search(qf, true);
                            IFeature fe = null;
                            while ((fe = feCursor.NextFeature()) != null)
                            {
                                IFeatureBuffer newFeBuffer = temp_fc.CreateFeatureBuffer();

                                //几何赋值
                                newFeBuffer.Shape = fe.Shape;
                                //属性赋值
                                newFeBuffer.set_Value(orgFCNameIndex, fc.AliasName);
                                newFeBuffer.set_Value(orgfidIndx, fe.OID);

                                newFeCursor.InsertFeature(newFeBuffer);
                            }
                            newFeCursor.Flush();

                            Marshal.ReleaseComObject(feCursor);
                            Marshal.ReleaseComObject(newFeCursor);
                            #endregion

                        }

                        if (fcload != null)
                        {
                            fcload.LoadOnlyMode = false;
                            fcload = null;
                        }

                        if (wo != null)
                            wo.SetText(string.Format("正在进行面重叠检查......"));

                        Dictionary<IPolygon, KeyValuePair<int, int>> errList = CheckHelper.AreaNoOverlap(temp_fc, null);
                        if (errList.Count > 0)
                        {
                            if (resultFile == null)
                            {
                                //建立结果文件
                                resultFile = new ShapeFileWriter();
                                Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                                fieldName2Len.Add("图层名1", 10);
                                fieldName2Len.Add("要素编号1", 10);
                                fieldName2Len.Add("图层名2", 10);
                                fieldName2Len.Add("要素编号2", 10);
                                fieldName2Len.Add("检查项", 32);
                                resultFile.createErrorResutSHPFile(resultSHPFileName, (temp_fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolygon, fieldName2Len);
                            }

                            //写入结果文件
                            foreach (var kv in errList)
                            {
                                IFeature tempFe1 = temp_fc.GetFeature(kv.Value.Key);
                                IFeature tempFe2 = temp_fc.GetFeature(kv.Value.Value);

                                string fcName1 = tempFe1.get_Value(orgFCNameIndex).ToString();
                                int oid1 = int.Parse(tempFe1.get_Value(orgfidIndx).ToString());
                                string fcName2 = tempFe2.get_Value(orgFCNameIndex).ToString();
                                int oid2 = int.Parse(tempFe2.get_Value(orgfidIndx).ToString());
      
                                Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                fieldName2FieldValue.Add("图层名1", fcName1);
                                fieldName2FieldValue.Add("要素编号1", oid1.ToString());
                                fieldName2FieldValue.Add("图层名2", fcName2);
                                fieldName2FieldValue.Add("要素编号2", oid2.ToString());
                                fieldName2FieldValue.Add("检查项", "面重叠检查");

                                resultFile.addErrorGeometry(kv.Key, fieldName2FieldValue);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    finally
                    {
                        if (fcload != null)
                        {
                            fcload.LoadOnlyMode = false;
                        }

                        if (temp_fc != null)
                        {
                            (temp_fc as IDataset).Delete();
                        }
                    }
                    #endregion
                }
                else
                {

                    foreach (var fc in fcList)
                    {
                        #region 单一图层重叠检查
                        if (fc.ShapeType != esriGeometryType.esriGeometryPolygon)
                            continue;

                        IQueryFilter qf = new QueryFilterClass();
                        if (fc.HasCollabField())
                            qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                        if (wo != null)
                            wo.SetText(string.Format("正在对要素类【{0}】进行面重叠检查......", fc.AliasName));

                        //核查
                        Dictionary<IPolygon, KeyValuePair<int, int>> errList = CheckHelper.AreaNoOverlap(fc, qf);
                        if (errList.Count > 0)
                        {
                            if (resultFile == null)
                            {
                                //建立结果文件
                                resultFile = new ShapeFileWriter();
                                Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                                fieldName2Len.Add("图层名1",10);
                                fieldName2Len.Add("要素编号1", 10);
                                fieldName2Len.Add("图层名2", 10);
                                fieldName2Len.Add("要素编号2", 10);
                                fieldName2Len.Add("检查项", 32);
                                resultFile.createErrorResutSHPFile(resultSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolygon, fieldName2Len);
                            }

                            //写入结果文件
                            foreach (var kv in errList)
                            {
                                int oid1 = kv.Value.Key;
                                int oid2 = kv.Value.Value;

                                Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                fieldName2FieldValue.Add("图层名1", fc.AliasName);
                                fieldName2FieldValue.Add("要素编号1", oid1.ToString());
                                fieldName2FieldValue.Add("图层名2", fc.AliasName);
                                fieldName2FieldValue.Add("要素编号2", oid2.ToString());
                                fieldName2FieldValue.Add("检查项", "面重叠检查");


                                resultFile.addErrorGeometry(kv.Key, fieldName2FieldValue);
                            }
                        }
                        #endregion
                    }
                }

                //保存结果文件
                if(resultFile != null)
                    resultFile.saveErrorResutSHPFile();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                err = ex.Message;
            }

            return err;
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

    }
}
