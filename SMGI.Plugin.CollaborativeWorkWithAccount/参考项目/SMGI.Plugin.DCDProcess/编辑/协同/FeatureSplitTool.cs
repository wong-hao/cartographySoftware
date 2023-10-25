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

namespace SMGI.Plugin.DCDProcess
{
    public class FeatureSplitTool:SMGITool
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

        public FeatureSplitTool()
        {
            m_caption = "分割";
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "修线.cur"));
            m_toolTip = "分割选中的要素:当需要追踪要素时，通过Ctrl+鼠标左键选中(切换)需要追踪要素，点击要素，系统自动追踪点;鼠标右键取消追踪;";
        }

        public override bool Enabled
        {
            get
            {
                if (m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    if (0 == m_Application.MapControl.Map.SelectionCount)
                        return false;


                    IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                    mapEnumFeature.Reset();
                    IFeature feature = mapEnumFeature.Next();

                    bool res = false;
                    while (feature != null)
                    {
                        if (feature.Shape is IPolyline || feature.Shape is IPolygon)
                        {
                            res = true;
                            break;
                        }

                        feature = mapEnumFeature.Next();
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);

                    return res;
                }
                else
                    return false;
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
            if (polyline.IsEmpty)
                return;

            IPointCollection reshapePath = new PathClass();
            reshapePath.AddPointCollection(polyline as IPointCollection);

            editor.StartOperation();
            try
            {
                IEnumFeature pEnumFeature = (IEnumFeature)m_Application.Workspace.Map.FeatureSelection;
                ((IEnumFeatureSetup)pEnumFeature).AllFields = true;
                IFeature SplitedFeature = null;
                while ((SplitedFeature = pEnumFeature.Next()) != null)
                {
                    if (SplitedFeature.Shape.GeometryType != esriGeometryType.esriGeometryPolyline && 
                        SplitedFeature.Shape.GeometryType != esriGeometryType.esriGeometryPolygon)
                        continue;

                    ITopologicalOperator2 pTopo = (ITopologicalOperator2)polyline;
                    pTopo.IsKnownSimple_2 = false;
                    pTopo.Simplify();

                    IGeometry SplitGeom = null;
                    if (SplitedFeature.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                    {
                        IGeometry InterGeo = pTopo.Intersect(SplitedFeature.Shape, esriGeometryDimension.esriGeometry0Dimension);
                        if (null == InterGeo || true == InterGeo.IsEmpty)
                            continue;

                        IPointCollection pPointColl = (IPointCollection)InterGeo;
                        SplitGeom = pPointColl.get_Point(0);
                    }
                    else
                    {
                        IGeometry InterGeo = pTopo.Intersect(SplitedFeature.Shape, esriGeometryDimension.esriGeometry1Dimension);
                        if (null == InterGeo || true == InterGeo.IsEmpty)
                            continue;

                        SplitGeom = polyline;

                    }

                    if (null == SplitGeom)
                        continue;

                    IFeatureEdit pFeatureEdit = (IFeatureEdit)SplitedFeature;

                    int guidIndex = SplitedFeature.Fields.FindField(cmdUpdateRecord.CollabGUID);
                    int verIndex = SplitedFeature.Fields.FindField(cmdUpdateRecord.CollabVERSION);

                    string collGUID = "";
                    if (guidIndex != -1)
                    {
                        collGUID = SplitedFeature.get_Value(guidIndex).ToString();
                    }
                    int smgiver = -999;
                    if (verIndex != -1)
                    {
                        int.TryParse(SplitedFeature.get_Value(verIndex).ToString(), out smgiver);
                        SplitedFeature.set_Value(verIndex, cmdUpdateRecord.NewState);//直接删除的标志
                    }

                    ISet pFeatureSet = pFeatureEdit.Split(SplitGeom);
                    if (pFeatureSet != null)//201605
                    {
                        pFeatureSet.Reset();

                        List<IFeature> flist = new List<IFeature>();
                        int maxIndex = -1;
                        double maxVal = 0;
                        while (true)
                        {
                            IFeature f = pFeatureSet.Next() as IFeature;
                            if (f == null)
                            {
                                break;
                            }

                            if (f.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                            {
                                if ((f.Shape as IPolyline).Length > maxVal)
                                {
                                    maxVal = (f.Shape as IPolyline).Length;
                                    maxIndex = flist.Count();
                                }
                            }
                            else
                            {
                                if ((f.Shape as IArea).Area > maxVal)
                                {
                                    maxVal = (f.Shape as IArea).Area;
                                    maxIndex = flist.Count();
                                }
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

                editor.StopOperation("分割");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                editor.AbortOperation();

                MessageBox.Show(ex.Message);
            }

            m_Application.MapControl.Map.ClearSelection();
            m_Application.ActiveView.Refresh();
        }

        public override bool Deactivate()
        {
            //卸掉该事件
            m_Application.MapControl.OnAfterScreenDraw -= new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);
            return base.Deactivate();
        }

        

    }
}
