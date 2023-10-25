using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.Windows.Forms;

namespace SMGI.Plugin.DCDProcess
{
    public class LayerOnlySelect:SMGI.Common.SMGIContextMenu
    {
        public LayerOnlySelect() {
            m_caption = "只选择当前图层";
            m_toolTip = "只选择当前图层，设置其他所有图层不可选";
            m_category = "图层管理";
        }
        public override bool Enabled
        {
            get
            {
                if (this.CurrentContextItem is IFeatureLayer)
                {
                    return true;
                }
                return false;
            }
        }
        public override void OnClick()
        {
            IFeatureLayer pSelectLayer = this.CurrentContextItem as IFeatureLayer;
            if (pSelectLayer != null)
            {
                var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l is IFeatureLayer && (l as IGeoFeatureLayer).FeatureClass != null;
                })).ToArray();
                for (int i = 0; i < lyrs.Length; i++)
                {
                    IFeatureLayer pFeatureLayer = lyrs[i] as IFeatureLayer;
                    if (pFeatureLayer != null)
                    {
                        if (pSelectLayer == pFeatureLayer)
                        {
                            pFeatureLayer.Selectable = true;
                        }
                        else
                        {
                            pFeatureLayer.Selectable = false;
                        }
                    }
                }
            }
        }

    }
}
