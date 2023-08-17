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

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 检查某一等级道路Grade的连通性：
    /// 若符合条件的道路起点或终点，某一端与其它道路的端点相连，且不存在与之匹配的道路（相邻道路的Grade不小于该道路的Grade），则视为该段道路不连通，将该段道路加入质检结果
    /// </summary>
    public class RoadGradeConnChkCmd : SMGI.Common.SMGICommand
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
            string roadLayerName = "LRDL";
            string gradeFN = m_Application.TemplateManager.getFieldAliasName("GRADE", roadLayerName);

            IFeatureLayer layer = (m_Application.Workspace.LayerManager.GetLayer(
                    l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == roadLayerName)).FirstOrDefault() as IFeatureLayer);
            if (null == layer)
            {
                MessageBox.Show(string.Format("没有找到道路图层【{0}】!", roadLayerName), "警告", MessageBoxButtons.OK);
                return;
            }

            IFeatureClass roadFC = layer.FeatureClass;
            int gradeIndex = roadFC.Fields.FindField(gradeFN);
            if (-1 == gradeIndex)
            {
                MessageBox.Show(string.Format("道路图层中没有找到等级字段【{0}】！", gradeFN), "警告", MessageBoxButtons.OK);
                return;
            }

            RoadGradeConnChkFrm frm = new RoadGradeConnChkFrm(roadFC);
            if (DialogResult.OK == frm.ShowDialog())
            {
                long grade = frm.Grade;
                string outPutFileName = frm.OutFilePath + string.Format("\\道路等级连通性检查_{0}_.shp", grade);

                string err = "";
                using (var wo = m_Application.SetBusy())
                {
                    RoadGradeConnChk ck = new RoadGradeConnChk();
                    err = ck.DoCheck(outPutFileName, roadFC, gradeIndex, grade, wo);

                }

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
