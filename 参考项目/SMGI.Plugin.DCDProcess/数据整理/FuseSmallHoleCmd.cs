using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 微小孔洞融合
    /// </summary>
    public class FuseSmallHoleCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            List<ESRI.ArcGIS.Geometry.esriGeometryType> lyrTypeList = new List<ESRI.ArcGIS.Geometry.esriGeometryType>();
            lyrTypeList.Add(ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon);
            FuseSmallHoleFrm frm = new FuseSmallHoleFrm(m_Application, lyrTypeList);
            if (DialogResult.OK == frm.ShowDialog())
            {
                string targerLayerName = frm.TargetLayerName;
                List<string> referLyrNameList = frm.ReferLayerNames;
                double minHoleArea = frm.MinHoleArea;

                IFeatureClass targetFC = (m_Application.Workspace.LayerManager.GetLayer(
                    l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name == targerLayerName)).FirstOrDefault() as IFeatureLayer).FeatureClass;

                List<IFeatureClass> referFCList = new List<IFeatureClass>();
                foreach (var referLyrName in referLyrNameList)
                {
                    IFeatureClass fc = (m_Application.Workspace.LayerManager.GetLayer(
                        l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name== referLyrName)).FirstOrDefault() as IFeatureLayer).FeatureClass;

                    referFCList.Add(fc);
                }

                m_Application.EngineEditor.StartOperation();

                string err = "";
                using (var wo = m_Application.SetBusy())
                {
                    QueryFilterClass qf = new QueryFilterClass();
                    if (targetFC.HasCollabField())
                    {
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                    }

                    FuseSmallHole fh = new FuseSmallHole();
                    err = fh.FuseHole(targetFC, referFCList, qf, minHoleArea, wo);
                }

                if (err != "")
                {
                    m_Application.EngineEditor.AbortOperation();

                    MessageBox.Show(err);
                }
                else
                {
                    m_Application.EngineEditor.StopOperation("微小孔洞处理");

                    MessageBox.Show("处理完毕！");
                }

                
            }

            

        }
    }
}
