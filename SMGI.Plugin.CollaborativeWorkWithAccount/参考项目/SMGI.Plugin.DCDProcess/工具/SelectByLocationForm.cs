using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.CartographyTools;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 使用要素在另一图层中的位置来选择要素
    /// </summary>
    public partial class SelectByLocationForm : Form
    {
        public SelectByLocationForm()
        {
            InitializeComponent();
        }

        private void SelectByLocationForm_Load(object sender, EventArgs e)
        {
            // 获取当前地图中的所有要素图层
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer);
            })).ToArray();

            //初始化控件
            clbObjFCList.ValueMember = "Key";
            clbObjFCList.DisplayMember = "Value";
            cmbSourceFC.ValueMember = "Key";
            cmbSourceFC.DisplayMember = "Value";
            foreach(var item in lyrs)
            {
                if (cbOnlyShowEnableSelectFC.Checked)
                {
                    if ((item as IFeatureLayer).Selectable)
                        clbObjFCList.Items.Add(new KeyValuePair<ILayer, string>(item,item.Name));
                }
                else
                {
                    clbObjFCList.Items.Add(new KeyValuePair<ILayer, string>(item, item.Name));
                }
                cmbSourceFC.Items.Add(new KeyValuePair<ILayer, string>(item, item.Name));
            }

            cmbSourceFC.SelectedIndex = cmbSourceFC.Items.Count > 0 ? 0 : -1;

            cmbSelectMethods.SelectedIndex = cmbSelectMethods.Items.Count > 0 ? 0 : -1;
            cmbobjFeatureSpatialSelectMethod.SelectedIndex = cmbobjFeatureSpatialSelectMethod.Items.Count > 0 ? 0 : -1; 
        }


        private void clbObjFCList_SelectedValueChanged(object sender, EventArgs e)
        {
            if (clbObjFCList.SelectedItem == null)
                return;

            var obj = (KeyValuePair<ILayer, string>)clbObjFCList.SelectedItem;

            bool bContained = false;//源图层中是否包含该图层
            for (int i = 0; i < cmbSourceFC.Items.Count; ++i)
            {
                var item = (KeyValuePair<ILayer, string>)cmbSourceFC.Items[i];
                if (item.Key == obj.Key)
                {
                    bContained = true;
                    break;
                }
            }
            
            if (clbObjFCList.GetItemChecked(clbObjFCList.SelectedIndex))
            {
                //若当前选择图层没有选择的要素，则在源图层列表中舍去
                if (bContained)
                {
                    IFeatureLayer feLayer = obj.Key as IFeatureLayer;
                    ISelectionSet selectionSet = (feLayer as IFeatureSelection).SelectionSet;
                    if (selectionSet.Count == 0)
                    {
                        cmbSourceFC.Items.Remove(obj);
                        if(cmbSourceFC.SelectedIndex == -1)
                            cmbSourceFC.SelectedIndex = cmbSourceFC.Items.Count > 0 ? 0 : -1;
                    }
                }

                //更新复选框状态
                var sourceLayer = ((KeyValuePair<ILayer, string>)cmbSourceFC.SelectedItem).Key as IFeatureLayer;
                if (obj.Key == sourceLayer)
                {
                    cbUseSlectedFeature.Checked = true;
                    cbUseSlectedFeature.Enabled = false;
                }
            }
            else
            {
                if (!bContained)
                {
                    cmbSourceFC.Items.Add(new KeyValuePair<ILayer, string>(obj.Key, obj.Value));
                }

                var sourceLayer = ((KeyValuePair<ILayer, string>)cmbSourceFC.SelectedItem).Key as IFeatureLayer;
                if (obj.Key == sourceLayer)
                {
                    cbUseSlectedFeature.Enabled = true;
                }
            }

            
        }

        private void cbOnlyShowEnableSelectFC_CheckedChanged(object sender, EventArgs e)
        {
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer);
            })).ToArray();

            clbObjFCList.Items.Clear();
            foreach (var item in lyrs)
            {
                if (cbOnlyShowEnableSelectFC.Checked)
                {
                    if ((item as IFeatureLayer).Selectable)
                        clbObjFCList.Items.Add(new KeyValuePair<ILayer, string>(item, item.Name));
                }
                else
                {
                    clbObjFCList.Items.Add(new KeyValuePair<ILayer, string>(item, item.Name));
                }
            }
        }

        private void cmbSourceFC_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSourceFC.SelectedItem == null)
                return;

            var obj = (KeyValuePair<ILayer, string>)cmbSourceFC.SelectedItem;

            cbUseSlectedFeature.Checked = false;
            cbUseSlectedFeature.Enabled = true;

            for (int i = 0; i < clbObjFCList.Items.Count; i++)
            {
                if (obj.Key != ((KeyValuePair<ILayer, string>)clbObjFCList.Items[i]).Key)
                    continue;

                if (clbObjFCList.GetItemChecked(i))//源图层也是目标图层
                {
                    cbUseSlectedFeature.Checked = true;
                    cbUseSlectedFeature.Enabled = false;
                }
            }


            IFeatureLayer feLayer = obj.Key as IFeatureLayer;
            ISelectionSet selectionSet = (feLayer as IFeatureSelection).SelectionSet;
            cbUseSlectedFeature.Enabled = selectionSet.Count > 0;

            lbselFeatureCount.Text = "(选择了 " + selectionSet.Count.ToString() + " 个要素)";
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (clbObjFCList.CheckedItems.Count == 0)
            {
                MessageBox.Show("请至少选择一个目标图层！");
                return;
            }

            if (cmbSourceFC.Text == "")
            {
                MessageBox.Show("请选择一个源图层！");
                return;
            }

            esriSelectionResultEnum resutEnum = esriSelectionResultEnum.esriSelectionResultNew; 
            switch (cmbSelectMethods.Text)
            {
                case "从以下图层中选择要素":
                    resutEnum = esriSelectionResultEnum.esriSelectionResultNew; 
                    break;
                case "添加到当前在以下图层中选择的要素":
                    resutEnum = esriSelectionResultEnum.esriSelectionResultAdd;
                    break;
                case "移除当前在以下图层中选择的要素":
                    resutEnum = esriSelectionResultEnum.esriSelectionResultSubtract;
                    break;
                case "从当前在以下图层中选择的要素中选择":
                    resutEnum = esriSelectionResultEnum.esriSelectionResultAnd;
                    break;
                default:
                    MessageBox.Show("不支持该选择方法！");
                    return;
            }
            
            esriLayerSelectionMethod spatialSelMethod = esriLayerSelectionMethod.esriLayerSelectIntersect;
            switch (cmbobjFeatureSpatialSelectMethod.Text)
            {
                case "与源图层要素相交":
                    spatialSelMethod = esriLayerSelectionMethod.esriLayerSelectIntersect;
                    break;
                case "完全位于源图层要素范围内":
                    spatialSelMethod = esriLayerSelectionMethod.esriLayerSelectCompletelyWithin;
                    break;
                case "完全包含源图层要素":
                    spatialSelMethod = esriLayerSelectionMethod.esriLayerSelectCompletelyContains;
                    break;
                case "与源图层要素共线":
                    spatialSelMethod = esriLayerSelectionMethod.esriLayerSelectShareALineSegmentWith;
                    break;
                case "与源图层要素完全相同":
                    spatialSelMethod = esriLayerSelectionMethod.esriLayerSelectAreIdenticalTo;
                    break;
                default:
                    MessageBox.Show("不支持该选择方法！");
                    return;
            }

            try
            {
                btOK.Enabled = false;
                #region 选择要素
                using (var wo = GApplication.Application.SetBusy())
                {
                    QueryByLayerClass queryLayerSelect = new QueryByLayerClass();
                    queryLayerSelect.ResultType = resutEnum;
                    queryLayerSelect.ByLayer = ((KeyValuePair<ILayer, string>)cmbSourceFC.SelectedItem).Key as IFeatureLayer;
                    queryLayerSelect.LayerSelectionMethod = spatialSelMethod;
                    queryLayerSelect.UseSelectedFeatures = cbUseSlectedFeature.Checked;
                    foreach (var item in clbObjFCList.CheckedItems)
                    {
                        var layer = ((KeyValuePair<ILayer, string>)item).Key as IFeatureLayer;
                        queryLayerSelect.FromLayer = layer;
                        ISelectionSet selectionSet = queryLayerSelect.Select();

                        IFeatureSelection featureSelection = layer as IFeatureSelection;
                        featureSelection.SelectionSet = selectionSet;

                        Helper.RefreshAttributeWindow(layer);

                        GApplication.Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, layer, GApplication.Application.ActiveView.Extent);
                    }
                }

                DialogResult = System.Windows.Forms.DialogResult.OK;
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btOK.Enabled = true;
            }

            
        }
        
    }
}
