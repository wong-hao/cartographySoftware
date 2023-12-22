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
using System.Data;
using System.Xml.Linq;
using System.Xml;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public class FeatureCreateTool: SMGITool
    {
        // 参考SMGI.Plugin.GeneralEdit.FeatureEditTool
        private IEngineEditor m_engineEditor = null;
        private IFeatureLayer m_Featurelayer = null;

        private IEngineEditSketch m_editSketch = null;
        private int m_sketchVerticesCount = 0;

        private INewLineFeedback m_lineFeedback = null;
        private INewPolygonFeedback m_plyFeedback = null;

        private IGeometry m_lastGeo = null;

        private bool bEditSketchEvent = false;//本工具触发的新建要素事件标识（防止该工具激活时，其它命令间接触发的新建要素事件导致的属性赋值错误）

        /// <summary>
        /// 要素创建，考虑GB赋值（若可以获取该信息）
        /// </summary>
        public FeatureCreateTool()
        {
            m_caption = "创建要素";
            m_toolTip = "创建要素";
            m_category = "辅助";
        }

        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
        }

        public override void OnClick()
        {
            m_engineEditor = m_Application.EngineEditor;
            m_editSketch = m_engineEditor as IEngineEditSketch;

            if (m_lastGeo != null && m_editSketch.Geometry != m_lastGeo) 
            {
                m_Featurelayer = null;//IEngineEditSketch几何体已发生变化，初始化要素创建
            }

            (m_engineEditor as IEngineEditEvents_Event).OnCreateFeature += new IEngineEditEvents_OnCreateFeatureEventHandler(EngineEditEvent_Event_OnCreateFeature);
            m_Application.MapControl.OnAfterScreenDraw += new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);

            LoadAutoFillInfo();
        }

        public override bool Deactivate()
        {
            (m_engineEditor as IEngineEditEvents_Event).OnCreateFeature -= new IEngineEditEvents_OnCreateFeatureEventHandler(EngineEditEvent_Event_OnCreateFeature);
            m_Application.MapControl.OnAfterScreenDraw -= new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);

            m_lastGeo = m_editSketch.Geometry;

            return base.Deactivate();
        }

        public override bool OnContextMenu(int x, int y)
        {
            return base.OnContextMenu(x, y);
        }

        public override void OnKeyDown(int keyCode, int shift)
        {

        }

        public override void OnKeyUp(int keyCode, int shift)
        {

        }

        public override void OnDblClick()
        {
            if (null == m_Featurelayer || null == m_Featurelayer.FeatureClass)
                return;

            IGeometry geo = null;
            switch (m_Featurelayer.FeatureClass.ShapeType)
            {
                case esriGeometryType.esriGeometryPolyline:
                    {
                        if (m_lineFeedback != null)
                        {
                            m_lineFeedback.Stop();
                            m_lineFeedback = null;
                        }

                        try
                        {
                            bEditSketchEvent = true;
                            m_editSketch.FinishSketch();//通知EngineEditor的当前任务完成，触发相应事件（如在“创建要素任务”下，将创建一个新的要素）
                        }
                        catch
                        {
                        }

                        break;
                    }
                case esriGeometryType.esriGeometryPolygon:
                    {
                        if (m_plyFeedback != null)
                        {
                            m_plyFeedback.Stop();
                            m_plyFeedback = null;
                        }

                        try
                        {
                            bEditSketchEvent = true;
                            m_editSketch.FinishSketch();//通知EngineEditor的当前任务完成，并触发相应事件（如在EngineEditor的“创建要素任务”下，将创建一个新的要素）
                        }
                        catch
                        {
                        }

                        break;
                    }
            }
        }


        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
            {
                return;
            }

            var Lyr = m_Application.TOCSelectItem.Layer;
            if (!(Lyr is FeatureLayer))
            {
                MessageBox.Show("请选择要素层！");
                return;
            }
            IFeatureLayer pFeatureLayer = Lyr as IFeatureLayer;
            if (pFeatureLayer == null || pFeatureLayer.FeatureClass == null)
                return;
            IEngineEditLayers editLayer = m_engineEditor as IEngineEditLayers;
            if (!editLayer.IsEditable(pFeatureLayer))
            {
                MessageBox.Show("图层不可编辑！");
                return;
            }

            if (m_Featurelayer != pFeatureLayer)
            {
                m_Application.MapControl.Map.ClearSelection();

                m_Featurelayer = pFeatureLayer;
                editLayer.SetTargetLayer(pFeatureLayer, 0);
                IEngineEditTask edittask = m_engineEditor.GetTaskByUniqueName("ControlToolsEditing_CreateNewFeatureTask");
                m_engineEditor.CurrentTask = edittask;

                if (m_lineFeedback != null)
                {
                    m_lineFeedback.Stop();
                    m_lineFeedback.Refresh(m_Application.ActiveView.ScreenDisplay.hDC);
                    m_lineFeedback = null;
                }
                if (m_plyFeedback != null)
                {
                    m_plyFeedback.Stop();
                    m_plyFeedback.Refresh(m_Application.ActiveView.ScreenDisplay.hDC);
                    m_plyFeedback = null;
                }
            }

            IPoint pt = ToSnapedMapPoint(x, y);

            switch (m_Featurelayer.FeatureClass.ShapeType)
            {
                case esriGeometryType.esriGeometryPoint:
                {
                        bEditSketchEvent = true;
                        CreateFeature(pt as IGeometry);
                        break;
                    }
                case esriGeometryType.esriGeometryPolyline:
                    {
                        ISimpleRenderer render = (pFeatureLayer as IGeoFeatureLayer).Renderer as ISimpleRenderer;

                        if (null == m_lineFeedback)
                        {
                            m_editSketch.GeometryType = esriGeometryType.esriGeometryPolyline;
                            m_editSketch.Geometry = new PolylineClass();
                            m_sketchVerticesCount = 0;

                            //修改sketchsymbol
                            if (render != null && render.Symbol is ILineSymbol)
                            {
                                IEngineEditProperties editorProp = m_engineEditor as IEngineEditProperties;
                                editorProp.SketchSymbol = render.Symbol as ILineSymbol;
                            }

                        }
                        m_editSketch.AddPoint(pt, true);

                        if (m_lineFeedback != null)
                        {
                            m_lineFeedback.Stop();
                            m_lineFeedback = null;
                        }

                        m_lineFeedback = CreateNewLineFeedbackClass(render);
                        m_lineFeedback.Start(pt);

                        IPointCollection ptColl = m_editSketch.Geometry as IPointCollection;
                        m_sketchVerticesCount = ptColl.PointCount;

                        break;
                    }
                case esriGeometryType.esriGeometryPolygon:
                    {
                        ISimpleRenderer render = (pFeatureLayer as IGeoFeatureLayer).Renderer as ISimpleRenderer;

                        if (null == m_plyFeedback)
                        {
                            m_editSketch.GeometryType = esriGeometryType.esriGeometryPolygon;
                            m_editSketch.Geometry = new PolygonClass();
                            m_sketchVerticesCount = 0;

                            //修改sketchsymbol
                            if (render != null && render.Symbol is IFillSymbol)
                            {
                                IEngineEditProperties editorProp = m_engineEditor as IEngineEditProperties;
                                editorProp.SketchSymbol = (render.Symbol as IFillSymbol).Outline;
                            }
                        }
                        m_editSketch.AddPoint(pt, true);

                        if (m_plyFeedback != null)
                        {
                            m_plyFeedback.Stop();
                            m_plyFeedback = null;
                        }

                        m_plyFeedback = CreateNewPolygonFeedbackClass();

                        IPointCollection ptColl = m_editSketch.Geometry as IPointCollection;
                        if (0 == m_sketchVerticesCount)
                        {
                            m_plyFeedback.Start(pt);
                        }
                        else
                        {
                            m_plyFeedback.Start(ptColl.get_Point(0));
                            m_plyFeedback.AddPoint(pt);
                        }
                        m_sketchVerticesCount = ptColl.PointCount;

                        break;
                    }
            }

        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (null == m_Featurelayer || null == m_Featurelayer.FeatureClass)
                return;

            IPointCollection ptColl = m_editSketch.Geometry as IPointCollection;
            if (null == ptColl)
                return;

            IPoint pt = ToSnapedMapPoint(x, y);

            switch (m_Featurelayer.FeatureClass.ShapeType)
            {
                case esriGeometryType.esriGeometryPolyline:
                    {
                        if (ptColl.PointCount != m_sketchVerticesCount)//撤销或回退
                        {
                            m_sketchVerticesCount = ptColl.PointCount;

                            if (m_lineFeedback != null)
                            {
                                m_lineFeedback.Refresh(m_Application.ActiveView.ScreenDisplay.hDC);
                                m_lineFeedback.Stop();
                                m_lineFeedback = null;
                            }

                            if (m_sketchVerticesCount > 0)
                            {
                                m_lineFeedback = CreateNewLineFeedbackClass((m_Featurelayer as IGeoFeatureLayer).Renderer as ISimpleRenderer);
                                m_lineFeedback.Start(m_editSketch.LastPoint);
                            }
                        }
                        break;
                    }
                case esriGeometryType.esriGeometryPolygon:
                    {
                        if (ptColl.PointCount != m_sketchVerticesCount)//撤销或回退
                        {
                            m_sketchVerticesCount = ptColl.PointCount;

                            if (m_plyFeedback != null)
                            {
                                m_plyFeedback.Refresh(m_Application.ActiveView.ScreenDisplay.hDC);
                                m_plyFeedback.Stop();
                                m_plyFeedback = null;
                            }

                            if (m_sketchVerticesCount > 2)
                            {
                                m_plyFeedback = CreateNewPolygonFeedbackClass((m_Featurelayer as IGeoFeatureLayer).Renderer as ISimpleRenderer);
                                m_plyFeedback.Start(ptColl.get_Point(0));
                                m_plyFeedback.AddPoint(m_editSketch.LastPoint);
                            }
                            else if (m_sketchVerticesCount > 1)
                            {
                                m_plyFeedback = CreateNewPolygonFeedbackClass();
                                m_plyFeedback.Start(ptColl.get_Point(0));
                            }
                        }
                        break;
                    }
            }

            if (m_lineFeedback != null)
            {
                m_lineFeedback.MoveTo(pt);
            }

            if (m_plyFeedback != null)
            {
                m_plyFeedback.MoveTo(pt);
            }
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {

        }

        public override void Refresh(int hdc)
        {
            if (m_lineFeedback != null)
            {
                m_lineFeedback.Refresh(hdc);
            }
            if (m_plyFeedback != null)
            {
                m_plyFeedback.Refresh(hdc);
            }
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

        private void EngineEditEvent_Event_OnCreateFeature(IObject Object)
        {
            if (!bEditSketchEvent)
                return;

            try
            {
                IFeature pFeature = Object as IFeature;

                //属性赋值
                ILayer layer = m_Application.TOCSelectItem.Layer;
                ILegendGroup group = m_Application.TOCSelectItem.Group;
                ILegendClass cls = m_Application.TOCSelectItem.Class;

                string layerName = layer.Name;
                string heading = group.Heading;
                string labelName = cls.Label;

                int headingIndex = pFeature.Fields.FindField(heading);
                if (headingIndex != -1)//根据选中的符号，先自动尝试对相应的属性字段赋值
                {
                    pFeature.set_Value(headingIndex, labelName);
                }

                if (autoFillInfo.Keys.Contains(layerName))
                {
                    if (autoFillInfo[layerName].Keys.Contains(labelName))
                    {
                        foreach (string fieldName in autoFillInfo[layerName][labelName].Keys)
                        {
                            string content = autoFillInfo[layerName][labelName][fieldName];
                            int index = pFeature.Fields.FindField(fieldName);
                            if (index != -1)
                            {
                                pFeature.set_Value(index, content);
                            }
                        }
                    }
                }

                pFeature.Store();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("属性赋值失败");
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }
            finally
            {
                bEditSketchEvent = false;
            }

        }

        private void MapControl_OnAfterScreenDraw(object sender, IMapControlEvents2_OnAfterScreenDrawEvent e)
        {
            if (m_lineFeedback != null)
            {
                m_lineFeedback.Refresh(m_Application.ActiveView.ScreenDisplay.hDC);
            }
            if (m_plyFeedback != null)
            {
                m_plyFeedback.Refresh(m_Application.ActiveView.ScreenDisplay.hDC);
            }
        }

        private void CreateFeature(IGeometry geo)
        {
            try
            {
                if (m_Featurelayer == null || m_Featurelayer.FeatureClass == null)
                {
                    return;
                }

                m_Application.MapControl.Map.ClearSelection();

                ITopologicalOperator pTop = geo as ITopologicalOperator;
                pTop.Simplify();
                IGeoDataset pGeoDataset = m_Featurelayer.FeatureClass as IGeoDataset;
                if (pGeoDataset.SpatialReference != null)
                {
                    geo.Project(pGeoDataset.SpatialReference);
                }

                m_engineEditor.StartOperation();

                IFeature pFeature = m_Featurelayer.FeatureClass.CreateFeature();
                pFeature.Shape = geo;
                pFeature.Store();

                m_engineEditor.StopOperation("新建要素");

                m_Application.MapControl.Map.SelectFeature(m_Featurelayer, pFeature);

                IEnvelope pExtendEnvelop = pFeature.Shape.Envelope;
                pExtendEnvelop.Expand(20, 20, false);

                m_Application.ActiveView.Refresh();
            }
            catch (Exception ex)
            {

            }
        }

        private INewLineFeedback CreateNewLineFeedbackClass(ISimpleRenderer render = null)
        {
            INewLineFeedback lineFeedback = new NewLineFeedbackClass() { Display = m_Application.ActiveView.ScreenDisplay };

            if (render != null && render.Symbol != null && render.Symbol is ISimpleLineSymbol)
            {
                var lineSymbol = render.Symbol as ISimpleLineSymbol;

                var symbol = new SimpleLineSymbolClass();
                symbol.Color = lineSymbol.Color;
                symbol.Style = lineSymbol.Style;
                symbol.Width = lineSymbol.Width;
                symbol.ROP2 = esriRasterOpCode.esriROPNotXOrPen;

                lineFeedback.Symbol = symbol as ISymbol;
            }

            return lineFeedback;
        }

        private INewPolygonFeedback CreateNewPolygonFeedbackClass(ISimpleRenderer render = null)
        {
            INewPolygonFeedback plyFeedback = new NewPolygonFeedbackClass() { Display = m_Application.ActiveView.ScreenDisplay };

            if (render != null && render.Symbol != null && render.Symbol is ISimpleFillSymbol)
            {
                var fillSymbol = render.Symbol as ISimpleFillSymbol;

                var symbol = new SimpleFillSymbolClass();
                symbol.Color = fillSymbol.Color;
                symbol.Outline = fillSymbol.Outline;
                symbol.Style = esriSimpleFillStyle.esriSFSDiagonalCross;
                symbol.ROP2 = esriRasterOpCode.esriROPNotXOrPen;

                plyFeedback.Symbol = symbol as ISymbol;
            }

            return plyFeedback;
        }

        private string autoFillPath = "AutoFill.xml";
        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> autoFillInfo = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        private void LoadAutoFillInfo() 
        {
            string cfgFileName = m_Application.Template.Root + "\\" + autoFillPath;
            if (!System.IO.File.Exists(cfgFileName))
                return;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(cfgFileName);
            XmlNodeList nodes = xmlDoc.SelectNodes("/AutoFill/AutoFillBackground/Layer");

            foreach (XmlNode Layer in nodes)
            {
                if (Layer.NodeType != XmlNodeType.Element)
                    continue;
                string layerName = (Layer as XmlElement).GetAttribute("name");
                string layerHeading = (Layer as XmlElement).GetAttribute("heading");

                if (!autoFillInfo.Keys.Contains(layerName)) 
                {
                    autoFillInfo[layerName] = new Dictionary<string, Dictionary<string, string>>();
                }

                foreach (XmlNode Label in Layer)
                {
                    string labelName = (Label as XmlElement).GetAttribute("name");
                    if (!autoFillInfo[layerName].Keys.Contains(labelName))
                    {
                        autoFillInfo[layerName][labelName] = new Dictionary<string, string>();
                    }

                    foreach (XmlNode Field in Label)
                    {
                        string fieldName = (Field as XmlElement).GetAttribute("name");
                        string content = (Field as XmlElement).GetAttribute("content");
                        autoFillInfo[layerName][labelName][fieldName] = content;
                    }
                }
            }
        }
    }
}
