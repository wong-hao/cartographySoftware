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
    /// 安徽——地名简称赋值：对简称字段为空的地名点进行赋值
    /// 1)	CLASS=AH，“xx街办”或者“xx街道办事处”统一修改简称为“xx街道”
    /// 2)	CLASS=AK，BB，不注尾称“村”，两个字的“x村”这种保留全称；
    /// 3)	CLASS=AK1，不注尾称“社区”；
    /// 创建于2022.10.3
    /// </summary>
    public class AGNPJCAHCmd : SMGI.Common.SMGICommand
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
            if (agnpFC == null)
            {
                MessageBox.Show(string.Format("图层【{0}】指向的要素类为空!", lyrName));
                return;
            }

            string nameFN = m_Application.TemplateManager.getFieldAliasName("Name", agnpFC.AliasName);//NAME字段
            int nameIndex = agnpFC.FindField(nameFN);
            if (nameIndex == -1)
            {
                MessageBox.Show(string.Format("图层【{0}】中没有找到字段【{1}】!", feLayer.Name, nameFN));
                return;
            }

            string jcFN = m_Application.TemplateManager.getFieldAliasName("JIANCH", agnpFC.AliasName); //简称字段
            int jcIndex = agnpFC.FindField(jcFN);
            if (jcIndex == -1)
            {
                MessageBox.Show(string.Format("图层【{0}】中没有找到字段【{1}】!", feLayer.Name, jcFN));
                return;
            }

            string clsFN = m_Application.TemplateManager.getFieldAliasName("CLASS", agnpFC.AliasName); //CLASS字段
            int clsIndex = agnpFC.FindField(clsFN);
            if (clsIndex == -1)
            {
                MessageBox.Show(string.Format("图层【{0}】中没有找到字段【{1}】!", feLayer.Name, clsFN));
                return;
            }  
                
            try
            {
                int modifiedNum = 0;

                m_Application.EngineEditor.StartOperation();
                using (var wo = m_Application.SetBusy())
                {
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = string.Format("({0} is null or {0} = '')", jcFN);//已赋值的，不再修改
                    if (agnpFC.HasCollabField())
                        qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;

                    IFeatureCursor feCursor = agnpFC.Search(qf, false);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        wo.SetText(string.Format("正在为图层【{0}】的要素【{1}】赋值......", lyrName, fe.OID));

                        string cls = fe.get_Value(clsIndex).ToString();
                        string name = fe.get_Value(nameIndex).ToString();

                        string jc = name;

                        if (cls == "AH")
                        {
                            if (name.EndsWith("街道办事处"))
                            {
                                jc = name.Substring(0, name.Length - 3);
                            }
                            else if (name.EndsWith("街办"))
                            {
                                jc = name.Substring(0, name.Length - 2) + "街道";
                            }
                        }
                        else if ((cls == "AK" || cls == "BB") && name.Length > 2 && name.EndsWith("村"))
                        {
                            jc = name.Substring(0, name.Length - 1);
                        }
                        

                        fe.set_Value(jcIndex, jc);
                        fe.Store();

                        modifiedNum++;

                        Marshal.ReleaseComObject(fe);
                        //内存监控
                        if (Environment.WorkingSet > DCDHelper.MaxMem)
                        {
                            GC.Collect();
                        }
                    }
                    Marshal.ReleaseComObject(feCursor);
                }
                
                m_Application.EngineEditor.StopOperation("地名简称赋值");
                MessageBox.Show(string.Format("赋值完成,共完成{0}个要素的简称赋值", modifiedNum)); 
            }
            catch (Exception ex)
            {
                m_Application.EngineEditor.AbortOperation();

                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);                

                MessageBox.Show(string.Format("赋值失败:{0}",ex.Message));
            }
            
        }
    }
}
