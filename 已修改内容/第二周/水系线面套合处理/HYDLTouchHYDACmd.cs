using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using SMGI.Plugin.CollaborativeWorkWithAccount.工具.水系线面套合处理;
using SMGI.Plugin.DCDProcess;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
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

        private List<string> roadNames = new List<string>();
        private List<string> areaNames = new List<string>();
        private string configuartionFile = "FacilityRoadAreaMapping.xml";

        // Load names from XML configuration
        private void LoadConfigurations()
        {
            string cfgFileName = m_Application.Template.Root + "\\" + configuartionFile;
            if (!System.IO.File.Exists(cfgFileName))
                return;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(cfgFileName);

            roadNames = LoadRoadNamesFromNode(xmlDoc, "Roads");
            areaNames = LoadAreaNamesFromNode(xmlDoc, "Areas");
        }

        private List<string> LoadRoadNamesFromNode(XmlDocument doc, string nodeName)
        {
            List<string> RoadNames = new List<string>();

            XmlNodeList facilityNodes = doc.SelectNodes("/Mapping/" + nodeName + "/Road");
            foreach (XmlNode node in facilityNodes)
            {
                RoadNames.Add(node.InnerText);
            }

            return RoadNames;
        }

        private List<string> LoadAreaNamesFromNode(XmlDocument doc, string nodeName)
        {
            List<string> AreaNames = new List<string>();

            XmlNodeList facilityNodes = doc.SelectNodes("/Mapping/" + nodeName + "/Area");
            foreach (XmlNode node in facilityNodes)
            {
                AreaNames.Add(node.InnerText);
            }

            return AreaNames;
        }

        /// <summary>
        ///     显示窗体
        /// </summary>
        private void ShowSelectionForm()
        {
            try
            {
                selectionForm = new HYDLTouchHYDAForm();
                selectionForm.roadNames = roadNames;
                selectionForm.areaNames = areaNames;
                selectionForm.currentMap = currentMap;
                selectionForm.ShowDialog();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        /// <Date>2023/8/4</Date>
        /// <Author>HaoWong</Author>
        /// <summary>
        ///     子函数，得到目前图层
        /// </summary>
        private void GetCurrentMap()
        {
            _currentMapControl = m_Application.MapControl;

            // 确保当前地图控件和地图对象不为空
            if (_currentMapControl != null) currentMap = _currentMapControl.Map;
        }

        private HYDLTouchHYDAForm selectionForm; // 窗体
        private AxMapControl _currentMapControl; // 当前的MapControl控件
        private IMap currentMap; // 当前MapControl控件中的Map对象   

        public override void OnClick()
        {
            // 获取当前地图
            GetCurrentMap();

            // 检查地图是否为空
            if (currentMap == null)
            {
                MessageBox.Show("地图未加载，请先加载地图。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadConfigurations();

            //显示窗体
            ShowSelectionForm();

            if ((selectionForm.selectedRoadLayer == null) || (selectionForm.selectedAreaLayer == null))
            {
                return;
            }

            /*string hydlLayerName = "HYDL";
            string hydaLayerName = "HYDA";
             */

            string hydlLayerName = selectionForm.selectedRoadLayer.Name;
            string hydaLayerName = selectionForm.selectedAreaLayer.Name;


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
