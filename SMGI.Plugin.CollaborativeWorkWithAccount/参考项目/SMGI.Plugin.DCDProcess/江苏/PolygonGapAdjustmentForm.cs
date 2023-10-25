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
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.DCDProcess
{
    public partial class PolygonGapAdjustmentForm : Form
    {
        public IFeatureLayer ObjFeatureLayer
        {
            get;
            internal set;
        }

        public Dictionary<IFeature, IGeometry> Fe2BufferGeo
        {
            get;
            internal set;
        }

        private GApplication _app;
        public PolygonGapAdjustmentForm(GApplication app)
        {
            InitializeComponent();

            _app = app;
        }

        private void PolygonGapAdjustmentForm_Load(object sender, EventArgs e)
        {
            //检索当前工作空间中所有的面图层名称
            Dictionary<IFeatureLayer, string> lyr2lyrname = new Dictionary<IFeatureLayer, string>();
            var lyrs = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon
                    && ((l as IGeoFeatureLayer).FeatureClass as IDataset).Workspace.PathName == _app.Workspace.EsriWorkspace.PathName;
            })).ToArray();

            foreach (var lyr in lyrs)
            {
                lyr2lyrname.Add(lyr as IFeatureLayer, lyr.Name);
            }

            cbLayerNames.ValueMember = "Key";
            cbLayerNames.DisplayMember = "Value";
            if (lyr2lyrname.Count > 0)
            {
                foreach (var item in lyr2lyrname)
                {
                    cbLayerNames.Items.Add(new KeyValuePair<IFeatureLayer, string>(item.Key, item.Value));
                }
                cbLayerNames.SelectedIndex = 0;
            }
        }

        private void PolygonGapAdjustmentForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            (_app.MapControl.Map as IGraphicsContainer).DeleteAllElements();
            _app.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, _app.MapControl.ActiveView.Extent);
        }

        private void bufferRangeBtn_Click(object sender, EventArgs e)
        {
            if (!ParamIsValidate())
                return;

            UpdateBufferGeometry();

            (_app.MapControl.Map as IGraphicsContainer).DeleteAllElements();
            _app.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, _app.MapControl.ActiveView.Extent);

            //绘制临时元素
            if (Fe2BufferGeo.Count > 0)
            {
                IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                fillSymbol.Color = new RgbColorClass() { Red = 255, Green = 255, Blue = 255, Transparency = 0 };
                ILineSymbol lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Color = new RgbColorClass() { Red = 255, Green = 0, Blue = 0 };
                lineSymbol.Width = 0.5;
                fillSymbol.Outline = lineSymbol;

                foreach (var kv in Fe2BufferGeo)
                {
                    IElement bufferEle = new PolygonElementClass();
                    bufferEle.Geometry = kv.Value;
                    (bufferEle as IFillShapeElement).Symbol = fillSymbol;

                    (_app.MapControl.Map as IGraphicsContainer).AddElement(bufferEle, 0);
                }
                _app.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, _app.MapControl.ActiveView.Extent);

            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (!ParamIsValidate())
                return;

            ObjFeatureLayer = ((KeyValuePair<IFeatureLayer, string>)cbLayerNames.SelectedItem).Key;

            UpdateBufferGeometry();


            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private bool ParamIsValidate()
        {
            double bufferVal = 0;
            double.TryParse(tbBufferValue.Text, out bufferVal);
            if (bufferVal <= 0)
            {
                MessageBox.Show("请指定一个合法的缓冲距离值", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }

            if (string.IsNullOrEmpty(cbLayerNames.Text))
            {
                MessageBox.Show("请指定目标面图层！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }

            return true;
        }

        private void UpdateBufferGeometry()
        {
            Fe2BufferGeo = new Dictionary<IFeature, IGeometry>();

            double bufferDist = double.Parse(tbBufferValue.Text);

            IEnumFeature enumFeature = _app.MapControl.Map.FeatureSelection as IEnumFeature;
            IFeature fe = null;
            while ((fe = enumFeature.Next()) != null)
            {
                if (fe.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)//线要素缓冲：末端采用平头
                {
                    IBufferConstruction bfConstrut = new BufferConstructionClass();
                    (bfConstrut as IBufferConstructionProperties).EndOption = esriBufferConstructionEndEnum.esriBufferFlat;

                    IEnumGeometry enumGeo = new GeometryBagClass();
                    (enumGeo as IGeometryCollection).AddGeometry(fe.Shape);
                    IGeometryCollection outputBuffer = new GeometryBagClass();

                    //IGeometry bufferShape = bfConstrut.Buffer(fe.Shape, bufferDist);//无法利用IBufferConstructionProperties设置相关选项
                    bfConstrut.ConstructBuffers(enumGeo, bufferDist, outputBuffer);

                    IGeometry bufferShape = null;
                    for (int i = 0; i < outputBuffer.GeometryCount; i++)
                    {
                        IGeometry geo = outputBuffer.get_Geometry(i);
                        if (bufferShape == null)
                        {
                            bufferShape = geo;
                        }
                        else
                        {
                            bufferShape = (bufferShape as ITopologicalOperator).Union(geo);
                        }
                    }
                    Fe2BufferGeo.Add(fe, bufferShape);
                }
                else
                {
                    IGeometry bufferShape = (fe.Shape as ITopologicalOperator).Buffer(double.Parse(tbBufferValue.Text));
                    Fe2BufferGeo.Add(fe, bufferShape);
                }
                
            }
        }

        
    }
}
