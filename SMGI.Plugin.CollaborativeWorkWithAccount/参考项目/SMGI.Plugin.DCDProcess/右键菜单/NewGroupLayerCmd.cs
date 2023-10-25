using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.DCDProcess
{
    public class NewGroupLayerCmd : SMGI.Common.SMGIContextMenu
    {
        public NewGroupLayerCmd()
        {
            m_caption = "新建图层组";

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

            IGroupLayer newGroupLayer = new GroupLayerClass();
            newGroupLayer.Name = "新建图层组";

            map.AddLayer(newGroupLayer);
        }
    }
}
