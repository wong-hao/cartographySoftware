using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Data;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.DataSourcesFile;
using SMGI.Common;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.DCDProcess
{
    public class CulvertCheckClass : SMGICommand
    {
        public List<int> errHYDLList;
        public List<int> errLRDLList;
        public List<int> errLRRLList;
        public List<int> errList;
        public List<int> errListHYDL;
        public List<int> errListDistance;
        public string outPutFileName;
        public IFeatureClass shpFeatureClass;
        public List<int> allCulvertOID;
        public List<string> fcls;

        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            if (m_Application.ActiveView.FocusMap.ReferenceScale < 1e-2)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;
            }
            double scale = m_Application.ActiveView.FocusMap.ReferenceScale;
            double step = 0.01 * scale / 1000;
            errList = new List<int>();
            errListHYDL = new List<int>();
            errListDistance = new List<int>();
            errHYDLList = new List<int>();
            errLRDLList = new List<int>();
            errLRRLList = new List<int>();
            allCulvertOID = new List<int>();
            fcls = new List<string>();
            fcls.Add("LRDL");
            fcls.Add("HYDL");
            fcls.Add("LRRL");

            using (var wo = m_Application.SetBusy())
            {
                wo.SetText("开始检查");
                outPutFileName = OutputSetup.GetDir() + string.Format("\\涵洞水系道路检查.shp");
                IFeatureWorkspace pWorkspace = m_Application.Workspace.EsriWorkspace as IFeatureWorkspace;
                IFeatureClass culvertFCL = pWorkspace.OpenFeatureClass("HFCP");
                allCulvertOID = GetAllCulvertOID(culvertFCL);
                wo.SetText("正在进行涵洞与水系道路关系检查...请等待");
                foreach (var fcl in fcls)
                {
                    var tempFCL = (m_Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(fcl);
                    
                    if (fcl == "LRDL")
                    {
                        errLRDLList = CheckPointCoverd(culvertFCL, tempFCL, wo); //GetIntersectCluvertOID(culvertFCL, tempFCL);
                    }
                    else if (fcl == "HYDL")
                    {
                        errHYDLList = CheckPointCoverd(culvertFCL, tempFCL, wo); //GetIntersectCluvertOID(culvertFCL, tempFCL);
                    }
                    else if(fcl == "LRRL")
                    {
                        errLRRLList = CheckPointCoverd(culvertFCL, tempFCL, wo);
                    }
                }

                foreach (var item in errHYDLList)
                {
                    wo.SetText("正在检查：" + item.ToString());
                    try
                    {
                        allCulvertOID.Remove(item);
                    }
                    catch
                    {

                    }
                }

                foreach (var item in errLRDLList)
                {
                    wo.SetText("正在检查：" + item.ToString());
                    try
                    {
                        allCulvertOID.Remove(item);
                    }
                    catch
                    {

                    }
                }

                foreach (var item in errLRRLList)
                {
                    wo.SetText("正在检查：" + item.ToString());
                    try
                    {
                        allCulvertOID.Remove(item);
                    }
                    catch
                    {

                    }
                }

                foreach (var item in allCulvertOID)
                {
                    wo.SetText("正在检查：" + item.ToString());
                    if (!errList.Contains(item))
                    {
                        errList.Add(item);
                    }
                }
                
                //添加交点30m范围线上的点
                List<int> zl = errLRDLList.Union(errHYDLList).ToList<int>();
                List<int> zlz = zl.Union(errLRRLList).ToList<int>();

                IFeatureClass culvertLRDL = pWorkspace.OpenFeatureClass("LRDL");
                IFeatureClass culvertLRRL = pWorkspace.OpenFeatureClass("LRRL");
                IFeatureClass culvertHYDL = pWorkspace.OpenFeatureClass("HYDL");
                foreach (var item in zlz)
                {
                    //if (item == 35590)
                    //{
                    //    MessageBox.Show("");
                    //}
                    try
                    {
                    wo.SetText("正在检查："+item.ToString());
                    int n1 = 0, n2 = 0, n3 = 0;
                    List<IFeature> pLIF = new List<IFeature>();
                    IFeature pfectpl = culvertFCL.GetFeature(item);
                    //IFeatureCursor cursorctpl = culvertLRDL.Search(null, false);
                    //while ((pfectpl = cursorctpl.NextFeature()) != null)
                    
                    ISpatialFilter qf = new SpatialFilter();
                    qf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    qf.Geometry = (pfectpl.ShapeCopy as ITopologicalOperator).Buffer(30);
                    if (culvertLRDL.HasCollabField())//已删除的要素不参与
                    {
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                    }

                    IFeatureCursor cursor = culvertLRDL.Search(qf, false);
                    IFeature pf = null;
                    while ((pf = cursor.NextFeature()) != null) 
                    {
                        pLIF.Add(pf);
                        n1++;
                        
                    } System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);


                    qf.WhereClause = "";
                    if (culvertLRRL.HasCollabField())//已删除的要素不参与
                    {
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                    }
                    IFeatureCursor cursorLe = culvertLRRL.Search(qf, false);
                    IFeature pfLr = null;
                    while ((pfLr = cursorLe.NextFeature()) != null)
                    {
                        pLIF.Add(pfLr);
                        n2++;
                        
                    } System.Runtime.InteropServices.Marshal.ReleaseComObject(cursorLe);

                    qf.WhereClause = "";
                    if (culvertHYDL.HasCollabField())//已删除的要素不参与
                    {
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                    }
                    IFeatureCursor cursorHy = culvertHYDL.Search(qf, false);
                    IFeature pfHy = null;
                    while ((pfHy = cursorHy.NextFeature()) != null)
                    {
                        pLIF.Add(pfHy);
                        n3++;
                        
                    } System.Runtime.InteropServices.Marshal.ReleaseComObject(cursorHy);
                    if (n1 == 1 && n2 == 0 && n3 == 0)
                    { continue; }
                    bool pand = true;
                    for (int i = 0; i < pLIF.Count - 1; i++)
                    {
                        string layerName = (pLIF[i].Class as IFeatureClass).AliasName;
                        if (layerName.ToUpper() == "HYDL")
                        {
                            break;
                        }
                        for (int n = i + 1; n < pLIF.Count;n++ )
                        {
                            ITopologicalOperator topoOperator = pLIF[i].ShapeCopy as ITopologicalOperator;
                            IGeometry geo = topoOperator.Intersect(pLIF[n].ShapeCopy, esriGeometryDimension.esriGeometry0Dimension);
                            if (!geo.IsEmpty)
                            {
                                IPointCollection Pc = geo as IPointCollection;
                                for (int j = 0; j < Pc.PointCount; j++)
                                {
                                    IPoint pt = Pc.get_Point(j);
                                    IProximityOperator ProxiOP = (pt) as IProximityOperator;
                                    if (ProxiOP.ReturnDistance(pfectpl.ShapeCopy) <= step)
                                    {
                                        pand = false;
                                        continue;
                                    }
                                    if (ProxiOP.ReturnDistance(pfectpl.ShapeCopy) < 30)
                                    {
                                        if (!errListDistance.Contains(item))
                                        {
                                            errListDistance.Add(item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (pand == false && errListDistance.Contains(item))
                    {
                        errListDistance.Remove(item);
                    }
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                foreach (var item in errHYDLList)
                {
                    if (errLRDLList.Contains(item) || errLRRLList.Contains(item) || errListDistance.Contains(item))
                    {
                        continue;
                    }
                    errListHYDL.Add(item);

                }



                ShapeFileWriter resultFile = new ShapeFileWriter();
                var temp = errHYDLList.Select(i => i).Intersect(errLRDLList);
                Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                fieldName2Len.Add("图层名", 20);
                fieldName2Len.Add("涵洞编号", 20);
                fieldName2Len.Add("问题描述", 40);
                var isSuccess = resultFile.createErrorResutSHPFile(outPutFileName, (culvertFCL as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPoint, fieldName2Len);
                if (isSuccess)
                {
                    IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactoryClass();
                    IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(outPutFileName.Substring(0, outPutFileName.LastIndexOf("\\")), 0);
                    shpFeatureClass = pFeatureWorkspace.OpenFeatureClass("涵洞水系道路检查");
                    using (ComReleaser comReleaser = new ComReleaser())
                    {
                        IFeatureBuffer tempFeature = shpFeatureClass.CreateFeatureBuffer();
                        comReleaser.ManageLifetime(tempFeature);
                        IFeatureCursor insertCursor = shpFeatureClass.Insert(true);
                        comReleaser.ManageLifetime(insertCursor);

                        int verIndex = culvertFCL.Fields.FindField(cmdUpdateRecord.CollabVERSION);
                        int delIndex = culvertFCL.Fields.FindField(cmdUpdateRecord.CollabDELSTATE);
                        foreach (var item in errList)
                        {
                            wo.SetText("正在生成检查报告");
                            if (verIndex != -1 && delIndex != -1)
                            {
                                int smgiver = 0;
                                int.TryParse(culvertFCL.GetFeature(item).get_Value(verIndex).ToString(), out smgiver);
                                string delState = culvertFCL.GetFeature(item).get_Value(delIndex).ToString();
                                if (smgiver < 0 && delState == cmdUpdateRecord.DelStateText)//删除要素
                                {
                                    continue;
                                }
                            }

                            tempFeature.set_Value(tempFeature.Fields.FindField("图层名"), "HFCP");
                            tempFeature.set_Value(tempFeature.Fields.FindField("涵洞编号"), item.ToString());
                            tempFeature.set_Value(tempFeature.Fields.FindField("问题描述"), "涵洞没在水系或道路线上");
                            tempFeature.Shape = culvertFCL.GetFeature(item).Shape;

                            insertCursor.InsertFeature(tempFeature);
                        }
                        foreach (var item in errListHYDL) 
                        {
                            wo.SetText("正在生成检查报告");
                            if (verIndex != -1 && delIndex != -1)
                            {
                                int smgiver = 0;
                                int.TryParse(culvertFCL.GetFeature(item).get_Value(verIndex).ToString(), out smgiver);
                                string delState = culvertFCL.GetFeature(item).get_Value(delIndex).ToString();
                                if (smgiver < 0 && delState == cmdUpdateRecord.DelStateText)//删除要素
                                {
                                    continue;
                                }
                            }

                            tempFeature.set_Value(tempFeature.Fields.FindField("图层名"), "HFCP");
                            tempFeature.set_Value(tempFeature.Fields.FindField("涵洞编号"), item.ToString());
                            tempFeature.set_Value(tempFeature.Fields.FindField("问题描述"), "涵洞仅在水系上");
                            tempFeature.Shape = culvertFCL.GetFeature(item).Shape;

                            insertCursor.InsertFeature(tempFeature);
                            
                        }
                        foreach (var item in errListDistance)
                        {
                            wo.SetText("正在生成检查报告");
                            if (verIndex != -1 && delIndex != -1)
                            {
                                int smgiver = 0;
                                int.TryParse(culvertFCL.GetFeature(item).get_Value(verIndex).ToString(), out smgiver);
                                string delState = culvertFCL.GetFeature(item).get_Value(delIndex).ToString();
                                if (smgiver < 0 && delState == cmdUpdateRecord.DelStateText)//删除要素
                                {
                                    continue;
                                }
                            }

                            tempFeature.set_Value(tempFeature.Fields.FindField("图层名"), "HFCP");
                            tempFeature.set_Value(tempFeature.Fields.FindField("涵洞编号"), item.ToString());
                            tempFeature.set_Value(tempFeature.Fields.FindField("问题描述"), "涵洞在水系或道路线上，距离交点小于30m");
                            tempFeature.Shape = culvertFCL.GetFeature(item).Shape;

                            insertCursor.InsertFeature(tempFeature);
                        }
                        insertCursor.Flush();
                        Marshal.ReleaseComObject(insertCursor);
                        GC.Collect();
                    }
                    Marshal.ReleaseComObject(pFeatureWorkspace);
                    Marshal.ReleaseComObject(pWorkspaceFactory);

                }
            }
            if (shpFeatureClass.FeatureCount(null) > 0)
            {
                if (MessageBox.Show("检查完成！是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    CheckHelper.AddTempLayerFromSHPFile(m_Application.Workspace.LayerManager.Map, outPutFileName);

                }
            }
            else
            {
                MessageBox.Show("涵洞检查结果为0");
            }
        }

        public List<int> GetAllCulvertOID(IFeatureClass fcl)
        {
            try
            {
                List<int> oidList = new List<int>();
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "GB = 220900";
                IFeature fe = null;
                IFeatureCursor cursor = fcl.Search(qf,true);
                while ((fe = cursor.NextFeature()) != null)
                {
                    if(!oidList.Contains(fe.OID))
                    {
                        oidList.Add(fe.OID);
                    }
                }
                Marshal.ReleaseComObject(cursor);
                return oidList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return null;
            }
        }


        public List<int> GetIntersectCluvertOID(IFeatureClass fcl, IFeatureClass compareFCL)
        {
            try
            {
                List<int> tempList = new List<int>();
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "GB = 220900";
                IFeature fe = null;
                IFeatureCursor cursor = fcl.Search(qf, true);
                while ((fe = cursor.NextFeature()) != null)
                {
                   ISpatialFilter sp = new SpatialFilter();
                    sp.Geometry = fe.Shape;
                    sp.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    IFeature compareFE;
                    IFeatureCursor fecursor = compareFCL.Search(sp, true);
                    while ((compareFE = fecursor.NextFeature()) != null)
                    {
                        if (!tempList.Contains(fe.OID))
                        {
                            tempList.Add(fe.OID);
                        }
                    }                    
                    Marshal.ReleaseComObject(fecursor);
                }               
                Marshal.ReleaseComObject(cursor);
                return tempList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return null;
            }
        }

        #region 拓扑相关函数
        public static ITopology OpenTopology(IFeatureWorkspace featureWorkspace, string featureDatasetName, string topologyName)
        {
            IFeatureDataset featureDataset = featureWorkspace.OpenFeatureDataset(featureDatasetName);
            ITopologyContainer topologyContainer = (ITopologyContainer)featureDataset;
            ITopology topology = topologyContainer.get_TopologyByName(topologyName);
            return topology;
        }

        public static void AddRuleToTopology(ITopology topology, esriTopologyRuleType ruleType, String ruleName, IFeatureClass featureClass, IFeatureClass oFeatureClass)
        {

            ITopologyRule topologyRule = new TopologyRuleClass();
            topologyRule.TopologyRuleType = ruleType;
            topologyRule.Name = ruleName;
            topologyRule.OriginClassID = featureClass.FeatureClassID;
            topologyRule.OriginSubtype = 1;
            topologyRule.DestinationClassID = oFeatureClass.FeatureClassID;
            topologyRule.DestinationSubtype = 1;
            topologyRule.AllOriginSubtypes = true;
            topologyRule.AllDestinationSubtypes = true;
            ITopologyRuleContainer topologyRuleContainer = (ITopologyRuleContainer)topology;
            if (topologyRuleContainer.get_CanAddRule(topologyRule))
            {
                topologyRuleContainer.AddRule(topologyRule);
            }
            else
            {
                throw new ArgumentException("Could not add specified rule to the topology.");
            }
        }

        public static void ValidateTopology(ITopology topology, IEnvelope envelope)
        {
            IPolygon locationPolygon = new PolygonClass();
            ISegmentCollection segmentCollection = (ISegmentCollection)locationPolygon;
            segmentCollection.SetRectangle(envelope);
            IPolygon polygon = topology.get_DirtyArea(locationPolygon);
            {
                IEnvelope areaToValidate = polygon.Envelope;
                IEnvelope areaValidated = topology.ValidateTopology(areaToValidate);
            }
        }
        #endregion

        public List<int> CheckPointCoverd(IFeatureClass fcls,IFeatureClass lineFCL,WaitOperation wot)
        {
            ITopology topology = null;
            try
            {
                var tempList = GetAllCulvertOID(fcls);
                List<int> oidList = new List<int>();
                ITopologyContainer2 topologyContainer = (ITopologyContainer2)fcls.FeatureDataset;
                topology = topologyContainer.CreateTopology("点在线上检查", topologyContainer.DefaultClusterTolerance, -1, "");
                topology.AddClass(fcls, 5, 1, 1, false);
                topology.AddClass(lineFCL, 5, 1, 1, false);
                AddRuleToTopology(topology, esriTopologyRuleType.esriTRTPointCoveredByLine, "Point Must Be Covered By Line",fcls, lineFCL);
                IGeoDataset geoDataset = (IGeoDataset)topology;
                IEnvelope envelope = geoDataset.Extent;
                ValidateTopology(topology, envelope);

                ITopology topologyChecke = OpenTopology((fcls.FeatureDataset.Workspace as IFeatureWorkspace), fcls.FeatureDataset.Name, "点在线上检查");
                IErrorFeatureContainer errorFeatureContainer = (IErrorFeatureContainer)topologyChecke;
                IGeoDataset geoDatasetTopo = (IGeoDataset)topologyChecke;
                ISpatialReference SpatialReference = geoDatasetTopo.SpatialReference;

                IEnumTopologyErrorFeature enumTopologyErrorFeature = errorFeatureContainer.get_ErrorFeaturesByRuleType(SpatialReference, esriTopologyRuleType.esriTRTPointCoveredByLine, geoDatasetTopo.Extent, true, false);
                ITopologyErrorFeature topologyErrorFeature = null;
                while ((topologyErrorFeature = enumTopologyErrorFeature.Next()) != null)
                {
                    wot.SetText("正在检查：" + topologyErrorFeature.OriginOID.ToString());
                    IFeature SelectFeature = fcls.GetFeature(int.Parse(topologyErrorFeature.OriginOID + ""));
                    //IFeature DestinationFeature = lineFCL.GetFeature(int.Parse(topologyErrorFeature.DestinationOID + ""));
                            //IFeature storeFeature = shpCls.CreateFeature();
                    //int smgiver = 0;
                    //int.TryParse(SelectFeature.get_Value(SelectFeature.Fields.FindField(cmdUpdateRecord.CollabVERSION)).ToString(), out smgiver);
                    //if (smgiver != -int.MaxValue)//&& int.Parse(DestinationFeature.get_Value(DestinationFeature.Fields.FindField(cmdUpdateRecord.CollabVERSION)).ToString()) != -int.MaxValue)
                    //{
                    oidList.Add(SelectFeature.OID);
                    //}
                }//删除临时拓扑
                topology.RemoveClass(fcls);
                topology.RemoveClass(lineFCL);
                (topology as IDataset).Delete();
                foreach (var item in oidList)
                {
                    wot.SetText("正在检查：" + item.ToString());
                    tempList.Remove(item);
                }
                return tempList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                if (topology != null)
                {
                    //删除临时拓扑
                    topology.RemoveClass(fcls);
                    topology.RemoveClass(lineFCL);
                    (topology as IDataset).Delete();
                }
                MessageBox.Show(ex.Message);

                return null;
            }
        }

    }
}
