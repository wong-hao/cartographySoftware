using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using System.Data;
using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.ConversionTools;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geoprocessor;
using SMGI.Plugin.DCDProcess;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{

    public class CheckLineOverlapsCmd : SMGI.Common.SMGICommand
    {
        private List<Tuple<string, string, string, string, string>> tts = new List<Tuple<string, string, string, string, string>>(); //配置信息表（单行）
        public static readonly StringBuilder _sbResult = new StringBuilder();
        public static ISpatialReference srf;
        private static List<ErrLineOverLineRelation> ErrLineOverLineList;
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null;
            }
        }

        private DataTable ruleDataTable = null;

        public override void OnClick()
        {
            ErrLineOverLineList = new List<ErrLineOverLineRelation>();
            //LGB检查规则表
            //前置条件检查：已设置参考比例尺
            if (m_Application.MapControl.Map.ReferenceScale == 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;
            }
            IWorkspace workspace = m_Application.Workspace.EsriWorkspace;
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;

            #region 读取配置
            //读取检查配置文件
            ReadConfig();

            if (ruleDataTable.Rows.Count == 0)
            {
                MessageBox.Show("质检内容配置表不存在或内容为空！");
                return;
            }

            string outPutFileName = OutputSetup.GetDir() + string.Format("\\共线检查.shp");
            #endregion

            using (var wo = m_Application.SetBusy())
            {
                Check(outPutFileName, featureWorkspace, tts);
            }

        }

        //读取质检内容配置表
        private void ReadConfig()
        {
            tts.Clear();
            string dbPath = GApplication.Application.Template.Root + @"\质检\质检内容配置.mdb";
            string tableName = "线线套合拓扑检查";
            ruleDataTable = DCDHelper.ReadToDataTable(dbPath, tableName);
            if (ruleDataTable == null)
            {
                return;
            }
            for (int i = 0; i < ruleDataTable.Rows.Count; i++)
            {
                string ptName = (ruleDataTable.Rows[i]["线状设施图层名称"]).ToString();
                string ptSQL = (ruleDataTable.Rows[i]["线状设施条件"]).ToString();
                string relName = (ruleDataTable.Rows[i]["相关图层名称"]).ToString();
                string relSQL = (ruleDataTable.Rows[i]["相关条件"]).ToString();
                string beizhu = (ruleDataTable.Rows[i]["说明"]).ToString();
                Tuple<string, string, string, string, string> tt = new Tuple<string, string, string, string, string>(ptName, ptSQL, relName, relSQL, beizhu);
                tts.Add(tt);
            }
        }

        public void Check(string outPutFileName, IFeatureWorkspace featureWorkspace, List<Tuple<string, string, string, string, string>> tts, Progress progWindow = null)
        {
            Geoprocessor geoprocessor = new Geoprocessor();
            geoprocessor.OverwriteOutput = true;
            IFeatureWorkspace fws = (m_Application.Workspace.EsriWorkspace as IWorkspace2) as IFeatureWorkspace;
            Dictionary<string, IFeatureClass> featureClasses = new Dictionary<string, IFeatureClass>();
            List<string> bufferList = new List<string>();
            int loopcount = 0;

            foreach (var tt in tts)
            {
                string plSSName = tt.Item1;
                string plSSSQL = tt.Item2;
                string plGLName = tt.Item3;
                string plGLSQL = tt.Item4;
                string beizhu = tt.Item5;
                IFeatureClass plSSFC = null;
                IFeatureClass plGLFC = null;
                try
                {
                    plSSFC = featureWorkspace.OpenFeatureClass(plSSName);
                    plGLFC = featureWorkspace.OpenFeatureClass(plGLName);
                }
                catch (Exception ex)
                {
                    //return new ResultMessage { stat = ResultState.Failed, msg = ex.Message };
                    continue;
                }

                if (plSSFC == null || plGLFC == null)
                {
                    //return new ResultMessage { stat = ResultState.Failed, msg = String.Format("{0} {1} 有空图层", plSSName, plGLName) };
                    continue;
                }

                ISpatialReference plSSRF = (plSSFC as IGeoDataset).SpatialReference;
                if (srf == null)
                {
                    srf = plSSRF;
                }
                string aliasName = plSSFC.AliasName;

                try
                {
                    Intersect intersect = new Intersect();
                    intersect.in_features = plSSFC.AliasName + ";" + plGLFC.AliasName;
                    intersect.join_attributes = "ALL";
                    intersect.output_type = "LINE";
                    intersect.out_feature_class = "IntersectedFeatures" + loopcount;

                    Helper.ExecuteGPTool(geoprocessor, intersect, null);

                    Console.WriteLine("Intersect 成功！");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Intersect 发生错误：" + e);
                    throw;
                }

                try
                {
                    MultipartToSinglepart multipartToSinglepart = new MultipartToSinglepart();
                    multipartToSinglepart.in_features = "IntersectedFeatures" + loopcount;
                    multipartToSinglepart.out_feature_class = "MultipartToSinglepartIntersected" + loopcount;

                    Helper.ExecuteGPTool(geoprocessor, multipartToSinglepart, null);

                    Console.WriteLine("Multipart To Singlepart成功！");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Multipart To Singlepart时发生错误：" + e.Message);
                    throw;
                }

                try
                {
                    FeatureVerticesToPoints featureVerticesToPoints = new FeatureVerticesToPoints();
                    featureVerticesToPoints.in_features = "MultipartToSinglepartIntersected" + loopcount;
                    featureVerticesToPoints.point_location = "DANGLE";
                    featureVerticesToPoints.out_feature_class = "MultipartToSinglepartVertices" + loopcount;

                    Helper.ExecuteGPTool(geoprocessor, featureVerticesToPoints, null);

                    Console.WriteLine("Feature Vertieces to Points成功！");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Feature Vertieces to Points时发生错误：" + e.Message);
                    throw;
                }

                try
                {
                    AddField addField = new AddField();
                    addField.in_table = "MultipartToSinglepartVertices" + loopcount;
                    addField.field_name = "说明";
                    addField.field_type = "TEXT";
                    addField.field_is_nullable = "NULLABLE";
                    addField.field_is_required = "NON_REQUIRED";

                    Helper.ExecuteGPTool(geoprocessor, addField, null);

                    Console.WriteLine("Add field 成功！");

                    // 获取表格对象
                    ITable table = fws.OpenTable("MultipartToSinglepartVertices" + loopcount);

                    // 更新新字段的值
                    string fieldName = "说明";
                    string defaultValue = beizhu;

                    // 创建查询过滤器
                    IQueryFilter queryFilter = new QueryFilter();
                    queryFilter.WhereClause = "1=1"; // 更新所有行

                    // 使用ICursor进行更新操作
                    ICursor cursor = table.Update(queryFilter, false);
                    IRow row = cursor.NextRow();
                    int fieldIndex = table.FindField(fieldName);

                    while (row != null)
                    {
                        row.set_Value(fieldIndex, defaultValue);
                        cursor.UpdateRow(row);
                        row = cursor.NextRow();
                    }

                    Marshal.ReleaseComObject(cursor);
                    Console.WriteLine("字段赋值成功！");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Add field 发生错误：" + e);
                    throw;
                }

                featureClasses.Add("MultipartToSinglepartVertices" + loopcount, fws.OpenFeatureClass("MultipartToSinglepartVertices" + loopcount));

                if (loopcount == 0)
                {
                    bufferList.Add("output");

                    try
                    {
                        FeatureClassToFeatureClass featureClassToFeatureClass = new FeatureClassToFeatureClass();
                        featureClassToFeatureClass.in_features = "MultipartToSinglepartVertices" + loopcount; // 输入要素类
                        featureClassToFeatureClass.out_path = fws; // 输出要素类的路径（当前工作空间）
                        featureClassToFeatureClass.out_name = "output"; // 输出要素类的名称

                        Helper.ExecuteGPTool(geoprocessor, featureClassToFeatureClass, null);

                        Console.WriteLine("FeatureClass to FeatureClass成功！");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FeatureClass to FeatureClass时发生错误：" + e.Message);
                        throw;
                    }
                }
                
                bufferList.Add("IntersectedFeatures" + loopcount);
                bufferList.Add("MultipartToSinglepartIntersected" + loopcount);
                bufferList.Add("MultipartToSinglepartVertices" + loopcount);

                loopcount++;
            }

            bool firstIteration = true; // 标志位，用于判断是否是第一次循环

            // 获取新要素类的编辑游标
            IFeatureClass tempFC = fws.OpenFeatureClass("output");
            IFeatureCursor insertCursor = tempFC.Insert(true);

            // 遍历每个原始要素类，将其要素插入到新要素类中
            foreach (var kvp in featureClasses)
            {
                if (firstIteration)
                {
                    firstIteration = false; // 将标志位设置为false，跳过第一次循环
                    continue; // 跳过第一次循环
                }

                // 获取源要素类的字段集合
                IFields sourceFields = kvp.Value.Fields;
                IFeatureCursor cursor = kvp.Value.Search(null, true);
                IFeature feature = cursor.NextFeature();
                while (feature != null)
                {
                    // 创建新要素类的要素缓冲区
                    IFeatureBuffer featureBuffer = tempFC.CreateFeatureBuffer();
                    featureBuffer.Shape = feature.ShapeCopy; // 复制几何信息

                    // 设置其他字段的值
                    for (int i = 0; i < sourceFields.FieldCount; i++)
                    {
                        IField sourceField = sourceFields.Field[i];
                        if (sourceField.Type != esriFieldType.esriFieldTypeOID &&
                            sourceField.Type != esriFieldType.esriFieldTypeGeometry)
                        {
                            // 如果字段不是OID字段或几何字段，将其值从源要素复制到新要素
                            int fieldIndex = tempFC.FindField(sourceField.Name);
                            if (fieldIndex != -1)
                            {
                                // 找到相应字段在新要素类中的索引
                                featureBuffer.set_Value(fieldIndex, feature.get_Value(i));
                            }
                        }
                    }

                    // 插入要素到新的要素类中
                    insertCursor.InsertFeature(featureBuffer);
                    feature = cursor.NextFeature();
                }
                Marshal.ReleaseComObject(cursor);
            }

            // 释放新要素类的编辑游标
            Marshal.ReleaseComObject(insertCursor);

            try
            {
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

            try
            {
                // 要删除的要素类名称数组
                string[] featureClassNames = bufferList.ToArray();

                foreach (string featureClassName in featureClassNames)
                {
                    try
                    {
                        // 打开要删除的要素类
                        IFeatureClass featureClass = fws.OpenFeatureClass(featureClassName);

                        // 如果要素类存在，则删除
                        if (featureClass != null)
                        {
                            // 获取数据集
                            IDataset dataset = featureClass as IDataset;

                            // 删除要素类
                            dataset.Delete();

                            Console.WriteLine("要素类 " + featureClassName + " 删除成功！");
                        }
                        else
                        {
                            Console.WriteLine("要素类 " + featureClassName + " 不存在或无法打开。");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("删除要素类 " + featureClassName + " 时发生错误：" + ex.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("删除要素类时发生错误：" + e.Message);
                throw;
            }
            return;
        }
    }
}
