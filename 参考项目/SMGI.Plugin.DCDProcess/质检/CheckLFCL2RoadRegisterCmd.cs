using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using System.Data;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 检查道路附属设施线（LFCL）与道路套合关系的检查
    /// 铁路桥（450305）：检查其与LRRL中的要素是否套合；
    /// 公路桥（450306）：检查其与LRDL中的要素是否套合，同时检查其LGB赋值的合法性；
    /// 铁路公路两用桥(450307）：检查其与LRDL、LRRL中的要素是否套合，同时检查其LGB赋值的合法性；
    /// 火车隧道（450601）：检查其与LRRL中的要素是否套合；
    /// 汽车隧道（450602）：检查其与LRDL中的要素是否套合，同时检查其LGB赋值的合法性。
    /// </summary>
    public class CheckLFCL2RoadRegisterCmd : SMGI.Common.SMGICommand
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
            string lfclLyrName = "LFCL";
            IGeoFeatureLayer lfclLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == lfclLyrName);
            })).ToArray().First() as IGeoFeatureLayer;
            if (lfclLyr == null)
            {
                MessageBox.Show(string.Format("当前工作空间中没有找到图层【{0}】!", lfclLyrName));
                return;
            }
            IFeatureClass lfclFC = lfclLyr.FeatureClass;

            string lrdlLyrName = "LRDL";
            IGeoFeatureLayer lrdlLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == lrdlLyrName);
            })).ToArray().First() as IGeoFeatureLayer;
            if (lrdlLyr == null)
            {
                MessageBox.Show(string.Format("当前工作空间中没有找到图层【{0}】!", lrdlLyrName));
                return;
            }
            IFeatureClass lrdlFC = lrdlLyr.FeatureClass;

            string lrrlLyrName = "LRRL";
            IGeoFeatureLayer lrrlLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == lrrlLyrName);
            })).ToArray().First() as IGeoFeatureLayer;
            if (lrrlLyr == null)
            {
                MessageBox.Show(string.Format("当前工作空间中没有找到图层【{0}】!", lrrlLyrName));
                return;
            }
            IFeatureClass lrrlFC = lrrlLyr.FeatureClass;

            //LGB检查规则表
            string roadMDBPath = m_Application.Template.Root + "\\质检\\质检内容配置.mdb";
            string referenceScale = string.Empty;
            if (m_Application.MapControl.Map.ReferenceScale >= 50000)
            {
                referenceScale = "5W";
            }
            else if (m_Application.MapControl.Map.ReferenceScale == 10000)
            {
                referenceScale = "1W";
            }
            else
            {
                MessageBox.Show(string.Format("未找到当前设置的参考比例尺【{0}】对应的规则表!", m_Application.MapControl.Map.ReferenceScale));
                return;
            }
            string tableName = "桥隧LGB赋值检查_" + referenceScale;
            var lgbRuleTable = DCDHelper.ReadToDataTable(roadMDBPath, tableName);
            if (lgbRuleTable == null)
            {
                return;
            }

            string outPutFileName = OutputSetup.GetDir() + string.Format("\\桥隧与道路套合检查.shp");


            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                err = DoCheck(outPutFileName, lfclFC, lrdlFC, lrrlFC, lgbRuleTable, wo);
            }

            if (err == "")
            {
                IFeatureClass errFC = CheckHelper.OpenSHPFile(outPutFileName);
                int count = errFC.FeatureCount(null);
                if (count > 0)
                {
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

        public static string DoCheck(string resultSHPFileName, IFeatureClass lfclFC,
            IFeatureClass lrdlFC, IFeatureClass lrrlFC, DataTable lgbRuleTable, WaitOperation wo = null)
        {
            string err = "";

            int lfclGBIndex = lfclFC.FindField("GB");

            try
            {
                //新建结果文件
                ShapeFileWriter resultFile = new ShapeFileWriter();
                Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                fieldName2Len.Add("图层名", 16);
                fieldName2Len.Add("要素编号", 16);
                fieldName2Len.Add("说明", 32);
                fieldName2Len.Add("检查项", 32);
                
                resultFile.createErrorResutSHPFile(resultSHPFileName, (lfclFC as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);

                Dictionary<string, List<int>> errInfo2OIDList = new Dictionary<string, List<int>>();
                List<int> errOIDList;

                IQueryFilter qf = new QueryFilterClass();
                IQueryFilter qf2 = new QueryFilterClass();


                #region 铁路桥（450305）、火车隧道（450601）套合检查：返回所有不与LRRL要素套合的要素
                if (wo != null)
                    wo.SetText("正在检查铁路桥、火车隧道的套合情况......");

                qf.WhereClause = "(GB = 450305 OR GB = 450601)";
                if (lfclFC.HasCollabField())
                    qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;

                qf2.WhereClause = "";
                if (lrrlFC.HasCollabField())
                    qf2.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                errOIDList = CheckHelper.LineNotCoveredByLineClass(lfclFC, qf, lrrlFC, qf2);
                foreach (var errOID in errOIDList)
                {
                    IFeature fe = lfclFC.GetFeature(errOID);
                    string gb = fe.get_Value(lfclGBIndex).ToString();

                    if (gb == "450305")
                    {
                        if (!errInfo2OIDList.ContainsKey("铁路桥与道路不套合"))
                            errInfo2OIDList.Add("铁路桥与道路不套合", new List<int>());

                        errInfo2OIDList["铁路桥与道路不套合"].Add(fe.OID);
                    }
                    else if (gb == "450601")
                    {
                        if (!errInfo2OIDList.ContainsKey("火车隧道与道路不套合"))
                            errInfo2OIDList.Add("火车隧道与道路不套合", new List<int>());

                        errInfo2OIDList["火车隧道与道路不套合"].Add(fe.OID);
                    }
                }

                #endregion

                #region 公路桥（450306）、汽车隧道（450602）套合检查：返回所有不与LRDL要素套合的要素
                if (wo != null)
                    wo.SetText("正在检查公路桥、汽车隧道的套合情况......");

                qf.WhereClause = "(GB = 450306 OR GB = 450602)";
                if (lfclFC.HasCollabField())
                    qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;

                qf2.WhereClause = "";
                if (lrdlFC.HasCollabField())
                    qf2.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                errOIDList = CheckHelper.LineNotCoveredByLineClass(lfclFC, qf, lrdlFC, qf2);
                foreach (var errOID in errOIDList)
                {
                    IFeature fe = lfclFC.GetFeature(errOID);
                    string gb = fe.get_Value(lfclGBIndex).ToString();

                    if (gb == "450306")
                    {
                        if (!errInfo2OIDList.ContainsKey("公路桥与道路不套合"))
                            errInfo2OIDList.Add("公路桥与道路不套合", new List<int>());

                        errInfo2OIDList["公路桥与道路不套合"].Add(fe.OID);
                    }
                    else if (gb == "450602")
                    {
                        if (!errInfo2OIDList.ContainsKey("汽车隧道与道路不套合"))
                            errInfo2OIDList.Add("汽车隧道与道路不套合", new List<int>());

                        errInfo2OIDList["汽车隧道与道路不套合"].Add(fe.OID);
                    }
                }

                #endregion

                #region 铁路公路两用桥(450307）套合检查：返回所有不与LRDL要素套合公路桥要素
                if (wo != null)
                    wo.SetText("正在检查铁路公路两用桥的套合情况......");

                qf.WhereClause = "(GB = 450307)";
                if (lfclFC.HasCollabField())
                    qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;

                qf2.WhereClause = "";
                if (lrdlFC.HasCollabField())
                    qf2.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                //几何不套合的要素集合（与LRDL）
                var errOIDList1 = CheckHelper.LineNotCoveredByLineClass(lfclFC, qf, lrdlFC, qf2);


                qf2.WhereClause = "";
                if (lrrlFC.HasCollabField())
                    qf2.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                //几何不套合的要素集合（与LRRL）
                var errOIDList2 = CheckHelper.LineNotCoveredByLineClass(lfclFC, qf, lrrlFC, qf2);

                //合并
                errOIDList = new List<int>();
                if (errOIDList1.Count > 0 || errOIDList2.Count > 0)
                {
                    IFeatureCursor feCursor = lfclFC.Search(qf, true);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        if (errOIDList1.Contains(fe.OID) && errOIDList2.Contains(fe.OID))//与LRDL要素不套合、同时与LRRL要素也不套合
                        {
                            errOIDList.Add(fe.OID);
                        }
                    }
                    Marshal.ReleaseComObject(feCursor);

                    if (errOIDList.Count > 0)
                    {
                        errInfo2OIDList.Add("铁路公路两用桥与道路不套合", errOIDList);
                    }
                }

                #endregion
                
                #region 桥梁、隧道（几何套合的前提下）LGB合法性检查
                string lgbFN = GApplication.Application.TemplateManager.getFieldAliasName("LGB", lfclFC.AliasName);
                int lfclLGBIndex = lfclFC.FindField(lgbFN);

                lgbFN = GApplication.Application.TemplateManager.getFieldAliasName("LGB", lrdlFC.AliasName);
                int lrdlLGBIndex = lrdlFC.FindField(lgbFN);

                if (lgbRuleTable.Rows.Count > 0 && lfclGBIndex != -1 && 
                    lfclLGBIndex != -1 && lrdlLGBIndex != -1)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在进行LGB合法性检查......"));

                    #region 过滤条件
                    qf.WhereClause = "(GB = 450306 OR GB = 450307 OR GB = 450602)";//公路桥、公路铁路两用桥、汽车隧道
                    if (lfclFC.HasCollabField())
                        qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;

                    //几何套合，但LGB赋值不合法的要素集合
                    if (errInfo2OIDList.Count > 0)
                    {
                        string oidSet = "";
                        foreach (var item in errInfo2OIDList.Values)
                        {
                            foreach (var oid in item)
                            {
                                if (oidSet != "")
                                    oidSet += string.Format(",{0}", oid);
                                else
                                    oidSet = string.Format("{0}", oid);
                            }
                        }

                        qf.WhereClause += string.Format(" and (OBJECTID not in ({0}))", oidSet);//从几何套合的桥梁、隧道中查找LGB赋值不合法的要素
                    }

                    qf2.WhereClause = "";
                    if (lrdlFC.HasCollabField())
                        qf2.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                    #endregion

                    //拓扑检查
                    Dictionary<int, List<int>> overlapResult = CheckHelper.LineOverlapLine(lfclFC, qf, lrdlFC, qf2);

                    IFeature fe = null;
                    foreach (var item in overlapResult)
                    {
                        fe = lfclFC.GetFeature(item.Key);

                        ITopologicalOperator opLine = fe.Shape as ITopologicalOperator;

                        //LGB合法性判断
                        string gb = fe.get_Value(lfclGBIndex).ToString();
                        string lgb = fe.get_Value(lfclLGBIndex).ToString();
                        
                        //与其套合的道路LGB属性值
                        string matchRoadLGB = "-1";//默认为无效值

                        #region 求与其套合的道路LGB属性值
                        double maxMatchLen = 0;
                        IFeature lrdlFe = null;
                        foreach(var oid in item.Value)
                        {
                            lrdlFe = lrdlFC.GetFeature(oid);

                            string roadLGB = lrdlFe.get_Value(lrdlLGBIndex).ToString();

                            //取与之匹配度最高的道路LGB值
                            var difLine = opLine.Difference(lrdlFe.Shape) as IPolyline;
                            if (difLine.Length == 0)
                            {
                                matchRoadLGB = roadLGB;

                                break;
                            }
                            else
                            {
                                var matchLen = (fe.Shape as IPolyline).Length - difLine.Length;
                                if (matchLen > maxMatchLen)
                                {
                                    maxMatchLen = matchLen;
                                    matchRoadLGB = roadLGB;
                                }
                            }
                        }
                        #endregion

                        DataRow[] drArray = lgbRuleTable.Select().Where(i => i["BridgeGB"].ToString().Contains(gb) 
                            && i["RoadLGB"].ToString().Contains(matchRoadLGB)).ToArray();

                        bool bInvalid = false;
                        if (drArray.Length > 0)
                        {
                            string bridgeLGB = drArray[0]["BridgeLGB"].ToString().Trim();
                            if (lgb != bridgeLGB)
                                bInvalid = true;
                        }

                        if (bInvalid)//LGB不合法
                        {
                            if (gb == "450306")
                            {
                                if (!errInfo2OIDList.ContainsKey("公路桥的LGB赋值不合法"))
                                    errInfo2OIDList.Add("公路桥的LGB赋值不合法", new List<int>());

                                errInfo2OIDList["公路桥的LGB赋值不合法"].Add(fe.OID);
                            }
                            else if (gb == "450307")
                            {
                                if (!errInfo2OIDList.ContainsKey("铁路公路两用桥的LGB赋值不合法"))
                                    errInfo2OIDList.Add("铁路公路两用桥的LGB赋值不合法", new List<int>());

                                errInfo2OIDList["铁路公路两用桥的LGB赋值不合法"].Add(fe.OID);
                            }
                            else if (gb == "450602")
                            {
                                if (!errInfo2OIDList.ContainsKey("汽车隧道的LGB赋值不合法"))
                                    errInfo2OIDList.Add("汽车隧道的LGB赋值不合法", new List<int>());

                                errInfo2OIDList["汽车隧道的LGB赋值不合法"].Add(fe.OID);
                            }
                        }
                    }
                }
                #endregion

                if (wo != null)
                    wo.SetText("正在输出检查结果......");
                foreach (var item in errInfo2OIDList)
                {
                    if (item.Value.Count == 0)
                        continue;

                    string oidSet = "";
                    foreach (var oid in item.Value)
                    {
                        if (oidSet != "")
                            oidSet += string.Format(",{0}", oid);
                        else
                            oidSet = string.Format("{0}", oid);
                    }

                    IFeatureCursor illegalFeCursor = lfclFC.Search(new QueryFilterClass() { WhereClause = string.Format("OBJECTID in ({0})", oidSet) }, true);
                    IFeature illegalFe = null;
                    while ((illegalFe = illegalFeCursor.NextFeature()) != null)
                    {
                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("图层名", lfclFC.AliasName);
                        fieldName2FieldValue.Add("要素编号", illegalFe.OID.ToString());
                        fieldName2FieldValue.Add("说明", item.Key);
                        fieldName2FieldValue.Add("检查项", "道路辅助设施线与道路套合检查");
                        

                        resultFile.addErrorGeometry(illegalFe.Shape, fieldName2FieldValue);

                    }
                    Marshal.ReleaseComObject(illegalFeCursor);

                }

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
    }
}
