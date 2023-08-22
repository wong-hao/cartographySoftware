using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using SMGI.Plugin.DCDProcess;


namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 悬挂点检查（端点离线，点不在线上打断的情况：点所在线可能相离，也可能相交）
    /// </summary>
    public class LineHeadClosedToLineChkCmd : SMGI.Common.SMGICommand
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
            //图层选择、最大距离对话框
            var layerSelectForm = new LayerSelectForm(m_Application)
            {
                GeoTypeFilter = esriGeometryType.esriGeometryPolyline
            };
            if (layerSelectForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            
            //检查图层
            ILayer curLayer = layerSelectForm.pSelectLayer;
            if (curLayer == null)
            {
                MessageBox.Show("没有找到需要检查的图层");
                return;
            }

            //检查距离
            //超过该距离的孤立点，不认为是悬挂点
            double bigDistance = double.Parse(layerSelectForm.tbValue.Text);            
            
            //待检查要素类
            IFeatureClass fc = (curLayer as IFeatureLayer).FeatureClass;

            LineHeadClosedToLineChk check2 = new LineHeadClosedToLineChk();

            ResultMessage resultMessage;
            using (var wo = m_Application.SetBusy())
            {
                resultMessage = check2.Check(fc, (fc as IGeoDataset).Extent,bigDistance);
                if (resultMessage.stat == ResultState.Ok)
                {
                    resultMessage = check2.SaveResult(OutputSetup.GetDir());
                    if (check2.Count == 0)
                    {
                        MessageBox.Show("检查完成,未发现悬挂点！");
                    }
                    else
                    {
                        if (MessageBox.Show("检查完成！是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            var strs = (string[])resultMessage.info;
                            CheckHelper.AddTempLayerFromSHPFile(m_Application.Workspace.LayerManager.Map, strs[0]);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(resultMessage.msg);
                }
            }
        }
    }
}
