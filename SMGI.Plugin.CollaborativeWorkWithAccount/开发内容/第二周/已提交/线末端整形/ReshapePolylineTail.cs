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
    public class ReshapePolylineTail:SMGITool
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

        bool panD = false;
        public ReshapePolylineTail()
        {
            m_caption = "修线";
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "修线.cur"));
            m_toolTip = "修线工具";
            m_category = "基础编辑";
          
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
            if (Button != 1)
                return;
            if (lineFeedback == null)
            {
                var dis = m_Application.ActiveView.ScreenDisplay;
                lineFeedback = new NewLineFeedbackClass { Display = dis, Symbol = lineSymbol as ISymbol };
                lineFeedback.Start(ToSnapedMapPoint(x, y));
            }
            else
            {
                lineFeedback.AddPoint(ToSnapedMapPoint(x, y));
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
                IPolyline editShape = feature.ShapeCopy as IPolyline;
                ITopologicalOperator topoOpx = editShape as ITopologicalOperator;
                IGeometry geox = topoOpx.Intersect(polyline, esriGeometryDimension.esriGeometry0Dimension);
                if (geox != null)
                {
                    IGeometryCollection geoc = geox as IGeometryCollection;
                    if (geoc.GeometryCount == 1) //只能有1个交点
                    {
                        IPoint ptX = geoc.get_Geometry(0) as IPoint;
                        //用设施的起始点，打断道路线（点不在线上，则投影）
                        bool splitH1;
                        int newPartIndex1;
                        int newSegmentIndex1;
                        editShape.SplitAtPoint(ptX, true, true, out splitH1, out newPartIndex1, out newSegmentIndex1);
                        var gc = editShape as IGeometryCollection;
                        IPath shp1Max = null;
                        if (splitH1)
                        {                            
                            IPath gc0 = gc.Geometry[0] as IPath;
                            IPath gc1 = gc.Geometry[1] as IPath;
                            if (gc0.Length > gc1.Length)
                                shp1Max = gc0;
                            else
                                shp1Max = gc1;
                        }
                        else
                            shp1Max = gc.Geometry[0] as IPath;

                        bool splitH2;
                        int newPartIndex2;
                        int newSegmentIndex2;
                        polyline.SplitAtPoint(ptX, true, true, out splitH2, out newPartIndex2, out newSegmentIndex2);
                        gc = polyline as IGeometryCollection;
                        IPath shp2Max = null;
                        if (splitH2)
                        {
                            IPath gc0 = gc.Geometry[0] as IPath;
                            IPath gc1 = gc.Geometry[1] as IPath;
                            if (gc0.Length > gc1.Length)
                                shp2Max = gc0;
                            else
                                shp2Max = gc1;
                        }
                        else
                            shp2Max = gc.Geometry[0] as IPath;
                        if (shp1Max != null && shp2Max != null)
                        {
                            IPoint pt1to = shp1Max.ToPoint;
                            IPoint pt1from = shp1Max.FromPoint;
                            IPoint pt2from = shp2Max.FromPoint;
                            IPoint pt2to = shp2Max.ToPoint;
                            IProximityOperator proxOper = ptX as IProximityOperator;
                            double dist1to = proxOper.ReturnDistance(pt1to);
                            double dist1from = proxOper.ReturnDistance(pt1from);
                            double dist2to = proxOper.ReturnDistance(pt2to);
                            double dist2from = proxOper.ReturnDistance(pt2from);
                            if((dist1to<0.001 && dist2to<0.001)||(dist1from<0.001 && dist2from<0.001))//反向处理
                                shp2Max.ReverseOrientation();
                            PolylineClass plNewC = new PolylineClass();
                            var gcx = plNewC as IGeometryCollection;
                            gcx.AddGeometry(shp1Max);
                            gcx.AddGeometry(shp2Max);
                            IPolyline plNew = plNewC as IPolyline;                           
                            ITopologicalOperator topoopr = plNew as ITopologicalOperator;
                            topoopr.Simplify();
                            feature.Shape = topoopr as IGeometry;
                            feature.Store();                           
                        }                        
                    }
                }


                #region 平移(?)
                if (panD == true)
                {
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
                }
                #endregion
                editor.StopOperation("修线");

                m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);

                ClearSnapperCache();
            }
            catch (Exception ex)
            {
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
                        if (feature.Shape is IPolyline)
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
       
    }
}
