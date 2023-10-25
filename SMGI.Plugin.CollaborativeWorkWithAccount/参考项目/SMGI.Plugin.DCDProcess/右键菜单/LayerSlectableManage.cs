using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.CartoUI;

namespace SMGI.Plugin.DCDProcess
{
    public class LayerSlectableManage : SMGI.Common.SMGIContextMenu
    {
        public LayerSlectableManage()
        {
            m_caption = "图层选择管理";
            m_toolTip = "图层选择管理";
            m_category = "地图管理";
        }

        public override bool Enabled
        {
            get
            {
                return this.CurrentContextItem is IMap;
            }
        }

        public override void OnClick()
        {
            var mm = new LayerSelectableManageForm(m_Application);
            mm.ShowDialog();
        }
    }
}
