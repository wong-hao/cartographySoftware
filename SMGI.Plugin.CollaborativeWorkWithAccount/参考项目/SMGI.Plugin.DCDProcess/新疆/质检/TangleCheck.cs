using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;


namespace SMGI.Plugin.DCDProcess.DataProcess
{
    /// <summary>
    /// 悬挂点检查
    /// </summary>
    public class TangleCheck
    {
        //距离阈值
        double disValue;

        //算法相关
        TinClass tin;
        Dictionary<int, List<int>> nID_fID_dic;
        List<Dictionary<int, IPoint>> groups;
        GeometryBagClass gb;

        ISpatialReference _spatialReference;
        string _lyrName = "";
        readonly StringBuilder _sbResult = new StringBuilder();
        readonly Dictionary<int,IGeometry> _eptlist = new Dictionary<int, IGeometry>();
        readonly Dictionary<int ,string> _eFidsDic=new Dictionary<int, string>();

        /// <summary>
        /// 检查结果条数
        /// </summary>
        public int Count { get; private set; }

        public TangleCheck(double t_dis)
        {
            disValue = t_dis;
            //参数初始化
        }

        /// <summary>
        /// 执行检查
        /// </summary>
        /// <param name="fcls"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public ResultMessage Check(IFeatureClass fcls, IEnvelope env, WaitOperation wo = null)
        {
            var geod = fcls as IGeoDataset;
            _spatialReference = geod.SpatialReference;
            _lyrName = fcls.AliasName;
            _eptlist.Clear();
            _eFidsDic.Clear();

            ResultMessage rm = Analysis(fcls, env, wo);

            for (int i = gb.GeometryCount - 1; i >= 0; i--)
            {
                ReportResult(i);
            }

            return rm;
        }

        /// <summary>
        /// 获取质检报告文本
        /// </summary>
        /// <returns></returns>
        public string GetReport()
        {
            return _sbResult.ToString();

        }

        /// <summary>
        /// 保存质检结果 +2
        /// </summary>
        /// <param name="dir">输出目录</param>
        /// <param name="bAutoOver"></param>
        /// <returns></returns>
        public ResultMessage SaveResult(string dir, bool bAutoOver = false, bool toGdb=false)
        {
            //保存shp文件
            ResultMessage rm = new ResultMessage() { stat = ResultState.Ok };
            try
            {
                if (_eptlist.Count > 0)
                {
                    var shpWriter = new ShapeFileWriter();

                    var shpdir = dir;
                    if (!Directory.Exists(shpdir))
                        Directory.CreateDirectory(shpdir);
                    var sf = shpdir + "\\悬挂点检查_" + _lyrName;
                    shpWriter.createErrorResutSHPFile(sf, _spatialReference,
                        esriGeometryType.esriGeometryMultipoint, new Dictionary<string, int> { { "图层名", 20 }, { "编号", 60 }, { "检查项", 8 } }, bAutoOver);

                    foreach (var item in _eptlist)
                    {
                        shpWriter.addErrorGeometry(item.Value, new Dictionary<string, string> { { "图层名", _lyrName }, { "编号", _eFidsDic[item.Key] }, { "检查项", "悬挂点" } });
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
        private void SaveToGDB()
        {
            var fcls = ResultGDBWriter.GetMutiPointFC(_spatialReference);
            var cursor = fcls.Insert(true);
            var n = 0;
            for (var t = 0; t < _eptlist.Count; t++)
            {
                ResultGDBWriter.addErrorGeometry(fcls, cursor, _eptlist[t],
                    new Dictionary<string, string> { { "检查内容", "悬挂点检查" }, { "图层", _lyrName }, { "错误描述", "悬挂点"} });
                n++;
                if (n > 5000)
                {
                    cursor.Flush();
                    n = 0;
                }

            }
            cursor.Flush();
            Marshal.ReleaseComObject(cursor);
            Marshal.ReleaseComObject(fcls);
        }
		
		
        public void Dispose()
        {
            nID_fID_dic.Clear();
            groups.Clear();
            _sbResult.Clear();
            _eptlist.Clear();
            _eFidsDic.Clear();
            _spatialReference = null;
        }
        /// <summary>
        /// 分析问题函数
        /// </summary>
        ResultMessage Analysis(IFeatureClass fcls, IEnvelope env, WaitOperation wo = null)
        {
            if(wo != null)
                wo.SetText("正在进行分析准备……");
            tin = new TinClass();
            tin.InitNew(env);
            tin.StartInMemoryEditing();

            var versionFilter = "";
            if (fcls.HasCollabField())
                versionFilter = cmdUpdateRecord.CurFeatureFilter;

            IFeatureCursor fCursor = fcls.Search(new QueryFilterClass { WhereClause = versionFilter }, true);
            IFeature feature = null;
            IPoint p = new PointClass();
            p.Z = 0;
            ITinNode node = new TinNodeClass();
            nID_fID_dic = new Dictionary<int, List<int>>();
            while ((feature = fCursor.NextFeature()) != null)
            {
                IPolyline line = feature.Shape as IPolyline;
                if (line == null)
                    continue;

                if (line.Length > 0)
                {
                    p.X = line.FromPoint.X;
                    p.Y = line.FromPoint.Y;
                    tin.AddPointZ(p, 1, node);
                    if (!nID_fID_dic.ContainsKey(node.Index))
                    {
                        nID_fID_dic[node.Index] = new List<int>();
                    }
                    nID_fID_dic[node.Index].Add(feature.OID);
                    p.X = line.ToPoint.X;
                    p.Y = line.ToPoint.Y;
                    tin.AddPointZ(p, 1, node);
                    if (!nID_fID_dic.ContainsKey(node.Index))
                    {
                        nID_fID_dic[node.Index] = new List<int>();
                    }
                    nID_fID_dic[node.Index].Add(-feature.OID);
                }

            }

            groups = new List<Dictionary<int, IPoint>>();
            for (int i = 1; i <= tin.NodeCount; i++)
            {
                if (wo != null)
                    wo.SetText("正在分析:" + i.ToString());

                ITinNode n = tin.GetNode(i);
                if (n.TagValue != -1 && n.IsInsideDataArea)
                {
                    Dictionary<int, IPoint> g = new Dictionary<int, IPoint>();
                    FindNode(n, g, disValue);
                    if (g.Count <= 1)
                        continue;

                    //排除微短线
                    List<Dictionary<int, IPoint>> gList = this.EliminateSmallLine(g, fcls);

                    //添加到问题组中
                    foreach (var item in gList)
                    {
                        if (item.Count > 1)
                        {
                            groups.Add(item);
                        }
                    }
                }
            }
            gb = new GeometryBagClass();
            object miss = Type.Missing;

            if (wo != null)
                wo.SetText("正在整理分析结果");

            foreach (Dictionary<int, IPoint> g in groups)
            {

                MultipointClass mp = new MultipointClass();
               
                foreach (int nid in g.Keys)
                {
                    IPoint pend = g[nid];
                    mp.AddGeometry(pend, ref miss, ref miss);

                  
                }
                gb.AddGeometry(mp, ref miss, ref miss);
               
            }
            Count = groups.Count;

            var rm = new ResultMessage();
            rm.stat = ResultState.Ok;
            return rm;
        }


        /// <summary>
        /// 递归查找三角网指定节点周边存在的所有问题，作为一个问题集返回
        /// </summary>
        /// <param name="seed">三角网节点</param>
        /// <param name="nodes">存储问题点集</param>
        /// <param name="distance">距离阈值</param>
        void FindNode(ITinNode seed, Dictionary<int, IPoint> nodes, double distance)
        {
            ITinEdgeArray edges = seed.GetIncidentEdges();
            ITinEdit tin = seed.TheTin as ITinEdit;
            tin.SetNodeTagValue(seed.Index, -1);
            PointClass p = new PointClass();
            p.X = seed.X;
            p.Y = seed.Y;
            nodes.Add(seed.Index, p);
            for (int i = 0; i < edges.Count; i++)
            {
                ITinEdge edge = edges.get_Element(i);
                if (edge.Length < distance)
                {
                    if (edge.ToNode.IsInsideDataArea && edge.ToNode.TagValue != -1)
                    {
                        FindNode(edge.ToNode, nodes, distance);
                    }
                }
            }
        }
        void ReportResult(int index)
        {
            _sbResult.Append("\t悬挂点" + (index + 1) + ":");

            IGeometry geo = gb.get_Geometry(index);
            IEnvelope env = geo.Envelope;
            IPoint p = (env as IArea).Centroid;

            _eptlist.Add(index,geo);
            _sbResult.Append(p.X + "," + p.Y + "\r\n");
            var str = "";
            Dictionary<int, IPoint> group = groups[index];
            foreach (var item in group.Keys)
            {
                List<int> fids = nID_fID_dic[item];
                foreach (var fid in fids)
                {
                    str += Math.Abs(fid) + ",";
                }
                str = str.Substring(0, str.Length - 1);
                str += ";";
            }
            _eFidsDic.Add(index,str);
            groups.RemoveAt(index);
            gb.RemoveGeometries(index, 1);
        }


        /// <summary>
        /// 排除微短线的干扰
        /// </summary>
        /// <param name="g"></param>
        /// <param name="fc"></param>
        /// <returns></returns>
        List<Dictionary<int, IPoint>> EliminateSmallLine(Dictionary<int, IPoint> g, IFeatureClass fc)
        {
            List<Dictionary<int, IPoint>> result = new List<Dictionary<int, IPoint>>();

            List<int> elliKey = new List<int>();//要素起点、终点重合的节点索引(且该节点就只包含起点和终点)

            Dictionary<int, int> oid2GPIndex = new Dictionary<int, int>();//<FeatureID,GroupKeys>
            foreach (var item in g)
            {
                List<int> fids = nID_fID_dic[item.Key];
                foreach (var fid in fids)
                {
                    int oid = Math.Abs(fid);

                    #region 一个问题组中存在同一个微短线的两个端点
                    if (oid2GPIndex.ContainsKey(oid))//一个要素的端点最多只可能出现在两个节点中(微短线)
                    {
                        if (fids.Contains(-fid))
                        {
                            if (fids.Count == 2)//节点仅包含起点和终点，则不参与
                                elliKey.Add(item.Key);

                            continue;//排除一个要素的两个端点同时在一个节点的情况,该种情况不进行分组（否则进入死循环）
                        }

                        IPoint p1 = g[oid2GPIndex[oid]];
                        IPoint p2 = item.Value;

                        //微短线的两个端点分开为两个子问题组
                        Dictionary<int, IPoint> g1 = new Dictionary<int, IPoint>();
                        g1.Add(oid2GPIndex[oid], p1);
                        Dictionary<int, IPoint> g2 = new Dictionary<int, IPoint>();
                        g2.Add(item.Key, p2);

                        //计算该要素两个端点到其它端点的距离，将问题组中的其它节点分配到与之最近的一个端点子问题组中
                        foreach (var kv in g)
                        {
                            if (kv.Key == item.Key || kv.Key == oid2GPIndex[oid])//跳过短点所在节点
                                continue;

                            IProximityOperator ProxiOP = kv.Value as IProximityOperator;

                            //计算p到微短线端点p1,p2的距离
                            double len1 = ProxiOP.ReturnDistance(p1);
                            double len2 = ProxiOP.ReturnDistance(p2);
                            if (len1 < len2)
                            {
                                g1.Add(kv.Key, kv.Value);
                            }
                            else
                            {
                                g2.Add(kv.Key, kv.Value);
                            }

                        }

                        //递归
                        result.AddRange(EliminateSmallLine(g1, fc));
                        result.AddRange(EliminateSmallLine(g2, fc));

                        return result;
                    }
                    #endregion

                    oid2GPIndex.Add(oid, item.Key);
                }

            }

            //将要素起点和终点重合的节点从问题组中去掉
            if (elliKey.Count > 0)
            {
                foreach (var item in elliKey)
                {
                    g.Remove(item);
                }
            }

            result.Add(g);

            return result;
        }

    }
}
