using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public class SwitchTKCmd : SMGICommand
    {
        private static double curRefScale = 0;
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null ;
            }
        }
        
        public override void OnClick()
        {
            var map = GApplication.Application.MapControl.Map;
            if (map.ReferenceScale != 0)
            {
                curRefScale = GApplication.Application.MapControl.Map.ReferenceScale;
               map.ReferenceScale = 0;
                (map as IActiveView).Refresh();
            }
            else
            {
                map.ReferenceScale = curRefScale;
                (map as IActiveView).Refresh();
            }
          

        }
       
    }
}
