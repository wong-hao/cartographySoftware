using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesGDB;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.DCDProcess.DataProcess
{
    public class LGBAssignment : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null &&
                    m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }
        string referenceScale = "";
        public override void OnClick()
        {
            if (m_Application.MapControl.Map.ReferenceScale == 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;
            }

            if (m_Application.MapControl.Map.ReferenceScale == 1000000)
            {
                referenceScale = "100W";
            }
            else if (m_Application.MapControl.Map.ReferenceScale == 500000)
            {
                referenceScale = "50W";
            }
            else if (m_Application.MapControl.Map.ReferenceScale == 250000)
            {
                referenceScale = "25W";
            }
            else if (m_Application.MapControl.Map.ReferenceScale == 100000)
            {
                referenceScale = "10W";
            }
            else if (m_Application.MapControl.Map.ReferenceScale == 50000)
            {
                referenceScale = "5W";
            }
            else if (m_Application.MapControl.Map.ReferenceScale == 10000)
            {
                referenceScale = "1W";
            }
            else
            {
                MessageBox.Show(string.Format("未找到当前设置的参考比例尺【{0}】对应的规则表!", m_Application.MapControl.Map.ReferenceScale));
                return;
            }
            String
                templatecp = GApplication.Application.Template.Root + @"\规则配置.mdb";


            string tableName = "桥隧LGB赋值_" + referenceScale;
            DataTable ruleDataTable = DCDHelper.ReadToDataTable(templatecp, tableName);
            if (ruleDataTable == null)
            {
                return;
            }

            //  DataRankHelper.LGBAssignment(SMGI.Common.GApplication.Application, ruleDataTable);
            ReadTemplData(ruleDataTable);

            Assignment();

        }

        void Assignment()
        {

            var lfcl = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == "LFCL")).FirstOrDefault();
            var lrdl = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == "LRDL")).FirstOrDefault();

            if (lfcl == null || lrdl == null)
            {
                MessageBox.Show("图层没找到！");
                return;

            }

            var roadFcls = (lrdl as IFeatureLayer).FeatureClass;
            var fsFcls = (lfcl as IFeatureLayer).FeatureClass;

            if (roadFcls == null || fsFcls == null)
            {
                MessageBox.Show("图层没找到！");
                return;

            }

            var f_lgbIdx = fsFcls.Fields.FindField("LGB");
            var r_gbIdx = roadFcls.Fields.FindField("LGB");//modify 赵春红  根据LGB进行判断道路，不根据GB（翁中银建议）

            if (f_lgbIdx == -1 || r_gbIdx == -1)
            {
                MessageBox.Show("图层字段不存在！");
                return;
            }


            using (var wo = m_Application.SetBusy())
            {
                wo.SetText("正在查询桥梁");
                var queryfilter = new QueryFilterClass();
                queryfilter.WhereClause = "GB= 450306 OR GB = 450307";//所有桥梁
                var pCursor = fsFcls.Search(queryfilter, false);
                IFeature pfea = null;

                while (null != (pfea = pCursor.NextFeature()))
                {
                    //if (pfea.OID == 350)
                    {
                        wo.SetText("处理" + pfea.OID);
                        var sf = new SpatialFilterClass();
                        sf.Geometry = pfea.Shape;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelWithin;
                        var rCursor = roadFcls.Search(sf, true);
                        var tmpFea = rCursor.NextFeature();//一个就够
                        if (tmpFea != null)
                        {
                            try
                            {
                                int gb = (int)tmpFea.Value[r_gbIdx];
                                if (brigeDic.ContainsKey(gb))
                                {
                                    pfea.Value[f_lgbIdx] = brigeDic[gb];
                                    pfea.Store();
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Trace.WriteLine(ex.Message);
                                System.Diagnostics.Trace.WriteLine(ex.Source);
                                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                            }
                        }
                    }
                }
                Marshal.ReleaseComObject(pCursor);


                wo.SetText("正在查询隧道");
                queryfilter.WhereClause = "GB= 450602 ";//所有隧道
                pCursor = fsFcls.Search(queryfilter, false);
                while (null != (pfea = pCursor.NextFeature()))
                {
                    wo.SetText("处理" + pfea.OID);
                    var sf = new SpatialFilterClass();
                    sf.Geometry = pfea.Shape;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelWithin;
                    var rCursor = roadFcls.Search(sf, true);
                    var tmpFea = rCursor.NextFeature();//一个就够
                    if (tmpFea != null)
                    {
                        try
                        {
                            int gb = (int)tmpFea.Value[r_gbIdx];
                            if (sdDic.ContainsKey(gb))
                            {
                                pfea.Value[f_lgbIdx] = sdDic[gb];
                                pfea.Store();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine(ex.Message);
                            System.Diagnostics.Trace.WriteLine(ex.Source);
                            System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                        }
                    }

                    Marshal.ReleaseComObject(rCursor);
                }
                Marshal.ReleaseComObject(pCursor);
            }


            MessageBox.Show("处理完成！");
        }



        Dictionary<int, int> brigeDic = new Dictionary<int, int>();
        Dictionary<int, int> sdDic = new Dictionary<int, int>();

        void ReadTemplData(DataTable dt)
        {
            brigeDic.Clear();
            sdDic.Clear();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                int gb = Convert.ToInt32(dt.Rows[i]["相关图层要素GB码"]);
                int lgb = Convert.ToInt32(dt.Rows[i]["LGB"]);
                string _type = (dt.Rows[i]["_TYPE"]).ToString();
                if (_type == "1")//qiao
                {
                    brigeDic.Add(gb, lgb);
                }
                else
                {
                    sdDic.Add(gb, lgb);
                }
            }
        }
    }
}
