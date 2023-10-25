using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using System.Data;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 街区道路分类码赋值（按所在居民地级别划分）工具
    /// 详细参见需求文档《街区道路CLASS1赋值20180408.docx》
    /// </summary>
    public class RoadClass1CmdJS : SMGI.Common.SMGICommand
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
            string lrdlLyrName = "LRDL";
            var lrdlLayer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
                        ((l as IGeoFeatureLayer).Name.Trim().ToUpper() == lrdlLyrName)).FirstOrDefault() as IFeatureLayer;
            if (lrdlLayer == null)
            {
                MessageBox.Show(string.Format("当前工作空间中没有找到图层【{0}】!", lrdlLyrName));
                return;
            }
            var lrdlFC = lrdlLayer.FeatureClass;

            string resaLyrName = "RESA";
            var resaLayer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
                        ((l as IGeoFeatureLayer).Name.Trim().ToUpper() == resaLyrName)).FirstOrDefault() as IFeatureLayer;
            if (resaLayer == null)
            {
                MessageBox.Show(string.Format("当前工作空间中没有找到图层【{0}】!", resaLyrName));
                return;
            }
            var resaFC = resaLayer.FeatureClass;

            string dbPath = m_Application.Template.Root + @"\规则配置.mdb";
            string tableName = "道路CLASS1赋值";
            DataTable ruleDataTable = DCDHelper.ReadToDataTable(dbPath, tableName);
            if (ruleDataTable == null)
            {
                return;
            }

            int num = 0;
            try
            {
                m_Application.EngineEditor.StartOperation();


                using (WaitOperation wo = GApplication.Application.SetBusy())
                {
                    num = LRDLClass1ByRESA(lrdlFC, resaFC, ruleDataTable, wo);
                }

                m_Application.EngineEditor.StopOperation("街区道路分类码赋值");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                m_Application.EngineEditor.AbortOperation();

                MessageBox.Show(ex.Message);

                return;
            }

            MessageBox.Show(string.Format("更改了{0}条要素", num));
        }

        private int LRDLClass1ByRESA(IFeatureClass lrdlFC, IFeatureClass resaFC, DataTable ruleDataTable, WaitOperation wo = null)
        {
            int num = 0;

            for (int i = 0; i < ruleDataTable.Rows.Count; i++)
            {
                string resaFilterStr = (ruleDataTable.Rows[i]["居民地面要素条件"]).ToString();
                string lrdlFilterStr = (ruleDataTable.Rows[i]["道路要素条件"]).ToString();
                string lrdlClass1FN = (ruleDataTable.Rows[i]["道路分类码字段名"]).ToString();
                string lrdlClass1Val = (ruleDataTable.Rows[i]["道路分类值"]).ToString();

                lrdlClass1FN = m_Application.TemplateManager.getFieldAliasName(lrdlClass1FN, lrdlFC.AliasName);
                int lrdlClass1Index = lrdlFC.Fields.FindField(lrdlClass1FN);
                if (lrdlClass1Index == -1)
                {
                    throw new Exception(string.Format("要素类【{0}】中没有找到字段【{1}】!", lrdlFC.AliasName, lrdlClass1FN));
                }

                IQueryFilter qf = new QueryFilterClass();
                if (resaFC.HasCollabField())
                    qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                if (resaFilterStr != "")
                {
                    if (qf.WhereClause != "")
                        qf.WhereClause += string.Format(" and ({0})", resaFilterStr);
                    else
                        qf.WhereClause = resaFilterStr;
                }
                IFeatureCursor resaFeCursor = resaFC.Search(qf, true);
                IFeature resaFe = null;
                while ((resaFe = resaFeCursor.NextFeature()) != null)
                {
                    if (resaFe.Shape == null || resaFe.Shape.IsEmpty)
                        continue;

                    if (wo != null)
                        wo.SetText(string.Format("正在遍历街区要素【{0}】......", resaFe.OID));

                    ISpatialFilter sf = new SpatialFilterClass();
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    sf.Geometry = resaFe.Shape;
                    sf.WhereClause = lrdlFilterStr;
                    IFeatureCursor feCursor = lrdlFC.Search(sf, false);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        fe.set_Value(lrdlClass1Index, lrdlClass1Val);
                        fe.Store();

                        num++;
                    }
                    Marshal.ReleaseComObject(feCursor);
                }
                Marshal.ReleaseComObject(resaFeCursor);
            }

            return num;
        }
    }
}
