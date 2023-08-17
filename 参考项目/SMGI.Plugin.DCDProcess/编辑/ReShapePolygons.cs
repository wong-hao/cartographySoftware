using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using SMGI.Common;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    public class ReShapePolygons : SMGI.Common.SMGITool
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
        private IFeature tracePolygon = null;//追踪目标polygon
        private IPoint traceStartPoint = null;  //追踪起点
        #endregion

        public ReShapePolygons()
        {
            m_caption = "高级修面";
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "修线.cur"));
            m_toolTip = "高级修面工具，当需要追踪要素时，通过Ctrl+鼠标左键选中(切换)需要追踪要素，点击要素，系统自动追踪点;鼠标右键取消追踪;";
            m_category = "基础编辑";
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
                    
                    tracePolyline = DCDHelper.SelectTraceLineAndPolygonByPoint(ToSnapedMapPoint(x, y), out tracePolygon);
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
                    MessageBoxTimeOut box = new MessageBoxTimeOut();
                    if (tracePolyline == null) {
                        isTracing = false;
                        //box.Show("追踪线为空，请点击右键取消追踪", "提示", 800);
                        return;
                    }
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
                                Console.WriteLine("被添加点坐标({0},{1})", ToSnapedMapPoint(x, y).X, ToSnapedMapPoint(x, y).Y);
                            }
                            else
                            {
                                lineFeedback.AddPoint(ToSnapedMapPoint(x, y));
                                Console.WriteLine("被添加点坐标({0},{1})", ToSnapedMapPoint(x, y).X, ToSnapedMapPoint(x, y).Y);
                            }

                            //初始化追踪起点
                            traceStartPoint = outPoint;
                        }
                        else
                        {
                            //确认用户处于追踪状态且点击位置不能离追踪要素过远
                            //屏幕坐标到地图坐标
                            IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                            ISnappingResult snapResult = m_snapper.Snap(currentMouseCoords);
                            if (snapResult != null)
                            {
                                IPoint currentMouseSnappedCoord = snapResult.Location;
                                IFeature tracePolygon2 = null;
                                IPolyline tracePolyline2 = DCDHelper.SelectTraceLineAndPolygonByPoint(ToSnapedMapPoint(x, y), out tracePolygon2);

                                if (tracePolygon2 != null)
                                {
                                    if (tracePolygon.OID == tracePolygon2.OID)
                                    {
                                        IPointCollection tracePoints = DCDHelper.getColFromTwoPoint(tracePolyline, traceStartPoint, outPoint);
                                        for (int j = 1; j < tracePoints.PointCount; j++)//起点不重复添加
                                        {
                                            lineFeedback.AddPoint(tracePoints.get_Point(j));
                                            Console.WriteLine("被添加点坐标({0},{1})", tracePoints.get_Point(j).X, tracePoints.get_Point(j).Y);
                                        }
                                    }
                                    else
                                    {
                                        //isTracing = false;
                                        //tracePolyline = null;
                                        //traceStartPoint = null;
                                        //DCDHelper.DeleteTraceLineElement();
                                        //lineFeedback.AddPoint(ToSnapedMapPoint(x, y));
                                        Console.WriteLine("离{0}过远", tracePolygon.OID);
                                        box.Show("请在被追踪的要素上点击或者取消追踪", "提示2", 800);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("离{0}过远", tracePolygon.OID);
                                    box.Show("请在被追踪的要素上点击或者取消追踪", "提示3", 800);
                                }
                                //更新追踪起点
                                traceStartPoint = outPoint;
                            }
                            else 
                            {
                                //未捕捉到要素
                                box.Show("请在被追踪的要素上点击或者取消追踪", "提示1", 800);
                            }
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
                        Console.WriteLine("被添加点坐标({0},{1})", ToSnapedMapPoint(x, y).X, ToSnapedMapPoint(x, y).Y);
                    }
                    else
                    {
                        lineFeedback.AddPoint(ToSnapedMapPoint(x, y));
                        Console.WriteLine("被添加点坐标({0},{1})", ToSnapedMapPoint(x, y).X, ToSnapedMapPoint(x, y).Y);
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
                //Console.WriteLine("!=null:{0},{1}", x,y);
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

            //IPointCollection reshapePath = new PathClass();
            //reshapePath.AddPointCollection(polyline as IPointCollection);

            ITopologicalOperator trackTopo = polyline as ITopologicalOperator;
            trackTopo.Simplify();


            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon;

            })).ToArray();

            IPointCollection geoReshapePath = new PathClass();
            geoReshapePath.AddPointCollection(polyline as IPointCollection);
            //for (int i = 0; i < (polyline as IPointCollection).PointCount; i++) {
            //    geoReshapePath.AddPoint((polyline as IPointCollection).get_Point(i));
            //    Console.WriteLine("{0}:{1},{2}",i, (polyline as IPointCollection).get_Point(i).X, (polyline as IPointCollection).get_Point(i).Y);
            //}
            //geoReshapePath.AddPoint((polyline as IPointCollection).get_Point(0));
            DCDHelper.DrawTraceLineElement(polyline);

                try
                {
                    IEngineEditLayers editLayer = editor as IEngineEditLayers;

                    editor.StartOperation();

                    foreach (var item in lyrs)
                    {
                        if (!item.Visible)
                        {
                            continue;
                        }

                        if (!editLayer.IsEditable(item as IFeatureLayer))
                        {
                            continue;
                        }


                        IGeoFeatureLayer geoFealyr = item as IGeoFeatureLayer;
                        IFeatureClass fc = geoFealyr.FeatureClass;

                        List<int> oidList = new List<int>();//当前图层中参与到该操作的要素OID集合（删除要素除外）

                        ISpatialFilter sf = new SpatialFilter();
                        sf.Geometry = polyline;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        if (fc.HasCollabField())//已删除的要素不参与
                        {
                            sf.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                        }

                        IFeatureCursor pCursor = fc.Search(sf, true);
                        IFeature pFeature;
                        while ((pFeature = pCursor.NextFeature()) != null)
                        {
                            oidList.Add(pFeature.OID);
                        }
                        Marshal.ReleaseComObject(pCursor);

                        foreach (var oid in oidList)
                        {
                            pFeature = fc.GetFeature(oid);

                            IPolygon pg = pFeature.ShapeCopy as IPolygon;
                            //DCDHelper.DrawPolygonElement(geoReshapePath as IPolygon);
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
                            pFeature.Shape = pg;
                            pFeature.Store();
                        }
                    }

                    editor.StopOperation("高级修面");

                    m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);

                    ClearSnapperCache();

                    Console.WriteLine("高级修面结束");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    editor.AbortOperation();
                    System.Diagnostics.Trace.WriteLine(ex.Message, "Edit Geometry Failed");
                }
        }

        public override bool Deactivate()
        {
            //卸掉该事件
            m_Application.MapControl.OnAfterScreenDraw -= new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);

            return base.Deactivate();
        }
    }


    class MessageBoxTimeOut
     {
         private string _caption;
 
         public void Show(string text, string caption, int timeout)
         {
             this._caption = caption;
             StartTimer(timeout);
             MessageBox.Show(text, caption);
         }
 
         private void StartTimer(int interval)
         {
             Timer timer = new Timer();
             timer.Interval = interval;
             timer.Tick += new EventHandler(Timer_Tick);
             timer.Enabled = true;
         }
 
         private void Timer_Tick(object sender, EventArgs e)
         {
             KillMessageBox();
             //停止计时器
             ((Timer)sender).Enabled = false;
         }
 
         [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
         private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
 
         [DllImport("user32.dll", CharSet = CharSet.Auto)]
         public static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
 
         public const int WM_CLOSE = 0x10;
 
         private void KillMessageBox()
         {
             //查找MessageBox的弹出窗口,注意对应标题
             IntPtr ptr = FindWindow(null, this._caption);
             if (ptr != IntPtr.Zero)
             {
                 //查找到窗口则关闭
                 PostMessage(ptr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
             }
         }       
     }
}

