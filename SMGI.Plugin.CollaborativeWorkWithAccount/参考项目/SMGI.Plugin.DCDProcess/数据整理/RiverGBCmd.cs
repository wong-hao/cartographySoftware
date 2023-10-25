

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using System.Diagnostics;
using ESRI.ArcGIS.esriSystem;
namespace SMGI.Plugin.DCDProcess.DataProcess
{
    public class RiverGBCmd : SMGI.Common.SMGICommand
    {
        public RiverGBCmd()
        {
            m_category = "水系GB统改";
            m_caption = "水系GB统改";
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
            string layerName = "HYDL";
            Stopwatch swch = new Stopwatch();
            swch.Start();
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName == layerName;
            })).ToArray();

            ILayer HYDA = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == ("HYDA"))).FirstOrDefault();
            IFeatureClass hyda = (HYDA as IFeatureLayer).FeatureClass;
            int hydagbIndex = hyda.Fields.FindField("GB");
            IFeatureLayer layer = lyrs[0] as IFeatureLayer;
            if (layer == null)
            {
                MessageBox.Show(string.Format("当前数据库缺少水系图层[{0}]!", layerName), "警告", MessageBoxButtons.OK);
                return;
            }

            IFeatureClass fc = layer.FeatureClass;

            int gbIndex = fc.Fields.FindField("GB");
            int hgbIndex = fc.Fields.FindField("HGB");
            int nameIndex = fc.Fields.FindField("NAME");
            if (gbIndex == -1 || hgbIndex == -1 || nameIndex == -1)
                return;

            m_Application.EngineEditor.StartOperation();

            int num = 0;
            int num2 = 0;
            using (var wo = m_Application.SetBusy())
            {
                IQueryFilter querFilter = new QueryFilterClass();
                IFeatureCursor feaCursor;
                IFeature fea = null;
                ITopologicalOperator pTOPO;

                #region step1
                //给非结构线HGB赋值
                querFilter.WhereClause = "GB<>210400 and  (HGB = 0 or HGB is null)";
                feaCursor = fc.Search(querFilter, false);

                while ((fea = feaCursor.NextFeature()) != null)
                {
                    if (wo != null)
                        wo.SetText("Step1:正在处理要素【" + fea.OID + "】.......");
                    fea.set_Value(hgbIndex, fea.get_Value(gbIndex));
                    fea.Store();
                    Marshal.ReleaseComObject(fea);
                    num2++;
                }
                feaCursor.Flush();
                Marshal.ReleaseComObject(feaCursor);
                #endregion step1
                //goto START;
                #region step2
                //结构线赋值
                querFilter.WhereClause = "GB=210400 and  (HGB = 0 or HGB is null)";//约1/10数据量 14000+条

                feaCursor = fc.Search(querFilter, false);
                while ((fea = feaCursor.NextFeature()) != null)
                {
                    if (wo != null)
                        wo.SetText("Step2:正在处理要素【" + fea.OID + "】.......");

                    IPolyline CheckLine = fea.Shape as IPolyline;
                    if (CheckLine.Length > 0)
                    {
                        pTOPO = CheckLine as ITopologicalOperator;
                        pTOPO.Simplify();
                        IGeometry fromBufferGeometry = pTOPO.Buffer(0.1);
                        ISpatialFilter fromSpatialFilter = new SpatialFilterClass();
                        fromSpatialFilter.Geometry = fromBufferGeometry;
                        fromSpatialFilter.GeometryField = fc.ShapeFieldName;
                        fromSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        // int fromFeatureCount = fc.FeatureCount(fromSpatialFilter);
                        IFeatureCursor fromSelectCursor = hyda.Search(fromSpatialFilter, false);
                        IFeature fromSelectFeature = null;
                        while ((fromSelectFeature = fromSelectCursor.NextFeature()) != null)
                        {
                            string fromGB = fromSelectFeature.get_Value(hydagbIndex).ToString();
                            Marshal.ReleaseComObject(fromSelectFeature);

                            if (fromGB.StartsWith("21") || fromGB.StartsWith("22"))//非静止水面的水系结构线的HGB依水系面：河流面、沟渠面
                            {
                                if (fromGB != "210400")//正常情况下是不会存在水系面的gb为210400，这里加上该条件是为了防止数据的gb可能赋值不正确
                                {
                                    fea.set_Value(hgbIndex, fromGB);
                                    fea.Store();
                                    num++;
                                    break;
                                }
                            }
                            else if (fromGB.StartsWith("23") || fromGB.StartsWith("24"))//静止水面的水系结构线依上下游水系：湖泊面、水库面
                            {
                                //  起点
                                pTOPO = CheckLine.FromPoint as ITopologicalOperator;
                                pTOPO.Simplify();
                                IGeometry bufferGeometry1 = pTOPO.Buffer(0.1);
                                ISpatialFilter spatialFilter1 = new SpatialFilterClass();
                                spatialFilter1.Geometry = bufferGeometry1;
                                spatialFilter1.GeometryField = fc.ShapeFieldName;
                                spatialFilter1.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                string validGB = "";
                                IFeature selectFeaturehydl = null;
                                IFeatureCursor selectCursorhydl = fc.Search(spatialFilter1, false);
                                while ((selectFeaturehydl = selectCursorhydl.NextFeature()) != null)
                                {
                                    var cd = selectFeaturehydl.get_Value(gbIndex).ToString();
                                    if (cd != "" && cd != "210400")
                                    {
                                        validGB = cd;
                                        break;
                                    }
                                    Marshal.ReleaseComObject(selectFeaturehydl);
                                }
                                Marshal.ReleaseComObject(selectCursorhydl);

                                if (validGB == "")
                                { //下一个节点
                                    pTOPO = CheckLine.ToPoint as ITopologicalOperator;
                                    pTOPO.Simplify();
                                    bufferGeometry1 = pTOPO.Buffer(0.1);
                                    spatialFilter1 = new SpatialFilterClass();
                                    spatialFilter1.Geometry = bufferGeometry1;
                                    spatialFilter1.GeometryField = fc.ShapeFieldName;
                                    spatialFilter1.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                    selectCursorhydl = fc.Search(spatialFilter1, false);
                                    while ((selectFeaturehydl = selectCursorhydl.NextFeature()) != null)
                                    {
                                        var cd = selectFeaturehydl.get_Value(gbIndex).ToString();
                                        if (cd != "" && cd != "210400")
                                        {
                                            validGB = cd;
                                            break;
                                        }
                                        Marshal.ReleaseComObject(selectFeaturehydl);
                                    }
                                    Marshal.ReleaseComObject(selectCursorhydl);

                                }

                                if (validGB != "")
                                {
                                    fea.set_Value(hgbIndex, validGB);
                                    fea.Store();
                                    num++;
                                }
                            }
                        }
                        Marshal.ReleaseComObject(fromSelectCursor);
                    }
                    Marshal.ReleaseComObject(fea);
                }
                feaCursor.Flush();
                Marshal.ReleaseComObject(feaCursor);


                #endregion step2

                #region step3
                // 检查湖泊水库中间骨架结构线
                querFilter.WhereClause = "HGB is null or HGB = 0";
                //  querFilter.WhereClause = " OBJECTID=83822 ";
                feaCursor = fc.Search(querFilter, false);
                fea = null;

                ISpatialFilter spatialFilter = new SpatialFilterClass();
                spatialFilter.GeometryField = fc.ShapeFieldName;
                spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                while ((fea = feaCursor.NextFeature()) != null)//约7k
                {
                    //Log("start:" + fea.OID + "--------------------------------------");
                    if (wo != null)
                        wo.SetText("Step3:正在处理要素【" + fea.OID + "】.......");

                    IPolyline CheckLine = fea.Shape as IPolyline;
                    if (CheckLine.Length > 0)
                    {
                        tmpOIDhash.Clear();
                        RiverChain rchain = new RiverChain();
                        rchain.Fealist = new List<IFeature>();
                        rchain.Fealist.Add(fea);
                        // rchain.Pts = new IPoint[] { CheckLine.FromPoint, CheckLine.ToPoint };
                        tmpOIDhash.Add(fea.OID);
                        rchain.Pts = new IPoint[] { CheckLine.FromPoint, CheckLine.ToPoint };
                        rchain.Cur = 1;
                        rchain.sid = 0;
                        listChain.Add(rchain);
                        FindChain(fc, fea, spatialFilter, ref hgbIndex, ref gbIndex, ref nameIndex, 0, true);
                        //处理当前Chain,清空
                        foreach (var ch in listChain)
                        {
                            if (!string.IsNullOrEmpty(ch.HGB))
                            {
                                var flist = ch.Fealist;
                                foreach (var f in flist)
                                {
                                    if (f.get_Value(gbIndex).ToString() == "210400")
                                    {

                                        f.set_Value(hgbIndex, ch.HGB);
                                        f.Store();
                                        num++;
                                    }
                                    Marshal.ReleaseComObject(f);

                                }
                            }
                            else
                            {
                                // unlistChain.Add(ch);
                                //string oids = "";
                                foreach (var f in ch.Fealist)
                                { //如果有河源
                                    // oids += "," + f.OID;
                                    //f.set_Value(hgbIndex, "210101");
                                    if (f.get_Value(gbIndex).ToString() == "210400")
                                    {
                                        f.set_Value(hgbIndex, "210101");
                                        f.Store();
                                        num++;
                                    }

                                    //
                                    Marshal.ReleaseComObject(f);

                                }
                                // tmpOIDdic.Add(oids, ch.sid);
                                //  Log("unChain  !!!!!!");
                            }
                        }
                        feaCursor.Flush();
                        listChain.Clear();


                    }
                    //  Log("***************************************finish:" + fea.OID);
                    //fea.set_Value(hgbIndex, fea.get_Value(gbIndex));
                    //fea.Store();
                    //Marshal.ReleaseComObject(fea);
                }
                feaCursor.Flush();


                #endregion step3
            }
            //START:
            m_Application.EngineEditor.StopOperation("水系GB赋值");
            swch.Stop();

            if (num > 0)
            {
                // MessageBox.Show(string.Format("{1}:更改了{0}条要素", num, swch.ElapsedMilliseconds));
                MessageBox.Show(string.Format("更改了{0}条结构线,{1}条河流", num, num2));
            }
        }
        //private void Log(string info)
        //{
        //     Console.WriteLine(info);
        //}
        List<RiverChain> listChain = new List<RiverChain>();
        //List<RiverChain> unlistChain = new List<RiverChain>();
        //Dictionary<string, int> tmpOIDdic = new Dictionary<string, int>();
        HashSet<int> tmpOIDhash = new HashSet<int>();

        private void FindChain(IFeatureClass fc, IFeature fea, ISpatialFilter spatialFilter, ref int hgbidx, ref int gbidx, ref int nameidx, int chainIdx, bool bOther = false, bool bnear = true)
        {
            var curChain = listChain[chainIdx];
            IPolyline CheckLine = fea.Shape as IPolyline;
            int cid = 0;//端点
            var curpt = (curChain.Pts[curChain.Cur] as IClone).Clone() as IPoint;
            LatestPt(curpt, CheckLine, ref cid);

            //端点1 靠近的点

            ITopologicalOperator pTOPO;
            if (bnear)
                pTOPO = (cid == 0 ? CheckLine.FromPoint : CheckLine.ToPoint) as ITopologicalOperator;
            else
                pTOPO = (cid == 0 ? CheckLine.ToPoint : CheckLine.FromPoint) as ITopologicalOperator;

            pTOPO.Simplify();
            IGeometry bufferGeometry = pTOPO.Buffer(0.1);
            spatialFilter.Geometry = bufferGeometry;
            var oid = fea.OID;
            IFeatureCursor selectCursor = fc.Search(spatialFilter, false);
            List<IFeature> fealist = new List<IFeature>();
            IFeature subfea = null;
            while ((subfea = selectCursor.NextFeature()) != null)
            {
                if (oid != subfea.OID)
                    fealist.Add(subfea);
            }
            //  Log("相交" + fealist.Count + "个");
            //存在分支
            if (fealist.Count > 1)
            {
                var feaName = fea.get_Value(nameidx).ToString();

                //新建链
                for (int k = 0; k < fealist.Count; k++)
                {

                    var name = fealist[k].get_Value(nameidx).ToString();
                    if (!string.IsNullOrEmpty(name) && feaName == name)//合并继续前进
                    {
                        if (!tmpOIDhash.Contains(fealist[k].OID))
                        {
                            if (curChain.Cur == 1)
                                curChain.Fealist.Add(fealist[k]);
                            else
                                curChain.Fealist.Insert(0, fealist[k]);
                            tmpOIDhash.Add(fealist[k].OID);
                            //远离的点
                            int iid = 0;
                            IPolyline ply = fealist[k].Shape as IPolyline;
                            LatestPt(curpt, ply, ref iid);
                            curChain.Pts[curChain.Cur] = (iid == 0 ? ply.ToPoint : ply.FromPoint);
                            //    Log(chainIdx + "链：合并前行 " + fealist[k].OID);
                            FindChain(fc, fealist[k], spatialFilter, ref hgbidx, ref gbidx, ref nameidx, chainIdx);
                        }
                    }
                    else
                    { //开链 新增链/结束链
                        if (!tmpOIDhash.Contains(fealist[k].OID))
                        {
                            RiverChain rchain = new RiverChain();
                            rchain.Fealist = new List<IFeature>();
                            rchain.Fealist.Add(fealist[k]);
                            int iid = 0;
                            IPolyline ply = fealist[k].Shape as IPolyline;
                            LatestPt(curpt, ply, ref iid);
                            rchain.Pts = new IPoint[] { (iid == 0 ? ply.FromPoint : ply.ToPoint), (iid == 0 ? ply.ToPoint : ply.FromPoint) };
                            //var pt = curChain.Pts[curChain.Cur] as IClone;
                            //rchain.Pts = new IPoint[] { pt.Clone() as IPoint, pt.Clone() as IPoint };
                            rchain.Cur = 1;//远的开始
                            var ich = listChain.Count;
                            rchain.sid = ich;
                            listChain.Add(rchain);
                            tmpOIDhash.Add(fealist[k].OID);
                            //  Log(ich + "链：新 " + fealist[k].OID);
                            FindChain(fc, fealist[k], spatialFilter, ref hgbidx, ref gbidx, ref nameidx, ich);
                        }
                    }
                }

            }
            else if (fealist.Count == 1)
            {
                var hgb = fealist[0].get_Value(hgbidx).ToString();
                if (!string.IsNullOrEmpty(hgb))
                { //遇到结束
                    //?
                    // curChain.FinishNum ++;
                    // Log("完成：" + fealist[0].OID);
                    curChain.HGB = hgb;
                    if (string.IsNullOrEmpty(curChain.Name))
                        curChain.Name = fealist[0].get_Value(nameidx).ToString();
                }
                else
                {//继续向前查找
                    // Log(chainIdx + "链：合并前行 " + fealist[0].OID);
                    if (curChain.Cur == 1)
                        curChain.Fealist.Add(fealist[0]);
                    else
                        curChain.Fealist.Insert(0, fealist[0]);
                    var poly = fealist[0].Shape as IPolyline;

                    int iid = 0;
                    LatestPt(curpt, poly, ref iid);
                    curChain.Pts[curChain.Cur] = (iid == 0 ? poly.ToPoint : poly.FromPoint);
                    FindChain(fc, fealist[0], spatialFilter, ref hgbidx, ref gbidx, ref nameidx, chainIdx);
                }
            }
            Marshal.ReleaseComObject(curpt);
            //另一端点
            if (bOther)
            {
                curChain.Cur = 1 - curChain.Cur;
                FindChain(fc, fea, spatialFilter, ref hgbidx, ref gbidx, ref nameidx, chainIdx, false, true);
            }
        }
        private void LatestPt(IPoint pt, IPolyline line, ref int idx)
        {
            IPoint p1 = line.FromPoint, p2 = line.ToPoint;
            var d1 = Math.Abs(pt.X - p1.X) + Math.Abs(pt.Y - p1.Y);
            var d2 = Math.Abs(pt.X - p2.X) + Math.Abs(pt.Y - p2.Y);
            if (d1 > d2)
                idx = 1;
            else
                idx = 0;
        }

        class RiverChain
        {
            public List<IFeature> Fealist;
            // public int FinishNum=0;
            public string HGB;
            public string Name;
            //public IPoint FromPt;
            //public IPoint ToPt;
            public IPoint[] Pts;
            public int Cur;
            public int sid;
            //public int PreIdx;
            //public int NextIdx;

        }

    }
}


