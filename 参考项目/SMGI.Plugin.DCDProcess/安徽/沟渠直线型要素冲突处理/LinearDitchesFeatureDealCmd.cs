using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using SMGI.Common;
using ESRI.ArcGIS.DataSourcesFile;


namespace SMGI.Plugin.DCDProcess
{
    public class LinearDitchesFeatureDealCmd : SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return
                    m_Application != null
                    && m_Application.MapControl != null
                    && m_Application.LayoutState == Common.LayoutState.MapControl
                    && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public LinearDitchesFeatureDealCmd()
        {
            m_caption = "沟渠直线型要素冲突处理";
            m_toolTip = "工具支持选择一根直线型多段沟渠进行偏移，偏移后与相交的其余沟渠整体进行打断、合并处理";
        }

        public override void OnClick()
        {
            if (m_Application.ActiveView.FocusMap.SelectionCount != 1)
            {
                MessageBox.Show("请选择一条直线型要素进行操作！");
               // m_Application.MapControl.CurrentTool = CurrentTool();
                return;
            }
            LinearDitchesFeatureDealForm frm = new LinearDitchesFeatureDealForm(m_Application );
            frm.StartPosition = FormStartPosition.CenterScreen;

            if (frm.ShowDialog() != DialogResult.OK)
            {
                return;
            }
        }


        //返回鼠标选择模式方法
        private ITool CurrentTool()
        {
            ITool CurTool = null;
            if (m_Application.PluginManager.Commands.ContainsKey("SMGI.Plugin.GeneralEdit.EditSelector"))
            {
                PluginCommand cmd = m_Application.PluginManager.Commands["SMGI.Plugin.GeneralEdit.EditSelector"];
                if (cmd != null && cmd.Enabled)
                {
                    m_Application.MapControl.CurrentTool = cmd.Command as ITool;
                    CurTool = cmd.Command as ITool;
                }
            }
            return CurTool;
        }

    }
}
