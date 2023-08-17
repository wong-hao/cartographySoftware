using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using System.Diagnostics;
using SMGI.Common.Algrithm;
using DevExpress.XtraBars.Ribbon;

namespace SMGI.Plugin.DCDProcess
{
    public class RoadGrade2Set : SMGITool// SMGI.Common.SMGITool
    {
        IFeatureCursor cur;
        int level;
        int index = -1;
        RoadRankSetForm rrs;

        public RoadGrade2Set()
        {
            m_caption = "等级调整";
            m_toolTip = "单击“等级调整”按钮，弹出窗体，依次选择操作图层（LRDL、HYDL、AGNP）、等级，然后框选指定要素，完成等级值调整";
            m_category = " ";

        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            rrs = new RoadRankSetForm(level, m_Application);
            var sz = (m_Application.MainForm as RibbonForm).Size;
            rrs.Location = new System.Drawing.Point(sz.Width - rrs.Size.Width - 30, rrs.Size.Height - 20);
            rrs.Show(m_Application.MainForm as IWin32Window);//窗体不隐藏
        }

        ///// <summary>
        ///// 处理键盘指令
        ///// </summary>
        ///// <param name="keyCode">键盘标识码</param>
        ///// <param name="Shift"></param>
        //public override void OnKeyUp(int keyCode, int Shift)
        //{
        //    int grade2old = int.Parse(fe.get_Value(index).ToString());
        //    level = grade2old;
        //    switch (keyCode)
        //    {
        //        case 87://(int)System.Windows.Forms.Keys.Up:
        //            if (level < 12) { level = level + 1; }
        //            //sBar.set_Message(0, "当前等级..." + level);
        //            break;
        //        case 83://(int)System.Windows.Forms.Keys.Down:
        //            if (level > 0) { level = level - 1; }
        //            break;
        //    }
        //    fe.set_Value(index, level);
        //    fe.Store();
        //    m_Application.ActiveView.Refresh();
        //    level = 5;
        //    // m_Application.SetStusBar(string.Format("当前等级：【{0}】", level));
        //}

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1 && button != 2) { return; }
            var env = m_Application.MapControl.TrackRectangle();
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = env;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            cur = rrs.layerclass.Search(sf, false);//查询要素
            IFeature fea = null;
            index = rrs.idx;
            IFeatureSelection pFeatureSelection = rrs.layer as IFeatureSelection;
            IQueryFilter pQueryFilter = new QueryFilter();
            string pwhere = "OBJECTID=";
            while ((fea = cur.NextFeature()) != null)
            {
                level = rrs.level;
                fea.set_Value(index, level);
                fea.Store();
                pwhere += fea.OID + " OR OBJECTID=";
            }
            Marshal.ReleaseComObject(cur);
            if (pwhere.Length<13)
            {
                MessageBox.Show(string.Format("请选择{0}图层要素！ ", rrs.str));
                return;
            }
            pwhere = pwhere.Substring(0, pwhere.Length - 13);
            pQueryFilter.WhereClause = pwhere;
            pFeatureSelection.SelectFeatures(pQueryFilter, esriSelectionResultEnum.esriSelectionResultNew, false);
            m_Application.ActiveView.Refresh(); 
        }
        private IFeature GetFeature(IPoint position)
        {
            IFeature result = null;
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l => { return (l is IGeoFeatureLayer); })).ToArray();
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = position;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            foreach (var lyr in lyrs)
            {
                if ((lyr as IFeatureLayer).FeatureClass.FeatureCount(sf) > 0)
                {
                    var curcor = (lyr as IFeatureLayer).Search(sf as IQueryFilter, false);
                    IFeature f = null;
                    while ((f = curcor.NextFeature()) != null)
                    {
                        result = f;
                        break;
                    }
                    Marshal.ReleaseComObject(curcor);
                }

            }
            return result;
        }
    }
}
