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
using ESRI.ArcGIS.SystemUI;
using SMGI.Common;
using System.Runtime.InteropServices;
namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 多点联动工具
    /// </summary>
    public class MultiPointLinkageTool : SMGITool
    {
        private ControlsEditingEditToolClass _editTool;
        private IPoint _curPoint;
        private bool _bEditVertices;//是否在编辑要素
        private IFeature _selFeature;//被选中要素
        private Dictionary<int, IPoint> _selFeatureVertexs;//被选中要素的节点集合  

        
        public MultiPointLinkageTool()
        {
            m_caption = "多点联动";

            _editTool = new ControlsEditingEditToolClass();
            _curPoint = null;
            _bEditVertices = false;
            _selFeature = null;
            _selFeatureVertexs = new Dictionary<int, IPoint>();

            NeedSnap = false;
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null &&
                    m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }


        public override void setApplication(GApplication app)
        {
            base.setApplication(app);

            _editTool.OnCreate(m_Application.MapControl.Object);

        }


        public override void OnClick()
        {
            _editTool.OnClick();


            (m_Application.EngineEditor as IEngineEditEvents_Event).OnCurrentTaskChanged += new IEngineEditEvents_OnCurrentTaskChangedEventHandler(EngineEditor_OnCurrentTaskChanged);
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnVertexMoved += new IEngineEditEvents_OnVertexMovedEventHandler(EngineEditor_OnVertexMoved);

        }

        public override bool Deactivate()
        {
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnCurrentTaskChanged -= new IEngineEditEvents_OnCurrentTaskChangedEventHandler(EngineEditor_OnCurrentTaskChanged);
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnVertexMoved -= new IEngineEditEvents_OnVertexMovedEventHandler(EngineEditor_OnVertexMoved);

            return _editTool.Deactivate();
        }

        public override int Cursor
        {
            get
            {
                return _editTool.Cursor;
            }

        }

        public override bool OnContextMenu(int x, int y)
        {
            return _editTool.OnContextMenu(x, y);
        }

        public override void OnDblClick()
        {
            _editTool.OnDblClick();

        }


        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            _editTool.OnMouseDown(button, shift, x, y);


            if (2 == button)//右键
            {
                NeedSnap = true;
                ModifyFeature(_selFeature, _curPoint);
                NeedSnap = false;
            }

            if (_bEditVertices && 1 == button)
            {
                _selFeatureVertexs = new Dictionary<int, IPoint>();

                IEnumFeature enumFeature = m_Application.EngineEditor.EditSelection;

                IFeature selectFeature = enumFeature.Next();
                if (selectFeature != null && selectFeature.Shape != null &&  selectFeature.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                {
                    _selFeature = selectFeature;

                    Polyline pp = selectFeature.Shape as Polyline;
                    IPointCollection ptcoll = selectFeature.Shape as IPointCollection;
                    if (ptcoll != null)
                    {
                        for (int i = 0; i < ptcoll.PointCount; ++i)
                        {
                            _selFeatureVertexs.Add(i, ptcoll.get_Point(i));

                        }
                    }

                }
            }
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            _editTool.OnMouseMove(button, shift, x, y);

            if (1 == button)
            {
                if (_bEditVertices)
                {
                    NeedSnap = true;
                    _curPoint = ToSnapedMapPoint(x, y);
                }
            }
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            _editTool.OnMouseUp(button, shift, x, y);

            NeedSnap = false;
        }

        public override void Refresh(int hdc)
        {
            _editTool.Refresh(hdc);
        }

        void EngineEditor_OnCurrentTaskChanged()
        {
            if (m_Application.EngineEditor.CurrentTask.UniqueName == "ControlToolsEditing_ModifyFeatureTask")
            {
                _bEditVertices = true;
            }
            else
            {
                _bEditVertices = false;
            }

        }
        void EngineEditor_OnVertexMoved<T>(T param)
        {
            IPoint pt = param as IPoint;
            IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;

            int index = -1;
            IPointCollection ptcoll = editsketch.Geometry as IPointCollection;
            for (int i = 0; i < ptcoll.PointCount; ++i)
            {
                if (0 == pt.Compare(ptcoll.get_Point(i)))
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                int x, y;
                m_Application.ActiveView.ScreenDisplay.DisplayTransformation.FromMapPoint(pt, out x, out y);

                IPoint snapPoint = ToSnapedMapPoint(x, y);
                ptcoll.UpdatePoint(index, snapPoint);//修正坐标
            }
        }

        void ModifyFeature(IFeature selFe, IPoint p)
        {
            IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;

            int index = -1;
            IPointCollection ptcoll = editsketch.Geometry as IPointCollection;
            for (int i = 0; i < ptcoll.PointCount; ++i)
            {
                if (0 == p.Compare(ptcoll.get_Point(i)))
                {
                    index = i;
                    break;
                }
            }
            if (index == -1) 
            { 
                return; 
            }


            IPoint oldPoint = _selFeatureVertexs[index];

            
            ISpatialFilter sf = new SpatialFilterClass();
            IGeometry bufferGeo = (oldPoint as ITopologicalOperator).Buffer(0.00000009);
            sf.Geometry = bufferGeo;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l.Visible && l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline;

            })).ToArray();

            IEngineEditLayers editLayer = m_Application.EngineEditor as IEngineEditLayers;
            foreach (var item in lyrs)
            {
                if (!editLayer.IsEditable(item as IFeatureLayer))
                    continue;

                int modfiycount = 0;

                IFeatureCursor feCursor = (item as IFeatureLayer).Search(sf, false);
                IFeature connFe = null;
                while ((connFe = feCursor.NextFeature()) != null)
                {
                    //if (connFe.Class.ObjectClassID == selFe.Class.ObjectClassID && connFe.OID == selFe.OID)
                    //{
                    //    System.Runtime.InteropServices.Marshal.ReleaseComObject(connFe);
                    //    modfiycount++;
                    //    continue;
                    //}

                    #region 联动关联要素
                    bool bModify = false;
                    IPointCollection connPtColl = connFe.ShapeCopy as IPointCollection;
                    for (int i = 0; i < connPtColl.PointCount; ++i)
                    {
                        if (0 == connPtColl.get_Point(i).Compare(oldPoint))
                        {
                            int x1, y1;
                            m_Application.ActiveView.ScreenDisplay.DisplayTransformation.FromMapPoint(p, out x1, out y1);

                            IPoint snapPoint = ToSnapedMapPoint(x1, y1);

                            connPtColl.UpdatePoint(i, snapPoint);

                            bModify = true;
                            break;
                        }

                    }

                    if (!bModify)
                        continue;

                    try
                    {
                        m_Application.EngineEditor.StartOperation();

                        connFe.Shape = connPtColl as IPolyline;
                        connFe.Store();

                        m_Application.EngineEditor.StopOperation("多点联动");

                        modfiycount++;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.Message);
                        System.Diagnostics.Trace.WriteLine(ex.Source);
                        System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                        m_Application.EngineEditor.AbortOperation();
                    }
                    #endregion

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(connFe);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);

                if (modfiycount > 0)
                {
                    m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, (object)item, m_Application.ActiveView.Extent);
                }
            }            
        }
        
        
    }
}