using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Display;

namespace SMGI.Plugin.DCDProcess
{
    public class FeatureCutTool : SMGITool
    {
        private ISimpleLineSymbol lineSymbol;
        private INewLineFeedback lineFeedback;

        #region 追踪
        private bool isTracing = false;  //判断是否处于追踪状态
        private IPolyline tracePolyline = null;//追踪目标线
        private IPoint traceStartPoint = null;  //追踪起点
        #endregion

        public FeatureCutTool()
        {
            m_caption = "面分割";
        }

        public override void OnClick()
        {
            var map = m_Application.ActiveView.FocusMap;
            if (map.SelectionCount == 0)
            {
                MessageBox.Show("请先选择一个要素");
            }

            IEngineEditor editor = m_Application.EngineEditor;
            //#region Create a symbol to use for feedback
            lineSymbol = new SimpleLineSymbolClass();
            IRgbColor color = new RgbColorClass();	 //red
            color.Red = 255;
            color.Green = 0;
            color.Blue = 0;
            lineSymbol.Color = color;
            lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
            lineSymbol.Width = 1.5;
            (lineSymbol as ISymbol).ROP2 = esriRasterOpCode.esriROPNotXOrPen;//这个属性很重要
            //#endregion
            lineFeedback = null;
            //用于解决在绘制feedback过程中进行地图平移出现线条混乱的问题
            m_Application.MapControl.OnAfterScreenDraw += new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);
        }

        private void MapControl_OnAfterScreenDraw(object sender, IMapControlEvents2_OnAfterScreenDrawEvent e)
        {
            if (lineFeedback != null)
            {
                lineFeedback.Refresh(m_Application.ActiveView.ScreenDisplay.hDC);
            }
        }

        public override bool Enabled
        {
            get
            {
                if (m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    if (m_Application.MapControl.Map.SelectionCount != 1)
                        return false;
                    IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                    mapEnumFeature.Reset();
                    IFeature feature = mapEnumFeature.Next();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);
                    if (feature != null)
                    {
                        if (feature.Shape is IPolygon)
                        {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                            return true;
                        }
                        else
                        {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                            return false;
                        }
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button == 1)
            {
                if (shift == 2)
                {
                    #region 追踪目标确定:Ctrl+鼠标左键,选择目标线/面要素，开启追踪标识
                    tracePolyline = DCDHelper.SelectTraceLineByPoint(ToSnapedMapPoint(x, y));
                    if (tracePolyline == null)
                    {
                        MessageBox.Show("请选择要追踪的要素");
                        return;
                    }

                    isTracing = true;
                    DCDHelper.DrawTraceLineElement(tracePolyline);
                    #endregion

                    return;
                }

                if (isTracing)
                {
                    #region 追踪
                    double ms = m_Application.ActiveView.FocusMap.ReferenceScale;
                    double dis = 2.5 * ms / 1000 == 0 ? 2.5 : 2.5 * ms / 1000;

                    IPoint outPoint = new PointClass();
                    double alCurve = 0, frCurve = 0; bool rSide = false;
                    tracePolyline.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, ToSnapedMapPoint(x, y), false, outPoint, ref alCurve, ref frCurve, ref rSide);
                    if (frCurve < dis)
                    {
                        if (traceStartPoint == null)
                        {
                            if (lineFeedback == null)
                            {
                                var distance = m_Application.ActiveView.ScreenDisplay;
                                lineFeedback = new NewLineFeedbackClass { Display = distance, Symbol = lineSymbol as ISymbol };
                                lineFeedback.Start(ToSnapedMapPoint(x, y));
                            }
                            else
                            {
                                lineFeedback.AddPoint(ToSnapedMapPoint(x, y));
                            }

                            //初始化追踪起点
                            traceStartPoint = outPoint;
                        }
                        else
                        {
                            IPointCollection tracePoints = DCDHelper.getColFromTwoPoint(tracePolyline, traceStartPoint, outPoint);
                            for (int j = 1; j < tracePoints.PointCount; j++)//起点不重复添加
                            {
                                lineFeedback.AddPoint(tracePoints.get_Point(j));
                            }

                            //更新追踪起点
                            traceStartPoint = outPoint;
                        }
                    }
                    #endregion
                }
                else
                {
                    #region 非追踪:直接画点
                    if (lineFeedback == null)
                    {
                        var distance = m_Application.ActiveView.ScreenDisplay;
                        lineFeedback = new NewLineFeedbackClass { Display = distance, Symbol = lineSymbol as ISymbol };
                        lineFeedback.Start(ToSnapedMapPoint(x, y));
                    }
                    else
                    {
                        lineFeedback.AddPoint(ToSnapedMapPoint(x, y));
                    }
                    #endregion
                }

            }
            else if (button == 2)
            {
                #region 追踪状态清除
                isTracing = false;
                tracePolyline = null;
                traceStartPoint = null;

                DCDHelper.DeleteTraceLineElement();
                #endregion
            }

        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (lineFeedback != null)
            {
                lineFeedback.MoveTo(ToSnapedMapPoint(x, y));
            }
        }

        public override void OnDblClick()
        {
            #region 清除追踪线
            isTracing = false;
            tracePolyline = null;
            traceStartPoint = null;

            DCDHelper.DeleteTraceLineElement();
            #endregion

            IPolyline polyline = lineFeedback.Stop();
            lineFeedback = null;
            //双击完毕进行线条的打断
            if (null == polyline || polyline.IsEmpty)
                return;

            ITopologicalOperator2 pTopo = (ITopologicalOperator2)polyline;
            pTopo.IsKnownSimple_2 = false;
            pTopo.Simplify();

            splitPolygon(polyline);

            m_Application.MapControl.Map.ClearSelection();
            m_Application.ActiveView.Refresh();
        }

        public override void Refresh(int hdc)
        {
        }
     
        public override bool Deactivate()
        {
            //卸掉该事件
            m_Application.MapControl.OnAfterScreenDraw -= new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);
            return base.Deactivate();
        }


        private void splitPolygon(IGeometry splitGeo)
        {
            var editor = m_Application.EngineEditor;
            var map = m_Application.ActiveView.FocusMap;

            try
            {
                editor.StartOperation();
                var selectFeas = map.FeatureSelection as IEnumFeature;
                IFeature fe = null;
                while ((fe = selectFeas.Next()) != null)
                {
                    if (fe.Shape.GeometryType != esriGeometryType.esriGeometryPolygon)
                    {
                        continue;
                    }

                    ITopologicalOperator2 pTopo = (ITopologicalOperator2)splitGeo;
                    pTopo.IsKnownSimple_2 = false;
                    pTopo.Simplify();
                    IGeometry InterGeo = pTopo.Intersect(fe.Shape, esriGeometryDimension.esriGeometry1Dimension);
                    if (null == InterGeo || true == InterGeo.IsEmpty)
                        continue;


                    int guidIndex = fe.Fields.FindField(cmdUpdateRecord.CollabGUID);
                    int verIndex = fe.Fields.FindField(cmdUpdateRecord.CollabVERSION);

                    string collGUID = "";
                    if (guidIndex != -1)
                    {
                        collGUID = fe.get_Value(guidIndex).ToString();
                    }
                    int smgiver = -999;
                    if (verIndex != -1)
                    {
                        int.TryParse(fe.get_Value(verIndex).ToString(), out smgiver);
                        fe.set_Value(verIndex, cmdUpdateRecord.NewState);//直接删除的标志
                    }

                    IFeatureEdit feEdit = (IFeatureEdit)fe;
                    var feSet = feEdit.Split(splitGeo);
                    if (feSet != null)//201605
                    {
                        feSet.Reset();

                        List<IFeature> flist = new List<IFeature>();
                        int maxIndex = -1;
                        double maxAera = 0;
                        while (true)
                        {
                            IFeature f = feSet.Next() as IFeature;
                            if (f == null)
                            {
                                break;
                            }

                            if ((f.Shape as IArea).Area > maxAera)
                            {
                                maxAera = (f.Shape as IArea).Area;
                                maxIndex = flist.Count();
                            }
                            flist.Add(f);

                        }

                        if (cmdUpdateRecord.EnableUpdate && guidIndex != -1)
                        {
                            for (int k = 0; k < flist.Count(); ++k)//201605
                            {
                                if (maxIndex == k)
                                {
                                    if (smgiver >= 0 || smgiver == cmdUpdateRecord.EditState)
                                    {
                                        flist[k].set_Value(verIndex, cmdUpdateRecord.EditState);
                                    }

                                    flist[k].set_Value(guidIndex, collGUID);

                                    flist[k].Store();

                                    break;
                                }
                            }

                        }

                    }

                }
                Marshal.ReleaseComObject(selectFeas);
                editor.StopOperation("面分割");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                editor.AbortOperation();

                MessageBox.Show(ex.Message);
            }
            
        }

    }
}
