using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    public class cmdSplitPolygon : SMGICommand
    {
        /// <summary>
        /// 根据当前选择的线要素,分割所有与其相交的所有面要素(仅可见)
        /// </summary>
        public cmdSplitPolygon()
        {
            m_caption = "分割多边形";
            m_toolTip = "分割多边形";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }
        
        public override void OnClick()
        {
            var map = m_Application.ActiveView.FocusMap;
            var selection = map.FeatureSelection;
            if (map.SelectionCount == 1)
            {
                IEnumFeature selectEnumFeature = (selection as MapSelection) as IEnumFeature;
                selectEnumFeature.Reset();
                IFeature fe = selectEnumFeature.Next();

                if (fe.Shape.GeometryType != esriGeometryType.esriGeometryPolyline)
                {
                    MessageBox.Show("请先选择一个线要素");
                    return;
                }

                splitPolygon(fe);

                map.ClearSelection();
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            }
            else if(map.SelectionCount >1)
            {
                IList<IFeature> splitFeatures = new List<IFeature>();
                IEnumFeature selectEnumFeature = (selection as MapSelection) as IEnumFeature;
                selectEnumFeature.Reset();
                IFeature fe = selectEnumFeature.Next();

                while (fe!=null&&fe.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                {
                    splitFeatures.Add(fe);
                    fe = selectEnumFeature.Next();
                }

                if (splitFeatures.Count > 0)
                {
                    splitPolygon(splitFeatures); 
                }
                map.ClearSelection();
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);

            }
        }

        private void splitPolygon(IFeature splitFeature)
        {
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && l.Visible && (l as IGeoFeatureLayer).FeatureClass != null && (l as IGeoFeatureLayer).FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon;
            })).ToArray();

            ISpatialFilter qf;
            IFeatureCursor cursor;
            IFeature fe;
            var splitShape = splitFeature.ShapeCopy as IPolyline;
            ITopologicalOperator2 pTopo = (ITopologicalOperator2)splitShape;
            pTopo.IsKnownSimple_2 = false;
            pTopo.Simplify();


            IEngineEditLayers editLayer = m_Application.EngineEditor as IEngineEditLayers;
            foreach (var item in lyrs)
            {
                IFeatureClass fc = (item as IFeatureLayer).FeatureClass;
                if (fc.ShapeType != esriGeometryType.esriGeometryPolygon || !editLayer.IsEditable(item as IFeatureLayer))
                {
                    continue;
                }

                List<int> oidList = new List<int>();//当前图层中参与到该操作的要素OID集合（删除要素除外）

                qf = new SpatialFilter();
                qf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                qf.Geometry = splitShape as IGeometry;
                qf.GeometryField = "SHAPE";
                if (fc.HasCollabField())
                {
                    qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;//已删除的要素不参与
                }

                cursor = fc.Search(qf, true);
                while ((fe = cursor.NextFeature()) != null)
                {
                    oidList.Add(fe.OID);
                }
                Marshal.ReleaseComObject(cursor);

                foreach (var oid in oidList)
                {
                    fe = fc.GetFeature(oid);

                    try
                    {
                        m_Application.EngineEditor.StartOperation();

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
                        

                        IFeatureEdit feEdit = (IFeatureEdit)fe;
                        var feSet = feEdit.Split(splitShape);
                        if (feSet != null)
                        {
                            feSet.Reset();

                            List<IFeature> flist = new List<IFeature>();
                            int maxIndex = -1;
                            double maxAera = 0;
                            while (true)
                            {
                                IFeature f = feSet.Next() as IFeature;
                                if (f == null)
                                {
                                    break;
                                }

                                if ((f.Shape as IArea).Area > maxAera)
                                {
                                    maxAera = (f.Shape as IArea).Area;
                                    maxIndex = flist.Count();
                                }
                                flist.Add(f);

                            }

                            if (cmdUpdateRecord.EnableUpdate && guidIndex != -1)
                            {
                                for (int k = 0; k < flist.Count(); ++k)
                                {
                                    if (maxIndex == k)
                                    {
                                        if (smgiver >= 0 || smgiver == cmdUpdateRecord.EditState)//服务器中下载下来的要素，在打断等操作时，最大部分的协同版本号应该修改（这样才能更新服务器）2017.06.27
                                        {
                                            flist[k].set_Value(verIndex, cmdUpdateRecord.EditState);
                                        }

                                        flist[k].set_Value(guidIndex, collGUID);

                                        flist[k].Store();

                                        break;
                                    }
                                }

                            }

                        }

                        m_Application.EngineEditor.StopOperation("分割多边形");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.Message);
                        System.Diagnostics.Trace.WriteLine(ex.Source);
                        System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                        m_Application.EngineEditor.AbortOperation();

                        MessageBox.Show(ex.Message);
                    }
                }


            }

            m_Application.ActiveView.Refresh();

        }

        private void splitPolygon(IList<IFeature> splitFeatures)
        {
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && l.Visible && (l as IGeoFeatureLayer).FeatureClass != null && (l as IGeoFeatureLayer).FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon;
            })).ToArray();

            ISpatialFilter qf;
            IFeatureCursor cursor;
            IFeature fe;

            IPolyline splitShape = MergePolyline2(splitFeatures);
            //var splitShape = splitFeature.ShapeCopy as IPolyline;
            ITopologicalOperator2 pTopo = (ITopologicalOperator2)splitShape;
            pTopo.IsKnownSimple_2 = false;
            pTopo.Simplify();


            IEngineEditLayers editLayer = m_Application.EngineEditor as IEngineEditLayers;
            foreach (var item in lyrs)
            {
                IFeatureClass fc = (item as IFeatureLayer).FeatureClass;
                if (fc.ShapeType != esriGeometryType.esriGeometryPolygon || !editLayer.IsEditable(item as IFeatureLayer))
                {
                    continue;
                }

                List<int> oidList = new List<int>();//当前图层中参与到该操作的要素OID集合（删除要素除外）

                qf = new SpatialFilter();
                qf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                qf.Geometry = splitShape as IGeometry;
                qf.GeometryField = "SHAPE";
                if (fc.HasCollabField())
                {
                    qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;//已删除的要素不参与
                }

                cursor = fc.Search(qf, true);
                while ((fe = cursor.NextFeature()) != null)
                {
                    oidList.Add(fe.OID);
                }
                Marshal.ReleaseComObject(cursor);

                foreach (var oid in oidList)
                {
                    fe = fc.GetFeature(oid);

                    try
                    {
                        m_Application.EngineEditor.StartOperation();

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


                        IFeatureEdit feEdit = (IFeatureEdit)fe;
                        var feSet = feEdit.Split(splitShape);
                        if (feSet != null)
                        {
                            feSet.Reset();

                            List<IFeature> flist = new List<IFeature>();
                            int maxIndex = -1;
                            double maxAera = 0;
                            while (true)
                            {
                                IFeature f = feSet.Next() as IFeature;
                                if (f == null)
                                {
                                    break;
                                }

                                if ((f.Shape as IArea).Area > maxAera)
                                {
                                    maxAera = (f.Shape as IArea).Area;
                                    maxIndex = flist.Count();
                                }
                                flist.Add(f);

                            }

                            if (cmdUpdateRecord.EnableUpdate && guidIndex != -1)
                            {
                                for (int k = 0; k < flist.Count(); ++k)
                                {
                                    if (maxIndex == k)
                                    {
                                        if (smgiver >= 0 || smgiver == cmdUpdateRecord.EditState)//服务器中下载下来的要素，在打断等操作时，最大部分的协同版本号应该修改（这样才能更新服务器）2017.06.27
                                        {
                                            flist[k].set_Value(verIndex, cmdUpdateRecord.EditState);
                                        }

                                        flist[k].set_Value(guidIndex, collGUID);

                                        flist[k].Store();

                                        break;
                                    }
                                }

                            }

                        }

                        m_Application.EngineEditor.StopOperation("分割多边形");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.Message);
                        System.Diagnostics.Trace.WriteLine(ex.Source);
                        System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                        m_Application.EngineEditor.AbortOperation();

                        MessageBox.Show(ex.Message);
                    }
                }


            }

            m_Application.ActiveView.Refresh();

        }

        public IPolyline MergePolyline(IList<IPolyline> pls)
        {
            if (pls.Count == 0)
                return null;

            IGeometry outGeometry = new PolylineClass();

            outGeometry.SpatialReference = pls[0].SpatialReference;

            IGeometryCollection geometryCollection = outGeometry as IGeometryCollection;

            foreach(IPolyline pl in pls)
            {
                object missing = Type.Missing;
                IGeometry igeo = pl as IGeometry;
                geometryCollection.AddGeometry(igeo, ref missing, ref missing);
            }
            return geometryCollection as IPolyline;
        }

        public IPolyline MergePolyline2(IList<IFeature> fetures)
        {
            if (fetures.Count == 0)
                return null;
            else if (fetures.Count == 1)
                return fetures[0].ShapeCopy as IPolyline;
            else
            {
                IGeometry outGeometry = fetures[0].ShapeCopy;
                
                for (int i = 1; i < fetures.Count; i++)
                {
                    IGeometry geo = fetures[i].ShapeCopy;
                    ITopologicalOperator feaTopo = geo as ITopologicalOperator;
                    outGeometry = feaTopo.Union(outGeometry);
                    //feaTopo.Simplify();                   
                }                
                return outGeometry as IPolyline;
            } 
        }

       
    }
}
