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
            string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
            IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);
            IFeatureWorkspace fws = ws as IFeatureWorkspace;

            Geoprocessor geoprocessor = new Geoprocessor();
            geoprocessor.OverwriteOutput = true;

            int loopcount = 0;

            string fcNameArray = string.Empty;

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
                    intersect.out_feature_class = fullPath + "\\" + "IntersectedFeatures" + loopcount;

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
                    multipartToSinglepart.in_features = fullPath + "\\" + "IntersectedFeatures" + loopcount;
                    multipartToSinglepart.out_feature_class = fullPath + "\\" + "MultipartToSinglepartIntersected" + loopcount;

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
                    featureVerticesToPoints.in_features = fullPath + "\\" + "MultipartToSinglepartIntersected" + loopcount;
                    featureVerticesToPoints.point_location = "DANGLE";
                    featureVerticesToPoints.out_feature_class = fullPath + "\\" + "MultipartToSinglepartVertices" + loopcount;

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
                    addField.in_table = fullPath + "\\" + "MultipartToSinglepartVertices" + loopcount;
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

                fcNameArray = fcNameArray + fullPath + "\\" + "MultipartToSinglepartVertices" + loopcount + ";";

                loopcount++;
            }

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

            return;
        }
    }
}
