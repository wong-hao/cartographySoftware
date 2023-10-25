using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common.Algrithm;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 检索河流树种长度（树中最长路径长度）小于指定阈值的孤立河流
    /// </summary>
    public class CheckIsolatedRiverCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null;
            }
        }

        public override void OnClick()
        {
            if (m_Application.MapControl.Map.ReferenceScale == 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;
            }

            IGeoFeatureLayer hydlLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "河流");
            })).ToArray().First() as IGeoFeatureLayer;
            if (hydlLyr == null)
            {
                MessageBox.Show("缺少河流要素类！");
                return;
            }
            IFeatureClass fc = hydlLyr.FeatureClass;

            CheckIsolatedRiverFrm frm = new CheckIsolatedRiverFrm();
            if (DialogResult.OK == frm.ShowDialog())
            {
                string outPutFileName = OutputSetup.GetDir() + string.Format("\\孤立河流检查.shp");
                double tol = frm.ThresholdValue * m_Application.MapControl.Map.ReferenceScale * 0.001;
                IPolygon extPlg = frm.RangeGeometry;


                string err = "";
                using (var wo = m_Application.SetBusy())
                {
                    err = DoCheck(outPutFileName, fc, tol, extPlg, wo,frm.SQLText);

                    if (err == "")
                    {
                        IFeatureClass errFC = CheckHelper.OpenSHPFile(outPutFileName);
                        int count = errFC.FeatureCount(null);
                        if (count > 0)
                        {
                            if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                        }
                        else
                        {
                            MessageBox.Show("检查完毕，没有发现小于指标的孤立河流！");
                        }
                    }
                    else
                    {
                        MessageBox.Show(err);
                    }
                }
            }
        }

        public static string DoCheck(string resultSHPFileName, IFeatureClass fc, double tol, 
            IPolygon extPlg = null, WaitOperation wo = null,String SQLText= "")
        {
            string err = "";

            try
            {
                IPolyline extPl = null;//范围面的边界线
                if (extPlg != null)
                {
                    if (extPlg.SpatialReference.Name != (fc as IGeoDataset).SpatialReference.Name)
                    {
                        //投影变换
                        extPlg.Project((fc as IGeoDataset).SpatialReference);
                    }

                    ITopologicalOperator to = extPlg as ITopologicalOperator;
                    to.Simplify();

                    extPl = to.Boundary as IPolyline;
                }

                //建立结果文件
                ShapeFileWriter resultFile = new ShapeFileWriter();
                Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                fieldName2Len.Add("最长路径", 20);
                fieldName2Len.Add("检查项", 40);
                resultFile.createErrorResutSHPFile(resultSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);

                IQueryFilter qf = new QueryFilterClass();
                //qf.WhereClause = "hgb<210400";
                qf.WhereClause = SQLText;
                // if (fc.HasCollabField())
                    qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;
                IFeatureCursor feCursor = fc.Search(qf, false);
                int feCount = fc.FeatureCount(qf);

                HydroAlgorithm.HydroGraph hydroGraph = new HydroAlgorithm.HydroGraph();

                IFeature fe = null;
                while ((fe = feCursor.NextFeature()) != null)
                {
                    if(wo != null)
                        wo.SetText("正在读取数据【" + fe.OID.ToString() + "】......");
                    hydroGraph.Add(fe);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);

                if (wo != null)
                    wo.SetText("正在获取Tree列表......");
                var hydroTrees = hydroGraph.BuildTrees();

                if (wo != null)
                    wo.SetText("正在分析......");
                foreach (var tree in hydroTrees)
                {
                    //获取Tree的Path列表
                    var hydroPaths = tree.BuildHydroPaths();
                    if (hydroPaths.Count == 0)
                        continue;

                    //计算Tree中各河流长度
                    foreach (var path in hydroPaths)
                    {
                        path.CalLength();
                    }

                    //对Tree中河流长度排序
                    hydroPaths.Sort();

                    var maxPath = hydroPaths.Last();
                    if (maxPath.Lenght < tol)
                    {
                        IPolyline geo = null;
                        foreach (var path in hydroPaths)
                        {
                            foreach (var edge in path.Edges)
                            {
                                if (geo == null)
                                {
                                    geo = edge.Feature.ShapeCopy as IPolyline;
                                }
                                else
                                {
                                    ITopologicalOperator topologicalOperator = geo as ITopologicalOperator;
                                    geo = topologicalOperator.Union(edge.Feature.ShapeCopy) as IPolyline;
                                }
                            }

                        }

                        if (extPlg != null)
                        {
                            //是否完全落于范围面内
                            IRelationalOperator relationalOperator = extPlg as IRelationalOperator;
                            if (!relationalOperator.Contains(geo))
                            {
                                //完全包含在面内
                                continue;
                            }

                            if (extPl != null)
                            {
                                //非否与边界线有交点
                                relationalOperator = extPl as IRelationalOperator;
                                if (!relationalOperator.Disjoint(geo))
                                {
                                    //与边界线有交点
                                    continue;
                                }
                            }
                        }

                        
                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("最长路径", string.Format("{0:F}", maxPath.Lenght));
                        fieldName2FieldValue.Add("检查项", "孤立河流");

                        resultFile.addErrorGeometry(geo, fieldName2FieldValue);
                    }
                    
                }

                //保存结果文件
                resultFile.saveErrorResutSHPFile();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                err = ex.Message;
            }

            return err;
        }
    }
}
