using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 对LRDL中的城际道路和乡村道路进行处理，将要素的LGB码赋值为该要素的GB码
    /// </summary>
    public class StRoadLGBCmd : SMGI.Common.SMGICommand
    {
        public StRoadLGBCmd()
        {
            m_category = "街区增补LGB赋值";
            m_caption = "街区增补LGB赋值";
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
        Dictionary<string, List<RoadSegment>> roadSegDict = new Dictionary<string, List<RoadSegment>>();
        public override void OnClick()
        {

            string layerName = "LRDL";

            var lyr = m_Application.Workspace.LayerManager.GetLayer(l => l is IGeoFeatureLayer && ((l as IGeoFeatureLayer).FeatureClass as IDataset).Name.ToUpper() == layerName).FirstOrDefault();

            IFeatureLayer layer = lyr as IFeatureLayer;
            if (layer == null)
            {
                MessageBox.Show(string.Format("当前数据库缺少道路图层[{0}]!", layerName), "警告", MessageBoxButtons.OK);
                return;
            }

            IFeatureClass fc = layer.FeatureClass;

            int gbIndex = fc.Fields.FindField("GB");
            int lgbIndex = fc.Fields.FindField("LGB");
            int rnIdx = fc.Fields.FindField("RN");
            if (gbIndex == -1 || lgbIndex == -1 || rnIdx == -1)
                return;

            try
            {
                m_Application.EngineEditor.StartOperation();
                using (var wo = m_Application.SetBusy())
                {
                    //第一步 把有RN的街区道路 赋LGB值
                    RnToLGB(fc, rnIdx, gbIndex, lgbIndex);

                    //第二步 把剩下
                    ////检查带RN的道路连通性
                    //CheckRnConnection(fc, rnIdx);
                    ////处理不连通的

                    //List<string> rnList=new List<string>();

                    // foreach (var segs in roadSegDict)
                    // {
                    //     rnList.Add(segs.Key);
                    // }
                    // int xx = 0;
                }
                m_Application.EngineEditor.StopOperation("街区增补LGB赋值");
                MessageBox.Show("处理完成！");
            }
            catch (Exception err)
            {
                m_Application.EngineEditor.AbortOperation();
                
                System.Diagnostics.Trace.WriteLine(err.Message);
                System.Diagnostics.Trace.WriteLine(err.Source);
                System.Diagnostics.Trace.WriteLine(err.StackTrace);

                MessageBox.Show(String.Format("处理失败:{0}",err.Message));
            }
        }
        

        Dictionary<string,List<IFeature>> tmpFeaDic=new Dictionary<string, List<IFeature>>(); 
        Dictionary<string,int>rnLGBDic=new Dictionary<string, int>(); 
        private void RnToLGB(IFeatureClass fc, int rnIdx,int gbidx,int lgbidx)
        {
            
            IQueryFilter querFilter = new QueryFilterClass();
            querFilter.WhereClause = "rn<>'' and rn is not null";
            var pCursor = fc.Search(querFilter, false);
            IFeature pFeature = null;;
            while (null != (pFeature = pCursor.NextFeature()))
            {
                var rn = pFeature.Value[rnIdx].ToString();
                var gb = (int) pFeature.Value[gbidx];

                if (!rnLGBDic.ContainsKey(rn))
                {
                    if (gb <= 430503 && gb >= 430200)
                    {
                        if (tmpFeaDic.ContainsKey(rn))
                        {
                            tmpFeaDic[rn].Add(pFeature);
                        }
                        else
                        {
                            var ls = new List<IFeature>();
                            ls.Add(pFeature);
                            tmpFeaDic.Add(rn, ls);
                        }
                    }
                    else
                    {
                        rnLGBDic.Add(rn, gb);
                    }
                }
                else
                {
                    if (gb <= 430503 && gb >= 430200)
                    {
                       if( string.IsNullOrEmpty(pFeature.Value[lgbidx].ToString()))//避免覆盖 人工修改的
                        {
                            int lgb = rnLGBDic[rn];
                        pFeature.Value[lgbidx] = lgb;
                        pFeature.Store();
                        }                        
                    }
                }
               
            }
           
            foreach (var kv in tmpFeaDic)
            {
                var v = kv.Value;
                if(!rnLGBDic.ContainsKey(kv.Key))
                    continue;

                var lgb = rnLGBDic[kv.Key];

                foreach (var feature in v)
                {
                    if (string.IsNullOrEmpty(feature.Value[lgbidx].ToString()))//避免覆盖 人工修改的
                    {
                        feature.Value[lgbidx] = lgb;
                        feature.Store();
                    }
                }
            }

            rnLGBDic.Clear();
            tmpFeaDic.Clear();
            Marshal.ReleaseComObject(pCursor);
        }

      
        private void CheckRnConnection2(IFeatureClass fc, int rnIdx)
        {
            roadSegDict.Clear();
            IQueryFilter querFilter = new QueryFilterClass();
            querFilter.WhereClause = "GB<=430200";
            IFeatureCursor feaCursor = fc.Search(querFilter, false);
            IFeature fea = null;
            while ((fea = feaCursor.NextFeature()) != null)
            {
                string rn = fea.Value[rnIdx].ToString();
                if(rn=="")continue;
                if (roadSegDict.ContainsKey(rn))
                {
                    var line = fea.Shape as IPolyline;
                    //先判断是否合并链,先合并
                    var b = CombineChain(roadSegDict[rn], fea);
                    if (!b)
                    {//新增路段
                        var seg = new RoadSegment();
                        seg.End = line.ToPoint;
                        seg.Start = line.FromPoint;
                        seg.StartId = seg.EndId = fea.OID;
                        roadSegDict[rn].Add(seg);
                    }
                }
                else
                {//新增RN链
                    List<RoadSegment> rsegs = new List<RoadSegment>();
                    var line = fea.Shape as IPolyline;
                    var seg = new RoadSegment();
                    seg.End = line.ToPoint;
                    seg.Start = line.FromPoint;
                    seg.StartId = seg.EndId = fea.OID;
                    rsegs.Add(seg);
                    roadSegDict.Add(rn, rsegs);
                }
            }
            Marshal.ReleaseComObject(feaCursor);

            var roadSegDict2 = new Dictionary<string, List<RoadSegment>>();

            //剔除连通的
            foreach (var segs in roadSegDict)
            {
                 if(segs.Value.Count>1)
                     roadSegDict2.Add(segs.Key,segs.Value);
            }
            roadSegDict.Clear();
            roadSegDict = roadSegDict2;
        }

        bool CombineChain(List<RoadSegment> rchain, IFeature fea)
        {
            var road = fea.Shape as IPolyline;
            bool b = false;
            int idx = 0;
            for (; idx < rchain.Count; idx++)//判断连接
            {
                var seg = rchain[idx];
                if (JudgeNeighbour(seg.Start, road.FromPoint))
                {
                    seg.Start = road.ToPoint;
                    seg.StartId = fea.OID;
                    b = true;
                    break;
                }
                if (JudgeNeighbour(seg.Start, road.ToPoint))
                {
                    seg.Start = road.FromPoint;
                    seg.StartId = fea.OID;
                    b = true;
                    break;
                }
                if (JudgeNeighbour(seg.End, road.FromPoint))
                {
                    seg.End = road.ToPoint;
                    seg.EndId = fea.OID;
                    b = true;
                    break;
                }
                if (JudgeNeighbour(seg.End, road.ToPoint))
                {
                    seg.End = road.FromPoint;
                    seg.EndId = fea.OID;
                    b = true;
                    break;
                }
            }
            if (b)//内部判断合并
            {
                var sg = rchain[idx];
                for (var i = 0; i < rchain.Count; i++)
                {
                    if (i == idx) continue;
                    var r = rchain[i];
                    if (JudgeNeighbour(r.Start, sg.Start))
                    {
                        r.Start = sg.End;
                        r.StartId = sg.EndId;

                        rchain.RemoveAt(idx);
                        break;
                    }
                    if (JudgeNeighbour(r.Start, sg.End))
                    {
                        r.Start = sg.Start;
                        r.StartId = sg.StartId;
                        rchain.RemoveAt(idx);

                        break;
                    }
                    if (JudgeNeighbour(r.End, sg.Start))
                    {
                        r.End = sg.End;
                        r.EndId = sg.EndId;
                        rchain.RemoveAt(idx);

                        break;
                    }
                    if (JudgeNeighbour(r.End, sg.End))
                    {
                        r.End = sg.Start;
                        r.EndId = sg.StartId;
                        rchain.RemoveAt(idx);
                        break;
                    }
                }
                return true;
            }
            return false;
        }

        //考虑到坐标单位为米，实际相差1米内就算相连吧？！
        bool JudgeNeighbour(IPoint p1, IPoint p2)
        {
            if (Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y) < 1)
            {
                return true;
            }
            return false;
        }
        internal class RoadSegment
        {
            public int StartId;
            public int EndId;
            public IPoint Start;
            public IPoint End;

        }
    }
}
