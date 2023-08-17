using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using System.IO;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 河流流向检查（LZ）
    /// 要求：1.检查的目标要素类为HYDL；2.必须存在HGB字段，且为整形；3.只对自然河流检查（HGB以21打头(优先HGB,若HGB不存在则根据GB筛
    /// 环形流向：河流线构面，在参与构面的河流线中进行检查，看是否存在环流情况，若存在，则报出参与构面的所有河流线供用户判断
    /// 对流流向：检测所有符合条件的自然河流，若某端点处仅存两条以上在上游河流，则报出疑似错误；
    /// </summary>
    public class CheckRiverDirectionCmd : SMGI.Common.SMGICommand
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
            string fcName = "HYDL";
            IGeoFeatureLayer lyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IGeoFeatureLayer;
            if (lyr == null)
            {
                MessageBox.Show(string.Format("当前工作空间中没有找到要素类【{0}】!", fcName));
                return;
            }
            IFeatureClass fc = lyr.FeatureClass;

            string filterFN = "";
            if(fc.Fields.FindField("HGB") != -1)
            {
                filterFN = "HGB";
            }
            else if(fc.Fields.FindField("GB") != -1)
            {
                filterFN = "GB";
            }
            else
            {
                MessageBox.Show(string.Format("当前工作空间中没有找到要素类【{0}】!", fcName));
                return;
            }

            string outputFileName = OutputSetup.GetDir() + string.Format("\\河流流向检查.shp");

            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                string filter = string.Format("{0} < 220000", filterFN);//自然河流
                if (fc.HasCollabField())
                    filter += " and " + cmdUpdateRecord.CurFeatureFilter;

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = filter;

                err = DoCheck(outputFileName, fc, qf, wo);

            }

            if (err == "")
            {
                if (File.Exists(outputFileName))
                {
                    if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName);
                        CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                    }
                }
                else
                {
                    MessageBox.Show("检查完毕，没有发现非法要素！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }
        }

        public static string DoCheck(string resultSHPFileName, IFeatureClass fc, IQueryFilter qf, WaitOperation wo = null)
        {
            string err = "";

            Geoprocessor gp = new Geoprocessor();
            IFeatureClass tempAreaFC = null;
            IFeatureClass tempStartPtFC = null;
            IFeatureClass tempEndPtFC = null;
            try
            {
                ShapeFileWriter resultFile = null;

                //创建临时数据库
                string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
                IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);

                gp.OverwriteOutput = true;
                gp.SetEnvironmentValue("workspace", ws.PathName);

                ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer makeFeatureLayer = new ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer();
                makeFeatureLayer.in_features = fc;
                makeFeatureLayer.out_layer = fc.AliasName + "_Layer";
                SMGI.Common.Helper.ExecuteGPTool(gp, makeFeatureLayer, null);

                ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute selectLayerByAttribute = new ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute();
                selectLayerByAttribute.in_layer_or_view = fc.AliasName + "_Layer";
                selectLayerByAttribute.where_clause = qf.WhereClause;
                SMGI.Common.Helper.ExecuteGPTool(gp, selectLayerByAttribute, null);


                if (true)
                {
                    #region 检测环形流向
                    if (wo != null)
                        wo.SetText("正在查找环形河流......");
                    //线转面
                    FeatureToPolygon feToPolygon = new FeatureToPolygon();
                    feToPolygon.in_features = fc.AliasName + "_Layer";
                    feToPolygon.out_feature_class = fc.AliasName + "_LineToPolygon";
                    SMGI.Common.Helper.ExecuteGPTool(gp, feToPolygon, null);

                    tempAreaFC = (ws as IFeatureWorkspace).OpenFeatureClass(fc.AliasName + "_LineToPolygon");

                    IFeatureCursor areaFeCursor = tempAreaFC.Search(null, true);
                    IFeature areaFe = null;
                    while ((areaFe = areaFeCursor.NextFeature()) != null)
                    {
                        if (areaFe.Shape == null || areaFe.Shape.IsEmpty)
                            continue;//空几何不参与

                        //查找组成面的河流线
                        List<IFeature> feList = new List<IFeature>();
                        ISpatialFilter sf = new SpatialFilter();
                        sf.Geometry = areaFe.Shape;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelRelation;
                        sf.SpatialRelDescription = "***T*****";
                        IFeatureCursor feCursor = fc.Search(sf, false);
                        IFeature fe = null;
                        while ((fe = feCursor.NextFeature()) != null)
                        {
                            feList.Add(fe);
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);

                        //判断是否为环形河流
                        bool isAnnularRiver = false;
                        int annularFeCount = 0;
                        TraversalRiver(feList, feList[0], ref annularFeCount);
                        if (feList.Count == 1 || annularFeCount >= feList.Count)
                            isAnnularRiver = true;
                        if (isAnnularRiver)
                        {
                            if (resultFile == null)
                            {
                                //建立结果文件
                                resultFile = new ShapeFileWriter();
                                Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                                fieldName2Len.Add("图层名", 16);
                                fieldName2Len.Add("编号列表", 256);
                                fieldName2Len.Add("检查项", 16);
                                fieldName2Len.Add("说明", 32);
                                resultFile.createErrorResutSHPFile(resultSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);
                            }

                            string oidListStr = "";
                            foreach (var item in feList)
                            {
                                if (oidListStr == "")
                                {
                                    oidListStr = item.OID.ToString();
                                }
                                else
                                {
                                    oidListStr += "," + item.OID.ToString();
                                }

                                Marshal.ReleaseComObject(item);
                            }
                            IPolyline b = (areaFe.Shape as ITopologicalOperator).Boundary as IPolyline;

                            Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                            fieldName2FieldValue.Add("图层名", fc.AliasName);
                            fieldName2FieldValue.Add("编号列表", string.Format("{0}", oidListStr));
                            fieldName2FieldValue.Add("检查项", "河流流向检查");
                            fieldName2FieldValue.Add("说明", "环形流向");
                            resultFile.addErrorGeometry(b, fieldName2FieldValue);
                        }

                        Marshal.ReleaseComObject(areaFe);

                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(areaFeCursor);
                    #endregion
                }

                //内存监控
                if (Environment.WorkingSet > DCDHelper.MaxMem)
                {
                    GC.Collect();
                }

                if (true)
                {
                    #region 检测流向对流：多个河流汇入同一点，但该点没有流出河流
                    if (wo != null)
                        wo.SetText("正在查找流向冲突河流......");

                    //节点（起点\终点）转点
                    FeatureVerticesToPoints vert2pt = new FeatureVerticesToPoints();
                    vert2pt.in_features = fc.AliasName + "_Layer";

                    vert2pt.out_feature_class = fc.AliasName + "_StartPtToPoints";
                    vert2pt.point_location = "START";
                    SMGI.Common.Helper.ExecuteGPTool(gp, vert2pt, null);

                    tempStartPtFC = (ws as IFeatureWorkspace).OpenFeatureClass(fc.AliasName + "_StartPtToPoints");

                    vert2pt.out_feature_class = fc.AliasName + "_EndPtToPoints";
                    vert2pt.point_location = "END";
                    SMGI.Common.Helper.ExecuteGPTool(gp, vert2pt, null);

                    tempEndPtFC = (ws as IFeatureWorkspace).OpenFeatureClass(fc.AliasName + "_EndPtToPoints");

                    Dictionary<string, string> endCoord2FIDListStr = new Dictionary<string, string>();
                    #region 收集终点信息,同时排除存在下游河流的终点信息
                    //收集终点信息
                    IFeatureCursor ptFeCursor = tempEndPtFC.Search(null, true);
                    IFeature ptFe = null;
                    int origFIDIndex = tempEndPtFC.FindField("ORIG_FID");
                    while ((ptFe = ptFeCursor.NextFeature()) != null)
                    {
                        if (ptFe.Shape == null || ptFe.Shape.IsEmpty)
                            continue;//空几何不参与

                        IPoint pt = ptFe.Shape as IPoint;
                        int origOID = -1;
                        int.TryParse(ptFe.get_Value(origFIDIndex).ToString(), out origOID);

                        string coordStr = pt.X.ToString("#.###") + "," + pt.Y.ToString("#.###");
                        if (!endCoord2FIDListStr.ContainsKey(coordStr))
                        {
                            endCoord2FIDListStr.Add(coordStr, origOID.ToString());
                        }
                        else
                        {
                            var oidListStr = endCoord2FIDListStr[coordStr];
                            oidListStr += "," + origOID.ToString();

                            endCoord2FIDListStr[coordStr] = oidListStr;
                        }

                        Marshal.ReleaseComObject(ptFe);

                    }
                    Marshal.ReleaseComObject(ptFeCursor);

                    //利用起点信息，排除存在下游河流的终点信息
                    ptFeCursor = tempStartPtFC.Search(null, true);
                    origFIDIndex = tempStartPtFC.FindField("ORIG_FID");
                    while ((ptFe = ptFeCursor.NextFeature()) != null)
                    {
                        if (ptFe.Shape == null || ptFe.Shape.IsEmpty)
                            continue;//空几何不参与

                        IPoint pt = ptFe.Shape as IPoint;
                        int origOID = -1;
                        int.TryParse(ptFe.get_Value(origFIDIndex).ToString(), out origOID);

                        string coordStr = pt.X.ToString("#.###") + "," + pt.Y.ToString("#.###");
                        if (endCoord2FIDListStr.ContainsKey(coordStr))//该终点存在下游河流，可排除
                        {
                            endCoord2FIDListStr.Remove(coordStr);
                        }


                        Marshal.ReleaseComObject(ptFe);
                    }
                    Marshal.ReleaseComObject(ptFeCursor);


                    #endregion

                    //遍历终点信息,收集错误信息
                    Dictionary<string, string> conflictDirectInfo = new Dictionary<string, string>();
                    foreach (var kv in endCoord2FIDListStr)
                    {
                        if (kv.Value.Contains(","))//流入河流大于一条，同时没有下游河流
                        {
                            conflictDirectInfo.Add(kv.Key, kv.Value);
                        }
                    }

                    //输出
                    if (conflictDirectInfo.Count > 0)
                    {
                        if (resultFile == null)
                        {
                            //建立结果文件
                            resultFile = new ShapeFileWriter();
                            Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                            fieldName2Len.Add("图层名", 16);
                            fieldName2Len.Add("编号列表", 256);
                            fieldName2Len.Add("检查项", 16);
                            fieldName2Len.Add("说明", 32);
                            resultFile.createErrorResutSHPFile(resultSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);
                        }

                        foreach (var kv in conflictDirectInfo)
                        {
                            var oidListStr = kv.Value;

                            IPolyline shape = null;
                            string[] oidList = oidListStr.Split(',');
                            foreach (var item in oidList)
                            {
                                IFeature fe = fc.GetFeature(int.Parse(item));
                                if (fe == null)
                                    continue;

                                if (shape == null)
                                {
                                    shape = fe.Shape as IPolyline;
                                }
                                else
                                {
                                    ITopologicalOperator topologicalOperator = shape as ITopologicalOperator;
                                    shape = topologicalOperator.Union(fe.Shape) as IPolyline;
                                }
                                Marshal.ReleaseComObject(fe);
                            }

                            Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                            fieldName2FieldValue.Add("图层名", fc.AliasName);
                            fieldName2FieldValue.Add("编号列表", string.Format("{0}", oidListStr));
                            fieldName2FieldValue.Add("检查项", "河流流向检查");
                            fieldName2FieldValue.Add("说明", "对流流向");
                            resultFile.addErrorGeometry(shape, fieldName2FieldValue);
                        }
                    }

                    #endregion
                }

                //保存结果文件
                if (resultFile != null)
                {
                    resultFile.saveErrorResutSHPFile();
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
                if (tempAreaFC != null)
                    (tempAreaFC as IDataset).Delete();
                if (tempStartPtFC != null)
                    (tempStartPtFC as IDataset).Delete();
                if (tempEndPtFC != null)
                    (tempEndPtFC as IDataset).Delete();
            }

            return err;
        }

        public static void TraversalRiver(List<IFeature> feList, IFeature curFe, ref int annularFeCount)
        {
            if (annularFeCount >= feList.Count)
                return;

            IPoint toPoint = (curFe.Shape as IPolyline).ToPoint;
            List<int> feOIDList = new List<int>();
            foreach (var item in feList)
            {
                if (curFe.OID == item.OID)
                    continue;

                IPoint fromPoint = (item.Shape as IPolyline).FromPoint;
                if (Math.Abs((toPoint.X - fromPoint.X)) < 0.001 &&
                    Math.Abs((toPoint.Y - fromPoint.Y)) < 0.001)//找到当前要素的下游河流
                {
                    annularFeCount++;

                    TraversalRiver(feList, item, ref annularFeCount);
                }
            }

            return;
        }

    }
}
