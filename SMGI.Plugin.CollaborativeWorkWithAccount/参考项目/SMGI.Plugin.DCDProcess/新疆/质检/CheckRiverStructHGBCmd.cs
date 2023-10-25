using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common.Algrithm;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 检查河流、沟渠等非静止水系面内的水系结构线HGB赋值是否合理：
    /// 非静止水系面内的主干水系结构线的HGB应与该水系面的gb一致(汇入的单线河流支流的水系结构线除外)
    /// </summary>
    public class CheckRiverStructHGBCmd : SMGI.Common.SMGICommand
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
            IGeoFeatureLayer hydlLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "HYDL");
            })).ToArray().First() as IGeoFeatureLayer;
            if (hydlLyr == null)
            {
                MessageBox.Show("缺少HYDL要素类！");
                return;
            }
            IFeatureClass hydlFC = hydlLyr.FeatureClass;

            IGeoFeatureLayer hydaLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "HYDA");
            })).ToArray().First() as IGeoFeatureLayer;
            if (hydaLyr == null)
            {
                MessageBox.Show("缺少HYDA要素类！");
                return;
            }
            IFeatureClass hydaFC = hydaLyr.FeatureClass;


            string outPutFileName = OutputSetup.GetDir() + string.Format("\\水系结构线HGB检查.shp");


            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                err = DoCheck(outPutFileName, hydlFC, hydaFC, wo);
            }

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
                    MessageBox.Show("检查完毕！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }
        }

        /// <summary>
        /// 检查非静态水系面内的水系结构线的HGB是否合理
        /// </summary>
        /// <param name="resultSHPFileName"></param>
        /// <param name="hydlFC"></param>
        /// <param name="hydaFC"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public static string DoCheck(string resultSHPFileName, IFeatureClass hydlFC, IFeatureClass hydaFC, WaitOperation wo = null)
        {
            string err = "";

            int hgbIndex = hydlFC.FindField("HGB");
            if (hgbIndex == -1)
            {
                err = string.Format("要素类【{0}】中没有找到HGB字段！", hydlFC.AliasName);
                return err;
            }

            int hydaGBIndex = hydaFC.FindField("GB");
            if (hydaGBIndex == -1)
            {
                err = string.Format("要素类【{0}】中没有找到GB字段！", hydaFC.AliasName);
                return err;
            }

            try
            {
                List<KeyValuePair<int, int>> hydlOIDList = new List<KeyValuePair<int, int>>();//List<KeyValuePair<水系结构线OID, 水系面OID>>

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "gb >210000 and gb <230000";//非静态水系面：河流、沟渠
                if (hydaFC.HasCollabField())
                    qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;
                IFeatureCursor hydaCursor = hydaFC.Search(qf, true);
                IFeature hydaFe = null;
                while ((hydaFe = hydaCursor.NextFeature()) != null)
                {
                    if (wo != null)
                        wo.SetText("正在检查水系面【" + hydaFe.OID.ToString() + "】中的水系结构线......");

                    string hydaGB = hydaFe.get_Value(hydaGBIndex).ToString();

                    List<int> oidList = CheckRiverStructHGB(hydlFC, hydaFe.Shape as IPolygon, hydaGB);
                    foreach(var oid in oidList)
                    {
                        hydlOIDList.Add( new KeyValuePair<int,int>(oid, hydaFe.OID));
                    }

                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(hydaCursor);


                #region 输出结果
                if (wo != null)
                    wo.SetText("正在输出检查结果......");

                //新建结果文件
                ShapeFileWriter resultFile = new ShapeFileWriter();
                Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                fieldName2Len.Add("结构线OID", 16);
                fieldName2Len.Add("结构线HGB", 16);
                fieldName2Len.Add("水系面OID", 16);
                fieldName2Len.Add("检查项", 32);
                resultFile.createErrorResutSHPFile(resultSHPFileName, (hydlFC as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);

                //输出内容
                if (hydlOIDList.Count > 0)
                {
                    foreach (var item in hydlOIDList)
                    {
                        IFeature fe = hydlFC.GetFeature(item.Key);

                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("结构线OID", item.Key.ToString());
                        fieldName2FieldValue.Add("结构线HGB", fe.get_Value(hgbIndex).ToString());
                        fieldName2FieldValue.Add("水系面OID", item.Value.ToString());
                        fieldName2FieldValue.Add("检查项", "非静态水面结构线HGB检查");

                        resultFile.addErrorGeometry(fe.Shape, fieldName2FieldValue);
                    }
                }

                //保存结果文件
                resultFile.saveErrorResutSHPFile();
                #endregion
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

        /// <summary>
        /// 检查某一非静态水系面要素内的水系结构线的HGB是否合理
        /// 非静态水面的主干水系结构线的hgb与该水系面的gb一致，非主干水系结构线的hgb与汇入的单线河流的hgb一致
        /// </summary>
        /// <param name="hydlFC"></param>
        /// <param name="hydaShape"></param>
        /// <param name="hydaGB"></param>
        /// <returns>返回不合理的水系结构线要素的对象ID集合</returns>
        public static List<int> CheckRiverStructHGB(IFeatureClass hydlFC, IPolygon hydaShape, string hydaGB)
        {
            List<int> reuslt = new List<int>();

            if (hydaShape == null || hydaShape.IsEmpty)
                return reuslt;

            try
            {
                int hgbIndex = hydlFC.FindField("HGB");

                HydroAlgorithm.HydroGraph hydroGraph = new HydroAlgorithm.HydroGraph();

                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = hydaShape;
                sf.WhereClause = "GB = 210400";//水系结构线
                sf.GeometryField = hydlFC.ShapeFieldName;
                if(hydlFC.HasCollabField())
                    sf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter; //排除删除
                //sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureCursor feCursor = hydlFC.Search(sf, false);
                IFeature fe = null;
                while ((fe = feCursor.NextFeature()) != null)
                {
                    hydroGraph.Add(fe);

                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);

                if (hydroGraph.Edges.Count > 0)
                {
                    //获取Path列表
                    var hydroPaths = hydroGraph.BuildHydroPaths();

                    //计算Tree中各河流长度
                    foreach (var path in hydroPaths)
                    {
                        path.CalLength();
                    }

                    //按长度排序
                    hydroPaths.Sort();

                    for (int i = 0; i < hydroPaths.Count; ++i)
                    {
                        HydroAlgorithm.HydroPath path = hydroPaths[i];
                        if (i == hydroPaths.Count - 1)//主干水系结构线:HGB与动态水系面的GB一致
                        {
                            foreach (var edge in path.Edges)
                            {
                                string hgb = edge.Feature.get_Value(hgbIndex).ToString();
                                if (hgb != hydaGB)
                                {
                                    reuslt.Add(edge.Feature.OID);
                                }
                            }

                        }
                        else//分支水系结构线：若该水系结构线一端与单线河流邻接，则其HGB应与该单线河流一致
                        {
                            string hydlHGB = "";
                            #region 查找该分支水系结构线相邻的非水系结构线的HGB
                            foreach (var edge in path.Edges)
                            {
                                IFeature f = edge.Feature;
                                IPolyline pl = f.Shape as IPolyline;
                                if (pl == null || pl.IsEmpty)
                                    continue;

                                //判断水系结构线的其中一个端点是否与一个单线水系邻接
                                ISpatialFilter spatialFilter = new SpatialFilterClass();
                                spatialFilter.GeometryField = hydlFC.ShapeFieldName;
                                spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                spatialFilter.WhereClause = "GB <> 210400";//非水系结构线
                                if(hydlFC.HasCollabField())
                                    spatialFilter.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;


                                spatialFilter.Geometry = pl.FromPoint;//起点
                                if (hydlFC.FeatureCount(spatialFilter) == 0)
                                {
                                    spatialFilter.Geometry = pl.ToPoint;//终点
                                }
                                if (hydlFC.FeatureCount(spatialFilter) > 0)
                                {
                                    string hgb = f.get_Value(hgbIndex).ToString();//水系结构线HGB

                                    List<string> hydlHGBList = new List<string>();//相邻水系HGB集合
                                    IFeatureCursor hydlCursor = hydlFC.Search(spatialFilter, true);
                                    IFeature hydlFe = null;
                                    while ((hydlFe = hydlCursor.NextFeature()) != null)
                                    {
                                        hydlHGBList.Add(hydlFe.get_Value(hgbIndex).ToString());
                                    }
                                    System.Runtime.InteropServices.Marshal.ReleaseComObject(hydlCursor);

                                    if (hydlHGBList.Contains(hgb))
                                    {
                                        hydlHGB = hgb;
                                    }
                                    else
                                    {
                                        hydlHGB = hydlHGBList.First();
                                    }

                                    break;
                                }
                            }
                            #endregion

                            //分支水系结构线在端点处与某一非水系结构线相邻，则该分支水系结构线的hgb应与其hgb一致
                            if (hydlHGB != "")
                            {
                                foreach (var edge in path.Edges)
                                {
                                    string hgb = edge.Feature.get_Value(hgbIndex).ToString();
                                    if (hgb != hydlHGB)
                                    {
                                        reuslt.Add(edge.Feature.OID);
                                    }
                                }
                            }
                            else
                            {
                                //没有找到相邻接的水系？
                                string exString = "该分支水系结构线没有找到与其相邻接的水系！";
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                throw ex;
            }

            return reuslt;
        }
    }
}
