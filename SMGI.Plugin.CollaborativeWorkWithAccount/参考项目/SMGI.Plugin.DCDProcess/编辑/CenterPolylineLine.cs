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

namespace SMGI.Plugin.DCDProcess
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
                MessageBox.Show("请选择要素层！");
                return;
            }
            if (((IFeatureLayer)pLayer).FeatureClass.ShapeType != esriGeometryType.esriGeometryPolyline)
            {
                MessageBox.Show("请选择线要素层！");
                return;
            }
            var map = view.FocusMap;
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
                    var cl = clh.Create(fe.Shape as IPolygon).Line;                   
                    var gc = (IGeometryCollection)cl;
                    for (var i = 0; i < gc.GeometryCount; i++)
                    {
                        var pl = new PolylineClass();
                        pl.AddGeometry(gc.Geometry[i]);
                        var fci = lineFc.Insert(true);
                        var fb = lineFc.CreateFeatureBuffer();
                        fb.Shape = pl;
                        bool bCoola = true;
                        #region 协同
                        if (fb.Fields.FindField(cmdUpdateRecord.CollabGUID) == -1)
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
                            fb.set_Value(fb.Fields.FindField(cmdUpdateRecord.CollabVERSION), cmdUpdateRecord.NewState);
                            fb.set_Value(fb.Fields.FindField(cmdUpdateRecord.CollabGUID), Guid.NewGuid().ToString());
                            fb.set_Value(fb.Fields.FindField(cmdUpdateRecord.CollabOPUSER), System.Environment.MachineName);
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
                        fci.InsertFeature(fb);
                        fci.Flush();

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
