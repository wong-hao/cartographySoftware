using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using System.Xml.Linq;
using ESRI.ArcGIS.Geometry;
using System.Data;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 处理地名等点符号与道路等线符号的冲突
    /// 当某个点符号与多个线符号冲突时，仅处理与最高级别线之间的冲突（其它低级别的冲突不考虑）
    /// 增加点符号与面边线符号的压盖冲突
    /// </summary>
    public class AGNPConflictMovingTool : SMGI.Common.SMGITool
    {
        private static Dictionary<string, Dictionary<double, string>> _ptFCName2SizeAndSql = null;
        private static Dictionary<string, Dictionary<double, string>> _lineFCName2WidthAndSql = null;
        private static double _minSpace;
        private static Dictionary<string, IFeatureLayer> _fcName2Layer = null;

        public AGNPConflictMovingTool()
        {
            m_category = "点符号与线符号冲突处理";

            NeedSnap = false;
        }

        public override bool Enabled
        {
            get
            {
                if (m_Application.Workspace == null)
                    _ptFCName2SizeAndSql = null;

                return m_Application != null && m_Application.Workspace != null &&
                    m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            if (_ptFCName2SizeAndSql == null)
            {
                ParamInit();
            }
            
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
                return;

            if (m_Application.MapControl.Map.ReferenceScale == 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;
            }

            //拉框
            IRubberBand rubberBand = new RubberRectangularPolygonClass();
            IGeometry geo = rubberBand.TrackNew(m_Application.ActiveView.ScreenDisplay, null);
            if (geo == null || geo.IsEmpty)
                return;

            
            //清理所选要素
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            m_Application.MapControl.Map.ClearSelection();

            //自动避让处理
            PointSymbolAvoidLineSymbol(_ptFCName2SizeAndSql, _lineFCName2WidthAndSql, _fcName2Layer, geo, m_Application.MapControl.Map.ReferenceScale, _minSpace);

            //刷新
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, m_Application.MapControl.ActiveView.Extent);
            
        }

        public override void OnKeyUp(int keyCode, int shift)
        {
            switch (keyCode)
            {
                case 32:
                    ParamInit();
                    break;
                default:
                    break;
            }
        }

        private void ParamInit()
        {
            try
            {
                #region 
                string fileName = m_Application.Template.Root + @"\POIMovingRule.xml";
                if (!System.IO.File.Exists(fileName))
                {
                    throw new Exception(string.Format("未找到配置文件【{0}】！", fileName));
                }

                XDocument ruleDoc = XDocument.Load(fileName);
                AGNPConflictMovingForm frm;
                if (_ptFCName2SizeAndSql == null)
                {
                    frm = new AGNPConflictMovingForm(m_Application, ruleDoc);
                }
                else
                {
                    frm = new AGNPConflictMovingForm(m_Application, ruleDoc, _ptFCName2SizeAndSql, _lineFCName2WidthAndSql, _minSpace);
                }
                frm.StartPosition = FormStartPosition.CenterParent;
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    _ptFCName2SizeAndSql = frm.PtFCName2SizeAndSql;
                    _lineFCName2WidthAndSql = frm.LineFCName2WidthAndSql;
                    _minSpace = frm.MinSpacing;

                    _fcName2Layer = new Dictionary<string, IFeatureLayer>();
                    foreach (var kv in _ptFCName2SizeAndSql)
                    {
                        string fcName = kv.Key.ToUpper().Trim();
                        IFeatureLayer layer = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                        {
                            return (l is IFeatureLayer) && (l as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper().Trim() == fcName);
                        })).FirstOrDefault() as IFeatureLayer;
                        if (layer == null)
                        {
                            throw new Exception(string.Format("未找到要素类【{0}】对应的点图层！", fcName));
                        }

                        if (!_fcName2Layer.ContainsKey(fcName))
                        {
                            _fcName2Layer.Add(fcName, layer);
                        }
                    }

                    foreach (var kv in _lineFCName2WidthAndSql)
                    {
                        string fcName = kv.Key.ToUpper().Trim();
                        IFeatureLayer layer = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                        {
                            return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline || (l as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper().Trim() == fcName);
                        })).FirstOrDefault() as IFeatureLayer;
                        if (layer == null)
                        {
                            throw new Exception(string.Format("未找到要素类【{0}】对应的线（面）图层！", fcName));
                        }

                        if (!_fcName2Layer.ContainsKey(fcName))
                        {
                            _fcName2Layer.Add(fcName, layer);
                        }
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                _ptFCName2SizeAndSql = null;
                _lineFCName2WidthAndSql = null;
                _fcName2Layer = null;
            }

            if (_ptFCName2SizeAndSql == null)
            {
                m_Application.MapControl.CurrentTool = null;
            }
        }

        private void PointSymbolAvoidLineSymbol(Dictionary<string, Dictionary<double, string>> ptFCName2SizeAndSql, Dictionary<string, Dictionary<double, string>> lineFCName2WidthAndSql,
             Dictionary<string, IFeatureLayer> fcName2Layer,IGeometry extentGeo, double refScale, double minSpace = 0)
        {
            m_Application.EngineEditor.StartOperation();
            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    ISpatialFilter sf = new SpatialFilterClass();
                    sf.Geometry = extentGeo;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                    Dictionary<string, Dictionary<int, double>> fcName2oidsize = new Dictionary<string,Dictionary<int,double>>();
                    #region 查询范围内点要素信息
                    foreach (var kv in ptFCName2SizeAndSql)
                    {
                        string fcName = kv.Key;
                        IFeatureLayer layer = fcName2Layer[fcName];
                        foreach (var kv2 in kv.Value)
                        {
                            sf.WhereClause = kv2.Value;
                            double size = kv2.Key;

                            IFeatureCursor feCursor = layer.FeatureClass.Search(sf, true);
                            IFeature fe = null;
                            while ((fe = feCursor.NextFeature()) != null)
                            {
                                wo.SetText(string.Format("正在检索范围内图层【{0}】的要素【{1}】......", layer.Name, fe.OID));

                                if (fcName2oidsize.ContainsKey(fcName))
                                {
                                    fcName2oidsize[fcName].Add(fe.OID, size);
                                }
                                else
                                {
                                    Dictionary<int, double> oid2size = new Dictionary<int, double>();
                                    oid2size.Add(fe.OID, size);

                                    fcName2oidsize.Add(fcName, oid2size);
                                }
                            }
                            Marshal.ReleaseComObject(feCursor);
                        }
                        
                    }
                    #endregion

                    Dictionary<string, Dictionary<int, double>> fcName2oidwidth = new Dictionary<string, Dictionary<int, double>>();
                    #region 查询范围内参考线要素信息
                    foreach (var kv in lineFCName2WidthAndSql)
                    {
                        string fcName = kv.Key;
                        IFeatureLayer layer = fcName2Layer[fcName];
                        foreach (var kv2 in kv.Value)
                        {
                            sf.WhereClause = kv2.Value;
                            double width = kv2.Key;

                            IFeatureCursor referFeCursor = layer.FeatureClass.Search(sf, true);
                            IFeature referFe = null;
                            while ((referFe = referFeCursor.NextFeature()) != null)
                            {
                                wo.SetText(string.Format("正在检索范围内图层【{0}】的要素【{1}】......", layer.Name, referFe.OID));

                                if (fcName2oidwidth.ContainsKey(fcName))
                                {
                                    fcName2oidwidth[fcName].Add(referFe.OID, width);
                                }
                                else
                                {
                                    Dictionary<int, double> oid2width = new Dictionary<int, double>();
                                    oid2width.Add(referFe.OID, width);

                                    fcName2oidwidth.Add(fcName, oid2width);
                                }
                            }
                            Marshal.ReleaseComObject(referFeCursor);
                        }
                    }
                    #endregion

                    if (fcName2oidsize.Count > 0 && fcName2oidwidth.Count > 0)
                    {
                        double maxWidthInAllLine = 0;
                        Dictionary<string,string> fcName2oidSet = new Dictionary<string,string>();
                        foreach (var kv in fcName2oidwidth)
                        {
                            string fcName = kv.Key;

                            foreach (var kv2 in kv.Value)
                            {
                                double width = kv2.Value;
                                if (width > maxWidthInAllLine)
                                    maxWidthInAllLine = width;

                                if (fcName2oidSet.ContainsKey(fcName))
                                {
                                    fcName2oidSet[fcName] += string.Format(",{0}", kv2.Key);
                                }
                                else
                                {
                                    string oidSet = string.Format("{0}", kv2.Key);
                                    fcName2oidSet.Add(fcName, oidSet);
                                }
                            }

                            
                        }
                        
                        foreach (var kv in fcName2oidsize)
                        {
                            string fcName = kv.Key;
                            IFeatureLayer ptLayer = fcName2Layer[fcName];
                            foreach(var kv2 in kv.Value)
                            {
                                int ptFeOID = kv2.Key;
                                double ptSize = kv2.Value;

                                #region 更新点的位置
                                wo.SetText(string.Format("正在检查及处理图层【{0}】的要素【{1}】......", fcName2Layer[fcName].Name, ptFeOID));

                                IFeature ptFe = ptLayer.FeatureClass.GetFeature(ptFeOID);

                                double maxWidth = 0;//与该地名点冲突的最高级别线符号的宽度
                                IPoint nearestPoint = null;//线要素中与点要素的最近点

                                #region 查找目标点范围内与之可能存在冲突的最高级别（宽度）的线要素OID，若某一点符号与多个线符号冲突，则只处理与最高级别（宽度最大）线符号之间的冲突
                                double bufferVal = (ptSize + maxWidthInAllLine) * 0.5 * 0.001 * refScale;
                                IGeometry bufferGeo = (ptFe.Shape as ITopologicalOperator).Buffer(bufferVal);

                                sf.Geometry = bufferGeo;

                                foreach(var kv3 in fcName2oidwidth)//遍历所有线图层
                                {
                                    IFeatureLayer referLineLayer = fcName2Layer[kv3.Key];
                                    sf.WhereClause = string.Format("OBJECTID in ({0})", fcName2oidSet[kv3.Key]);
                                    IFeatureCursor referFeCursor = referLineLayer.FeatureClass.Search(sf, true);
                                    IFeature referFe = null;
                                    while ((referFe = referFeCursor.NextFeature()) != null)
                                    {
                                        if (kv3.Value.ContainsKey(referFe.OID))
                                        {
                                            double width = kv3.Value[referFe.OID];

                                            #region 计算地名点与该线的最短距离,判断与该线（面边线）是否存在冲突
                                            IPoint outPoint = new Point();
                                            double distanceAlongCurve = 0;
                                            double distanceFromCurve = 0;
                                            bool bRightSide = false;
                                            if (referFe.Shape is IPolyline)
                                            {
                                                (referFe.Shape as IPolyline).QueryPointAndDistance(esriSegmentExtension.esriNoExtension, ptFe.Shape as IPoint, false, outPoint,
                                                    ref distanceAlongCurve, ref distanceFromCurve, ref bRightSide);
                                            }
                                            else if (referFe.Shape is IPolygon)
                                            {
                                                ((referFe.Shape as ITopologicalOperator).Boundary as IPolyline).QueryPointAndDistance(esriSegmentExtension.esriNoExtension, ptFe.Shape as IPoint, false, outPoint,
                                                    ref distanceAlongCurve, ref distanceFromCurve, ref bRightSide);
                                            }
                                            if (distanceFromCurve >= (ptSize + width) * 0.5 * 0.001 * refScale)
                                                continue;//点符号与线符号没有冲突
                                            #endregion

                                            //存在冲突的情况下，只记录最大宽度的线要素信息
                                            if (width > maxWidth)
                                            {
                                                maxWidth = width;
                                                nearestPoint = outPoint;
                                            }
                                        }
                                    }
                                    Marshal.ReleaseComObject(referFeCursor);
                                }

                                #endregion

                                if (nearestPoint == null)
                                continue;//没有与之冲突的线符号

                                #region 计算点符号偏移的位置
                                double minDistace = ((ptSize + maxWidth) * 0.5 + minSpace) * 0.001 * refScale;//该点要素与线要素应保持的最小距离
                                IPolyline tempLine = new PolylineClass();
                                tempLine.FromPoint = nearestPoint;
                                tempLine.ToPoint = ptFe.Shape as IPoint;
                                if (tempLine.Length == 0)
                                    continue;//目标点在冲突线上，不进行处理

                                IPoint tempPoint = new Point();
                                tempLine.QueryPoint(esriSegmentExtension.esriExtendTangents, minDistace, false, tempPoint);//延长

                                //更新点的几何位置
                                ptFe.Shape = tempPoint;
                                ptFe.Store();
                                
                                //选择要素
                                m_Application.MapControl.Map.SelectFeature(ptLayer, ptFe);

                                #endregion

                                #endregion
                            }
                        }
                    }

                }

                m_Application.EngineEditor.StopOperation("点符号与线符号冲突处理");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                m_Application.EngineEditor.AbortOperation();
            }
        }

    }
}
