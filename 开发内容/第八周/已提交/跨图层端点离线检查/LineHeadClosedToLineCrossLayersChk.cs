using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.AnalysisTools;
using SMGI.Plugin.DCDProcess;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 悬挂点检查(点离线)
    /// </summary>
    public class LineHeadClosedToLineCrossLayersChk
    {
        ISpatialReference _spatialReference;

        List<Tuple<string, IGeometry>> _errorInfoList = new List<Tuple<string, IGeometry>>();

        public int Count
        {
            get { return _errorInfoList.Count; }
        }

        /// <summary>
        /// 执行检查
        /// </summary>
        /// <param name="fcls"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public ResultMessage Check(IFeatureClass fcls, IEnvelope env, double disValue, WaitOperation wo = null)
        {
            if (fcls == null)
            {
                //停止函数执行
            }
            var geod = fcls as IGeoDataset;
            _spatialReference = geod.SpatialReference;

            ResultMessage rm = Analysis(fcls, env, disValue, wo);
            return rm;
        }

        /// <summary>
        /// 保存质检结果 +2
        /// </summary>
        /// <param name="dir">输出目录</param>
        /// <param name="bAutoOver"></param>
        /// <returns></returns>
        public ResultMessage SaveResult(string layername, string dir, bool bAutoOver = false, bool toGdb = false)
        {
            //保存shp文件
            ResultMessage rm = new ResultMessage() { stat = ResultState.Ok };
            try
            {
                if (_errorInfoList.Count > 0)
                {
                    var shpWriter = new ShapeFileWriter();
                    var shpdir = dir;
                    if (!Directory.Exists(shpdir))
                        Directory.CreateDirectory(shpdir);
                    var sf = shpdir + "\\跨图层端点离线检查_" + layername;
                    shpWriter.createErrorResutSHPFile(sf, _spatialReference,
                        esriGeometryType.esriGeometryMultipoint,
                        new Dictionary<string, int> { { "图层名", 20 }, { "编号", 60 }, { "检查项", 8 } }, bAutoOver);

                    foreach (var tp in _errorInfoList)
                    {
                        Tuple<string, string> result = ExtractNumericAndChinese(tp.Item1);
                        shpWriter.addErrorGeometry(tp.Item2,
                            new Dictionary<string, string>
                                { { "图层名", result.Item2 }, { "编号", result.Item1 }, { "检查项", "跨图层端点离线检查" } });
                    }

                    shpWriter.saveErrorResutSHPFile();

                    rm.info = new[] { sf };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                rm.stat = ResultState.Failed;
                rm.msg = ex.Message;
            }

            return rm;
        }


        public void Dispose()
        {
            //nID_fID_dic.Clear();
            //groups.Clear();
            //_sbResult.Clear();
            //_eptlist.Clear();
            //_eFidsDic.Clear();
            _spatialReference = null;
        }

        /// <summary>
        /// 分析问题函数
        /// </summary>
        ResultMessage Analysis(IFeatureClass fcls, IEnvelope env, double disValue, WaitOperation wo = null)
        {
            if (wo != null)
                wo.SetText("正在进行分析准备……");

            string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
            IWorkspace _ws = DCDHelper.createTempWorkspace(fullPath);
            IFeatureWorkspace fws = _ws as IFeatureWorkspace;

            //IFeatureClass _tempFC = DCDHelper.CreateFeatureClassStructToWorkspace(fws, fcls, fcls.AliasName + "_temp");

            string outfullPath = String.Join("\\", fullPath, fcls.AliasName + "_temp");

            ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = new Geoprocessor();
            gp.OverwriteOutput = true;

            string fclsLyr = fcls.AliasName + "_sel";
            MakeFeatureLayer makeFeatureLayer = new MakeFeatureLayer();
            makeFeatureLayer.in_features = fcls;
            makeFeatureLayer.out_layer = fclsLyr;
            Helper.ExecuteGPTool(gp, makeFeatureLayer, null);

            // if (fcls.HasCollabField())
            {
                SelectLayerByAttribute selectLayerByAttribute = new SelectLayerByAttribute();
                selectLayerByAttribute.in_layer_or_view = fclsLyr;
                selectLayerByAttribute.where_clause = cmdUpdateRecord.CurFeatureFilter;
                Helper.ExecuteGPTool(gp, selectLayerByAttribute, null);
            }

            //输入图层已考虑删除的线
            //CurFeatureFilter
            FeatureVerticesToPoints fvtp = new FeatureVerticesToPoints();
            fvtp.in_features = fclsLyr; //使用当前要素（排除删除）
            fvtp.point_location = "DANGLE";
            fvtp.out_feature_class = outfullPath;

            gp.Execute(fvtp, null);

            //List<int> feOIDList = new List<int>();

            IFeatureClass fcDanglePt = DCDHelper.GetFclViaWs(_ws, fcls.AliasName + "_temp");
            int origFIDindex = fcDanglePt.FindField("ORIG_FID");

            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "";
            IFeatureCursor fcDanglePtCursor = fcDanglePt.Update(qf, true); //edit
            IFeature feaDanglePt = null;
            while ((feaDanglePt = fcDanglePtCursor.NextFeature()) != null)
            {
                int danglePtOID = feaDanglePt.OID;
                int origLineFID = (int)(feaDanglePt.get_Value(origFIDindex));

                ITopologicalOperator topoOper = feaDanglePt.Shape as ITopologicalOperator;
                IGeometry geo = topoOper.Buffer(disValue); //

                ISpatialFilter filter = new SpatialFilterClass();
                filter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                filter.Geometry = geo;
                IFeatureCursor fcLineCursor = fcls.Search(filter, false);
                IFeature fcLine = null;
                double selfDist = 0.0;
                int nearLineCount = 0;
                while ((fcLine = fcLineCursor.NextFeature()) != null)
                {
                    int lineFid = fcLine.OID;
                    if (lineFid == origLineFID)
                    {
                        IPolyline pl = fcLine.Shape as IPolyline;
                        selfDist = pl.Length;
                    }

                    nearLineCount += 1;
                }

                if (nearLineCount == 1) //micro Line bug
                {
                    fcDanglePtCursor.DeleteFeature();
                }
                else if (nearLineCount > 1) //Y-T type bug
                {
                    if (selfDist < disValue)
                    {
                        fcDanglePtCursor.DeleteFeature();
                    }
                }

                Marshal.ReleaseComObject(fcLineCursor);
            }

            Marshal.ReleaseComObject(fcDanglePtCursor);

            GenerateNearTable gnt = new GenerateNearTable();
            gnt.in_features = outfullPath;
            gnt.near_features = outfullPath;
            gnt.out_table = String.Join("\\", fullPath, fcls.AliasName + "_GNTable");
            gnt.search_radius = disValue;
            gnt.closest = "ALL";
            gnt.closest_count = 5;

            gp.Execute(gnt, null);

            //悬挂点分组
            Dictionary<int, int> ptID_groupID = new Dictionary<int, int>();

            ITable tbl = fws.OpenTable(fcls.AliasName + "_GNTable");
            int inFID = tbl.FindField("IN_FID");
            int nearFID = tbl.FindField("NEAR_FID");
            ICursor cursor = tbl.Search(null, true);
            IRow row = null;
            int maxGroupID = 0;
            while ((row = cursor.NextRow()) != null)
            {
                int id1 = (int)(row.get_Value(inFID));
                int id2 = (int)(row.get_Value(nearFID));
                if (!ptID_groupID.ContainsKey(id1) && !ptID_groupID.ContainsKey(id2))
                {
                    maxGroupID += 1;
                    ptID_groupID.Add(id1, maxGroupID);
                    ptID_groupID.Add(id2, maxGroupID);
                }

                else if (ptID_groupID.ContainsKey(id1) && !ptID_groupID.ContainsKey(id2))
                {
                    ptID_groupID.Add(id2, ptID_groupID[id1]);
                }
                else if (!ptID_groupID.ContainsKey(id1) && ptID_groupID.ContainsKey(id2))
                {
                    ptID_groupID.Add(id1, ptID_groupID[id2]);
                }
                else
                {
                    if (ptID_groupID[id1] != ptID_groupID[id2])
                    {
                        int groupMinID = ptID_groupID[id1] < ptID_groupID[id2] ? ptID_groupID[id1] : ptID_groupID[id2];
                        int groupMaxID = ptID_groupID[id1] > ptID_groupID[id2] ? ptID_groupID[id1] : ptID_groupID[id2];

                        // 创建一个临时字典来保存需要修改的键值对
                        Dictionary<int, int> tempDict = new Dictionary<int, int>();

                        foreach (int ptID in ptID_groupID.Keys)
                        {
                            if (ptID_groupID[ptID] == groupMaxID)
                            {
                                // 将需要修改的键值对添加到临时字典中
                                tempDict[ptID] = groupMinID;
                            }
                        }

                        // 使用临时字典来修改原始字典
                        foreach (var kvp in tempDict)
                        {
                            ptID_groupID[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
            //Marshal.ReleaseComObject(row);  

            Dictionary<int, List<int>> groupID_ptsD = new Dictionary<int, List<int>>();
            foreach (KeyValuePair<int, int> kv in ptID_groupID)
            {
                int ptID = kv.Key;
                int groupID = kv.Value;
                if (groupID_ptsD.ContainsKey(groupID))
                {
                    groupID_ptsD[groupID].Add(ptID);
                }
                else
                {
                    groupID_ptsD.Add(groupID, new List<int> { ptID });
                }
            }

            IDictionary<int, Tuple<int, IPoint>> ptIdGeoD = new Dictionary<int, Tuple<int, IPoint>>();
            IFeatureCursor fcDanglePtCursor2 = fcDanglePt.Search(qf, true); //edit
            IFeature feaDanglePt2 = null;

            while ((feaDanglePt2 = fcDanglePtCursor2.NextFeature()) != null)
            {
                int danglePtOID = feaDanglePt2.OID;
                int origLineFID = (int)(feaDanglePt2.get_Value(origFIDindex));
                IPoint pt = feaDanglePt2.ShapeCopy as IPoint;
                if (ptID_groupID.ContainsKey(danglePtOID))
                {
                    Tuple<int, IPoint> tp = new Tuple<int, IPoint>(origLineFID, pt);
                    ptIdGeoD.Add(danglePtOID, tp);
                }

                else
                {
                    MultipointClass mp = new MultipointClass();
                    object miss = Type.Missing;
                    mp.AddGeometry(pt, miss, miss);
                    string txt = fcls.AliasName + origLineFID.ToString();
                    _errorInfoList.Add(new Tuple<string, IGeometry>(txt, mp));
                }

            }
            /*
            foreach (var ptsD in groupID_ptsD.Values)
            {
                MultipointClass mp = new MultipointClass();
                string txt = "";
                foreach (int ptID in ptsD)
                {
                    Tuple<int, IPoint> tp = ptIdGeoD[ptID];
                    object miss = Type.Missing;
                    mp.AddGeometry(tp.Item2,  miss,  miss);
                    txt += string.Format(";{0}", tp.Item1);
                }
                _errorInfoList.Add(new Tuple<string, IGeometry>(txt, mp));
            }*/

            Marshal.ReleaseComObject(fcDanglePtCursor2);

            var rm = new ResultMessage();
            rm.stat = ResultState.Ok;
            return rm;
        }

        static Tuple<string, string> ExtractNumericAndChinese(string input)
        {
            // 使用正则表达式匹配数字部分
            Match numericMatch = Regex.Match(input, @"\d+");

            if (numericMatch.Success)
            {
                string numericPart = numericMatch.Value; // 数字部分

                // 从原始字符串中删除数字部分，剩下的即为中文部分
                string chinesePart = input.Replace(numericPart, "");

                return new Tuple<string, string>(numericPart, chinesePart);
            }
            else
            {
                return new Tuple<string, string>("", input); // 如果没有数字部分，返回空字符串作为数字部分
            }
        }
    }
}
