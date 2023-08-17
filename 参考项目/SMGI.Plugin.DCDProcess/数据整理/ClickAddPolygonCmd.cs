using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;

namespace SMGI.Plugin.DCDProcess
{
    public class ClickAddPolygonCmd : SMGI.Common.SMGITool
    {
        public ClickAddPolygonCmd()
        {
            m_category = "封闭空白区面要素填充";
            m_caption = "封闭空白区面要素填充";
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
        }
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            IPoint point = m_Application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

            m_Application.EngineEditor.StartOperation();
            List<IFeatureClass> referFCList = new List<IFeatureClass>();
            var gls = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && l.Visible && (l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon);
            foreach (var gl in gls) 
            {
                IFeatureClass fc = (gl as IFeatureLayer).FeatureClass;
                referFCList.Add(fc);
            }
            
            if (referFCList.Count == 0)
            {
                MessageBox.Show("选择图层不能为空！");
                return;
            }
            //获取目标图层，并查询点击位置是否存在面要素
            IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;
            IEngineEditLayers engineEditLayer = editsketch as IEngineEditLayers;
            IFeatureLayer targetFeatureLayer = engineEditLayer.TargetLayer;

            if (!(targetFeatureLayer is IGeoFeatureLayer) || targetFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                MessageBox.Show("目标图层不为面图层！");
                return;
            }
            IFeatureClass targetFC = targetFeatureLayer.FeatureClass;
            ISpatialFilter sf = new SpatialFilterClass { Geometry = point, SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects };
            bool co = true;
            foreach (var gl in gls)
            {
                if ((gl as IFeatureLayer).FeatureClass.FeatureCount(sf) > 0)
                {
                    co = false;
                }
            }
            if (co == false)
            {
                MessageBox.Show("点击处不为空白区域！");
                return;
            }
            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                //QueryFilterClass qf = new QueryFilterClass();
                //获取屏幕范围矩形
                IEnvelope ext = ((IActiveView)m_Application.Workspace.Map).Extent;
                ISpatialFilter qf = new SpatialFilter();
                qf.Geometry = ext;
                qf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                if (targetFC.HasCollabField())
                {
                    qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                }

                ClickAddPolygon fh = new ClickAddPolygon();
                err = fh.CloseSpaceAddPolygon(targetFC, referFCList, qf, point, wo);
            }

            if (err != "")
            {
                m_Application.EngineEditor.AbortOperation();

                MessageBox.Show(err);
            }
            else
            {
                m_Application.EngineEditor.StopOperation("空白处填充面要素处理");

                m_Application.ActiveView.Refresh();
                MessageBox.Show("处理完毕！");
            }
        }
    }
}
