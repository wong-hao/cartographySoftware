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
    public class AttributeTableOpenCmd : SMGI.Common.SMGIContextMenu
    {
       
        public string name = null;
        public IFeatureClass pFcl = null;
        public AttributeTableOpenCmd()
        {
            m_caption = "打开属性表";
            m_toolTip = "打开属性表";
            m_category = "图层管理";

        }
        public override bool Enabled
        {
            get
            {
                if (this.CurrentContextItem is ILayer)
                {
                    ILayer l = this.CurrentContextItem as ILayer;
                    if (!(l is IFeatureLayer))
                        return false;

                    if ((l as IFeatureLayer).FeatureClass == null)
                        return false;
                   
                }

                return this.CurrentContextItem is ILayer;
            }
        }
        public override void OnClick()
        {
            ILayer player = this.CurrentContextItem as ILayer;
          TableForm form = new TableForm(m_Application, player as IFeatureLayer);
            form.Show(m_Application.MainForm as IWin32Window);

            //AttributeTableForm ATF = new AttributeTableForm(m_Application, m_Application.MapControl.Map, player, "");
            //ATF.Show(m_Application.MainForm as IWin32Window);
        }
    }
}
