using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using System.Runtime.InteropServices;
using System.Data;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 根据要素的GB或称CLASS，按要素名称YSMC赋值表对YSMC字段进行赋值（湖南特殊要求）
    /// </summary>
    public class SetYSMCCmd : SMGI.Common.SMGICommand
    {
        private Dictionary<string, IFeatureClass> _fcName2FC;
        private Dictionary<string, List<Tuple<string, string>>> _fc2sqlysmc;
        public SetYSMCCmd()
        {
            m_category = "要素名称赋值";
            m_caption = "要素名称赋值";
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
            _fcName2FC = GetEditFCs();
            _fc2sqlysmc = ReadYSMCConfig();

            var frm = new CheckLayerSelectForm(m_Application, true, true, true, false, false);
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Text = "要素名称赋值";

            if (frm.ShowDialog() != DialogResult.OK)
                return;

            List<IFeatureClass> fcList = new List<IFeatureClass>();
            foreach (var layer in frm.CheckFeatureLayerList)
            {
                IFeatureClass fc = layer.FeatureClass;
                if (!fcList.Contains(fc))
                    fcList.Add(fc);
            }

            string temp_fcName = "";
            try
            {
                m_Application.EngineEditor.StartOperation();

                using (var wo = m_Application.SetBusy())
                {
                    foreach (KeyValuePair<string, IFeatureClass> kvp in _fcName2FC)
                    {
                        string fcName = kvp.Key;
                        IFeatureClass fc = kvp.Value;
                        temp_fcName = fcName;
                        if (_fc2sqlysmc.ContainsKey(fcName) && fcList.Contains(fc))
                        {
                            int ysmcIdx = fc.FindField("YSMC");
                            if (-1 == ysmcIdx)
                            {
                                MessageBox.Show(fcName + "没有字段YSMC");
                                continue;
                            }
                            wo.SetText("正在处理要素【" + fcName + "】.......");

                            foreach (Tuple<string, string> sql_ysmc in _fc2sqlysmc[fcName])
                            {
                                string sql = sql_ysmc.Item1;
                                string ysmc = sql_ysmc.Item2;
                                var qfilter = new QueryFilterClass();
                                qfilter.WhereClause = sql;
                                var pCursor = fc.Search(qfilter, false);
                                IFeature pFea = null;

                                while (null != (pFea = pCursor.NextFeature()))
                                {
                                    wo.SetText("正在处理要素【" + fcName + "】......." + pFea.OID);
                                    if (pFea.Value[ysmcIdx].ToString() != ysmc)
                                    {
                                        pFea.Value[ysmcIdx] = ysmc;
                                        pFea.Store();
                                    }
                                }

                                Marshal.ReleaseComObject(pCursor);
                            }

                        }
                    }
                }
                m_Application.EngineEditor.StopOperation("要素名称赋值");
                MessageBox.Show("要素赋值完成");
            }
            catch (Exception err)
            {
                m_Application.EngineEditor.AbortOperation();
                
                System.Diagnostics.Trace.WriteLine(err.Message);
                System.Diagnostics.Trace.WriteLine(err.Source);
                System.Diagnostics.Trace.WriteLine(err.StackTrace);

                MessageBox.Show(String.Format("{0}-赋值失败:{1}", temp_fcName,err.Message));
            }
        }

        private Dictionary<string, IFeatureClass> GetEditFCs()
        {
            Dictionary<string, IFeatureClass> fcName2FC=new Dictionary<string,IFeatureClass>();
            IEngineEditLayers editLayer = m_Application.EngineEditor as IEngineEditLayers;

            //获取所有可编辑图层列表
            var pLayers = m_Application.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
                (l is IGeoFeatureLayer))).ToArray();
            
            for (int i = 0; i < pLayers.Length; i++)
            {
                IFeatureLayer layer = pLayers[i] as IFeatureLayer;
                if ((layer.FeatureClass as IDataset).Workspace.PathName != m_Application.Workspace.EsriWorkspace.PathName)//临时数据不参与
                    continue;

                if (!editLayer.IsEditable(layer))
                    continue;


                fcName2FC.Add(layer.Name.ToUpper(), layer.FeatureClass);
            }
            return fcName2FC;
        }
        private Dictionary<string, List<Tuple<string, string>>> ReadYSMCConfig()
        {
            //读取  规则配置.mdb\要素名称YSMC赋值  到
            Dictionary<string, List<Tuple<string, string>>> fc2sqlysmc = new Dictionary<string, List<Tuple<string, string>>>();
            try
            {
                string dbPath = GApplication.Application.Template.Root + @"\规则配置.mdb";
                string tableName = "要素名称赋值";
                DataTable ruleDataTable = DCDHelper.ReadToDataTable(dbPath, tableName);
                if (ruleDataTable == null)
                {
                    return null;
                }

                for (int i = 0; i < ruleDataTable.Rows.Count; i++)
                {
                    string fcName = (ruleDataTable.Rows[i]["图层名"]).ToString();
                    //string gb = (ruleDataTable.Rows[i]["GB_CLASS"]).ToString();
                    string sql = (ruleDataTable.Rows[i]["SQL"]).ToString();
                    string ysmc = (ruleDataTable.Rows[i]["要素名称"]).ToString();
                    Tuple<string, string> tt = new Tuple<string, string>(sql, ysmc);
                    if (fc2sqlysmc.ContainsKey(fcName))
                    {                        
                        fc2sqlysmc[fcName].Add(tt);
                    }
                    else
                    {
                        List<Tuple<string,string>> ll = new List<Tuple<string,string>>();
                        ll.Add(tt);
                        fc2sqlysmc.Add(fcName,ll); 
                    }                   
                }
            }
            catch (Exception err)
            {                
                System.Diagnostics.Trace.WriteLine(err.Message);
                System.Diagnostics.Trace.WriteLine(err.Source);
                System.Diagnostics.Trace.WriteLine(err.StackTrace); 
                MessageBox.Show("读取规则配置-要素名称YSMC赋值失败\n" + err.Message);
                return null;
            }
            return fc2sqlysmc;
        }
    }
}
