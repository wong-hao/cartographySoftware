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
    public class EndsAdjustTool : SMGITool
    {
        private IFeatureClass _targetFcls;
        private IPoint _cenPt;
        private const double MaxRadius = 500; //米？
        private double _radius;
        private IFillSymbol _symbol;
 
        /// <summary>
        /// 节点平差
        /// </summary>
        public EndsAdjustTool()
        {
           
            m_caption = "节点平差";
            //m_cursor =
            //    new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "Brush.cur"));
            m_category = "编辑工具";
            m_toolTip = "将靠近的线要素端点聚在一起";

            NeedSnap = false;

        }

        public override void OnClick()
        {
            // var cir = new CircularArcClass() as IConstructCircularArc;
            //cir.ConstructCircle();


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
            //获取圆心点
            _cenPt = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

          //var cir=  m_Application.MapControl.TrackCircle();
            Console.WriteLine("Down");
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (_cenPt == null) return;
            //   base.OnMouseMove(button, shift, x, y);
            //获取半径 并画圆
            var actView = m_Application.ActiveView;
            var curpt = actView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            var r = (curpt as IProximityOperator).ReturnDistance(_cenPt);
            _radius = r > MaxRadius ? MaxRadius : r;
            var cir = new CircularArcClass() as IConstructCircularArc;
            cir.ConstructCircle(_cenPt, _radius, false);

            var plg = new PolygonClass();
            var segs = plg as ISegmentCollection;segs.AddSegment(cir as ISegment);

            var cirEle = new CircleElementClass() as IElement;
            cirEle.Geometry = plg;
            (cirEle as IFillShapeElement).Symbol = _symbol;

            var gcontainer = actView.FocusMap as IGraphicsContainer;
            gcontainer.DeleteAllElements();
            gcontainer.AddElement(cirEle, 0);
           
             actView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null,null);
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            
            if (_cenPt == null) return;//base.OnMouseUp(button, shift, x, y);
            //确定半径，空间查询，处理... 结束工具

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
            var actView = m_Application.ActiveView;
            var gct = actView.FocusMap as IGraphicsContainer;
            gct.DeleteAllElements();
           // actView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            
            if (fealist.Count > 1){
         
               AdjustToMidPoint(fealist);
                //if (b)
                //    actView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, env);
            }

            actView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, null);
       
            _cenPt = null;//var mappan = new ControlsMapPanToolClass();
            //mappan.OnCreate(m_Application.MapControl.Object);
            //mappan.OnClick();
            //(this as ITool).Deactivate();
        }

        /// <summary>
        /// 判断是否在圆内，找到最近节点，1起点，2终点 0不在内
        /// </summary>
        /// <param name="fea"></param>
        /// <param name="cpt"></param>
        /// <returns></returns>
        int Judge(IFeature fea, out IPoint cpt)
        {
            var line = fea.Shape as IPolyline;
            var px = line.FromPoint as IProximityOperator;
            var res = 0;
            var d1 = px.ReturnDistance(_cenPt);
            if (d1 < _radius)
                res |= 1;
            px = line.ToPoint as IProximityOperator;
            var d2 = px.ReturnDistance(_cenPt);
            if (d2 < _radius)
                res |= 2;
            if (res == 3)//找最近
            {
                res = d1 < d2 ? 1 : 2;
            }
            if (res == 0) cpt = null;
            else
                cpt = res == 1 ? line.FromPoint : line.ToPoint;
            return res;
        }

        bool AdjustToMidPoint(IList<IFeature> features)
        {
            m_Application.EngineEditor.StartOperation();
            try
            {
                //判断端点 是否在圆内
                var endsIdx = new Dictionary<int, int>();//数组索引 端点索引1 2 起点 终点

                double midx = 0;
                double midy = 0;
                for (var i = 0; i < features.Count; i++)
                {
                    IPoint pt;
                    var v = Judge(features[i], out pt);
                    if (v != 0)
                    {
                        endsIdx.Add(i, v);
                        midx += pt.X;
                        midy += pt.Y;
                    }
                }

                if (endsIdx.Count < 1)
                    return false;

                //算中点
                var cpt = new PointClass { X = midx / endsIdx.Count, Y = midy / endsIdx.Count };

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

                m_Application.EngineEditor.StopOperation("节点平差");
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
    }
}
