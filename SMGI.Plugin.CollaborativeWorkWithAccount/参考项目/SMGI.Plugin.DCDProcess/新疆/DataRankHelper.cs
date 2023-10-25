using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Data;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;

namespace SMGI.Plugin.DCDProcess.DataProcess
{
    class DataRankHelper
    {
        //2022.2.28 修改 张怀相
        // 增加了参数选项
        //   1、gradeFieldName="GRADE2"（默认字段名，长整型）
        //   2、指定rankID  -1 表示无特定等级赋值；整数表示按指定等级赋值
        //   3、skipExist = false 是否跳过已有数据，Null被认为是空，非Null为已有数据
        /// <summary>
        /// 道路选取等级GRADE2赋值函数
        /// </summary>
        /// <param name="application"></param>
        /// <param name="ruleDataTable"></param>
        public static void GradeClass(GApplication application, DataTable ruleDataTable,String gradeFieldName="GRADE2",int rankID=-1,bool skipExist = false)
        {
            int changeCount = 0;
            var activeView = application.ActiveView;
            IQueryFilter qf = new QueryFilterClass();
            application.EngineEditor.StartOperation();
            using (WaitOperation wo = GApplication.Application.SetBusy())
            {                
                for (int i = 0; i < ruleDataTable.Rows.Count; i++)
                {
                    int strName3 = Convert.ToInt32(ruleDataTable.Rows[i][gradeFieldName]);                     
                    if(rankID>0 &&strName3 != rankID)
                        continue;

                    string strName = ruleDataTable.Rows[i]["LAYERNAME"].ToString();
                    var layer = application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == strName)).FirstOrDefault();
                    IFeatureLayer pFeatureLayer = layer as IFeatureLayer;
                    IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                    string strName2 = (ruleDataTable.Rows[i]["CONDITION"]).ToString();
                    qf.WhereClause = strName2;
                    IFeatureCursor pFC = pFeatureClass.Search(qf, false);
					
                    string gradeFN = GApplication.Application.TemplateManager.getFieldAliasName(gradeFieldName, pFeatureClass.AliasName);
                    int gradeIndex = pFeatureClass.FindField(gradeFN);
                    if (gradeIndex == -1)
                    {
                        MessageBox.Show(string.Format("图层{0}没有找到字段：{1}！", strName,gradeFN));
                        break;
                    }

                    IFeature pFeature = null;                    
                    while ((pFeature = pFC.NextFeature()) != null)
                    {
                        int gradeIndexO = pFeature.Fields.FindField("OBJECTID");
                        string sO = (pFeature.get_Value(gradeIndexO)).ToString();
                        wo.SetText("正在分级数据OBJECTID为：" + sO);
                        var gradeValue = pFeature.get_Value(gradeIndex).ToString();
                        if (!String.IsNullOrEmpty(gradeValue) && skipExist)
                            continue;
                        pFeature.set_Value(gradeIndex, strName3);
                        pFeature.Store();
                        changeCount += 1;
                    }
                }
            }
            if (changeCount > 0)
            {
                application.EngineEditor.StopOperation("选取等级分级");
                System.Windows.Forms.MessageBox.Show(String.Format("数据等级分级完成,修改了{0}行数据", changeCount));
            }
            else
            {
                application.EngineEditor.AbortOperation();
                System.Windows.Forms.MessageBox.Show("数据等级分级完成,但未修改数据");
            }            
        }

        /// <summary>
        /// 桥梁、隧道LGB赋值函数
        /// </summary>
        /// <param name="application"></param>
        /// <param name="ruleDataTable"></param>
        public static void LGBAssignment(GApplication application, DataTable ruleDataTable)
        {
            var activeView = application.ActiveView;
            IQueryFilter qf = new QueryFilterClass();
            using (WaitOperation wo = GApplication.Application.SetBusy())
            {
                for (int i = 0; i < ruleDataTable.Rows.Count; i++)
                {
                    int LGb = Convert.ToInt32(ruleDataTable.Rows[i]["相关图层要素GB码"]);
                    int Lgb = Convert.ToInt32(ruleDataTable.Rows[i]["LGB"]);
                    string Condition = (ruleDataTable.Rows[i]["条件"]).ToString();
                    string strNameB = ruleDataTable.Rows[i]["目标图层名"].ToString();
                    string strNameL = ruleDataTable.Rows[i]["相关图层名"].ToString();
                    var layerB = application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == strNameB)).FirstOrDefault();
                    var layerL = application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == strNameL)).FirstOrDefault();
                    IFeatureLayer pFeatureLayerB = layerB as IFeatureLayer;
                    IFeatureLayer pFeatureLayerL = layerL as IFeatureLayer;
                    IFeatureClass pFeatureClassB = pFeatureLayerB.FeatureClass;
                    IFeatureClass pFeatureClassL = pFeatureLayerL.FeatureClass;
                    qf.WhereClause = Condition;
                    IFeatureCursor pFC = pFeatureClassB.Search(qf, false);
                    IFeature pFeature = null;
                    while ((pFeature = pFC.NextFeature()) != null)
                    {
                        int gradeIndexO = pFeature.Fields.FindField("OBJECTID");
                        string sO = (pFeature.get_Value(gradeIndexO)).ToString();
                        int gradeIndex = pFeature.Fields.FindField("LGB");
                        if (gradeIndex == -1)
                        {
                            wo.Dispose();
                            System.Windows.Forms.MessageBox.Show("符合条件数据不含有LGB字段！");
                            return;
                        }
                        wo.SetText("正在赋值数据OBJECTID为：" + sO);
                        IPolyline pPolyline = pFeature.Shape as IPolyline;
                        IGeometry pGeometry = pPolyline as IGeometry;
                        SpatialFilterClass sf = new SpatialFilterClass();
                        sf.Geometry = pGeometry;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelWithin;
                        IFeatureCursor pFeatureCursorL = pFeatureClassL.Search(sf, false);
                        IFeature pFeatureL = null;
                        while ((pFeatureL = pFeatureCursorL.NextFeature()) != null)
                        {
                            int gradeIndexL = pFeatureL.Fields.FindField("GB");
                            int LS = Convert.ToInt32(pFeatureL.get_Value(gradeIndexL));
                            if (LS == LGb)
                            {
                                pFeature.set_Value(gradeIndex, Lgb);
                                pFeature.Store();
                            }
                        }
                    }
                }

                var layerLN = application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == "LFCL")).FirstOrDefault();
                IFeatureLayer pFeatureLayerLN = layerLN as IFeatureLayer;
                IFeatureClass pFeatureClassLN = pFeatureLayerLN.FeatureClass;
                var layerLoad = application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == "LRDL")).FirstOrDefault();
                IFeatureLayer pFeatureLayerLoad = layerLoad as IFeatureLayer;
                IFeatureClass pFeatureClassLoad = pFeatureLayerLoad.FeatureClass;
                IQueryFilter nf = new QueryFilterClass();
                nf.WhereClause = "(GB = 450305 OR GB = 450306 OR GB = 450307 OR GB = 450601 OR GB = 450602) AND (LGB is null)";
                IFeatureCursor pLN = pFeatureClassLN.Search(nf, false);
                IFeature pFeatureLN = null;
                while ((pFeatureLN = pLN.NextFeature()) != null)
                {
                    int gradeIndexOB = pFeatureLN.Fields.FindField("OBJECTID");
                    string sOB = (pFeatureLN.get_Value(gradeIndexOB)).ToString();
                    wo.SetText("正在赋值数据OBJECTID为：" + sOB);
                    int gbIndexB = pFeatureLN.Fields.FindField("GB");
                    int GbB = Convert.ToInt32(pFeatureLN.get_Value(gbIndexB));
                    IPolyline pPolyline = pFeatureLN.Shape as IPolyline;
                    IGeometry pGeometry = pPolyline as IGeometry;
                    SpatialFilterClass sf = new SpatialFilterClass();
                    sf.Geometry = pGeometry;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    IFeatureCursor pFeatureCursorLoad = pFeatureClassLoad.Search(sf, false);
                    IFeature pFeatureLoad = null;
                    long roadGB = 0;
                    double maxLen = 0;
                    while ((pFeatureLoad = pFeatureCursorLoad.NextFeature()) != null)
                    {
                        IGeometry outGeo = pFeatureLN.ShapeCopy;
                        ITopologicalOperator outTopo = outGeo as ITopologicalOperator;
                        IGeometry intersOrigGeo = outTopo.Intersect(pFeatureLoad.Shape, esriGeometryDimension.esriGeometry1Dimension);
                        if (intersOrigGeo.IsEmpty)
                        {
                            continue;
                        }
                        if (intersOrigGeo is IPolyline)
                        {
                            if ((intersOrigGeo as IPolyline).Length > maxLen)
                            {

                                int gbIndex = pFeatureLoad.Fields.FindField("GB");
                                if (gbIndex != -1)
                                {
                                    maxLen = (intersOrigGeo as IPolyline).Length;
                                    long.TryParse(pFeatureLoad.get_Value(gbIndex).ToString(), out roadGB);
                                }
                            }
                        }
                    }
                    DataRow[] drArray = ruleDataTable.Select().Where(i => i["相关图层要素GB码"].ToString() == roadGB.ToString() && i["条件"].ToString().Contains(GbB.ToString())).ToArray();
                    int L = Convert.ToInt32(drArray[0]["LGB"]);
                    int gbIndexL = pFeatureLN.Fields.FindField("LGB");
                    pFeatureLN.set_Value(gbIndexL, L);
                    pFeatureLN.Store();
                }
            }
            System.Windows.Forms.MessageBox.Show("数据LGB赋值完成");
        }
    }
}
