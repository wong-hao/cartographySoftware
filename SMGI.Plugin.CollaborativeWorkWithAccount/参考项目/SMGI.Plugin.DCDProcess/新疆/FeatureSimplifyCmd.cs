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
    public class FeatureSimplifyCmd : SMGI.Common.SMGICommand
    {
        private IMap pMap = null;
        private IActiveView pActiveView = null;


        public FeatureSimplifyCmd()
        {

            m_caption = "要素边化简";
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
        public override void OnClick()
        {
            List<int> Error = new List<int>();
            pMap = m_Application.ActiveView as IMap;
            pActiveView = m_Application.ActiveView;

            var view = pActiveView;

            var map = pMap;

            if (map.SelectionCount == 0)
            {
                MessageBox.Show("未选中任何要素");
                return;
            }
            //获取选中要素类
            Dictionary<string, List<IFeature>> fesDic = new Dictionary<string, List<IFeature>>();

            IEngineEditLayers editLayer = m_Application.EngineEditor as IEngineEditLayers;
            var layers = m_Application.Workspace.LayerManager.GetLayer(l => (l is IFeatureLayer && l.Visible));
            foreach (var layer in layers)
            {
                if (!editLayer.IsEditable(layer as IFeatureLayer))
                {
                    continue;
                }

                List<IFeature> fes = GetFeatureList(layer as IFeatureLayer);
                fesDic[layer.Name] = fes;
            }

            if (fesDic.Count == 0)
            {
                MessageBox.Show("未选中任何可编辑要素");
                return;
            }

            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)+"\\";
            TxtRecordFile TxtRecordFile = new TxtRecordFile("要素边化简错误记录", folderPath);
            FrmSimplify frm = new FrmSimplify();
            double width = 0, heigth = 0;
            if (frm.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            width = frm.width;
            heigth = frm.height;

            m_Application.EngineEditor.StartOperation();

            using (var waitOp = m_Application.SetBusy())
            {
                foreach (var kv in fesDic)
                {
                    foreach (IFeature fe in kv.Value)
                    {
                        try
                        {
                            if (!(fe.Shape is IPolycurve))
                            {
                                continue;

                            }

                            if (fe.HasOID)
                                waitOp.SetText(string.Format("正在处理要素类【{0}】中的要素【{1}】", fe.Class.AliasName, fe.OID));
                            if ((fe.Class as IFeatureClass).ShapeType == esriGeometryType.esriGeometryPolyline)
                            {
                                IMultipoint IMultipoint = getInersectPoint(fe.ShapeCopy as IPolyline);
                                if (!IMultipoint.IsEmpty)
                                {
                                    //  drawpolyline(0, 0, 255, fe.Shape as IPolyline);
                                    continue;
                                }//若多段线自相交则跳过
                            }
                       //     drawPolyine(fe.ShapeCopy as IPolyline, map);
                            var pl = SimplifyByDTAlgorithm.SimplifyByDT(fe.ShapeCopy as IPolycurve, heigth, width);
                            fe.Shape = pl as IGeometry;
                            fe.Store();

                            view.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewAll, null, pl.Envelope);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine(ex.Message);
                            System.Diagnostics.Trace.WriteLine(ex.Source);
                            System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                            Error.Add(fe.OID);
                            //  MessageBox.Show(fe.OID+"错误");
                            //  string str = null;
                        }
                    }

                }
            }
            m_Application.EngineEditor.StopOperation("要素边化简");


            string errorid = null;
            if (Error.Count == 0) { return; }
            for (int i = 0; i < Error.Count; i++)
            {
                TxtRecordFile.RecordError(Error[i]+"");
                //  errorid += Error[i]+",";
                //   drawpolyline(0, 0, 255, fe.Shape as IPolyline);
            }
            //  MessageBox.Show("错误要素ID:"+" "+errorid);
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
        public IMultipoint getInersectPoint(IPolyline pPolyline)//找出多段线自相交交点
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

    }
}
