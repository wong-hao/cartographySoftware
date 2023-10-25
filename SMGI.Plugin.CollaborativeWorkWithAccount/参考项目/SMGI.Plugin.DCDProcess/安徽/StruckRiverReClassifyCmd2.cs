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
    public class StruckRiverReClassifyCmd2 : SMGICommand
    {
        public StruckRiverReClassifyCmd2()
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
            IMap map = m_Application.MapControl.ActiveView as IMap;
            map.ClearSelection();

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

            int xoid = -1;
            //有交叉的List
            List<int> feaCrossOIDList = new List<int>();
            Dictionary<int, IFeature> outStructnoHydcHydlFeaDict = new Dictionary<int, IFeature>();   //无HYDC
            //有HYDC的
            Dictionary<string, Dictionary<int, IFeature>> outStructwithHydcHydlFeaDict = new Dictionary<string, Dictionary<int, IFeature>>();
            //临时HYDC的
            Dictionary<string, Dictionary<int, IFeature>> outStructwithHydcHydlFeaDict2 = new Dictionary<string, Dictionary<int, IFeature>>();
            Dictionary<int, string> oid_Err_Dict = new Dictionary<int, string>();
            Dictionary<int, string> oid_gbDict = new Dictionary<int, string>();
            try
            {
                using (var wo = m_Application.SetBusy(this.Caption))
                {
                    wo.SetText("1-获取水系外的结构线和交叉的结构线");
                   
                    string structHydlSQLnoHYDC = string.Format(" {0}=210400 and ({1} is null or {1}='' )", hydlGBFN, hydlHYDCFN);
                    string hydlnoHYDC = string.Format(" {0}<>210400 and ({1} is null or {1}='' )", hydlGBFN, hydlHYDCFN);

                    wo.SetText("1-获取水系外的结构线和交叉的结构线-1-无HYDC的");
                    #region 无HYDC的水面外结构线
                    {
                        if (hydlFC.HasCollabField())
                        {
                            structHydlSQLnoHYDC += " and " + cmdUpdateRecord.CurFeatureFilter;
                            hydlnoHYDC += " and " + cmdUpdateRecord.CurFeatureFilter;
                        }
                        IQueryFilter structHydlQFnoHYDC = new QueryFilterClass() { WhereClause = structHydlSQLnoHYDC };
                        IFeatureCursor cursorStructNoHydc = hydlFC.Search(structHydlQFnoHYDC, false);
                        IFeature feaL = null;
                        while ((feaL = cursorStructNoHydc.NextFeature()) != null)
                        {
                            ISpatialFilter spQF = new SpatialFilterClass()
                            {
                                Geometry = feaL.Shape,
                                SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects
                            };

                            List<IFeature> feaAList = new List<IFeature>();
                            IFeatureCursor cursorHYDA = hydaFC.Search(spQF, false);
                            IFeature feaA = null;
                            while ((feaA = cursorHYDA.NextFeature()) != null)
                            {
                                feaAList.Add(feaA);
                            }
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursorHYDA);


                            if (feaAList.Count == 0)
                            {
                                outStructnoHydcHydlFeaDict.Add(feaL.OID, feaL);
                            }
                            else
                            {
                                IPolyline pl = feaL.ShapeCopy as IPolyline;
                                foreach (var feaAX in feaAList)
                                {
                                    ITopologicalOperator topoA = feaAX.Shape as ITopologicalOperator;
                                    IGeometry geox = topoA.Intersect(pl, esriGeometryDimension.esriGeometry1Dimension);
                                    pl = (pl as ITopologicalOperator).Difference(geox) as IPolyline;
                                }
                                if (!pl.IsEmpty)
                                {
                                    
                                    if (pl.Length > 0.2)
                                    {
                                        feaCrossOIDList.Add(feaL.OID);
                                        outStructnoHydcHydlFeaDict.Add(feaL.OID, feaL);
                                    }
                                }
                            }
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(cursorStructNoHydc);
                    }
                    #endregion

                   
                    string structHydlSQLwithHYDC = string.Format(" {0}=210400 and ({1} <>'' )", hydlGBFN, hydlHYDCFN);

                    wo.SetText("1-获取水系外的结构线和交叉的结构线-2-有HYDC的");
                    #region 有HYDC的水面外结构线
                    {
                        if (hydlFC.HasCollabField())
                            structHydlSQLwithHYDC += " and " + cmdUpdateRecord.CurFeatureFilter;
                        IQueryFilter structHydlQFwithHYDC = new QueryFilterClass() { WhereClause = structHydlSQLwithHYDC };
                        IFeatureCursor cursorStructWithHydc = hydlFC.Search(structHydlQFwithHYDC, false);
                        IFeature feaL2 = null;
                        while ((feaL2 = cursorStructWithHydc.NextFeature()) != null)
                        {
                            xoid = feaL2.OID;
                            string hydc = feaL2.get_Value(hydlHYDCIndex).ToSafeString();
                            ISpatialFilter spQF = new SpatialFilterClass()
                            {
                                Geometry = feaL2.Shape,
                                SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects
                            };

                            List<IFeature> feaAList = new List<IFeature>();
                            IFeatureCursor cursorHYDA = hydaFC.Search(spQF, false);
                            IFeature feaA = null;
                            while ((feaA = cursorHYDA.NextFeature()) != null)
                            {
                                feaAList.Add(feaA);
                            }
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursorHYDA);

                            if (feaAList.Count == 0)
                            {
                                if (IsHYDCTemp(hydc))
                                {
                                    if (outStructwithHydcHydlFeaDict2.ContainsKey(hydc))
                                        outStructwithHydcHydlFeaDict2[hydc].Add(feaL2.OID, feaL2);
                                    else
                                        outStructwithHydcHydlFeaDict2.Add(hydc, new Dictionary<int, IFeature>() { { feaL2.OID, feaL2 } });
                                }
                                else
                                {
                                    if (outStructwithHydcHydlFeaDict.ContainsKey(hydc))
                                        outStructwithHydcHydlFeaDict[hydc].Add(feaL2.OID, feaL2);
                                    else
                                        outStructwithHydcHydlFeaDict.Add(hydc, new Dictionary<int, IFeature>() { { feaL2.OID, feaL2 } });
                                }
                            }
                            else
                            {
                                IPolyline pl = feaL2.ShapeCopy as IPolyline;
                                foreach (var feaAX in feaAList)
                                {
                                    try
                                    {
                                        ITopologicalOperator topoA = feaAX.Shape as ITopologicalOperator;
                                        IGeometry geox = topoA.Intersect(pl, esriGeometryDimension.esriGeometry1Dimension);
                                        pl = (pl as ITopologicalOperator).Difference(geox) as IPolyline;
                                    }
                                    catch (Exception ex)
                                    {
                                        continue;
                                    }

                                }
                                if (!pl.IsEmpty)
                                {                                    
                                    if (pl.Length > 0.2)
                                    {
                                        feaCrossOIDList.Add(feaL2.OID);
                                        if (IsHYDCTemp(hydc))
                                        {
                                            if (outStructwithHydcHydlFeaDict2.ContainsKey(hydc))
                                                outStructwithHydcHydlFeaDict2[hydc].Add(feaL2.OID, feaL2);
                                            else
                                                outStructwithHydcHydlFeaDict2.Add(hydc, new Dictionary<int, IFeature>() { { feaL2.OID, feaL2 } });
                                        }
                                        else
                                        {
                                            if (outStructwithHydcHydlFeaDict.ContainsKey(hydc))
                                                outStructwithHydcHydlFeaDict[hydc].Add(feaL2.OID, feaL2);
                                            else
                                                outStructwithHydcHydlFeaDict.Add(hydc, new Dictionary<int, IFeature>() { { feaL2.OID, feaL2 } });
                                        }
                                    }
                                }
                            }
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(cursorStructWithHydc);
                    }
                    #endregion

                    
                   
                    wo.SetText("2-分析水系线面关系-1-无HYDC");
                    #region 无HYDC的情况
                    {
                        Dictionary<string, List<string>> ptXDict = null;
                        Dictionary<int, List<int>> plXDict = null;
                        GetLinkGroups(outStructnoHydcHydlFeaDict, out ptXDict, out plXDict);//生成分组

                        foreach (var oidList in plXDict.Values)  //分组遍历
                        {
                            HashSet<string> gbhs = new HashSet<string>();

                            int maxCross = 0;

                            List<string> coreNodeList = new List<string>(); //单头点
                            foreach (var oid in oidList)
                            {
                                string xk1 = string.Format("{0}_{1}", oid, 1);
                                string xk2 = string.Format("{0}_{1}", oid, 2);
                                
                                if (ptXDict.ContainsKey(xk1))
                                {
                                    int nxxx = ptXDict[xk1].Count;
                                    if (nxxx > maxCross)
                                        maxCross = nxxx;
                                    if (nxxx == 1)
                                        coreNodeList.Add(xk1);
                                }
                                if (ptXDict.ContainsKey(xk2))
                                {
                                    int nxxx = ptXDict[xk2].Count;
                                    if (nxxx > maxCross)
                                        maxCross = nxxx;
                                    if (ptXDict[xk2].Count == 1)
                                        coreNodeList.Add(xk2);
                                }
                            }

                            foreach (var coreNodeStr in coreNodeList) //用单头点,逐个搜索
                            {
                                string[] items = coreNodeStr.Split(new char[] { '_' });
                                int oidL = int.Parse(items[0]);
                                IPolyline pl = outStructnoHydcHydlFeaDict[oidL].ShapeCopy as IPolyline;
                                IPoint pt = items[1] == "1" ? pl.FromPoint : pl.ToPoint;

                                ISpatialFilter hydlNoHYDCQF = new SpatialFilterClass()
                                {
                                    WhereClause = hydlnoHYDC,
                                    Geometry = pt,
                                    SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects
                                };

                                HashSet<string> gbhs0 = new HashSet<string>(); //当前节点
                                int nx0 = 0;
                                IFeatureCursor cursorL = hydlFC.Search(hydlNoHYDCQF, false);
                                IFeature feaL = null;
                                while ((feaL = cursorL.NextFeature()) != null)
                                {
                                    string gb = feaL.get_Value(hydlGBIndex).ToString();
                                    gbhs0.Add(gb);
                                    nx0++;
                                }
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursorL);
                                if (nx0 == 1)
                                {
                                    gbhs.Add(gbhs0.First());
                                }
                            }
                            if (gbhs.Count == 1)
                            {
                                string gb = gbhs.ToList().First();
                                foreach (var oid in oidList)
                                {
                                    string gb0 = outStructnoHydcHydlFeaDict[oid].get_Value(hydlGBIndex).ToString();

                                    if (maxCross > 2 && !feaCrossOIDList.Contains(oid))
                                        oid_Err_Dict.Add(oid, string.Format("警告：存在{0}岔口,核实。GB:{1} --> {2}", maxCross, gb0, gb));
                                    else
                                        oid_gbDict.Add(oid, gb);

                                }
                            }
                            else if (gbhs.Count > 1)
                            {
                                string errInfo = "参考GB不唯一：" + string.Join(";", gbhs);
                                foreach (var oid in oidList)
                                {
                                    if(!feaCrossOIDList.Contains(oid))
                                        oid_Err_Dict.Add(oid, errInfo);
                                }
                            }
                            else if (gbhs.Count == 0)
                            {
                                string errInfo = "未找到参考GB";
                                foreach (var oid in oidList)
                                {
                                    if (!feaCrossOIDList.Contains(oid))
                                        oid_Err_Dict.Add(oid, errInfo);
                                }
                            }
                        }

                    }
                    #endregion
                    wo.SetText("2-分析水系线面关系-2-有HYDC");
                    #region 有HYDC(正常)的情况
                    {
                        string hydlSGSQL = string.Format(" {0}<>210400 ", hydlGBFN);
                        foreach (var kv1 in outStructwithHydcHydlFeaDict)
                        {
                            string hydc = kv1.Key;
                            string hydlSQLX = hydlSGSQL + string.Format(" and {0}='{1}'",hydlHYDCFN,hydc);

                            var oidFeaDcit = kv1.Value; //同hydc的要素

                            Dictionary<string, List<string>> ptXDict = null;
                            Dictionary<int, List<int>> plXDict = null;
                            GetLinkGroups(oidFeaDcit, out ptXDict, out plXDict);//生成分组                   

                            foreach (var oidList in plXDict.Values)  //分组遍历
                            {
                                //List<IFeature> feaLinkList = new List<IFeature>(); //相连的同RN的要素的List
                                HashSet<string> gbhs = new HashSet<string>();

                                List<string> coreNodeList = new List<string>(); //单头点
                                foreach (var oid in oidList)
                                {
                                    string xk1 = string.Format("{0}_{1}", oid, 1);
                                    string xk2 = string.Format("{0}_{1}", oid, 2);
                                    if (ptXDict.ContainsKey(xk1))
                                    {
                                        if (ptXDict[xk1].Count == 1)
                                            coreNodeList.Add(xk1);
                                    }
                                    if (ptXDict.ContainsKey(xk2))
                                    {
                                        if (ptXDict[xk2].Count == 1)
                                            coreNodeList.Add(xk2);
                                    }
                                }

                                foreach (var coreNodeStr in coreNodeList) //用单头点,逐个搜索
                                {
                                    string[] items = coreNodeStr.Split(new char[] { '_' });
                                    int oidL = int.Parse(items[0]);
                                    IPolyline pl = oidFeaDcit[oidL].ShapeCopy as IPolyline;
                                    IPoint pt = items[1] == "1" ? pl.FromPoint : pl.ToPoint;

                                    ISpatialFilter hydlSPQF2 = new SpatialFilterClass()
                                    {
                                        WhereClause = hydlSQLX,
                                        Geometry = pt,
                                        SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects
                                    };

                                    IFeatureCursor cursorLHYDC = hydlFC.Search(hydlSPQF2, false);
                                    IFeature feaLHYDC = null;
                                    while ((feaLHYDC = cursorLHYDC.NextFeature()) != null)
                                    {
                                        string gb = feaLHYDC.get_Value(hydlGBIndex).ToString();
                                        gbhs.Add(gb);
                                    }
                                    System.Runtime.InteropServices.Marshal.ReleaseComObject(cursorLHYDC);
                                }
                                if (gbhs.Count == 1)
                                {
                                    string gb = gbhs.ToList().First();
                                    foreach (var oid in oidList)
                                        oid_gbDict.Add(oid, gb);
                                }
                                else if (gbhs.Count > 1)
                                {
                                    string errInfo = "参考GB不唯一：" + string.Join(";", gbhs);
                                    foreach (var oid in oidList)
                                    {
                                        if (!feaCrossOIDList.Contains(oid))
                                            oid_Err_Dict.Add(oid, errInfo);
                                    }
                                }
                                else if (gbhs.Count == 0)
                                {
                                    string errInfo = "未找到参考GB";
                                    foreach (var oid in oidList)
                                    {
                                        if (!feaCrossOIDList.Contains(oid))
                                            oid_Err_Dict.Add(oid, errInfo);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    wo.SetText("2-分析水系线面关系-3-有HYDC（临时）");
                    #region 有HYDC(临时)的情况
                    {
                        string hydlSGSQL = string.Format(" {0}<>210400 ", hydlGBFN);
                        foreach (var kv2 in outStructwithHydcHydlFeaDict2)
                        {
                            string hydc = kv2.Key;
                            string hydlSQLX = hydlSGSQL + string.Format(" and {0}='{1}'", hydlHYDCFN,hydc);

                            var oidFeaDcit = kv2.Value; //同hydc的要素

                            Dictionary<string, List<string>> ptXDict = null;
                            Dictionary<int, List<int>> plXDict = null;
                            GetLinkGroups(oidFeaDcit, out ptXDict, out plXDict);//生成分组                   

                            foreach (var oidList in plXDict.Values)  //分组遍历
                            {
                                //List<IFeature> feaLinkList = new List<IFeature>(); //相连的同RN的要素的List
                                HashSet<string> gbhs = new HashSet<string>();

                                List<string> coreNodeList = new List<string>(); //单头点
                                foreach (var oid in oidList)
                                {
                                    string xk1 = string.Format("{0}_{1}", oid, 1);
                                    string xk2 = string.Format("{0}_{1}", oid, 2);
                                    if (ptXDict.ContainsKey(xk1))
                                    {
                                        if (ptXDict[xk1].Count == 1)
                                            coreNodeList.Add(xk1);
                                    }
                                    if (ptXDict.ContainsKey(xk2))
                                    {
                                        if (ptXDict[xk2].Count == 1)
                                            coreNodeList.Add(xk2);
                                    }
                                }

                                foreach (var coreNodeStr in coreNodeList) //用单头点,逐个搜索
                                {
                                    string[] items = coreNodeStr.Split(new char[] { '_' });
                                    int oidL = int.Parse(items[0]);
                                    IPolyline pl = oidFeaDcit[oidL].ShapeCopy as IPolyline;
                                    IPoint pt = items[1] == "1" ? pl.FromPoint : pl.ToPoint;

                                    ISpatialFilter hydlSPQF2 = new SpatialFilterClass()
                                    {
                                        WhereClause = hydlSQLX,
                                        Geometry = pt,
                                        SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects
                                    };

                                    IFeatureCursor cursorLHYDC = hydlFC.Search(hydlSPQF2, false);
                                    IFeature feaLHYDC = null;
                                    while ((feaLHYDC = cursorLHYDC.NextFeature()) != null)
                                    {
                                        string gb = feaLHYDC.get_Value(hydlGBIndex).ToString();
                                        gbhs.Add(gb);
                                    }
                                    System.Runtime.InteropServices.Marshal.ReleaseComObject(cursorLHYDC);
                                }
                                if (gbhs.Count == 1)
                                {
                                    string gb = gbhs.ToList().First();
                                    foreach (var oid in oidList)
                                        oid_gbDict.Add(oid, gb);
                                }
                                else if (gbhs.Count > 1)
                                {
                                    string errInfo = "参考GB不唯一：" + string.Join(";", gbhs);
                                    foreach (var oid in oidList)
                                    {
                                        if (!feaCrossOIDList.Contains(oid))
                                            oid_Err_Dict.Add(oid, errInfo);
                                    }
                                }
                                else if (gbhs.Count == 0)
                                {
                                    string errInfo = "未找到参考GB";
                                    foreach (var oid in oidList)
                                    {
                                        if (!feaCrossOIDList.Contains(oid))
                                            oid_Err_Dict.Add(oid, errInfo);
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                   
                }
                if (oid_gbDict.Count > 0) 
                {
                    m_Application.EngineEditor.StartOperation();
                    ICursor cs = hydlFC.Update(null, false) as ICursor;
                    IRow row = null;
                    while ((row = cs.NextRow()) != null)
                    {
                        if (oid_gbDict.ContainsKey(row.OID) && !feaCrossOIDList.Contains(row.OID))
                        {
                            string gb = oid_gbDict[row.OID];
                            row.set_Value(hydlGBIndex, gb);
                            row.Store();
                        }
                    }
                    cs.Flush();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(cs);
                    m_Application.EngineEditor.StopOperation(this.Caption);
                    m_Application.MapControl.ActiveView.Refresh();
                }

                string unUpdateReusltFileName = DCDHelper.GetAppDataPath() + string.Format("\\水面外结构线_未更新信息_{0}.shp", DateTime.Now.ToString("yyMMdd_HHmmss"));

                int nCheck = feaCrossOIDList.Count + oid_Err_Dict.Count;
                if (nCheck > 0)
                {                    
                    ShapeFileWriter resultFile = new ShapeFileWriter();
                    Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                    fieldName2Len.Add("要素类名", 16);
                    fieldName2Len.Add("要素编号", 16);
                    fieldName2Len.Add("说明", 128);

                    resultFile.createErrorResutSHPFile(unUpdateReusltFileName, (hydlFC as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);

                    foreach (var oid in feaCrossOIDList)
                    {
                        var fe = hydlFC.GetFeature(oid);

                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("要素类名", hydlFC.AliasName);
                        fieldName2FieldValue.Add("要素编号", oid.ToString());
                        fieldName2FieldValue.Add("说明", "结构线与水系面交叉");

                        resultFile.addErrorGeometry(fe.ShapeCopy, fieldName2FieldValue);
                    }

                    foreach (var item in oid_Err_Dict)
                    {
                        var fe = hydlFC.GetFeature(item.Key);

                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("要素类名", hydlFC.AliasName);
                        fieldName2FieldValue.Add("要素编号", item.Key.ToString());
                        fieldName2FieldValue.Add("说明", item.Value);

                        resultFile.addErrorGeometry(fe.ShapeCopy, fieldName2FieldValue);
                    }
                }
                string infoTxt = "";
                if (oid_gbDict.Count > 0)
                {                    
                    infoTxt = string.Format("修改了{0}条\n", oid_gbDict.Count);
                }
                else
                {                  
                    infoTxt = string.Format("没有修改");
                }

                if (nCheck > 0)
                {
                    infoTxt += "\r\n是否加载本次未更新的要素信息？";
                    if (MessageBox.Show(infoTxt, "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        IFeatureClass errFC = CheckHelper.OpenSHPFile(unUpdateReusltFileName);
                        CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                    }
                }
                else
                {
                    MessageBox.Show(infoTxt);
                }
                

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                //throw (ex);
                MessageBox.Show(ex.Message, this.Caption);
            }
        }
       
        public void GetLinkGroups(Dictionary<int, IFeature> oidFeaDict,
                         out Dictionary<string, List<string>> ptXDict,
                         out Dictionary<int, List<int>> plXDict)
        {
            Dictionary<string, IPoint> nodeDict = new Dictionary<string, IPoint>();

            #region 生成全节点
            foreach (var kv in oidFeaDict)
            {
                int oid = kv.Key;
                IFeature fea = kv.Value;
                IPolyline pl = fea.ShapeCopy as IPolyline;
                if (pl.IsClosed)
                {
                    string nodeKey = string.Format("{0}_{1}", fea.OID, 1);
                    nodeDict.Add(nodeKey, pl.FromPoint);
                }
                else
                {
                    string nodeKey1 = string.Format("{0}_{1}", fea.OID, 1);
                    nodeDict.Add(nodeKey1, pl.FromPoint);
                    string nodeKey2 = string.Format("{0}_{1}", fea.OID, 2);
                    nodeDict.Add(nodeKey2, pl.ToPoint);
                }
            }
            #endregion

            List<Tuple<string, string>> nearPtPairList = new List<Tuple<string, string>>();
            #region 生成全点位邻接表（小--大）
            foreach (var kv1 in nodeDict)
            {
                string k1 = kv1.Key;
                IPoint pt1 = kv1.Value;
                foreach (var kv2 in nodeDict)
                {
                    string k2 = kv2.Key;
                    IPoint pt2 = kv2.Value;
                    if (k1.CompareTo(k2) >= 0)
                        continue;
                    if (GetDist(pt1, pt2) < 0.001)
                    {
                        nearPtPairList.Add(new Tuple<string, string>(k1, k2));
                    }
                }
            }
            #endregion

            Dictionary<string, string> ptKey2ptKeyDict = new Dictionary<string, string>();
            #region 生成核心点邻接字典
            //点-牵头点 字典生成：大--小

            foreach (var tp in nearPtPairList)
            {
                string k1 = tp.Item1;
                string k2 = tp.Item2;
                if (!ptKey2ptKeyDict.ContainsKey(k1))
                    ptKey2ptKeyDict.Add(k1, k1);
                if (!ptKey2ptKeyDict.ContainsKey(k2))
                    ptKey2ptKeyDict.Add(k2, k1);
            }

            //点关系分析
            Dictionary<string, string> moveD = new Dictionary<string, string>();
            do
            {
                moveD.Clear();
                foreach (var kv in ptKey2ptKeyDict)
                {
                    string k2 = kv.Key;
                    string k1 = kv.Value;
                    foreach (var tp in nearPtPairList)
                    {
                        if (k1 == tp.Item2 && !moveD.ContainsKey(k2))
                        {
                            moveD.Add(k2, tp.Item1);
                        }
                    }
                }
                foreach (var kv in moveD)
                {
                    ptKey2ptKeyDict[kv.Key] = kv.Value;
                }

            } while (moveD.Count > 0);
            #endregion

            Dictionary<string, List<string>> ptKey2ptKeyListDict = new Dictionary<string, List<string>>();
            #region 生成字典：核心点-聚合点列表
            foreach (var kv in ptKey2ptKeyDict)
            {
                if (ptKey2ptKeyListDict.ContainsKey(kv.Value))
                    ptKey2ptKeyListDict[kv.Value].Add(kv.Key);
                else
                    ptKey2ptKeyListDict.Add(kv.Value, new List<string>() { kv.Key });
            }
            #endregion
            //加上自身
            List<string> gotOids = new List<string>();
            foreach (var oids in ptKey2ptKeyListDict.Values)
            {
                foreach (var oid in oids)
                {
                    if (gotOids.Contains(oid))
                        continue;
                    else
                        gotOids.Add(oid);
                }
            }
            foreach (var k in nodeDict.Keys)
            {
                if (!gotOids.Contains(k))
                    ptKey2ptKeyListDict.Add(k, new List<string>() { k });
            }




            List<Tuple<int, int>> nearPlPairList = new List<Tuple<int, int>>();
            #region 生成线邻接表（小--大）
            foreach (var kv1 in oidFeaDict)
            {
                int oid1 = kv1.Key;
                IFeature fea1 = kv1.Value;
                foreach (var kv2 in oidFeaDict)
                {
                    int oid2 = kv2.Key;
                    IFeature fea2 = kv2.Value;
                    if (oid2 <= oid1)
                        continue;
                    if (GetDist(fea1.Shape as IPolyline, fea2.Shape as IPolyline) < 0.001)
                    {
                        nearPlPairList.Add(new Tuple<int, int>(oid1, oid2));
                    }
                }
            }
            #endregion

            Dictionary<int, int> plKey2plKeyDict = new Dictionary<int, int>();
            //线-牵头线 字典生成：大--小
            #region 生成核心线邻接字典
            foreach (var tp in nearPlPairList)
            {
                int k1 = tp.Item1;
                int k2 = tp.Item2;
                if (!plKey2plKeyDict.ContainsKey(k1))
                    plKey2plKeyDict.Add(k1, k1);
                if (!plKey2plKeyDict.ContainsKey(k2))
                    plKey2plKeyDict.Add(k2, k1);
            }
            #endregion


            //线关系分析
            #region
            Dictionary<int, int> moveDL = new Dictionary<int, int>();
            do
            {
                moveDL.Clear();
                foreach (var kv in plKey2plKeyDict)
                {
                    int k2 = kv.Key;
                    int k1 = kv.Value;
                    foreach (var tp in nearPlPairList)
                    {
                        if (k1 == tp.Item2 && !moveDL.ContainsKey(k2))
                        {
                            moveDL.Add(k2, tp.Item1);
                        }
                    }
                }
                foreach (var kv in moveDL)
                {
                    plKey2plKeyDict[kv.Key] = kv.Value;
                }

            } while (moveDL.Count > 0);
            #endregion

            Dictionary<int, List<int>> plKey2plKeyListDict = new Dictionary<int, List<int>>();
            #region 生成字典：核心点-聚合点列表
            foreach (var kv in plKey2plKeyDict)
            {
                if (plKey2plKeyListDict.ContainsKey(kv.Value))
                    plKey2plKeyListDict[kv.Value].Add(kv.Key);
                else
                    plKey2plKeyListDict.Add(kv.Value, new List<int>() { kv.Key });
            }
            #endregion
            //加上自身
            List<int> gotOidsL = new List<int>();
            foreach (var oids in plKey2plKeyListDict.Values)
            {
                foreach (var oid in oids)
                {
                    if (gotOidsL.Contains(oid))
                        continue;
                    else
                        gotOidsL.Add(oid);
                }
            }
            foreach (var k in oidFeaDict.Keys)
            {
                if (!gotOidsL.Contains(k))
                    plKey2plKeyListDict.Add(k, new List<int>() { k });
            }


            ptXDict = ptKey2ptKeyListDict;
            plXDict = plKey2plKeyListDict;
        }

        //获取两点距离（本身的空间参考）
        public double GetDist(IPoint pt1, IPoint pt2)
        {
            IProximityOperator proxiOP = pt1 as IProximityOperator;
            return proxiOP.ReturnDistance(pt2);
        }

        //获取两要素距离
        public double GetDist(IPolyline pl1, IPolyline pl2)
        {
            double d1 = GetDist(pl1.FromPoint, pl2.FromPoint);
            double d2 = GetDist(pl1.FromPoint, pl2.ToPoint);
            double d3 = GetDist(pl1.ToPoint, pl2.ToPoint);
            double d4 = GetDist(pl1.ToPoint, pl2.FromPoint);
            List<double> dists = new List<double>() { d1, d2, d3, d4 };
            return dists.Min();
        }

        public bool IsHYDCTemp(string hydc)
        {
            if (hydc.EndsWith("9"))
                return true;
            else
                return false;

        }
    }
}
