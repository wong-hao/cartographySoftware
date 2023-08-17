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
using ESRI.ArcGIS.Display;
using SMGI.Common;

namespace SMGI.Plugin.DCDProcess
{
    public partial class SelectInterFeatureForm : Form
    {
        public InterFeatureWarp InterFeature
        {
            get;
            internal set;
        }

        private List<InterFeatureWarp> _interFeList;

        public SelectInterFeatureForm(List<InterFeatureWarp> interFeList)
        {
            InitializeComponent();

            _interFeList = interFeList;
        }

        private void SelectInterFeatureForm_Load(object sender, EventArgs e)
        {
            FeListBox.Items.AddRange(_interFeList.ToArray());
            if (FeListBox.Items.Count > 0)
                FeListBox.SelectedIndex = 0;
        }

        private void FeListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var fw = FeListBox.SelectedItem as InterFeatureWarp;
            if (fw == null)
                return;

            //闪烁
            GApplication.Application.MapControl.FlashShape(fw.InterPolyline);

        }

        private void FeListBox_DoubleClick(object sender, EventArgs e)
        {
            InterFeature = FeListBox.SelectedItem as InterFeatureWarp;

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            InterFeature = FeListBox.SelectedItem as InterFeatureWarp; 

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        
    }
}
