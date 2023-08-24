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
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public partial class CheckSamePlaceNameForm : Form
    {
        #region 属性
        public IFeatureClass ObjFeatureClass
        {
            get;
            internal set;
        }

        public List<string> FieldNameList
        {
            get
            {
                return _fieldNameList;
            }
        }
        private List<string> _fieldNameList;

        public double Distance
        {
            get;
            internal set;
        }

        public string OutputPath
        {
            get
            {
                return tbOutFilePath.Text;
            }
        }
        #endregion

        private GApplication _app;
        public CheckSamePlaceNameForm(GApplication app)
        {
            InitializeComponent();

            _app = app;
        }

        private void CheckSamePlaceNameForm_Load(object sender, EventArgs e)
        {
            cbLayerNames.ValueMember = "Key";
            cbLayerNames.DisplayMember = "Value";

            

            //检索所有的图层名称
            var layers = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint));
            foreach (var lyr in layers)
            {
                cbLayerNames.Items.Add(new KeyValuePair<IFeatureClass, string>((lyr as IFeatureLayer).FeatureClass, lyr.Name));
            }

            /*

            int agnpIndex = -1;

            for (int i = 0; i < cbLayerNames.Items.Count; ++i)
            {
                var item = (KeyValuePair<IFeatureClass, string>)cbLayerNames.Items[i];
                if (item.Key.AliasName.ToUpper() == "AGNP")
                {
                    agnpIndex = i;
                    break;
                }
            }
            if (agnpIndex != -1)
            {
                cbLayerNames.SelectedIndex = agnpIndex;
            }
             */

            cbLayerNames.SelectedIndex = 0;

            tbOutFilePath.Text = OutputSetup.GetDir();
        }

        private void cbLayerNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkFieldList.Items.Clear();
            if (cbLayerNames.SelectedIndex != -1)
            {
                var item = (KeyValuePair<IFeatureClass, string>)cbLayerNames.SelectedItem;
                IFeatureClass fc = item.Key;
                if (fc != null)
                {
                    for (var i = 0; i < fc.Fields.FieldCount; i++)
                    {
                        var fd = fc.Fields.Field[i];

                        if (fd.Type == esriFieldType.esriFieldTypeGeometry ||
                            fd.Type == esriFieldType.esriFieldTypeOID)
                            continue;

                        if (fd.Name.ToUpper() == "SHAPE_LENGTH" || fd.Name.ToUpper() == "SHAPE_AREA")
                            continue;

                        chkFieldList.Items.Add(fd.Name);
                    }
                }
            }

        }

        private void cbDistance_CheckedChanged(object sender, EventArgs e)
        {
            tbDistance.Enabled = cbDistance.Checked;
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
            if (cbLayerNames.SelectedIndex == -1)
            {
                MessageBox.Show("请指定检查的目标点图层！");
                return;
            }
            ObjFeatureClass = ((KeyValuePair<IFeatureClass, string>)cbLayerNames.SelectedItem).Key;

            if (chkFieldList.CheckedItems.Count == 0)
            {
                MessageBox.Show("请选择至少一个字段项！");
                return;
            }

            Distance = -1;//-1时表示不启用距离阈值
            if (cbDistance.Checked)
            {
                double dis = 0;
                double.TryParse(tbDistance.Text, out dis);
                if (dis == 0)
                {
                    MessageBox.Show("请指定一个合法的距离阈值！");
                    return;
                }

                Distance = dis;
            }
            
            if (tbOutFilePath.Text == "")
            {
                MessageBox.Show("请指定检查结果输出路径!");
                return;
            }

            _fieldNameList = new List<string>();
            foreach (var item in chkFieldList.CheckedItems)
            {
                _fieldNameList.Add(item.ToString());
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;

        }
    }
}
