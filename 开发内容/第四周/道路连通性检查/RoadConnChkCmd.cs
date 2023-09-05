using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Data;
using System.IO;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 检查某一等级道路的连通性：
    /// 若符合条件的道路起点或终点，某一端与其它道路的端点相连，，则视为该段道路不连通，将该段道路加入质检结果
    /// </summary>
    public class RoadConnChkCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            RoadConnChkFrm frm = new RoadConnChkFrm();

            if (DialogResult.OK == frm.ShowDialog())
            {
                string roadLayerName = frm.RoadLayerName;

                IFeatureLayer layer = (m_Application.Workspace.LayerManager.GetLayer(
                    l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == roadLayerName)).FirstOrDefault() as IFeatureLayer);
                if (null == layer)
                {
                    MessageBox.Show(string.Format("没有找到道路图层【{0}】!", roadLayerName), "警告", MessageBoxButtons.OK);
                    return;
                }

                IFeatureClass roadFC = layer.FeatureClass;

                string outPutFileName = frm.OutFilePath + string.Format("\\道路连通性检查_{0}.shp", roadLayerName);

                string err = "";
                using (var wo = m_Application.SetBusy())
                {
                    RoadConnChk ck = new RoadConnChk();
                    err = ck.DoCheck(outPutFileName, roadFC, wo);

                    if (err != "")
                    {
                        MessageBox.Show(err);
                    }
                    else
                    {
                        if (File.Exists(outPutFileName))
                        {
                            IFeatureClass errFC = CheckHelper.OpenSHPFile(outPutFileName);

                            if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                        }
                        else
                        {
                            MessageBox.Show("检查完毕，没有发现错误！");
                        }
                    }
                }
            }
        }
    }
}
