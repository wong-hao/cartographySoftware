using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using System.Data;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
        /// <summary>
        /// 点拓扑检查
        /// </summary>
        public class TopoPointRelationcmd : SMGI.Common.SMGICommand
        {
            private TopoPointRelationCheck _check;
            //List<string> checkedpointlayers = new List<string>();
            //List<string> checkedobjectlayers = new List<string>();
            IFeatureWorkspace featureWorkspace;
            public TopoPointRelationcmd()
            {
                m_caption = "点拓扑";
                m_toolTip = "点拓扑关系检查（点在线上、点不在线上、点在面上、点不在面上）";
                m_category = "数据";
            }

            public override bool Enabled
            {
                get
                {
                    return m_Application != null && m_Application.Workspace != null;
                }
            }
            public override void OnClick()
            {

               
                if (m_Application.MapControl.Map.ReferenceScale == 0)
                {
                    MessageBox.Show("请先设置参考比例尺！");
                    return;
                }
                IWorkspace workspace = m_Application.Workspace.EsriWorkspace;
                featureWorkspace = (IFeatureWorkspace)workspace;

                Progress progWindow = new Progress();
                progWindow.Show();
               
                progWindow.lbInfo.Text = "点拓扑检查......";
                System.Windows.Forms.Application.DoEvents();
                                                  
                _check = new TopoPointRelationCheck(featureWorkspace, progWindow);

                var rm = _check.Check();

                progWindow.Close();
                   
                if (rm.stat != ResultState.Ok)
                {
                    MessageBox.Show(rm.msg);
                    return;
                }
                rm = _check.SaveResult(OutputSetup.GetDir());
                if (rm.stat != ResultState.Ok)
                {
                    MessageBox.Show(rm.msg);
                    return;
                }
                if (MessageBox.Show("检查完成！是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var strs = (string[])rm.info;
                    CheckHelper.AddTempLayerFromSHPFile(m_Application.Workspace.LayerManager.Map, strs[1]);

                    System.Diagnostics.Process.Start(strs[0]);
                }

                
                

            }
            public List<string> gb(string str)
            {
                List<string> tempgb = new List<string>();
                IFeatureClass spFC = featureWorkspace.OpenFeatureClass(str);
                int gb = spFC.Fields.FindField("GB");
                if (gb == -1) { MessageBox.Show("GB字段不存在"); return tempgb; }
                IFeatureCursor featureCursor = spFC.Search(null, false);
                IFeature CheckFeature = null;
                while ((CheckFeature = featureCursor.NextFeature()) != null)
                {
                    string gbstr = CheckFeature.get_Value(gb).ToString().Trim();
                    if (tempgb.Contains(gbstr)) { continue; } else { tempgb.Add(gbstr); }
                }

                Marshal.ReleaseComObject(featureCursor);
                return tempgb;
            }
    }
}
