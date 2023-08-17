using System;
using System.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Collections.Generic;

namespace SMGI.Plugin.DCDProcess
{
    public class CmdMultiToSingle : SMGICommand
    {
        public CmdMultiToSingle()
        {
            m_caption = "打散要素";
            m_toolTip = "打散选中要素";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing &&
                       m_Application.Workspace.Map.SelectionCount > 0;
            }
        }
        
        public override void OnClick()
        {
            var map = m_Application.Workspace.Map;
            if (map.SelectionCount == 0) return;

            var fes = (IEnumFeature)map.FeatureSelection;
            fes.Reset();
            IFeature selFeature = null;
            m_Application.EngineEditor.StartOperation();
            while ((selFeature = fes.Next()) != null)
            {
                var layer = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l is IGeoFeatureLayer && l.Visible && (l as IGeoFeatureLayer).FeatureClass != null && (l as IGeoFeatureLayer).FeatureClass.AliasName == selFeature.Class.AliasName;
                })).ToArray().FirstOrDefault();
                if (layer == null) continue;

                int guidIndex = selFeature.Fields.FindField(cmdUpdateRecord.CollabGUID);
                int verIndex = selFeature.Fields.FindField(cmdUpdateRecord.CollabVERSION);
                int userIndex = selFeature.Fields.FindField(cmdUpdateRecord.CollabOPUSER);

                string collGUID = "";
                if (guidIndex != -1)
                {
                    collGUID = selFeature.get_Value(guidIndex).ToString();
                }
                int smgiver = -999;
                if (verIndex != -1)
                {
                    int.TryParse(selFeature.get_Value(verIndex).ToString(), out smgiver);
                    selFeature.set_Value(verIndex, cmdUpdateRecord.NewState);//直接删除的标志
                }

                try
                {
                    int maxGeoIndex = 0;
                    double maxVal = 0;

                    var fc = ((IFeatureLayer)layer).FeatureClass;
                    if (selFeature.Shape.GeometryType == esriGeometryType.esriGeometryPolygon) //面打散
                    {
                        var po = (IPolygon4)selFeature.ShapeCopy;
                        var gc = (IGeometryCollection)po.ConnectedComponentBag;
                        if (gc.GeometryCount <= 1) continue;

                        //获取最大几何体的索引号
                        for (var i = 0; i < gc.GeometryCount; i++)
                        {
                            if ((gc.Geometry[i] as IArea).Area > maxVal)
                            {
                                maxVal = (gc.Geometry[i] as IArea).Area;
                                maxGeoIndex = i;
                            }
                        }

                        //打散要素
                        var fci = fc.Insert(true);
                        for (var i = 0; i < gc.GeometryCount; i++)
                        {
                            var fb = fc.CreateFeatureBuffer();

                            //几何赋值
                            fb.Shape = gc.Geometry[i];

                            //属性赋值
                            for (int j = 0; j < fb.Fields.FieldCount; j++)
                            {
                                IField pfield = fb.Fields.get_Field(j);
                                if (pfield.Type == esriFieldType.esriFieldTypeGeometry || pfield.Type == esriFieldType.esriFieldTypeOID)
                                {
                                    continue;
                                }

                                if (pfield.Name.ToUpper() == "SHAPE_LENGTH" || pfield.Name.ToUpper() == "SHAPE_AREA")
                                {
                                    continue;
                                }

                                int index = selFeature.Fields.FindField(pfield.Name);
                                if (index != -1 && pfield.Editable)
                                {
                                    fb.set_Value(j, selFeature.get_Value(index));
                                }

                            }

                            if (cmdUpdateRecord.EnableUpdate && guidIndex != -1)
                            {
                                if (maxGeoIndex == i)
                                {
                                    if (smgiver >= 0 || smgiver == cmdUpdateRecord.EditState)
                                    {
                                        fb.set_Value(verIndex, cmdUpdateRecord.EditState);
                                    }

                                    fb.set_Value(guidIndex, collGUID);
                                }
                                else
                                {
                                    fb.set_Value(guidIndex, Guid.NewGuid().ToString());
                                    fb.set_Value(verIndex, cmdUpdateRecord.NewState);
                                }

                                fb.set_Value(userIndex, System.Environment.MachineName);
                            }

                            int oid = int.Parse(fci.InsertFeature(fb).ToString());
                        }
                        fci.Flush();
                    }
                    else if (selFeature.Shape.GeometryType == esriGeometryType.esriGeometryPolyline) //线打散
                    {
                        var gc = (IGeometryCollection)selFeature.ShapeCopy;
                        if (gc.GeometryCount <= 1) continue;

                        //获取最大几何体的索引号
                        for (var i = 0; i < gc.GeometryCount; i++)
                        {
                            IPointCollection pc = new PolylineClass();
                            pc.AddPointCollection((IPointCollection)gc.Geometry[i]);

                            if ((pc as IPolyline).Length > maxVal)
                            {
                                maxVal = (pc as IPolyline).Length;
                                maxGeoIndex = i;
                            }
                        }

                        //打散要素
                        var fci = fc.Insert(true);
                        for (var i = 0; i < gc.GeometryCount; i++)
                        {
                            var fb = fc.CreateFeatureBuffer();
                            fb = (IFeatureBuffer)selFeature;
                            IPointCollection pc = new PolylineClass();
                            pc.AddPointCollection((IPointCollection)gc.Geometry[i]);
                            fb.Shape = pc as IPolyline;
                            if (cmdUpdateRecord.EnableUpdate && guidIndex != -1)
                            {
                                if (maxGeoIndex == i)
                                {
                                    if (smgiver >= 0 || smgiver == cmdUpdateRecord.EditState)
                                    {
                                        fb.set_Value(verIndex, cmdUpdateRecord.EditState);
                                    }

                                    fb.set_Value(guidIndex, collGUID);
                                }
                                else
                                {
                                    fb.set_Value(guidIndex, Guid.NewGuid().ToString());
                                    fb.set_Value(verIndex, cmdUpdateRecord.NewState);
                                }

                                fb.set_Value(userIndex, System.Environment.MachineName);
                            }

                            int oid = int.Parse(fci.InsertFeature(fb).ToString());
                        }
                        fci.Flush();
                    }

                    //删除原要素
                    if (verIndex != -1)
                    {
                        selFeature.set_Value(verIndex, cmdUpdateRecord.NewState);//直接删除的标志
                    }
                    selFeature.Delete();

                    m_Application.EngineEditor.StopOperation("打散选中要素");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    m_Application.EngineEditor.AbortOperation();

                    MessageBox.Show(ex.Message);
                }

            }

            
            m_Application.ActiveView.Refresh();
        }
    }
}