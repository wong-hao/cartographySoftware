using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using SMGI.Plugin.DCDProcess;
using SMGI.Plugin.DCDProcess.DataProcess;


namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 悬挂点检查
    /// </summary>
    public class TangleChkCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;
            }
        }
        public override void OnClick()
        {
            var layerSelector = new LayerSelectForm(m_Application)
            {
                GeoTypeFilter = esriGeometryType.esriGeometryPolyline
            };
            if (layerSelector.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            var curLayer = layerSelector.pSelectLayer;

            if (curLayer == null)
            {
                MessageBox.Show("没有找到需要检查的图层");
                return;
            }

            TangleCheck check = new TangleCheck(double.Parse(layerSelector.tbValue.Text));

            var fcls = (curLayer as IFeatureLayer).FeatureClass;

            ResultMessage rm;
            using (var wo = m_Application.SetBusy())
            {
                rm = check.Check(fcls, (fcls as IGeoDataset).Extent);
                if (rm.stat == ResultState.Ok)
                {
                    rm = check.SaveResult(OutputSetup.GetDir());
                }

                if (rm.stat != ResultState.Ok)
                {
                    MessageBox.Show(rm.msg);
                    return;
                }

                if (check.Count == 0)
                {
                    MessageBox.Show("检查完成,未发现悬挂点！");
                }
                else
                {
                    if (MessageBox.Show("检查完成！是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        var strs = (string[])rm.info;
                        CheckHelper.AddTempLayerFromSHPFile(m_Application.Workspace.LayerManager.Map, strs[0]);
                    }
                }
            }
        }
    }
}
