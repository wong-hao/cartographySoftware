using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 将目标线要素端节点附近指定距离阈值内的节点删除
    /// </summary>
    public class PolylineEndPointProcessCmd : SMGI.Common.SMGICommand
    {
        public PolylineEndPointProcessCmd()
        {
            m_caption = "线节点密度处理";
            m_toolTip = "处理辅助线端点附近的节点密度";
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
            PolylineEndPointProcessForm frm = new PolylineEndPointProcessForm();
            if (DialogResult.OK != frm.ShowDialog())
                return;

            int numFeatures = 0;
            using (var wo = m_Application.SetBusy())
            {
                numFeatures = NodeProcess(frm.ObjLayer.FeatureClass, frm.FilterText, frm.ThresholdValue, wo);
            }
            if (numFeatures >=0 )
            {
                MessageBox.Show(string.Format("处理完毕，本次被处理的要素量为【{0}】！", numFeatures));
            }

            
        }

        private int NodeProcess(IFeatureClass fc, string filter, double distance, WaitOperation wo = null)
        {
            int num = 0;

            try
            {
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = filter;

                IFeatureCursor feCursor = fc.Search(qf, false);
                IFeature fe = null;
                while ((fe = feCursor.NextFeature()) != null)
                {
                    IPolyline pl = fe.Shape as IPolyline;
                    if (pl == null)
                        continue;


                    bool bModified = false;
                    IGeometryCollection geoCol = pl as IGeometryCollection;
                    for (int i = 0; i < geoCol.GeometryCount; i++)
                    {
                        IPointCollection ptCol = geoCol.get_Geometry(i) as IPointCollection;

                        IPoint startPt = ptCol.get_Point(0);
                        IPoint endPt = ptCol.get_Point(ptCol.PointCount-1);

                        for (int k = 1; k < ptCol.PointCount -1 ; ++k)//剔除起始节点附近的多余点
                        {
                            IPoint p = ptCol.get_Point(k);
                            if ((startPt as IProximityOperator).ReturnDistance(p) <= distance)
                            {
                                ptCol.RemovePoints(k, 1);//移除节点
                                k--;

                                bModified = true;
                            }
                        }

                        for (int k = ptCol.PointCount -2; k > 0; --k)//剔除终止节点附近的多余点
                        {
                            IPoint p = ptCol.get_Point(k);
                            if ((endPt as IProximityOperator).ReturnDistance(p) <= distance)
                            {
                                ptCol.RemovePoints(k, 1);//移除节点

                                bModified = true;
                            }
                        }

                    }

                    if (bModified)
                    {
                        fe.Shape = pl;
                        fe.Store();

                        num++;
                    }

                    Marshal.ReleaseComObject(fe);

                }
                Marshal.ReleaseComObject(feCursor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                num = -1;
            }

            return num;
        }
    }
}
