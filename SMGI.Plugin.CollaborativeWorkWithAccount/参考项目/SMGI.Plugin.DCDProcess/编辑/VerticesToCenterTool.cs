using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;
using SMGI.Common;
using System.Windows.Forms;

namespace SMGI.Plugin.DCDProcess
{
    public class VerticesToCenterTool : SMGITool
    {
        private IFeatureClass _targetFcls;
        private IPoint _cenPt;
        private const double MaxRadius = 500; //米？
        private double _radius;
        private IFillSymbol _symbol;

        private INewCircleFeedback m_CircleFeedback;
        private bool m_IsSnapKeyDown = false;
 
        /// <summary>
        /// 节点到圆心
        /// </summary>
        public VerticesToCenterTool()
        {
           
            m_caption = "节点到圆心";
            //m_cursor =
            //    new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "Brush.cur"));
            m_category = "编辑工具";
            m_toolTip = "将选中线要素的端点移动到圆心";

            NeedSnap = true;

        }

        public override void OnClick()
        {
            IRgbColor pColor = new RgbColorClass();
            pColor.Red = 255;
            pColor.Green = 0;
            pColor.Blue = 0;
            pColor.Transparency = 255;

            ILineSymbol plineSym = new SimpleLineSymbolClass();
            plineSym.Width = 1;
            plineSym.Color = pColor;


            pColor = new RgbColorClass { Transparency = 0 };

            ISimpleFillSymbol pfsym = new SimpleFillSymbolClass();
            pfsym.Color = pColor;
            pfsym.Outline = plineSym;

            _symbol = pfsym;

        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button == 1) //左键有效
            {
                //获取圆心点
                _cenPt = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                //if (NeedSnap && m_IsSnapKeyDown)//若开启了圆心捕捉，且按下了(自定义)捕捉开关键
                if (m_IsSnapKeyDown)//若开启了圆心捕捉，且按下了(自定义)捕捉开关键
                {
                    //PixelSnap=5                
                    IPoint pointSnap = Snapping(m_Application.ActiveView, esriGeometryHitPartType.esriGeometryPartVertex, _cenPt, 5);
                    if (pointSnap != null)
                    {
                        _cenPt = pointSnap;
                        IMarkerElement markerElement = new MarkerElementClass();
                        ISimpleMarkerSymbol simpleMarkerSymbol = new SimpleMarkerSymbolClass();
                        IRgbColor color = new RgbColorClass();
                        color.Red = 200;
                        color.Green = 100;
                        color.Blue = 100;
                        simpleMarkerSymbol.Color = color as IColor;
                        simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSSquare;
                        markerElement.Symbol = simpleMarkerSymbol;

                        IElement element = markerElement as IElement;
                        element.Geometry = pointSnap;

                        IGraphicsContainer graphicsContainer = m_Application.ActiveView as IGraphicsContainer;
                        graphicsContainer.AddElement(element, 0);
                        m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                    }
                    else
                    {
                        IGraphicsContainer graphicsContainer = m_Application.ActiveView as IGraphicsContainer;
                        graphicsContainer.DeleteAllElements();
                        m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                    }
                }

                if (m_CircleFeedback == null)
                {
                    m_CircleFeedback = new NewCircleFeedbackClass();
                    m_CircleFeedback.Display = m_Application.ActiveView.ScreenDisplay;
                    m_CircleFeedback.Start(_cenPt);
                }
            }
            else //右键、滑轮不进行特殊操作
            {
                base.OnMouseDown(button, shift, x, y);
            }
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (_cenPt == null) return;

            if (m_CircleFeedback != null)
            {
                var curpt = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);  
                m_CircleFeedback.MoveTo(curpt);
                m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }            
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            
            if (_cenPt == null) return;

            //确定半径，空间查询
            var curpt = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            var r = (curpt as IProximityOperator).ReturnDistance(_cenPt);
            _radius = r > MaxRadius ? MaxRadius : r;

            var cir = new CircularArcClass() as IConstructCircularArc;
            cir.ConstructCircle(_cenPt, _radius, false);
            var plg = new PolygonClass();
            var segs = plg as ISegmentCollection;
            segs.AddSegment(cir as ISegment);

            var spfilter = new SpatialFilterClass
            {
                Geometry = plg,
                SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects
            };
            var pCur = _targetFcls.Search(spfilter, false);
            var fealist = new List<IFeature>();
            IFeature pFea;
            while (null != (pFea = pCur.NextFeature()))
            {
                fealist.Add(pFea);
            }
            if (m_CircleFeedback != null)
            {
                m_CircleFeedback.Stop();
                m_CircleFeedback = null;
            }

            var gct = m_Application.ActiveView.FocusMap as IGraphicsContainer;
            gct.DeleteAllElements();
            
            if (fealist.Count > 1){

                PointToCenter(fealist, _cenPt);                
            }

            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, null);
       
            _cenPt = null;
            
        }

        public override void OnKeyDown(int keyCode, int shift)
        {
           
            if ((int)Keys.ControlKey==keyCode)
            {
                m_IsSnapKeyDown = true;
                //this.Cursor = System.Windows.Forms.Cursors.Cross;
            }   
        }

        public override void OnKeyUp(int keyCode, int shift)
        {
            m_IsSnapKeyDown = false;
            //this.Cursor = System.Windows.Forms.Cursors.UpArrow;

            IGraphicsContainer graphicsContainer = m_Application.ActiveView as IGraphicsContainer;
            graphicsContainer.DeleteAllElements();
            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null); 
        }


        /// <summary>
        /// 当前图层为可编辑线图层有效
        /// </summary>
        public override bool Enabled
        {
            get
            {
                if (m_Application != null && m_Application.Workspace != null &&
                    m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    var lyr = m_Application.TOCSelectItem.Layer; ;
                    if (lyr == null)
                    {
                        _targetFcls = null;
                        return false;
                    }
                    var flyr = lyr as IFeatureLayer;
                    if (null != flyr && flyr.FeatureClass != null &&
                        flyr.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                    {
                        _targetFcls = flyr.FeatureClass;
                        return true;
                    }

                }
                _targetFcls = null;
                return false;

            }
        }

        public static IPoint Snapping(IActiveView activeView, esriGeometryHitPartType geometryHitPartType, IPoint queryPoint, int snapPix)
        {
            //获取MapUnit
            IDisplayTransformation DisplayTransformation = activeView.ScreenDisplay.DisplayTransformation;
            IPoint Point1 = DisplayTransformation.VisibleBounds.UpperLeft;
            IPoint Point2 = DisplayTransformation.VisibleBounds.UpperRight;
            int x1, x2, y1, y2;
            DisplayTransformation.FromMapPoint(Point1, out x1, out y1);
            DisplayTransformation.FromMapPoint(Point2, out x2, out y2);
            double pixelExtent = x2 - x1;
            double realWorldDisplayExtent = DisplayTransformation.VisibleBounds.Width;
            double mapUnit = realWorldDisplayExtent / pixelExtent;

            double searchRaius = snapPix * mapUnit;


            IPoint vetexPoint = null;
            IPoint hitPoint = new PointClass();
            IHitTest hitTest = null;
            IPointCollection pointCollection = new MultipointClass();
            IProximityOperator proximityOperator = null;
            double hitDistance = 0;
            int hitPartIndex = 0, hitSegmentIndex = 0;
            Boolean rightSide = false;
            IFeatureCache2 featureCache = new FeatureCacheClass();
            featureCache.Initialize(queryPoint, searchRaius);  //初始化缓存
            for (int i = 0; i < activeView.FocusMap.LayerCount; i++)
            {
                //只有点、线、面并且可视的图层才加入缓存
                IFeatureLayer featLayer = (IFeatureLayer)activeView.FocusMap.get_Layer(i);
                if (featLayer != null && featLayer.Visible == true &&
                    (featLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline ||
                    featLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon ||
                    featLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint))
                {
                    featureCache.AddFeatures(featLayer.FeatureClass, null);
                    for (int j = 0; j < featureCache.Count; j++)
                    {
                        IFeature feature = featureCache.get_Feature(j);
                        hitTest = (IHitTest)feature.Shape;
                        //捕捉节点，另外可以设置esriGeometryHitPartType，捕捉边线点，中间点等。
                        if (hitTest.HitTest(queryPoint, searchRaius, geometryHitPartType, hitPoint, ref hitDistance, ref hitPartIndex, ref hitSegmentIndex, ref rightSide))
                        {
                            object obj = Type.Missing;
                            pointCollection.AddPoint(hitPoint, ref obj, ref obj);
                            break;
                        }
                    }
                }
            }
            proximityOperator = (IProximityOperator)queryPoint;
            double minDistance = 0, distance = 0;
            for (int i = 0; i < pointCollection.PointCount; i++)
            {
                IPoint tmpPoint = pointCollection.get_Point(i);
                distance = proximityOperator.ReturnDistance(tmpPoint);
                if (i == 0)
                {
                    minDistance = distance;
                    vetexPoint = tmpPoint;
                }
                else
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        vetexPoint = tmpPoint;
                    }
                }
            }
            return vetexPoint;
        }

        /// <summary>
        /// 判断是否在圆内，找到最近节点，1起点，2终点 0不在内
        /// </summary>
        /// <param name="fea"></param>
        /// <param name="cpt"></param>
        /// <returns></returns>
        int Judge(IFeature fea, IPoint cpt)
        {
            var line = fea.Shape as IPolyline;
            var px = line.FromPoint as IProximityOperator;
            var res = 0;
            var d1 = px.ReturnDistance(cpt);
            if (d1 < _radius)
                res |= 1;
            px = line.ToPoint as IProximityOperator;
            var d2 = px.ReturnDistance(cpt);
            if (d2 < _radius)
                res |= 2;
            if (res == 3)//找最近
            {
                res = d1 < d2 ? 1 : 2;
            }

            return res;
        }

        bool PointToCenter(IList<IFeature> features, IPoint cpt)
        {
            m_Application.EngineEditor.StartOperation();
            try
            {
                //判断端点 是否在圆内
                var endsIdx = new Dictionary<int, int>();//数组索引 端点索引1 2 起点 终点

                for (var i = 0; i < features.Count; i++)
                {
                    var v = Judge(features[i], cpt);
                    if (v != 0)
                    {
                        endsIdx.Add(i, v);
                    }
                }

                if (endsIdx.Count <= 1)
                    return false;

                //更新校正
                foreach (var idx in endsIdx)
                {
                    var geom = features[idx.Key].ShapeCopy;

                    var line = geom as IPolyline;
                    if (idx.Value == 1) line.FromPoint = cpt;
                    else line.ToPoint = cpt;
                    features[idx.Key].Shape = line;
                    features[idx.Key].Store();
                }

                m_Application.EngineEditor.StopOperation("节点到圆心");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                m_Application.EngineEditor.AbortOperation();

                MessageBox.Show(ex.Message);
                return false;
            }

            return true;
        }

    }
}
