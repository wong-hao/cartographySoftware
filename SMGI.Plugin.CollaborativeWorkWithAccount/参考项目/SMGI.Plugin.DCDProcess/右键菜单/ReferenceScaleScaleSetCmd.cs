using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.DCDProcess
{
    public class ReferenceScaleScaleSetCmd : SMGI.Common.SMGIContextMenu
    {
        public ReferenceScaleScaleSetCmd()
        {
            m_caption = "设置参考比例尺";

        }
        public override bool Enabled
        {
            get
            {
                return (this.CurrentContextItem is IMap);
                
            }
        }
        public override void OnClick()
        {
            IMap map = (this.CurrentContextItem as IMap);
            ReferenceScaleDialog dlg = new ReferenceScaleDialog(map.ReferenceScale);
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                map.ReferenceScale = dlg.ReferScale;
                (map as IActiveView).Refresh();
            }
        }
    }
}
