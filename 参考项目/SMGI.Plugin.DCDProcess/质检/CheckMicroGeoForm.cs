using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.DCDProcess
{
    public partial class CheckMicroGeoForm : Form
    {
        #region 属性
        public List<IFeatureLayer> CheckFeatureLayerList
        {
            get
            {
                return _checkFeatureLayerList;
            }
        }
        private List<IFeatureLayer> _checkFeatureLayerList;

        public double MicroLenThreshold
        {
            get
            {
                return double.Parse(tbMicroLenThreshold.Text);
            }
        }

        public double MicroAreaThreshold
        {
            get
            {
                return double.Parse(tbMicroAreaThreshold.Text);
            }
        }

        public string OutputPath
        {
            get
            {
                return tbOutFilePath.Text;
            }
        }

        public bool UsingRuleTable 
        { 
            get 
            { 
                return checkBox_LoadFromRuleTable.Checked; 
            } 
        }

        public string RuleTablePath
        {
            get
            {
                return textBox_RuleTablePath.Text;
            }
        }
        #endregion

        private GApplication _app;
        public CheckMicroGeoForm(GApplication app)
        {
            InitializeComponent();

            _app = app;
        }

        private void CheckLayerSelectForm_Load(object sender, EventArgs e)
        {
            //检索所有的点图层名称
            chkLayerList.ValueMember = "Key";
            chkLayerList.DisplayMember = "Value";
            var layers = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer));
            foreach (var lyr in layers)
            {
                IFeatureLayer feLayer = lyr as IFeatureLayer;
                if (feLayer.FeatureClass == null)
                    continue;//空图层

                if ((feLayer.FeatureClass as IDataset).Workspace.PathName != _app.Workspace.EsriWorkspace.PathName)
                    continue;//临时数据

                if (feLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
                    continue;//点图层

                chkLayerList.Items.Add(new KeyValuePair<IFeatureLayer, string>(feLayer, feLayer.Name));
            }

            tbOutFilePath.Text = OutputSetup.GetDir();

            string mdbPath = String.Join("\\", GApplication.Application.Template.Root, "质检", "质检内容配置.mdb");
            if (!System.IO.File.Exists(mdbPath))
            { return; }
            //DataTable dataTable = DCDHelper.ReadToDataTable(mdbPath, "微短要素检查");
            //if (dataTable == null)
            //{ return; }
            textBox_RuleTablePath.Text = String.Join("\\", mdbPath, "微短要素检查");
        }

        private void btnSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkLayerList.Items.Count; i++)
            {
                chkLayerList.SetItemChecked(i, true);
            }
        }

        private void btnUnSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkLayerList.Items.Count; i++)
            {
                chkLayerList.SetItemChecked(i, false);
            }
            chkLayerList.ClearSelected();
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
            if (chkLayerList.CheckedItems.Count == 0)
            {
                MessageBox.Show("请选择至少一个图层进行检查！");
                return;
            }

            double len = 0;
            if (!double.TryParse(tbMicroLenThreshold.Text, out len))
            {
                MessageBox.Show("长度阈值设置不合法！");
                return;
            }

            double area = 0;
            if (!double.TryParse(tbMicroAreaThreshold.Text, out area))
            {
                MessageBox.Show("面积阈值设置不合法！");
                return;
            }

            if (tbOutFilePath.Text == "")
            {
                MessageBox.Show("请指定检查结果输出路径！");
                return;
            }

            _checkFeatureLayerList = new List<IFeatureLayer>();
            foreach (var lyr in chkLayerList.CheckedItems)
            {
                var kv = (KeyValuePair<IFeatureLayer, string>)lyr;
                _checkFeatureLayerList.Add(kv.Key);
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void checkBox_LoadFromRuleTable_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_LoadFromRuleTable.Checked)
            {
                this.tbMicroLenThreshold.Enabled = false;
                this.tbMicroAreaThreshold.Enabled = false;
            }
            else
            {
                this.tbMicroLenThreshold.Enabled = true;
                this.tbMicroAreaThreshold.Enabled = true;
            }
        }
    }
}
