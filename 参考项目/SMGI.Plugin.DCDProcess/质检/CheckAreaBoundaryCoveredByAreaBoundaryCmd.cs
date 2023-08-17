using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.IO;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 检查目标面要素是否被参考面要素边界完全覆盖，导出不被参考面要素类要素边界覆盖的所目标面要素信息
    /// </summary>
    public class CheckAreaBoundaryCoveredByAreaBoundaryCmd : SMGI.Common.SMGICommand
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
            //读取配置表，获取需检查的内容
            string mdbPath = m_Application.Template.Root + "\\质检\\质检内容配置.mdb";
            if (!System.IO.File.Exists(mdbPath))
            {
                MessageBox.Show(string.Format("未找到配置文件:{0}!", mdbPath));
                return;
            }
            DataTable dataTable = DCDHelper.ReadToDataTable(mdbPath, "面边界被其它要素边界覆盖检查");
            if (dataTable == null)
            {
                return;
            }

            string outputFileName = OutputSetup.GetDir() + string.Format("\\面边界被其它要素边界覆盖检查.shp");

            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                Dictionary<Dictionary<string, Dictionary<int, List<IPolyline>>>, string> checkResult = new Dictionary<Dictionary<string, Dictionary<int, List<IPolyline>>>, string>();
                
                ISpatialReference sr = null;
                foreach (DataRow dr in dataTable.Rows)
                {
                    string areaFCName = dr["AreaFCName"].ToString();
                    string areaFilter = dr["AreaFilterString"].ToString();
                    string referAreaFCName = dr["ReferAreaFCName"].ToString();
                    string referAreaFilter = dr["ReferAreaFilterString"].ToString();


                    IGeoFeatureLayer areaLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == areaFCName.ToUpper());
                    })).ToArray().First() as IGeoFeatureLayer;
                    if (areaLyr == null)
                    {
                        continue;
                    }
                    IFeatureClass areaFC = areaLyr.FeatureClass;
                    if (sr != null)
                        sr = (areaFC as IGeoDataset).SpatialReference;

                    IGeoFeatureLayer referAreaLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == referAreaFCName.ToUpper());
                    })).ToArray().First() as IGeoFeatureLayer;
                    if (referAreaLyr == null)
                    {
                        continue;
                    }
                    IFeatureClass referAreaFC = referAreaLyr.FeatureClass;

                    wo.SetText(string.Format("正在检查线要素......"));

                    Dictionary<string, Dictionary<int, List<IPolyline>>> errFCName2GeoList = new Dictionary<string, Dictionary<int, List<IPolyline>>>();
                    err = DoCheck(areaFC, areaFilter, referAreaFC, referAreaFilter, ref errFCName2GeoList, wo);
                    if (err != "")
                        break;

                    if (errFCName2GeoList.Count > 0)
                    {
                        checkResult.Add(errFCName2GeoList, referAreaFC.AliasName);
                    }
                }

                //输出
                if (err == "" && checkResult.Count > 0)
                {
                    wo.SetText("正在输出检查结果......");

                    //新建结果文件
                    ShapeFileWriter resultFile = new ShapeFileWriter();
                    Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                    fieldName2Len.Add("要素类名", 16);
                    fieldName2Len.Add("要素编号", 16);
                    fieldName2Len.Add("说明", 32);
                    fieldName2Len.Add("检查项", 32);

                    resultFile.createErrorResutSHPFile(outputFileName, sr, esriGeometryType.esriGeometryPolyline, fieldName2Len);

                    foreach (var kv1 in checkResult)
                    {
                        string referFCName = kv1.Value;//参考面图层名
                        foreach (var kv2 in kv1.Key)
                        {
                            string fcName = kv2.Key;//待检查面图层名
                            foreach (var kv3 in kv2.Value)
                            {
                                int oid = kv3.Key;//待检查面要素ID

                                Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                fieldName2FieldValue.Add("要素类名", fcName);
                                fieldName2FieldValue.Add("要素编号", oid.ToString());
                                fieldName2FieldValue.Add("说明", string.Format("参考面要素类名：{0}", referFCName));
                                fieldName2FieldValue.Add("检查项", "面边界被其它要素边界覆盖检查");

                                IGeometry shape = null;
                                foreach (var item in kv3.Value)
                                {
                                    #region 同一个要素的几何合并
                                    //if (shape == null)
                                    //{
                                    //    shape = item;
                                    //}
                                    //else
                                    //{
                                    //    ITopologicalOperator topo = shape as ITopologicalOperator;
                                    //    shape = topo.Union(item);
                                    //    topo.Simplify();
                                    //}
                                    #endregion

                                    shape = item;
                                    resultFile.addErrorGeometry(shape, fieldName2FieldValue);
                                }

                                //resultFile.addErrorGeometry(shape, fieldName2FieldValue);
                            }
                        }
                    }

                    //保存结果文件
                    resultFile.saveErrorResutSHPFile();
                }
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
                    MessageBox.Show("检查完毕！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }
        }

        public static string DoCheck(IFeatureClass areaFC, string areaFilterString, IFeatureClass referAreaFC, string referAreaFilterString,
            ref Dictionary<string, Dictionary<int, List<IPolyline>>> errFCName2GeoList, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                IQueryFilter areaQF = new QueryFilterClass();
                areaQF.WhereClause = areaFilterString;
                if (areaFC.HasCollabField())
                    areaQF.WhereClause = string.Format("({0}) and ", areaFilterString) + cmdUpdateRecord.CurFeatureFilter;

                IQueryFilter referAreaQF = new QueryFilterClass();
                referAreaQF.WhereClause = referAreaFilterString;
                if (referAreaFC.HasCollabField())
                    referAreaQF.WhereClause = string.Format("({0}) and ", referAreaFilterString) + cmdUpdateRecord.CurFeatureFilter;

                var oid2GeoList = CheckHelper.PolygonBoundariesNotBeCoveredByPolygonBoundaries(areaFC, areaQF, referAreaFC, referAreaQF, null);

                if (oid2GeoList.Count > 0)
                {
                    if (errFCName2GeoList.ContainsKey(areaFC.AliasName))
                    {
                        foreach (var kv in oid2GeoList)
                        {
                            if (errFCName2GeoList[areaFC.AliasName].ContainsKey(kv.Key))
                            {
                                errFCName2GeoList[areaFC.AliasName][kv.Key].AddRange(kv.Value);
                            }
                            else
                            {
                                errFCName2GeoList[areaFC.AliasName].Add(kv.Key, kv.Value);
                            }
                        }
                    }
                    else
                    {
                        errFCName2GeoList.Add(areaFC.AliasName, oid2GeoList);
                    }
                }

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
