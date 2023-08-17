using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DataUpdate
{
    public class CustPolygonTool : SMGICommand
    {
        Dictionary<string, List<int>> fcOidsD = new Dictionary<string, List<int>>();
        /// <summary>
        /// 以选择的多边形要素(源要素)，扣除所有可见目标面要素中与其相交的的公共部分，且源要素不删除
        /// </summary>
        public CustPolygonTool() 
        {
            m_caption = "扣面";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            fcOidsD.Clear();

            List<IFeature> featureList = new List<IFeature>();
            var map = m_Application.ActiveView.FocusMap;
            var selection = map.FeatureSelection;
            
            IEnumFeature selectEnumFeature = (selection as MapSelection) as IEnumFeature;
            selectEnumFeature.Reset();
            IFeature fe = null;
            int n = 0;
            while ((fe = selectEnumFeature.Next()) != null)
            {
                if (fe.Shape.GeometryType != esriGeometryType.esriGeometryPolygon)
                    continue;
                else
                {
                    n += 1;
                    featureList.Add(fe);
                    IFeatureClass fc = fe.Class as IFeatureClass;                    
                    string name = (fe.Class as IDataset).Name;
                    if (fcOidsD.ContainsKey(name))
                        fcOidsD[name].Add(fe.OID);
                    else
                        fcOidsD.Add(name, new List<int>() { fe.OID });                    
                }
            }
            if (n == 0)
                return;
            else 
                deleteIntersectPolygon(featureList);

            map.ClearSelection();
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                   
       }

        private void deleteIntersectPolygon(List<IFeature> polygonFeatureList)
        {
            IGeometry geox = polygonFeatureList[0].ShapeCopy;
            if(polygonFeatureList.Count>1)
            {
                for(int i=1;i<polygonFeatureList.Count;i++)
                    geox = (geox as ITopologicalOperator).Union(polygonFeatureList[i].ShapeCopy);
                (geox as ITopologicalOperator).Simplify();
            }
            IPolygon plg = geox as IPolygon; //有可能是多部件            

            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && l.Visible && (l as IGeoFeatureLayer).FeatureClass != null && (l as IGeoFeatureLayer).FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon;
            })).ToArray();
            if (lyrs.Length > 0)
            {
                IEngineEditLayers editLayer = m_Application.EngineEditor as IEngineEditLayers;
                foreach (var lyr in lyrs)
                {
                    if (!editLayer.IsEditable(lyr as IFeatureLayer))
                        continue;
                                       
                    IFeatureClass fc = (lyr as IGeoFeatureLayer).FeatureClass;
                    string name = (fc as IDataset).Name;

                    IFeatureDataset fd = fc.FeatureDataset;
                    if (fd.Workspace != m_Application.EngineEditor.EditWorkspace)//非可编辑图层，跳过
                        continue;

                    ISpatialFilter qf = new SpatialFilter();
                    qf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    qf.Geometry = plg;
                    qf.GeometryField = "SHAPE";
                    IFeatureLayerDefinition  fld = lyr as IFeatureLayerDefinition;
                    qf.WhereClause = fld.DefinitionExpression;

                    int idxDelState = -1;
                    string CollabDELSTATE = m_Application.TemplateManager.getFieldAliasName("SMGIDEL");
                    idxDelState = fc.FindField(CollabDELSTATE);

                    List<int> CustFeaIDList = new List<int>();//当前图层中参与到该扣面操作的要素OID集合（删除要素除外）

                    IFeatureCursor cursor = (lyr as IFeatureLayer).Search(qf, true);
                    IFeature fe = null;
                    while ((fe = cursor.NextFeature()) != null)
                    {
                        if(idxDelState>-1)
                        {
                            string delText = fe.get_Value(idxDelState).ToString();
                            if (delText == "是")
                                continue;
                        }
                        if (fcOidsD.ContainsKey(name)&&fcOidsD[name].Contains(fe.OID))  
                            continue;//该要素为裁切要素自身，跳过
                        else
                            CustFeaIDList.Add(fe.OID);
                    }
                    Marshal.ReleaseComObject(cursor);

                    foreach (var oid in CustFeaIDList)
                    {
                        IFeature f = fc.GetFeature(oid);
                        cust(f, plg);
                    }
                }

                m_Application.ActiveView.Refresh();
            }
            
        }

       
        private bool cust(IFeature fe, IPolygon plg)
        {
            m_Application.EngineEditor.StartOperation();

            IRelationalOperator trackRel = plg as IRelationalOperator;
            var shapePolyline = (plg as ITopologicalOperator).Boundary as IPolyline;

            if (trackRel.Contains(fe.Shape as IGeometry))
            {
                //要素被裁切面包含，则直接删除
                fe.Delete();

                m_Application.EngineEditor.StopOperation("扣面");

                return true;
            }

            try
            {
                IFeatureEdit feEdit = (IFeatureEdit)fe;

                int guidIndex = fe.Fields.FindField(cmdUpdateRecord.CollabGUID);
                int verIndex = fe.Fields.FindField(cmdUpdateRecord.CollabVERSION);

                string collGUID = "";
                if (guidIndex != -1)
                {
                    collGUID = fe.get_Value(guidIndex).ToString();
                }
                int smgiver = -999;
                if (verIndex != -1)
                {
                    int.TryParse(fe.get_Value(verIndex).ToString(), out smgiver);
                    fe.set_Value(verIndex, cmdUpdateRecord.NewState);//直接删除的标志
                }

                ISet pFeatureSet = feEdit.Split(shapePolyline);
                if (pFeatureSet != null)
                {
                    pFeatureSet.Reset();

                    List<IFeature> flist = new List<IFeature>();
                    int maxIndex = -1;
                    double maxAera = 0;
                    while (true)
                    {
                        IFeature newFe = pFeatureSet.Next() as IFeature;
                        if (newFe == null)
                        {
                            break;
                        }

                        if ((newFe.Shape as IArea).Area > maxAera)
                        {
                            maxAera = (newFe.Shape as IArea).Area;
                            maxIndex = flist.Count();
                        }
                        flist.Add(newFe);

                    }

                    for (int k = 0; k < flist.Count(); ++k)//201605
                    {
                        if (trackRel.Contains(flist[k].Shape as IGeometry))
                        {
                            if (verIndex != -1)
                            {
                                flist[k].set_Value(verIndex, cmdUpdateRecord.NewState);//201605，直接删除的标志
                            }

                            flist[k].Delete();
                        }
                        else
                        {
                            if (cmdUpdateRecord.EnableUpdate  && guidIndex != -1)
                            {
                                if (maxIndex == k)
                                {
                                    if ((smgiver >= 0 && smgiver <= m_Application.Workspace.LocalBaseVersion) || smgiver == cmdUpdateRecord.EditState)
                                    {
                                        flist[k].set_Value(verIndex, cmdUpdateRecord.EditState);
                                    }

                                    flist[k].set_Value(guidIndex, collGUID);

                                    flist[k].Store();
                                }
                            }
                        }
                    }

                }

                m_Application.EngineEditor.StopOperation("扣面");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                //MessageBox.Show(string.Format("对要素类【{0}】中的要素【{1}】进行扣面操作时失败：{2} \n", fe.Class.AliasName, fe.OID, ex.Message)); 

                m_Application.EngineEditor.AbortOperation();

                return false;
            }

            return true;

        }
    }
}
