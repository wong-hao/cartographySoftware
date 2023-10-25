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
namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public class ReshapeFeature : SMGITool
    { /// <summary>
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
        List<IPoint> ptList; //记录点
        IPoint ptLast;

        #region 追踪
        private bool isTracing = false;  //判断是否处于追踪状态
        private IPolyline tracePolyline = null;//追踪目标线
        private IPoint traceStartPoint = null;  //追踪起点
        
        #endregion
        public ReshapeFeature()
        {
            m_caption = "修形";
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "修线.cur"));
            m_toolTip = "修形工具，当需要追踪要素时，通过Ctrl+鼠标左键选中(切换)需要追踪的线要素，点击线要素，系统自动追踪点;鼠标右键取消追踪;";
            m_category = "基础编辑";

        }

        public override bool Enabled
        {
            get
            {
                if (m_Application == null || m_Application.Workspace == null || m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing)
                    return false;

                if (m_Application.MapControl.Map.SelectionCount < 1)
                    return false;

                //IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                //mapEnumFeature.Reset();
                //IFeature feature = mapEnumFeature.Next();
                //Marshal.ReleaseComObject(mapEnumFeature);
                //if (feature == null || feature.Shape.GeometryType == esriGeometryType.esriGeometryPoint)
                //    return false;

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
            ptList = new List<IPoint>();
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
                                ptList.Clear();
                                ptList.Add(ToSnapedMapPoint(x, y));
                            }
                            else
                            {
                                lineFeedback.AddPoint(ToSnapedMapPoint(x, y));
                                ptList.Add(ToSnapedMapPoint(x, y));
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
                                ptList.Add(tracePoints.get_Point(j));
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
                        ptList.Add(ToSnapedMapPoint(x, y));
                    }
                    else
                    {
                        lineFeedback.AddPoint(ToSnapedMapPoint(x, y));
                        ptList.Add(ToSnapedMapPoint(x, y));
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
                ptLast = ToSnapedMapPoint(x, y);
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
            ptList.Clear();
            lineFeedback = null;
            ptLast = null;
            if (null == polyline || polyline.IsEmpty)
                return;

            IPointCollection reshapePath = new PathClass();
            reshapePath.AddPointCollection(polyline as IPointCollection);
            ITopologicalOperator trackTopo = polyline as ITopologicalOperator;
            trackTopo.Simplify();
            IPointCollection geoReshapePath = new PathClass();
            geoReshapePath.AddPointCollection(polyline as IPointCollection);
            IEnumFeature pEnumFeature = (IEnumFeature)m_Application.MapControl.Map.FeatureSelection;
            ((IEnumFeatureSetup)pEnumFeature).AllFields = true;
            IFeature feature = null;
            editor.StartOperation();
            while ((feature = pEnumFeature.Next()) != null)
            {
                IFeatureClass fc = feature.Class as IFeatureClass;
                if ((fc as IDataset).Workspace != m_Application.EngineEditor.EditWorkspace)
                {
                    continue; //非开启编辑空间的要素，不编辑 zhx@2022.4.12
                }

                try
                {
                    if (feature.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                    {
                        //修线
                        IPolyline editShape = feature.Shape as IPolyline;
                        editShape.Reshape(reshapePath as IPath);
                        feature.Shape = editShape;
                        feature.Store();

                    }
                    else if (feature.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                    {
                        IPolygon pg = feature.ShapeCopy as IPolygon;
                        IGeometryCollection pgCol = pg as IGeometryCollection;
                        for (int i = 0; i < pgCol.GeometryCount; i++)
                        {
                            IRing r = pgCol.get_Geometry(i) as IRing;
                            try
                            {
                                r.Reshape(geoReshapePath as IPath);
                            }
                            catch (Exception)
                            {

                                continue;
                            }
                        }
                        (pg as ITopologicalOperator).Simplify();
                        feature.Shape = pg;
                        feature.Store();
                    }
                  

                    //m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
                    m_Application.MapControl.ActiveView.Refresh();
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
            editor.StopOperation("修形");


        }

        public override bool Deactivate()
        {
            //卸掉该事件
            m_Application.MapControl.OnAfterScreenDraw -= new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);
            return base.Deactivate();
        }

        public override void OnKeyUp(int keyCode, int shift)
        {
            base.OnKeyUp(keyCode, shift);
            if (keyCode == (int)Keys.Z)
            {
                int n = ptList.Count;
                if (n <= 1)
                {
                    if (lineFeedback != null)
                    {
                        lineFeedback.Stop();
                        lineFeedback = null;
                        ptLast = null;
                    }
                    ptList.Clear();
                }
                else
                {
                    if (lineFeedback != null)
                    {
                        lineFeedback.Stop();
                        lineFeedback = new NewLineFeedbackClass { Display = m_Application.ActiveView.ScreenDisplay, Symbol = lineSymbol as ISymbol };
                        lineFeedback.Start(ptList[0]);
                        for (int i = 0; i < n - 1; i++)
                            lineFeedback.AddPoint(ptList[i]);                        
                        lineFeedback.MoveTo(ptLast);
                        ptList.RemoveAt(n - 1);
                    }
                }
               
            }
            
        }
    }
}
