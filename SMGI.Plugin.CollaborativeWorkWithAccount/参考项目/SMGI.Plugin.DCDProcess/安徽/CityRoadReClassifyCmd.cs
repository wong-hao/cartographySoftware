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
    /// 街区外道路改城际：根据街区面的取舍情况更新街区道路的GB属性显示(以及NAME)
    /// 修改自广西：CityRoadSymbolUpdateCmdGX.cs
    /// 当街区面舍去后，原面内的街区道路，需要处理
    /// 
    /// 设计原则：对GB为430501,430502,430503,430200对应的四类城市道路GB进行更新，更新原则如下：
    /// 若目标城市道路不与任何街区面存在叠置关系时，分以下情况进行处理：
    ///     ①RN为空，在数据库中检索是否存在与之相邻的城际道路，
    ///            1）若存在1种GB，则将之更新为该城际道路GB
    ///            2）若存在多种GB，则不更新
    ///            3）不存在，标识为未更新。
    ///     ②RN不为空，在数据库中检索是否存在RN相同的、相邻的城际道路
    ///            1）若存在，则将之更新为该城际公路GB；
    ///            2）不存在，按字母对照表更新GB;
    ///     
    ///    
    /// </summary>
    public class CityRoadReClassifyCmd : SMGICommand
    {
        private Dictionary<char, int> RN_GB_D = new Dictionary<char, int>() { { 'G', 420101 },
                                                                              { 'S', 420101 }, 
                                                                              { 'X', 420301 }, 
                                                                              { 'Y', 420400 }, 
                                                                              { 'Z', 420500 },
                                                                              { 'C', 420800 } };
        public CityRoadReClassifyCmd()
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
            #endregion


            #region 2-创建临时数据库MyWorkspace.gdb
            IWorkspace tempWS = DCDHelper.createTempWorkspace(DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb");
            Geoprocessor gp = new Geoprocessor();
            gp.OverwriteOutput = true;
            gp.SetEnvironmentValue("workspace", tempWS.PathName);
            #endregion


            #region 选择表达式(lrdlSQL、resaSQL)、过滤器lrdlQF
            string lrdlSQL = string.Format("{0} in (430501,430502,430503,430200) and (CLASS1 IS NOT NULL and CLASS1<>'') ", lrdlGBFN); //城市道路
            if (lrdlLayer.FeatureClass.HasCollabField())
                lrdlSQL += " and " + cmdUpdateRecord.CurFeatureFilter;
            string resaSQL = "GB = 310200";   //街区
            if(resaLayer.FeatureClass.HasCollabField())
                resaSQL += " and " + cmdUpdateRecord.CurFeatureFilter;

            IQueryFilter lrdlQF = new QueryFilterClass();
            lrdlQF.WhereClause = lrdlSQL;
            #endregion           

            
            List<int> oidOfInRESAList = new List<int>();//与街区面存在叠置关系的街区道路OID集合

            #region 3.1 街区道路与街区面进行叠置分析，整理所有街区道路与街区面的叠置情况
            //主要用于获取区分街区外道路
            string outFCName = lrdlLayer.FeatureClass.AliasName + "_" + resaLayer.FeatureClass.AliasName + "_intersect";
            int c = 0;
            while ((tempWS as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, outFCName))
            {
                ++c;
                outFCName += c.ToString();
            }


            MakeFeatureLayer mkFeatureLayer = new MakeFeatureLayer();
            SelectLayerByAttribute selLayerByAttribute = new SelectLayerByAttribute();

            mkFeatureLayer.in_features = lrdlLayer;
            mkFeatureLayer.out_layer = lrdlLayer.FeatureClass.AliasName + "_Layer";
            gp.Execute(mkFeatureLayer, null);
            selLayerByAttribute.in_layer_or_view = lrdlLayer.FeatureClass.AliasName + "_Layer";
            selLayerByAttribute.where_clause = lrdlSQL;
            gp.Execute(selLayerByAttribute, null);

            mkFeatureLayer.in_features = resaLayer;
            mkFeatureLayer.out_layer = resaLayer.FeatureClass.AliasName + "_Layer";
            gp.Execute(mkFeatureLayer, null);
            selLayerByAttribute.in_layer_or_view = resaLayer.FeatureClass.AliasName + "_Layer";
            selLayerByAttribute.where_clause = resaSQL;//是否排除删除面、空地面
            gp.Execute(selLayerByAttribute, null);

            Intersect intersectTool = new Intersect(); //疑问：边界上是否存在多选、漏选的情况
            intersectTool.in_features = lrdlLayer.FeatureClass.AliasName + "_Layer;" + resaLayer.FeatureClass.AliasName + "_Layer";
            intersectTool.out_feature_class = outFCName;
            intersectTool.join_attributes = "ONLY_FID";
            intersectTool.output_type = "LINE";
            gp.Execute(intersectTool, null);

            IFeatureClass temp_intersectFC = (tempWS as IFeatureWorkspace).OpenFeatureClass(outFCName);

            string objFIDFieldName = string.Format("FID_{0}", lrdlLayer.FeatureClass.AliasName);
            int objFIDIndex = temp_intersectFC.FindField(objFIDFieldName);

            IFeatureCursor interFeCursor = temp_intersectFC.Search(null, true);
            IFeature interFe = null;
            while ((interFe = interFeCursor.NextFeature()) != null)
            {
                int objFID = int.Parse(interFe.get_Value(objFIDIndex).ToString());

                if (!oidOfInRESAList.Contains(objFID))
                {
                    oidOfInRESAList.Add(objFID);
                }
            }
            Marshal.ReleaseComObject(interFeCursor);
            #endregion

            
            try
            {
                m_Application.EngineEditor.StartOperation();

                #region 3-编辑启动之后的操作

                Dictionary<int,string> updatedLRDLFIDList = new Dictionary<int,string>();//本次更新的要素信息
                int unUpdateLRDLCount = 0;//未能最终确认GB的街区道路
                string unUpdateReusltFileName = DCDHelper.GetAppDataPath() + string.Format("\\街区道路_未更新信息_{0}.shp", DateTime.Now.ToString("yyMMdd_HHmmss"));
                using (var wo = m_Application.SetBusy())
                {
                    Dictionary<int, string> unUpdateFIDListOfRN = new Dictionary<int, string>();//未能判断GB的非法街区道路(RN)
                    Dictionary<int, string> unUpdateFIDListOfTouch = new Dictionary<int, string>();//未能判断GB的非法街区道路(邻接)
                    Dictionary<int, int> oid_GB = new Dictionary<int, int>();

                    wo.SetText("遍历街区外的城市道路");
                    #region 3.2 遍历街区道路，对比街区道路并进行相应更新，无法判断更新的，收集起来
                    IFeatureCursor feCursor = lrdlLayer.Search(lrdlQF, false);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {                        
                        int gb = int.Parse(fe.get_Value(lrdlGBIndex).ToString());
                        string rn = fe.get_Value(lrdlRNIndex).ToString();

                        #region fe-pl起始2点几何用于选择，避免相交
                        IPolyline pl = fe.ShapeCopy as IPolyline;
                        IPointCollection pc = new MultipointClass();
                        pc.AddPoint(pl.FromPoint);
                        pc.AddPoint(pl.ToPoint);
                        IGeometry geo2P = pc as IGeometry; //
                        #endregion

                        //街区内的城际公路,此工具忽略
                        if (oidOfInRESAList.Contains(fe.OID))                        
                            continue;                                         
                        else
                        {
                            #region 3.2.2-街区外的街区道路
                            //无RN的情况
                            if (String.IsNullOrEmpty(rn))
                            {
                                #region A-没有RN的情况                               

                                #region A-1 获取GBs: 所有的相连的城际公路的GB
                                List<int> GBs = new List<int>();
                                string sql = string.Format("{0} not in ({1},{2},{3},{4}) and (rn is null  or rn='' ) ", lrdlGBFN, 430200, 430501, 430502, 430503);
                                if (lrdlLayer.FeatureClass.HasCollabField())
                                    sql += " and " + cmdUpdateRecord.CurFeatureFilter; 
                                IFeatureCursor feCursorTouch = lrdlLayer.Search(new SpatialFilterClass() { WhereClause = sql, Geometry = geo2P, SpatialRel = esriSpatialRelEnum.esriSpatialRelTouches }, true);
                                IFeature feTouch = null;
                                while ((feTouch = feCursorTouch.NextFeature()) != null)
                                {
                                    int gbt = -1;
                                    int.TryParse(feTouch.get_Value(lrdlGBIndex).ToString(), out gbt);
                                    if (gbt > 1)
                                    {  
                                        if (!GBs.Contains(gbt))
                                            GBs.Add(gbt);                                        
                                    }
                                }
                                Marshal.ReleaseComObject(feCursorTouch);
                                #endregion

                                #region A-2 根据GBs的个数，分3种情况，对街区外的街区道路 处理/报错
                                //1种 |0种 |多种
                                if (GBs.Count ==1)
                                {
                                    #region 情况1：1个GB，直接赋(改成后面赋)
                                    /*
                                    int correctGB = GBs[0];
                                    fe.set_Value(lrdlGBIndex, correctGB);
                                    string name0 = fe.get_Value(lrdlNAMEIndex).ToString();
                                    if(name0!=null)
                                        fe.set_Value(lrdlNAMEIndex, null); //RN为空的，原来街道的NAME也不要
                                    fe.Store();

                                    updatedLRDLFIDList.Add(fe.OID, string.Format("已更新， GB从{0}更新为{1} 1", gb, correctGB));
                                     */ 
                                    #endregion
                                    oid_GB.Add(fe.OID, GBs[0]);
                                }
                                else if (GBs.Count == 0)
                                {
                                    #region 情况2：0个GB，没法赋，记录问题
                                    unUpdateFIDListOfTouch.Add(fe.OID, string.Format("未更新(0)，RN为空,无相邻城际公路"));
                                    #endregion
                                }
                                else
                                {
                                    #region 情况3：多个GB，没法赋，记录问题 pass                                    
                                    string gbStrs = "";
                                    foreach (var gb00 in GBs)
                                    {
                                        gbStrs += String.Format(" {0}", gb00);
                                    }
                                    unUpdateFIDListOfTouch.Add(fe.OID, string.Format("未更新(1)，RN为空，相邻城际公路GB不唯一：" + gbStrs));
                                    #endregion
                                }
                                #endregion

                                #endregion
                            }
                            else
                            {
                                #region B-有RN的情况

                                #region B-1-信息存储在GB_NAME_touchList/GB_NAME中
                                //相邻的有RN的
                                List<Tuple<int, string>> GB_NAME_touchList = new List<Tuple<int, string>>(); 
                                string sql = string.Format("{0} not in ({1},{2},{3},{4}) and rn = '{5}'", lrdlGBFN, 430200, 430501, 430502, 430503, rn);
                                if(lrdlLayer.FeatureClass.HasCollabField())
                                    sql += " and " + cmdUpdateRecord.CurFeatureFilter;
                                IFeatureCursor feCursorRNtouch = lrdlLayer.Search(new SpatialFilterClass() { WhereClause = sql, Geometry = geo2P, SpatialRel = esriSpatialRelEnum.esriSpatialRelTouches }, true);
                                IFeature feRNtouch = null;
                                while ((feRNtouch = feCursorRNtouch.NextFeature()) != null)
                                {
                                    int gbSRN = -1;
                                    int.TryParse(feRNtouch.get_Value(lrdlGBIndex).ToString(), out gbSRN);
                                    if (gbSRN > 1)
                                    {
                                        Tuple<int, string> tp = new Tuple<int, string>(gbSRN, feRNtouch.get_Value(lrdlNAMEIndex).ToString());
                                        if (!GB_NAME_touchList.Contains(tp))
                                            GB_NAME_touchList.Add(tp);                                        
                                    }
                                }
                                Marshal.ReleaseComObject(feCursorRNtouch);
                                //全局的有RN的
                                List<Tuple<int, string>> GB_NAME_List = new List<Tuple<int, string>>();
                                IFeatureCursor feCursorRN = lrdlLayer.Search(new QueryFilterClass() { WhereClause = sql }, true);
                                IFeature feRN = null;
                                while ((feRN = feCursorRN.NextFeature()) != null)
                                {
                                    int gbSRN = -1;
                                    int.TryParse(feRN.get_Value(lrdlGBIndex).ToString(), out gbSRN);
                                    if (gbSRN > 1)
                                    {
                                        Tuple<int, string> tp = new Tuple<int, string>(gbSRN, feRN.get_Value(lrdlNAMEIndex).ToString());
                                        if (!GB_NAME_List.Contains(tp))
                                            GB_NAME_List.Add(tp);
                                    }
                                }
                                Marshal.ReleaseComObject(feCursorRN);
                                #endregion

                                #region B-2-根据GBNAME的个数，分类处理
                                if (GB_NAME_List.Count == 1)
                                {
                                    int correctGB = GB_NAME_List[0].Item1;
                                    string correctName = GB_NAME_List[0].Item2;
                                    fe.set_Value(lrdlGBIndex, correctGB);
                                    fe.set_Value(lrdlNAMEIndex, correctName);
                                    fe.Store();

                                    updatedLRDLFIDList.Add(fe.OID, string.Format("已更新(3)， RN：{0}，GB：{1}→{2}，NAME：→{3}", rn, gb, correctGB, correctName));
                                }
                                else if (GB_NAME_List.Count == 0)
                                {
                                    unUpdateFIDListOfRN.Add(fe.OID, string.Format("未更新(4)，RN：{0}，同RN城际公路个数=0", rn)); 
                                }
                                else //GB_NAME_List多于1个的情况(GB_NAME_List.Count>1)
                                {
                                    if (GB_NAME_touchList.Count == 1)
                                    {
                                        int correctGB = GB_NAME_touchList[0].Item1;
                                        string correctName = GB_NAME_touchList[0].Item2;
                                        fe.set_Value(lrdlGBIndex, correctGB);
                                        fe.set_Value(lrdlNAMEIndex, correctName);
                                        fe.Store();
                                        updatedLRDLFIDList.Add(fe.OID, string.Format("已更新(5)，RN：{0}，相邻唯一,GB：{1}→{2}，NAME：→{3}", rn, gb, correctGB, correctName)); 
                                    }
                                    else if (GB_NAME_touchList.Count == 0)
                                    {
                                        unUpdateFIDListOfRN.Add(fe.OID, string.Format("未更新（暂时），RN：{0}，同RN城际公路不唯一，又无相邻", rn));  
                                    }
                                    
                                    //根据RN直接为GB赋值
                                    /*
                                    char chr = rn[0];
                                    int temp_gb;
                                    if (RN_GB_D.TryGetValue(chr, out temp_gb))//从配置文件中直接获取RN--GB
                                    {
                                        fe.set_Value(lrdlGBIndex, temp_gb);
                                        fe.Store();
                                        updatedLRDLFIDList.Add(fe.OID, string.Format("已更新(61)，RN：{7}，只改了GB", rn));
                                    }
                                    else
                                    {
                                        unUpdateFIDListOfRN.Add(fe.OID, string.Format("未更新(62)，RN：{8}，RN有误", rn));
                                    }
                                    */
                                }
                                #endregion

                                #endregion
                            }
                            #endregion
                        }

                        Marshal.ReleaseComObject(fe);
                    }
                    Marshal.ReleaseComObject(feCursor);
                    #endregion

                    wo.SetText("处理-第二阶段");
                    #region 对有唯一对应GB的结构线，进行赋值
                    IFeatureCursor feCursor2 = lrdlLayer.Search(lrdlQF, false);
                    IFeature fe2 = null;
                    while ((fe2 = feCursor2.NextFeature()) != null)
                    {
                        if(oid_GB.Keys.Contains(fe2.OID))
                        {
                            int gb0 = Int32.Parse(fe2.get_Value(lrdlGBIndex).ToString());
                            int newgb = oid_GB[fe2.OID];
                            fe2.set_Value(lrdlGBIndex,newgb);
                            string name0 = fe2.get_Value(lrdlNAMEIndex).ToString();
                            if (name0 != null)
                                fe2.set_Value(lrdlNAMEIndex, null); //RN为空的，原来街道的NAME也不要
                            fe2.Store();
                            updatedLRDLFIDList.Add(fe2.OID, string.Format("已更新(9)， GB:{0}→{1}", gb0,newgb));                                    
                        }
                        Marshal.ReleaseComObject(fe2);
                    }
                    Marshal.ReleaseComObject(feCursor2);
                    #endregion

                    wo.SetText("处理-第三阶段");
                    #region 3.3-街区外的街区道路，逐步向内传染，赋GB //算法是否有问题(暂时屏蔽)
                    /*
                    if (unUpdateFIDListOfTouch.Count > 0)
                    {
                        int unUpdateOfTouchCount = 0;
                        while (unUpdateOfTouchCount != unUpdateFIDListOfTouch.Count)
                        {
                            unUpdateOfTouchCount = unUpdateFIDListOfTouch.Count;//遍历前数量
                            var tempDic = new Dictionary<int, string>();

                            #region 循环遍历，解决与城际道路相邻街区道路
                            foreach (var item in unUpdateFIDListOfTouch)
                            {
                                fe = lrdlLayer.FeatureClass.GetFeature(item.Key);
                                if (oidOfInRESAList.Contains(fe.OID))
                                    continue;
                                int gbt = -1;
                                int.TryParse(fe.get_Value(lrdlGBIndex).ToString(), out gbt);

                                List<int> GBs = new List<int>();
                                IFeatureCursor feCursorTouch = lrdlLayer.Search(new SpatialFilterClass() { WhereClause = string.Format("{0} not in ({1},{2},{3},{4})  and (rn is null  or rn='' )  ", lrdlGBFN, 430200, 430501, 430502, 430502), Geometry = fe.ShapeCopy, SpatialRel = esriSpatialRelEnum.esriSpatialRelTouches }, true);
                                IFeature feTouch = null;
                                while ((feTouch = feCursorTouch.NextFeature()) != null)
                                {
                                    int gbtTouch = -1;
                                    int.TryParse(feTouch.get_Value(lrdlGBIndex).ToString(), out gbtTouch);
                                    if (gbtTouch > 1)
                                    {
                                        if (!GBs.Contains(gbtTouch))
                                            GBs.Add(gbtTouch);                                        
                                    }
                                }
                                Marshal.ReleaseComObject(feCursorTouch);

                                if (GBs.Count == 1)
                                {   
                                    int correctGB = GBs[0];
                                    fe.set_Value(lrdlGBIndex, correctGB);

                                    string name0 = fe.get_Value(lrdlNAMEIndex).ToString();
                                    if (name0 != null)
                                        fe.set_Value(lrdlNAMEIndex, null); //RN为空的，原来街道的NAME也不要

                                    fe.Store();

                                    updatedLRDLFIDList.Add(fe.OID, string.Format("已更新， GB从{0}更新为{1}", gbt, correctGB));
                                }
                                else if (GBs.Count == 0)
                                {
                                    tempDic.Add(fe.OID, string.Format("未更新，道路RN为空且没有找到与之相邻接的城际道路GB"));
                                }
                                else//多余1种的情况
                                {                                    
                                    string gbStrs = "";
                                    foreach (var gb00 in GBs)
                                    {
                                        gbStrs += String.Format(" {0}", gb00);
                                    }
                                    tempDic.Add(fe.OID, string.Format("未更新，道路RN为空且相邻接的城际道路GB/NAME不唯一" + gbStrs));
                                }
                            }
                            #endregion

                            unUpdateFIDListOfTouch = tempDic;

                        }

                    }
                    */
                    #endregion


                    wo.SetText("输出");
                    unUpdateLRDLCount = unUpdateFIDListOfTouch.Count + unUpdateFIDListOfRN.Count;
                    if (updatedLRDLFIDList.Count()>0 || unUpdateLRDLCount > 0)//输出文件，供用户判断
                    {
                        #region 输出修改情况的shp信息
                        ShapeFileWriter resultFile = new ShapeFileWriter();
                        Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                        fieldName2Len.Add("要素类名", 16);
                        fieldName2Len.Add("要素编号", 16);
                        fieldName2Len.Add("说明", 64);

                        resultFile.createErrorResutSHPFile(unUpdateReusltFileName, (lrdlLayer.FeatureClass as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);
                        //已更新的
                        /*
                        foreach (var item in updatedLRDLFIDList)
                        {
                            fe = lrdlLayer.FeatureClass.GetFeature(item.Key);

                            Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                            fieldName2FieldValue.Add("要素类名", lrdlLayer.FeatureClass.AliasName);
                            fieldName2FieldValue.Add("要素编号", item.Key.ToString());
                            fieldName2FieldValue.Add("说明", item.Value);

                            resultFile.addErrorGeometry(fe.ShapeCopy, fieldName2FieldValue);
                        }*/
                        //未更新的（邻接）
                        foreach (var item in unUpdateFIDListOfTouch)
                        {
                            fe = lrdlLayer.FeatureClass.GetFeature(item.Key);

                            Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                            fieldName2FieldValue.Add("要素类名", lrdlLayer.FeatureClass.AliasName);
                            fieldName2FieldValue.Add("要素编号", item.Key.ToString());
                            fieldName2FieldValue.Add("说明", item.Value);

                            resultFile.addErrorGeometry(fe.ShapeCopy, fieldName2FieldValue);
                        }
                        //未更新的（同RN）
                        foreach (var item in unUpdateFIDListOfRN)
                        {
                            fe = lrdlLayer.FeatureClass.GetFeature(item.Key);

                            Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                            fieldName2FieldValue.Add("要素类名", lrdlLayer.FeatureClass.AliasName);
                            fieldName2FieldValue.Add("要素编号", item.Key.ToString());
                            fieldName2FieldValue.Add("说明", item.Value);

                            resultFile.addErrorGeometry(fe.ShapeCopy, fieldName2FieldValue);
                        }

                        resultFile.saveErrorResutSHPFile();
                        #endregion
                    }
                }

                //提示是否加载导出的shp文件
                string info = "";
                if (unUpdateLRDLCount == 0)
                {
                    info = string.Format("已完成街区外道路GB更新,本次更新要素数量：【{0}】！", updatedLRDLFIDList.Count());
                }
                else
                {
                    info = string.Format("已完成街区外道路GB更新,本次更新要素数量：【{0}】,未更新要素数量:【{1}】！", updatedLRDLFIDList.Count(), unUpdateLRDLCount);
                }

                if (!File.Exists(unUpdateReusltFileName))
                {
                    MessageBox.Show(info);
                }
                else
                {
                    info += "\r\n是否加载本次未更新的要素信息？";
                    if (MessageBox.Show(info, "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        IFeatureClass errFC = CheckHelper.OpenSHPFile(unUpdateReusltFileName);
                        CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                    }
                }

                #endregion 

                                
                if (updatedLRDLFIDList.Count()>0)
                {
                    //1-正常结束，有修改，记录操作（可回退）
                    m_Application.EngineEditor.StopOperation(string.Format("街区外道路GB更新-{0}条", updatedLRDLFIDList.Count()));
                    m_Application.MapControl.ActiveView.Refresh();
                    //m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, lrdlLayer, m_Application.ActiveView.Extent);
                }
                else
                {
                    //2-正常结束，无修改，取消操作
                    m_Application.EngineEditor.AbortOperation(); 
                }               
                
            }
            catch (Exception ex)
            {
                //3-异常情况，取消操作
                m_Application.EngineEditor.AbortOperation();

                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (temp_intersectFC != null)
                {
                    (temp_intersectFC as IDataset).Delete();
                    temp_intersectFC = null;
                }
            }
        }
    }
}
