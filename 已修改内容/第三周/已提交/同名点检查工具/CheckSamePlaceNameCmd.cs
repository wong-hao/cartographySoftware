using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geometry;
using System.IO;
using ESRI.ArcGIS.Geoprocessor;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 检查一定距离范围内是否存在其它同名点
    /// 补充（20190530）：只需检查同一级别内的同名点，如行政村“新中村”和自然村“新中村”，两者不属于同名点---《【20190508】20190418生产子系统功能问题.docx》
    /// </summary>

    public class CheckSamePlaceNameCmd : SMGICommand
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
            CheckSamePlaceNameForm frm = new CheckSamePlaceNameForm(m_Application);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string outputFileName = frm.OutputPath + string.Format("\\同名点检查_{0}.shp", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));

            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                err = DoCheck(outputFileName, frm.ObjFeatureClass, frm.FieldNameList, frm.Distance, wo);

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
                        MessageBox.Show("检查完毕，没有发现同名点！");
                    }
                }
                else
                {
                    MessageBox.Show(err);
                }
            }
        }

        public static string DoCheck(string resultSHPFileName, IFeatureClass fc, List<string> fieldNameList, double distance, WaitOperation wo = null)
        {
            string err = "";

            Dictionary<string, int> fn2Index = new Dictionary<string, int>();
            foreach (var item in fieldNameList)
            {
                int index = fc.FindField(item);
                if (index == -1)
                {
                    err = string.Format("要素类【{0}】中没有找到【{1}】字段！", fc.AliasName, item);
                    return err;
                }

                fn2Index.Add(item, index);
            }

            ITable temp_table = null;
            try
            {
                Dictionary<string, List<int>> filter2OIDList = new Dictionary<string, List<int>>();

                #region 1.在全图范围内查找同名点(频数大于1)
                if (wo != null)
                    wo.SetText("正在查找同名点......");

                #region GP
                //临时数据库
                string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
                IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);
                IFeatureWorkspace fws = ws as IFeatureWorkspace;

                Geoprocessor gp = new Geoprocessor();
                gp.OverwriteOutput = true;
                gp.SetEnvironmentValue("workspace", ws.PathName);

                string freFileds = "";
                foreach (var item in fieldNameList)
                {
                    freFileds += item + ";";
                }

                ESRI.ArcGIS.AnalysisTools.Frequency freque = new ESRI.ArcGIS.AnalysisTools.Frequency();
                freque.in_table = fc;
                freque.out_table = fc.AliasName + "_Frequency";
                freque.frequency_fields = freFileds;
                SMGI.Common.Helper.ExecuteGPTool(gp, freque, null);
                temp_table = (ws as IFeatureWorkspace).OpenTable(fc.AliasName + "_Frequency");

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "FREQUENCY > 1";
                ICursor cur = temp_table.Search(qf, true);
                IRow row = null;
                while ((row = cur.NextRow()) != null)
                {
                    string filter = "";
                    #region
                    foreach (var item in fieldNameList)
                    {
                        object val = row.get_Value(row.Fields.FindField(item));

                        if (filter != "")
                            filter += " and ";

                        if (val == null || Convert.IsDBNull(val))
                        {
                            filter += string.Format("{0} is null", item);
                        }
                        else
                        {
                            if (fc.Fields.get_Field(fn2Index[item]).Type == esriFieldType.esriFieldTypeString)
                            {
                                filter += string.Format("{0}='{1}'", item, val.ToString());
                            }
                            else
                            {
                                filter += string.Format("{0}={1}", item, val.ToString());
                            }
                        }


                    }
                    #endregion

                    List<int> oidList = new List<int>();
                    filter2OIDList.Add(filter, oidList);
                }
                Marshal.ReleaseComObject(cur);
                #endregion

                IQueryFilter qf2 = new QueryFilterClass();
                // if (fc.HasCollabField())
                {
                    qf2.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                }

                IFeatureCursor feCursor = fc.Search(qf2, true);
                IFeature fe = null;
                while ((fe = feCursor.NextFeature()) != null)
                {
                    string filter = "";
                    #region
                    foreach (var item in fieldNameList)
                    {
                        object val = fe.get_Value(fn2Index[item]);

                        if (filter != "")
                            filter += " and ";

                        if (val == null || Convert.IsDBNull(val))
                        {
                            filter += string.Format("{0} is null", item);
                        }
                        else
                        {
                            if (fc.Fields.get_Field(fn2Index[item]).Type == esriFieldType.esriFieldTypeString)
                            {
                                filter += string.Format("{0}='{1}'", item, val.ToString());
                            }
                            else
                            {
                                filter += string.Format("{0}={1}", item, val.ToString());
                            }
                        }


                    }
                    #endregion

                    if (filter2OIDList.ContainsKey(filter))
                    {
                        filter2OIDList[filter].Add(fe.OID);
                    }
                }
                Marshal.ReleaseComObject(feCursor);
                #endregion


                #region 2.根据距离阈值筛选非法同名点
                Dictionary<List<int>, string> errOIDList2Filter = new Dictionary<List<int>, string>();
                if (distance > 0)//开启了距离阈值
                {
                    foreach (var kv in filter2OIDList)
                    {
                        List<int> oidList = kv.Value;
                        if (oidList.Count < 2)
                            continue;

                        if (wo != null)
                            wo.SetText(string.Format("正在根据距离阈值筛选非法同名点【{0}】......", kv.Key));

                        //依次遍历各点，收集距离小于阈值的其它点（后续）集合
                        Dictionary<int, List<int>> errOID2OIDList = new Dictionary<int, List<int>>();
                        for (int i = 0; i < oidList.Count; ++i)
                        {
                            var p1 = fc.GetFeature(oidList[i]).Shape;

                            for (int j = i + 1; j < oidList.Count; ++j)
                            {
                                var p2 = fc.GetFeature(oidList[j]).Shape;

                                double d = (p1 as IProximityOperator).ReturnDistance(p2);
                                if (d <= distance)
                                {
                                    if (errOID2OIDList.ContainsKey(oidList[i]))
                                    {
                                        errOID2OIDList[oidList[i]].Add(oidList[j]);
                                    }
                                    else
                                    {
                                        List<int> tempOIDList = new List<int>();
                                        tempOIDList.Add(oidList[j]);

                                        errOID2OIDList.Add(oidList[i], tempOIDList);
                                    }
                                }
                            }
                        }

                        //合并分组
                        List<List<int>> errGroupOIDList = new List<List<int>>();
                        foreach (var item in errOID2OIDList)
                        {
                            List<int> tempList = new List<int>();
                            tempList.Add(item.Key);
                            tempList.AddRange(item.Value);

                            errGroupOIDList.Add(tempList);
                        }
                        foreach (var oid in oidList)
                        {
                            int firstMatchIndex = -1;
                            List<List<int>> removeOIDList = new List<List<int>>();
                            for (int i = 0; i < errGroupOIDList.Count; ++i)
                            {
                                if (errGroupOIDList[i].Contains(oid))
                                {
                                    if (firstMatchIndex == -1)
                                    {
                                        firstMatchIndex = i;
                                    }
                                    else
                                    {
                                        removeOIDList.Add(errGroupOIDList[i]);
                                    }
                                }

                            }
                            if (firstMatchIndex != -1 && removeOIDList.Count > 0)
                            {
                                foreach (var item in removeOIDList)
                                {
                                    errGroupOIDList[firstMatchIndex] = errGroupOIDList[firstMatchIndex].Union(item).ToList<int>();//合并，并去除重复值

                                    errGroupOIDList.Remove(item);//移除已合并的子组
                                }
                            }
                        }

                        foreach (var item in errGroupOIDList)
                        {
                            errOIDList2Filter.Add(item, kv.Key);
                        }

                    }
                }
                else//没有开启距离阈值
                {
                    foreach (var kv in filter2OIDList)
                    {
                        List<int> oidList = kv.Value;
                        if (oidList.Count < 2)
                            continue;

                        errOIDList2Filter.Add(kv.Value, kv.Key);
                    }
                }

                #endregion

                if (wo != null)
                    wo.SetText("正在输出检查结果......");
                if (errOIDList2Filter.Count > 0)
                {
                    //新建结果文件
                    ShapeFileWriter resultFile = new ShapeFileWriter();
                    Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                    fieldName2Len.Add("图层名", 16);
                    fieldName2Len.Add("要素编号", 16);
                    fieldName2Len.Add("分组编号", 16);
                    fieldName2Len.Add("说明", errOIDList2Filter.Values.First().Length * 2.5 > 60 ? 255 : 60);
                    fieldName2Len.Add("距离阈值", 16);
                    fieldName2Len.Add("检查项", 16);

                    resultFile.createErrorResutSHPFile(resultSHPFileName, (fc as IGeoDataset).SpatialReference, fc.ShapeType, fieldName2Len);

                    int groupIndex = 0;
                    foreach (var kv in errOIDList2Filter)
                    {
                        groupIndex++;

                        foreach (var oid in kv.Key)
                        {
                            IFeature errFe = fc.GetFeature(oid);

                            Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                            fieldName2FieldValue.Add("图层名", fc.AliasName);
                            fieldName2FieldValue.Add("要素编号", errFe.OID.ToString());
                            fieldName2FieldValue.Add("分组编号", groupIndex.ToString());
                            fieldName2FieldValue.Add("说明", kv.Value);
                            if (distance > 0)
                                fieldName2FieldValue.Add("距离阈值", distance.ToString());
                            fieldName2FieldValue.Add("检查项", "同名点检查");


                            resultFile.addErrorGeometry(errFe.Shape, fieldName2FieldValue);
                        }
                    }

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
                if (temp_table != null)
                {
                    (temp_table as IDataset).Delete();
                }
            }

            return err;
        }
    }
}
