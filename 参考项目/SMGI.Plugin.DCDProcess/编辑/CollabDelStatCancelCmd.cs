using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using SMGI.Plugin.DCDProcess;
namespace SMGI.Plugin.GeneralEdit
{
    /// <summary>
    /// 撤销协同删除
    /// </summary>
    public class CollabDelStatCancelCmd : SMGICommand
    {        
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null &&
                    m_Application.ActiveView.FocusMap.SelectionCount > 0 &&
                    m_Application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            var selection = m_Application.ActiveView.FocusMap.FeatureSelection;
            IEnumFeature selectEnumFeature = (selection as MapSelection) as IEnumFeature;
            selectEnumFeature.Reset();
            IFeature fe = null;
            
            bool change = false;

            m_Application.EngineEditor.StartOperation();
            while ((fe = selectEnumFeature.Next()) != null)
            {
                int idxDELSTATE = fe.Fields.FindField(cmdUpdateRecord.CollabDELSTATE);
                int idxVERS = fe.Fields.FindField(cmdUpdateRecord.CollabVERSION);
                if (idxDELSTATE == -1 || idxVERS == -1)
                    continue;
                else
                {
                    string delstate = fe.get_Value(idxDELSTATE).ToString();
                    int vers = int.Parse(fe.get_Value(idxVERS).ToString());
                    if (vers == -2 && delstate == "是")
                    {                        
                        fe.set_Value(idxDELSTATE, null);//如何实现NULL
                        fe.Store();
                        change = true;
                    }
                }                
            }

            if (change)
            {
                m_Application.EngineEditor.StopOperation("撤销协同删除");
                m_Application.MapControl.Map.ClearSelection();
                m_Application.ActiveView.Refresh();
            }
            else
            {
                m_Application.EngineEditor.AbortOperation();
            }
 
        }
    }
}
