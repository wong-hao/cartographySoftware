using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.Runtime.InteropServices;
using System.Data;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 对LRDL中的城际道路和乡村道路进行处理，将要素的LGB码赋值为该要素的GB码
    /// </summary>
    public class RoadLGBSetCmd : SMGI.Common.SMGICommand
    {
        public RoadLGBSetCmd()
        {
            m_category = "道路LGB赋值";
            m_caption = "道路LGB赋值";
        }

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
            string layerName = "LRDL";

            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName == layerName;
            })).ToArray();

            IFeatureLayer layer = lyrs[0] as IFeatureLayer;
            if (layer == null)
            {
                MessageBox.Show(string.Format("当前数据库缺少道路图层[{0}]!", layerName), "警告", MessageBoxButtons.OK);
                return;
            }

            IFeatureClass fc = layer.FeatureClass;

            int gbIndex = fc.Fields.FindField("GB");
            int lgbIndex = fc.Fields.FindField("LGB");
            int nameIndex = fc.Fields.FindField("NAME");
            int rnIndex = fc.Fields.FindField("RN");
            if (gbIndex == -1 || lgbIndex == -1 || nameIndex == -1 || rnIndex == -1)
                return;

            //读取  规则配置.mdb\道路LGB赋值  到gb2lgb
            Dictionary<string, string> gb2lgb = new Dictionary<string, string>();
            try
            {
                string dbPath = GApplication.Application.Template.Root + @"\规则配置.mdb";
                string tableName = "道路LGB赋值";
                DataTable ruleDataTable = DCDHelper.ReadToDataTable(dbPath, tableName);
                if (ruleDataTable == null)
                {
                    return;
                }

                for (int i = 0; i < ruleDataTable.Rows.Count; i++)
                {
                    string gbVal = (ruleDataTable.Rows[i]["GB"]).ToString();
                    string lgbVal = (ruleDataTable.Rows[i]["LGB"]).ToString();
                    gb2lgb[gbVal] = lgbVal;
                }
            }
            catch(Exception err)
            {
                MessageBox.Show("读取规则配置-道路LGB赋值失败\n"+err.Message);
                return;
            }

            //根据GB和RN，按对照关系gb2lgb修改LGB
            m_Application.EngineEditor.StartOperation();

            int num = 0;
            
            using (var wo = m_Application.SetBusy())
            {                
                IQueryFilter qf = new QueryFilterClass();
                int count = 0;
                foreach (var item in gb2lgb)
                {
                    count++;
                    string gb = item.Key;
                    string lgb = item.Value;          
                    if(fc.HasCollabField())
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter + string.Format(" and {0} = {1} and (LGB IS NULL OR LGB=0)", "GB", gb);
                    else
                        qf.WhereClause = string.Format(" {0} = {1} and (LGB IS NULL OR LGB=0)", "GB", gb);
                    IFeatureCursor feCursor = fc.Search(qf, false);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        wo.SetText("正在处理要素【"  + fe.OID + "】.......");
                        string rn = fe.get_Value(rnIndex).ToString().Trim();
                        if (rn != "" && gb.StartsWith("43")) //有RN的街道暂不赋LGB
                            continue;
                        else
                            fe.set_Value(lgbIndex, Int32.Parse(lgb));
                        
                        fe.Store();

                        num++;
                    }
                    Marshal.ReleaseComObject(feCursor);
                }
            }
            
            m_Application.EngineEditor.StopOperation("道路LGB赋值");

            if (num > 0)
            {
                MessageBox.Show(string.Format("更改了{0}条要素", num));
            }
        }
    }
}
