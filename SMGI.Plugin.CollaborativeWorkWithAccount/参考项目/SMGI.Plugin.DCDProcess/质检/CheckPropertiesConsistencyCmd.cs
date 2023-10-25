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

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 数据库属性一致性检查：基于参考数据库，检查当前工作空间中要素类中各要素的属性是否发生变化
    /// 详见《0917【要素一致性检查】 .docx》
    /// </summary>
    public class CheckPropertiesConsistencyCmd : SMGICommand
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
            DataTable dataTable = DCDHelper.ReadToDataTable(mdbPath, "数据库属性一致性检查");
            if (dataTable == null)
            {
                return;
            }


            var frm = new CheckPropertiesConsistencyForm();
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Text = "数据库属性一致性检查";
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            IWorkspace referWorkspace = null;//参考数据库
            #region 读取参考数据库
            var wsf = new FileGDBWorkspaceFactoryClass();
            if (!(Directory.Exists(frm.ReferGDB) && wsf.IsWorkspace(frm.ReferGDB)))
            {
                MessageBox.Show("指定的参考数据库不合法!");
                return;
            }
            referWorkspace = wsf.OpenFromFile(frm.ReferGDB, 0);
            #endregion
            string outputFileName = frm.OutputPath + string.Format("\\{0}.shp", frm.Text);

            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                Dictionary<IFeatureClass, List<string>> fc2FNList = new Dictionary<IFeatureClass, List<string>>();
                #region 读取规则表
                foreach (DataRow dr in dataTable.Rows)
                {
                    string fcName = dr["FCName"].ToString();
                    string fieldName = dr["FieldName"].ToString();

                    var lyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName.ToUpper())
                            && ((l as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == m_Application.Workspace.EsriWorkspace.PathName;
                    })).FirstOrDefault() as IGeoFeatureLayer;
                    if (lyr == null)
                    {
                        continue;
                    }
                    IFeatureClass fc = lyr.FeatureClass;
                    if (fc == null)
                        continue;

                    if (fc2FNList.ContainsKey(fc))
                    {
                        if(!fc2FNList[fc].Contains(fieldName))
                            fc2FNList[fc].Add(fieldName);
                    }
                    else
                    {
                        List<string> fnList = new List<string>();
                        fnList.Add(fieldName);

                        fc2FNList.Add(fc, fnList);
                    }
                }
                #endregion

                err = DoCheck(outputFileName, fc2FNList, referWorkspace, wo);
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
                    MessageBox.Show("检查完毕，数据库中没有发现属性不一致要素！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }

        }
        
        /// <summary>
        /// 数据库属性一致性检查
        /// </summary>
        /// <param name="resultSHPFileName"></param>
        /// <param name="fc2FNList"></param>
        /// <param name="referDB"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public static string DoCheck(string resultSHPFileName, Dictionary<IFeatureClass, List<string>> fc2FNList, IWorkspace referDB, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                ShapeFileWriter resultFile = null;

                foreach (var kv in fc2FNList)
                {
                    IFeatureClass fc = kv.Key;
                    int guidIndex = fc.FindField(cmdUpdateRecord.CollabGUID);
                    if (guidIndex == -1)
                        continue;//不包含guid字段

                    IFeatureClass referFC = null;
                    if ((referDB as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fc.AliasName))
                    {
                        referFC = (referDB as IFeatureWorkspace).OpenFeatureClass(fc.AliasName);
                    }
                    if (referFC == null)
                        continue;//参考数据库中没有对应的要素类
                    int referGUIDIndex = referFC.FindField(cmdUpdateRecord.CollabGUID);
                    if (referGUIDIndex == -1)
                        continue;//不包含guid字段

                    List<string> fieldList = new List<string>();
                    foreach (var item in kv.Value)
                    {
                        if (fc.FindField(item) == -1 || referFC.FindField(item) == -1)
                            continue;//该字段在其中一个要素类中不存在，则该字段跳过

                        fieldList.Add(item);
                    }
                    if (fieldList.Count == 0)
                        continue;//有效比较字段为0

                    Dictionary<int, List<string>> oid2FieldList = new Dictionary<int, List<string>>();//检查结果
                    #region 检查
                    IQueryFilter qf = new QueryFilterClass();
                    if (fc.HasCollabField())
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                    //获取参考数据库要素类要素集合
                    if (wo != null)
                            wo.SetText(string.Format("正在收集参考要素类【{0}】的要素信息......", referFC.AliasName));
                    Dictionary<string, Dictionary<string, object>> referGUID2FNValue = new Dictionary<string, Dictionary<string, object>>();
                    IFeatureCursor referFeCursor = referFC.Search(qf, true);
                    IFeature referFe = null;
                    while ((referFe = referFeCursor.NextFeature()) != null)
                    {
                        string guid = referFe.get_Value(referGUIDIndex).ToString().Trim();
                        if (guid == "")
                            continue;//guid为无效值，不进行收集

                        Dictionary<string, object> fn2Value = new Dictionary<string, object>();
                        foreach (var fn in fieldList)
                        {
                            int referFNIndex = referFe.Fields.FindField(fn);
                            if (referFNIndex == -1)
                                continue;

                            fn2Value.Add(fn, referFe.get_Value(referFNIndex));
                        }

                        if(!referGUID2FNValue.ContainsKey(guid))
                            referGUID2FNValue.Add(guid, fn2Value);//通常情况下参考数据库中的guid是唯一的，若出现异常情况，则该工具只取第一个要素的属性值
                    }
                    Marshal.ReleaseComObject(referFeCursor);

                    //遍历当前工作空间中的目标要素类
                    IFeatureCursor feCursor = fc.Search(qf, true);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        if (wo != null)
                            wo.SetText(string.Format("正在对要素类【{0}】中的要素【{1}】进行属性一致性检查......", fc.AliasName, fe.OID));

                        string guid = fe.get_Value(guidIndex).ToString().Trim();
                        if (guid == "")
                            continue;//guid为无效值，不进行属性比对

                        if (!referGUID2FNValue.ContainsKey(guid))
                            continue;//参考数据库中没有找到对应guid的要素

                        //对比属性
                        foreach (var fn in fieldList)
                        {
                            int fnIndex = fe.Fields.FindField(fn);
                            if (fnIndex == -1)
                                continue;

                            var fnValue = fe.get_Value(fnIndex);
                            var referFNValue = referGUID2FNValue[guid][fn];
                            if (!fnValue.Equals(referFNValue))//属性值是否一致
                            {
                                if(oid2FieldList.ContainsKey(fe.OID))
                                {
                                    oid2FieldList[fe.OID].Add(fn);
                                }
                                else
                                {
                                    List<string> fdList = new List<string>();
                                    fdList.Add(fn);

                                    oid2FieldList.Add(fe.OID, fdList);
                                }
                            }
                        }
                    }
                    Marshal.ReleaseComObject(feCursor);
                    #endregion

                    if (oid2FieldList.Count > 0)
                    {
                        if (resultFile == null)
                        {
                            //新建结果文件
                            resultFile = new ShapeFileWriter();
                            Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                            fieldName2Len.Add("要素类名", 16);
                            fieldName2Len.Add("要素编号", 16);
                            fieldName2Len.Add("不一致字段", 256);
                            fieldName2Len.Add("检查项", 32);

                            resultFile.createErrorResutSHPFile(resultSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPoint, fieldName2Len);
                        }

                        foreach (var item in oid2FieldList)
                        {
                            fe = fc.GetFeature(item.Key);

                            Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                            fieldName2FieldValue.Add("要素类名", fc.AliasName);
                            fieldName2FieldValue.Add("要素编号", item.Key.ToString());
                            string fnErrString = "";
                            foreach (var fd in item.Value)
                            {
                                if (fnErrString == "")
                                {
                                    fnErrString = fd;
                                }
                                else
                                {
                                    fnErrString += string.Format(",{0}", fd);
                                }
                            }
                            fieldName2FieldValue.Add("不一致字段", fnErrString);
                            fieldName2FieldValue.Add("检查项", "数据库属性一致性检查");

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

        
    }
}
