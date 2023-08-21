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

        public IFeatureLayer _selecteddlFeatureLayer; // 要选择的水系线图层
        public IFeatureLayer _selecteddaFeatureLayer; // 要选择的水系面图层


        private void HYDLTouchHYDAForm_Load(object sender, EventArgs e)
        {
            InitDlLayersUi();

            InitDaLayersUi();
        }

        // 检查是否已选择水系线图层
        private bool CheckSelectedDLLayer()
        {
            if (hydlLayerNameCombox.SelectedIndex == -1)
            {
                MessageBox.Show("请选择水系线图层！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            return true;
        }

        // 检查是否已选择水系面图层
        private bool CheckSelectedDALayer()
        {
            if (hydaLayerNameCombox.SelectedIndex == -1)
            {
                MessageBox.Show("请选择水系面图层！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            return true;
        }

        // 初始化水系线图层界面
        public void InitDlLayersUi()
        {
            try
            {
                //将当前图层列表清空
                hydlLayerNameCombox.Items.Clear();

                string layerName; //设置临时变量存储图层名称

                // 对Map中的每个图层进行判断并加载名称
                for (var i = 0; i < currentMap.LayerCount; i++)
                {
                    ILayer layer = currentMap.get_Layer(i);

                    // 判断是否为 IFeatureLayer 类型
                    if (layer is IFeatureLayer)
                    {
                        IFeatureLayer featureLayer = (IFeatureLayer)layer;

                        // 判断是否为折线类型的几何图形
                        if (featureLayer.FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline)
                        {
                            layerName = layer.Name;
                            hydlLayerNameCombox.Items.Add(layerName);
                        }
                    }
                }

                //将comboBoxLayerName控件的默认选项设置为空
                hydlLayerNameCombox.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        // 初始化水系面图层界面
        public void InitDaLayersUi()
        {
            try
            {
                //将当前图层列表清空
                hydaLayerNameCombox.Items.Clear();

                string layerName; //设置临时变量存储图层名称

                // 对Map中的每个图层进行判断并加载名称
                for (var i = 0; i < currentMap.LayerCount; i++)
                {
                    ILayer layer = currentMap.get_Layer(i);
                    IFeatureLayer featureLayer = layer as IFeatureLayer;

                    if (featureLayer != null)
                    {
                        IFeatureClass featureClass = featureLayer.FeatureClass;

                        // 判断是否为面类型的几何图形
                        if (featureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon)
                        {
                            layerName = layer.Name;
                            hydaLayerNameCombox.Items.Add(layerName);
                        }
                    }
                }

                //将comboBoxLayerName控件的默认选项设置为空
                hydaLayerNameCombox.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void hydlLayerNameCombox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 实时获取所选图层
            GetSelecteddlFeatureLayer();
        }

        // 选择目标图层
        public void GetSelecteddlFeatureLayer()
        {
            try
            {
                for (var i = 0; i < currentMap.LayerCount; i++)
                    if (currentMap.get_Layer(i) is GroupLayer)
                    {
                        var compositeLayer = currentMap.get_Layer(i) as ICompositeLayer;
                        for (var j = 0; j < compositeLayer.Count; j++)
                            //判断图层的名称是否与控件中选择的图层名称相同
                            if (compositeLayer.get_Layer(j).Name == hydlLayerNameCombox.SelectedItem.ToString())
                            {
                                //如果相同则设置为整个窗体所使用的IFeatureLayer接口对象
                                _selecteddlFeatureLayer = compositeLayer.get_Layer(j) as IFeatureLayer;
                                break;
                            }
                    }
                    else
                    {
                        //判断图层的名称是否与控件中选择的图层名称相同
                        if (currentMap.get_Layer(i).Name == hydlLayerNameCombox.SelectedItem.ToString())
                        {
                            //如果相同则设置为整个窗体所使用的IFeatureLayer接口对象
                            _selecteddlFeatureLayer = currentMap.get_Layer(i) as IFeatureLayer;
                            break;
                        }
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        // 选择目标图层
        public void GetSelecteddaFeatureLayer()
        {
            try
            {
                for (var i = 0; i < currentMap.LayerCount; i++)
                    if (currentMap.get_Layer(i) is GroupLayer)
                    {
                        var compositeLayer = currentMap.get_Layer(i) as ICompositeLayer;
                        for (var j = 0; j < compositeLayer.Count; j++)
                            //判断图层的名称是否与控件中选择的图层名称相同
                            if (compositeLayer.get_Layer(j).Name == hydaLayerNameCombox.SelectedItem.ToString())
                            {
                                //如果相同则设置为整个窗体所使用的IFeatureLayer接口对象
                                _selecteddaFeatureLayer = compositeLayer.get_Layer(j) as IFeatureLayer;
                                break;
                            }
                    }
                    else
                    {
                        //判断图层的名称是否与控件中选择的图层名称相同
                        if (currentMap.get_Layer(i).Name == hydaLayerNameCombox.SelectedItem.ToString())
                        {
                            //如果相同则设置为整个窗体所使用的IFeatureLayer接口对象
                            _selecteddaFeatureLayer = currentMap.get_Layer(i) as IFeatureLayer;
                            break;
                        }
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        // 清空选择的水系线图层
        public void ClearSelectedDlFeatureLayer()
        {
            try
            {
                hydlLayerNameCombox.Items.Clear();
                hydlLayerNameCombox.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        // 清空选择的水系面图层
        public void ClearSelectedDaFeatureLayer()
        {
            try
            {
                hydaLayerNameCombox.Items.Clear();
                hydaLayerNameCombox.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            // 检查是否有水系线图层被选中
            if (!CheckSelectedDLLayer()) return;

            // 检查是否有水系面图层被选中
            if (!CheckSelectedDALayer()) return;

            // 清空已选水系线图层
            ClearSelectedDlFeatureLayer();

            // 清空已选水系面图层
            ClearSelectedDaFeatureLayer();

            MessageBox.Show("选择的水系线图层为" + _selecteddlFeatureLayer.Name);
            MessageBox.Show("选择的水系面图层为" + _selecteddaFeatureLayer.Name);

            Close();
        }

        private void hydaLayerNameCombox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 实时获取所选图层
            GetSelecteddaFeatureLayer();
        }

    }
}
