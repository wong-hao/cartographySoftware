using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using System.Xml;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public class XFCLAdjust2XXXLCmd : SMGITool
    {
        INewEnvelopeFeedback feedback = null;

        public XFCLAdjust2XXXLCmd()
        {
            m_caption = "设施套水路";
            m_category = "整理工具";
            m_toolTip = "框选线状设施1条，设施套合到水系或者道路上";
            NeedSnap = false;
        }

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

        // Check if a name is in the specified list
        private bool IsInList(string name, List<string> list)
        {
            return list.Contains(name.ToUpper());
        }

        public override bool Enabled
        {
            get
            {
                if (m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    return true;
                }
                return false;
            }
        }

        public override void OnClick()
        {
            IWorkspace workspace = m_Application.Workspace.EsriWorkspace;
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {           
            if (button == 1)
            {
                feedback = new NewEnvelopeFeedbackClass();
                IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                feedback.Display = m_Application.ActiveView.ScreenDisplay;
                feedback.Start(currentMouseCoords);
            }
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (button == 1 && feedback != null)
            {
                IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                feedback.MoveTo(currentMouseCoords);
            }
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            if (button == 1 && feedback != null)
            {
                IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                IGeometry geo = feedback.Stop();
                feedback = null;
                var spfilter = new SpatialFilterClass
                {
                    Geometry = geo,
                    SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects,
                    WhereClause = cmdUpdateRecord.CurFeatureFilter
                };

                //执行图面选择
                ISelectionEnvironment selEnv = new SelectionEnvironmentClass();
                selEnv.CombinationMethod = esriSelectionResultEnum.esriSelectionResultNew;
                IMap map = m_Application.ActiveView.FocusMap;
                map.SelectByShape(geo, selEnv, false);

                IList<IFeature> plFeatureList = new List<IFeature>();
                //统计选择线个数
                IEnumFeature selectEnumFeature = (map.FeatureSelection) as IEnumFeature;
                selectEnumFeature.Reset();
                IFeature fe = null;
                while ((fe = selectEnumFeature.Next()) != null)
                {
                    if (fe.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                        plFeatureList.Add(fe);
                }
                map.ClearSelection();

                //线要素至少2个
                if (plFeatureList.Count < 2)
                    return;
                IFeature feStruct = null;
                IFeature feRoadWater = null;

                // Load names from XML configuration
                LoadConfigurations();

                foreach (IFeature fex in plFeatureList)
                {
                    string name = ((fex.Class as IFeatureClass) as IDataset).Name;

                    // if (new List<string>() { "HFCL", "LFCL" }.Contains(name.ToUpper()))
                    if (IsInList(name, facilityNames))
                    {
                        if (feStruct == null)
                            feStruct = fex;
                        else                        
                            MessageBox.Show("设施多余1条");                        
                    }
                    // else if (new List<string>() { "LRDL", "LRRL", "HYDL" }.Contains(name.ToUpper()))
                    else if (IsInList(name, roadNames))
                    {
                        if (feRoadWater == null)
                            feRoadWater = fex;
                        else                        
                            MessageBox.Show("道路水系多余1条");
                    }
                }
                if (feStruct != null && feRoadWater != null)
                {
                    IPolyline structPL = feStruct.ShapeCopy as IPolyline;
                    IPoint fromPt = structPL.FromPoint;
                    IPoint toPt = structPL.ToPoint;

                    IPolyline roadwaterPL = feRoadWater.ShapeCopy as IPolyline;

                    //用设施的起始点，打断道路线（点不在线上，则投影）
                    bool splitH1, splitH2;
                    int newPartIndex1, newPartIndex2;
                    int newSegmentIndex1, newSegmentIndex2;
                    roadwaterPL.SplitAtPoint(fromPt, true, true, out splitH1, out newPartIndex1, out newSegmentIndex1);
                    roadwaterPL.SplitAtPoint(toPt, true, true, out splitH2, out newPartIndex2, out newSegmentIndex2);

                    if (splitH1 && splitH2)
                    {
                        var gc = roadwaterPL as IGeometryCollection;
                        if (gc.GeometryCount == 3)
                        {
                            var g = gc.Geometry[1];//要中间那段
                            IPolyline plNew = null;
                            if (g is IPolyline)
                            {
                                plNew = g as IPolyline;
                            }
                            else if (g is IPath)
                            {
                                PolylineClass plNewC = new PolylineClass();
                                var gc2 = plNewC as IGeometryCollection;
                                gc2.AddGeometry(g as IPath);
                                plNew = plNewC as IPolyline;
                            }
                            double rx = plNew.Length > structPL.Length ? (plNew.Length - structPL.Length) / structPL.Length : (structPL.Length - plNew.Length) / plNew.Length;

                            IGeometry geoNew = plNew as IGeometry;
                            geoNew.SpatialReference = structPL.SpatialReference;

                            if (Math.Abs(rx) < 0.1)
                            {
                                if (!feStruct.Shape.Equals(geoNew))
                                {
                                    m_Application.EngineEditor.StartOperation();
                                    m_Application.MapControl.FlashShape(geoNew);
                                    feStruct.Shape = geoNew;
                                    feStruct.Store();
                                    m_Application.EngineEditor.StopOperation("设施靠路水");
                                    m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
                                }
                            }
                            else
                            {
                                MessageBox.Show("设施投影后长度差别过大");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("设施可能比道路水系长");
                    }
                }

            }
        }    
    }
}
