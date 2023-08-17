using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geoprocessor;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 检查道路与街区面关系是否合理
    /// 2022.4.24
    ///     1.街区内有城际公路；
    ///     2.街区外有城市道路；
    ///     3.限差按0.12mm
    /// </summary>
    public class CheckLrdlResaCmd : SMGI.Common.SMGICommand
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

            double maxDist = 0.12 * m_Application.MapControl.Map.ReferenceScale * 0.001; //0.12mm为图面限差

            IGeoFeatureLayer lrdlLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "LRDL");
            })).ToArray().First() as IGeoFeatureLayer;
            if (lrdlLyr == null)
            {
                MessageBox.Show("缺少LRDL要素类！");
                return;
            }
            IFeatureClass lrdlFC = lrdlLyr.FeatureClass;

            IGeoFeatureLayer resaLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "RESA");
            })).ToArray().First() as IGeoFeatureLayer;
            if (resaLyr == null)
            {
                MessageBox.Show("缺少RESA要素类！");
                return;
            }
            IFeatureClass resaFC = resaLyr.FeatureClass;


            string outPutFileName = OutputSetup.GetDir() + string.Format("\\道路与街区关系检查.shp");


            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                err = DoCheck(outPutFileName, lrdlFC, resaFC, true, true, true,maxDist, wo);
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
        /// 检查道路与街区面关系是否合理：
        /// 1.街区内有城际公路；
        /// 2.街区外有城市道路；
        /// 3.限差按0.12mm。
        /// </summary>
        /// <param name="resultSHPFileName"></param>
        /// <param name="lrdlFC"></param>
        /// <param name="resaFC"></param>
        /// <param name="bLrdlcrossResa">是否检查街区内有城际公路交叉</param>
        /// <param name="bLctloutResa">是否检查街区外有城市道路</param>
        /// <param name="bNotWithin">是否检查街区内有城际公路</param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public static string DoCheck(string resultSHPFileName, IFeatureClass lrdlFC, IFeatureClass resaFC, 
            bool bLrdlcrossResa, bool bLctloutResa, bool bNotWithin, double maxDistance,WaitOperation wo = null)
        {
            string err = "";

            int lrdlGBIndex = lrdlFC.FindField("GB");
            if (lrdlGBIndex == -1)
            {
                err = string.Format("要素类【{0}】中没有找到GB字段！", lrdlFC.AliasName);
                return err;
            }

            int resaGBIndex = resaFC.FindField("GB");
            if (resaGBIndex == -1)
            {
                err = string.Format("要素类【{0}】中没有找到GB字段！", resaFC.AliasName);
                return err;
            }

            try
            {
                Dictionary<int, string> oid2ErrInfo = new Dictionary<int, string>();

                //街区面
                IQueryFilter resaQF = new QueryFilterClass();
                resaQF.WhereClause = "GB = 310200";//街区面
                if (resaFC.HasCollabField())
                    resaQF.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;

                #region 检查街区内有城际公路
                if (bLrdlcrossResa)
                {
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = "(GB <430501 or GB>430600)";//城际公路
                    if (lrdlFC.HasCollabField())
                        qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;

                    IFeatureCursor feCursor = lrdlFC.Search(qf, true);
                    IFeature f;
                    while ((f = feCursor.NextFeature()) != null)
                    {
                        if (wo != null)
                            wo.SetText(string.Format("正在检查城际公路【{0}】......", f.OID));
                        ISpatialFilter sf = new SpatialFilter();
                        sf.Geometry = f.Shape;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;

                        //城际公路-街区面相交的情况
                        int n = resaFC.FeatureCount(sf);
                        if (n > 1)
                        {
                            string info = string.Format("11-城际公路跨街区，街区面个数-{0}", n);
                            oid2ErrInfo.Add(f.OID, info);
                        }
                        else if (n == 1)
                        {
                            IFeatureCursor cursor2 = resaFC.Search(sf, true);
                            IFeature fe2 = null;
                            while ((fe2 = cursor2.NextFeature()) != null)
                            {
                                IPolyline pl = f.Shape as IPolyline;
                                IPolygon pg = fe2.Shape as IPolygon;
                                ITopologicalOperator tTopo = pl as ITopologicalOperator;
                                IGeometry pl_clip = tTopo.Intersect(pg, esriGeometryDimension.esriGeometry1Dimension);
                                IPolyline pl2 = pl_clip as IPolyline;
                                if (pl2.Length >= maxDistance)
                                {
                                    string info = string.Format("12-城际公路入街区,距离-{0:F2}", pl2.Length);
                                    oid2ErrInfo.Add(f.OID, info);
                                }
                            }
                            Marshal.ReleaseComObject(cursor2);
                        }
                        else //n==0 城际公路-没有街区面相交——正确
                        {
                        }                        
                    }
                    Marshal.ReleaseComObject(feCursor);
                }
                #endregion
                
                #region 检查街区内的城际公路
                if (bNotWithin)
                {
                    IFeatureCursor resaCursor2 = resaFC.Search(resaQF, true);
                    IFeature resaFe = null;
                    while ((resaFe = resaCursor2.NextFeature()) != null)
                    {
                        if (wo != null)
                            wo.SetText(string.Format("正在检查街区内的城际公路【{0}】......", resaFe.OID));

                        ISpatialFilter sf = new SpatialFilter();
                        sf.Geometry = resaFe.Shape;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                        sf.WhereClause = "(GB <430501 or GB>430600)";
                        if (lrdlFC.HasCollabField())
                            sf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;

                        IFeatureCursor lrdlCursor = lrdlFC.Search(sf, true);
                        IFeature lrdlFe;
                        while ((lrdlFe = lrdlCursor.NextFeature()) != null)
                        {
                            oid2ErrInfo.Add(lrdlFe.OID, "13-城际公路完全包含于街区面内");                           
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(lrdlCursor); 
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(resaCursor2); 
                }
                #endregion
                
                #region 检查街区外的城市公路
                if (bLctloutResa)
                {                    
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = "(GB >=430501 and GB<=430600)";
                    if (lrdlFC.HasCollabField())
                        qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;

                    IFeatureCursor feCursor = lrdlFC.Search(qf, true);
                    IFeature f;
                    while ((f = feCursor.NextFeature()) != null)
                    {
                        if (wo != null)
                            wo.SetText(string.Format("正在检查城市公路【{0}】......", f.OID));

                        ISpatialFilter sf = new SpatialFilter();
                        sf.Geometry = f.Shape;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;

                        int n = resaFC.FeatureCount(sf);
                        if (n>1)//该城市公路横跨街区面
                        {
                            if (bLctloutResa)
                            {
                                string info = string.Format("21-城市道路横跨多个街区面");
                                oid2ErrInfo.Add(f.OID, info);
                            }
                        }
                        else if (n == 1)
                        {
                            if (bLctloutResa)
                            {
                                IFeatureCursor cursor2 = resaFC.Search(sf, true);
                                IFeature fe2 = null;
                                while ((fe2 = cursor2.NextFeature()) != null)
                                {
                                    IPolyline pl = f.Shape as IPolyline;
                                    IPolygon pg = fe2.Shape as IPolygon;
                                    ITopologicalOperator tTopo = pl as ITopologicalOperator;
                                    IGeometry pl_clip = tTopo.Intersect(pg,esriGeometryDimension.esriGeometry1Dimension);
                                    IPolyline pl2 = tTopo.Difference(pl_clip) as IPolyline;
                                    if (pl2.Length >= maxDistance)
                                    {
                                        string info = string.Format("22-城市道路出街区,距离-{0:F2}", pl2.Length);
                                        oid2ErrInfo.Add(f.OID, info);
                                    }
                                }
                                Marshal.ReleaseComObject(cursor2);
                            } 
                        }
                        else //n==0
                        {
                            if (bNotWithin)
                            {
                                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelWithin;
                                if (resaFC.FeatureCount(sf) == 0)//完全落在街区外
                                {
                                    oid2ErrInfo.Add(f.OID, "23-城市道路在街区外");
                                    continue;
                                }
                            }
                        }
                    }
                    Marshal.ReleaseComObject(feCursor);
                }
                #endregion


                if (wo != null)
                    wo.SetText("正在输出检查结果......");

                #region 输出结果
                //新建结果文件
                ShapeFileWriter resultFile = new ShapeFileWriter();
                Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                fieldName2Len.Add("图层名", 20);
                fieldName2Len.Add("编号", 20);
                fieldName2Len.Add("说明", 40);
                fieldName2Len.Add("检查项", 20);
                resultFile.createErrorResutSHPFile(resultSHPFileName, (lrdlFC as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);

                //输出内容
                if (oid2ErrInfo.Count > 0)
                {
                    foreach (var item in oid2ErrInfo)
                    {
                        IFeature fe = lrdlFC.GetFeature(item.Key);

                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("图层名", lrdlFC.AliasName);
                        fieldName2FieldValue.Add("编号", item.Key.ToString());
                        fieldName2FieldValue.Add("说明", item.Value.ToString());
                        fieldName2FieldValue.Add("检查项", "道路与街区关系检查");

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
    }
}
