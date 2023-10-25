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
    /// 水系面综合化简后，河流线及水系结构线应仍保持与水系面的touch关系
    /// </summary>
    public class HYDLTouchHYDACmd : SMGI.Common.SMGICommand
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
            string hydlLayerName = "HYDL";
            string hydaLayerName = "HYDA";
            IFeatureClass hydlFC = (m_Application.Workspace.LayerManager.GetLayer(
                    l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == hydlLayerName)).FirstOrDefault() as IFeatureLayer).FeatureClass;
            if (hydlFC == null)
            {
                MessageBox.Show(string.Format("没找到水系线层【{0}】", hydlLayerName));
                return;
            }
            IFeatureClass hydaFC = (m_Application.Workspace.LayerManager.GetLayer(
                    l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == hydaLayerName)).FirstOrDefault() as IFeatureLayer).FeatureClass;
            if (hydaFC == null)
            {
                MessageBox.Show(string.Format("没找到水系面层【{0}】", hydaLayerName));
                return;
            }

            m_Application.EngineEditor.StartOperation();

            string outputFilePath = OutputSetup.GetDir() + "\\ProcessInfo";
            if (!Directory.Exists(outputFilePath))
                Directory.CreateDirectory(outputFilePath);
            string outputFileName = outputFilePath + string.Format("\\水系线处理情况.shp");


            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                HYDLTouchHYDA obj = new HYDLTouchHYDA();
                err = obj.Process(hydaFC, hydlFC, outputFileName, wo);
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

            m_Application.EngineEditor.StopOperation("水系线拓扑关系纠正");

            

        }
    }
}
