using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 剔除线、面要素中的重叠节点（相邻节点，且坐标完全一致）
    /// </summary>
    public class RemoveOverlapNodeCmd : SMGI.Common.SMGICommand
    {
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
            var frm = new LayerSelectForm(m_Application, false, true, true);
            frm.StartPosition = FormStartPosition.CenterParent;

            if (frm.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                m_Application.EngineEditor.StartOperation();

                string msg = "";

                using (WaitOperation wo = GApplication.Application.SetBusy())
                {
                    List<IFeatureClass> fcList = new List<IFeatureClass>();
                    foreach (var layer in frm.SelectFeatureLayerList)
                    {
                        IFeatureClass fc = layer.FeatureClass;
                        if (!fcList.Contains(fc))
                            fcList.Add(fc);
                    }

                    msg = RemoveOverlapNode(fcList, wo);
                }

                m_Application.EngineEditor.StopOperation("重叠节点处理");

                if (msg == "")
                {
                    MessageBox.Show("重叠节点处理完成，未发现包含重叠节点的要素！");
                }
                else
                {
                    MessageBox.Show("重叠节点处理完成，处理情况如下:\n" + msg);
                }
                
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

        public static string RemoveOverlapNode(List<IFeatureClass> fcList, WaitOperation wo = null)
        {
            string msg = "";

            foreach (var fc in fcList)
            {
                List<int> oidList = new List<int>();//该要素类中被处理的要素

                if (fc.ShapeType == esriGeometryType.esriGeometryPoint)
                    continue;

                IQueryFilter qf = new QueryFilterClass();
                if (fc.HasCollabField())//已删除的要素不参与
                {
                    qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                }

                IFeatureCursor feCursor = fc.Search(qf, true);
                IFeature fe;
                while ((fe = feCursor.NextFeature()) != null)
                {
                    if (fe.Shape == null || fe.Shape.IsEmpty)
                        continue;

                    if (wo != null)
                        wo.SetText(string.Format("正在检查要素类【{0}】中的要素【{1}】......", fc.AliasName, fe.OID));

                    if (HasOverlapNode(fe.Shape))
                    {
                        oidList.Add(fe.OID);
                    }
                }
                Marshal.ReleaseComObject(feCursor);

                if (oidList.Count > 0)
                {
                    foreach (var oid in oidList)
                    {
                        if (wo != null)
                            wo.SetText(string.Format("正在处理要素类【{0}】中的要素【{1}】......", fc.AliasName, oid));

                        RemoveOvelapNode(fc.GetFeature(oid));
                    }

                    string info = string.Format("要素类【{0}】中被处理的要素数量为：【{1}】", fc.AliasName, oidList.Count);
                    if (msg != "")
                        msg += "\n";
                    msg += info;
                }
            }

            return msg;
        }

        /// <summary>
        /// 若oldGeo包含重复节点，则返回True，否则返回False
        /// </summary>
        /// <param name="oldGeo"></param>
        /// <returns></returns>
        public static bool HasOverlapNode(IGeometry geo)
        {
            if (geo.GeometryType == esriGeometryType.esriGeometryPolygon || geo.GeometryType == esriGeometryType.esriGeometryPolyline)
            {
                IGeometryCollection geoCol = geo as IGeometryCollection;
                for (int i = 0; i < geoCol.GeometryCount; i++)
                {
                    IPointCollection ptCol = geoCol.get_Geometry(i) as IPointCollection;
                    for (int k = 1; k < ptCol.PointCount; ++k)
                    {
                        IPoint p1 = ptCol.get_Point(k - 1);
                        IPoint p = ptCol.get_Point(k);
                        if (p1.X == p.X && p1.Y == p.Y)//该节点与上一个节点坐标相同
                        {
                            return true;
                        }
                    }

                }
            }

            return false;
        }

        /// <summary>
        /// 剔除几何中的重复节点
        /// </summary>
        /// <param name="geo"></param>
        public static void RemoveOvelapNode(IFeature fe)
        {
            IGeometry newGeo = fe.ShapeCopy;
            if (newGeo.GeometryType == esriGeometryType.esriGeometryPolygon || newGeo.GeometryType == esriGeometryType.esriGeometryPolyline)
            {
                IGeometryCollection geoCol = newGeo as IGeometryCollection;
                for (int i = 0; i < geoCol.GeometryCount; i++)
                {
                    IPointCollection ptCol = geoCol.get_Geometry(i) as IPointCollection;
                    for (int k = 1; k < ptCol.PointCount; ++k)
                    {
                        IPoint p1 = ptCol.get_Point(k - 1);
                        IPoint p = ptCol.get_Point(k);
                        if (p1.X == p.X && p1.Y == p.Y)//该节点与上一个节点坐标相同
                        {
                            ptCol.RemovePoints(k, 1);//移除重复节点
                            k--;
                        }
                    }

                }

                fe.Shape = newGeo;
                fe.Store();
            }
        }
    }
}
