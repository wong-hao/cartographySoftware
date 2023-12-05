using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public class CutBaseOnFea : SMGITool
    {
        IEngineEditor editor;
        /// <summary>
        /// 线型符号
        /// </summary>
        ISimpleLineSymbol lineSymbol;
        /// <summary>
        /// 线型反馈
        /// </summary>
        INewLineFeedback lineFeedback;
        IFeature basefea = null;//基线裁切要素
        IFeatureClass CheckClass;//当前编辑图层
        public CutBaseOnFea()
        {
            m_caption = "修剪裁切";
            NeedSnap = false;
        }
        public override void OnClick()
        {
            editor = m_Application.EngineEditor;

            IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
            mapEnumFeature.Reset();
            basefea = mapEnumFeature.Next();
            IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;
            IEngineEditLayers engineEditLayer = editsketch as IEngineEditLayers;
            IFeatureLayer featureLayer = engineEditLayer.TargetLayer as IFeatureLayer;
            CheckClass = featureLayer.FeatureClass;
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
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
            {
                return;
            }
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
                GApplication.NoDelete = true;
            }
        }
        private void MapControl_OnAfterScreenDraw(object sender, IMapControlEvents2_OnAfterScreenDrawEvent e)
        {
            if (lineFeedback != null)
            {
                lineFeedback.Refresh(m_Application.ActiveView.ScreenDisplay.hDC);
                GApplication.NoDelete = false;
            }
        }
        public override bool Deactivate()
        {
            //卸掉该事件
            m_Application.MapControl.OnAfterScreenDraw -= new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);
            return base.Deactivate();
        }
        public override void OnDblClick()
        {
            List<IFeature> fea = new List<IFeature>();
            List<IFeature> feacopy = new List<IFeature>();
            List<IFeature> feadlet = new List<IFeature>();
            IPolyline polyline = lineFeedback.Stop();
            lineFeedback = null;
            //双击完毕进行线条的打断
            if (polyline.IsEmpty)
                return;
            editor.StartOperation();
            #region 收集与划线相交的要素
            ISpatialFilter filter = new SpatialFilterClass();
            filter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            filter.Geometry = polyline;
            IFeature inFeature = null;
            IFeatureCursor maskedCursor = CheckClass.Search(filter, false);
            while ((inFeature = maskedCursor.NextFeature()) != null)
            {
                fea.Add(inFeature);
            }

            foreach (var feature in fea)
            {
                // 创建新要素并设置形状
                IFeature newFeature = CheckClass.CreateFeature(); // 这里根据你的要素类类型创建新要素
                newFeature.Shape = feature.Shape;

                // 将新要素添加到新列表中
                feacopy.Add(newFeature);
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(maskedCursor);
            #endregion
            try
            {
                #region 执行线打断
                for (int i = 0; i < feacopy.Count; i++)
                {
                    IFeature tempfea = feacopy[i];
                    IPolyline ply = tempfea.ShapeCopy as IPolyline;
                    ITopologicalOperator2 pTopo = (ITopologicalOperator2)ply;
                    IGeometry InterGeo = pTopo.Intersect(basefea.Shape, esriGeometryDimension.esriGeometry0Dimension);
                    IGeometryCollection geoCol = InterGeo as IGeometryCollection;
                    if (geoCol.GeometryCount == 0) { continue; }
                    List<int> featurelist = new List<int>();
                    //    List<int> splitfeaturelist = new List<int>();
                    for (int j = 0; j < geoCol.GeometryCount; j++)
                    {
                        IPoint splitPt = new PointClass();
                        splitPt = geoCol.get_Geometry(j) as IPoint;
                        IFeatureEdit pFeatureEdit = null;
                        if (featurelist.Count != 0)
                        {
                            for (int k = 0; k < featurelist.Count; k++)
                            {
                                IFeature featemp = CheckClass.GetFeature(featurelist[k]);
                                //   if (splitfeaturelist.Contains(featurelist[j])) { continue; }//已经分割过一次的跳过
                                IPolyline polylinetemp = featemp.Shape as IPolyline;
                                IRelationalOperator relationalOperator = polylinetemp as IRelationalOperator;//好接口
                                if (relationalOperator.Contains(splitPt))
                                {
                                    if ((featemp.ShapeCopy as IPolycurve).Length == 0) { continue; }
                                    pFeatureEdit = (IFeatureEdit)featemp;
                                    featurelist.Remove(featurelist[j]);
                                    //    splitfeaturelist.Add(featemp.OID);
                                    break;
                                }

                            }
                        }
                        else
                        {
                            if ((tempfea.ShapeCopy as IPolycurve).Length == 0) { continue; }
                            pFeatureEdit = (IFeatureEdit)tempfea; //splitfeaturelist.Add(pFeature.OID); 

                        } ISet pFeatureSet = pFeatureEdit.Split(splitPt);
                        if (pFeatureSet != null)
                        {
                            pFeatureSet.Reset();
                            IFeature feature = null;
                            while ((feature = pFeatureSet.Next() as IFeature) != null)
                            {
                                featurelist.Add(feature.OID);
                                IRelationalOperator relationalOperatorPt = polyline as IRelationalOperator;//好接口
                                if (!relationalOperatorPt.Disjoint(feature.Shape))//相交
                                {
                                    feadlet.Add(feature);
                                }
                                else
                                {
                                    feadlet.Add(feature);
                                    fea[i].Shape = feature.Shape;
                                    fea[i].Store();
                                }
                            }
                        }
                    }

                }
                #endregion
                foreach (var item in feadlet)
                {
                    try
                    {
                        item.Delete();
                    }
                    catch (Exception ex) { continue; }
                }
            }
            catch (Exception ex) { }
            m_Application.EngineEditor.StopOperation("修剪裁切");
            m_Application.MapControl.Refresh();
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
