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
    /// 设计原则：对GB为430501,430502,430503,430200对应的四类城市道路GB进行更新，更新原则如下：
    /// 若标城市道路不与任何街区面存在叠置关系，且CLASS1为空为''时，分以下情况进行处理：
    ///     ①RN为空，在数据库中检索是否存在与之一字相连的城际道路，
    ///            1）若存在1种GB，且不存在T字或十字相交，则将之更新为该城际道路GB；若存在，则提示
    ///            2）若存在多种GB，提示
    ///            3）不存在，提示。
    ///     ②RN不为空，在数据库中检索是否存在RN相同的、相邻的城际道路
    ///            1）若存在，且GB/NAME唯一，则将之更新为该城际公路GB、NAME；
    ///            2) 若不存在/不唯一，则提示
    ///            //---取消这条：2）不存在，按字母对照表更新GB;
    ///     
    ///    
    /// </summary>
    public class CityRoadReClassifyCmd2 : SMGICommand
    {
        private Dictionary<char, int> RN_GB_D = new Dictionary<char, int>() { { 'G', 420101 },
                                                                              { 'S', 420101 }, 
                                                                              { 'X', 420301 }, 
                                                                              { 'Y', 420400 }, 
                                                                              { 'Z', 420500 },
                                                                              { 'C', 420800 } };
        public CityRoadReClassifyCmd2()
        {
            m_caption = "街区外道路改城际";
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
            //主动清空选择几何（避免bug）
            IMap map = m_Application.MapControl.ActiveView as IMap;
            map.ClearSelection();

            IFeatureLayer resaLayer, lrdlLayer = null;
            IFeatureClass fcRESA, fcLRDL = null;
            string lrdlGBFN = "GB", lrdlRNFN = "RN", lrdlNAMEFN = "NAME", lrdlCLASS1FN = "CLASS1";
            int lrdlGBIndex = -1, lrdlRNIndex = -1, lrdlNAMEIndex = -1, lrdlCLASS1Index = -1;

            #region 1-获取相关图层[RESA/LRDL]及字段位置
            //RESA
            string fcName = "RESA";            
            resaLayer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (resaLayer == null)
            {
                MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            fcRESA = resaLayer.FeatureClass;

            //LRDL[GB/RN/NAME]
            fcName = "LRDL";
            
            lrdlLayer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lrdlLayer == null)
            {
                MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            lrdlGBIndex = lrdlLayer.FeatureClass.FindField(lrdlGBFN);
            if (lrdlGBIndex == -1)
            {
                MessageBox.Show(string.Format("要素类【{0}】中找不到字段【{1}】", fcName, lrdlGBFN));
                return;
            }
            lrdlRNIndex = lrdlLayer.FeatureClass.FindField(lrdlRNFN);
            if (lrdlRNIndex == -1)
            {
                MessageBox.Show(string.Format("要素类【{0}】中找不到字段【{1}】", fcName, lrdlRNFN));
                return;
            }
            lrdlNAMEIndex = lrdlLayer.FeatureClass.FindField(lrdlNAMEFN);
            if (lrdlNAMEIndex == -1)
            {
                MessageBox.Show(string.Format("要素类【{0}】中找不到字段【{1}】", fcName, lrdlNAMEFN));
                return;
            }
            lrdlCLASS1Index = lrdlLayer.FeatureClass.FindField(lrdlCLASS1FN);
            if (lrdlCLASS1Index == -1)
            {
                MessageBox.Show(string.Format("要素类【{0}】中找不到字段【{1}】", fcName, lrdlCLASS1FN));
                return;
            }

            fcLRDL = lrdlLayer.FeatureClass;
            #endregion


            #region 选择表达式(lrdlSQL、resaSQL)、过滤器lrdlQF
            //街区道路SQL
            string lrdlStreetSQL = string.Format("{0} in (430501,430502,430503,430200) and (CLASS1 IS NULL OR CLASS1 ='')  ", lrdlGBFN); //城市道路
            if (lrdlLayer.FeatureClass.HasCollabField())
                lrdlStreetSQL += " and " + cmdUpdateRecord.CurFeatureFilter;
            IQueryFilter lrdlStreetQF = new QueryFilterClass() { WhereClause = lrdlStreetSQL};

            string lrdlroadSQL = string.Format("{0} not in (430501,430502,430503,430200)  ", lrdlGBFN); //公路
            if (lrdlLayer.FeatureClass.HasCollabField())
                lrdlroadSQL += " and " + cmdUpdateRecord.CurFeatureFilter;
            //IQueryFilter lrdlRoadQF = new QueryFilterClass() { WhereClause = lrdlroadSQL }; 

            //街区SQL
            string resaSQL = "GB = 310200";   //街区
            if(resaLayer.FeatureClass.HasCollabField())
                resaSQL += " and " + cmdUpdateRecord.CurFeatureFilter;            
            #endregion

            //外部(有RN)
            Dictionary<string, Dictionary<int, IFeature>> streedoutRESAwithRNoidDict = new Dictionary<string, Dictionary<int, IFeature>>();

            //外部(无RN)
            Dictionary<int,IFeature> streedoutRESAoidDict = new  Dictionary<int,IFeature>();   
            
            //交叉
            List<int> streedcrossRESAoidList = new List<int>(); //交叉

            IFeatureCursor cursorStreet = lrdlLayer.Search(lrdlStreetQF, false);
            IFeature feaStreet = null;
           
            #region 线面关系分析
            while ((feaStreet = cursorStreet.NextFeature()) != null)
            {
                string tag ="unkown";
                IPolyline plStreet = feaStreet.ShapeCopy as IPolyline;
                //Within-线在面内
                ISpatialFilter spQFWithIn = new SpatialFilterClass()
                {
                    Geometry = plStreet,
                    WhereClause = resaSQL,
                    SpatialRel = esriSpatialRelEnum.esriSpatialRelWithin,
                };
                if (fcRESA.FeatureCount(spQFWithIn) > 0)
                {
                    tag ="in";
                    continue;
                }

                //InterSect
                ISpatialFilter spQFIntersect = new SpatialFilterClass()
                {
                    Geometry = plStreet,
                    WhereClause = resaSQL,
                    SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects,
                };
                if (fcRESA.FeatureCount(spQFIntersect) == 0)
                {
                    tag = "out";
                }
                else
                {                   
                    IFeatureCursor cursor2 = fcRESA.Search(spQFIntersect, false);
                    IFeature fea2 = null;
                    while ((fea2 = cursor2.NextFeature()) != null)
                    {
                        ITopologicalOperator op = fea2.Shape as ITopologicalOperator;
                        if (!plStreet.IsEmpty)
                        {
                            IGeometry clip = op.Intersect(plStreet,esriGeometryDimension.esriGeometry1Dimension);
                            ITopologicalOperator op2 = plStreet as ITopologicalOperator;
                            IGeometry erase = op2.Difference(clip);
                            plStreet = erase as IPolyline;                             
                        }
                        else
                            break;
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor2);

                    if (plStreet.IsEmpty)
                    {
                        tag = "in";
                        continue;
                    }
                    else
                    {
                        if (plStreet.Length < (feaStreet.Shape as IPolyline).Length)
                        {
                            tag = "cross";
                            streedcrossRESAoidList.Add(feaStreet.OID);
                            continue;
                        }
                        else
                        {
                            tag = "out";                            
                        }
                    }
                }
                if (tag == "out")
                {
                    string rn = feaStreet.get_Value(lrdlRNIndex).ToString();
                    if (rn != string.Empty)
                    {
                        if (streedoutRESAwithRNoidDict.ContainsKey(rn))
                        {
                            streedoutRESAwithRNoidDict[rn].Add(feaStreet.OID, feaStreet);
                        }
                        else
                        {
                            streedoutRESAwithRNoidDict.Add(rn, new Dictionary<int, IFeature>() { { feaStreet.OID, feaStreet } });
                        }
                    }
                    else
                    {
                        streedoutRESAoidDict.Add(feaStreet.OID, feaStreet);
                    }

                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursorStreet);
            
            #endregion

            
            #region 3.1 街区道路与街区面进行叠置分析，整理所有街区道路与街区面的叠置情况
            
            #endregion

            Dictionary<int, string> oid_Err_Dict = new Dictionary<int, string>();
            Dictionary<int, Tuple<string, string>> oid_gb_name_RN_Dict = new Dictionary<int, Tuple<string, string>>();
            Dictionary<int, string> oid_gbDict = new Dictionary<int, string>();
            //List<int> oid_noDeal = new List<int>();

            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    wo.SetText("分析");                   
                    #region 有RN的
                    foreach (var kv1 in streedoutRESAwithRNoidDict)
                    {
                        string rn = kv1.Key;
                        string lrdlroadSQL2 = lrdlroadSQL + string.Format(" and RN='{0}'", rn);

                        var oidFeaDcit = kv1.Value; //同RN的要素

                        Dictionary<string, List<string>> ptXDict = null;
                        Dictionary<int, List<int>> plXDict = null;
                        GetLinkGroups(oidFeaDcit, out ptXDict, out plXDict);//生成分组                   

                        foreach (var oidList in plXDict.Values)  //分组遍历
                        {
                            //List<IFeature> feaLinkList = new List<IFeature>(); //相连的同RN的要素的List
                            HashSet<string> gbhs = new HashSet<string>();
                            HashSet<string> namehs = new HashSet<string>();

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

                                ISpatialFilter lrdlRoadQF2 = new SpatialFilterClass()
                                {
                                    WhereClause = lrdlroadSQL2,
                                    Geometry = pt,
                                    SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects
                                };

                                IFeatureCursor cursorLRN = fcLRDL.Search(lrdlRoadQF2, false);
                                IFeature feaLRN = null;
                                while ((feaLRN = cursorLRN.NextFeature()) != null)
                                {
                                    string gb = feaLRN.get_Value(lrdlGBIndex).ToString();
                                    string name = feaLRN.get_Value(lrdlNAMEIndex).ToString();
                                    gbhs.Add(gb);
                                    namehs.Add(name);
                                }
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursorLRN);
                            }
                            if (gbhs.Count == 1 && namehs.Count == 1)
                            {
                                string gb = gbhs.ToList().First();
                                string name = namehs.ToList().First();
                                foreach (var oid in oidList)
                                    oid_gb_name_RN_Dict.Add(oid, new Tuple<string, string>(gb, name));
                            }
                            else if (gbhs.Count > 1)
                            {
                                string errInfo = "参考GB不唯一：" + string.Join(";", gbhs);
                                foreach (var oid in oidList)
                                    oid_Err_Dict.Add(oid, errInfo);
                            }
                            else if (gbhs.Count == 0)
                            {
                                string errInfo = "未找到参考GB";
                                foreach (var oid in oidList)
                                    oid_Err_Dict.Add(oid, errInfo);
                            }
                            else if (namehs.Count > 1)
                            {
                                string errInfo = "参考NAME不唯一：" + string.Join(";", namehs);
                                foreach (var oid in oidList)
                                    oid_Err_Dict.Add(oid, errInfo);
                            }
                            else if (namehs.Count == 0)
                            {
                                string errInfo = "未找到参考NAME";
                                foreach (var oid in oidList)
                                    oid_Err_Dict.Add(oid, errInfo);
                            }

                        }
                    }
                    #endregion
                   
                    #region 无RN的
                    {
                        Dictionary<string, List<string>> ptXDict = null;
                        Dictionary<int, List<int>> plXDict = null;
                        GetLinkGroups(streedoutRESAoidDict, out ptXDict, out plXDict);//生成分组

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
                                    if (ptXDict[xk1].Count == 1)
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
                                IPolyline pl = streedoutRESAoidDict[oidL].ShapeCopy as IPolyline;
                                IPoint pt = items[1] == "1" ? pl.FromPoint : pl.ToPoint;

                                ISpatialFilter lrdlRoadQF = new SpatialFilterClass()
                                {
                                    WhereClause = lrdlroadSQL+ " and (rn is null or rn='')",
                                    Geometry = pt,
                                    SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects
                                };

                                HashSet<string> gbhs0 = new HashSet<string>(); //当前节点
                                int nx0 = 0;
                                IFeatureCursor cursorL = fcLRDL.Search(lrdlRoadQF, false);
                                IFeature feaL = null;
                                while ((feaL = cursorL.NextFeature()) != null)
                                {
                                    string gb = feaL.get_Value(lrdlGBIndex).ToString();
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
                                    string gb0 = streedoutRESAoidDict[oid].get_Value(lrdlGBIndex).ToString();
                                    
                                    if (maxCross > 2)
                                        oid_Err_Dict.Add(oid, string.Format("警告：存在{0}岔口,核实。GB:{1} --> {2}", maxCross,gb0,gb));
                                    else
                                        oid_gbDict.Add(oid, gb);

                                }
                            }
                            else if (gbhs.Count > 1)
                            {
                                string errInfo = "参考GB不唯一："+string.Join(";", gbhs);
                                foreach (var oid in oidList)
                                    oid_Err_Dict.Add(oid, errInfo);
                            }
                            else if (gbhs.Count == 0)
                            {
                                string errInfo = "未找到参考GB";
                                foreach (var oid in oidList)
                                    oid_Err_Dict.Add(oid, errInfo);
                            }

                        }
                    }
                    #endregion

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                MessageBox.Show("分析失败\n"+ex.Message);
            }

            int editCount = 0;
            m_Application.EngineEditor.StartOperation();
            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    wo.SetText("修改数据");
                    ICursor cs = fcLRDL.Update(null, false) as ICursor;
                    IRow row = null;
                    while ((row = cs.NextRow()) != null)
                    {
                        if(oid_gb_name_RN_Dict.ContainsKey(row.OID))
                        {
                            string gb = oid_gb_name_RN_Dict[row.OID].Item1;
                            string name = oid_gb_name_RN_Dict[row.OID].Item2;
                            row.set_Value(lrdlGBIndex, gb);
                            row.set_Value(lrdlNAMEIndex, name);
                            row.Store();
                            editCount++;
                        }
                        else if (oid_gbDict.ContainsKey(row.OID))
                        {
                            string gb = oid_gbDict[row.OID];
                            row.set_Value(lrdlGBIndex, gb);
                            row.Store();
                            editCount++;
                        }
                    }
                    cs.Flush();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(cs);
                }
                string unUpdateReusltFileName = DCDHelper.GetAppDataPath() + string.Format("\\街区道路_未更新信息_{0}.shp", DateTime.Now.ToString("yyMMdd_HHmmss"));                        
                    
                int nCheck = streedcrossRESAoidList.Count + oid_Err_Dict.Count;
                if (nCheck > 0)
                {
                    using (var wo = m_Application.SetBusy())
                    {
                        wo.SetText("输出结果");
                        ShapeFileWriter resultFile = new ShapeFileWriter();
                        Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                        fieldName2Len.Add("要素类名", 16);
                        fieldName2Len.Add("要素编号", 16);
                        fieldName2Len.Add("说明", 128);

                        resultFile.createErrorResutSHPFile(unUpdateReusltFileName, (lrdlLayer.FeatureClass as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);

                        foreach (var oid in streedcrossRESAoidList)
                        {
                            var fe = lrdlLayer.FeatureClass.GetFeature(oid);

                            Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                            fieldName2FieldValue.Add("要素类名", lrdlLayer.FeatureClass.AliasName);
                            fieldName2FieldValue.Add("要素编号", oid.ToString());
                            fieldName2FieldValue.Add("说明", "道路与街区面交叉");

                            resultFile.addErrorGeometry(fe.ShapeCopy, fieldName2FieldValue);
                        }

                        foreach (var item in oid_Err_Dict)
                        {
                            var fe = lrdlLayer.FeatureClass.GetFeature(item.Key);

                            Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                            fieldName2FieldValue.Add("要素类名", lrdlLayer.FeatureClass.AliasName);
                            fieldName2FieldValue.Add("要素编号", item.Key.ToString());
                            fieldName2FieldValue.Add("说明", item.Value);

                            resultFile.addErrorGeometry(fe.ShapeCopy, fieldName2FieldValue);
                        }
                    }
                }

                string infoTxt = "";
                if (editCount > 0)
                {
                    m_Application.EngineEditor.StopOperation(this.Caption);
                    m_Application.MapControl.ActiveView.Refresh();
                    infoTxt = string.Format("修改了{0}条\n", editCount);                    
                }
                else
                {
                    m_Application.EngineEditor.AbortOperation();
                    infoTxt = string.Format("没有修改\n", editCount);
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
                m_Application.EngineEditor.AbortOperation();

                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                
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
            List<double> dists = new List<double>() { d1,d2,d3,d4};
            return dists.Min();          
        }
    }
}
