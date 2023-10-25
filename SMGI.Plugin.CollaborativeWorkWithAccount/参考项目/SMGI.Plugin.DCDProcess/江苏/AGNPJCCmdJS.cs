using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 地名简称赋值：对简称字段为空的地名点进行赋值
    /// 原则：不注尾称‘街道’、‘办事处’、‘村’、‘社区’字。
    /// 注：1.若村名只有两个字，则‘村’字保留”进行赋值；2.若村名以“XX村街道”、 “XX村社区”结尾，则村字也保留。
    /// </summary>
    public class AGNPJCCmdJS : SMGI.Common.SMGICommand
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
            string lyrName = "AGNP";
            var feLayer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
                        ((l as IGeoFeatureLayer).Name.Trim().ToUpper() == lyrName)).FirstOrDefault() as IFeatureLayer;
            if (feLayer == null)
            {
                MessageBox.Show(string.Format("当前工作空间中没有找到图层【{0}】!", lyrName));
                return;
            }

            var agnpFC = feLayer.FeatureClass;

            string nameFN = m_Application.TemplateManager.getFieldAliasName("Name", agnpFC.AliasName);
            int nameIndex = agnpFC.FindField(nameFN);
            if (nameIndex == -1)
            {
                MessageBox.Show(string.Format("图层【{0}】中没有找到字段【{1}】!", feLayer.Name, nameFN));
                return;
            }

            string jcFN = m_Application.TemplateManager.getFieldAliasName("JC", agnpFC.AliasName);
            int jcIndex = agnpFC.FindField(jcFN);
            if (jcIndex == -1)
            {
                MessageBox.Show(string.Format("图层【{0}】中没有找到字段【{1}】!", feLayer.Name, jcFN));
                return;
            }

            m_Application.EngineEditor.StartOperation();
            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = string.Format("({0} is null or {0} = '')", jcFN);
                    if (agnpFC.HasCollabField())
                        qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;
                    IFeatureCursor feCursor = agnpFC.Search(qf, false);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        wo.SetText(string.Format("正在为图层【{0}】的要素【{1}】赋值......", lyrName, fe.OID));

                        string name = fe.get_Value(nameIndex).ToString();
                        string jc = name;
                        if (name.EndsWith("街道"))
                        {
                            jc = name.Substring(0, name.Length - 2);
                        }
                        else if(name.EndsWith("办事处"))
                        {
                            jc = name.Substring(0, name.Length - 3);
                        }
                        else if (name.EndsWith("社区"))
                        {
                            jc = name.Substring(0, name.Length - 2);
                        }
                        else if (name.EndsWith("村") && name.Length > 2)
                        {
                            jc = name.Substring(0, name.Length - 1);
                        }

                        fe.set_Value(jcIndex, jc);
                        fe.Store();
                    }
                    Marshal.ReleaseComObject(feCursor);

                }

                m_Application.EngineEditor.StopOperation("地名简称赋值");

                MessageBox.Show(string.Format("赋值完成")); 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                m_Application.EngineEditor.AbortOperation();

                MessageBox.Show(ex.Message);
            }
            
        }
    }
}
