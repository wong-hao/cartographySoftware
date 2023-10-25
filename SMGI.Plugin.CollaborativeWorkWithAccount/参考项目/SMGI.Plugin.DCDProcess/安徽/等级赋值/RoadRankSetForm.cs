using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;

using ESRI.ArcGIS.Controls;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using System.Diagnostics;
using SMGI.Common.Algrithm;

namespace SMGI.Plugin.DCDProcess
{
    public partial class RoadRankSetForm : Form
    {
        GApplication m_Application;
        public IFeatureClass layerclass;
        public ILayer layer;
        public int idx;
        public string str=null;
        public RoadRankSetForm(int s, GApplication app)
        {
            InitializeComponent();
            level = s;
            m_Application = app;
        }
        public int level
        {
            get;
            set;
        }
        string[] levels = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
        private void RoadRankSetForm_Load(object sender, EventArgs e)
        {
            foreach (var s in levels)
            {
                listBox.Items.Add(s);
            }
            for (int k = 0; k < listBox.Items.Count; k++)
            {
                if (listBox.Items[k].ToString() == level.ToString())
                {
                    listBox.SetSelected(k, true);
                }
            }
            cmbLayers.Items.Add("LRDL");
            cmbLayers.Items.Add("AGNP");
            cmbLayers.Items.Add("HYDL");
            cmbLayers.SelectedIndex = 0;
        }
      
        //public void setValue(int value)
        //{
        //    for (int k = 0; k < listBox.Items.Count; k++)
        //    {
        //        if (listBox.Items[k].ToString() == value.ToString())
        //        {
        //            listBox.SetSelected(k, true);
        //        }
        //    }

        //}

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            level = int.Parse(listBox.SelectedItem.ToString());
        }

        private void cmbLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            layer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == cmbLayers.Text)).FirstOrDefault();
            if (layer == null)
            {
                MessageBox.Show(string.Format("无{0}图层！ ", cmbLayers.Text));
                return;
            }
            layerclass = (layer as IFeatureLayer).FeatureClass;
            str = cmbLayers.Text;
            if (str == "LRDL")
            {
                idx = layerclass.FindField("GRADE2");
                if (idx == -1)
                {
                    MessageBox.Show("LRDL图层无GRADE2字段", "提示");
                    return;
                }
            }
            if (str == "AGNP")
            {
                idx = layerclass.FindField("PRIORITY");
                if (idx == -1)
                {
                    MessageBox.Show("AGNP图层无PRIORITY字段", "提示");
                    return;
                }
            }
            if (str == "HYDL")
            {
                idx = layerclass.FindField("GRADE2");
                if (idx == -1)
                {
                    MessageBox.Show("HYDL图层无GRADE2字段", "提示");
                    return;
                }
            }
        }
    }
}
