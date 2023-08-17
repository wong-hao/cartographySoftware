using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ESRI.ArcGIS.Carto;
namespace SMGI.Plugin.DCDProcess.DataProcess
{
    public partial class FrmRiverClass : DevExpress.XtraEditors.XtraForm
    {
        public IMap pMap { get; set; }
        public ILayer hydlLayer;
        public FrmRiverClass(IMap _pmap)
        {
            InitializeComponent();
            pMap = _pmap;

        }

        private void FrmRiverClass_Load(object sender, EventArgs e)
        {
            var ls = pMap.get_Layers();
            var layer = ls.Next();
            for (; layer != null; layer = ls.Next())
            {
                if (!(layer is ESRI.ArcGIS.Carto.IFeatureLayer))
                    continue;
                cmbHydlLayerSelect.Items.Add(layer.Name.ToString());
                if ((layer as IFeatureLayer).FeatureClass.AliasName == "HYDL")
                {
                    cmbHydlLayerSelect.SelectedItem = layer.Name.ToString();
                }
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            string hydlSelectLayerName = cmbHydlLayerSelect.SelectedItem.ToString();
          

            var ls = pMap.get_Layers();
            var layer = ls.Next();
            for (; layer != null; layer = ls.Next())
            {
                if (!(layer is ESRI.ArcGIS.Carto.IFeatureLayer))
                    continue;
                if (layer.Name == hydlSelectLayerName)
                {
                    hydlLayer = layer;
                }

                
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}