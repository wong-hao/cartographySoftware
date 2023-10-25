using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using SMGI.Common.AttributeTable;
using SMGI.Common;
namespace SMGI.Plugin.DCDProcess
{
    public class CreateLayerBySelectionCmd : SMGI.Common.SMGIContextMenu
    {
        public CreateLayerBySelectionCmd()
        {
            m_caption = "根据所选要素创建图层";
            m_toolTip = "根据所选要素创建图层";
            m_category = "图层管理";

        }
        public override bool Enabled
        {
            get
            {
                if (!(this.CurrentContextItem is IFeatureLayer))
                    return false;

                if ((this.CurrentContextItem as IFeatureLayer).FeatureClass == null)
                    return false;

                IFeatureLayer layer = this.CurrentContextItem as IFeatureLayer;
                if ((layer as IFeatureSelection).SelectionSet.Count == 0)
                    return false;


                return true;
            }
        }
        public override void OnClick()
        {
            IFeatureLayer layer = this.CurrentContextItem as IFeatureLayer;

            IFeatureLayer selectionLayer = (layer as IFeatureLayerDefinition).CreateSelectionLayer(layer.Name + "_选择", true,"","");

            m_Application.MapControl.Map.AddLayer(selectionLayer);
        }
    }
}
