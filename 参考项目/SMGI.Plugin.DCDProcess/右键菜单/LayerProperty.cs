using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Drawing;
namespace SMGI.Plugin.DCDProcess
{
    public class LayerProperty : SMGI.Common.SMGIContextMenu
    {
        public LayerProperty()
        {
            m_caption = "图层属性";
        }
        public override bool Enabled
        {
            get
            {
                return (this.CurrentContextItem is IFeatureLayer || this.CurrentContextItem is IRasterLayer);
            }
        }
        public override void OnClick()
        {
            ILayer layer = this.CurrentContextItem as ILayer;
            if (layer == null)
            {
                return;
            }

            if (layer is IFeatureLayer)
            {
                Helper.SetupFeatureLayerPropertySheet(layer);


                //更新图例
                IMapSurround pMapSurround = null;
                ILegend pLegend = null;
                for (int i = 0; i < (m_Application.ActiveView as IMap).MapSurroundCount; i++)
                {
                    pMapSurround = (m_Application.ActiveView as IMap).get_MapSurround(i);
                    if (pMapSurround is ILegend)
                    {
                        pLegend = pMapSurround as ILegend;
                        pLegend.AutoVisibility = true;
                        pLegend.Refresh();

                    }
                }

                m_Application.TOCControl.Update();
                m_Application.TOCControl.Refresh();

                (m_Application.ActiveView).PartialRefresh(esriViewDrawPhase.esriViewGeography, null, (m_Application.ActiveView).Extent);
            }
            else if (layer is IRasterLayer)
            {
                //RasterClassifyColorRampRendererClass;
                //RasterColormapRendererClass;
                //RasterDiscreteColorRendererClass;
                //RasterUniqueValueRendererClass;
                //RasterRGBRendererClass;
                //RasterStretchColorRampRendererClass;

                IRasterRenderer rasterRender = (layer as IRasterLayer).Renderer;
                if (!(rasterRender is IRasterRGBRenderer) && !(rasterRender is IRasterStretchColorRampRenderer))
                {
                    MessageBox.Show("栅格图层当前的渲染设置不支持！");
                    return;//暂不支持其它渲染方法的属性修改
                }

                RasterLayerPropertyForm renderFrm = new RasterLayerPropertyForm(layer as IRasterLayer);
                renderFrm.ShowDialog();
            }


        }
    }
}
