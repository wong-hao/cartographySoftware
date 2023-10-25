using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Web;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using SMGI.Common;
using ESRI.ArcGIS.DataSourcesFile;

namespace SMGI.Plugin.DCDProcess
{
    public partial class LinearDitchesFeatureDealForm : Form
    {
        public LinearDitchesFeatureDealForm(GApplication app)
        {
            InitializeComponent();
            m_Application = app;
        }
        private double _tolerance = 0.001;//容差
        public double distancevalue = 0.0;
        GApplication m_Application;
        IGeometry Line1;
        IGeometry Line2;
        IFeature Feature1;
        IFeature Feature2;
        IPoint Pt;
        List<IFeature> _select = new List<IFeature>();//选择集
        List<IFeature> _temp = new List<IFeature>();//临时集合
        List<IFeature> _current = new List<IFeature>();//当前操作集合
        List<IFeature> _result = new List<IFeature>();//最终选择集合
        List<IFeature> _deletefeas = new List<IFeature>();//删除要素集合
        IFeatureLayer fl;
        IFeatureClass fc;


        private void Close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OktBtn_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void LinearDitchesFeatureDealForm_Load(object sender, EventArgs e)
        {
            #region 批量选择要素
            IFeature Pfe = (m_Application.MapControl.Map.FeatureSelection as IEnumFeature).Next();// 首次选中的要素
            while (Pfe == null)
            {
                return;
            }
            string layername = (Pfe.Class as IDataset).Name;
            var layer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == layername)).FirstOrDefault();
            fc = (layer as IFeatureLayer).FeatureClass;
            fl = layer as IFeatureLayer;
            _select.Add(Pfe);
            _current.Add(Pfe);
            _result = SelectFeature(_current, _select);
            string pwhere = "OBJECTID=";
            for (int i = 0; i < _result.Count; i++)
            {
                pwhere += _result[i].OID + " OR OBJECTID=";
            }
            pwhere = pwhere.Substring(0, pwhere.Length - 13);
            IFeatureSelection pFeatureSelection;
            pFeatureSelection = fl as IFeatureSelection;
            IQueryFilter pQueryFilter = new QueryFilter();
            pQueryFilter.WhereClause = pwhere;
            pFeatureSelection.SelectFeatures(pQueryFilter, esriSelectionResultEnum.esriSelectionResultNew, false);
            m_Application.ActiveView.Refresh();
            #endregion

        }

        private void btLeft_Click(object sender, EventArgs e)
        {
            m_Application.EngineEditor.EnableUndoRedo(true);
            bool left = true;
            double distancevalue = Convert.ToDouble(MoveDistanceText.Text);
            Double x = 0.0, y = 0.0, l = distancevalue;
            ISelection selectfeature = m_Application.MapControl.Map.FeatureSelection;
            IEnumFeature enumFeature = (IEnumFeature)selectfeature;
            enumFeature.Reset();
            IFeature feature = null;
            m_Application.EngineEditor.StartOperation();

            Dictionary<double, IPoint> PointDics = new Dictionary<double, IPoint>();//存放所有:节点的X值,节点
            while ((feature = enumFeature.Next()) != null)
            {
                IPolyline pl = feature.Shape as IPolyline;
                IPoint Fpoint = pl.FromPoint;
                PointDics[Fpoint.X] = Fpoint;
                IPoint Topoint = pl.ToPoint;
                PointDics[Topoint.X] = Topoint;
            }
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(enumFeature);
            double key = PointDics.Min(kv => kv.Key);//最小的点
            double key2 = PointDics.Max(kv => kv.Key);//最大的点

            IPolyline pline = new PolylineClass();//基线
            pline.FromPoint = PointDics[key];
            pline.ToPoint = PointDics[key2];
            double Angle = GetAngle(pline);//获取基线的切线角
            x = Math.Abs(l * Math.Sin(Angle));
            y = Math.Abs(l * Math.Cos(Angle));

            //  enumFeature = (IEnumFeature)selectfeature;
            enumFeature.Reset();
            while ((feature = enumFeature.Next()) != null)//遍历要素，进行移动
            {
                IGeometry pGeometry = feature.Shape;
                //分2种情况
                if (Angle >= 0 & Angle <= Math.PI / 2 || Angle >= Math.PI & Angle <= 1.5 * Math.PI)
                {
                    if (left == true)
                    {
                        (pGeometry as ITransform2D).Move(-x, y);
                    }
                }
                else
                {
                    if (left == true)
                    {
                        (pGeometry as ITransform2D).Move(-x, -y);
                    }
                }
                feature.Shape = pGeometry;
                feature.Store();
            }

            // System.Runtime.InteropServices.Marshal.ReleaseComObject(enumFeature);
            m_Application.ActiveView.Refresh();

            #region 合并移动之后的要素
            _current.Clear(); _select.Clear();
            enumFeature.Reset();
            while ((feature = enumFeature.Next()) != null)//遍历要素，进行移动
            {
                _current.Add(feature);
                _select.Add(feature);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(enumFeature);
            IFeature ResultFeature = MergeFeature(_current[0], _select);//合并后的选择要素


            //List<IFeature> OtherMergeFeas = new List<IFeature>();//与ResultFeature相交合并后的要素
            //ISpatialFilter sf = new SpatialFilterClass();
            //sf.Geometry = ResultFeature.Shape;
            //sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            //IFeatureCursor pcursor = fc.Search(sf, false);
            //IFeature f;
            //while ((f = pcursor.NextFeature()) != null)//遍历与ResultFeature相交的要素
            //{
            //    double a1 = GetAngle(f.Shape as IPolyline);
            //    IGeometry geo = f.Shape;
            //    IFeature Newfea;
            //    ISpatialFilter sf2 = new SpatialFilterClass();
            //    sf2.Geometry = ResultFeature.Shape;
            //    sf2.SpatialRel = esriSpatialRelEnum.esriSpatialRelTouches;
            //    IFeatureCursor pcursor2 = fc.Search(sf2, false);
            //    IFeature f2;
            //    while ((f2 = pcursor2.NextFeature()) != null)
            //    {
            //        double a2 = GetAngle(f2.Shape as IPolyline);
            //        if (Math.Abs(a1 - a2) <= Math.PI / 32)
            //        {
            //            ITopologicalOperator2 tpo = geo as ITopologicalOperator2;
            //            geo = tpo.Union(f2.Shape);
            //            Newfea = fc.CreateFeature();
            //            Newfea.Shape = geo;
            //            Newfea.Store();
            //            OtherMergeFeas.Add(Newfea );
            //            f.Delete();
            //        }
            //    }
            //    System.Runtime.InteropServices.Marshal.ReleaseComObject(pcursor2);
            //}
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(pcursor);
            #endregion
            m_Application.EngineEditor.StopOperation("移动");
        }

        private void btRight_Click(object sender, EventArgs e)
        {
            m_Application.EngineEditor.EnableUndoRedo(true);
            bool right = true;
            double distancevalue = Convert.ToDouble(MoveDistanceText.Text);
            Double x = 0.0, y = 0.0, l = distancevalue;
            ISelection selectfeature = m_Application.MapControl.Map.FeatureSelection;
            IEnumFeature enumFeature = (IEnumFeature)selectfeature;
            enumFeature.Reset();
            IFeature feature = null;
            m_Application.EngineEditor.StartOperation();

            Dictionary<double, IPoint> PointDics = new Dictionary<double, IPoint>();//存放所有:节点的X值,节点
            while ((feature = enumFeature.Next()) != null)
            {
                IPolyline pl = feature.Shape as IPolyline;
                IPoint Fpoint = pl.FromPoint;
                PointDics[Fpoint.X] = Fpoint;
                IPoint Topoint = pl.ToPoint;
                PointDics[Topoint.X] = Topoint;
            }
            // System.Runtime.InteropServices.Marshal.ReleaseComObject(enumFeature);
            double key = PointDics.Min(kv => kv.Key);//最小的点
            double key2 = PointDics.Max(kv => kv.Key);//最大的点

            IPolyline pline = new PolylineClass();//基线
            pline.FromPoint = PointDics[key];
            pline.ToPoint = PointDics[key2];
            double Angle = GetAngle(pline);//获取基线的切线角
            x = Math.Abs(l * Math.Sin(Angle));
            y = Math.Abs(l * Math.Cos(Angle));

            enumFeature = (IEnumFeature)selectfeature;
            enumFeature.Reset();
            while ((feature = enumFeature.Next()) != null)
            {
                IGeometry pGeometry = feature.Shape;
                //分2种情况
                if (Angle >= 0 & Angle <= Math.PI / 2 || Angle >= Math.PI & Angle <= 1.5 * Math.PI)
                {
                    if (right == true)
                    {
                        (pGeometry as ITransform2D).Move(x, -y);
                    }
                }
                else
                {
                    if (right == true)
                    {
                        (pGeometry as ITransform2D).Move(x, y);
                    }
                }
                feature.Shape = pGeometry;
                feature.Store();
            }
            m_Application.EngineEditor.StopOperation("移动");
            // System.Runtime.InteropServices.Marshal.ReleaseComObject(enumFeature);
            m_Application.ActiveView.Refresh();

        }

        List<IFeature> Verticalfeas = new List<IFeature>();//最终选择集合
        /// <summary>
        /// 批量选择相邻线段
        /// </summary>
        /// <param name="_Pcurrent">当前操作的要素集</param>
        /// <param name="_Pselect">当前选择的要素集</param>
        /// <returns></returns>
        private List<IFeature> SelectFeature(List<IFeature> _Pcurrent, List<IFeature> _Pselect)
        {
            for (int i = 0; i < _Pcurrent.Count; i++)//遍历pcurrent
            {
                IFeature fe = _Pcurrent[i];
                IPolyline pl = fe.Shape as IPolyline;
                double a1 = GetAngle(pl);//当前要素的切线角
                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = fe.Shape;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelTouches;
                IRelationalOperator2 ro = pl as IRelationalOperator2;
                IFeature f;
                IFeatureCursor pcursor = fc.Search(sf, false);
                List<IFeature> NoSelect = new List<IFeature>();//收集非选择集的要素

                while ((f = pcursor.NextFeature()) != null)
                {
                    bool equal = true;
                    for (int j = 0; j < _Pselect.Count; j++)//遍历选择集
                    {
                        if (f.OID == _Pselect[j].OID)//与选择集要素相同,不收集。
                        { equal = false; }
                    }
                    if (equal)
                        NoSelect.Add(f);
                }
                for (int m = 0; m < NoSelect.Count; m++)//遍历非选择集
                {
                    if (ro.Touches(NoSelect[m].Shape))//相接
                    {
                        double a2 = GetAngle(NoSelect[m].Shape as IPolyline);
                        if (Math.Abs(a1 - a2) <= Math.PI / 32)
                        {
                            _temp.Add(NoSelect[m]);
                        }
                        else
                        {
                            Verticalfeas.Add(NoSelect[m]);
                        }
                    }
                }
                NoSelect.Clear();
            }
            if (_temp.Count != 0)//终止条件
            {
                for (int k = 0; k < _temp.Count; k++)
                {
                    _select.Add(_temp[k]);
                }
                _current.Clear();
                for (int l = 0; l < _temp.Count; l++)
                {
                    _current.Add(_temp[l]);
                }
                _temp.Clear();
                SelectFeature(_current, _select);
            }
            return _select;
        }

        /// <summary>
        /// 合并移动后的要素
        /// </summary>
        /// <param name="currentfea">传入的操作要素</param>
        /// <param name="_Pselect">初始选择的要素集</param>
        /// <returns></returns>
        private IFeature MergeFeature(IFeature currentfea, List<IFeature> _Pselect)//currentfea传入的时候需要删掉
        {
            IFeature Resultfea = null;
            IFeature fe = currentfea;
            currentfea.Delete();
            IGeometry ResultGeo = fe.Shape;
            IPolyline pl = fe.Shape as IPolyline;
            double a1 = GetAngle(pl);//切线角
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = fe.Shape;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelTouches;
            IRelationalOperator2 ro = pl as IRelationalOperator2;
            IFeature f;
            IFeatureCursor pcursor = fc.Search(sf, false);
            while ((f = pcursor.NextFeature()) != null)
            {
                if (ro.Touches(f.Shape) & fe.OID != f.OID)//相接
                {
                    double a2 = GetAngle(f.Shape as IPolyline);
                    if (Math.Abs(a1 - a2) <= Math.PI / 32)
                    {
                        ITopologicalOperator2 tpo = ResultGeo as ITopologicalOperator2;
                        ResultGeo = tpo.Union(f.Shape);
                        Resultfea = fc.CreateFeature();
                        Resultfea.Shape = ResultGeo;
                        Resultfea.Store();
                        _Pselect.Remove(f);
                        f.Delete();
                    }
                }
            }
            if (_Pselect.Count != 1)//终止条件
            {
                MergeFeature(Resultfea, _Pselect);
            }
            return Resultfea;
        }

        /// <summary>
        /// 获取polyline切线方向的角度（弧度）
        /// </summary>
        /// <param name="pPolyline">线</param>
        /// <returns></returns>
        private double GetAngle(IPolyline pPolyline)
        {
            ILine pTangentLine = new Line();
            pPolyline.QueryTangent(esriSegmentExtension.esriNoExtension, 0.5, true, pPolyline.Length, pTangentLine);
            Double radian = pTangentLine.Angle;
            Double angle = radian * 180 / Math.PI;
            // 如果要设置正角度执行以下方法
            while (angle < 0)
            {
                angle = angle + 360;
            }
            radian = angle * Math.PI / 180;
            // 返回弧度
            return radian;
        }

        /// <summary>
        /// 线打断的方法
        /// </summary>
        private void breakline(IFeature pFeature)//传入选中要素
        {
            #region
            IEnumFeature pFeatures = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;//遍历与选中要素intersect的要素

            IFeature oneFea;
            while ((oneFea = pFeatures.Next()) != null)
            {
                if (Line1 == null)
                {
                    Feature1 = oneFea;
                    Line1 = oneFea.Shape as IGeometry;
                }
                else
                {
                    Feature2 = oneFea;
                    Line2 = oneFea.Shape as IGeometry;
                }
            }

            //IRelationalOperator 
            ITopologicalOperator pTopo = Line1 as ITopologicalOperator;//line1为传进来的移动直线
            //遍历所有与line1相交的直线，收集交点
            IGeometry onePoint = pTopo.Intersect(Line2, esriGeometryDimension.esriGeometry0Dimension);

            if (!onePoint.IsEmpty)
            {
                IPointCollection Pc = onePoint as IPointCollection;
                Pt = Pc.get_Point(0);
            }
            ISet pFeatureSet1 = (Feature1 as IFeatureEdit).Split(Pt);
            ISet pFeatureSet2 = (Feature2 as IFeatureEdit).Split(Pt);
            #endregion
        }
    }
}
