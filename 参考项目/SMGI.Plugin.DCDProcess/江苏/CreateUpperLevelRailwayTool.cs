using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 生成上层路（铁路）-江西
    /// </summary>
    public class CreateUpperLevelRailwayTool : SMGITool
    {
        private IFeatureLayer _lrdlLayer;
        private IFeatureLayer _lralLayer;

        public CreateUpperLevelRailwayTool()
        {
            m_category = "生成上层路（铁路）";
            m_caption = "生成上层路（铁路）";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        public override void OnClick() 
        {

            string lrdlLyrName = "LRRL";
            _lrdlLayer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
                        ((l as IGeoFeatureLayer).Name.Trim().ToUpper() == lrdlLyrName)).FirstOrDefault() as IFeatureLayer;
            if (_lrdlLayer == null)
            {
                MessageBox.Show(string.Format("当前工作空间中没有找到图层【{0}】!", lrdlLyrName));
                return;
            }

            string lralLyrName = "LRAL";
            _lralLayer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
                        ((l as IGeoFeatureLayer).Name.Trim().ToUpper() == lralLyrName)).FirstOrDefault() as IFeatureLayer;
            if (_lralLayer == null)
            {
                MessageBox.Show(string.Format("当前工作空间中没有找到图层【{0}】!", lralLyrName));
                return;
            }

        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
                return;

            if (_lralLayer == null || _lralLayer.FeatureClass == null || _lrdlLayer == null || _lrdlLayer.FeatureClass == null)
                return;

            //画范围
            IRubberBand rubberBand = new RubberRectangularPolygonClass();
            var trackGeo = rubberBand.TrackNew(m_Application.ActiveView.ScreenDisplay, null);
            if (trackGeo == null || trackGeo.IsEmpty)
                return;

            List<InterFeatureWarp> interFeList = new List<InterFeatureWarp>();

            #region 获取与范围面相交的要素(道路线)及相交几何
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = trackGeo;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            if (_lrdlLayer.FeatureClass.HasCollabField())
                sf.WhereClause = cmdUpdateRecord.CurFeatureFilter;

            IFeatureCursor feCursor = _lrdlLayer.FeatureClass.Search(sf, false);
            IFeature fe = null;
            while ((fe = feCursor.NextFeature()) != null)
            {
                if (fe.Shape == null || fe.Shape.IsEmpty)
                    continue;

                ITopologicalOperator2 topo = fe.Shape as ITopologicalOperator2;
                topo.IsKnownSimple_2 = false;
                topo.Simplify();

                IPolyline interPolyline = topo.Intersect(trackGeo, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                if (interPolyline == null || interPolyline.IsEmpty)
                    continue;

                interFeList.Add(new InterFeatureWarp(fe, interPolyline));
            }
            Marshal.ReleaseComObject(feCursor);

            if (interFeList.Count == 0)
                return;
            #endregion

            //交互选择
            SelectInterFeatureForm frm = new SelectInterFeatureForm(interFeList);
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            //插入上层路要素到交通附属设置
            if (frm.InterFeature != null)
            {
                m_Application.EngineEditor.StartOperation();

                IFeatureClassLoad fcLoad = _lralLayer.FeatureClass as IFeatureClassLoad;
                fcLoad.LoadOnlyMode = true;

                try
                {
                    var lralFeCursor = _lralLayer.FeatureClass.Insert(true);

                    var fb = _lralLayer.FeatureClass.CreateFeatureBuffer();

                    //几何
                    fb.Shape = frm.InterFeature.InterPolyline;

                    #region 属性
                    //协同字段
                    if (_lralLayer.FeatureClass.HasCollabField())
                    {
                        fb.set_Value(fb.Fields.FindField(cmdUpdateRecord.CollabVERSION), cmdUpdateRecord.NewState);
                        fb.set_Value(fb.Fields.FindField(cmdUpdateRecord.CollabGUID), Guid.NewGuid().ToString());
                        fb.set_Value(fb.Fields.FindField(cmdUpdateRecord.CollabOPUSER), System.Environment.MachineName);
                    }

                    //LGB赋值
                    string lgbFN = m_Application.TemplateManager.getFieldAliasName("LGB", _lralLayer.FeatureClass.AliasName);
                    int lgbIndex = _lralLayer.FeatureClass.FindField(lgbFN);
                    if (lgbIndex != -1)
                    {
                        int roadGB = 0;
                        int roadGBIndex = frm.InterFeature.Feature.Fields.FindField("GB");
                        if (roadGBIndex != -1)
                        {
                            int.TryParse(frm.InterFeature.Feature.get_Value(roadGBIndex).ToString(), out roadGB);
                        }

                        if (roadGB != 0)
                        {
                            fb.set_Value(lgbIndex, roadGB);
                            fb.set_Value(roadGBIndex, roadGB);//上层路也设置GB
                        }
                    }
                    #endregion

                    var oid = (int)lralFeCursor.InsertFeature(fb);
                    lralFeCursor.Flush();
                    Marshal.ReleaseComObject(lralFeCursor);

                    //选择新插入的上层路
                    m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                    m_Application.MapControl.Map.ClearSelection();

                    m_Application.MapControl.Map.SelectFeature(_lralLayer, _lralLayer.FeatureClass.GetFeature(oid));

                    m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);

                    m_Application.EngineEditor.StopOperation("生成上层路(铁路)");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    MessageBox.Show(ex.Message);

                    m_Application.EngineEditor.AbortOperation();
                }
                finally
                {
                    fcLoad.LoadOnlyMode = false;
                }
            }

        }
    }
}
