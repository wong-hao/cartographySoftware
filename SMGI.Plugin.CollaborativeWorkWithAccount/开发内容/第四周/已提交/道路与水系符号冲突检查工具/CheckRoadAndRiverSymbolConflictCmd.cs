using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.CartographyTools;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataSourcesFile;
using System.IO;
using System.Xml;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public class CheckRoadAndRiverSymbolConflictCmd : SMGICommand
    {
         public CheckRoadAndRiverSymbolConflictCmd()
        {
            m_caption = "道路与水系符号冲突检查工具";
            m_toolTip = "检查指定目标图层要素符号与其它相关图层符号之间的冲突情况";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;

            }
        }

        public List<string> ObjectLayerNames = new List<string>();
        public List<string> ConnLayerNames = new List<string>();

        private List<string> facilityNames = new List<string>();
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

            facilityNames = LoadFacilityNamesFromNode(xmlDoc, "Facilities");
            roadNames = LoadRoadNamesFromNode(xmlDoc, "Roads");
            areaNames = LoadAreaNamesFromNode(xmlDoc, "Areas");
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

        public override void OnClick()
        {
            if (m_Application.MapControl.Map.ReferenceScale <= 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;
            }

            ObjectLayerNames.Add("其他道路");
            ObjectLayerNames.Add("乡道");
            ObjectLayerNames.Add("国省县道");

            // Load names from XML configuration
            LoadConfigurations();

            // 合并 facilityNames, roadNames 和 areaNames 到 ConnLayerNames
            ConnLayerNames.Clear();
            ConnLayerNames.AddRange(facilityNames);
            ConnLayerNames.AddRange(roadNames);
            ConnLayerNames.AddRange(areaNames);

            CheckRoadAndRiverSymbolConflictForm frm = new CheckRoadAndRiverSymbolConflictForm(m_Application.MapControl.Map.ReferenceScale);

            frm.ObjectLayerNames = ObjectLayerNames;
            frm.ConnLayerNames = ConnLayerNames;

            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string outputFileName = frm.OutputPath + string.Format("\\符号冲突检查_{0}_{1}.shp", frm.ObjFeatureLayer.Name, DateTime.Now.ToString("yyMMdd_HHmm"));

            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                err = DoCheck(outputFileName, frm.ObjFeatureLayer, frm.ConnFeatureLayer, frm.ReferenceScale, frm.TbMinDistance, wo);

                if (err == "")
                {
                    if (File.Exists(outputFileName))
                    {
                        if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName);
                            CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                        }
                    }
                    else
                    {
                        MessageBox.Show("检查完毕！");
                    }
                }
                else
                {
                    MessageBox.Show(err);
                }
            }
        }

        public static string DoCheck(string resultSHPFileName, IFeatureLayer objFeatureLayer, IFeatureLayer connFeatureLayer, double refScale, double minDistance, WaitOperation wo = null)
        {
            string err = "";

            if (objFeatureLayer == null || connFeatureLayer == null)
                return err;

            IFeatureClass tempFC = null;
            Geoprocessor gp = null;
            try
            {
                if (wo != null)
                    wo.SetText(string.Format("正在创建临时数据库......"));
                string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
                IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);
                IFeatureWorkspace fws = ws as IFeatureWorkspace;

                if (wo != null)
                    wo.SetText(string.Format("正在进行冲突检测......"));
                gp = new Geoprocessor();
                gp.OverwriteOutput = true;
                gp.SetEnvironmentValue("workspace", ws.PathName);
                gp.SetEnvironmentValue("referenceScale", refScale);

                DetectGraphicConflict detectGraphicConflict = new DetectGraphicConflict(); 
                detectGraphicConflict.in_features = objFeatureLayer;
                detectGraphicConflict.conflict_features = connFeatureLayer;
                detectGraphicConflict.out_feature_class = string.Format("{0}_DetectGraphicConflict", objFeatureLayer.Name);
                detectGraphicConflict.conflict_distance = minDistance.ToString() + " meters";

                SMGI.Common.Helper.ExecuteGPTool(gp, detectGraphicConflict, null);

                tempFC = (ws as IFeatureWorkspace).OpenFeatureClass(string.Format("{0}_DetectGraphicConflict", objFeatureLayer.Name));


                CheckHelper.DeleteShapeFile(resultSHPFileName);
                if (tempFC != null && tempFC.FeatureCount(null) > 0)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在导出检查结果......"));
                    CheckHelper.ExportFeatureClassToShapefile(tempFC, resultSHPFileName);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                err = ex.Message;               
            }
            finally
            {
                if (tempFC != null)
                {
                    (tempFC as IDataset).Delete();
                    tempFC = null;
                }
            }

            return err;
        }

        
    }
}
