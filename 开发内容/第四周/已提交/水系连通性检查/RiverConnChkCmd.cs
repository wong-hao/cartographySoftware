using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.IO;
using System.Xml;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 若符合条件的水系起点或终点，某一端与其它水系的端点相连，且不存在与之匹配的水系，则视为该段水系不连通，将该段水系加入质检结果
    /// </summary>
    public class RiverConnChkCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing;
            }
        }

        public List<string> RiverLayerNames = new List<string>();

        private List<string> facilityNames = new List<string>();
        private List<string> roadNames = new List<string>();
        private string configuartionFile = "FacilityRoadAreaMapping.xml";

        // Load names from XML configuration
        private void LoadConfigurations()
        {
            string cfgFileName = m_Application.Template.Root + "\\" + configuartionFile;
            if (!System.IO.File.Exists(cfgFileName))
                return;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(cfgFileName);

            facilityNames = LoadFacilityNamesFromNode(xmlDoc, "Facilities");
            roadNames = LoadRoadNamesFromNode(xmlDoc, "Roads");
        }

        private List<string> LoadFacilityNamesFromNode(XmlDocument doc, string nodeName)
        {
            List<string> facilityNames = new List<string>();

            XmlNodeList facilityNodes = doc.SelectNodes("/Mapping/" + nodeName + "/Facility");
            foreach (XmlNode node in facilityNodes)
            {
                facilityNames.Add(node.InnerText);
            }

            return facilityNames;
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

        public override void OnClick()
        {
            // Load names from XML configuration
            LoadConfigurations();

            RiverConnChkFrm frm = new RiverConnChkFrm();

            // 合并 facilityNames 和 roadNames 到 RiverLayerNames
            RiverLayerNames.Clear();
            RiverLayerNames.AddRange(facilityNames);
            RiverLayerNames.AddRange(roadNames); 
            
            frm.RiverLayerNames = RiverLayerNames;

            if (DialogResult.OK == frm.ShowDialog())
            {
                string riverLayerName = frm.RiverLayerName;

                IFeatureLayer layer = (m_Application.Workspace.LayerManager.GetLayer(
                    l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == riverLayerName)).FirstOrDefault() as IFeatureLayer);
                if (null == layer)
                {
                    MessageBox.Show(string.Format("没有找到水系图层【{0}】!", riverLayerName), "警告", MessageBoxButtons.OK);
                    return;
                }

                IFeatureClass riverFC = layer.FeatureClass;

                string outPutFileName = frm.OutFilePath + string.Format("\\水系连通性检查_{0}.shp", riverLayerName);

                string err = "";
                using (var wo = m_Application.SetBusy())
                {

                    RiverConnChk ck = new RiverConnChk();
                    err = ck.DoCheck(outPutFileName, riverFC, wo);

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
