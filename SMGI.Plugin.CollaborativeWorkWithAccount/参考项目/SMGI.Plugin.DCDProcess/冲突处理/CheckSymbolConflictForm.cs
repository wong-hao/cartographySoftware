using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.DCDProcess
{
    public partial class CheckSymbolConflictForm : Form
    {
        /// <summary>
        /// 待检查的目标图层
        /// </summary>
        public IFeatureLayer ObjFeatureLayer
        {
            get
            {
                if (cbObjectLayer.SelectedItem == null)
                    return null;

                var selItem = (KeyValuePair<IFeatureLayer, string>)cbObjectLayer.SelectedItem;
                return selItem.Key;
            }
        }

        public IFeatureLayer ConnFeatureLayer
        {
            get
            {
                if (cbConnLayer.SelectedItem == null)
                    return null;

                var selItem = (KeyValuePair<IFeatureLayer, string>)cbConnLayer.SelectedItem;
                return selItem.Key;
            }
        }

        public double ReferenceScale
        {
            get
            {
                return double.Parse(tbScale.Text);
            }
        }

        public string OutputPath
        {
            get
            {
                return tbOutFilePath.Text;
            }
        }

        public CheckSymbolConflictForm(double scale)
        {
            InitializeComponent();

            tbScale.Text = scale.ToString();
        }

        private void CheckSymbolConflictForm_Load(object sender, EventArgs e)
        {
            cbObjectLayer.ValueMember = "Key";
            cbObjectLayer.DisplayMember = "Value";

            cbConnLayer.ValueMember = "Key";
            cbConnLayer.DisplayMember = "Value";

            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && (l as IFeatureLayer).FeatureClass != null;
            })).ToArray();
            if (lyrs != null)
            {
                foreach (var l in lyrs)
                {
                    cbObjectLayer.Items.Add(new KeyValuePair<IFeatureLayer, string>(l as IFeatureLayer, l.Name));
                    cbConnLayer.Items.Add(new KeyValuePair<IFeatureLayer, string>(l as IFeatureLayer, l.Name));
                }
            }

            

        }

        private void cbObjectLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void btnFilePath_Click(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK && fd.SelectedPath.Length > 0)
            {
                tbOutFilePath.Text = fd.SelectedPath;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (ObjFeatureLayer == null)
            {
                MessageBox.Show(string.Format("请指定待检查的目标图层！"));
                return;
            }

            if (ConnFeatureLayer == null)
            {
                MessageBox.Show("请指定冲突检测的相关图层！");
                return;
            }

            if (tbOutFilePath.Text == "")
            {
                MessageBox.Show("请选择检查结果输出路径！");
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
