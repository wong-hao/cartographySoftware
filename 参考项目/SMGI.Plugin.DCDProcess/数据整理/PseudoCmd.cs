using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using System.Runtime.InteropServices;
using SMGI.Common;

namespace SMGI.Plugin.DCDProcess
{
    public class PseudoCmd : SMGI.Common.SMGICommand
    {
        public const double Tolerance = 0.0001;

        /// <summary>
        /// 伪节点处理工具（拓扑）
        /// </summary>
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
            var layerSelector = new LayerSelectWithFiedsForm(m_Application)
            {
                GeoTypeFilter = esriGeometryType.esriGeometryPolyline
            };

            if (layerSelector.ShowDialog() != DialogResult.OK)
                return;

            if (layerSelector.pSelectLayer == null)
                return;

            var lyr = layerSelector.pSelectLayer as IFeatureLayer;
            IFeatureClass fc = lyr.FeatureClass;

            IGeometry geom = null;
            if (layerSelector.Shapetxt != "")
            {
                string info = getRangeGeometry(layerSelector.Shapetxt, ref geom);
                if (!string.IsNullOrEmpty(info))
                {
                    MessageBox.Show(info);
                    return;
                }

                if (geom.SpatialReference.Name != (fc as IGeoDataset).SpatialReference.Name)
                    geom.Project((fc as IGeoDataset).SpatialReference);//投影变换(与输入要素类的空间参考保持一致)

            }

            if (geom == null)
            {
                MessageBox.Show("请加载作业范围面");
                return;
            }
            try
            {
                m_Application.EngineEditor.StartOperation();


                using (WaitOperation wo = GApplication.Application.SetBusy())
                {
                    PseudonodesProcess(fc, layerSelector.FieldArray, geom, wo);
                }

                m_Application.EngineEditor.StopOperation("伪节点处理");

                MessageBox.Show("伪节点处理完成");
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

        public static string getRangeGeometry(string fileName, ref IGeometry geo)
        {
            string err = "";

            IWorkspaceFactory workspaceFactory = null;
            IWorkspace workspace = null;
            try
            {
                workspaceFactory = new ShapefileWorkspaceFactory();
                workspace = workspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName(fileName), 0);
                IFeatureWorkspace featureWorkspace = workspace as IFeatureWorkspace;
                IFeatureClass shapeFC = featureWorkspace.OpenFeatureClass(System.IO.Path.GetFileName(fileName));

                //是否为多边形几何体
                if (shapeFC.ShapeType != esriGeometryType.esriGeometryPolygon)
                {
                    err = "范围文件应为多边形几何体，请重新指定范围文件！";
                    return err;
                }

                //默认为第一个要素的几何体
                IFeatureCursor featureCursor = shapeFC.Search(null, false);
                IFeature fe = featureCursor.NextFeature();
                if (fe == null || fe.Shape.IsEmpty)
                {
                    err = "范围文件包含无效多边形几何！";
                    return err;
                }
                Marshal.ReleaseComObject(featureCursor);

                geo = fe.Shape;

                if (geo.SpatialReference == null)
                {
                    err = "范围文件没有指定空间参考！";
                    return err;
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
                if (workspace != null)
                    Marshal.ReleaseComObject(workspace);

                if (workspaceFactory != null)
                    Marshal.ReleaseComObject(workspaceFactory);
            }

            return err;
        }


        public static void PseudonodesProcess(IFeatureClass lineFC, List<string> fieldNames, IGeometry rangeGeometry = null, WaitOperation wo = null)
        {
            if (lineFC == null)
                return;

            try
            {
                IQueryFilter qf = new QueryFilterClass();
                if (lineFC.HasCollabField())
                    qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                Dictionary<IPoint, KeyValuePair<int, int>> pseudoList = new Dictionary<IPoint, KeyValuePair<int, int>>();
                #region 构建拓扑，检查伪节点(不考虑属性、不考虑范围)
                if (wo != null)
                    wo.SetText(string.Format("正在对要素类【{0}】进行伪节点预处理......", lineFC.AliasName));

                pseudoList = CheckHelper.NotHavePseudonodes(lineFC, qf, Tolerance);
                #endregion

                List<List<int>> pseudoFIDList = new List<List<int>>();//待合并的要素OID集合
                #region 核查伪节点
                //核查伪节点
                IRelationalOperator relationalOperator = rangeGeometry as IRelationalOperator;
                int count = pseudoList.Count;
                foreach (var item in pseudoList)
                {
                    count--;

                    IPoint p = item.Key;
                    int fid1 = item.Value.Key;
                    int fid2 = item.Value.Value;

                    //范围判断
                    if (relationalOperator != null && !relationalOperator.Contains(p))
                        continue;//不在指定范围内


                    IFeature fe1 = lineFC.GetFeature(fid1);
                    IFeature fe2 = lineFC.GetFeature(fid2);

                    if (relationalOperator != null && !relationalOperator.Contains(fe1.Shape))
                        continue;
                    if (relationalOperator != null && !relationalOperator.Contains(fe2.Shape))
                        continue;

                    if (wo != null)
                        wo.SetText(string.Format("要素类【{0}】伪节点核查：剩余检查节点数【{1}】......", lineFC.AliasName, count));

                    //属性判断
                    bool judge = false;//是否是伪节点
                    for (int i = 0; i < fieldNames.Count; i++)
                    {
                        int FieldIndex = lineFC.FindField(fieldNames[i]);
                        if (FieldIndex != -1)
                        {
                            string valString1 = fe1.get_Value(FieldIndex).ToString().Trim();
                            string valString2 = fe2.get_Value(FieldIndex).ToString().Trim();
                            if (valString1 != valString2)
                            {
                                judge = true;
                                break;
                            }
                        }
                    }

                    if (judge == false)//要素属性都一致，伪节点
                    {
                        int bContainFIDIndex1 = -1;//fe1被pseudoFIDList中某一组所包含的索引号
                        int bContainFIDIndex2 = -1;//fe2被pseudoFIDList中某一组所包含的索引号
                        for(int i = 0; i < pseudoFIDList.Count; ++i)
                        {
                            if (bContainFIDIndex1 != -1 && bContainFIDIndex2 != -1)
                                break;

                            List<int> fidlist = pseudoFIDList[i];
                            if (fidlist.Contains(fid1))
                            {
                                bContainFIDIndex1 = i;
                            }

                            if (fidlist.Contains(fid2))
                            {
                                bContainFIDIndex2 = i;
                            }
                        }

                        if (bContainFIDIndex1 != -1 && bContainFIDIndex2 != -1 )//两个要素ID已被不同组包含，则合并两个组
                        {
                            if (bContainFIDIndex1 != bContainFIDIndex2)
                            {
                                //合并，将第二组合并进第一组
                                foreach (var fid in pseudoFIDList[bContainFIDIndex2])
                                {
                                    pseudoFIDList[bContainFIDIndex1].Add(fid);
                                }

                                //删除原第二个组
                                pseudoFIDList.RemoveAt(bContainFIDIndex2);
                            }

                        }
                        else if (bContainFIDIndex1 != -1)
                        {
                            pseudoFIDList[bContainFIDIndex1].Add(fid2);//要素1的ID已被某一组包含，则将要素2的ID加入该组
                        }
                        else if (bContainFIDIndex2 != -1)//要素2的ID已被某一组包含，则将要素1的ID加入该组
                        {
                            pseudoFIDList[bContainFIDIndex2].Add(fid1);
                        }
                        else//均没有被任何组包含，则新建组
                        {
                            List<int> fids = new List<int>();
                            fids.Add(item.Value.Key);
                            fids.Add(item.Value.Value);

                            pseudoFIDList.Add(fids);
                        }
                    }

                }
                #endregion

                #region 伪节点处理
                count = pseudoFIDList.Count;
                foreach (var item in pseudoFIDList)
                {
                    count--;

                    if (wo != null)
                        wo.SetText(string.Format("正在对要素类【{0}】进行要素合并：剩余合并单元【{1}】......", lineFC.AliasName, count));
                    

                    int maxLenFid = -1;
                    double maxLen = -1;

                    //找集合中几何长度最长的要素ID
                    foreach (var fid in item)
                    {
                        IFeature fe = lineFC.GetFeature(fid);

                        IPolyline pl = fe.Shape as IPolyline;
                        if (pl == null)
                            continue;

                        double len = pl.Length;
                        if (len > maxLen)
                        {
                            maxLen = len;
                            maxLenFid = fid;
                        }
                    }

                    //合并几何
                    IFeature maxFe = lineFC.GetFeature(maxLenFid);
                    foreach (var fid in item)
                    {
                        if (fid == maxLenFid)
                            continue;

                        IFeature fe = lineFC.GetFeature(fid);

                        //合并几何，更新最长要素
                        IGeometry newGeo = (maxFe.Shape as ITopologicalOperator).Union(fe.Shape);
                        maxFe.Shape = newGeo;
                        maxFe.Store();

                        //删除较短要素
                        fe.Delete();
                    }
                }

                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
