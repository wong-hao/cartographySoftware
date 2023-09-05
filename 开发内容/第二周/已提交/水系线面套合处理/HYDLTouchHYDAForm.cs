using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.CollaborativeWorkWithAccount.工具.水系线面套合处理
{
    public partial class HYDLTouchHYDAForm : Form
    {

        public IMap currentMap; //当前MapControl控件中的Map对象

        public HYDLTouchHYDAForm()
        {
            InitializeComponent();
        }

        public IFeatureLayer selectedRoadLayer; // 要选择的水系线图层
        public IFeatureLayer selectedAreaLayer; // 要选择的水系面图层

        public List<string> roadNames = new List<string>();
        public List<string> areaNames = new List<string>();

        private void HYDLTouchHYDAForm_Load(object sender, EventArgs e)
        {
            InitUi();
        }

        // 检查是否已选择水系图层
        private bool CheckSelectedLayer()
        {
            if (roadLayerNameCombox.SelectedIndex == -1)
            {
                MessageBox.Show("请选择水系线图层！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (areaLayerNameCombox.SelectedIndex == -1)
            {
                MessageBox.Show("请选择水系面图层！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            return true;
        }

        // 初始化水系图层界面
        public void InitUi()
        {
            try
            {
                //将水系线图层列表清空
                roadLayerNameCombox.Items.Clear();

                foreach (string roadName in roadNames)
                {
                    roadLayerNameCombox.Items.Add(roadName);
                }

                //将comboBoxLayerName控件的默认选项设置为空
                roadLayerNameCombox.SelectedIndex = -1;

                //将水系面图层列表清空
                areaLayerNameCombox.Items.Clear();

                foreach (string areaName in areaNames)
                {
                    areaLayerNameCombox.Items.Add(areaName);
                }

                //将comboBoxLayerName控件的默认选项设置为空
                areaLayerNameCombox.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void RoadLayerNameCombox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 实时获取所选图层
            GetSelectedRoadLayer();
        }

        // 选择目标图层
        public void GetSelectedRoadLayer()
        {
            try
            {
                for (var i = 0; i < currentMap.LayerCount; i++)
                    if (currentMap.get_Layer(i) is GroupLayer)
                    {
                        var compositeLayer = currentMap.get_Layer(i) as ICompositeLayer;
                        for (var j = 0; j < compositeLayer.Count; j++)
                            //判断图层的名称是否与控件中选择的图层名称相同
                            if (compositeLayer.get_Layer(j).Name == roadLayerNameCombox.SelectedItem.ToString())
                            {
                                //如果相同则设置为整个窗体所使用的IFeatureLayer接口对象
                                selectedRoadLayer = compositeLayer.get_Layer(j) as IFeatureLayer;
                                break;
                            }
                    }
                    else
                    {
                        //判断图层的名称是否与控件中选择的图层名称相同
                        if (currentMap.get_Layer(i).Name == roadLayerNameCombox.SelectedItem.ToString())
                        {
                            //如果相同则设置为整个窗体所使用的IFeatureLayer接口对象
                            selectedRoadLayer = currentMap.get_Layer(i) as IFeatureLayer;
                            break;
                        }
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void AreaLayerNameCombox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 实时获取所选图层
            GetSelectedAreaLayer();
        }

        // 选择目标图层
        public void GetSelectedAreaLayer()
        {
            try
            {
                for (var i = 0; i < currentMap.LayerCount; i++)
                    if (currentMap.get_Layer(i) is GroupLayer)
                    {
                        var compositeLayer = currentMap.get_Layer(i) as ICompositeLayer;
                        for (var j = 0; j < compositeLayer.Count; j++)
                            //判断图层的名称是否与控件中选择的图层名称相同
                            if (compositeLayer.get_Layer(j).Name == areaLayerNameCombox.SelectedItem.ToString())
                            {
                                //如果相同则设置为整个窗体所使用的IFeatureLayer接口对象
                                selectedAreaLayer = compositeLayer.get_Layer(j) as IFeatureLayer;
                                break;
                            }
                    }
                    else
                    {
                        //判断图层的名称是否与控件中选择的图层名称相同
                        if (currentMap.get_Layer(i).Name == areaLayerNameCombox.SelectedItem.ToString())
                        {
                            //如果相同则设置为整个窗体所使用的IFeatureLayer接口对象
                            selectedAreaLayer = currentMap.get_Layer(i) as IFeatureLayer;
                            break;
                        }
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            // 检查是否有水系图层被选中
            if (!CheckSelectedLayer()) return;

            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            selectedRoadLayer = null;
            selectedAreaLayer = null;
            Close();
        }

    }
}
