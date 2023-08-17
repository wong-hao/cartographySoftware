using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.DCDProcess
{
    public partial class LayerLabelSetForm : Form
    {
        private List<string> _fieldNames;
        private IGeoFeatureLayer _layer;
        public LayerLabelSetForm(IGeoFeatureLayer layer)
        {
            InitializeComponent();
            _layer = layer;
            chb.Checked = _layer.DisplayAnnotation;

            _fieldNames = new List<string>();
            for (var i = 0; i < _layer.FeatureClass.Fields.FieldCount; i++)
            {
                var ln = _layer.FeatureClass.Fields.Field[i].Name;
                if(ln.ToUpper() != "SHAPE" && ln.ToUpper() != "OVERRIDE")
                    _fieldNames.Add(_layer.FeatureClass.Fields.Field[i].Name);
            }
            cbb.DataSource = _fieldNames;
            var fn = GetAnnoFieldName(_layer, "", false);
            if (!string.IsNullOrEmpty(fn)) cbb.SelectedItem = fn;
        }

        private void btn_Click(object sender, EventArgs e)
        {
            _layer.DisplayAnnotation = chb.Checked;
            var fn = cbb.SelectedItem.ToString();
            if (_fieldNames.SingleOrDefault(i => i == fn) != null)
                GetAnnoFieldName(_layer, fn, true);
        }

        public string GetAnnoFieldName(IGeoFeatureLayer layer, string fn, bool isSet)
        {
            var anprs = layer.AnnotationProperties;
            IAnnotateLayerProperties ap;
            IElementCollection ec1;
            IElementCollection ec2;
            anprs.QueryItem(0, out ap, out ec1, out ec2);
            var le = (ILabelEngineLayerProperties) ap;
            if (isSet)
            {
                le.Expression = "[" + fn + "]";
                return fn;
            }
            return le.Expression.Replace("[", "").Replace("]", "");
        }
    }
}
