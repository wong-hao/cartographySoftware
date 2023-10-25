using System.Runtime.InteropServices;
using System.Windows.Forms;
using DevExpress.XtraBars.Ribbon;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;

namespace SMGI.Plugin.DCDProcess
{
    public class EntitySelectTool : SMGITool
    {
        private IFeatureClass _targetFcls;
        EntitySelectFrm _selectFrm;
        /// <summary>
        /// 实体选择
        /// </summary>
        public EntitySelectTool()
        {
            m_caption = "实体选择";
            //m_cursor =
            //    new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "Brush.cur"));
            m_category = "数据编辑";
            m_toolTip = "根据属性相同的实体片段全部选中";
            NeedSnap = false;
        }

        public override void OnClick()
        {
            if (_selectFrm == null)
            {
                _selectFrm = new EntitySelectFrm(m_Application.ActiveView.FocusMap);
                var sz = (m_Application.MainForm as RibbonForm).Size;
                _selectFrm.Location = new System.Drawing.Point(sz.Width - _selectFrm.Size.Width - 30, _selectFrm.Size.Height - 20);
                _selectFrm.Show(m_Application.MainForm as IWin32Window);
            }
            else
            {
                if (!_selectFrm.Visible)
                    _selectFrm.Show(m_Application.MainForm as IWin32Window);
            }
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            //获取圆心点
            //_cenPt = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            var fds = _selectFrm.SelectFields;
            if (fds.Count < 1)
            {
                MessageBox.Show("请先设置图层和字段！");
                if (!_selectFrm.Visible)
                    _selectFrm.Show(m_Application.MainForm as IWin32Window);
                return;
            }
            var env = m_Application.MapControl.TrackRectangle();
            var layer = _selectFrm.SelectFeatureLayer;
            var fcls = layer.FeatureClass;
            var sfilter = new SpatialFilterClass
            {
                Geometry = env,
                SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects
            };
            var pCur = fcls.Search(sfilter, true);
            var fea = pCur.NextFeature();
            if (fea != null)
            {
                var sqlstr = "1=1 ";
                //根据相同属性查找同类
                foreach (string t in fds)
                {
                    var idx = fcls.FindField(t);
                    if (fcls.Fields.Field[idx].Type == esriFieldType.esriFieldTypeString)  
                    {
                        if (fea.Value[idx] != System.DBNull.Value)
                            sqlstr += " and " + t + "='" + fea.Value[idx] + "'";
                        else
                            sqlstr += " and " + t + " is null";
                    }
                    else
                    {
                        if (fea.Value[idx] != System.DBNull.Value)
                            sqlstr += " and " + t + "=" + fea.Value[idx];
                        else
                            sqlstr += " and " + t + " is null";
                    }

                }

                var selection = layer as IFeatureSelection;
                if (selection == null) return;
                selection.SelectFeatures(new QueryFilterClass { WhereClause = sqlstr }, esriSelectionResultEnum.esriSelectionResultNew, false);
                Helper.RefreshAttributeWindow(layer);

                m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            }
            if (fea != null)
                Marshal.ReleaseComObject(fea);
            Marshal.ReleaseComObject(pCur);
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;
            }
        }
    }
}
