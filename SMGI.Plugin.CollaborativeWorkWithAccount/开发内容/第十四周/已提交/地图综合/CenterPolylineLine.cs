using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using SMGI.Common;
using SMGI.Plugin;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Plugin.GeneralEdit;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public   class CenterPolylineLine : SMGICommand
    {
        public CenterPolylineLine()
        {
            m_caption = "生成中心线";
            m_toolTip = "生成中心线";
            m_category = "生成中心线";
        }

        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing && 
                       m_Application.Workspace != null;
                     
            }
        }
        public override void OnClick()
        {
            var view = m_Application.ActiveView;
            ILayer pLayer = m_Application.TOCSelectItem.Layer;
            if (!(pLayer is FeatureLayer))
            {
                MessageBox.Show("请在左侧图层列表选择要素层，中心线将写到该图层！");
                return;
            }
            if (((IFeatureLayer)pLayer).FeatureClass.ShapeType != esriGeometryType.esriGeometryPolyline)
            {
                MessageBox.Show("请选择线要素层！");
                return;
            }
            

            var map = view.FocusMap;
            IEnumFeature selection = (IEnumFeature)map.FeatureSelection;
            IFeature fea;
            bool polygonSelected = false;
            while ((fea = selection.Next()) != null)
            {
                if (fea.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                {
                    polygonSelected = true;
                    break;
                }
            }
            if(!polygonSelected)
            {
                MessageBox.Show("未选择polygon类型的要素，请在图面上选择polygon类型要素重新点击“生成中心线”按钮！");
                return;
            }
            var wo = GApplication.Application.SetBusy();
            m_Application.EngineEditor.StartOperation();
            try
            {
                var lineFc = ((IFeatureLayer)pLayer).FeatureClass;
                var clh = new CenterLineHelper();
                var enFe = (IEnumFeature)map.FeatureSelection;
                enFe.Reset();
                IFeature fe;
                while ((fe = enFe.Next()) != null)
                {
                    if (fe.Shape.GeometryType != esriGeometryType.esriGeometryPolygon)
                        continue;
                    var cl = clh.Create(fe.Shape as IPolygon).Line;                   
                    var gc = (IGeometryCollection)cl;
                    for (var i = 0; i < gc.GeometryCount; i++)
                    {
                        var pl = new PolylineClass();
                        pl.AddGeometry(gc.Geometry[i]);
                        var fci = lineFc.CreateFeature(); // CreateFeature is used to create a new feature
                        var fb = fci as IFeature;
                        fb.Shape = pl;
                        bool bCoola = true;
                        #region 协同
                        if (fb.Fields.FindField(ServerDataInitializeCommand.CollabGUID) == -1)
                        {
                            bCoola = false;
                        }
                        int gb = 0;
                        string name = "";
                        if (fe.Fields.FindField("GB") != -1)
                        {
                            int.TryParse(fe.get_Value(fe.Fields.FindField("GB")).ToSafeString(), out gb);
                        }
                        if (fe.Fields.FindField("NAME") != -1)
                        {
                            name = fe.get_Value(fe.Fields.FindField("NAME")).ToSafeString();
                        }
                       
                        if (bCoola)
                        {
                            if (fb.Fields.FindField("GB") != -1 && lineFc.AliasName == "HYDL")
                            {
                                fb.set_Value(fb.Fields.FindField("GB"), 210400);
                            }
                            if(fb.Fields.FindField("NAME") != -1)
                                fb.set_Value(fb.Fields.FindField("NAME"), name);
                            if(fb.Fields.FindField("HGB") != -1)
                                fb.set_Value(fb.Fields.FindField("HGB"), gb);  
                        }
                       
                        
                        #endregion
                        fci.Store(); // Store the new feature

                    }
                }
                m_Application.MapControl.Map.ClearSelection();
                view.Refresh();
                wo.Dispose();
                m_Application.EngineEditor.StopOperation("生成中心线");
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
                System.Diagnostics.Trace.WriteLine(e.Source);
                System.Diagnostics.Trace.WriteLine(e.StackTrace);

                MessageBox.Show(e.Message, "错误");
                wo.Dispose();
                m_Application.EngineEditor.AbortOperation();
            }
        }
    }
}
