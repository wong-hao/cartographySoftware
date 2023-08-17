using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.DCDProcess
{
    public class MapDoActive : SMGI.Common.SMGIContextMenu
    {
        public MapDoActive()
        {
            m_caption = "激活地图";
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
            var map=this.CurrentContextItem as IMap;
            m_Application.PageLayoutControl.ActiveView.FocusMap = map;
            m_Application.PageLayoutControl.Refresh();
            m_Application.TOCControl.Refresh();
        }
    }
}
