using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private ComboBox comboBox1;
        private Control dynamicControl;
        private string layer = "乡镇街道"; // 默认值

        public Form1()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            comboBox1 = new ComboBox();
            comboBox1.Items.Add("TYPE");
            comboBox1.Items.Add("GB");
            comboBox1.Items.Add("CODE");
            comboBox1.Items.Add("DJ");
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;

            dynamicControl = GetDynamicControl("TYPE"); // 默认添加一个 ComboBox
            dynamicControl.Dock = DockStyle.Fill;

            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel.Controls.Add(comboBox1, 0, 0);
            tableLayoutPanel.Controls.Add(dynamicControl, 1, 0);

            Controls.Add(tableLayoutPanel);
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedOption = comboBox1.SelectedItem.ToString();

            dynamicControl.Dispose(); // 清除当前的动态控件

            dynamicControl = GetDynamicControl(selectedOption);
            dynamicControl.Dock = DockStyle.Fill;

            TableLayoutPanel tableLayoutPanel = Controls[0] as TableLayoutPanel;
            if (tableLayoutPanel != null)
            {
                tableLayoutPanel.Controls.Add(dynamicControl, 1, 0); // 将新的动态控件添加到第二列
            }
        }

        private Control GetDynamicControl(string option)
        {
            Control newControl = null;

            if (option == "TYPE")
            {
                ComboBox comboBox = new ComboBox();
                if (layer == "县以上居民地")
                {
                    comboBox.Items.AddRange(new string[] { "省会", "设区市", "综合实验区", "县级市", "县", "区" });
                }
                else if (layer == "乡镇街道")
                {
                    comboBox.Items.AddRange(new string[] { "乡", "镇", "街道" });
                }
                newControl = comboBox;
            }
            else if (option == "GB")
            {
                newControl = new TextBox();
            }
            else if (option == "CODE")
            {
                newControl = new Label();
                (newControl as Label).Text = "Enter Code:";
            }
            else if (option == "DJ")
            {
                newControl = new NumericUpDown();
            }
            // ... 可根据需要添加更多选项

            return newControl ?? new Label(); // 默认返回 Label 控件
        }

        // 修改 layer 的方法，例如在响应一个按钮的点击事件中修改 layer
        private void ChangeLayer(string newLayer)
        {
            layer = newLayer;
            // 根据新的 layer 更新 TYPE 的选项
            if (comboBox1.SelectedItem.ToString() == "TYPE")
            {
                ComboBox comboBox = dynamicControl as ComboBox;
                if (comboBox != null)
                {
                    comboBox.Items.Clear();
                    if (layer == "县以上居民地")
                    {
                        comboBox.Items.AddRange(new string[] { "省会", "设区市", "综合实验区", "县级市", "县", "区" });
                    }
                    else if (layer == "乡镇街道")
                    {
                        comboBox.Items.AddRange(new string[] { "乡", "镇", "街道" });
                    }
                }
            }
        }
    }
}
