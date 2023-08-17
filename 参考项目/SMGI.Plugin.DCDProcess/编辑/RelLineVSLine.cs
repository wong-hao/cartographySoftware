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
    public class RelLineVSLine: SMGITool
    {
        private int m_Step = 0;
        private ControlsEditingEditToolClass currentTool;
        private IPolyline m_BaseLine;



        public RelLineVSLine()
        {
            m_caption = "线线关系处理";
            m_toolTip = "----";
            currentTool = new ControlsEditingEditToolClass();
            m_BaseLine = null;
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            m_Step = 0;
            m_Application.MapControl.Map.ClearSelection();
            base.OnClick();
            currentTool.OnClick();
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            IPoint mousePt = ToSnapedMapPoint(x, y);
            ISelectionEnvironment selEnv = new SelectionEnvironmentClass();
            if (button == 1)//Left Mouse 
            {
                if (m_Step == 0) //Base Line
                {                    
                    if (shift == 2)//Ctrl
                    {
                        selEnv.CombinationMethod = esriSelectionResultEnum.esriSelectionResultXOR;
                        m_Application.MapControl.Map.SelectByShape(mousePt, selEnv, false);
                    }
                    else
                    {
                        selEnv.CombinationMethod = esriSelectionResultEnum.esriSelectionResultNew;
                        m_Application.MapControl.Map.SelectByShape(mousePt, selEnv, true);
                    }
                    m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
                }
                else if (m_Step == 1) //modify Feature
                {
                    selEnv.CombinationMethod = esriSelectionResultEnum.esriSelectionResultNew;                    
                    m_Application.MapControl.Map.SelectByShape(mousePt, null, true);
                    m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
                }
            }
            else if (button == 2)//Right Mouse 
            {
                m_Step += 1; 
                if(m_Step==1)
                {
                    m_BaseLine = new PolylineClass();
                    IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                    mapEnumFeature.Reset();
                    IFeature feature = mapEnumFeature.Next();
                    while (feature != null)
                    {
                        if (feature.Shape is IPolyline)
                        {
                            IGeometry newGeo = (m_BaseLine as ITopologicalOperator).Union(feature.Shape);
                            m_BaseLine = newGeo as IPolyline;
                        }
                        feature = mapEnumFeature.Next();                       
                    }
                    if (m_BaseLine != null&&m_BaseLine.Length>0.0)
                    {
                        ISimpleLineSymbol lineSymbol = SetBaseLineSymbol();
                        ILineElement lineElement = new LineElementClass();
                        lineElement.Symbol = lineSymbol;
                        IElement element = lineElement as IElement;
                        element.Geometry = m_BaseLine;
                        IGraphicsContainer gc = m_Application.MapControl.Map as IGraphicsContainer;
                        gc.AddElement(element, 0);
                        IActiveView activeView = m_Application.MapControl.Map as IActiveView;
                        m_Application.MapControl.Map.ClearSelection();
                        m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
                        //activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                    }
                    else
                    {
                        m_Step = 0;
                    }
                }
                else if (m_Step == 2)
                {
 
                }
                
            }
            currentTool.OnMouseDown(button, shift, x, y);
            base.OnMouseDown(button, shift, x, y);
        }

        public override void OnKeyUp(int keyCode, int shift)
        {
            base.OnKeyUp(keyCode, shift);
            if (keyCode == (int)Keys.Escape) //ESC key
            {
                m_Step = 0;
                m_BaseLine = null;
                m_Application.MapControl.Map.ClearSelection();

                IGraphicsContainer graphicsContainer = m_Application.MapControl.Map as IGraphicsContainer;
                graphicsContainer.DeleteAllElements();
                IActiveView activeView = m_Application.MapControl.Map as IActiveView;
                activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);  

                m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
            }
        }

        public ISimpleLineSymbol SetBaseLineSymbol()
        {
            ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
            lineSymbol.Style = esriSimpleLineStyle.esriSLSDash;
            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Red=255;
            rgbColor.Green=0;
            rgbColor.Blue=0;
            lineSymbol.Color = rgbColor;
            lineSymbol.Width = 3;
            return lineSymbol;
        }
    }
}
