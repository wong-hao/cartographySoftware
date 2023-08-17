using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.DCDProcess
{
    //点到线
    public class PointAdjust2LineCmd : SMGITool
    {
        INewEnvelopeFeedback feedback = null;

        

        public PointAdjust2LineCmd()
        {
            m_caption = "点到线";
            m_category = "整理工具";
            m_toolTip = "框选点1个，线1个或2个，点移动到2线交点或者1线的垂足";
            NeedSnap = false;
        }

        public override bool Enabled
        {
            get
            {
                if (m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    return true;
                }
                return false;
            }
        }

        public override void OnClick()
        {
            IWorkspace workspace = m_Application.Workspace.EsriWorkspace;
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button == 1)
            {
                feedback = new NewEnvelopeFeedbackClass();
                IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                feedback.Display = m_Application.ActiveView.ScreenDisplay;
                feedback.Start(currentMouseCoords);
            }
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (button == 1 && feedback != null)
            {
                IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                feedback.MoveTo(currentMouseCoords);
            }
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            if (button == 1 && feedback != null)
            {
                IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                IGeometry geo = feedback.Stop();
                feedback = null;
                var spfilter = new SpatialFilterClass
                {
                    Geometry = geo,
                    SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects,
                    WhereClause = cmdUpdateRecord.CurFeatureFilter
                };

                IList<IFeature> ptFeatureList = new List<IFeature>();
                IList<IFeature> plFeatureList = new List<IFeature>();

                //执行图面选择
                ISelectionEnvironment selEnv = new SelectionEnvironmentClass();
                selEnv.CombinationMethod = esriSelectionResultEnum.esriSelectionResultNew;
                IMap map = m_Application.ActiveView.FocusMap;
                map.SelectByShape(geo, selEnv, false);

                //统计点、线个数
                IEnumFeature selectEnumFeature = (map.FeatureSelection) as IEnumFeature;
                selectEnumFeature.Reset();
                IFeature fe = null;
                while ((fe = selectEnumFeature.Next()) != null)
                {
                    if (fe.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                        plFeatureList.Add(fe);
                    else if (fe.Shape.GeometryType == esriGeometryType.esriGeometryPoint)
                        ptFeatureList.Add(fe);

                    if (plFeatureList.Count > 2 || ptFeatureList.Count > 1)
                        break;
                }
                map.ClearSelection();

                //只处理单点
                if (ptFeatureList.Count != 1)
                    return;

                IFeature fePt = ptFeatureList[0];
                IFeatureClass fc = fePt.Class as IFeatureClass;
                if (fc == null)
                    return;
                
                IFeatureDataset fd = fc.FeatureDataset;
                //非开启开启编辑的要素，不处理
                if (fd.Workspace != m_Application.EngineEditor.EditWorkspace)
                    return;               

                if (plFeatureList.Count == 1) //单线，移动垂足的情况
                {                    
                    IFeature fePl = plFeatureList[0];
                    IPoint pt = fePt.ShapeCopy as IPoint;
                    IPolyline pl = fePl.ShapeCopy as IPolyline;
                    IPoint ptOut = new PointClass();
                    double distAlongPL=0;
                    double distFromPL=0;
                    bool side=false;
                    pl.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, pt, true, ptOut,ref distAlongPL,ref distFromPL,ref side);
                    
                    //垂足(可能)落在了延长线上，不处理
                    //if (distAlongPL <= 0.0 || distAlongPL >= 1.0) 
                    //    return;

                    //获取到了垂足，且垂足不同于输入点
                    if (distFromPL > 0.0 && ptOut != null) 
                    {
                        IPolyline plx = new PolylineClass();
                        plx.FromPoint = pt;
                        plx.ToPoint = ptOut;
                        m_Application.MapControl.FlashShape(plx,1,200,null);//闪烁

                        m_Application.EngineEditor.StartOperation();  
                        fePt.Shape = ptOut;
                        fePt.Store();
                        m_Application.EngineEditor.StopOperation("点移动到垂足");
                        m_Application.ActiveView.Refresh();                        
                    }
                }
                else if (plFeatureList.Count == 2) //移动到交点的情况
                {
                    IFeature fePl1 = plFeatureList[0];
                    IFeature fePl2 = plFeatureList[1];
                    IPolyline pl1 = fePl1.ShapeCopy as IPolyline;
                    IPolyline pl2 = fePl2.ShapeCopy as IPolyline;

                    ITopologicalOperator topoOpx = pl1 as ITopologicalOperator;
                    IGeometry geox = topoOpx.Intersect(pl2, esriGeometryDimension.esriGeometry0Dimension);
                    if (geox != null)
                    {
                        IGeometryCollection geoc = geox as IGeometryCollection;
                        if (geoc.GeometryCount > 1)
                        {
                            MessageBox.Show("线交点多于1个");
                            return;
                        }
                        IPoint ptX = geoc.get_Geometry(0) as IPoint;
                        IPolyline plx = new PolylineClass();
                        plx.FromPoint = fePt.ShapeCopy as IPoint;
                        plx.ToPoint = ptX;
                        if (plx.Length > 0.0)
                        {
                            m_Application.MapControl.FlashShape(plx, 1, 200, null);//闪烁
                            m_Application.EngineEditor.StartOperation();
                            fePt.Shape = ptX;
                            fePt.Store();
                            m_Application.EngineEditor.StopOperation("点移动到交点");
                            m_Application.ActiveView.Refresh();   
                        }                       
                    } 
                }                
            }
        }


    }
}
