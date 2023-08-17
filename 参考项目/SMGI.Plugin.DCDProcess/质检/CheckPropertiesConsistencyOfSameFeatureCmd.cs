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

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 要素属性一致性检查：在指定图层内，检查相同要素（所有参考字段属性值相同）的待检查字段属性值是否一致
    /// </summary>
    public class CheckPropertiesConsistencyOfSameFeatureCmd : SMGICommand
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
            var frm = new CheckPropertiesConsistencyOfSameFeatureForm(m_Application);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string outputFileName = frm.OutputPath + string.Format("\\要素属性一致性检查_{0}.shp", frm.CheckFeatureLayer.Name);

            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                string filter = "";
                if (frm.CheckFeatureLayer.FeatureClass.HasCollabField())
                {
                    filter = cmdUpdateRecord.CurFeatureFilter;
                }

                err = DoCheck(outputFileName, frm.CheckFeatureLayer, filter, frm.ObjFieldName, frm.ReferFieldNameList, frm.BEliminateNullValue, wo);
            }

            if (err == "")
            {
                if (File.Exists(outputFileName))
                {
                    if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName);
                        CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                    }
                }
                else
                {
                    MessageBox.Show("检查完毕，没有发现非法要素！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }
        }


        public static string DoCheck(string resultSHPFileName, IFeatureLayer layer, string filter, string objFieldName, List<string> referFNList, bool bEliminateNullValue, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                IFeatureClass fc = layer.FeatureClass;


                //核查
                ShapeFileWriter resultFile = null;
                Dictionary<Dictionary<string, object>, Dictionary<object, List<int>>> values2FIDList = CheckPropertiesConsistency(fc, filter, objFieldName, referFNList, bEliminateNullValue, wo);//检错错误结果
                if (values2FIDList.Count > 0)
                {
                    if (resultFile == null)
                    {
                        //建立结果文件
                        resultFile = new ShapeFileWriter();
                        Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                        fieldName2Len.Add("图层名", 20);
                        fieldName2Len.Add("待查字段名", 20);
                        fieldName2Len.Add("筛选条件", 256);
                        fieldName2Len.Add("要素分布", 256);
                        fieldName2Len.Add("检查项", 32);
                        resultFile.createErrorResutSHPFile(resultSHPFileName, (fc as IGeoDataset).SpatialReference, fc.ShapeType, fieldName2Len);
                    }

                    //写入结果文件
                    if (wo != null)
                        wo.SetText("正在导出结果文件......");
                    foreach (var kv in values2FIDList)
                    {
                        string groupFilter = "";//分组检索条件
                        #region 分组检索条件
                        foreach (var item in kv.Key)
                        {
                            if (groupFilter != "")
                                groupFilter += " and ";

                            var idx = fc.FindField(item.Key);
                            if (fc.Fields.Field[idx].Type == esriFieldType.esriFieldTypeString)
                            {

                                if (Convert.IsDBNull(item.Value))
                                {
                                    groupFilter += string.Format("{0} is null", item.Key);
                                }
                                else
                                {
                                    groupFilter += string.Format("{0} = '{1}'", item.Key, item.Value);
                                }
                            }
                            else
                            {
                                if (Convert.IsDBNull(item.Value))
                                {
                                    groupFilter += string.Format("{0} is null", item.Key);
                                }
                                else
                                {
                                    groupFilter += string.Format("{0} = {1}", item.Key, item.Value);
                                }
                            }
                        }
                        #endregion

                        Dictionary<object, List<int>> val2OIDList = kv.Value;
                        val2OIDList = val2OIDList.OrderByDescending(o => o.Value.Count).ToDictionary(p => p.Key, o => o.Value);//按要素个数降序
                        string typeCount = "";//要素分布信息
                        foreach (var item in val2OIDList)
                        {
                            if (typeCount != "")
                                typeCount += "; ";

                            typeCount += string.Format("{0}({1}个)", item.Key.ToString(), item.Value.Count);
                        }
                        


                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("图层名", layer.Name);
                        fieldName2FieldValue.Add("待查字段名", objFieldName);
                        fieldName2FieldValue.Add("筛选条件", groupFilter);
                        fieldName2FieldValue.Add("要素分布", typeCount);
                        fieldName2FieldValue.Add("检查项", "要素属性一致性检查");

                        IGeometry errGeo = null;
                        bool bMaxCount = true;
                        foreach (var item in val2OIDList)
                        {
                            if (bMaxCount)
                            {
                                bMaxCount = false;
                                continue;//最大数量要素不进行几何输出
                            }

                            foreach (var oid in item.Value)
                            {
                                IFeature fe = fc.GetFeature(oid);

                                if (errGeo == null)
                                {
                                    errGeo = fe.ShapeCopy as IPolyline;
                                }
                                else
                                {
                                    ITopologicalOperator topologicalOperator = errGeo as ITopologicalOperator;
                                    errGeo = topologicalOperator.Union(fe.Shape);
                                }

                                Marshal.ReleaseComObject(fe);

                                //内存监控
                                if (Environment.WorkingSet > DCDHelper.MaxMem)
                                {
                                    GC.Collect();
                                }
                            }
                        }
                        
                        resultFile.addErrorGeometry(errGeo, fieldName2FieldValue);
                    }
                }

                //保存结果文件
                if (resultFile != null)
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

        //按参考字段属性值分组，查看每个分组要素里的目标字段属性值是否一致
        public static Dictionary<Dictionary<string, object>, Dictionary<object, List<int>>> CheckPropertiesConsistency(IFeatureClass fc, string filter, string objFieldName, List<string> referFNList, bool bEliminateNullValue, WaitOperation wo = null)
        {
            Dictionary<Dictionary<string, object>, Dictionary<object, List<int>>> result = new Dictionary<Dictionary<string, object>, Dictionary<object, List<int>>>();//Dictionary<Dictionary<参考字段名, 参考字段属性值>, Dictionary<检查字段属性值, List<要素ID>>>

            int objFNIndex = fc.FindField(objFieldName);//待检查字段索引

            if (wo != null)
                wo.SetText("正在按照参考字段属性值进行分组......");
            List<Dictionary<string, object>> groupValueList = new List<Dictionary<string, object>>();
            #region 将要素按参考字段进行分组
            string subFields = "";
            foreach (var fn in referFNList)
            {
                if (subFields == "")
                    subFields = fn;
                else
                    subFields += ", " + fn;
            }
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = filter;
            qf.SubFields = subFields;
            (qf as IQueryFilterDefinition2).PrefixClause = subFields;
            (qf as IQueryFilterDefinition2).PostfixClause = "GROUP BY " + subFields;

            //获取分组集合
            IFeatureCursor feCursor = fc.Search(qf, true);
            IFeature fe = null;
            while ((fe = feCursor.NextFeature()) != null)
            {
                bool bValid = true;//该分组是否有效

                Dictionary<string, object> valList = new Dictionary<string, object>();
                foreach (var fn in referFNList)
                {
                    var objVal = fe.get_Value(fe.Fields.FindField(fn));
                    valList.Add(fn, objVal);

                    if (bEliminateNullValue && (Convert.IsDBNull(objVal) || objVal.ToString().Trim() == ""))
                    {
                        bValid = false;
                        break;
                    }
                }

                if(bValid)
                    groupValueList.Add(valList);
            }
            Marshal.ReleaseComObject(feCursor);
            #endregion

            //遍历每个分组要素，查看同一组中待检查字段属性值是否一致
            foreach (var item in groupValueList)
            {
                Dictionary<object, List<int>> val2oidList = new Dictionary<object, List<int>>();//该组待检查字段属性值列表

                string groupFilter = "";//分组检索条件
                #region 分组检索条件
                foreach (var kv in item)
                {
                    if (groupFilter != "")
                        groupFilter += " and ";

                    var idx = fc.FindField(kv.Key);
                    if (fc.Fields.Field[idx].Type == esriFieldType.esriFieldTypeString)
                    {
                        
                        if (Convert.IsDBNull(kv.Value))
                        {
                            groupFilter += string.Format("{0} is null", kv.Key);
                        }
                        else
                        {
                            groupFilter += string.Format("{0} = '{1}'", kv.Key, kv.Value);
                        }
                    }
                    else
                    {
                        if (Convert.IsDBNull(kv.Value))
                        {
                            groupFilter += string.Format("{0} is null", kv.Key);
                        }
                        else
                        {
                            groupFilter += string.Format("{0} = {1}", kv.Key, kv.Value);
                        }
                    }
                }

                if (filter != "")
                    groupFilter = string.Format("({0}) and ({1})", groupFilter, filter);
                #endregion

                IQueryFilter groupQF = new QueryFilterClass();
                groupQF.WhereClause = groupFilter;
                feCursor = fc.Search(groupQF, true);
                while ((fe = feCursor.NextFeature()) != null)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在遍历要素【{0}】......", fe.OID));

                    var objVal = fe.get_Value(objFNIndex);
                    if (val2oidList.ContainsKey(objVal))
                    {
                        val2oidList[objVal].Add(fe.OID);
                    }
                    else
                    {
                        List<int> oidList = new List<int>();
                        oidList.Add(fe.OID);

                        val2oidList.Add(objVal, oidList);
                    }
                }
                Marshal.ReleaseComObject(feCursor);

                //属性不一致，记录信息
                if (val2oidList.Count > 1)
                {
                    result.Add(item, val2oidList);//将该组要素信息加入到结果列表
                }
            }

            return result;
        }
    }
}
