using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.DCDProcess.DataProcess
{
    public partial class FrmBackspace : Form
    {
        private GApplication _app;
        private Dictionary<string, IFeatureClass> _fcName2FC;

        public IFeatureClass ProcFeatureClass
        {
            get
            {
                return _fcName2FC[cmbLayer.Text];
            }
        }

        public List<string> fieldNameList
        {
            get
            {
                return (from object t in cklsFields.CheckedItems select t.ToString()).ToList();
            }
        }
 
        public FrmBackspace(GApplication app)
        {
            InitializeComponent();

            _app = app;
            _fcName2FC = new Dictionary<string, IFeatureClass>();
        }

        private void FrmBackspace_Load(object sender, EventArgs e)
        {
            IEngineEditLayers editLayer = _app.EngineEditor as IEngineEditLayers;

            //获取所有可编辑图层列表
            var pLayers = _app.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
                (l is IGeoFeatureLayer) )).ToArray();
            for (int i = 0; i < pLayers.Length; i++)
            {
                IFeatureLayer layer = pLayers[i] as IFeatureLayer;
                if ((layer.FeatureClass as IDataset).Workspace.PathName != _app.Workspace.EsriWorkspace.PathName)//临时数据不参与
                    continue;


                if (!editLayer.IsEditable(layer))
                    continue;


                _fcName2FC.Add(layer.Name.ToUpper(), layer.FeatureClass);
            }

            cmbLayer.Items.AddRange(_fcName2FC.Keys.ToArray());
        }

        private void cmbLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetFieldCheck(_fcName2FC[cmbLayer.Text]);
        }


        private void btn_All_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < cklsFields.Items.Count; i++)
            {
                cklsFields.SetItemChecked(i, true);
            }
        }

        private void btn_Nothing_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < cklsFields.Items.Count; i++)
            {
                cklsFields.SetItemChecked(i, false);
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (cmbLayer.Text == "")
            {
                MessageBox.Show("没选择处理图层！");
                return;
            }

            if (cklsFields.CheckedItems.Count < 1)
            {
                MessageBox.Show("没选择检查字段！"); 
                return;
            }



            DialogResult = System.Windows.Forms.DialogResult.OK;

        }


        private void SetFieldCheck(IFeatureClass pfClass)
        {
            var fds = pfClass.Fields;
            var flist = new List<string>();
            for (var i = 0; i < fds.FieldCount; i++)
            {
                var fd = fds.Field[i];
                if (fd.Type == esriFieldType.esriFieldTypeString && fd.Name.ToUpper() != "PINYIN")
                {

                    flist.Add(fd.Name);
                }
            }

            cklsFields.Items.Clear();
            cklsFields.Items.AddRange(flist.ToArray());
        }

        
    }
}
