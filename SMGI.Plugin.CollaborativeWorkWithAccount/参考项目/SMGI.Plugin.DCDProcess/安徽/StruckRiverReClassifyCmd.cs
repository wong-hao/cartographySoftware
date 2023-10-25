using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Data;
using System.IO;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesGDB;
using SMGI.Common;

namespace SMGI.Plugin.DCDProcess.DataProcess
{
    /// <summary>
    /// 水面外结构线改河流更新（安徽）：根据河流面的取舍情况更新结构线的GB属性
    /// 
    /// 当河流面舍去后，该面内的结构线，有HYDC的GB=同HYDC河流的GB，没有HYDC但与高等级河流相连的GB=高等级河流的GB，
    ///                                   没有HYDC且不与任何高等级河流直接相连则报出来
    /// 
    /// 设计原则：对GB为210400,对应的结构线GB进行更新，更新原则如下：
    /// （1）若目标结构线不与任何河流面存在叠置关系时，分以下情况进行处理：
    ///     ①若该结构线的GB为结构线，且HYDC不为空，则在数据库中检索是否存在HYDC相同的河流（非结构线）
    ///                                               若存在，则将之更新为该河流GB；
    ///                                               否则，按字母对照表更新GB;***
    ///     ②若该结构线的GB为结构线，但HYDC为空，则在数据库中检索是否存在与之相邻的河流，
    ///                                               若存在则将之更新为该河流GB，
    ///                                               否则，标识为未更新。
    ///    
    /// </summary>
    public class StruckRiverReClassifyCmd : SMGICommand
    {
        public StruckRiverReClassifyCmd()
        {
            m_caption = "水面外结构线改河流";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                      m_Application.Workspace != null &&
                      m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            IFeatureLayer hydaLayer = null;
            IFeatureLayer hydlLayer = null;
            IFeatureClass hydaFC = null;
            IFeatureClass hydlFC = null;
            string hydlGBFN = "GB";
            string hydlHYDCFN = "HYDC";
            int hydlGBIndex = -1;
            int hydlHYDCIndex = -1;

            #region 获取相关图层及字段位置
            //HYDA
            string fcName = "HYDA";
            hydaLayer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName); //用别名有风险
            })).FirstOrDefault() as IFeatureLayer;
            if (hydaLayer == null)
            {
                MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            hydaFC = hydaLayer.FeatureClass;

            //HYDL
            fcName = "HYDL";
            hydlLayer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);//用别名有风险
            })).FirstOrDefault() as IFeatureLayer;
            if (hydlLayer == null)
            {
                MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            hydlFC = hydlLayer.FeatureClass;
            hydlGBIndex = hydlFC.FindField(hydlGBFN);
            if (hydlGBIndex == -1)
            {
                MessageBox.Show(string.Format("要素类【{0}】中找不到字段【{1}】", fcName, hydlGBFN));
                return;
            }
            hydlHYDCIndex = hydlLayer.FeatureClass.FindField(hydlHYDCFN);
            if (hydlHYDCIndex == -1)
            {
                MessageBox.Show(string.Format("要素类【{0}】中找不到字段【{1}】", fcName, hydlHYDCFN));
                return;
            }

            #endregion
            
            using (var wo = m_Application.SetBusy())
            {
                wo.SetText("1-创建临时数据库");
                #region 创建临时数据库
                IWorkspace tempWS = DCDHelper.createTempWorkspace(DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb");
                Geoprocessor gp = new Geoprocessor();
                gp.OverwriteOutput = true;
                gp.SetEnvironmentValue("workspace", tempWS.PathName);
                #endregion

                wo.SetText("2-擦除分析");
                List<int> oidOfInHYDAList = new List<int>();//与水面外的结构线OID集合
                #region 擦除分析：HYDA vs HYDL-210400
                IMap map = m_Application.MapControl.ActiveView as IMap;
                map.ClearSelection();

                string outFCName = hydlLayer.FeatureClass.AliasName + "_" + hydaLayer.FeatureClass.AliasName + "_erase";
                int c = 0;
                while ((tempWS as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, outFCName))
                {
                    ++c;
                    outFCName += c.ToString();
                }

                MakeFeatureLayer mkFeatureLayer = new MakeFeatureLayer();
                SelectLayerByAttribute selLayerByAttribute = new SelectLayerByAttribute();

                IQueryFilter hydlQF = new QueryFilterClass();
                if (hydlFC.HasCollabField())
                    hydlQF.WhereClause = string.Format("{0} and {1}= 210400 ", cmdUpdateRecord.CurFeatureFilter, hydlGBFN);
                else
                    hydlQF.WhereClause = string.Format("{0} = 210400 ", hydlGBFN);

                mkFeatureLayer.in_features = hydlLayer;
                mkFeatureLayer.out_layer = hydlLayer.FeatureClass.AliasName + "_Layer";
                gp.Execute(mkFeatureLayer, null);
                selLayerByAttribute.in_layer_or_view = hydlLayer.FeatureClass.AliasName + "_Layer";
                selLayerByAttribute.where_clause = hydlQF.WhereClause;  //只要结构线
                gp.Execute(selLayerByAttribute, null);

                mkFeatureLayer.in_features = hydaLayer;
                mkFeatureLayer.out_layer = hydaLayer.FeatureClass.AliasName + "_Layer";
                gp.Execute(mkFeatureLayer, null);
                selLayerByAttribute.in_layer_or_view = hydaLayer.FeatureClass.AliasName + "_Layer";
                if (hydaFC.HasCollabField())
                    selLayerByAttribute.where_clause = cmdUpdateRecord.CurFeatureFilter;
                else
                    selLayerByAttribute.where_clause = "";
                gp.Execute(selLayerByAttribute, null);

                Erase erase = new Erase();
                erase.in_features = hydlLayer.FeatureClass.AliasName + "_Layer";
                erase.erase_features = hydaLayer.FeatureClass.AliasName + "_Layer";
                erase.out_feature_class = outFCName;
                gp.Execute(erase, null);
                #endregion

                wo.SetText("3-初步获取水系外的结构线和交叉的结构线");
                List<int> outHydlOids = new List<int>();
                #region 1-初步获取-位于（包括部分位于）水系面外的结构线
                IFeatureClass temp_eraseFC = (tempWS as IFeatureWorkspace).OpenFeatureClass(outFCName);
                IFeatureCursor feCursor0 = temp_eraseFC.Search(null, false);
                IFeature fe0 = null;
                while ((fe0 = feCursor0.NextFeature()) != null)
                {
                    if ((fe0.ShapeCopy as IPolyline).Length < 0.5) //过短的线，不参与获取
                        continue;

                    SpatialFilter sqf = new SpatialFilterClass();
                    sqf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    sqf.Geometry = fe0.ShapeCopy;
                    sqf.WhereClause = hydlQF.WhereClause;
                    IFeatureCursor feCursor2 = hydlFC.Search(sqf, false);
                    IFeature fe2 = null;
                    while ((fe2 = feCursor2.NextFeature()) != null)
                    {
                        if (outHydlOids.Contains(fe2.OID))
                            continue;
                        else
                            outHydlOids.Add(fe2.OID);
                    }
                    Marshal.ReleaseComObject(feCursor2);
                }
                Marshal.ReleaseComObject(feCursor0);
                #endregion

                wo.SetText("4-准确获取水系外的结构线和交叉的结构线");
                Dictionary<int, string> DealInfo = new Dictionary<int, string>();
                List<int> OIDsHydlOutHyda = new List<int>();
                List<int> OIDsHydlCrossHyda = new List<int>();

                #region 准确获取水系面外的结构线和交叉的结构线
                foreach (var oid in outHydlOids)
                {
                    IFeature feHydl = hydlFC.GetFeature(oid);

                    SpatialFilter sqf = new SpatialFilterClass();
                    sqf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    sqf.Geometry = feHydl.ShapeCopy;
                    sqf.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                    IFeatureCursor hydaCursor = hydaFC.Search(sqf, false);
                    IFeature feHyda = null;
                    IGeometry pgx = null;
                    while ((feHyda = hydaCursor.NextFeature()) != null)
                    {
                        if (pgx == null)
                            pgx = feHyda.ShapeCopy;
                        else
                            pgx = (pgx as ITopologicalOperator).Union(feHyda.ShapeCopy);
                    }
                    Marshal.ReleaseComObject(hydaCursor);

                    //线在面外
                    if (pgx == null)
                        OIDsHydlOutHyda.Add(feHydl.OID);
                    //线面交叉
                    else
                    {
                        IRelationalOperator relationOpx = pgx as IRelationalOperator;
                        if (relationOpx.Contains(feHydl.ShapeCopy))//包含
                        { }
                        else if (relationOpx.Crosses(feHydl.ShapeCopy))
                        {
                            IPolyline pl1 = feHydl.ShapeCopy as IPolyline;
                            IGeometry geo2 = (pl1 as ITopologicalOperator).Difference(pgx);
                            IPolyline pl2 = geo2 as IPolyline;
                            if (pl2.Length < pl1.Length)
                                OIDsHydlCrossHyda.Add(feHydl.OID);
                            else
                                OIDsHydlOutHyda.Add(feHydl.OID);
                        }
                        else
                        {
                            OIDsHydlOutHyda.Add(feHydl.OID);
                        }
                    }
                }
                #endregion
                                

                wo.SetText("5-根据HYDC和上游关系-赋值");
                int changedCount = 0;
                m_Application.EngineEditor.StartOperation();
                #region 递归修改未赋值的水系结构线，结合HYDC和上游属性
                {
                    List<int> changedHydlOids = new List<int>();
                    do
                    {
                        changedHydlOids.Clear();
                        List<IFeature> backHydlList = new List<IFeature>();
                        List<IFeature> frontHydlList = new List<IFeature>();
                        foreach (var oidFeHydl in OIDsHydlOutHyda)
                        {
                            IFeature feHydl = hydlFC.GetFeature(oidFeHydl);
                            string hydc = feHydl.get_Value(hydlHYDCIndex).ToString();
                            //选择相邻的，oid不同的（不选择自身），非结构线的要素
                            SpatialFilter hydlSQF = new SpatialFilterClass();
                            if (hydc == "")
                            {
                                if (hydlFC.HasCollabField())
                                    hydlSQF.WhereClause = string.Format("{0} and {1}<>{2} ", cmdUpdateRecord.CurFeatureFilter, hydlFC.OIDFieldName, oidFeHydl, hydlGBFN);
                                else
                                    hydlSQF.WhereClause = string.Format("{0}<>{1}", hydlFC.OIDFieldName, oidFeHydl, hydlGBFN);

                                //用于选择的几何，线的起点
                                HashSet<int> GBset = new HashSet<int>();
                                IPoint ptFrom = (feHydl.ShapeCopy as IPolyline).FromPoint;
                                hydlSQF.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                hydlSQF.Geometry = ptFrom;
                                ICursor cs = (hydlFC.Search(hydlSQF, false)) as ICursor;
                                IRow row = null;
                                while ((row = cs.NextRow()) != null)
                                {
                                    int gb = int.Parse(row.get_Value(hydlGBIndex).ToString());
                                    GBset.Add(gb);
                                }
                                Marshal.ReleaseComObject(cs);

                                if (GBset.Count == 1)
                                {
                                    int gb = GBset.FirstOrDefault();
                                    if (gb != 210400)
                                    {
                                        feHydl.set_Value(hydlGBIndex, gb);
                                        feHydl.Store();
                                        changedCount += 1;
                                        changedHydlOids.Add(oidFeHydl);
                                        DealInfo.Add(oidFeHydl, string.Format("已处理1-根据上游线,gb={0}", gb));
                                    }
                                }
                                else if (GBset.Count >= 2)
                                {
                                    if (GBset.Contains(210400))
                                    {
                                        GBset.Remove(210400);
                                    }

                                    if(GBset.Count>=2)
                                    {
                                        if (!DealInfo.ContainsKey(oidFeHydl))
                                            DealInfo.Add(oidFeHydl, string.Format("未处理1-上游线不唯一,gbs={0}", string.Join(",", GBset)));
                                    }
                                }
                                else
                                {
                                    if (!DealInfo.ContainsKey(oidFeHydl))
                                        DealInfo.Add(oidFeHydl, string.Format("未处理3-上游无参考GB")); 
                                }
                            }
                            else //有HYDC
                            {
                                if (hydlFC.HasCollabField())
                                    hydlSQF.WhereClause = string.Format("{0} and {1}<>{2} and {3}<>210400 and {4}='{5}'", cmdUpdateRecord.CurFeatureFilter, hydlFC.OIDFieldName, oidFeHydl, hydlGBFN, hydlHYDCFN, hydc);
                                else
                                    hydlSQF.WhereClause = string.Format("{0}<>{1} and {2}<>210400 and {3}='{4}'", hydlFC.OIDFieldName, oidFeHydl, hydlGBFN, hydlHYDCFN, hydc);

                                //用于选择的几何，起始点
                                HashSet<int> GBset = new HashSet<int>();
                                Multipoint mp = new MultipointClass();
                                IPoint ptFrom = (feHydl.ShapeCopy as IPolyline).FromPoint;
                                IPoint ptTo = (feHydl.ShapeCopy as IPolyline).ToPoint;
                                mp.AddPoint(ptFrom);
                                mp.AddPoint(ptTo);
                                hydlSQF.Geometry = mp as IGeometry;
                                hydlSQF.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                ICursor cs = (hydlFC.Search(hydlSQF, false)) as ICursor;
                                IRow row = null;
                                while ((row = cs.NextRow()) != null)
                                {
                                    int gb = int.Parse(row.get_Value(hydlGBIndex).ToString());
                                    GBset.Add(gb);
                                }
                                Marshal.ReleaseComObject(cs);
                                if (GBset.Count == 1)
                                {
                                    int gb = GBset.FirstOrDefault();
                                    feHydl.set_Value(hydlGBIndex, gb);
                                    feHydl.Store();
                                    changedCount += 1;
                                    changedHydlOids.Add(oidFeHydl);
                                    DealInfo.Add(oidFeHydl, string.Format("已处理2-根据同HYDC,gb={0}", gb));
                                }
                                else if (GBset.Count > 1)
                                {
                                    if (!DealInfo.ContainsKey(oidFeHydl))
                                        DealInfo.Add(oidFeHydl, string.Format("未处理2-同HYDC的GB不唯一,gbs={0}", string.Join(",", GBset)));
                                }
                            }
                        }
                        foreach (var oid in DealInfo.Keys)
                        {
                            OIDsHydlOutHyda.Remove(oid);
                        }
                    } while (changedHydlOids.Count > 0);
                }
                #endregion
                
                /*
                wo.SetText("6-根据上下游关系");
                #region 根据上下游(实际下游)关系修改水系GB
                {
                    List<int> changedHydlOids = new List<int>();
                    do
                    {
                        changedHydlOids.Clear();
                        foreach (var oidFeHydl in OIDsHydlOutHyda)
                        {
                            IFeature feHydl = hydlFC.GetFeature(oidFeHydl);
                            //选择相邻的，oid不同的（不选择自身），非结构线的要素
                            SpatialFilter hydlSQF = new SpatialFilterClass();

                            if (hydlFC.HasCollabField())
                                hydlSQF.WhereClause = string.Format("{0} and {1}<>{2} and {3}<>210400", cmdUpdateRecord.CurFeatureFilter, hydlFC.OIDFieldName, oidFeHydl, hydlGBIndex);
                            else
                                hydlSQF.WhereClause = string.Format("{0}<>{1} and {2}<>210400", hydlFC.OIDFieldName, oidFeHydl, hydlGBIndex);

                            //用于选择的几何，起始点
                            HashSet<int> GBset = new HashSet<int>();
                            Multipoint mp = new MultipointClass();
                            IPoint ptFrom = (feHydl.ShapeCopy as IPolyline).FromPoint;
                            IPoint ptTo = (feHydl.ShapeCopy as IPolyline).ToPoint;
                            mp.AddPoint(ptFrom);
                            mp.AddPoint(ptTo);
                            hydlSQF.Geometry = mp as IGeometry;
                            hydlSQF.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                            ICursor cs = (hydlFC.Search(hydlSQF, false)) as ICursor;
                            IRow row = null;
                            while ((row = cs.NextRow()) != null)
                            {
                                int gb = int.Parse(row.get_Value(hydlGBIndex).ToString());
                                GBset.Add(gb);
                            }
                            Marshal.ReleaseComObject(cs);
                            if (GBset.Count == 1)
                            {
                                int gb = GBset.FirstOrDefault();
                                feHydl.set_Value(hydlGBIndex, gb);
                                feHydl.Store();
                                changedCount += 1;
                                changedHydlOids.Add(oidFeHydl);
                                DealInfo.Add(oidFeHydl, string.Format("已处理3-根据上下游（实际下游），需核实,gb={0}", gb));
                            }
                            else if (GBset.Count > 1)
                            {
                                if (!DealInfo.ContainsKey(oidFeHydl))
                                    DealInfo.Add(oidFeHydl, string.Format("未处理3-上下游线不唯一,gbs={0}", string.Join(",", GBset)));
                            }
                        }
                        foreach (var oid in DealInfo.Keys)
                        {
                            OIDsHydlOutHyda.Remove(oid);
                        }
                    } while (changedHydlOids.Count > 0);
                }
                #endregion
                */

                if(changedCount>0)
                    m_Application.EngineEditor.StopOperation("处理水系面外的结构线，修改GB");
                else
                    m_Application.EngineEditor.AbortOperation();

                wo.SetText("7-处理结果");
                //写修改记录、未修改记录
                #region
                if (DealInfo.Count > 0)
                {
                    
                    string unUpdateReusltFileName = DCDHelper.GetAppDataPath() + string.Format("\\结构线HYDL_处理说明_{0}.shp", DateTime.Now.ToString("yyMMdd_HHmmss"));
                    //shp的字段信息
                    ShapeFileWriter resultFile = new ShapeFileWriter();
                    Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                    fieldName2Len.Add("要素编号", 16);
                    fieldName2Len.Add("说明", 255);

                    resultFile.createErrorResutSHPFile(unUpdateReusltFileName, (hydlLayer.FeatureClass as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);

                    foreach (var item in DealInfo)
                    {
                        IFeature fe = hydlFC.GetFeature(item.Key);

                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("要素编号", item.Key.ToString());
                        fieldName2FieldValue.Add("说明", item.Value);
                        resultFile.addErrorGeometry(fe.ShapeCopy, fieldName2FieldValue);
                    }

                    foreach (var oid in OIDsHydlCrossHyda)
                    {
                        IFeature fe = hydlFC.GetFeature(oid);
                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("要素编号", oid.ToString());
                        fieldName2FieldValue.Add("说明", "结构线与面相交");
                        resultFile.addErrorGeometry(fe.ShapeCopy, fieldName2FieldValue);
                    }

                    foreach (var oid in OIDsHydlOutHyda)
                    {
                        IFeature fe = hydlFC.GetFeature(oid);
                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("要素编号", oid.ToString());
                        fieldName2FieldValue.Add("说明", "无法处理的结构线");
                        resultFile.addErrorGeometry(fe.ShapeCopy, fieldName2FieldValue);
                    }

                    resultFile.saveErrorResutSHPFile();
                    string info= "是否加载本次未更新的要素信息？";
                    if (MessageBox.Show(info, "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        IFeatureClass errFC = CheckHelper.OpenSHPFile(unUpdateReusltFileName);
                        CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                    }
                }
                #endregion
            }
        }

    }
}
