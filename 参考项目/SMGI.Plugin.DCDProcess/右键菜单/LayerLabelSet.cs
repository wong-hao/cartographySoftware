using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
namespace SMGI.Plugin.DCDProcess
{
    public class LayerLabelSet : SMGI.Common.SMGIContextMenu
    {
        public LayerLabelSet()
        {
            m_caption = "设置图层标注";
            m_toolTip = "设置图层标注";
            m_category = "图层管理";
        }

        public override bool Enabled
        {
            get { return CurrentContextItem is ILayer && CurrentContextItem is IGeoFeatureLayer; }
        }

        public override void OnClick()
        {
            var layer = CurrentContextItem as IGeoFeatureLayer;
            if (layer != null)
            {
                var lsf = new LayerLabelSetForm(layer);
                if (lsf.ShowDialog() == DialogResult.OK) m_Application.ActiveView.Refresh();
            }
        }
    }
}
