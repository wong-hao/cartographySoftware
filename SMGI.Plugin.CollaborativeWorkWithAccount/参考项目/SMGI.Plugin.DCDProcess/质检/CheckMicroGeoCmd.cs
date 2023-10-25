using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesFile;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using System.IO;
using System.Data;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 微小几何检查
    /// </summary>
    public class CheckMicroGeoCmd : SMGICommand
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

            var frm = new CheckMicroGeoForm(m_Application);
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Text = "微小几何检查";

            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string err = "";//检查过程出错提示
            string outputFileName_line, outputFileName_area;
            if (frm.CheckFeatureLayerList.Count > 1)
            {
                outputFileName_line = OutputSetup.GetDir() + string.Format("\\{0}_线.shp", frm.Text);
                outputFileName_area = OutputSetup.GetDir() + string.Format("\\{0}_面.shp", frm.Text);
            }
            else
            {
                outputFileName_line = OutputSetup.GetDir() + string.Format("\\{0}_线_{1}.shp", frm.Text, frm.CheckFeatureLayerList.First().Name);
                outputFileName_area = OutputSetup.GetDir() + string.Format("\\{0}_面_{1}.shp", frm.Text, frm.CheckFeatureLayerList.First().Name);
            }

            //获得待查的要素类 （fcList==frm.CheckFeatureLayerList）
            List<IFeatureClass> fcList = new List<IFeatureClass>();
            foreach (var layer in frm.CheckFeatureLayerList)
            {
                IFeatureClass fc = layer.FeatureClass;
                if (!fcList.Contains(fc))
                    fcList.Add(fc);
            }

            //使用直接输入的阈值
            if (!frm.UsingRuleTable)
            {
                using (var wo = m_Application.SetBusy())
                {                   
                    err = DoCheck(outputFileName_line, outputFileName_area, fcList, frm.MicroLenThreshold, frm.MicroAreaThreshold, m_Application.MapControl.Map.ReferenceScale, wo);
                }
            }
            else
            //使用阈值表质检规则
            {
                using (var wo = m_Application.SetBusy())
                {
                    err = DoCheck2(outputFileName_line, outputFileName_area, fcList, frm.RuleTablePath, m_Application.MapControl.Map.ReferenceScale, wo); 
                } 
            }

            if (err == "")
            {
                if (File.Exists(outputFileName_line) || File.Exists(outputFileName_area))
                {
                    if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        if (File.Exists(outputFileName_line))
                        {
                            IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName_line);
                            CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                        }

                        if (File.Exists(outputFileName_area))
                        {
                            IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName_area);
                            CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("检查完毕，未发现小于指标的微短要素！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }
        }
        
        /// <summary>
        /// 微小几何检查
        /// </summary>
        /// <param name="resultLineSHPFileName"></param>
        /// <param name="resultAreaSHPFileName"></param>
        /// <param name="fcList"></param>
        /// <param name="lenThreshold"></param>
        /// <param name="areaThreshold"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public static string DoCheck(string resultLineSHPFileName, string resultAreaSHPFileName, List<IFeatureClass> fcList, double lenThreshold, double areaThreshold, double referScale, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                ShapeFileWriter resultLineFile = null;
                ShapeFileWriter resultAreaFile = null;

                foreach (var fc in fcList)
                {
                    if (fc.ShapeType == esriGeometryType.esriGeometryPoint)
                        continue;

                    IQueryFilter qf = new QueryFilterClass();
                    if (fc.HasCollabField())
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                    if (wo != null)
                        wo.SetText(string.Format("正在对要素类【{0}】进行微小几何检查......", fc.AliasName));

                    //核查该图层的微小几何
                    Dictionary<IGeometry, int> errList = CheckMicroGeo(fc, qf, lenThreshold * referScale * 1.0e-3, areaThreshold * referScale * referScale * 1.0e-6, wo);
                    if (errList.Count > 0)
                    {
                        foreach (var kv in errList)
                        {
                            if (kv.Key.GeometryType == esriGeometryType.esriGeometryPolyline)
                            {
                                if (resultLineFile == null)
                                {
                                    //建立结果文件
                                    resultLineFile = new ShapeFileWriter();
                                    Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                                    fieldName2Len.Add("图层名", 20);
                                    fieldName2Len.Add("要素编号", 32);
                                    fieldName2Len.Add("阈值", 32);
                                    fieldName2Len.Add("检查项", 32);
                                    resultLineFile.createErrorResutSHPFile(resultLineSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);
                                }
                                
                                Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                fieldName2FieldValue.Add("图层名", fc.AliasName);
                                fieldName2FieldValue.Add("要素编号", kv.Value.ToString());
                                fieldName2FieldValue.Add("阈值", lenThreshold.ToString());
                                fieldName2FieldValue.Add("检查项", "微小几何检查");


                                resultLineFile.addErrorGeometry(kv.Key, fieldName2FieldValue);
                            }
                            else if (kv.Key.GeometryType == esriGeometryType.esriGeometryPolygon)
                            {
                                if (resultAreaFile == null)
                                {
                                    //建立结果文件
                                    resultAreaFile = new ShapeFileWriter();
                                    Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                                    fieldName2Len.Add("图层名", 20);
                                    fieldName2Len.Add("要素编号", 32);
                                    fieldName2Len.Add("阈值", 32);
                                    fieldName2Len.Add("检查项", 32);
                                    resultAreaFile.createErrorResutSHPFile(resultAreaSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolygon, fieldName2Len);
                                }

                                Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                fieldName2FieldValue.Add("图层名", fc.AliasName);
                                fieldName2FieldValue.Add("要素编号", kv.Value.ToString());
                                fieldName2FieldValue.Add("阈值", areaThreshold.ToString());
                                fieldName2FieldValue.Add("检查项", "微小几何检查");


                                resultAreaFile.addErrorGeometry(kv.Key, fieldName2FieldValue);
                            }
                        }
                    }

                }

                //保存结果文件
                if (resultLineFile != null)
                    resultLineFile.saveErrorResutSHPFile();
                if (resultAreaFile != null)
                    resultAreaFile.saveErrorResutSHPFile();
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

        public static Dictionary<IGeometry, int> CheckMicroGeo(IFeatureClass fc, IQueryFilter qf, double lenThreshold, double areaThreshold, WaitOperation wo = null)
        {
            Dictionary<IGeometry, int> result = new Dictionary<IGeometry, int>();

            IFeatureCursor feCursor = fc.Search(qf, true);
            IFeature fe;
            while ((fe = feCursor.NextFeature()) != null)
            {
                if (wo != null)
                    wo.SetText(string.Format("正在检查要素类【{0}】中的要素【{1}】......", fc.AliasName, fe.OID));

                bool bMicro = false;
                if (fe.Shape == null || fe.Shape.IsEmpty)
                {
                    bMicro = true;
                }
                else
                {
                    if (fc.ShapeType == esriGeometryType.esriGeometryPolyline)
                    {
                        IPolyline geo = fe.Shape as IPolyline;
                        if (geo.Length < lenThreshold)
                        {
                            bMicro = true;
                        }
                    }
                    else if (fc.ShapeType == esriGeometryType.esriGeometryPolygon)
                    {
                        IArea geo = fe.Shape as IArea;
                        if (geo.Area < areaThreshold)
                        {
                            bMicro = true;
                        }
                    }
                }

                if (bMicro)
                {
                    result.Add(fe.ShapeCopy, fe.OID);
                }
            }
            Marshal.ReleaseComObject(feCursor);

            return result;
        }

        public static string DoCheck2(string resultLineSHPFileName, string resultAreaSHPFileName, List<IFeatureClass> fcList, string ruleTablePath, double referScale, WaitOperation wo = null)
        {
            string err = "";
            Dictionary<string, List<Tuple<string, double>>> fc_filterConfigD = new Dictionary<string, List<Tuple<string, double>>>();
            string mdbPath = System.IO.Path.GetDirectoryName(ruleTablePath);
            string tableName = System.IO.Path.GetFileName(ruleTablePath);
            DataTable dataTable = DCDHelper.ReadToDataTable(mdbPath, tableName);
            foreach (DataRow dr in dataTable.Rows)
            {
                string fcName = dr["FCName"].ToString();
                string filterString = dr["FilterString"].ToString();
                double minValue = double.Parse((dr["MinValue"]).ToString());
                Tuple<string, double> tp = new Tuple<string, double>(filterString, minValue);
                if (fc_filterConfigD.ContainsKey(fcName))
                {
                    fc_filterConfigD[fcName].Add(tp);
                }
                else
                {
                    fc_filterConfigD.Add(fcName, new List<Tuple<string, double>>() { tp });
                }
            }

            try 
            {
                ShapeFileWriter resultLineFile = null;
                ShapeFileWriter resultAreaFile = null;
                foreach (var fc in fcList)
                {
                    IDataset ds = fc as IDataset;
                    string name = ds.Name;
                    if(fc_filterConfigD.ContainsKey(name))
                    {
                        foreach (var filter_minValue in fc_filterConfigD[name])
                        {
                            string filterString = filter_minValue.Item1;
                            double minValue = filter_minValue.Item2;
                            IQueryFilter qf = new QueryFilterClass();
                            if (fc.HasCollabField())
                                qf.WhereClause = cmdUpdateRecord.CurFeatureFilter+" and "+filterString;
                            else
                                qf.WhereClause = filterString;

                            if (wo != null)
                                wo.SetText(string.Format("正在对要素类【{0}】进行微小几何检查......", name));
                            //核查该图层的微小几何
                            Dictionary<IGeometry, int> errList = CheckMicroGeo(fc, qf, minValue * referScale * 1.0e-3, minValue * referScale * referScale * 1.0e-6, wo);
                            if (errList.Count > 0)
                            {
                                foreach (var kv in errList)
                                {
                                    if (kv.Key.GeometryType == esriGeometryType.esriGeometryPolyline)
                                    {
                                        if (resultLineFile == null)
                                        {
                                            //建立结果文件
                                            resultLineFile = new ShapeFileWriter();
                                            Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                                            fieldName2Len.Add("图层名", 20);
                                            fieldName2Len.Add("要素编号", 32);
                                            fieldName2Len.Add("阈值", 32);
                                            fieldName2Len.Add("检查项", 32);
                                            resultLineFile.createErrorResutSHPFile(resultLineSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);
                                        }

                                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                        fieldName2FieldValue.Add("图层名", fc.AliasName);
                                        fieldName2FieldValue.Add("要素编号", kv.Value.ToString());
                                        fieldName2FieldValue.Add("阈值", minValue.ToString());
                                        fieldName2FieldValue.Add("检查项", "微小几何检查" + filterString);


                                        resultLineFile.addErrorGeometry(kv.Key, fieldName2FieldValue);
                                    }
                                    else if (kv.Key.GeometryType == esriGeometryType.esriGeometryPolygon)
                                    {
                                        if (resultAreaFile == null)
                                        {
                                            //建立结果文件
                                            resultAreaFile = new ShapeFileWriter();
                                            Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                                            fieldName2Len.Add("图层名", 20);
                                            fieldName2Len.Add("要素编号", 32);
                                            fieldName2Len.Add("阈值", 32);
                                            fieldName2Len.Add("检查项", 32);
                                            resultAreaFile.createErrorResutSHPFile(resultAreaSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolygon, fieldName2Len);
                                        }

                                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                        fieldName2FieldValue.Add("图层名", fc.AliasName);
                                        fieldName2FieldValue.Add("要素编号", kv.Value.ToString());
                                        fieldName2FieldValue.Add("阈值", minValue.ToString());
                                        fieldName2FieldValue.Add("检查项", "微小几何检查" + filterString);


                                        resultAreaFile.addErrorGeometry(kv.Key, fieldName2FieldValue);
                                    }
                                }
                            }
                    
                        }
                    }
                }
                //保存结果文件
                if (resultLineFile != null)
                    resultLineFile.saveErrorResutSHPFile();
                if (resultAreaFile != null)
                    resultAreaFile.saveErrorResutSHPFile();

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
