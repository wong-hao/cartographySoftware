using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.DCDProcess
{
    public class LayerRemove : SMGI.Common.SMGIContextMenu
    {
        public LayerRemove()
        {
            m_caption = "移除图层";
        }
        public override bool Enabled
        {
            get
            {
                return this.CurrentContextItem is ILayer;
            }
        }
        public override void OnClick()
        {
            ILayer player = this.CurrentContextItem as ILayer;
            if (player == null)
            {
                return;
            }

            if (player is IFeatureLayer)
            {
                IFeatureClass fc = (player as IFeatureLayer).FeatureClass;
                IDataset dt = fc as IDataset;
                if (dt != null && dt.Workspace.PathName == m_Application.Workspace.EsriWorkspace.PathName)
                {
                    if (MessageBox.Show("该图层不是临时图层，移除后可能导致问题，确定移除吗？", "警告", MessageBoxButtons.OKCancel) != DialogResult.OK) return;
                }
            }

            try
            {
                m_Application.Workspace.LayerManager.Map.DeleteLayer(player);
            }
            catch
            {
                m_Application.MapControl.Map.DeleteLayer(player);
            }
            m_Application.TOCControl.ActiveView.Refresh();
            m_Application.TOCControl.Update();
        }
    }
}
