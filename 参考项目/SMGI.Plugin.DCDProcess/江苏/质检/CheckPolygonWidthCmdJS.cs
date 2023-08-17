using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using System.Data;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using System.Runtime.InteropServices;
using System.IO;
using SMGI.Plugin.GeneralEdit;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 检测水系面等宽度是否符合指标
    /// 详见需求文档《【20181225回复】追加需求（质检工具等）_董先敏.docx》
    /// 按《生产系统优化工具321.docx》-张宇的要求，优化了尖角处误报情况的排除。张怀相20220401
    /// </summary>
    public class CheckPolygonWidthCmdJS : SMGI.Common.SMGICommand
    {

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null;
            }
        }

        public override void OnClick()
        {
            if (m_Application.MapControl.Map.ReferenceScale == 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;
            }

            //读取配置表，获取需检查的内容
            Dictionary<KeyValuePair<string, string>, double> feType2Width = new Dictionary<KeyValuePair<string, string>, double>();//Dictionary<KeyValuePair<要素类名称,过滤条件>, 线宽>
            string mdbPath = m_Application.Template.Root + "\\质检\\质检内容配置.mdb";
            if (!System.IO.File.Exists(mdbPath))
            {
                MessageBox.Show(string.Format("未找到配置文件:{0}!", mdbPath));
                return;
            }
            string tabaleName = "面要素宽度指标检查";
            DataTable dataTable = DCDHelper.ReadMDBTable(mdbPath, tabaleName);
            if (dataTable == null)
            {
                MessageBox.Show(string.Format("配置文件【{0}】中未找到表【{1}】！", mdbPath, tabaleName));
                return;
            }
            foreach (DataRow dr in dataTable.Rows)
            {
                string fcName = dr["FCName"].ToString();
                string filterString = dr["FilterString"].ToString();
                double minWidth = 0;
                double.TryParse(dr["MinWidth"].ToString(), out minWidth);

                KeyValuePair<string, string> kv = new KeyValuePair<string, string>(fcName, filterString);
                feType2Width.Add(kv, minWidth);
            }

            CheckPolygonWidthForm frm = new CheckPolygonWidthForm(m_Application.MapControl.Map.ReferenceScale, feType2Width);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string outputFileName = OutputSetup.GetDir() + string.Format("\\面要素宽度指标检查.shp");


            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                err = DoCheck(outputFileName, frm.FeatureType2Width, frm.ReferScale, wo);
            }

            if (err == "")
            {
                if (File.Exists(outputFileName))
                {
                    IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName);

                    if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                }
                else
                {
                    MessageBox.Show("检查完毕,没有发现异常！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }
        }

        public static string DoCheck(string resultSHPFileName, Dictionary<KeyValuePair<string, string>, double> feType2Width, double mapScale, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                List<CheckResultInfo> checkResultList = new List<CheckResultInfo>();
                ISpatialReference sr = null;

                foreach (var kv in feType2Width)
                {
                    string fcName = kv.Key.Key;
                    string filter = kv.Key.Value;
                    double bufferVal = kv.Value * 0.5 * 0.001 * mapScale;

                    var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name == fcName);
                    })).ToArray();
                    if (lyrs.Count() == 0)
                        continue;
                    IGeoFeatureLayer geoFeLayer = lyrs.First() as IGeoFeatureLayer;
                    if (geoFeLayer == null)
                        continue;
                    IFeatureClass fc = geoFeLayer.FeatureClass;
                    if (sr == null)
                    {
                        sr = (fc as IGeoDataset).SpatialReference;
                    }

                    IQueryFilter qf = new QueryFilterClass();
                    if (fc.HasCollabField())
                    {
                        qf.WhereClause = string.Format("({0}) and ", filter) + cmdUpdateRecord.CurFeatureFilter;
                    }
                    else
                    {
                        qf.WhereClause = filter;
                    }

                    IFeatureCursor feCursor = fc.Search(qf, true);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        if (wo != null)
                            wo.SetText(string.Format("正在检查要素类【{0}】中的要素【{1}】......", fcName, fe.OID));

                        if (fe.Shape == null || fe.Shape.IsEmpty)
                            continue;//忽略

                        IGeometry inBufferShape = null;
                        #region 创建收缩缓冲面
                        if (true)
                        {
                            IBufferConstruction bfConstrut = new BufferConstructionClass();

                            //平头缓冲
                            (bfConstrut as IBufferConstructionProperties).EndOption = esriBufferConstructionEndEnum.esriBufferFlat;
                            IEnumGeometry enumGeo = new GeometryBagClass();
                            (enumGeo as IGeometryCollection).AddGeometry(fe.Shape);
                            IGeometryCollection outputBuffer = new GeometryBagClass();
                            bfConstrut.ConstructBuffers(enumGeo, -bufferVal, outputBuffer);
                            for (int i = 0; i < outputBuffer.GeometryCount; i++)
                            {
                                IGeometry geo = outputBuffer.get_Geometry(i);
                                if (inBufferShape == null)
                                {
                                    inBufferShape = geo;
                                }
                                else
                                {
                                    inBufferShape = (inBufferShape as ITopologicalOperator).Union(geo);
                                }
                            }
                        }
                        #endregion

                        IGeometry outBufferShape = null;
                        #region 创建扩张后的缓冲面
                        if (inBufferShape != null && !inBufferShape.IsEmpty)
                        {
                            IBufferConstruction bfConstrut = new BufferConstructionClass();

                            //平头缓冲
                            (bfConstrut as IBufferConstructionProperties).EndOption = esriBufferConstructionEndEnum.esriBufferFlat;
                            IEnumGeometry enumGeo = new GeometryBagClass();
                            (enumGeo as IGeometryCollection).AddGeometry(inBufferShape);
                            IGeometryCollection outputBuffer = new GeometryBagClass();
                            bfConstrut.ConstructBuffers(enumGeo, bufferVal * 1.1, outputBuffer);
                            for (int i = 0; i < outputBuffer.GeometryCount; i++)
                            {
                                IGeometry geo = outputBuffer.get_Geometry(i);
                                if (outBufferShape == null)
                                {
                                    outBufferShape = geo;
                                }
                                else
                                {
                                    outBufferShape = (outBufferShape as ITopologicalOperator).Union(geo);
                                }
                            }
                        }
                        #endregion

                        IPolygon errShape = null;//裁切面
                        #region 用扩张后的缓冲面区擦除原面几何，得到不符合宽度指标的部分
                        if (outBufferShape == null)
                        {
                            errShape = fe.ShapeCopy as IPolygon;
                        }
                        else
                        {
                            errShape = (fe.Shape as ITopologicalOperator).Difference(outBufferShape) as IPolygon;
                        }
                        #endregion

                        IPolygon4 po4 = errShape as IPolygon4;                        
                        var gc = (IGeometryCollection)po4.ConnectedComponentBag;

                        for (var i = 0; i < gc.GeometryCount; i++)
                        {
                            IPolygon geoPart = gc.Geometry[i] as IPolygon;

                            ITopologicalOperator tTopo = fe.ShapeCopy as ITopologicalOperator;
                            IGeometry pl_clip = tTopo.Intersect(geoPart, esriGeometryDimension.esriGeometry1Dimension);

                            ITopologicalOperator tTopo2 = pl_clip as ITopologicalOperator;
                            tTopo2.Simplify();//简化处理 

                            IGeometryCollection gc2 = tTopo2 as IGeometryCollection;

                            string mark = "";
                            if (gc2.GeometryCount == 1)
                            {
                                IPolyline pl2 = tTopo2 as IPolyline;

                                if (pl2.IsClosed) //是否闭合
                                {
                                    mark = "2-整体宽度不足";
                                }
                                else
                                {
                                    mark = "3-小尖角";
                                    continue;
                                }
                            }
                            else if (gc2.GeometryCount > 1)
                            {
                                mark = "1-局部宽度不足";
                            }


                            CheckResultInfo cri = new CheckResultInfo();
                            cri.FCName = fcName;
                            cri.FID = fe.OID;
                            cri.MinWidth = bufferVal * 2.0;
                            cri.Mark = mark;
                            cri.Shape = geoPart;

                            checkResultList.Add(cri);


                        } 

                    }
                    Marshal.ReleaseComObject(feCursor);
                }


                if (checkResultList.Count > 0)
                {
                    if (wo != null)
                        wo.SetText("正在输出检查结果......");

                    ShapeFileWriter resultFile = null;
                    foreach (var item in checkResultList)
                    {
                        if (resultFile == null)
                        {
                            //新建结果文件
                            resultFile = new ShapeFileWriter();
                            Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                            fieldName2Len.Add("图层名", 16);
                            fieldName2Len.Add("要素编号", 16);
                            fieldName2Len.Add("宽度指标", 16);
                            fieldName2Len.Add("说明", 32);
                            fieldName2Len.Add("检查项", 32);

                            resultFile.createErrorResutSHPFile(resultSHPFileName, sr, esriGeometryType.esriGeometryPolygon, fieldName2Len);
                        }

                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("图层名", item.FCName);
                        fieldName2FieldValue.Add("要素编号", item.FID.ToString());
                        fieldName2FieldValue.Add("宽度指标", item.MinWidth.ToString());
                        fieldName2FieldValue.Add("说明", item.Mark.ToString());
                        fieldName2FieldValue.Add("检查项", "面要素宽度指标检查");

                        resultFile.addErrorGeometry(item.Shape, fieldName2FieldValue);
                    }

                    //保存结果文件
                    if(resultFile != null)
                        resultFile.saveErrorResutSHPFile();
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
            }

            return err;
        }

        public class CheckResultInfo
        {
            public string FCName { get; set; }
            public int FID { get; set; }
            public double MinWidth { get; set; }
            public String Mark { get; set; }
            public IPolygon Shape { get; set; }

        }
    }
}
