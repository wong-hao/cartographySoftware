using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 相交线打断（对被选择的相交线打断为多条线）
    /// </summary>
    public class IntersectLineBreakCmd : SMGI.Common.SMGICommand
    {
        private double _tolerance;//容差
        public IntersectLineBreakCmd()
        {
            m_caption = "相交线打断";
            m_toolTip = "相交线打断";

            _tolerance = 0.001;//米
        }

        public override bool Enabled
        {
            get
            {
                if (m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    if (0 == m_Application.MapControl.Map.SelectionCount)
                        return false;

                    try
                    {
                        IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                        mapEnumFeature.Reset();
                        IFeature feature = mapEnumFeature.Next();

                        bool res = false;
                        int nSelLine = 0;
                        while (feature != null)
                        {
                            if (feature.Shape is IPolyline)
                            {
                                nSelLine++;
                            }

                            if (nSelLine > 1)
                            {
                                res = true;
                                break;
                            }

                            feature = mapEnumFeature.Next();
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);

                        return res;
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                    return false;
            }
        }

        public override void OnClick()
        {
            #region 收集所有被选择的要素
            List<IFeature> selFeas = new List<IFeature>();
            IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
            mapEnumFeature.Reset();
            IFeature feature = mapEnumFeature.Next();
            while (feature != null)
            {
                if (feature.Shape is IPolyline)
                {
                    selFeas.Add(feature);
                }

                feature = mapEnumFeature.Next();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);
            #endregion

            #region 收集交点
            //容差
            UnitConverterClass unitConverter = new UnitConverterClass();
            double tolerance = unitConverter.ConvertUnits(_tolerance, ESRI.ArcGIS.esriSystem.esriUnits.esriMeters, m_Application.MapControl.MapUnits);
            
            Dictionary<IFeature, IPointCollection> fea2InterPoints = new Dictionary<IFeature, IPointCollection>();
            for (int i = 0; i < selFeas.Count(); ++i)
            {
                IFeature f1 = selFeas[i];

                for (int j = i + 1; j < selFeas.Count(); ++j)
                {
                    IFeature f2 = selFeas[j];

                    ITopologicalOperator2 pTopo = (ITopologicalOperator2)f1.ShapeCopy;
                    pTopo.IsKnownSimple_2 = false;
                    pTopo.Simplify();

                    IGeometry InterGeo = pTopo.Intersect(f2.Shape, esriGeometryDimension.esriGeometry0Dimension);
                    if (null == InterGeo || true == InterGeo.IsEmpty)
                        continue;

                    IPointCollection pPointColl = (IPointCollection)InterGeo;

                    if (fea2InterPoints.ContainsKey(f1))
                    {
                        IPointCollection pts = fea2InterPoints[f1];
                        for (int k = 0; k < pPointColl.PointCount; ++k)
                        {
                            IProximityOperator ProxiOP = (pPointColl.get_Point(k)) as IProximityOperator;
                            bool bContain = false;
                            for (int g = 0; g < pts.PointCount; ++g)
                            {
                                if (ProxiOP.ReturnDistance(pts.get_Point(g)) < tolerance)
                                {
                                    bContain = true;
                                    break;
                                }
                            }

                            if (!bContain)
                            {
                                pts.AddPoint(pPointColl.get_Point(k));
                            }
                            
                        }
                    }
                    else
                    {
                        IPointCollection newPts = new PolylineClass();
                        newPts.AddPointCollection(pPointColl);
                        fea2InterPoints.Add(f1, newPts);
                    }

                    if (fea2InterPoints.ContainsKey(f2))
                    {
                        IPointCollection pts = fea2InterPoints[f2];
                        for (int k = 0; k < pPointColl.PointCount; ++k)
                        {
                            IProximityOperator ProxiOP = (pPointColl.get_Point(k)) as IProximityOperator;
                            bool bContain = false;
                            for (int g = 0; g < pts.PointCount; ++g)
                            {
                                if (ProxiOP.ReturnDistance(pts.get_Point(g)) < tolerance)
                                {
                                    bContain = true;
                                    break;
                                }
                            }

                            if (!bContain)
                            {
                                pts.AddPoint(pPointColl.get_Point(k));
                            }

                        }
                    }
                    else
                    {
                        IPointCollection newPts = new PolylineClass();
                        newPts.AddPointCollection(pPointColl);
                        fea2InterPoints.Add(f2, newPts);
                    }
                }
            }
            #endregion

            #region 打断
            if (fea2InterPoints.Count == 0)
                return;

            m_Application.EngineEditor.StartOperation();
            foreach (var item in fea2InterPoints)
            {
                List<IFeature> SplitedFeas = new List<IFeature>();
                SplitedFeas.Add(item.Key);
                IPointCollection pts = item.Value;

                for (int i = 0; i < pts.PointCount; ++i)
                {
                    foreach (var f in SplitedFeas)
                    {
                        int guidIndex = f.Fields.FindField(cmdUpdateRecord.CollabGUID);
                        int verIndex = f.Fields.FindField(cmdUpdateRecord.CollabVERSION);

                        string collGUID = "";
                        if(guidIndex != -1)
                        {
                            collGUID = f.get_Value(guidIndex).ToString();
                        }
                        int smgiver = -999;
                        if(verIndex != -1)
                        {
                            int.TryParse(f.get_Value(verIndex).ToString(), out smgiver);
                            f.set_Value(verIndex, cmdUpdateRecord.NewState);//直接删除的标志
                        }
                        
                        try
                        {
                            IPoint interPt = pts.get_Point(i);

                            ITopologicalOperator2 pTopo = (ITopologicalOperator2)f.ShapeCopy;
                            pTopo.IsKnownSimple_2 = false;
                            pTopo.Simplify();
                            IGeometry InterGeo = pTopo.Intersect(interPt, esriGeometryDimension.esriGeometry0Dimension);
                            if (null == InterGeo || true == InterGeo.IsEmpty)
                                continue;

                            IFeatureEdit pFeatureEdit = (IFeatureEdit)f;
                            ISet pFeatureSet = pFeatureEdit.Split(interPt);
                            if (pFeatureSet != null)
                            {
                                pFeatureSet.Reset();

                                List<IFeature> flist = new List<IFeature>();
                                int maxIndex = -1;
                                double maxLen = 0;
                                while (true)
                                {
                                    IFeature fe = pFeatureSet.Next() as IFeature;
                                    if (fe == null)
                                    {
                                        break;
                                    }

                                    if ((fe.Shape as IPolyline).Length > maxLen)
                                    {
                                        maxLen = (fe.Shape as IPolyline).Length;
                                        maxIndex = flist.Count();
                                    }
                                    flist.Add(fe);

                                    //新增要素
                                    SplitedFeas.Add(fe);
                                }

                                if (cmdUpdateRecord.EnableUpdate && guidIndex != -1)
                                {
                                    for (int k = 0; k < flist.Count(); ++k)
                                    {
                                        if (maxIndex == k)
                                        {
                                            if (smgiver >= 0 || smgiver == cmdUpdateRecord.EditState)
                                            {
                                                flist[k].set_Value(verIndex, cmdUpdateRecord.EditState);
                                            }

                                            flist[k].set_Value(guidIndex, collGUID);//默认由最大的新要素继承原要素的collGUID

                                            flist[k].Store();

                                            break;
                                        }
                                    }
                                }

                                //移除原要素
                                SplitedFeas.Remove(f);
                                break;
                            }

                            
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine(ex.Message);
                            System.Diagnostics.Trace.WriteLine(ex.Source);
                            System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                        }
                        
                    }
                }
            }
            m_Application.EngineEditor.StopOperation("相交线打断");
            #endregion

            m_Application.MapControl.Map.ClearSelection();
            m_Application.ActiveView.Refresh();
        }
    }
}
