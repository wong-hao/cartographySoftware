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
    public class WaterDataRankHYDCCmd : SMGI.Common.SMGICommand
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

        public override void OnClick()
        {
            string templatecp = GApplication.Application.Template.Root + @"\规则配置.mdb";
            string tableName = "水系名称等级赋初值"; 

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

            //从配置表中获取所有的HYDC尾字符-等级
            Dictionary<string, int> HYDCRankD = new Dictionary<string, int>();
            List<int> ranks = new List<int>();
            for (int i = 0; i < ruleDataTable.Rows.Count; i++)
            {
                string endString = (ruleDataTable.Rows[i]["HYDCEND"]).ToString();
                int rank = Int32.Parse((ruleDataTable.Rows[i]["GRADE"]).ToString());
                if(!HYDCRankD.ContainsKey(endString))
                {
                    HYDCRankD.Add(endString, rank);
                }                
            }

            foreach (string layerName in new List<string> { "HYDL","HYDA"})
            {
                ILayer layer = null;
                layer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == layerName)).FirstOrDefault();
                if (layer == null)
                    continue;

                IFeatureLayer pFeatureLayer = layer as IFeatureLayer;
                IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;                

                int hydcIndex = pFeatureClass.Fields.FindField("HYDC");
                int gradeIndex = pFeatureClass.Fields.FindField("GRADE");

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "HYDC IS NOT NULL AND HYDC<>''";
                if (pFeatureClass.HasCollabField())
                    qf.WhereClause += " AND "+cmdUpdateRecord.CurFeatureFilter;
                IFeatureCursor pFC = pFeatureClass.Search(qf, false);

                IFeature pFeature = null;
                m_Application.EngineEditor.StartOperation();
                try
                {
                    using (WaitOperation wo = GApplication.Application.SetBusy())
                    {
                        int changeCount = 0;
                        while ((pFeature = pFC.NextFeature()) != null)
                        {
                            string hydc = pFeature.get_Value(hydcIndex).ToString();
                            string endString = hydc[hydc.Length - 1].ToString();
                            if (HYDCRankD.ContainsKey(endString))
                            {
                                wo.SetText(layerName + ":正在赋值的OID为：" + pFeature.OID);
                                pFeature.set_Value(gradeIndex, HYDCRankD[endString]);
                                pFeature.Store();
                                changeCount += 1;
                            }
                        }
                        pFC.Flush();
                        if (changeCount > 0)
                        {
                            m_Application.EngineEditor.StopOperation("河流名称等级赋初值(根据HYDC)");
                            System.Windows.Forms.MessageBox.Show(String.Format("数据等级分级完成,修改了{0}行数据", changeCount));
                        }
                        else
                        {
                            m_Application.EngineEditor.AbortOperation();
                            System.Windows.Forms.MessageBox.Show("数据等级分级完成,但未修改数据");
                        }
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
                Marshal.ReleaseComObject(pFC);
            }
        }

    }
}

