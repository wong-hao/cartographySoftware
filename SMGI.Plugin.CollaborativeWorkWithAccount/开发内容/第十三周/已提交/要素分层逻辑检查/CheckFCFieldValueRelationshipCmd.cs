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

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 要素分层逻辑检查
    /// </summary>
    public class CheckFCFieldValueRelationshipCmd : SMGICommand
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
            //读取配置表，获取需检查的内容
            string mdbPath = m_Application.Template.Root + "\\质检\\质检内容配置.mdb";
            if (!System.IO.File.Exists(mdbPath))
            {
                MessageBox.Show(string.Format("未找到配置文件:{0}!", mdbPath));
                return;
            }

            string tabaleName = "要素分层逻辑检查";
            DataTable dataTable = DCDHelper.ReadToDataTable(mdbPath, tabaleName);

            if (dataTable == null)
                return;

            if (dataTable.Rows.Count == 0)
            {
                MessageBox.Show("质检内容配置表不存在或内容为空！");
                return;
            }
            string tdir = OutputSetup.GetDir();
            if (tdir == "")
            {
                MessageBox.Show("请指定输出路径！");
                return;
            }
            string outputFileName = tdir + string.Format("\\要素分层逻辑检查.shp");

            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                Dictionary<IFeatureClass, List<string>> checkItems = new Dictionary<IFeatureClass, List<string>>();
                foreach (DataRow dr in dataTable.Rows)
                {
                    string fcName = dr["FCName"].ToString();
                    string domain = dr["DomainCheck"].ToString();

                    var lyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
                    })).FirstOrDefault() as IGeoFeatureLayer;
                    if (lyr == null)
                    {
                        //MessageBox.Show("缺少" + fcName + "要素类！");
                        continue;
                    }

                    IFeatureClass fc = lyr.FeatureClass;
                    if (fc == null)
                    {
                        //MessageBox.Show("缺少" + fcName + "要素类！");
                        continue;
                    }

                    if (checkItems.ContainsKey(fc))
                    {
                        checkItems[fc].Add(domain);
                    }
                    else
                    {
                        List<string> Domain = new List<string>();
                        Domain.Add(domain);

                        checkItems.Add(fc, Domain);
                    }
                }

                err = DoCheck(outputFileName, checkItems, wo);

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
        }

        public static string DoCheck(string resultSHPFileName, Dictionary<IFeatureClass, List<string>> checkItems, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                ShapeFileWriter resultFile = null;


                foreach (var kv in checkItems)
                {
                    IFeatureClass fc = kv.Key;
                    List<string> Domain = kv.Value;

                    List<int> oid = new List<int>();
                    List<int> originalOid = new List<int>();
                    List<int> newOid = new List<int>();


                    bool fieldNotFound = false; // 新增布尔变量

                    foreach (var kv2 in Domain)
                    {
                        if (wo != null)
                            wo.SetText(string.Format("正在对要素类【{0}】进行检查......", fc.AliasName));

                        // 获取查询中涉及的所有字段
                        List<string> fieldsInQuery = GetFieldsFromQuery(kv2);

                        // 检查要素类中是否存在查询所需的所有字段
                        foreach (var field in fieldsInQuery)
                        {
                            if (!FieldExistsInFeatureClass(fc, field))
                            {
                                // 在这里处理字段缺失的情况
                                MessageBox.Show("要素类" + fc.AliasName + "中缺少必要的字段" + field);

                                // 标记字段未找到
                                fieldNotFound = true;
                                break; // 跳出当前循环
                            }
                        }

                        // 如果字段未找到，则跳过当前循环
                        if (fieldNotFound)
                            break;

                        #region 检查
                        IQueryFilter qf = new QueryFilterClass();
                        // if (fc.HasCollabField())
                            qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                        //MessageBox.Show("kv2: " + kv2);
                        if (kv2 != "")
                        {
                            if (qf.WhereClause != "")
                                qf.WhereClause += " and ";
                            qf.WhereClause += string.Format("({0})", kv2);
                        }
                        else
                        {
                            if (qf.WhereClause != "")
                                qf.WhereClause += " and ";
                            qf.WhereClause += string.Format("({0})", "1=0");
                        }

                        IFeatureCursor feCursor = fc.Search(qf, true);
                        IFeature fe = null;
                        while ((fe = feCursor.NextFeature()) != null)
                        {
                            if(oid.Contains(fe.OID)) continue;
                            oid.Add(fe.OID);
                        }
                        Marshal.ReleaseComObject(feCursor);
                        #endregion
                    }

                    originalOid = GetOIDListFromFeatureClass(fc);
                    newOid = GetDifferentElements(originalOid, oid);

                    if (newOid.Count > 0)
                    {
                        if (resultFile == null)
                        {
                            //新建结果文件
                            resultFile = new ShapeFileWriter();
                            Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                            fieldName2Len.Add("要素类名", 16);
                            fieldName2Len.Add("要素编号", 16);
                            fieldName2Len.Add("检查项", 32);

                            resultFile.createErrorResutSHPFile(resultSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPoint, fieldName2Len);
                        }

                        //写入结果文件
                        foreach (var item in newOid)
                        {
                            IFeature fe = fc.GetFeature(item);

                            Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                            fieldName2FieldValue.Add("要素类名", fc.AliasName);
                            fieldName2FieldValue.Add("要素编号", item.ToString());
                            fieldName2FieldValue.Add("检查项", "要素分层逻辑检查");
                            IPoint geo = null;
                            try
                            {
                                if (fc.ShapeType == esriGeometryType.esriGeometryPoint)
                                {
                                    geo = fe.ShapeCopy as IPoint;
                                }
                                else if (fc.ShapeType == esriGeometryType.esriGeometryPolyline)
                                {
                                    geo = new PointClass();
                                    (fe.Shape as ICurve).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, geo);
                                }
                                else if (fc.ShapeType == esriGeometryType.esriGeometryPolygon)
                                {
                                    geo = (fe.Shape as IArea).LabelPoint;
                                }
                            }
                            catch
                            {
                                //求几何中心点失败，空几何返回
                            }
                            resultFile.addErrorGeometry(geo, fieldName2FieldValue);

                            Marshal.ReleaseComObject(fe);

                            //内存监控
                            if (Environment.WorkingSet > DCDHelper.MaxMem)
                            {
                                GC.Collect();
                            }
                        }

                    }//oid2FieldList.Count > 0

                }

                //保存结果文件
                if(resultFile != null)
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

        static List<int> GetOIDListFromFeatureClass(IFeatureClass fc)
        {
            List<int> originalOidList = new List<int>();

            // 使用游标遍历要素并获取 OID
            IFeatureCursor featureCursor = fc.Search(null, false);
            IFeature feature = featureCursor.NextFeature();
            while (feature != null)
            {
                originalOidList.Add(feature.OID);
                feature = featureCursor.NextFeature();
            }

            // 释放游标
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);

            return originalOidList;
        }

        static List<int> GetDifferentElements(List<int> originalOid, List<int> oid)
        {
            List<int> newOid = originalOid.Except(oid).ToList();
            return newOid;
        }

        static List<string> GetFieldsFromQuery(string query)
        {
            List<string> fields = new List<string>();

            string[] parts = query.Split(new string[] { "AND" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                string trimmedPart = part.Trim();
                int equalIndex = trimmedPart.IndexOf('=');
                if (equalIndex != -1)
                {
                    string fieldName = trimmedPart.Substring(0, equalIndex).Trim();
                    fields.Add(fieldName);
                }
            }

            return fields;
        }


        // 检查要素类中是否存在指定字段
        static bool FieldExistsInFeatureClass(IFeatureClass fc, string fieldName)
        {
            try
            {
                int fieldIndex = fc.Fields.FindField(fieldName);
                return (fieldIndex != -1);
            }
            catch (Exception ex)
            {
                // 处理查找字段时的异常情况
                // 这里你可以根据实际需求处理异常
                MessageBox.Show("查找字段时出现异常：" + ex);
                return false;
            }
        }
        
    }
}
