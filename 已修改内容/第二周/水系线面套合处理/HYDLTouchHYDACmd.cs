using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using System.IO;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using SMGI.Plugin.CollaborativeWorkWithAccount.工具.水系线面套合处理;
using SMGI.Plugin.DCDProcess;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 水系面综合化简后，河流线及水系结构线应仍保持与水系面的touch关系
    /// </summary>
    public class HYDLTouchHYDACmd : SMGI.Common.SMGITool
    {
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        /// <summary>
        ///     显示窗体
        /// </summary>
        private void ShowSelectionForm()
        {
            try
            {
                if (selectionForm == null || selectionForm.IsDisposed)
                {
                    selectionForm = new HYDLTouchHYDAForm();
                    selectionForm.currentMap = currentMap;
                    selectionForm.Show();
                }
                else
                {
                    selectionForm.Activate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        /// <Date>2023/8/4</Date>
        /// <Author>HaoWong</Author>
        /// <summary>
        ///     子函数，得到目前图层
        /// </summary>
        private void GetCurrentMap()
        {
            _currentMapControl = m_Application.MapControl;

            // 确保当前地图控件和地图对象不为空
            if (_currentMapControl != null) currentMap = _currentMapControl.Map;
        }

        private HYDLTouchHYDAForm selectionForm; // 窗体
        private AxMapControl _currentMapControl; // 当前的MapControl控件
        private IMap currentMap; // 当前MapControl控件中的Map对象   

        public override void OnClick()
        {
            // 获取当前地图
            GetCurrentMap();

            // 检查地图是否为空
            if (currentMap == null)
            {
                MessageBox.Show("地图未加载，请先加载地图。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //显示窗体
            ShowSelectionForm();

        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button == 1)
            {
                string hydlLayerName = selectionForm._selecteddlFeatureLayer.Name;
                string hydaLayerName = selectionForm._selecteddaFeatureLayer.Name;
                IFeatureClass hydlFC = (m_Application.Workspace.LayerManager.GetLayer(
                        l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == hydlLayerName)).FirstOrDefault() as IFeatureLayer).FeatureClass;
                if (hydlFC == null)
                {
                    MessageBox.Show(string.Format("没找到水系线层【{0}】", hydlLayerName));
                    return;
                }
                IFeatureClass hydaFC = (m_Application.Workspace.LayerManager.GetLayer(
                        l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == hydaLayerName)).FirstOrDefault() as IFeatureLayer).FeatureClass;
                if (hydaFC == null)
                {
                    MessageBox.Show(string.Format("没找到水系面层【{0}】", hydaLayerName));
                    return;
                }

                m_Application.EngineEditor.StartOperation();

                string outputFilePath = OutputSetup.GetDir() + "\\ProcessInfo";
                if (!Directory.Exists(outputFilePath))
                    Directory.CreateDirectory(outputFilePath);
                string outputFileName = outputFilePath + string.Format("\\水系线处理情况.shp");


                string err = "";
                using (var wo = m_Application.SetBusy())
                {
                    HYDLTouchHYDA obj = new HYDLTouchHYDA();
                    err = obj.Process(hydaFC, hydlFC, outputFileName, wo);
                }


                if (err != "")
                {
                    m_Application.EngineEditor.AbortOperation();

                    MessageBox.Show(err);
                }
                else
                {
                    if (MessageBox.Show("是否加载处理情况数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        CheckHelper.AddTempLayerFromSHPFile(m_Application.Workspace.LayerManager.Map, outputFileName);
                    }
                }

                m_Application.EngineEditor.StopOperation("水系线拓扑关系纠正");

            
            }
        }
    }
}
