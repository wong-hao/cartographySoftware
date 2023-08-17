using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
/// <summary>
/// 批处理空格
/// </summary>
namespace SMGI.Plugin.DCDProcess.DataProcess
{
    public class FieldBackspaceBatCmd : SMGI.Common.SMGICommand
    {
        //private Dictionary<string, IFeatureClass> _fcName2FC;
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            IEngineEditLayers editLayer = m_Application.EngineEditor as IEngineEditLayers;

            //获取所有可编辑图层列表
            var pLayers = m_Application.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
                (l is IGeoFeatureLayer))).ToArray();

            //获得复选框的图层list
            var frm = new CheckLayerSelectForm(m_Application, true, true, true, false, false);
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Text = "字段空格批处理";

            if (frm.ShowDialog() != DialogResult.OK)
                return;

            List<IFeatureClass> fcList = new List<IFeatureClass>();
            foreach (var layer in frm.CheckFeatureLayerList)
            {
                IFeatureClass fc = layer.FeatureClass;
                if (!fcList.Contains(fc))
                    fcList.Add(fc);
            }

            try
            {
                m_Application.EngineEditor.StartOperation();
                                
                using (var wo = m_Application.SetBusy())
                {
                    #region 逐层处理空格
                    for (int i = 0; i < pLayers.Length; i++)
                    {
                        IFeatureLayer layer = pLayers[i] as IFeatureLayer;
                        if ((layer.FeatureClass as IDataset).Workspace.PathName != m_Application.Workspace.EsriWorkspace.PathName)//临时数据不参与
                            continue;

                        if (!editLayer.IsEditable(layer))
                            continue;

                        //未选择的图层跳过
                        if (!fcList.Contains(layer.FeatureClass))
                            continue;

                        string layerName = layer.Name.ToUpper();
                        wo.SetText("正在处理要素【" + layerName + "】.......");

                        IFeatureClass fc = layer.FeatureClass;

                        //获取文本字段Fields----idx_fieldDic
                        var fds = fc.Fields;
                        var idx_fieldDic = new Dictionary<int, string>();
                        for (var j = 0; j < fds.FieldCount; j++)
                        {
                            var fd = fds.Field[j];
                            if (fd.Type == esriFieldType.esriFieldTypeString && fd.Name.ToUpper() != "PINYIN")
                            {
                                idx_fieldDic.Add(j, fd.Name);
                            }
                        }

                        //设置查询语句
                        var qfilter = new QueryFilterClass();
                        var str = "";
                        int k = 0;
                        foreach (var kvp in idx_fieldDic)
                        {
                            //var idx = kvp.Key;
                            var fldName = kvp.Value;
                            if (k > 0) str += " or ";
                            str += fldName + " LIKE '%　%' or " + fldName + " like '% %' ";
                            k += 1;
                        }

                        qfilter.WhereClause = str;
                        var pCursor = fc.Search(qfilter, false);
                        IFeature pFea = null;
                        while (null != (pFea = pCursor.NextFeature()))
                        {
                            bool changed = false;
                            foreach (var idx in idx_fieldDic.Keys)
                            {
                                string newStr = pFea.Value[idx].ToString().Replace(" ", "").Replace("　", "");
                                if (pFea.Value[idx].ToString() != newStr)
                                {
                                    pFea.Value[idx] = newStr;
                                    changed = true;
                                }
                            }
                            if (changed)
                            {
                                pFea.Store();
                            }
                        }
                    }
                    #endregion
                }
                m_Application.EngineEditor.StopOperation("字段空格批处理");
                MessageBox.Show("字段空格批处理完毕");
            }
            catch (Exception err)
            {
                m_Application.EngineEditor.AbortOperation();

                System.Diagnostics.Trace.WriteLine(err.Message);
                System.Diagnostics.Trace.WriteLine(err.Source);
                System.Diagnostics.Trace.WriteLine(err.StackTrace);

                MessageBox.Show(String.Format("字段空格批处理错误:{0}",err.Message));
            }           
        }
    }
}
