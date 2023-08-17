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
    /// 境界线构面
    /// </summary>
    public class BOULConstructBOUACmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            BOULConstructBOUAFrm frm = new BOULConstructBOUAFrm(m_Application);
            if (DialogResult.OK == frm.ShowDialog())
            {
                string boulFCName = frm.BOULFCName;
                string bouaFCName = frm.BOUAFCName; //若为空，则输出到临时文件中
                string filter = frm.FilterString;
                IFeatureClass rangeFC = frm.RangeFC;
                bool bPropJoin = frm.NeedPropJoin;
                string propFCName = frm.PropJoinFCName;

                IFeatureClass lFC = (m_Application.Workspace.LayerManager.GetLayer(
                    l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == boulFCName.ToUpper())).FirstOrDefault() as IFeatureLayer).FeatureClass;
                IFeatureClass pFC =null;
                if (bouaFCName != "")
                {
                    pFC = (m_Application.Workspace.LayerManager.GetLayer(
                        l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == bouaFCName.ToUpper())).FirstOrDefault() as IFeatureLayer).FeatureClass;
                }
                IFeatureClass propFC = null;
                if (bPropJoin)
                {
                    propFC = (m_Application.Workspace.LayerManager.GetLayer(
                         l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == propFCName.ToUpper())).FirstOrDefault() as IFeatureLayer).FeatureClass;
                }
                if (bouaFCName != "")
                {
                    DialogResult dialogResult = MessageBox.Show("境界线构面，将会先清空输出面要素类中的要素，是否继续？", "提示", MessageBoxButtons.YesNo);
                    if (dialogResult != DialogResult.Yes)
                        return;
                }
                string err = "";
                IFeatureClass xfc = null;
                Tuple<string, IFeatureClass> errTuple = new Tuple<string, IFeatureClass>("", null);
                BOULConstructBOUA con = new BOULConstructBOUA();
                using (var wo = m_Application.SetBusy())
                {
                    errTuple = con.ConstructBOUA(lFC, filter, rangeFC, pFC, 0.001, bPropJoin, propFC, wo);
                    err = errTuple.Item1;
                    xfc = errTuple.Item2;
                }

                if (err != "")
                {
                    MessageBox.Show(err);
                }
                else
                {
                    if (con.ErrOutPutFile != "")
                    {
                        if (MessageBox.Show("在境界构面属性赋值时发生异常，是否需要加载异常信息到当前地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            CheckHelper.AddTempLayerFromSHPFile(m_Application.Workspace.LayerManager.Map, con.ErrOutPutFile);
                            if (con.ErrPropPointFile != "")
                            {
                                CheckHelper.AddTempLayerFromSHPFile(m_Application.Workspace.LayerManager.Map, con.ErrPropPointFile);
                            }
                        }
                    }
                    else
                    {
                        if (bouaFCName == "" && xfc != null)
                        {
                            MessageBox.Show("完成境界构面,添加临时数据");
                            CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map,xfc);                            
                        }
                        else
                            MessageBox.Show("完成境界构面！");
                    }
                    
                }
            }

            

        }
    }
}
