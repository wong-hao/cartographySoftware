using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace WindowsFormsApplication1
{
    public partial class OnCreateFeatureForm : Form
    {
        private readonly List<string> FieldArray = new List<string>(); // 全局变量
        private readonly string layerName = "县以上居民地"; // 默认值
        private string CODE = "";
        private ComboBox comBoxCode;
        private ComboBox comBoxDj;
        private ComboBox comBoxGb;

        private ComboBox comBoxType;
        private string DJ = "";
        private Control dynamicControl;
        private string fieldOption = "";
        private string GB = "";
        private TextBox textBoxGb;

        private string TYPE = "";


        private XmlDocument xmlDoc;

        public OnCreateFeatureForm()
        {
            InitializeComponent();

            LoadConfiguration();

            InitializeFields();

            InitializeControls();
        }

        private void LoadConfiguration()
        {
            xmlDoc = new XmlDocument();
            xmlDoc.Load("config.xml"); // 文件路径根据实际情况修改
        }

        private void InitializeTypes()
        {
            var layerNodes = xmlDoc.SelectNodes("//Layer");
            foreach (XmlNode layerNode in layerNodes)
            {
                var layerNameNode = layerNode.SelectSingleNode("LayerName");
                var configLayerName = layerNameNode.InnerText;

                if (configLayerName == layerName)
                {
                    var typeNodes = layerNode.SelectNodes("Type");
                    foreach (XmlNode typeNode in typeNodes)
                    {
                        var name = typeNode.SelectSingleNode("Name").InnerText;
                        comBoxType.Items.Add(name);
                    }

                    return; // Exit loop after finding a matching layer
                }
            }

            comBoxType.Enabled = false; // Disable if no matching layer is found
        }

        private void InitializeGbs()
        {
            if (!string.IsNullOrEmpty(TYPE))
            {
                comBoxGb.Items.Clear();
                comBoxGb.Enabled = true; // 启用控件

                var layerNodes = xmlDoc.SelectNodes("//Layer");
                foreach (XmlNode layerNode in layerNodes)
                {
                    var layerNameNode = layerNode.SelectSingleNode("LayerName");
                    var configLayerName = layerNameNode.InnerText;

                    if (configLayerName == layerName)
                    {
                        var typeNodes = layerNode.SelectNodes("Type");
                        foreach (XmlNode typeNode in typeNodes)
                        {
                            var name = typeNode.SelectSingleNode("Name").InnerText;
                            if (name == TYPE)
                            {
                                var gbNodes = typeNode.SelectNodes("GB");
                                foreach (XmlNode gbNode in gbNodes)
                                {
                                    var gb = gbNode.InnerText;
                                    comBoxGb.Items.Add(gb);
                                }

                                return; // 找到匹配的 TYPE 后退出循环
                            }
                        }

                        break; // 找到匹配的图层后退出循环
                    }
                }
            }
            else
            {
                comBoxGb.Enabled = false; // 如果没有选择 TYPE，则禁用控件
            }
        }

        private void InitializeCodes()
        {
            if (!string.IsNullOrEmpty(TYPE))
            {
                comBoxCode.Items.Clear();
                comBoxCode.Enabled = true; // Enable the control

                var layerNodes = xmlDoc.SelectNodes("//Layer");
                foreach (XmlNode layerNode in layerNodes)
                {
                    var layerNameNode = layerNode.SelectSingleNode("LayerName");
                    var configLayerName = layerNameNode.InnerText;

                    if (configLayerName == layerName)
                    {
                        var typeNodes = layerNode.SelectNodes("Type");
                        foreach (XmlNode typeNode in typeNodes)
                        {
                            var name = typeNode.SelectSingleNode("Name").InnerText;
                            if (name == TYPE)
                            {
                                var codeNodes = typeNode.SelectNodes("CODE");
                                foreach (XmlNode codeNode in codeNodes)
                                {
                                    var code = codeNode.InnerText;
                                    comBoxCode.Items.Add(code);
                                }

                                return; // Exit loop after finding matching TYPE
                            }
                        }

                        break; // Exit loop after finding matching layer
                    }
                }
            }
            else
            {
                comBoxCode.Enabled = false; // Disable the control if TYPE is not selected
            }
        }

        private void InitializeDjs()
        {
            if (!string.IsNullOrEmpty(TYPE))
            {
                comBoxDj.Items.Clear();
                comBoxDj.Enabled = true; // 启用控件

                var layerNodes = xmlDoc.SelectNodes("//Layer");
                foreach (XmlNode layerNode in layerNodes)
                {
                    var layerNameNode = layerNode.SelectSingleNode("LayerName");
                    var configLayerName = layerNameNode.InnerText;

                    if (configLayerName == layerName)
                    {
                        var typeNodes = layerNode.SelectNodes("Type");
                        foreach (XmlNode typeNode in typeNodes)
                        {
                            var name = typeNode.SelectSingleNode("Name").InnerText;
                            if (name == TYPE)
                            {
                                var djNodes = typeNode.SelectNodes("DJ");
                                foreach (XmlNode djNode in djNodes)
                                {
                                    var dj = djNode.InnerText;
                                    comBoxDj.Items.Add(dj);
                                }

                                return; // 找到匹配的 TYPE 后退出循环
                            }
                        }

                        break; // 找到匹配的图层后退出循环
                    }
                }
            }
            else
            {
                comBoxDj.Enabled = false; // 如果没有选择 TYPE，则禁用控件
            }
        }

        private void InitializeFields()
        {
            FieldArray.Clear(); // 清空数组，以防已有数据

            var layerNodes = xmlDoc.SelectNodes("//Layer");
            foreach (XmlNode layerNode in layerNodes)
            {
                var layerNameNode = layerNode.SelectSingleNode("LayerName");
                var configLayerName = layerNameNode.InnerText;

                if (configLayerName == layerName)
                {
                    var typeNodes = layerNode.SelectNodes("Type");
                    foreach (XmlNode typeNode in typeNodes)
                    {
                        var type = typeNode.SelectSingleNode("Name").InnerText;
                        var gbNode = typeNode.SelectSingleNode("GB");
                        var codeNodes = typeNode.SelectNodes("CODE");
                        var djNode = typeNode.SelectSingleNode("DJ");

                        // 添加符合条件的字段到 FieldArray
                        FieldArray.Add("TYPE");

                        if (gbNode != null && !string.IsNullOrEmpty(gbNode.InnerText)) FieldArray.Add("GB");

                        foreach (XmlNode codeNode in codeNodes)
                            if (!string.IsNullOrEmpty(codeNode.InnerText))
                            {
                                FieldArray.Add("CODE");
                                break; // 添加一次后退出循环
                            }

                        if (djNode != null && !string.IsNullOrEmpty(djNode.InnerText)) FieldArray.Add("DJ");

                        break; // 只添加一次类型，然后退出循环
                    }

                    break; // 找到匹配的图层后退出循环
                }
            }
        }


        private void InitializeControls()
        {
            // 初始化 comboBoxField 的选项
            comboBoxField.Items.AddRange(FieldArray.ToArray());

            // 将 comboBoxField 添加到 tableLayoutPanelAll 的第一列
            tableLayoutPanelAll.Controls.Add(comboBoxField, 0, 0);

            // 初始化默认选项
            //dynamicControl = GetDynamicControl("TYPE");
            //dynamicControl.Dock = DockStyle.Fill;
            //tableLayoutPanelAll.Controls.Add(dynamicControl, 1, 0);
        }


        private Control GetDynamicControl()
        {
            Control newControl = null;

            if (fieldOption == "TYPE")
            {
                comBoxType = new ComboBox();

                InitializeTypes();

                comBoxType.SelectedIndexChanged += comBoxType_SelectedIndexChanged;

                newControl = comBoxType;
            }
            else if (fieldOption == "GB")
            {
                //textBoxGb = new TextBox();

                //textBoxGb.TextChanged += textBoxGb_TextChanged;

                //newControl = textBoxGb;

                comBoxGb = new ComboBox();

                InitializeGbs();

                comBoxGb.SelectedIndexChanged += comBoxGb_SelectedIndexChanged;

                newControl = comBoxGb;
            }
            else if (fieldOption == "CODE")
            {
                //newControl = new Label();
                //(newControl as Label).Text = "Enter Code:";

                comBoxCode = new ComboBox();

                InitializeCodes();

                comBoxCode.SelectedIndexChanged += comBoxCode_SelectedIndexChanged;

                newControl = comBoxCode;
            }
            else if (fieldOption == "DJ")
            {
                //newControl = new NumericUpDown();

                comBoxDj = new ComboBox();

                InitializeDjs();

                comBoxDj.SelectedIndexChanged += comBoxDj_SelectedIndexChanged;

                newControl = comBoxDj;
            }
            // ... 可根据需要添加更多选项

            return newControl ?? new Label(); // 默认返回 Label 控件
        }

        private void comboBoxField_SelectedIndexChanged(object sender, EventArgs e)
        {
            fieldOption = comboBoxField.SelectedItem.ToString();

            // 清除当前的动态控件
            tableLayoutPanelAll.Controls.Remove(dynamicControl);

            // 获取新的动态控件
            dynamicControl = GetDynamicControl();
            dynamicControl.Dock = DockStyle.Fill;

            // 将新的动态控件添加到 tableLayoutPanelAll 的第二列
            tableLayoutPanelAll.Controls.Add(dynamicControl, 1, 0);
        }

        private void comBoxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 将选中的字符串赋值给全局变量 TYPE
            if (comBoxType != null && comBoxType.SelectedItem != null) TYPE = comBoxType.SelectedItem.ToString();
        }


        private void comBoxGb_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 将选中的字符串赋值给全局变量 GB
            if (comBoxGb != null && comBoxGb.SelectedItem != null) GB = comBoxGb.SelectedItem.ToString();
        }

        private void comBoxCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 将选中的字符串赋值给全局变量 CODE
            if (comBoxCode != null && comBoxCode.SelectedItem != null) CODE = comBoxCode.SelectedItem.ToString();
        }

        private void comBoxDj_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 将选中的字符串赋值给全局变量 DJ
            if (comBoxDj != null && comBoxDj.SelectedItem != null) DJ = comBoxDj.SelectedItem.ToString();
        }

        private void textBoxGb_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TYPE))
            {
                MessageBox.Show("请先选择TYPE!");
                return;
            }

            textBoxGb.Clear();
            GB = textBoxGb.Text;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var allFieldsSelected = true;

            foreach (var field in FieldArray)
                if (!IsFieldSelected(field))
                {
                    allFieldsSelected = false;
                    break;
                }

            if (allFieldsSelected)
            {
                MessageBox.Show("TYPE: " + TYPE);
                MessageBox.Show("GB: " + GB);
                MessageBox.Show("CODE: " + CODE);
                MessageBox.Show("DJ: " + DJ);
                Close();
            }
            else
            {
                MessageBox.Show("请确保所有字段都已选择！");
            }
        }

        private bool IsFieldSelected(string fieldName)
        {
            switch (fieldName)
            {
                case "TYPE":
                    return !string.IsNullOrEmpty(TYPE);
                case "GB":
                    return !string.IsNullOrEmpty(GB);
                case "CODE":
                    return !string.IsNullOrEmpty(CODE);
                case "DJ":
                    return !string.IsNullOrEmpty(DJ);
                // Add more cases for other fields if needed
                default:
                    return false;
            }
        }


        private void buttonCancel_Click(object sender, EventArgs e)
        {
            MessageBox.Show("信息将不会添加。如需取消本条要素修改，请在工具条点击撤销按钮");
            Close();
        }
    }
}