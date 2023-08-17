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
using System.Threading;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 面化简交互式工具
    /// </summary>
    public class PolygonGeneralizeTool : SMGITool
    {
        private static IFeatureLayer _inputLayer = null;
        private static double _refScale;
        private static string _simplyAlgorithm;
        private static double _simplifyTolerance;
        private static bool _enableSmooth;
        private static string _smoothAlgorithm;
        private static double _smoothTolerance;

        public PolygonGeneralizeTool()
        {
            m_category = "面化简（交互）";
            m_caption = "面化简（交互）";
        }

        public override bool Enabled
        {
            get
            {
                if (m_Application.Workspace == null)
                    _inputLayer = null;

                return m_Application != null && m_Application.Workspace != null &&
                    m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        public override void OnClick() 
        {
            if (_inputLayer == null)
            {
                //弹框，设置成员参数
                var frm = new GeneralizeForm(esriGeometryType.esriGeometryPolygon, m_Application.MapControl.Map.ReferenceScale, false, false);
                frm.Text = "面化简";
                if (frm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    if (_inputLayer == null)
                    {
                        m_Application.MapControl.CurrentTool = null;
                    }
                    return;
                }

                _inputLayer = frm.ObjLayer as IFeatureLayer;
                _refScale = frm.RefScale;
                _simplyAlgorithm = frm.SimplifyAlgorithm;
                _simplifyTolerance = frm.SimplifyTolerance;
                _enableSmooth = frm.EnableSmooth;
                _smoothAlgorithm = frm.SmoothAlgorithm;
                _smoothTolerance = frm.SmoothTolerance;
            }
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
            {
                return;
            }

            if (_inputLayer == null)
            {
                MessageBox.Show("请先设置化简参数！");
                return;
            }

            //拉框
            IRubberBand rubberBand = new RubberRectangularPolygonClass();
            IGeometry geo = rubberBand.TrackNew(m_Application.ActiveView.ScreenDisplay, null);
            if (geo == null || geo.IsEmpty)
                return;

            //清理所选要素
            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            m_Application.ActiveView.FocusMap.ClearSelection();

            //查询范围内指定要素类中的所有要素,并进行化简
            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    wo.SetText("正在检索要素......");
                    ISpatialFilter sf = new SpatialFilterClass();
                    sf.Geometry = geo;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                    List<int> selFIDList = new List<int>();

                    IFeatureCursor feCursor = _inputLayer.Search(sf, true);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        selFIDList.Add(fe.OID);

                        m_Application.ActiveView.FocusMap.SelectFeature(_inputLayer, fe);
                    }
                    Marshal.ReleaseComObject(feCursor);

                    if (selFIDList.Count == 0)
                    {
                        MessageBox.Show(string.Format("拉框范围内没有找到目标图层【{0}】中的要素！", _inputLayer.Name));
                        return;
                    }


                    wo.SetText("正在化简......");
                    string oidSet = "";
                    foreach (var oid in selFIDList)
                    {
                        if (oidSet != "")
                            oidSet += string.Format(",{0}", oid);
                        else
                            oidSet = string.Format("{0}", oid);
                    }
                    string filterText = string.Format("OBJECTID in ({0})", oidSet);

                    var bsuccess = PolygonGeneralizeCmd.Generalize(_inputLayer.FeatureClass, filterText, _simplyAlgorithm, _simplifyTolerance, _enableSmooth, _smoothAlgorithm, _smoothTolerance, true);
                    if (bsuccess)
                    {
                        //刷新
                        m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                        m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, _inputLayer, m_Application.ActiveView.Extent);
                    }
                    else
                    {
                        //清理所选要素
                        m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                        m_Application.ActiveView.FocusMap.ClearSelection();
                    }

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return;
            }



            
        }

        public override void OnKeyUp(int keyCode, int shift)
        {
            switch (keyCode)
            {
                case 32:
                    //弹框，设置成员参数
                    var frm = new GeneralizeForm(esriGeometryType.esriGeometryPolygon, _inputLayer, _refScale, _simplyAlgorithm, _simplifyTolerance, _enableSmooth, _smoothAlgorithm, _smoothTolerance, false);
                    frm.Text = "面化简";
                    if (frm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        if (_inputLayer == null)
                        {
                            m_Application.MapControl.CurrentTool = null;
                        }
                        return;
                    }

                    _inputLayer = frm.ObjLayer as IFeatureLayer;
                    _refScale = frm.RefScale;
                    _simplyAlgorithm = frm.SimplifyAlgorithm;
                    _simplifyTolerance = frm.SimplifyTolerance;
                    _enableSmooth = frm.EnableSmooth;
                    _smoothAlgorithm = frm.SmoothAlgorithm;
                    _smoothTolerance = frm.SmoothTolerance;
                    break;
                default:
                    break;
            }
        }
    }
}
