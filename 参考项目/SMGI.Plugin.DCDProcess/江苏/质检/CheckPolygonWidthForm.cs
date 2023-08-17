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
    public partial class CheckPolygonWidthForm : Form
    {
        /// <summary>
        /// 参考比例尺
        /// </summary>
        public double ReferScale
        {
            get
            {
                return double.Parse(tbReferScale.Text);
            }
        }

        /// <summary>
        /// 待检查的面要素及要求的最小图面宽度（mm）信息
        /// </summary>
        public Dictionary<KeyValuePair<string, string>, double> FeatureType2Width
        {
            get
            {
                return _feType2Width;
            }
        }
        private Dictionary<KeyValuePair<string, string>, double> _feType2Width;

        public CheckPolygonWidthForm(double scale, Dictionary<KeyValuePair<string, string>, double> feType2Width)
        {
            InitializeComponent();

            tbReferScale.Text = scale.ToString();

            //初始化检查要素信息表
            foreach (var kv in feType2Width)
            {
                object[] datas = new object[3];
                datas[0] = kv.Key.Key;
                datas[1] = kv.Key.Value;
                datas[2] = kv.Value;

                dataGridView.Rows.Insert(dataGridView.Rows.Count, datas);
            }
            dataGridView.ClearSelection();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            double scale = 0;
            double.TryParse(tbReferScale.Text, out scale);
            if (scale <= 0)
            {
                MessageBox.Show("请先为地图指定一个合法的参考比例尺！");
                return;
            }

            _feType2Width = new Dictionary<KeyValuePair<string, string>, double>();
            for (int i = 0; i < dataGridView.Rows.Count; ++i)
            {
                var row = dataGridView.Rows[i];

                string fcName = row.Cells["FCName"].Value.ToString();
                string filter = row.Cells["sqlFilter"].Value.ToString();
                double minWidth = 0;
                double.TryParse(row.Cells["minWidth"].Value.ToString(), out minWidth);
                if (minWidth <= 0)
                {
                    MessageBox.Show("最小图面宽度存在不合法的参数！");
                    return;
                }

                KeyValuePair<string, string> kv = new KeyValuePair<string, string>(fcName, filter);
                _feType2Width.Add(kv, minWidth);
            }


            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
