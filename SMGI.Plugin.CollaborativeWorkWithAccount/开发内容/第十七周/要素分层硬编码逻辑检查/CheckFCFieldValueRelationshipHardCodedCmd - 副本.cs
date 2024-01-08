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
using System.Data.OleDb;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessor;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 要素分层检查
    /// </summary>
    public class CheckFCFieldValueRelationshipHardCodedCmd : SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null;
            }
        }

       private static string DJ = "DJ";
       private static string GB = "GB";
       private static string CODE = "CODE";
       private static string TYPE = "TYPE";

        static Dictionary<int, List<string>> oid2FieldList = new Dictionary<int, List<string>>();

        public override void OnClick()
        {
            CheckFCFieldValueFrm frm = new CheckFCFieldValueFrm(m_Application);
            if (frm.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            List<IFeatureClass> fcList = new List<IFeatureClass>();
            List<string> fcListName = new List<string>();
            string fcNameArray = string.Empty;

            string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
            IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);
            IFeatureWorkspace fws = ws as IFeatureWorkspace;

            Geoprocessor geoprocessor = new Geoprocessor();
            geoprocessor.OverwriteOutput = true;

            foreach (var layer in frm.CheckFeatureLayerList)
            {
                IFeatureClass fc = layer.FeatureClass;
                if (!fcList.Contains(fc))
                    fcList.Add(fc);
            }
            foreach (var name in frm.CheckFeatureLayerNameList)
            {
                string fcName = name;
                if (!fcListName.Contains(fcName))
                    fcListName.Add(fcName);
            }

            //读取配置表，获取需检查的内容
            string mdbPath =frm.mdbPath;
            if (!System.IO.File.Exists(mdbPath))
            {
                MessageBox.Show(string.Format("未找到配置文件:{0}!", mdbPath));
                return;
            }

            string err = "";
            List<string> outputFileNameList = new List<string>();
            string outputFileName = string.Empty;

            using (var wo = m_Application.SetBusy())
            {
                foreach (string tableName in fcListName)
                {
                    DataTable dataTable = DCDHelper.ReadToDataTable(mdbPath, tableName);

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
                    outputFileName = tdir + string.Format("\\{0}_属性逻辑关系硬编码检查.shp", tableName);
                    outputFileNameList.Add(outputFileName);

                    Dictionary<IFeatureClass, List<string>> checkItems = new Dictionary<IFeatureClass, List<string>>();
                    HashSet<string> uniqueDJs = new HashSet<string>();
                    HashSet<string> uniqueCODEs = new HashSet<string>();

                    foreach (DataRow dr in dataTable.Rows)
                    {
                        // 获取最后一列的列名（假设最后一列是字符串类型）
                        string lastColumnName = dataTable.Columns[dataTable.Columns.Count - 1].ColumnName;
                        string secondlastColumnName = dataTable.Columns[dataTable.Columns.Count - 2].ColumnName;

                        // 遍历每一行，获取最后一列的值
                        foreach (DataRow row in dataTable.Rows)
                        {
                            // 将最后一列的值添加到 HashSet 中，确保值不为空且不重复
                            string value = row[lastColumnName].ToString();
                            string value2 = row[secondlastColumnName].ToString();
                            if (!string.IsNullOrEmpty(value) && !uniqueDJs.Contains(value))
                            {
                                uniqueDJs.Add(value);
                            }
                            if (!string.IsNullOrEmpty(value2) && !uniqueCODEs.Contains(value2))
                            {
                                uniqueCODEs.Add(value2);
                            }
                        }
                    }

                    List<string> targetDJs = uniqueDJs.ToList();
                    List<string> targetCODEs = uniqueCODEs.ToList();

                    // uniqueDJsList 中是不含重复值的 List

                    foreach (DataRow dr in dataTable.Rows)
                    {
                        int checkItemsIndex = dataTable.Rows.IndexOf(dr);

                        string targetTYPE = dr[TYPE].ToString();
                        string targetGB = dr[GB].ToString();
                        string targetCODE = dr[CODE].ToString();
                        string targetDJ = dr[DJ].ToString();

                        List<string> domain = new List<string>();
                        domain.Add(targetTYPE);
                        domain.Add(targetGB);
                        domain.Add(targetCODE);
                        domain.Add(targetDJ);

                        var lyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                        {
                            return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == tableName);
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

                        // 避免字段不存在
                        int fieldTYPEIndex = fc.Fields.FindField(targetTYPE);
                        if (fieldTYPEIndex == -1)
                        {
                            // 如果字段不存在于要素类中，记录错误信息
                        }

                        int fieldGBIndex = fc.Fields.FindField(targetGB);
                        if (fieldGBIndex == -1)
                        {
                            // 如果字段不存在于要素类中，记录错误信息
                        }

                        int fieldCODEIndex = fc.Fields.FindField(targetCODE);
                        if (fieldCODEIndex == -1)
                        {
                            // 如果字段不存在于要素类中，记录错误信息
                        }

                        int fieldDJIndex = fc.Fields.FindField(targetDJ);
                        if (fieldDJIndex == -1)
                        {
                            // 如果字段不存在于要素类中，记录错误信息
                        }

                        if (checkItems.ContainsKey(fc))
                        {
                            checkItems[fc].AddRange(domain);
                        }
                        else
                        {
                            checkItems.Add(fc, domain);
                        }

                        err = DoCheck(outputFileName, targetDJs, targetCODEs, checkItemsIndex, checkItems, wo);

                        if (err == "")
                        {

                        }
                        else
                        {
                            MessageBox.Show(err);
                        }
                    }

                    oid2FieldList.Clear();
                }

                if (outputFileNameList.Count > 0)
                {
                    foreach (var outputFileNameInList in outputFileNameList)
                    {
                        IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileNameInList);
                        CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);


                        fcNameArray = fcNameArray + errFC.AliasName + "_Temp" + ";";
                    }
                }

                Console.WriteLine("fcNameArray: " + fcNameArray);
                try
                {
                    Merge merge = new Merge();
                    fcNameArray = fcNameArray.Remove(fcNameArray.Length - 1);
                    merge.inputs = fcNameArray;
                    merge.output = fullPath + "\\" + "output";

                    Helper.ExecuteGPTool(geoprocessor, merge, null);

                    Console.WriteLine("Merge成功！");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Merge时发生错误：" + e.Message);
                    throw;
                }

                IFeatureClass tempFC = fws.OpenFeatureClass("output");

                try
                {
                    string outPutFileName = OutputSetup.GetDir() + string.Format("\\共线检查.shp");
                    CheckHelper.DeleteShapeFile(outPutFileName);
                    if (tempFC != null && tempFC.FeatureCount(null) > 0)
                    {
                        if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            CheckHelper.ExportFeatureClassToShapefile(tempFC, outPutFileName);

                            CheckHelper.AddTempLayerFromSHPFile(m_Application.Workspace.LayerManager.Map, outPutFileName);

                            Console.WriteLine("添加临时图层成功！");
                        }
                    }
                    else
                    {
                        MessageBox.Show("检查完毕！");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("添加临时图层发生错误：" + e);
                    throw;
                }
            }
        }

        public static string GetFieldByName(IFeature feature, string fieldName)
        {
            try
            {
                int fieldIndex = feature.Fields.FindField(fieldName);
                if (fieldIndex != -1)
                {
                    object value = feature.get_Value(fieldIndex);
                    return Convert.ToString(value); // 将字段值转换为字符串
                }
                else
                {
                    // 如果字段不存在，你可以根据需要进行处理，比如返回一个默认值或者抛出异常
                    //throw new ArgumentException("字段名不存在");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                // 错误处理，你可以根据需要进行记录或者处理异常
                Console.WriteLine("发生错误：" + ex.Message);
                return string.Empty;
            }
        }

        public static string DoCheck(string resultSHPFileName, List<string> targetDJs, List<string> targetCODEs, int checkItemsIndex, Dictionary<IFeatureClass, List<String>> checkItems, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                ShapeFileWriter resultFile = null;

                foreach (var kv in checkItems)
                {
                    IFeatureClass fc = kv.Key;
                    List<string> domain = kv.Value;
                    string realDJ = string.Empty;

                    IFeatureCursor feCursor = fc.Search(null, true);
                    IFeature feloop = null;
                    while ((feloop = feCursor.NextFeature()) != null)
                    {
                        if (targetDJs.Count != 0)
                        {
                            realDJ = GetFieldByName(feloop, DJ);
                            int rowIndex = targetDJs.IndexOf(realDJ);
                            if (rowIndex == -1)
                            {
                                if (!oid2FieldList.ContainsKey(feloop.OID))
                                {
                                    oid2FieldList.Add(feloop.OID, new List<string>() { DJ });
                                }
                                else
                                {
                                    // 如果已经存在该OID，检查是否已经包含了相同的字段名
                                    if (!oid2FieldList[feloop.OID].Contains(DJ))
                                    {
                                        oid2FieldList[feloop.OID].Add(DJ);
                                    }
                                }

                            }
                            else
                            {
                                if (checkItemsIndex == rowIndex)
                                {
                                    string targetCODE = domain[domain.Count - 2];
                                    string realCODE = GetFieldByName(feloop, CODE);

                                    if (!targetCODE.Equals(realCODE))
                                    {
                                        if (!oid2FieldList.ContainsKey(feloop.OID))
                                        {
                                            oid2FieldList.Add(feloop.OID, new List<string>() { CODE });
                                        }
                                        else
                                        {
                                            // 如果已经存在该OID，检查是否已经包含了相同的字段名
                                            if (!oid2FieldList[feloop.OID].Contains(CODE))
                                            {
                                                oid2FieldList[feloop.OID].Add(CODE);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string targetGB = domain[domain.Count - 3];
                                        string realGB = GetFieldByName(feloop, GB);

                                        if (!targetGB.Equals(realGB))
                                        {
                                            if (!oid2FieldList.ContainsKey(feloop.OID))
                                            {
                                                oid2FieldList.Add(feloop.OID, new List<string>() { GB });
                                            }
                                            else
                                            {
                                                // 如果已经存在该OID，检查是否已经包含了相同的字段名
                                                if (!oid2FieldList[feloop.OID].Contains(GB))
                                                {
                                                    oid2FieldList[feloop.OID].Add(GB);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            string targetTYPE = domain[domain.Count - 4];
                                            string realTYPE = GetFieldByName(feloop, TYPE);

                                            if (!targetTYPE.Equals(realTYPE))
                                            {
                                                if (!oid2FieldList.ContainsKey(feloop.OID))
                                                {
                                                    oid2FieldList.Add(feloop.OID, new List<string>() { TYPE });
                                                }
                                                else
                                                {
                                                    // 如果已经存在该OID，检查是否已经包含了相同的字段名
                                                    if (!oid2FieldList[feloop.OID].Contains(TYPE))
                                                    {
                                                        oid2FieldList[feloop.OID].Add(TYPE);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            string realCODE = GetFieldByName(feloop, CODE);

                            int rowIndex = targetCODEs.IndexOf(realCODE);
                            if (rowIndex == -1)
                            {
                                if (!oid2FieldList.ContainsKey(feloop.OID))
                                {
                                    oid2FieldList.Add(feloop.OID, new List<string>() { CODE });
                                }
                                else
                                {
                                    // 如果已经存在该OID，检查是否已经包含了相同的字段名
                                    if (!oid2FieldList[feloop.OID].Contains(CODE))
                                    {
                                        oid2FieldList[feloop.OID].Add(CODE);
                                    }
                                }

                            }
                            else
                            {
                                if (checkItemsIndex == rowIndex)
                                {
                                    string targetGB = domain[domain.Count - 3];
                                    string realGB = GetFieldByName(feloop, GB);

                                    if (!targetGB.Equals(realGB))
                                    {
                                        if (!oid2FieldList.ContainsKey(feloop.OID))
                                        {
                                            oid2FieldList.Add(feloop.OID, new List<string>() { GB });
                                        }
                                        else
                                        {
                                            // 如果已经存在该OID，检查是否已经包含了相同的字段名
                                            if (!oid2FieldList[feloop.OID].Contains(GB))
                                            {
                                                oid2FieldList[feloop.OID].Add(GB);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string targetTYPE = domain[domain.Count - 4];
                                        string realTYPE = GetFieldByName(feloop, TYPE);

                                        if (!targetTYPE.Equals(realTYPE))
                                        {
                                            if (!oid2FieldList.ContainsKey(feloop.OID))
                                            {
                                                oid2FieldList.Add(feloop.OID, new List<string>() { TYPE });
                                            }
                                            else
                                            {
                                                // 如果已经存在该OID，检查是否已经包含了相同的字段名
                                                if (!oid2FieldList[feloop.OID].Contains(TYPE))
                                                {
                                                    oid2FieldList[feloop.OID].Add(TYPE);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Marshal.ReleaseComObject(feCursor);

                    if (oid2FieldList.Count > 0)
                    {
                        if (resultFile == null)
                        {
                            //新建结果文件
                            resultFile = new ShapeFileWriter();
                            Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                            fieldName2Len.Add("要素类名", 16);
                            fieldName2Len.Add("要素编号", 16);
                            fieldName2Len.Add("不合法字段", 256);
                            fieldName2Len.Add("检查项", 32);

                            resultFile.createErrorResutSHPFile(resultSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPoint, fieldName2Len);
                        }

                        //写入结果文件
                        foreach (var item in oid2FieldList)
                        {
                            IFeature fe = fc.GetFeature(item.Key);

                            Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                            fieldName2FieldValue.Add("要素类名", fc.AliasName);
                            fieldName2FieldValue.Add("要素编号", item.Key.ToString());
                            string oidErrString = "";
                            foreach (var fd in item.Value)
                            {
                                if (oidErrString == "")
                                {
                                    oidErrString = string.Format("{0}", fd);
                                }
                                else
                                {
                                    oidErrString += string.Format(",{0}", fd);
                                }
                            }
                            fieldName2FieldValue.Add("不合法字段", oidErrString);
                            fieldName2FieldValue.Add("检查项", "属性逻辑关系硬编码检查");
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
        
    }
}
