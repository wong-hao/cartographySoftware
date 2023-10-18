using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 检查水系结构线是否合理
    /// </summary>
    public class CheckRiverStructCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null;
            }
        }

        public String roadLyrName;
        public static String areaLryName = "面状水域";

        public override void OnClick()
        {
            CheckRiverStructFrm frm = new CheckRiverStructFrm();
            if (frm.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            roadLyrName = frm.roadLyrName;

            IGeoFeatureLayer hydlLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == roadLyrName);
            })).ToArray().First() as IGeoFeatureLayer;
            if (hydlLyr == null)
            {
                MessageBox.Show("缺少" + roadLyrName + "要素类！");
                return;
            }
            IFeatureClass hydlFC = hydlLyr.FeatureClass;

            IGeoFeatureLayer hydaLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == areaLryName);
            })).ToArray().First() as IGeoFeatureLayer;
            if (hydaLyr == null)
            {
                MessageBox.Show("缺少" + areaLryName + "要素类！");
                return;
            }
            IFeatureClass hydaFC = hydaLyr.FeatureClass;


            string outPutFileName = OutputSetup.GetDir() + string.Format("\\水系结构线检查包含.shp");

            // 创建空间过滤器
            ISpatialFilter spatialFilter = new SpatialFilterClass();
            spatialFilter.GeometryField = "Shape"; // Shape字段是几何字段的名称
            spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            // 设置hydlLyr的查询条件
            spatialFilter.WhereClause = "GB = 210000 OR GB = 220000";

            // 获取满足条件的hydlLyr要素
            IFeatureCursor hydlCursor = hydlLyr.Search(spatialFilter, true);

            // 创建选择集
            IFeatureSelection featureSelection = (IFeatureSelection)hydlLyr; // 强制类型转换为FeatureSelection
            featureSelection.Clear(); // 清除当前选择集

            // 遍历满足条件的hydlLyr要素
            IFeature hydlFeature = hydlCursor.NextFeature();
            while (hydlFeature != null)
            {
                // 设置hydaLyr的空间查询几何体为当前hydlLyr要素的几何体
                spatialFilter.Geometry = hydlFeature.Shape;

                // 设置hydaLyr的属性查询条件
                spatialFilter.WhereClause = "TYPE = '双线河流' OR TYPE = '水库' OR TYPE = '湖泊池塘'";

                // 在hydaLyr中进行空间和属性查询
                IFeatureCursor hydaCursor = hydaLyr.Search(spatialFilter, true);
                IFeature hydaFeature = hydaCursor.NextFeature();
                if (hydaFeature != null)
                {
                    // 如果有满足条件的hydaLyr要素，将其添加到选择集中
                    featureSelection.Add(hydlFeature);
                }

                // 继续处理下一个hydlLyr要素
                hydlFeature = hydlCursor.NextFeature();
            }


            // Check if there are selected features
            if (featureSelection.SelectionSet.Count > 0)
            {
                try
                {
                    MultipartToSinglepart multipartToSinglepart = new MultipartToSinglepart();
                    multipartToSinglepart.in_features = featureSelection;
                    multipartToSinglepart.out_feature_class = "hydl_MultipartToSinglepart2";

                    Geoprocessor geoprocessor = new Geoprocessor();
                    geoprocessor.OverwriteOutput = true;
                    Helper.ExecuteGPTool(geoprocessor, multipartToSinglepart, null);

                    Console.WriteLine("Multipart To Singlepart成功！");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Multipart To Singlepart时发生错误：" + e.Message);
                    throw;
                }
            }
            else
            {
                Console.WriteLine("No features selected in the 'hydlLyr' layer.");
            }

            try
            {
                Sort sort = new Sort();
                sort.in_dataset = "hydl_MultipartToSinglepart2";
                sort.sort_field = "Shape ASCENDING";
                sort.spatial_sort_method = "UR";
                sort.out_dataset = "hydl_MultipartToSinglepart2_Sor";

                Geoprocessor geoprocessor = new Geoprocessor();
                geoprocessor.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor, sort, null);


                Console.WriteLine("要素排序成功！");
            }
            catch (Exception e)
            {
                Console.WriteLine("要素排序时发生错误：" + e.Message);
                throw;
            }

            try
            {
                AddField addField = new AddField();
                addField.in_table = "hydl_MultipartToSinglepart2_Sor";
                addField.field_name = "BSM";
                addField.field_type = "TEXT";
                // addField.field_is_nullable = "false";

                Geoprocessor geoprocessor2 = new Geoprocessor();
                geoprocessor2.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor2, addField, null);

                Console.WriteLine("Add field 成功！");
            }
            catch (Exception e)
            {
                Console.WriteLine("Add field 发生错误：" + e);
                throw;
            }

            try
            {
                CalculateField calculateField = new CalculateField();
                calculateField.in_table = "hydl_MultipartToSinglepart2_Sor";
                calculateField.field = "BSM";
                calculateField.expression = "\"hl\" & [OBJECTID]";
                calculateField.expression_type = "VB";

                Geoprocessor geoprocessor2 = new Geoprocessor();
                geoprocessor2.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor2, calculateField, null);

                Console.WriteLine("Calculate field 成功！");
            }
            catch (Exception e)
            {
                Console.WriteLine("Calculate field 发生错误：" + e);
                throw;
            }

            try
            {
                Sort sort = new Sort();
                sort.in_dataset = hydaLyr;
                sort.sort_field = "Shape ASCENDING";
                sort.out_dataset = "hyda_Sort3";

                Geoprocessor geoprocessor = new Geoprocessor();
                geoprocessor.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor, sort, null);

                Console.WriteLine("要素排序成功！");
            }
            catch (Exception e)
            {
                Console.WriteLine("要素排序时发生错误：" + e.Message);
            }

            try
            {
                AddField addField = new AddField();
                addField.in_table = "hyda_Sort3";
                addField.field_name = "mianBSM";
                addField.field_type = "TEXT";
                // addField.field_is_nullable = "false";

                Geoprocessor geoprocessor2 = new Geoprocessor();
                geoprocessor2.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor2, addField, null);

                Console.WriteLine("Add field 成功！");
            }
            catch (Exception e)
            {
                Console.WriteLine("Add field 发生错误：" + e);
                throw;
            }

            try
            {
                CalculateField calculateField = new CalculateField();
                calculateField.in_table = "hyda_Sort3";
                calculateField.field = "mianBSM";
                calculateField.expression = "[OBJECTID]";
                calculateField.expression_type = "VB";

                Geoprocessor geoprocessor2 = new Geoprocessor();
                geoprocessor2.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor2, calculateField, null);

                Console.WriteLine("Calculate field 成功！");
            }
            catch (Exception e)
            {
                Console.WriteLine("Calculate field 发生错误：" + e);
                throw;
            }

            try
            {
                FeatureToLine featureToLine = new FeatureToLine();
                featureToLine.in_features = "hyda_Sort3";
                featureToLine.out_feature_class = "hyda_Sort_FeatureToLine";

                Geoprocessor geoprocessor2 = new Geoprocessor();
                geoprocessor2.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor2, featureToLine, null);

                Console.WriteLine("Feature To Line 成功！");
            }
            catch (Exception e)
            {
                Console.WriteLine("Feature To Line 发生错误：" + e);
                throw;
            }

            try
            {
                Intersect intersect = new Intersect();
                intersect.in_features = "hydl_MultipartToSinglepart2_Sor;hyda_Sort_FeatureToLine";
                intersect.join_attributes = "ALL";
                intersect.output_type = "POINT";
                intersect.out_feature_class = "hyda_Sort_FeatureToLine_Int";

                Geoprocessor geoprocessor2 = new Geoprocessor();
                geoprocessor2.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor2, intersect, null);

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
                multipartToSinglepart.in_features = "hyda_Sort_FeatureToLine_Int";
                multipartToSinglepart.out_feature_class = "hyda_Sort_FeatureToLine_Int1";

                Geoprocessor geoprocessor = new Geoprocessor();
                geoprocessor.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor, multipartToSinglepart, null);

                Console.WriteLine("Multipart To Singlepart processing completed successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Multipart To Singlepart时发生错误：" + e.Message);
                throw;
            }

            try
            {
                MultipartToSinglepart multipartToSinglepart = new MultipartToSinglepart();
                multipartToSinglepart.in_features = "hyda_Sort_FeatureToLine_Int";
                multipartToSinglepart.out_feature_class = "hyda_Sort_FeatureToLine_Int1";

                Geoprocessor geoprocessor = new Geoprocessor();
                geoprocessor.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor, multipartToSinglepart, null);

                Console.WriteLine("Multipart To Singlepart processing completed successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Multipart To Singlepart时发生错误：" + e.Message);
                throw;
            }

            try
            {
                Statistics statistics = new Statistics();
                statistics.in_table = "hyda_Sort_FeatureToLine_Int1";
                statistics.out_table = "hyda_Sort_FeatureToLine_Int2";
                statistics.statistics_fields = "mianBSM COUNT";
                statistics.case_field = "BSM;mianBSM";

                Geoprocessor geoprocessor = new Geoprocessor();
                geoprocessor.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor, statistics, null);

                Console.WriteLine("Statistics completed successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Statistics时发生错误：" + e.Message);
                throw;
            }

            try
            {
                AddField addField = new AddField();
                addField.in_table = "hyda_Sort_FeatureToLine_Int2";
                addField.field_name = "JN";
                addField.field_type = "TEXT";
                // addField.field_is_nullable = "false";

                Geoprocessor geoprocessor2 = new Geoprocessor();
                geoprocessor2.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor2, addField, null);

                Console.WriteLine("Add field 成功！");
            }
            catch (Exception e)
            {
                Console.WriteLine("Add field 发生错误：" + e);
                throw;
            }

            try
            {
                CalculateField calculateField = new CalculateField();
                calculateField.in_table = "hyda_Sort_FeatureToLine_Int2";
                calculateField.field = "JN";
                calculateField.expression = "[BSM] & [mianBSM]";
                calculateField.expression_type = "VB";

                Geoprocessor geoprocessor2 = new Geoprocessor();
                geoprocessor2.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor2, calculateField, null);

                Console.WriteLine("Calculate field 成功！");
            }
            catch (Exception e)
            {
                Console.WriteLine("Calculate field 发生错误：" + e);
                throw;
            }

            try
            {
                AddField addField = new AddField();
                addField.in_table = "hyda_Sort_FeatureToLine_Int1";
                addField.field_name = "JoinName";
                addField.field_type = "TEXT";
                // addField.field_is_nullable = "false";

                Geoprocessor geoprocessor2 = new Geoprocessor();
                geoprocessor2.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor2, addField, null);

                Console.WriteLine("Add field 成功！");
            }
            catch (Exception e)
            {
                Console.WriteLine("Add field 发生错误：" + e);
                throw;
            }

            try
            {
                CalculateField calculateField = new CalculateField();
                calculateField.in_table = "hyda_Sort_FeatureToLine_Int1";
                calculateField.field = "JoinName";
                calculateField.expression = "[BSM] & [mianBSM]";
                calculateField.expression_type = "VB";

                Geoprocessor geoprocessor2 = new Geoprocessor();
                geoprocessor2.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor2, calculateField, null);

                Console.WriteLine("Calculate field 成功！");
            }
            catch (Exception e)
            {
                Console.WriteLine("Calculate field 发生错误：" + e);
                throw;
            }

            try
            {
                JoinField joinField = new JoinField();
                joinField.in_data = "hyda_Sort_FeatureToLine_Int1";
                joinField.in_field = "JoinName";
                joinField.join_table = "hyda_Sort_FeatureToLine_Int2";
                joinField.join_field = "JN";
                joinField.fields = "FREQUENCY";
                joinField.out_layer_or_view = "hyda_Sort_FeatureToLine_Int1";

                Geoprocessor geoprocessor2 = new Geoprocessor();
                geoprocessor2.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor2, joinField, null);

                Console.WriteLine("Join field 成功！");

                IFeatureClass hyda_Sort_FeatureToLine_Int1 = null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Join field 发生错误：" + e);
                throw;
            }

            try
            {
                Select select = new Select();
                select.in_features = "hyda_Sort_FeatureToLine_Int1";
                select.out_feature_class = "found_Points";
                select.where_clause = "\"FREQUENCY\" > 2";

                Geoprocessor geoprocessor2 = new Geoprocessor();
                geoprocessor2.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor2, select, null);

                Console.WriteLine("Select 成功！");
            }
            catch (Exception e)
            {
                Console.WriteLine("Select 发生错误：" + e);
                throw;
            }

            try
            {
                Dissolve dissolve = new Dissolve();
                dissolve.in_features = "found_Points";
                dissolve.out_feature_class = "found_Pointsdis";
                dissolve.dissolve_field = "mianBSM;BSM;FREQUENCY";
                dissolve.multi_part = "MULTI_PART";

                Geoprocessor geoprocessor2 = new Geoprocessor();
                geoprocessor2.OverwriteOutput = true;
                Helper.ExecuteGPTool(geoprocessor2, dissolve, null);

                Console.WriteLine("Dissolve 成功！");
            }
            catch (Exception e)
            {
                Console.WriteLine("Dissolve 发生错误：" + e);
                throw;
            }

            IFeatureWorkspace fws = (m_Application.Workspace.EsriWorkspace as IWorkspace2) as IFeatureWorkspace;

            try
            {
                IFeatureClass tempFC = fws.OpenFeatureClass("found_Pointsdis");

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
                string[] featureClassNames = { "found_Pointsdis", "found_Points", "hyda_Sort_FeatureToLine_Int2", "hyda_Sort_FeatureToLine_Int1", "hyda_Sort_FeatureToLine_Int", "hyda_Sort_FeatureToLine", "hydl_MultipartToSinglepart2_Sor", "hydl_MultipartToSinglepart2", "hyda_Sort3" };

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
        }
    }
}
