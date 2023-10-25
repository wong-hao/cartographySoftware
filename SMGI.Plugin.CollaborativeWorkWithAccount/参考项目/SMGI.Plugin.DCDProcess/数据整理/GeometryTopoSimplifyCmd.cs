using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 对被选择要素，进行几何体的拓扑关系的纠正
    /// </summary>
    public class GeometryTopoSimplifyCmd : SMGI.Common.SMGICommand
    {
        public GeometryTopoSimplifyCmd()
        {
            m_caption = "拓扑纠正";
            m_toolTip = "使被选择要素的几何体拓扑关系正确";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing &&
                       m_Application.Workspace.Map.SelectionCount > 0;
            }
        }

        public override void OnClick()
        {
            var map = m_Application.Workspace.Map;
            if (map.SelectionCount == 0) return;

            using (var wo = GApplication.Application.SetBusy())
            {
                wo.SetText("正在处理...");

                m_Application.EngineEditor.StartOperation();
                try
                {
                    var fes = (IEnumFeature)map.FeatureSelection;
                    fes.Reset();
                    IFeature selFeature = null;
                    while ((selFeature = fes.Next()) != null)
                    {
                        IGeometry geo = selFeature.ShapeCopy;
                        ITopologicalOperator2 feaTopo = geo as ITopologicalOperator2;
                        feaTopo.IsKnownSimple_2 = false;
                        feaTopo.Simplify();
                        selFeature.Shape = geo;
                        selFeature.Store();


                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    MessageBox.Show(ex.Message);

                    m_Application.EngineEditor.AbortOperation();

                    return;
                }
                

                m_Application.EngineEditor.StopOperation("拓扑纠正");
                m_Application.ActiveView.Refresh();
            }
        }
    }
}
