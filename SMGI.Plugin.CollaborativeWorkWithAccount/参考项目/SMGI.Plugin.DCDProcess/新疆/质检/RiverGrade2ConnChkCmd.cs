using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.IO;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 检查某一等级水系Grade2的连通性：
    /// 若符合条件的水系起点或终点，某一端与其它水系的端点相连，且不存在与之匹配的水系（相邻水系的Grade2不小于该水系的Grade2），则视为该段水系不连通，将该段水系加入质检结果
    /// </summary>
    public class RiverGrade2ConnChkCmd : SMGI.Common.SMGICommand
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
            string roadLayerName = "HYDL";
            string gradeTwoFieldName = "GRADE2";

            IFeatureLayer layer = (m_Application.Workspace.LayerManager.GetLayer(
                    l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == roadLayerName)).FirstOrDefault() as IFeatureLayer);
            if (null == layer)
            {
                MessageBox.Show(string.Format("没有找到水系图层【{0}】!", roadLayerName), "警告", MessageBoxButtons.OK);
                return;
            }

            IFeatureClass riverFC = layer.FeatureClass;
            int gradeTwoIndex = riverFC.Fields.FindField(gradeTwoFieldName);
            if (-1 == gradeTwoIndex)
            {
                MessageBox.Show(string.Format("水系图层中没有找到几何等级字段【{0}】！", gradeTwoFieldName), "警告", MessageBoxButtons.OK);
                return;
            }

            RiverGrade2ConnChkFrm frm = new RiverGrade2ConnChkFrm(riverFC);
            if (DialogResult.OK == frm.ShowDialog())
            {
                long gradeTwo = frm.GradeTwo;
                string outPutFileName = frm.OutFilePath + string.Format("\\水系几何等级连通性检查_{0}_.shp", gradeTwo);

                string err = "";
                using (var wo = m_Application.SetBusy())
                {

                    RiverGrade2ConnChk ck = new RiverGrade2ConnChk();
                    err = ck.DoCheck(outPutFileName, riverFC, gradeTwoIndex, gradeTwo, wo);

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
