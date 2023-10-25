using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    class TopoHelper
    {
        TxtRecordFile errorTxtFile = null;
        public static void CreateTopology(IFeatureDataset featureDataset, IFeatureClass topoFC, string topoName, esriTopologyRuleType ruleType, double clusterTolerance, TxtRecordFile txtRecord, string errortype )
        {
            // Attempt to acquire an exclusive schema lock on the feature dataset.
            ISchemaLock schemaLock = (ISchemaLock)featureDataset;
            //try
            //{
            schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);

            // Create the topology.
            ITopologyContainer2 topologyContainer = (ITopologyContainer2)featureDataset;
            ITopology topology = topologyContainer.CreateTopology(topoName, clusterTolerance, -1, "");

            // Add feature classes and rules to the topology.
            topology.AddClass(topoFC, 5, 1, 1, false);
            AddRuleToTopology(topology, ruleType, "No Block Overlap", topoFC);

            // Get an envelope with the topology's extents and validate the topology.
            IGeoDataset geoDataset = (IGeoDataset)topology;
            IEnvelope envelope = geoDataset.Extent;
            ValidateTopology(topology, envelope);

            //    DisplayErrorFeature(topology, ruleType, envelope);
            ITopologyRule topologyRule = new TopologyRuleClass();
            topologyRule.TopologyRuleType = ruleType;
        }

        public static void CreateTopology(IFeatureDataset featureDataset, List<IFeatureClass> topoFCList, string topoName, esriTopologyRuleType ruleType, string ruleName, double clusterTolerance = -1)
        {
            // Attempt to acquire an exclusive schema lock on the feature dataset.
            ISchemaLock schemaLock = (ISchemaLock)featureDataset;

            //
            schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);

            // Create the topology.
            ITopologyContainer2 topologyContainer = (ITopologyContainer2)featureDataset;
            if (clusterTolerance == -1)
                clusterTolerance = topologyContainer.DefaultClusterTolerance;
            if ((featureDataset.Workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTTopology, topoName))
            {
                (topologyContainer.get_TopologyByName(topoName) as IDataset).Delete();
            }
            ITopology topology = topologyContainer.CreateTopology(topoName, clusterTolerance, -1, "");

            // Add feature classes and rules to the topology.
            if (topoFCList.Count == 1)
            {
                topology.AddClass(topoFCList[0], 5, 1, 1, false);
                AddRuleToTopology(topology, ruleType, ruleName, topoFCList[0]);
            }
            else if (topoFCList.Count >= 2)
            {
                topology.AddClass(topoFCList[0], 5, 1, 1, false);
                for (int i = 1; i < topoFCList.Count; i++)
                {
                    topology.AddClass(topoFCList[i], 5, 1, 1, false);
                    AddRuleToTopology(topology, ruleType, ruleName, topoFCList[0], topoFCList[i]);
                }
            }

            // Get an envelope with the topology's extents and validate the topology.
            IGeoDataset geoDataset = (IGeoDataset)topology;
            IEnvelope envelope = geoDataset.Extent;
            ValidateTopology(topology, envelope);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(topology);
        }

        public static void ValidateTopology(ITopology topology, IEnvelope envelope)
        {
            // Get the dirty area within the provided envelope.
            IPolygon locationPolygon = new PolygonClass();
            ISegmentCollection segmentCollection = (ISegmentCollection)locationPolygon;
            segmentCollection.SetRectangle(envelope);
            IPolygon polygon = topology.get_DirtyArea(locationPolygon);

            // If a dirty area exists, validate the topology.    
            if (!polygon.IsEmpty)
            {
                // Define the area to validate and validate the topology.
                IEnvelope areaToValidate = polygon.Envelope;
                IEnvelope areaValidated = topology.ValidateTopology(areaToValidate);
            }
        }

        public static void AddRuleToTopology(ITopology topology, esriTopologyRuleType ruleType, String ruleName, IFeatureClass featureClass)
        {
            // Create a topology rule.
            ITopologyRule topologyRule = new TopologyRuleClass();
            topologyRule.TopologyRuleType = ruleType;
            topologyRule.Name = ruleName;
            topologyRule.OriginClassID = featureClass.FeatureClassID;
            topologyRule.AllOriginSubtypes = true;
            // Cast the topology to the ITopologyRuleContainer interface and add the rule.
            ITopologyRuleContainer topologyRuleContainer = (ITopologyRuleContainer)topology;
            if (topologyRuleContainer.get_CanAddRule(topologyRule))
            {
                topologyRuleContainer.AddRule(topologyRule);
            }
            else
            {
                throw new ArgumentException("无法识别拓扑规则！");
            }
        }

        public static void AddRuleToTopology(ITopology topology, esriTopologyRuleType ruleType, String ruleName, IFeatureClass featureClass, IFeatureClass ofeatureClass)
        {
            // Create a topology rule.
            ITopologyRule topologyRule = new TopologyRuleClass();
            topologyRule.TopologyRuleType = ruleType;
            topologyRule.Name = ruleName;
            topologyRule.OriginClassID = featureClass.FeatureClassID;
            topologyRule.OriginSubtype = 1;
            topologyRule.AllOriginSubtypes = true;
            topologyRule.DestinationClassID = ofeatureClass.FeatureClassID;
            topologyRule.DestinationSubtype = 1;
            topologyRule.AllDestinationSubtypes = true;
            // Cast the topology to the ITopologyRuleContainer interface and add the rule.
            ITopologyRuleContainer topologyRuleContainer = (ITopologyRuleContainer)topology;
            if (topologyRuleContainer.get_CanAddRule(topologyRule))
            {
                topologyRuleContainer.AddRule(topologyRule);
            }
            else
            {
                throw new ArgumentException("无法识别拓扑规则！");
            }
        }





    }
}
