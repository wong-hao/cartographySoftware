using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Editor;


namespace SMGI.Plugin.DCDProcess
{
    public class ReshapePolyline:SMGITool
    {
        /// <summary>
        /// 编辑器
        /// </summary>
        IEngineEditor editor;
        /// <summary>
        /// 线型符号
        /// </summary>
        ISimpleLineSymbol lineSymbol;
        /// <summary>
        /// 线型反馈
        /// </summary>
        INewLineFeedback lineFeedback;

        #region 追踪
        private bool isTracing = false;  //判断是否处于追踪状态
        private IPolyline tracePolyline = null;//追踪目标线
        private IPoint traceStartPoint = null;  //追踪起点
        #endregion

        public ReshapePolyline()
        {
            m_caption = "修线";
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "修线.cur"));
            m_toolTip = "修线工具，当需要追踪要素时，通过Ctrl+鼠标左键选中(切换)需要追踪的线要素，点击线要素，系统自动追踪点;鼠标右键取消追踪;";
            m_category = "基础编辑";
          
        }

        public override bool Enabled
        {
            get
            {
                if (m_Application == null || m_Application.Workspace == null || m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing)
                    return false;

                if (m_Application.MapControl.Map.SelectionCount != 1)
                    return false;

                IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                mapEnumFeature.Reset();
                IFeature feature = mapEnumFeature.Next();
                Marshal.ReleaseComObject(mapEnumFeature);
                if (feature == null || feature.Shape.GeometryType != esriGeometryType.esriGeometryPolyline)
                    return false;

                return true;
            }
        }

        public override void OnClick()
        {
            editor = m_Application.EngineEditor;
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

        public override void OnMouseDown(int Button, int Shift, int x, int y)
        {
            if (Button == 1)
            {
                if (Shift == 2)
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
            else if (Button == 2)
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
            if (null == polyline || polyline.IsEmpty)
                return;

            IPointCollection reshapePath = new PathClass();
            reshapePath.AddPointCollection(polyline as IPointCollection);

            IEnumFeature pEnumFeature = (IEnumFeature)m_Application.MapControl.Map.FeatureSelection;
            ((IEnumFeatureSetup)pEnumFeature).AllFields = true;
            IFeature feature = pEnumFeature.Next();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumFeature);

            editor.StartOperation();
            try
            {
                //修线
                IPolyline editShape = feature.Shape as IPolyline;
                editShape.Reshape(reshapePath as IPath);
                feature.Shape = editShape;
                feature.Store();

                #region 线面联动
                ISpatialFilter sf = new SpatialFilter();
                sf.Geometry = polyline;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l.Visible && l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon;

                })).ToArray();

                IEngineEditLayers editLayer = m_Application.EngineEditor as IEngineEditLayers;
                foreach (var item in lyrs)
                {
                    if (!editLayer.IsEditable(item as IFeatureLayer))
                        continue;

                    IGeoFeatureLayer geoFealyr = item as IGeoFeatureLayer;
                    IFeatureClass fc = geoFealyr.FeatureClass;
                    IFeatureCursor pCursor = fc.Search(sf, false);
                    IFeature pFeature;
                    while ((pFeature = pCursor.NextFeature()) != null)
                    {
                        IPolygon pg = pFeature.ShapeCopy as IPolygon;
                        IGeometryCollection pgCol = pg as IGeometryCollection;
                        for (int i = 0; i < pgCol.GeometryCount; i++)
                        {
                            IRing r = pgCol.get_Geometry(i) as IRing;
                            try
                            {
                                r.Reshape(reshapePath as IPath);
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                        }
                        (pg as ITopologicalOperator).Simplify();
                        pFeature.Shape = pg;
                        pFeature.Store();
                    }
                    Marshal.ReleaseComObject(pCursor);
                }
                #endregion

                editor.StopOperation("修线");

                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);

                ClearSnapperCache();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                editor.AbortOperation();
                System.Diagnostics.Trace.WriteLine(ex.Message, "Edit Geometry Failed");

                MessageBox.Show(ex.Message);
            }

            
        }

        public override bool Deactivate()
        {
            //卸掉该事件
            m_Application.MapControl.OnAfterScreenDraw -= new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);
            return base.Deactivate();
        }
    }
}
