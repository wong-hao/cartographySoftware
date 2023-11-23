using System;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessor;
using SMGI.Common;
using SMGI.Plugin.DCDProcess;


namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 悬挂点检查（端点离线，点不在线上打断的情况：点所在线可能相离，也可能相交）
    /// </summary>
    public class LineHeadClosedToLineCrossLayersChkCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;
            }
        }

        private IFeatureClass GetFeatureClassByName(string layerName)
        {
            // 从工作空间中获取要素类
            try
            {
                // 根据图层名称打开对应的要素类
                IFeatureWorkspace fws = (m_Application.Workspace.EsriWorkspace as IWorkspace2) as IFeatureWorkspace;

                return fws.OpenFeatureClass(layerName);
            }
            catch (Exception ex)
            {
                // 处理异常（例如：图层不存在等）
                Console.WriteLine("无法获取图层：" + ex.Message);
                return null;
            }
        }

        private IFeatureClass fc;
        private IFeatureClass fc1;
        private IFeatureClass fc2;
        private IFeatureClass fc3;
        private IFeatureClass fc4;
        private IFeatureClass fc5;
        private IFeatureClass fc6;
        private IFeatureClass fc7;

        public override void OnClick()
        {
            //图层选择、最大距离对话框
            var layerSelectForm = new LineHeadClosedToLineCrossLayersChkLayerSelectForm(m_Application)
            {
                GeoTypeFilter = esriGeometryType.esriGeometryPolyline
            };
            if (layerSelectForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            //检查图层
            String layerName = String.Empty;
            layerName = layerSelectForm.layerName;

            string targetLayerGroupName = String.Empty;

            //检查距离
            //超过该距离的孤立点，不认为是悬挂点
            double bigDistance = double.Parse(layerSelectForm.tbValue.Text);

            string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
            IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);
            IFeatureWorkspace fws = ws as IFeatureWorkspace;

            Geoprocessor geoprocessor = new Geoprocessor();
            geoprocessor.OverwriteOutput = true;
            Merge merge = new Merge();

            string fcNameArray = String.Empty;

            //待检查要素类
            if (layerName == "道路")
            {
                fc1 = GetFeatureClassByName("高速公路");
                fc2 = GetFeatureClassByName("国省县道");
                fc3 = GetFeatureClassByName("乡道");
                fc4 = GetFeatureClassByName("其他道路");
                fc5 = GetFeatureClassByName("城市道路");

                if (fc1 == null)
                {
                    MessageBox.Show("要素类高速公路为空!");
                    return;
                }else if (fc2 == null)
                {
                    MessageBox.Show("要素类国省县道为空!");
                    return;
                }
                else if (fc3 == null)
                {
                    MessageBox.Show("要素类乡道为空!");
                    return;
                }
                else if (fc4 == null)
                {
                    MessageBox.Show("要素类其他道路为空!");
                    return;
                }
                else if (fc5 == null)
                {
                    MessageBox.Show("要素类城市道路为空!");
                    return;
                }

                fcNameArray = "高速公路;国省县道;乡道;其他道路;城市道路";

                targetLayerGroupName = "道路";
            }
            else if (layerName == "水系")
            {
                fc6 = GetFeatureClassByName("河流");
                fc7 = GetFeatureClassByName("水渠");

                if (fc6 == null)
                {
                    MessageBox.Show("要素类河流为空!");
                    return;
                }
                else if (fc7 == null)
                {
                    MessageBox.Show("要素类水渠为空!");
                    return;
                }

                fcNameArray = "河流;水渠";

                targetLayerGroupName = "水系";
            }
            else
            {
                MessageBox.Show("仅支持道路与水系，" + layerName + "并不受支持！");
                return;
            }

            merge.inputs = fcNameArray;
            merge.output = fullPath + "\\" + targetLayerGroupName;
            Helper.ExecuteGPTool(geoprocessor, merge, null);

            fc = fws.OpenFeatureClass(targetLayerGroupName);

            LineHeadClosedToLineChk check2 = new LineHeadClosedToLineChk();

            ResultMessage resultMessage;
            using (var wo = m_Application.SetBusy())
            {
                resultMessage = check2.Check(fc, (fc as IGeoDataset).Extent, bigDistance);
                if (resultMessage.stat == ResultState.Ok)
                {
                    string tdir = OutputSetup.GetDir();
                    if (tdir == "")
                    {
                        MessageBox.Show("请指定输出路径！");
                        return;
                    }
                    resultMessage = check2.SaveResult(tdir);
                    if (check2.Count == 0)
                    {
                        MessageBox.Show("检查完成,未发现悬挂点！");
                    }
                    else
                    {
                        if (MessageBox.Show("检查完成！是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            var strs = (string[])resultMessage.info;
                            CheckHelper.AddTempLayerFromSHPFile(m_Application.Workspace.LayerManager.Map, strs[0]);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(resultMessage.msg);
                }
            }
        }
    }
}
