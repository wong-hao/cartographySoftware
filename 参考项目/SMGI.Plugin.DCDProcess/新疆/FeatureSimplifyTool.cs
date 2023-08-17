using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using System.Diagnostics;
using SMGI.Common.Algrithm;

namespace SMGI.Plugin.DCDProcess
{
    public class FeatureSimplifyTool : SMGI.Common.SMGITool
    {
        private IMap pMap = null;
        private IActiveView pActiveView = null;

        private INewEnvelopeFeedback m_envelopeFeedback;
        private bool m_isMouseDown = false;
        private double m_width;
        private double m_height;

        public int frmCount = 0;
        FrmSimplify frm;

        public FeatureSimplifyTool()
        {
            m_caption = "要素边化简Tool";
            m_category = "实用工具";
            m_toolTip = "要素边化简";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }        

        public void drawpolyline(int red, int green, int blue, IPolyline polyline)
        {
            ISimpleLineSymbol sls = new SimpleLineSymbolClass();
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = red;
            rgb.Green = green;
            rgb.Blue = blue;
            sls.Color = rgb;
            sls.Width = 2;
            ISymbol symbol = (ISymbol)sls;
            IScreenDisplay dis = m_Application.ActiveView.ScreenDisplay;
            dis.StartDrawing(dis.hDC, System.Convert.ToInt16(ESRI.ArcGIS.Display.esriScreenCache.esriNoScreenCache));
            dis.SetSymbol(symbol);
            dis.DrawPolyline(polyline);

            dis.FinishDrawing();

        }
        public IPoint foreachlist(List<IPoint> lstPoint, IPoint pPoint)
        {
            IPoint pTemp = null;
            for (int i = 0; i < lstPoint.Count; i++)
            {
                if (Math.Round(pPoint.X, 3) == Math.Round(lstPoint[i].X, 3) && Math.Round(pPoint.Y, 3) == Math.Round(lstPoint[i].Y, 3)) { pTemp = lstPoint[i]; break; }
            }
            return pTemp;
        }
        public IMultipoint getIntersectPoint(IPolyline pPolyline)//找出多段线自相交交点
        {
            IGeometryCollection pGeoCol1 = pPolyline as IGeometryCollection;
            //List<IPoint> lstPoint = new List<IPoint>();
            //for (int i = 0; i < pGeoCol1.GeometryCount; i++)
            //{
            //    IPath pPath = pGeoCol1.get_Geometry(i) as IPath;
            //    IPoint pTemp = foreachlist(lstPoint, pPath.FromPoint);
            //    if (pTemp == null) lstPoint.Add(pPath.FromPoint);
            //    pTemp = foreachlist(lstPoint, pPath.ToPoint);
            //    if (pTemp == null) lstPoint.Add(pPath.ToPoint);
            //}
            ITopologicalOperator2 ptopo = pPolyline as ITopologicalOperator2;
            ptopo.IsKnownSimple_2 = false;
            ptopo.Simplify();
            IMultipoint pMultipoint = new MultipointClass();
            IPointCollection pPointCol1 = pMultipoint as IPointCollection;
            for (int i = 0; i < pGeoCol1.GeometryCount; i++)
            {
                IPath pPath = pGeoCol1.get_Geometry(i) as IPath;
                IPoint pTempF =  pPath.FromPoint;
                IPoint pTempT = pPath.ToPoint;
                if (Math.Round(pTempF.X, 3) == Math.Round(pTempT.X, 3) && Math.Round(pTempF.Y, 3) == Math.Round(pTempT.Y, 3)) { pPointCol1.AddPoint(pTempF); break; }

                //IPath pPath = pGeoCol1.get_Geometry(i) as IPath;
                //IPoint pTemp = foreachlist(lstPoint, pPath.FromPoint); ;
                //if (pTemp == null) pPointCol1.AddPoint(pPath.FromPoint);
                //pTemp = foreachlist(lstPoint, pPath.ToPoint);
                //if (pTemp == null) pPointCol1.AddPoint(pPath.ToPoint);
            }
            return pMultipoint;

        }
        public static void drawPolyine(IPolyline pPolyline, IMap pMap)
        {
            try
            {
                IGraphicsContainer pGra = pMap as IGraphicsContainer;
                IActiveView pAv = pGra as IActiveView;



                pGra.DeleteAllElements();

                ILineElement pLineEle = new LineElementClass();
                IElement pEle = pLineEle as IElement;
                pEle.Geometry = pPolyline;


                IRgbColor pColor = new RgbColorClass();
                Random r = new Random();
                pColor.Red = 255;

                pColor.Green = 0;
                pColor.Blue = 0;

                ILineSymbol pOutline = new SimpleLineSymbolClass();
                pOutline.Width = 2;
                pOutline.Color = pColor;

                pLineEle.Symbol = pOutline;

                pGra.AddElement((IElement)pLineEle, 0);
                pAv.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, pPolyline.Envelope);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
            }
        }
        private List<IFeature> GetFeatureList(IFeatureLayer pFeatureLayer)
        {
            IFeatureSelection pFeatureSelection = pFeatureLayer as IFeatureSelection;
            ISelectionSet pSelectionSet = pFeatureSelection.SelectionSet;
            IFeature pFeature;
            List<IFeature> featureList = new List<IFeature>();
            if (pSelectionSet.Count > 0)
            {
                IEnumIDs IDs = null;
                IDs = pSelectionSet.IDs;
                int ID = IDs.Next();
                while (ID > 0)
                {
                    pFeature = pFeatureLayer.FeatureClass.GetFeature(ID);

                    featureList.Add(pFeature);
                    ID = IDs.Next();
                }
            }

            return featureList;
        }

        public override void OnClick()
        {
            if (m_width > 0 && m_height > 0)
            {
                frm = new FrmSimplify(m_width, m_height);
            }
            else
            {
                frm = new FrmSimplify(); 
            }
            frm.Show();
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button == 1)//左键
            {
                m_isMouseDown = true;
                IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                if (m_envelopeFeedback == null)
                {
                    m_envelopeFeedback = new NewEnvelopeFeedbackClass();
                    m_envelopeFeedback.Display = m_Application.ActiveView.ScreenDisplay;
                    m_envelopeFeedback.Start(currentMouseCoords);
                }
                else
                {
                    m_envelopeFeedback.Start(currentMouseCoords);
                }
            }
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (button == 1)//左键
            {
                if (m_isMouseDown && m_envelopeFeedback != null)
                {
                    IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                    m_envelopeFeedback.MoveTo(currentMouseCoords);
                }
            }    
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            if (button == 1)//左键
            { 
                m_width = frm.width;
                m_height = frm.height;                    
              
                if ( m_width <= 0.0 || m_height <=0.0)
                {
                    MessageBox.Show(string.Format("参数未设置，请右键设置"));
                }
                IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                IGeometry geo = m_envelopeFeedback.Stop();
                IMap m_Map = m_Application.ActiveView as IMap;
                if (geo != null)
                {
                    m_Map.SelectByShape(geo, null, false);

                    if(m_Application.MapControl.Map.SelectionCount == 0)
                        return;
                    IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                    mapEnumFeature.Reset();
                    IFeature feature = null;

                    m_Application.EngineEditor.StartOperation();
                    using (var waitOp = m_Application.SetBusy())
                    {
                        while ((feature = mapEnumFeature.Next()) != null)
                        {
                            if (feature.Shape is IPolyline || feature.Shape is IPolygon)//只处理线要素和面要素
                            {                                
                                if (feature.HasOID)
                                    waitOp.SetText(string.Format("正在处理要素类【{0}】中的要素【{1}】", feature.Class.AliasName, feature.OID));

                                if ((feature.Class as IFeatureClass).ShapeType == esriGeometryType.esriGeometryPolyline)
                                {
                                    IMultipoint IMultipoint = getIntersectPoint(feature.ShapeCopy as IPolyline);
                                    if (!IMultipoint.IsEmpty)
                                    { continue; }                                    
                                }

                                var pl = SimplifyByDTAlgorithm.SimplifyByDT(feature.ShapeCopy as IPolycurve, m_height, m_width);
                                feature.Shape = pl as IGeometry;
                                feature.Store();
                                //m_Application.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewAll, null, pl.Envelope);
                            }
                        }
                        //System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                    }
                    m_Application.ActiveView.Refresh();
                    //m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, geo.Envelope);

                    //m_Application.EngineEditor.StopOperation("化简Tool操作");
                    m_Application.EngineEditor.StopOperation(this.Caption);

                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                }
                m_isMouseDown = false;
                m_envelopeFeedback = null;
                
            }
            else if (button == 2)//右键，弹出窗口
            {
                //pass
            }
        }

        public override void OnKeyUp(int keyCode, int shift)
        {
            /*
            if (frmCount == 0)
            {
                FrmSimplify frm = new FrmSimplify(m_width, m_height);
                frm.FormClosing+=new FormClosingEventHandler(frm_FormClosing);
                frm.Shown+=new EventHandler(frm_Shown);                
                frm.Show();
                m_width = frm.width;
                m_height = frm.height;
            } 
             */ 
        }

        public void frm_FormClosing(object sender, EventArgs e)
        {
            frmCount -= 1; 
        }
        public void frm_Shown(object sender, EventArgs e)
        {
            frmCount += 1; 
        }

        public override bool Deactivate()
        {
            if (frm != null)
                frm.Close();
            frm = null;
            return true;
        }
    }
}
