using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using System.IO;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 街区面综合化简后，城市道路、城际公路应仍保持与街区面的端点touch关系
    /// </summary>
    public class LRDLTouchRESACmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            string lrdlLayerName = "LRDL";
            string resaLayerName = "RESA";
            IFeatureClass lrdlFC = (m_Application.Workspace.LayerManager.GetLayer(
                    l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == lrdlLayerName)).FirstOrDefault() as IFeatureLayer).FeatureClass;
            if (lrdlFC == null)
            {
                MessageBox.Show(string.Format("没找到公路层【{0}】", lrdlLayerName));
                return;
            }
            IFeatureClass resaFC = (m_Application.Workspace.LayerManager.GetLayer(
                    l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == resaLayerName)).FirstOrDefault() as IFeatureLayer).FeatureClass;
            if (resaFC == null)
            {
                MessageBox.Show(string.Format("没找到街区面层【{0}】", resaLayerName));
                return;
            }

            m_Application.EngineEditor.StartOperation();

            string outputFilePath = OutputSetup.GetDir() + "\\ProcessInfo";
            if (!Directory.Exists(outputFilePath))
                Directory.CreateDirectory(outputFilePath);
            string outputFileName = outputFilePath + string.Format("\\道路街区处理情况.shp");


            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                LRDLTouchRESA obj = new LRDLTouchRESA();
                err = obj.Process(resaFC, lrdlFC, outputFileName, wo);
            }


            if (err != "")
            {
                m_Application.EngineEditor.AbortOperation();

                MessageBox.Show(err);
            }
            else
            {
                if (MessageBox.Show("是否加载处理情况数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    CheckHelper.AddTempLayerFromSHPFile(m_Application.Workspace.LayerManager.Map, outputFileName);
                }
            }

            m_Application.EngineEditor.StopOperation("道路街区拓扑关系纠正");

            

        }
    }
}
