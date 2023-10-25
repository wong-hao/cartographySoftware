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
    public class XFCLAdjust2XXXACmd : SMGITool
    {
        INewEnvelopeFeedback feedback = null;
        IFeature feL = null;
        IFeature feA = null;
        IPoint pt1 = null;
        IPoint pt2 = null;

        public XFCLAdjust2XXXACmd()
        {
            m_caption = "线设施套面";
            m_category = "整理工具";
            m_toolTip = "框选线状设施1条，就近设施套合到面上";
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
                if (feL == null || feA == null)
                {
                    feedback = new NewEnvelopeFeedbackClass();
                    IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                    feedback.Display = m_Application.ActiveView.ScreenDisplay;
                    feedback.Start(currentMouseCoords);
                }
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
            if (button == 1)
            {
                if (feedback != null)
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

                    //执行图面选择
                    ISelectionEnvironment selEnv = new SelectionEnvironmentClass();
                    selEnv.CombinationMethod = esriSelectionResultEnum.esriSelectionResultNew;
                    IMap map = m_Application.ActiveView.FocusMap;
                    map.SelectByShape(geo, selEnv, false);

                    //待处理要素归类
                    List<IFeature> feLlist = new List<IFeature>();
                    List<IFeature> feAlist = new List<IFeature>();
                    #region 待处理要素归类
                    IEnumFeature selectEnumFeature = (map.FeatureSelection) as IEnumFeature;
                    selectEnumFeature.Reset();
                    IFeature fe = null;
                    while ((fe = selectEnumFeature.Next()) != null)
                    {
                        IFeatureClass fc = fe.Class as IFeatureClass;
                        string name = (fc as IDataset).Name;
                        IFeatureDataset fd = fc.FeatureDataset;
                        if (fd.Workspace != m_Application.EngineEditor.EditWorkspace)//非编辑要素，不处理
                            continue;
                        if (fc.ShapeType == esriGeometryType.esriGeometryPolyline)
                        {
                            if (new List<string>() { "HFCL", "LFCL" }.Contains(name.ToUpper()))//限定图层
                            {
                                feLlist.Add(fe);                           
                            }
                        }
                        else if (fc.ShapeType == esriGeometryType.esriGeometryPolygon)
                        {
                            if (new List<string>() { "HYDA","HFCA" }.Contains(name.ToUpper()))//限定图层，去掉则不限定
                            {
                                feAlist.Add(fe);                            
                            }
                        }
                    }
                    if(feAlist.Count!=1 ||feLlist.Count!=1)
                    {
                        feL = null;
                        feA = null;
                        MessageBox.Show("选线面个数不对"); 
                    }
                    else
                    {
                        feL = feLlist[0];
                        feA = feAlist[0];
                        m_Application.MapControl.FlashShape(feL.ShapeCopy,1,200,null);
                        //MessageBox.Show("选线面成功");
                        this.m_cursor = System.Windows.Forms.Cursors.Cross;
                    }                    
                    map.ClearSelection();
                    m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
                    #endregion
                }

                else if (pt1 == null)
                {
                    pt1 = this.ToSnapedMapPoint(x, y);
                    
                    if (pt1 != null)
                    {
                        m_Application.MapControl.FlashShape(pt1, 1, 200, null);
                        //MessageBox.Show("选点1");
                    }
                }
                else if (pt2 == null)
                {
                    pt2 = this.ToSnapedMapPoint(x, y);
                    if (pt2 != null)
                    {
                        this.m_cursor = System.Windows.Forms.Cursors.Default;
                        m_Application.MapControl.FlashShape(pt2, 1, 200, null);
                        //MessageBox.Show("选点2");

                        IPolyline pl = feL.ShapeCopy as IPolyline;
                        IPoint fromPt = pl.FromPoint;
                        IPoint toPt = pl.ToPoint;
                        IProximityOperator proxOper1 = pt1 as IProximityOperator;
                        double dist1from = proxOper1.ReturnDistance(fromPt);
                        double dist1to = proxOper1.ReturnDistance(toPt);

                        IProximityOperator proxOper2 = pt2 as IProximityOperator;
                        double dist2from = proxOper2.ReturnDistance(fromPt);
                        double dist2to = proxOper2.ReturnDistance(toPt);

                        if((dist1from<dist2from&& dist1to<dist2to)||(dist1from>dist2from&& dist1to>dist2to))
                        {
                            MessageBox.Show("点击位置不对");
                            SetNull();                            
                            return;
                        }
                        IPoint fromPtx = null;
                        IPoint toPtx = null;
                        if (dist1from < dist2from)
                        {
                            fromPtx = pt1;
                            toPtx = pt2;
                        }
                        else
                        {
                            fromPtx = pt2;
                            toPtx = pt1;
                        }

                        bool splitH1, splitH2;
                        int newPartIndex1, newPartIndex2;
                        int newSegmentIndex1, newSegmentIndex2;
                        pl.SplitAtPoint(fromPtx, true, true, out splitH1, out newPartIndex1, out newSegmentIndex1);
                        pl.SplitAtPoint(toPtx, true, true, out splitH2, out newPartIndex2, out newSegmentIndex2);

                        ITopologicalOperator topoOpx = feA.ShapeCopy as ITopologicalOperator; //面的边线
                        IPolyline a2l = topoOpx.Boundary as IPolyline;

                        var gc = pl as IGeometryCollection; //设施线，被打断（1/2/3段）
                        IPath pth = null; //线段的中间部分，靠坝
                        if (!splitH1)
                            pth = gc.Geometry[0] as IPath;
                        else
                            pth = gc.Geometry[1] as IPath;

                        IPath pthA2L = null;
                        //对面边线a2l进行分割，利用pth的两个投影点，默认分为3部分
                        {
                            IPoint pt1x = pth.FromPoint;
                            IPoint pt2x = pth.ToPoint;
                            bool splitH1x, splitH2x;
                            int newPartIndex1x, newPartIndex2x;
                            int newSegmentIndex1x, newSegmentIndex2x;
                            a2l.SplitAtPoint(pt1x, true, true, out splitH1x, out newPartIndex1x, out newSegmentIndex1x);
                            a2l.SplitAtPoint(pt2x, true, true, out splitH2x, out newPartIndex2x, out newSegmentIndex2x);
                            var gc_a2l = a2l as IGeometryCollection;
                            pthA2L = gc_a2l.Geometry[1] as IPath;
                        }

                        PolylineClass plNewC = new PolylineClass();
                        var gc2 = plNewC as IGeometryCollection;
                        if (splitH1)
                        {
                            IPath pth0 = gc.Geometry[0] as IPath;
                            gc2.AddGeometry(pth0);
                            IPath pthx = new PathClass();
                            pthx.FromPoint = pth0.ToPoint;
                            pthx.ToPoint = pthA2L.ToPoint;
                            gc2.AddGeometry(pthx);
                        }
                        gc2.AddGeometry(pthA2L);//中间部分
                        if (splitH2)
                        {                               
                            IPath pth2 = gc.Geometry[2] as IPath; //末尾线
                            IPath pthx = new PathClass();
                            pthx.FromPoint = pthA2L.FromPoint;
                            pthx.ToPoint = pth2.FromPoint;
                            gc2.AddGeometry(pthx);                            
                            gc2.AddGeometry(pth2);
                        }

                        IPolyline plNew = plNewC as IPolyline;
                        m_Application.MapControl.FlashShape(plNew, 1, 800, null);                        
                        m_Application.EngineEditor.StartOperation();
                        ITopologicalOperator topoopr = plNew as ITopologicalOperator;
                        topoopr.Simplify();
                        feL.Shape = topoopr as IGeometry;
                        feL.Store();
                        m_Application.EngineEditor.StopOperation("修改设施");
                        
                        m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
                    
                        SetNull();
                        return;
                    }
                }
                else //不可能的情况，pt1/pt2/feL/feA都不为空
                {
                    SetNull();
                    return;
                }
            }
            else //非左键，取消
            {
                SetNull();
                return;               
            }
        }

        public IPoint SnapToPolyline(IPoint pt, IPolyline pl)
        {
            IHitTest hitTest = pl as IHitTest;
            IPoint hitPt = new PointClass();
            double hitDist = 0.0;
            int partIndex =0;
            int vertexIndex=0;
            bool bVertexHit = false;
            IEngineEditProperties2 _editorProp = GApplication.Application.EngineEditor as IEngineEditProperties2;
            double tol = _editorProp.StickyMoveTolerance; 

            if(hitTest.HitTest(pt,tol,esriGeometryHitPartType.esriGeometryPartVertex, hitPt,ref hitDist,partIndex,vertexIndex,bVertexHit))
            {
                IPoint ptx = hitPt;
            }
            return pt;
        }

        public void SetNull()
        {
            pt1 = null;
            pt2 = null;
            feL = null;
            feA = null;
            this.m_cursor = System.Windows.Forms.Cursors.Default;
        }
    }
}
