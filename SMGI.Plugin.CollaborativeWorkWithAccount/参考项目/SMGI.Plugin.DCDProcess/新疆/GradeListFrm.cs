using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Plugin.DCDProcess
{
    //选取等级 设置 对话框   
    //2022.2.28 创建 张怀相
    //用来设置 OneGrade（采用特定等级）、Grade(具体的特定等级) 两个参数
    //功能修改：
    //  1、OneGrade = true（默认），需要从Grade的下拉列表里选择特定等级
    //  2、OneGrade = false，       Grade的下拉列表里选择特定等级
    //  3、从下拉列表里选择特定等级，程序只对符合该等级的河流进行GRADE2赋值
    public partial class GradeListFrm : Form
    {
        public bool OneGrade
        {
            get { return checkBox1.Checked; } 
        }

        public int Grade
        {
            get
            {
                if (comboBox1.SelectedItem != null)
                {
                    return Int32.Parse(comboBox1.SelectedItem.ToString());
                }
                else
                {
                    return -2;
                }
            }
        }
        

        public GradeListFrm(string title,List<int> grades,bool xoneGrade=true)
        {
            InitializeComponent();
            this.Text = title;
            checkBox1.Checked = xoneGrade;
            comboBox1.Items.Clear();
            if (grades.Count > 0)
            {
                foreach (int grade in grades)
                {
                    comboBox1.Items.Add(grade.ToString());
                }
                comboBox1.SelectedIndex = 0;
            }

            this.button1.Click += (o, e) =>
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            };
            this.button2.Click += (o, e) =>
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            };
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                comboBox1.Enabled = true;
            }
            else
            {
                comboBox1.Enabled = false;
            }
        }
    }
}
