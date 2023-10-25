using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    public partial class CheckIsolatedRiverFrm : Form
    {
        /// <summary>
        /// 阈值
        /// </summary>
        public double ThresholdValue
        {
            get
            {
                return double.Parse(tbThresholdVaule.Text);
            }
        }

        /// <summary>
        /// 范围面，若检查的孤立河流与该面边线相交或在面外，则不做检查
        /// </summary>
        public IPolygon RangeGeometry
        {
            get
            {
                return _rangeGeometry;
            }
        }
        private IPolygon _rangeGeometry;

        public CheckIsolatedRiverFrm()
        {
            InitializeComponent();

            _rangeGeometry = null;
        }

        public String SQLText
        {
            get
            {
                if (_sqlText != null)
                    return _sqlText;
                else
                    return "";
            }
        }
        private string _sqlText;

        private void chkShp_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShp.Checked)
            {
                btnShape.Enabled = true;
            }
            else
            {
                btnShape.Enabled = false;

                shapetxt.Text = "";

            }
        }

        private void btnShape_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "选择一个范围文件";
            dlg.AddExtension = true;
            dlg.DefaultExt = "shp";
            dlg.Filter = "选择文件|*.shp";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                shapetxt.Text = dlg.FileName;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            double val = 0;
            double.TryParse(tbThresholdVaule.Text, out val);
            if (val <= 0)
            {
                MessageBox.Show("请输入一个大于0的合法长度阈值！");
                return;
            }

            if (chkShp.Checked && shapetxt.Text != "")
            {
                ISpatialReference sr = getRangeGeometryReference(shapetxt.Text);
                if (sr == null)
                {
                    MessageBox.Show("范围文件没有空间参考！");
                    return;
                }
            }

            _sqlText = comboBox1.Text;

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        //读取shp文件,获取范围几何体并返回空间参考
        private ISpatialReference getRangeGeometryReference(string fileName)
        {
            ISpatialReference sr = null;

            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName(fileName), 0);
            IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
            IFeatureClass shapeFC = pFeatureWorkspace.OpenFeatureClass(System.IO.Path.GetFileName(fileName));

            //是否为多边形几何体
            if (shapeFC.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                MessageBox.Show("范围文件应为多边形几何体，请重新指定范围文件！");
                return sr;
            }

            //默认为第一个要素的几何体
            IFeatureCursor featureCursor = shapeFC.Search(null, false);
            IFeature pFeature = featureCursor.NextFeature();
            if (pFeature != null && pFeature.Shape is IPolygon)
            {
                _rangeGeometry = pFeature.Shape as IPolygon;
                sr = _rangeGeometry.SpatialReference;
            }
            Marshal.ReleaseComObject(featureCursor);

            return sr;
        }

        
    }
}
