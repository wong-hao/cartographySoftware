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
    public class FuseSmallHole
    {
        public FuseSmallHole()
        {
        }
        
        public string FuseHole(IFeatureClass targetFC, List<IFeatureClass> referFCList, IQueryFilter qf, double minHoleArea, WaitOperation wo = null)
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
                DCDHelper.CopyFeaturesToFeatureClass(targetFC, qf, temp_union_fc, false);
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                err = ex.Message;

                return err;
            }

            int smgiuserindex = targetFC.FindField(cmdUpdateRecord.CollabOPUSER);
            int guidindex = targetFC.Fields.FindField(cmdUpdateRecord.CollabGUID);
            if (guidindex == -1)
            {
                err = string.Format("要素类【{0}】中没有找到字段【{1}】！", targetFC.AliasName, cmdUpdateRecord.CollabGUID);
                return err;
            }
            

            
            try
            {
                if (wo != null)
                    wo.SetText("正在提取微小孔洞......");

                #region 提取微小孔洞到targetFC中

                IFeatureClassLoad pFCLoad = targetFC as IFeatureClassLoad;
                pFCLoad.LoadOnlyMode = true;

                IFeatureCursor pTargetFeatureCursor = targetFC.Insert(true);
                IFeatureBuffer pFeatureBuffer = targetFC.CreateFeatureBuffer();

                //复制面积小于minHoleArea的孔洞到targetFC，并将GB置为-1
                IFeatureCursor pCursor = temp_diss_fc.Search(null, true);
                IFeature fe = pCursor.NextFeature();
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
                            IArea a = gc.get_Geometry(index) as IArea;
                            if (!r.IsExterior)//内环
                            {
                                if (Math.Abs(a.Area) < minHoleArea)
                                {
                                    IGeometryCollection shape = new PolygonClass();
                                    shape.AddGeometry(r);

                                    IPolygon polyGeo = shape as IPolygon;
                                    polyGeo.SimplifyPreserveFromTo();

                                    pFeatureBuffer.set_Value(guidindex, "tempFlag");

                                    //复制要素
                                    pFeatureBuffer.Shape = shape as IGeometry;
                                    pTargetFeatureCursor.InsertFeature(pFeatureBuffer);
                                }

                            }

                            --index;
                        }
                    }

                    fe = pCursor.NextFeature();
                }
                pTargetFeatureCursor.Flush();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pTargetFeatureCursor);
                pFCLoad.LoadOnlyMode = false;

                #endregion

                if (wo != null)
                    wo.SetText("正在清除微小孔洞......");

                #region 清除微小孔洞
                IQueryFilter qf2 = new QueryFilterClass();
                qf2.WhereClause = cmdUpdateRecord.CollabGUID + " = 'tempFlag'";

                //打散
                DCDHelper.MultiToSingle(targetFC, qf2);

                //清除
                DCDHelper.EliminateIllegalFeature(targetFC, qf2, ws);

                

                //将所有要素都记录为属性更改
                if (smgiuserindex != -1)
                {
                    IFeatureCursor pFeatureCursor = targetFC.Search(null, false);
                    IFeature f = null;
                    while ((f = pFeatureCursor.NextFeature()) != null)
                    {
                        //修改属性
                        f.set_Value(smgiuserindex, cmdUpdateRecord.UserName);
                        f.Store();
                    }
                    Marshal.ReleaseComObject(pFeatureCursor);
                }

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
