using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.DataSourcesGDB;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.IO;
using ESRI.ArcGIS.Controls;


namespace SMGI.Plugin.DCDProcess.DataProcess
{
    public class LakeDataRankClass : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
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
            string templatecp = GApplication.Application.Template.Root + @"\规则配置.mdb";
            string tableName = "湖泊名称等级赋值_" + referenceScale;
            DataTable ruleDataTable = DCDHelper.ReadToDataTable(templatecp, tableName);
            if (ruleDataTable == null)
            {
                return;
            }
            if (ruleDataTable.Rows.Count == 0)
            {
                MessageBox.Show(string.Format("规则表中的规则为空，没有有效的赋值规则！"));
                return;
            }

            //DataRankHelper.GradeClass(SMGI.Common.GApplication.Application, ruleDataTable);
            GradeClass(ruleDataTable);



        }

        void GradeClass(DataTable ruleDataTable)
        {
            var activeView = m_Application.ActiveView;
            IQueryFilter qf = new QueryFilterClass();

            m_Application.EngineEditor.StartOperation();
            try
            {
                using (WaitOperation wo = GApplication.Application.SetBusy())
                {
                    var layer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == "HYDA")).FirstOrDefault();
                    IFeatureLayer pFeatureLayer = layer as IFeatureLayer;
                    IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                    IFeature pFeature = null;
                    int gradeIndex = pFeatureClass.Fields.FindField("GRADE");
                    int volIndex = pFeatureClass.Fields.FindField("VOL");
                    for (int i = 0; i < ruleDataTable.Rows.Count; i++)
                    {
                        string strName2 = (ruleDataTable.Rows[i]["CONDITION"]).ToString();
                        string strtype = (ruleDataTable.Rows[i]["_TYPE"]).ToString();
                        //  if (strtype == "2") { continue; }

                        if (volIndex != -1)
                        {
                            qf.WhereClause = strName2 + " and GRADE is NULL";

                            IFeatureCursor pFC = pFeatureClass.Search(qf, false);

                            if (strtype == "0")
                            {
                                int strName3 = Convert.ToInt32(ruleDataTable.Rows[i]["GRADE"]);
                                while ((pFeature = pFC.NextFeature()) != null)
                                {
                                    wo.SetText("正在分级数据OBJECTID为：" + pFeature.OID);
                                    pFeature.set_Value(gradeIndex, strName3);
                                    pFeature.Store();
                                }
                            }
                            else if (strtype == "1")
                            {
                                //判断面积  湖泊 水库 自然湖岛
                                List<double> GradeSection = new List<double>();
                                List<string> gds = new List<string>();
                                string[] grades = ruleDataTable.Rows[i]["GRADE"].ToString().Split(';');
                                foreach (var o in grades)
                                {
                                    string[] kv = o.Split(':');
                                    GradeSection.Add(Double.Parse(kv[1]));
                                    gds.Add(kv[0]);
                                }
                                while ((pFeature = pFC.NextFeature()) != null)
                                {
                                    var area = pFeature.Shape as IArea;
                                    int k = 0;
                                    for (; k < GradeSection.Count; k++)
                                    {
                                        if (area.Area > GradeSection[k])
                                        {
                                            break;
                                        }
                                    }
                                    pFeature.set_Value(gradeIndex, gds[k]);
                                    pFeature.Store();
                                }
                            }

                            pFC.Flush();
                            Marshal.ReleaseComObject(pFC);
                        }
                        else
                        {
                            if (strtype == "2")
                            {
                                qf.WhereClause = strName2 + " and GRADE is NULL";

                                IFeatureCursor pFC = pFeatureClass.Search(qf, false);

                                //判断面积  湖泊 水库 自然湖岛
                                List<double> GradeSection = new List<double>();
                                List<string> gds = new List<string>();
                                string[] grades = ruleDataTable.Rows[i]["GRADE"].ToString().Split(';');
                                foreach (var o in grades)
                                {
                                    string[] kv = o.Split(':');
                                    GradeSection.Add(Double.Parse(kv[1]));
                                    gds.Add(kv[0]);
                                }
                                while ((pFeature = pFC.NextFeature()) != null)
                                {
                                    var area = pFeature.Shape as IArea;
                                    int k = 0;
                                    for (; k < GradeSection.Count; k++)
                                    {
                                        if (area.Area > GradeSection[k])
                                        {
                                            break;
                                        }
                                    }
                                    pFeature.set_Value(gradeIndex, gds[k]);
                                    pFeature.Store();
                                }
                                pFC.Flush();
                                Marshal.ReleaseComObject(pFC);
                            }
                        }
                    }
                }

                m_Application.EngineEditor.StopOperation("湖泊名称等级赋值");

                System.Windows.Forms.MessageBox.Show("执行完成！");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                m_Application.EngineEditor.AbortOperation();

                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            
            

        }
        private void DateRankclass_Load(object sender, EventArgs e)
        {

        }
    }
}
