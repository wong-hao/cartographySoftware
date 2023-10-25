using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.CartographyTools;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataManagementTools;
using System.Windows.Forms;
using ESRI.ArcGIS.Geoprocessing;

namespace SMGI.Plugin.DCDProcess
{
    public class ClickAddPolygon
    {
        public string CloseSpaceAddPolygon(IFeatureClass targetFC, List<IFeatureClass> referFCList, IQueryFilter qf, IPoint clickPoint, WaitOperation wo = null)
        {
            string err = "";

            //创建临时数据库
            string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
            IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);


            ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = new Geoprocessor();
            gp.OverwriteOutput = true;
            gp.SetEnvironmentValue("workspace", ws.PathName);

            IFeatureClass temp_union_fc = null; //所有相关面合并后的临时要素类（未融合）
            IFeatureClass temp_diss_fc = null; //Dissolve后得到的临时结果要素类

            try
            {
                if (wo != null)
                    wo.SetText("正在构建临时要素类......");

                #region 构建临时要素类
                //创建临时要素类
                temp_union_fc = DCDHelper.CreateFeatureClassStructToWorkspace(ws as IFeatureWorkspace, targetFC, targetFC.AliasName + "_temp");
                //复制相关要素
                //DCDHelper.CopyFeaturesToFeatureClass(targetFC, qf, temp_union_fc, false);
                foreach (var referFC in referFCList)
                {
                    DCDHelper.CopyFeaturesToFeatureClass(referFC, qf, temp_union_fc, false);
                }
                #endregion

                if (wo != null)
                    wo.SetText("正在修复几何......");
                #region 修复几何
                RepairGeometry reGeo = new RepairGeometry();
                reGeo.in_features = temp_union_fc.AliasName;
                SMGI.Common.Helper.ExecuteGPTool(gp, reGeo, null);
                #endregion

                #region 分区
                if (temp_union_fc.FeatureCount(null) > 10000)
                {
                    CreateCartographicPartitions gPpartition = new CreateCartographicPartitions();
                    gPpartition.in_features = temp_union_fc.AliasName;
                    gPpartition.out_features = "Partitions";
                    gPpartition.feature_count = 5000;
                    SMGI.Common.Helper.ExecuteGPTool(gp, gPpartition, null);

                    gp.SetEnvironmentValue("cartographicPartitions", ws.PathName + "\\Partitions");
                }
                #endregion

                if (wo != null)
                    wo.SetText("正在融合要素......");

                #region 融合
                Dissolve diss = new Dissolve();
                diss.in_features = temp_union_fc.AliasName;
                diss.out_feature_class = temp_union_fc.AliasName + "_diss";
                diss.multi_part = "SINGLE_PART";
                SMGI.Common.Helper.ExecuteGPTool(gp, diss, null);
                temp_diss_fc = (ws as IFeatureWorkspace).OpenFeatureClass(temp_union_fc.AliasName + "_diss");
                #endregion


                if (wo != null)
                    wo.SetText("正在检查是否为封闭空白区......");

                #region 提取微小孔洞到targetFC中
                IFeatureClassLoad pFCLoad = targetFC as IFeatureClassLoad;
                pFCLoad.LoadOnlyMode = true;
                IFeature pFeature = targetFC.CreateFeature();
                int gbindex = pFeature.Fields.FindField("GB");

                //查找是否有孔洞包含点击选取点，创建要素，并将GB置为-1
                IFeatureCursor pCursor = temp_diss_fc.Search(null, false);
                IFeature fe = pCursor.NextFeature();
                bool pand = false;
                while (fe != null)
                {
                    IPolygon plg = fe.Shape as IPolygon;

                    //找出内环，即为查找的面缝隙
                    var gc = plg as IGeometryCollection;
                    if (gc.GeometryCount > 1)
                    {
                        int index = gc.GeometryCount - 1;
                        while (index >= 0)
                        {
                            IRing r = gc.get_Geometry(index) as IRing;
                            if (!r.IsExterior)//内环
                            {
                                    IGeometryCollection shape = new PolygonClass();
                                    shape.AddGeometry(r);

                                    IPolygon polyGeo = shape as IPolygon;
                                    polyGeo.SimplifyPreserveFromTo();

                                    IRelationalOperator trackRel = polyGeo as IRelationalOperator;
                                    if (trackRel.Contains(clickPoint))
                                    {
                                        if (gbindex != -1)
                                        {
                                            pFeature.set_Value(gbindex, -1);
                                        }

                                        //创建要素
                                        pFeature.Shape = shape as IGeometry;
                                        pFeature.Store();
                                        pand = true;
                                        break;
                                    }
                            }
                            --index;
                        }
                    }

                    fe = pCursor.NextFeature();
                }
                if (pand == false)
                {
                    wo.Dispose();
                    err = "在可视视区范围内，此区域不是封闭空白区！";
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);

                pFCLoad.LoadOnlyMode = false;
                #endregion
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
                //删除临时要素类
                if (temp_union_fc != null)
                {
                    (temp_union_fc as IDataset).Delete();
                }
                if (temp_diss_fc != null)
                {
                    (temp_diss_fc as IDataset).Delete();
                }
            }

            return err;
        }
    }
}
