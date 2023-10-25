using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 面状间距快速调整
    /// </summary>
    public class PolygonGapAdjustmentCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing &&
                       m_Application.Workspace.Map.SelectionCount > 0; ;
            }
        }

        public override void OnClick()
        {
            PolygonGapAdjustmentForm frm = new PolygonGapAdjustmentForm(m_Application);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            IFeatureLayer feLayer = frm.ObjFeatureLayer;

            try
            {
                m_Application.EngineEditor.StartOperation();

                List<int> modifiedFeOIDList = new List<int>();
                foreach (var kv in frm.Fe2BufferGeo)
                {
                    IFeature currentFe = kv.Key;

                    ISpatialFilter sf = new SpatialFilterClass();
                    IGeometry bufferShape = kv.Value;
                    sf.Geometry = bufferShape;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    if (feLayer.FeatureClass.HasCollabField())
                        sf.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                    IFeatureCursor cursor = feLayer.FeatureClass.Search(sf, false);
                    IFeature overlapFe = null;
                    while ((overlapFe = cursor.NextFeature()) != null)
                    {
                        if (frm.Fe2BufferGeo.ContainsKey(overlapFe))
                            continue;

                        ITopologicalOperator op = overlapFe.ShapeCopy as ITopologicalOperator;
                        IGeometry newShape = op.Difference(bufferShape);
                        if (!newShape.IsEmpty)
                        {
                            overlapFe.Shape = newShape;
                            overlapFe.Store();
                        }
                        else
                        {
                            overlapFe.Delete();
                        }

                        if (!modifiedFeOIDList.Contains(overlapFe.OID))
                            modifiedFeOIDList.Add(overlapFe.OID);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
                }//foreach
                
                //清理所选要素
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                m_Application.MapControl.Map.ClearSelection();

                foreach (var oid in modifiedFeOIDList)
                {
                    IFeature f = feLayer.FeatureClass.GetFeature(oid);
                    if (f != null)
                    {
                        //选择要素
                        m_Application.MapControl.Map.SelectFeature(feLayer, f);
                    }
                }
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, m_Application.MapControl.ActiveView.Extent);

                MessageBox.Show(string.Format("完成,目标图层【{0}】中受影响的要素个数为【{1}】", feLayer.Name, modifiedFeOIDList.Count), "提示", MessageBoxButtons.OK);


                m_Application.EngineEditor.StopOperation("面状间距快速调整");
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
}
