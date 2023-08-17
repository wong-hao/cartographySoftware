using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using System.Windows.Forms;
namespace SMGI.Plugin.DCDProcess
{
    class RecalFullExtent:SMGIContextMenu
    {
        public RecalFullExtent()
        {
            m_caption = "重新计算范围";
            m_toolTip = "重新计算范围";
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
            IMap m = this.CurrentContextItem as IMap;
            m.RecalcFullExtent();
            
        }
    }

    public class ZoomToLayer : SMGIContextMenu
    {
        public ZoomToLayer()
        {
            m_caption = "缩放到图层";
            m_toolTip = "缩放到图层";
            m_category = "图层管理";
        }
        public override bool Enabled
        {
            get
            {
                return this.CurrentContextItem is ILayer && m_Application.LayoutState ==  LayoutState.MapControl;
            }
        }
        public override void OnClick()
        {
            ILayer l = this.CurrentContextItem as ILayer;
            m_Application.MapControl.Extent = l.AreaOfInterest;
        }
    }

    class RepresentationConvert : SMGIContextMenu
    {
        public RepresentationConvert()
        {
            m_caption = "转换制图表达";
            m_toolTip = "转换制图表达";
            m_category = "转换制图表达";
        }
        public override bool Enabled
        {
            get
            {
                return this.CurrentContextItem is IFeatureLayer 
                    && m_Application.Workspace != null 
                    && m_Application.LayoutState ==  LayoutState.MapControl;
            }
        }
        public override void OnClick()
        {
            IFeatureLayer l = this.CurrentContextItem as IFeatureLayer;
            IWorkspaceExtensionManager wem = m_Application.Workspace.EsriWorkspace as IWorkspaceExtensionManager;
            UID uid = new UIDClass();
            uid.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
            IRepresentationWorkspaceExtension rwe = wem.FindExtension(uid) as IRepresentationWorkspaceExtension;
            if (rwe.get_FeatureClassHasRepresentations(l.FeatureClass))
            {
                IEnumDatasetName names = rwe.get_FeatureClassRepresentationNames(l.FeatureClass);
                IDatasetName name = null;
                StringBuilder sb = new StringBuilder();
                
                while ((name = names.Next()) != null)
                {
                    sb.AppendLine(name.Name);
                }
                MessageBox.Show(sb.ToString());
            }
            else {
                IGeoFeatureLayer gl = l as IGeoFeatureLayer;
                IRepresentationRenderer rr = gl.Renderer as IRepresentationRenderer;
                IRepresentationClass cls = rr.RepresentationClass;
                rwe.CreateRepresentationClass(l.FeatureClass,l.FeatureClass.AliasName,"RULEID","OVERRIDE",true,cls.RepresentationRules
                    ,null);
            }
        }
    }
}
