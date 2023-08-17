using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.CartographyTools;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataManagementTools;
using System.Windows.Forms;

namespace SMGI.Plugin.DCDProcess
{
    public class BOULConstructBOUA
    {
        /// <summary>
        /// 输出的错误信息文件路径，构面后的boua直接替换服务器中的相应图层（这里没有记录协同状态，包括修改或原先已删除的状态）
        /// </summary>
        public string ErrOutPutFile
        {
            get
            {
                return _errOutPutFile;
            }
        }
        private string _errOutPutFile;

        public string ErrPropPointFile
        {
            get
            {
                return _errPropPointFile;
            }
        }
        private string _errPropPointFile;

        public BOULConstructBOUA()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="boulFC"></param>
        /// <param name="filter"></param>
        /// <param name="rangeFC"></param>
        /// <param name="bouaFC"></param>
        /// <param name="clusterTolerance"></param>
        /// <param name="bProp"></param>
        /// <param name="propFC"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public Tuple<string, IFeatureClass> ConstructBOUA(IFeatureClass boulFC, string filter, IFeatureClass rangeFC, IFeatureClass bouaFC, double clusterTolerance = 0.001, bool bProp = false, IFeatureClass propFC = null, WaitOperation wo = null)
        {
            string err = "";
            _errOutPutFile = "";
            _errPropPointFile = "";

            //创建临时数据库
            string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
            IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);

            IFeatureClass tempLineFC = null;//临时线要素类
            IFeatureClass tempPropFC = null;//临时属性面要素类
            IFeatureClass tempPtFC = null;//属性面要素类转点后的临时点要素类
            IFeatureClass tempBouaFC = null;//线构面后的临时要素类
            IFeatureClass tempPropBouaFC = null;//线构面后的临时要素类（带属性）

            IQueryFilter qf = null;

           
            ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = new Geoprocessor();
            gp.OverwriteOutput = true;
            gp.SetEnvironmentValue("workspace", ws.PathName);

            ShapeFileWriter resultFile = null;
            IFeatureClass xfc = null;//用于返回临时图层
            try
            {
                #region 1.创建临时要素类
                tempLineFC = DCDHelper.CreateFeatureClassStructToWorkspace(ws as IFeatureWorkspace, boulFC, boulFC.AliasName + "_temp");
                if (tempLineFC.HasCollabField())
                {
                    if(filter =="")
                        qf = new QueryFilterClass()
                        {
                            WhereClause = cmdUpdateRecord.CurFeatureFilter
                        };
                    else
                        qf = new QueryFilterClass()
                        {
                            WhereClause = filter + " and " + cmdUpdateRecord.CurFeatureFilter
                        };
                }
                CopyFeaturesToFeatureClass(boulFC, qf, tempLineFC, false);
                if (rangeFC != null)
                {
                    //复制范围面的边界至临时要素类
                    CopyPolygonToPolylineFeatureClass(rangeFC, null, tempLineFC, false);
                }
                #endregion

                #region 面转点
                if (bProp && propFC != null)
                {
                    if (wo != null)
                        wo.SetText("正在面转点......");

                    //临时要素类
                    tempPropFC = DCDHelper.CreateFeatureClassStructToWorkspace(ws as IFeatureWorkspace, propFC, propFC.AliasName + "_prop_temp");
                    if (tempPropFC.HasCollabField())
                    {
                        qf = new QueryFilterClass()
                        {
                            WhereClause = cmdUpdateRecord.CurFeatureFilter
                        };
                    }
                    CopyFeaturesToFeatureClass(propFC, qf, tempPropFC, false);

                    ESRI.ArcGIS.DataManagementTools.FeatureToPoint pFeatureToPoint = new FeatureToPoint();
                    pFeatureToPoint.in_features = tempPropFC;
                    pFeatureToPoint.out_feature_class = tempPropFC.AliasName + "_PolygonToPoint";
                    pFeatureToPoint.point_location = "INSIDE";
                    SMGI.Common.Helper.ExecuteGPTool(gp, pFeatureToPoint, null);

                    tempPtFC = (ws as IFeatureWorkspace).OpenFeatureClass(tempPropFC.AliasName + "_PolygonToPoint");
                }
                #endregion 

                #region 线构面
                if (wo != null)
                    wo.SetText("正在构面......");
                                
                ESRI.ArcGIS.DataManagementTools.FeatureToPolygon pFeatureToPolygon = new FeatureToPolygon();
                pFeatureToPolygon.in_features = tempLineFC.AliasName;
                string outBOUA;
                if(bouaFC!=null)
                    outBOUA = bouaFC.AliasName + "_temp";
                else
                    outBOUA = "bouaXX" + "";
                pFeatureToPolygon.out_feature_class = outBOUA;
                
                SMGI.Common.Helper.ExecuteGPTool(gp, pFeatureToPolygon, null);

                //打开临时要素类
                tempBouaFC = (ws as IFeatureWorkspace).OpenFeatureClass(outBOUA);
                #endregion

                #region 属性关联，包含异常提示
                if (tempPtFC != null)
                {
                    ESRI.ArcGIS.AnalysisTools.SpatialJoin pSpatialJoin = new ESRI.ArcGIS.AnalysisTools.SpatialJoin();
                    pSpatialJoin.target_features = tempBouaFC;
                    pSpatialJoin.join_features = tempPtFC;
                    pSpatialJoin.out_feature_class = tempBouaFC.AliasName + "_Prop";
                    SMGI.Common.Helper.ExecuteGPTool(gp, pSpatialJoin, null);

                    //打开临时要素类
                    tempPropBouaFC = (ws as IFeatureWorkspace).OpenFeatureClass(tempBouaFC.AliasName + "_Prop");

                    //根据字段Join_Count值，判断属性关联的结果
                    if (tempPropBouaFC.FindField("Join_Count") != -1)
                    {
                        int guidindex = tempPropBouaFC.FindField(cmdUpdateRecord.CollabGUID);
                        if (guidindex != -1)
                        {
                            IQueryFilter qf2 = new QueryFilterClass() { WhereClause = "Join_Count <> 1" };
                            if(tempPropBouaFC.FeatureCount(qf2) > 0)
                            {
                                _errOutPutFile = DCDHelper.GetAppDataPath() + "\\境界构面_异常境界面.shp";

                                //创建临时shp文件
                                resultFile = new ShapeFileWriter();
                                Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                                fieldName2Len.Add("异常", 64);
                                resultFile.createErrorResutSHPFile(_errOutPutFile, (tempPropBouaFC as IGeoDataset).SpatialReference, 
                                    esriGeometryType.esriGeometryPolygon, fieldName2Len);

                                List<int> mutiPropPointList = new List<int>();//一个面内包含了多个属性点对象的OID集合

                                IFeatureCursor feCursor = tempPropBouaFC.Search(qf2, true);
                                IFeature fe = null;
                                while ((fe = feCursor.NextFeature()) != null)
                                {
                                    int joinCount = 0;
                                    int.TryParse(fe.get_Value(tempPropBouaFC.FindField("Join_Count")).ToString(), out joinCount);
                                    string exInfo = "";

                                    if(joinCount == 0)
                                    {
                                        exInfo = "该面没有找到对应属性点，属性赋值失败,需人工处理。";
                                    }
                                    else
                                    {
                                        exInfo = string.Format("该面找到的对应属性点数量为【{0}】个，属性赋值结果可能异常,需人工处理。", joinCount);

                                        #region 获取该面内包含了那几个异常属性点对象的OID
                                        ISpatialFilter sf = new SpatialFilterClass();
                                        sf.Geometry = fe.Shape;
                                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                                        IFeatureCursor ptCursor = tempPtFC.Search(sf, true);
                                        IFeature ptFe = null;
                                        while ((ptFe = ptCursor.NextFeature()) != null)
                                        {
                                            if (!mutiPropPointList.Contains(ptFe.OID))
                                            {
                                                mutiPropPointList.Add(ptFe.OID);
                                            }
                                        }
                                        Marshal.ReleaseComObject(ptCursor);
                                        #endregion
                                    }

                                    Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                    fieldName2FieldValue.Add("异常", exInfo);

                                    resultFile.addErrorGeometry(fe.Shape, fieldName2FieldValue);
                                }
                                Marshal.ReleaseComObject(feCursor);


                                resultFile.saveErrorResutSHPFile();

                                if (mutiPropPointList.Count > 0)
                                {
                                    #region 输出异常属性点（1：N）
                                    _errPropPointFile = DCDHelper.GetAppDataPath() + "\\境界构面_异常关联属性点.shp";
                                    //创建临时shp文件
                                    resultFile = new ShapeFileWriter();
                                    Dictionary<string, int> fieldName2Len2 = new Dictionary<string, int>();
                                    fieldName2Len2.Add("属性描述", 64);
                                    resultFile.createErrorResutSHPFile(_errPropPointFile, (tempPropBouaFC as IGeoDataset).SpatialReference,
                                        esriGeometryType.esriGeometryPoint, fieldName2Len2);

                                    int nameIndex = tempPtFC.FindField("NAME");
                                    int guidIndex = tempPtFC.FindField(cmdUpdateRecord.CollabGUID);

                                    string oidSet = "";
                                    foreach (var oid in mutiPropPointList)
                                    {
                                        if (oidSet != "")
                                            oidSet += string.Format(",{0}", oid);
                                        else
                                            oidSet = string.Format("{0}", oid);
                                    }
                                    IFeatureCursor ptCursor = tempPtFC.Search(new QueryFilterClass() { WhereClause = string.Format("OBJECTID in ({0})", oidSet) }, true);
                                    IFeature ptFe = null;
                                    while ((ptFe = ptCursor.NextFeature()) != null)
                                    {
                                        string desc = "";

                                        if (nameIndex != -1)
                                        {
                                            desc += string.Format("name：{0}.", ptFe.get_Value(nameIndex).ToString());
                                        }

                                        if (guidIndex != -1)
                                        {
                                            desc += string.Format("guid：{0}.", ptFe.get_Value(guidIndex).ToString());
                                        }

                                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                        fieldName2FieldValue.Add("属性描述", desc);

                                        resultFile.addErrorGeometry(ptFe.Shape, fieldName2FieldValue);
                                    }
                                    Marshal.ReleaseComObject(ptCursor);

                                    resultFile.saveErrorResutSHPFile();
                                    #endregion
                                }
                            }
                            
                        }
                    }
                }
                #endregion

                if (bouaFC != null)
                {
                    //清空原要素类中的要素
                    (bouaFC as ITable).DeleteSearchedRows(null);
                    //复制
                    if (tempPropBouaFC != null)
                    {
                        CopyFeaturesToFeatureClass(tempPropBouaFC, null, bouaFC, false);
                    }
                    else
                    {
                        CopyFeaturesToFeatureClass(tempBouaFC, null, bouaFC, false);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                err = ex.Message;
            }
            finally
            {
                if (tempLineFC != null)
                {
                    (tempLineFC as IDataset).Delete();
                }

                if (tempPropFC != null)
                {
                    (tempPropFC as IDataset).Delete();
                }

                if (tempPtFC != null)
                {
                    (tempPtFC as IDataset).Delete();
                }

                
                if (bouaFC != null)
                {
                    if (tempBouaFC != null)
                    {
                        (tempBouaFC as IDataset).Delete();
                    }

                    if (tempPropBouaFC != null)
                    {
                        (tempPropBouaFC as IDataset).Delete();
                    }
                }
                else
                {
                    if (tempPropBouaFC != null)
                        xfc = tempPropBouaFC;
                    else if (tempBouaFC != null)
                        xfc = tempBouaFC; 
                }

            }

            
            return new Tuple<string,IFeatureClass>(err,xfc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inFeatureClss"></param>
        /// <param name="qf"></param>
        /// <param name="targetFeatureClss"></param>
        /// <param name="bRemoveCollaValue"></param>
        private void CopyPolygonToPolylineFeatureClass(IFeatureClass inPlyFeatureClss, IQueryFilter qf, IFeatureClass targetPolylineFeatureClss, bool bRemoveCollaValue = true)
        {
            if (inPlyFeatureClss.ShapeType != esriGeometryType.esriGeometryPolygon || targetPolylineFeatureClss.ShapeType != esriGeometryType.esriGeometryPolyline)
            {
                return;
            }

            bool bProjection = false;
            ISpatialReference in_sr = (inPlyFeatureClss as IGeoDataset).SpatialReference;
            ISpatialReference target_sr = (targetPolylineFeatureClss as IGeoDataset).SpatialReference;
            if (in_sr.Name != target_sr.Name)
            {
                bProjection = true;
            }

            IFeatureClassLoad pFCLoad = targetPolylineFeatureClss as IFeatureClassLoad;
            pFCLoad.LoadOnlyMode = true;

            IFeatureCursor pTargetFeatureCursor = targetPolylineFeatureClss.Insert(true);
            

            IFeatureCursor pInFeatureCursor = inPlyFeatureClss.Search(qf, true);
            IFeature pInFeature = null;

            try
            {
                while ((pInFeature = pInFeatureCursor.NextFeature()) != null)
                {
                    IFeatureBuffer pFeatureBuffer = targetPolylineFeatureClss.CreateFeatureBuffer();

                    IGeometry shape = (pInFeature.ShapeCopy as ITopologicalOperator).Boundary as IPolyline;
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
        /// 将要素类inFeatureClss中指定的要素复制到目标要素类中targetFeatureClss
        /// </summary>
        /// <param name="inFeatureClss"></param>
        /// <param name="qf"></param>
        /// <param name="targetFeatureClss"></param>
        /// <param name="bRemoveCollaValue"></param>
        private void CopyFeaturesToFeatureClass(IFeatureClass inFeatureClss, IQueryFilter qf, IFeatureClass targetFeatureClss, bool bRemoveCollaValue = true)
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
                            try
                            {
                                pFeatureBuffer.set_Value(i, pInFeature.get_Value(index));
                            }
                            catch
                            {
                            }
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
    }
}
