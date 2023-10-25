using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 
    /// </summary>
    public class WorkspaceSymbolizationCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null;
            }
        }

        public override void OnClick()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "mxd文件(*.mxd)|*.mxd";
            openFileDialog1.Title = "请选择一个符号模板";
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            //读取符号模板
            IMapDocument pMapDoc = new MapDocumentClass();
            pMapDoc.Open(openFileDialog1.FileName, "");
            if (pMapDoc.MapCount == 0)
            {
                MessageBox.Show("地图模板不能为空！");

                return;
            }
            IMap templateMap = pMapDoc.get_Map(0);

            List<string> layerNameList = new List<string>();
            using (var wo = m_Application.SetBusy())
            {

                for (int index = templateMap.LayerCount - 1; index >= 0; index--)
                {
                    var templateLyr = templateMap.get_Layer(index) as IFeatureLayer;

                    List<IFeatureLayer> feLayerList = new List<IFeatureLayer>();
                    GetFeatureLayers(templateLyr, ref feLayerList);
                    foreach (var item in feLayerList)
                    {
                        wo.SetText(string.Format("正在匹配图层【{0}】......", item.Name));

                        //在workspcace中找到与之对应的图层
                        var lyr = m_Application.Workspace.LayerManager.GetLayer(
                        l => (l is IFeatureLayer) && ((l as IFeatureLayer).Name.ToUpper().Trim() == item.Name.ToUpper().Trim())).FirstOrDefault() as IFeatureLayer;
                        if (lyr == null)
                            continue;

                        #region 符号设置
                        IFeatureRenderer render = (item as IGeoFeatureLayer).Renderer as IFeatureRenderer;
                        (lyr as IGeoFeatureLayer).Renderer = render;
                        #endregion

                        #region 可见性、可选择性等设置
                        lyr.Visible = templateLyr.Visible;
                        lyr.Selectable = templateLyr.Selectable;
                        (lyr as IFeatureLayerDefinition).DefinitionExpression = (templateLyr as IFeatureLayerDefinition).DefinitionExpression;
                        #endregion

                        #region 图层顺序调整：关联图层按模板中的顺序排列，非关联图层放置在关联图层后面
                        m_Application.Workspace.Map.MoveLayer(lyr, 0);
                        #endregion

                        layerNameList.Add(lyr.Name);
                    }
                }

                m_Application.TOCControl.ActiveView.Refresh();
                m_Application.TOCControl.Update();

            }

            if (layerNameList.Count == 0)
            {
                MessageBox.Show("符号模板与当前工作空间不存在同名图层，无更新图层！");
            }
            else
            {
                string info = "";
                foreach (var item in layerNameList)
                {
                    if (info != "")
                        info += "、";

                    info += item;
                }

                MessageBox.Show(string.Format("导入符号模板成功，更新的图层如下：\n{0}", info));
            }
        }

        private void GetFeatureLayers(ILayer layer, ref List<IFeatureLayer> feLayerList)
        {
            if (layer is IGroupLayer)
            {
                var pGroupLayer = layer as ICompositeLayer;
                for (int i = 0; i < pGroupLayer.Count; i++)
                {
                    ILayer subLayer = pGroupLayer.Layer[i];
                    GetFeatureLayers(subLayer, ref feLayerList);
                }
            }
            else
            {
                if (layer is IFeatureLayer)
                {
                    var fc = (layer as IFeatureLayer).FeatureClass;
                    if (null == fc) 
                        return;

                    feLayerList.Add(layer as IFeatureLayer);
                }
            }
        }
    }
}
