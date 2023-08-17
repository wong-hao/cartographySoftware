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
using ESRI.ArcGIS.Controls;


namespace SMGI.Plugin.DCDProcess.DataProcess
{
    //河流名称等级赋值（字段名：GRADE）
    //参考配置表：规则配置.mdb\河流名称等级赋值
    //2022.3.2 修改 张怀相
    //功能修改：
    //  1、执行后加载对话框 GRADEListFrm
    //  2、对话框里设置 是否 采用 特定等级，默认采用特定等级
    //  3、从下拉列表里选择特定等级，程序只对符合该等级的河流进行GRADE赋值
    //  4、增加了修改数量的统计信息
    public class RiverDataRankClass : SMGI.Common.SMGICommand
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
            string tableName = "河流名称等级赋值_" + referenceScale;

            //读取配置表： 河流名称等级赋值
            DataTable ruleDataTable = DCDHelper.ReadToDataTable(templatecp, tableName);
            if (ruleDataTable == null)
            {
                return;
            }
            if (ruleDataTable.Rows.Count == 0)
            {
                MessageBox.Show(string.Format("规则表：{0} 中的规则为空，没有有效的赋值规则！", tableName));
                return;
            }

            //从配置表中获取所有的等级
            List<int> ranks = new List<int>();
            for (int i = 0; i < ruleDataTable.Rows.Count; i++)
            {
                int rank = Int32.Parse((ruleDataTable.Rows[i]["GRADE"]).ToString());
               
                if (!ranks.Contains(rank))
                { ranks.Add(rank); }
            }
            ranks.Sort();

            /*
            //设置河流名称等级选取对话框
            GradeListFrm frm = new GradeListFrm("河流名称等级GRADE", ranks, false);

            if (frm.ShowDialog() == DialogResult.OK)
            {
                if (frm.OneGrade)
                {
                    GradeClass(ruleDataTable,"GRADE",frm.Grade,true); 
                }
                else
                {
                    GradeClass(ruleDataTable); 
                }
            } */
            GradeClass(ruleDataTable);

        }


        void GradeClass(DataTable ruleDataTable, String gradeFieldName = "GRADE", int rankID = -1, bool skipExist = true)
        {
            string curTable = "";

            IQueryFilter qf = new QueryFilterClass();

            int changeCount = 0;
            m_Application.EngineEditor.StartOperation();
            try
            {
                using (WaitOperation wo = GApplication.Application.SetBusy())
                {
                    ILayer layer = null;
                    for (int i = 0; i < ruleDataTable.Rows.Count; i++)
                    {
                        //_TYPE==2的直接跳过（？）
                        string strtype = (ruleDataTable.Rows[i]["_TYPE"]).ToString();
                        if (strtype == "2") { continue; }

                        //非指定层的跳过
                        int strName3 = Convert.ToInt32(ruleDataTable.Rows[i][gradeFieldName]);
                        if (rankID > 0 && strName3 != rankID)
                            continue;

                        var lName = (ruleDataTable.Rows[i]["LAYERNAME"]).ToString();
                        if (!lName.Equals(curTable))
                        {
                            curTable = lName;
                            layer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == curTable)).FirstOrDefault();
                        }
                        if (layer == null)
                            continue;

                        IFeatureLayer pFeatureLayer = layer as IFeatureLayer;
                        IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
                        IFeature pFeature = null;

                        int gradeIndex = pFeatureClass.Fields.FindField(gradeFieldName);                        
                        int nameIndex = pFeatureClass.Fields.FindField("NAME");

                        string strName2 = (ruleDataTable.Rows[i]["CONDITION"]).ToString();

                        //是否跳过已填写的内容，默认跳过
                        if (skipExist)
                        {
                            qf.WhereClause = strName2 + String.Format(" and {0} is NULL",gradeFieldName);
                        }
                        else
                        {
                            qf.WhereClause = strName2; 
                        }


                        IFeatureCursor pFC = pFeatureClass.Search(qf, false);

                        if (strtype == "0")
                        {
                            while ((pFeature = pFC.NextFeature()) != null)
                            {
                                if (nameIndex != -1)
                                {
                                    //名字为“排水渠”、“排碱渠”的不参与（制图中心要求，20170531）
                                    string name = pFeature.get_Value(nameIndex).ToString();
                                    if (name == "排水渠" || name == "排碱渠")
                                        continue;
                                }

                                wo.SetText("正在分级数据OBJECTID为：" + pFeature.OID);
                                pFeature.set_Value(gradeIndex, strName3);
                                changeCount += 1;
                                pFeature.Store();
                            }
                        }
                        //else if (strtype == "1")
                        //{
                        //    //判断面积  湖泊 水库 自然湖岛
                        //    List<double> GradeSection = new List<double>();
                        //    List<string> gds = new List<string>();
                        //    string[] grades = ruleDataTable.Rows[i]["GRADE"].ToString().Split(';');
                        //    foreach (var o in grades)
                        //    {
                        //        string[] kv = o.Split(':');
                        //        GradeSection.Add(Double.Parse(kv[1]));
                        //        gds.Add(kv[0]);
                        //    }
                        //    while ((pFeature = pFC.NextFeature()) != null)
                        //    {
                        //        var area = pFeature.Shape as IArea;
                        //        int k = 0;
                        //        for (; k < GradeSection.Count; k++)
                        //        {
                        //            if (area.Area > GradeSection[k])
                        //            {
                        //                break;
                        //            }
                        //        }
                        //        pFeature.set_Value(gradeIndex, gds[k]);
                        //        pFeature.Store();
                        //    }
                        //}

                        pFC.Flush();
                        Marshal.ReleaseComObject(pFC);
                    }
                }
                if (changeCount > 0)
                {
                    m_Application.EngineEditor.StopOperation("河流名称等级赋值");
                    System.Windows.Forms.MessageBox.Show(String.Format("数据等级分级完成,修改了{0}行数据", changeCount));
                }
                else 
                {
                    m_Application.EngineEditor.AbortOperation();
                    System.Windows.Forms.MessageBox.Show("数据等级分级完成,但未修改数据");
                }
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
    }
}
