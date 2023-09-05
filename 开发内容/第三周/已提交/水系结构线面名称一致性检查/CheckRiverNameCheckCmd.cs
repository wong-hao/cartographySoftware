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
using ESRI.ArcGIS.DataSourcesGDB;
using SMGI.Plugin.DCDProcess.GX;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public class CheckRiverNameCheckCmd : SMGICommand
    {
        public CheckRiverNameCheckCmd()
        {
            m_category = "DataCheck";
            m_caption = "水系结构线面名称一致性检查";
            m_message = "检查水系结构线面的名称是否与其对应河流面要素的名称是否一致";
            m_toolTip = "";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;
            }
        }


        Dictionary<string, string> gbsDic = new Dictionary<string, string>();
        public String roadLyrName;
        public static String areaLryName = "面状水域";
        public String checkField;
        double len = 100;
        public override void OnClick()
        {
            CheckRiverNameForm frm = new CheckRiverNameForm();
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            len = frm.len;

            roadLyrName = frm.roadLyrName;
            checkField = frm.checkField;

            gbsDic["210000"] = "";
            gbsDic["220000"] = "";

            var hydlLyr = m_Application.Workspace.LayerManager.GetLayer((l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == roadLyrName);
            })).FirstOrDefault();
            if (hydlLyr == null)
            {
                MessageBox.Show("缺少" + roadLyrName + "要素类！");
                return;
            }
            IFeatureClass fc = (hydlLyr as IFeatureLayer).FeatureClass;

            var hydaLyr = m_Application.Workspace.LayerManager.GetLayer((l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == areaLryName);
            })).FirstOrDefault();
            if (hydaLyr == null)
            {
                MessageBox.Show("缺少" + areaLryName + "要素类！");
                return;
            }
            IFeatureClass fchyda = (hydaLyr as IFeatureLayer).FeatureClass;

            if ((fc.FindField(checkField) == -1) || (fchyda.FindField(checkField) == -1))
            {
                MessageBox.Show("需要检查的名称字段" + checkField + "有误！");
                return;
            }

            string outputFileName = frm.OutputPath + string.Format("\\水系线面名称一致性检查.shp");
            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                err = DoCheck(outputFileName, fc, fchyda, wo);

                if (err == "")
                {
                    if (File.Exists(outputFileName))
                    {
                        IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName);

                        if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            CheckHelper.AddTempLayerToMap(m_Application.ActiveView.FocusMap, errFC);
                    }
                    else
                    {
                        MessageBox.Show("检查完毕，没有发现不一致性！");
                    }
                }
                else
                {
                    MessageBox.Show(err);
                }
            }
        }

        public string DoCheck(string resultSHPFileName, IFeatureClass fc, IFeatureClass fchyda, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                Dictionary<int, string> errsDic = new Dictionary<int, string>();

                IQueryFilter qf = new QueryFilterClass();
                ISpatialFilter sf = new SpatialFilter();
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                ISpatialFilter sf2 = new SpatialFilter();
                sf2.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                foreach (var kv in gbsDic)
                {
                    qf.WhereClause = "GB = " + kv.Key;
                    // sf.WhereClause = "GB =" + kv.Key;
                    int countToltal = fc.FeatureCount(qf);
                    IFeature fe;
                    IFeatureCursor cursor = fc.Search(qf, true);
                    int count = 0;
                    while ((fe = cursor.NextFeature()) != null)
                    {
                        count++;
                        wo.SetText("正在检查【" + count + "/" + countToltal + "】");
                        string name = fe.get_Value(fc.FindField(checkField)).ToString();
                        IFeature f;
                        IPolyline py = fe.Shape as IPolyline;
                        sf.Geometry = py;
                        sf.WhereClause = "TYPE = '双线河流'";
                        ITopologicalOperator topo = py as ITopologicalOperator;
                        IFeatureCursor fcursor = fchyda.Search(sf, true);
                        while ((f = fcursor.NextFeature()) != null)
                        {
                            IRelationalOperator2 retopo = f.Shape as IRelationalOperator2;
                            if (retopo.Contains(py))//水系结构线在水系面内,也分水系面的结构线和外部河流流入水系面的连接式结构线
                            {
                                if (py.Length > len)
                                {
                                    string name1 = f.get_Value(fchyda.FindField(checkField)).ToString();
                                    if ((name1 != name))
                                    {
                                        errsDic[fe.OID] = roadLyrName + ":" + fe.OID + "与" + areaLryName +"：" + f.OID + "的字段【" + checkField + "】不一致";
                                    }
                                }
                                else
                                {
                                    sf2.Geometry = py.FromPoint;
                                    if (fc.FeatureCount(sf2) == 1)
                                    {
                                        IFeatureCursor fcursorPt = fc.Search(sf2, false);
                                        IFeature feaPt = null;
                                        while ((feaPt = fcursorPt.NextFeature()) != null)
                                        {
                                            string name1 = feaPt.get_Value(fc.FindField(checkField)).ToString();
                                            if ((name1 != name))
                                            {
                                                errsDic[fe.OID] = roadLyrName + ":" + fe.OID + "与HYDL：" + feaPt.OID + "的字段【" + checkField + "】不一致";
                                                Marshal.ReleaseComObject(feaPt);
                                                break;
                                            }
                                            Marshal.ReleaseComObject(feaPt);
                                        }
                                        Marshal.ReleaseComObject(fcursorPt);
                                        break;
                                    }
                                    sf2.Geometry = py.ToPoint;
                                    if (fc.FeatureCount(sf2) == 1)
                                    {
                                        IFeatureCursor fcursorPt = fc.Search(sf2, false);
                                        IFeature feaPt = null;
                                        while ((feaPt = fcursorPt.NextFeature()) != null)
                                        {
                                            string name1 = feaPt.get_Value(fc.FindField(checkField)).ToString();
                                            if ((name1 != name))
                                            {
                                                errsDic[fe.OID] = roadLyrName + ":" + fe.OID + "与HYDL：" + feaPt.OID + "的字段【" + checkField + "】不一致";
                                                Marshal.ReleaseComObject(feaPt);
                                                break;
                                            }
                                            Marshal.ReleaseComObject(feaPt);
                                        }
                                        Marshal.ReleaseComObject(fcursorPt);
                                    }
                                }
                                Marshal.ReleaseComObject(f);
                                break;
                            }
                            //水系结构线不在水系面内，根据相交占比判断
                            var po = topo.Intersect(f.Shape, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;//求相交部分
                            if ((po.Length / py.Length) > 0.7)
                            {
                                if (py.Length > len)
                                {
                                    string name1 = f.get_Value(fchyda.FindField(checkField)).ToString();
                                    if ((string.IsNullOrEmpty(name1) && string.IsNullOrEmpty(name)) || (name1 != name))
                                    {
                                        errsDic[fe.OID] = roadLyrName + ":" + fe.OID + "与" + areaLryName + "：" + f.OID + "的字段【" + checkField + "】不一致";
                                        Marshal.ReleaseComObject(f);
                                        break;
                                    }
                                }
                                else
                                {
                                    if (retopo.Contains(py.FromPoint))//水系结构线起点在水系面内
                                    {
                                        sf2.Geometry = py.ToPoint;
                                        IFeatureCursor fcursorPt = fc.Search(sf2, false);
                                        IFeature feaPt = null;
                                        while ((feaPt = fcursorPt.NextFeature()) != null)
                                        {
                                            string name1 = feaPt.get_Value(fc.FindField(checkField)).ToString();
                                            if ((name1 != name))
                                            {
                                                errsDic[fe.OID] = roadLyrName + ":" + fe.OID + "与HYDL：" + feaPt.OID + "的字段【" + checkField + "】不一致";
                                                Marshal.ReleaseComObject(feaPt);
                                                break;
                                            }
                                            Marshal.ReleaseComObject(feaPt);
                                        }
                                        Marshal.ReleaseComObject(fcursorPt);
                                    }
                                    else if (retopo.Contains(py.ToPoint))//水系结构线终点在水系面内
                                    {
                                        sf2.Geometry = py.FromPoint;
                                        IFeatureCursor fcursorPt = fc.Search(sf2, false);
                                        IFeature feaPt = null;
                                        while ((feaPt = fcursorPt.NextFeature()) != null)
                                        {
                                            string name1 = feaPt.get_Value(fc.FindField(checkField)).ToString();
                                            if ((name1 != name))
                                            {
                                                errsDic[fe.OID] = roadLyrName + ":" + fe.OID + "与HYDL：" + feaPt.OID + "的字段【" + checkField + "】不一致，或两者均为空值";
                                                Marshal.ReleaseComObject(feaPt);
                                                break;
                                            }
                                            Marshal.ReleaseComObject(feaPt);
                                        }
                                        Marshal.ReleaseComObject(fcursorPt);
                                    }
                                }

                                Marshal.ReleaseComObject(f);
                            }

                            //  if (retopo.Contains(py))//水系结构线在水系面内
                            // {

                            // }
                            //else if (retopo.Contains(py.FromPoint))//水系结构线起点在水系面内
                            //{
                            //    sf2.Geometry = py.ToPoint;
                            //    IFeatureCursor fcursorPt = fc.Search(sf2, false);
                            //    IFeature feaPt = null;
                            //    while ((feaPt = fcursorPt.NextFeature()) != null)
                            //    {
                            //        string name1 = feaPt.get_Value(fc.FindField(checkField)).ToString();
                            //        if (name1 != name)
                            //        {
                            //            errsDic[fe.OID] = "HYDL:" + fe.OID + "与HYDL：" + feaPt.OID + "不一致";
                            //            Marshal.ReleaseComObject(feaPt);
                            //            break;
                            //        }
                            //        Marshal.ReleaseComObject(feaPt);
                            //    }
                            //    Marshal.ReleaseComObject(fcursorPt);
                            //}
                            //else if (retopo.Contains(py.ToPoint))//水系结构线终点在水系面内
                            //{
                            //    sf2.Geometry = py.FromPoint;
                            //    IFeatureCursor fcursorPt = fc.Search(sf2, false);
                            //    IFeature feaPt = null;
                            //    while ((feaPt = fcursorPt.NextFeature()) != null)
                            //    {
                            //        string name1 = feaPt.get_Value(fc.FindField(checkField)).ToString();
                            //        if (name1 != name)
                            //        {
                            //            errsDic[fe.OID] = "HYDL:" + fe.OID + "与HYDL：" + feaPt.OID + "不一致";
                            //            Marshal.ReleaseComObject(feaPt);
                            //            break;
                            //        }
                            //        Marshal.ReleaseComObject(feaPt);
                            //    }
                            //    Marshal.ReleaseComObject(fcursorPt);
                            //}
                        }
                        Marshal.ReleaseComObject(fcursor);
                    }
                    Marshal.ReleaseComObject(cursor);
                }

                if (errsDic.Count > 0)
                {
                    if (wo != null)
                        wo.SetText("正在输出检查结果......");

                    //新建结果文件
                    ShapeFileWriter resultFile = new ShapeFileWriter();
                    Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                    fieldName2Len.Add("图层名", 50);
                    fieldName2Len.Add("要素编号", 50);
                    fieldName2Len.Add("检查项", 50);
                    fieldName2Len.Add("错误信息", 50);
                    resultFile.createErrorResutSHPFile(resultSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);

                    foreach (var item in errsDic)
                    {
                        IFeature fe = fc.GetFeature(item.Key);

                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("图层名", fc.AliasName);
                        fieldName2FieldValue.Add("要素编号", fe.OID.ToString());
                        fieldName2FieldValue.Add("检查项", "水系结构线名称一致性检查");
                        fieldName2FieldValue.Add("错误信息", item.Value);
                        resultFile.addErrorGeometry(fe.ShapeCopy, fieldName2FieldValue);
                    }

                    //保存结果文件
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

            return err;
        }

    }
}
