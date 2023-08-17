using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 将临时数据中的选中要素复制到Workspace中的某个要素类（collguid保持不变）
    /// </summary>
    public class PasteCollaDataCmd : SMGI.Common.SMGICommand
    {
        public PasteCollaDataCmd()
        {
            m_caption = "临时数据导入";
            m_toolTip = "复制临时数据到目标图层";
        }

        public override bool Enabled
        {
            get
            {
                if (m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    if (0 == m_Application.MapControl.Map.SelectionCount)
                        return false;

                    return true;
                }

                return false;
            }
        }

        public override void OnClick()
        {
            IFeatureLayer targetFeatureLayer = null;
            IFeatureClass targetFC = null;
            #region 目标图层
            IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;
            IEngineEditLayers engineEditLayer = editsketch as IEngineEditLayers;
            targetFeatureLayer = engineEditLayer.TargetLayer;
            targetFC = targetFeatureLayer.FeatureClass;
            #endregion

            List<IFeature> selTempFeas = new List<IFeature>();
            List<string> guidList = new List<string>();

            string tempErr = "";//被选中要素不是临时数据的错误信息集合
            string typeErr = "";//被选中要素的几何类型与目标图层的几何类型不一致的错误信息集合

            #region 收集所有被选择的要素（仅支持与目标图层要素类型相同的临时数据导入）
            IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
            mapEnumFeature.Reset();
            IFeature feature = null;
            while ((feature = mapEnumFeature.Next()) != null)
            {

                if ((feature.Class as IDataset).Workspace.PathName == m_Application.Workspace.EsriWorkspace.PathName)//临时数据
                {
                    if (tempErr == "")
                    {
                        tempErr = string.Format("以下要素不是临时数据: ({0}-{1})", feature.Class.AliasName, feature.OID);
                    }
                    else
                    {
                        tempErr += string.Format(" ,({0}-{1})", feature.Class.AliasName, feature.OID);
                    }

                    continue;

                }

                if (targetFeatureLayer.FeatureClass.ShapeType != feature.Shape.GeometryType)
                {
                    if (typeErr == "")
                    {
                        typeErr = string.Format("以下要素的几何类型与目标图层的几何类型不一致: ({0}-{1})", feature.Class.AliasName, feature.OID);
                    }
                    else
                    {
                        typeErr += string.Format(" ,({0}-{1})", feature.Class.AliasName, feature.OID);
                    }

                    continue;
                }

                //符合要求的选中要素
                selTempFeas.Add(feature);

                int guidIndex = feature.Fields.FindField(cmdUpdateRecord.CollabGUID);
                if (guidIndex != -1)
                {
                    string guid = feature.get_Value(guidIndex).ToString();
                    if (guid != "")
                    {
                        guidList.Add(guid);
                    }
                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);

            if (tempErr != "" || typeErr != "")
            {
                string info = "导入选中要素失败：";
                if (tempErr != "")
                {
                    info += "\n" + tempErr;
                }
                if (typeErr != "")
                {
                    info += "\n" + typeErr;
                }

                MessageBox.Show(info);
                return;
            }
            #endregion

            #region 检测目标图层中是否已存在该GUID
            if (guidList.Count > 0)
            {
                int guidIndex = targetFC.FindField(cmdUpdateRecord.CollabGUID);
                if (guidIndex != -1)
                {
                    string guidSet = "";
                    foreach (var guid in guidList)
                    {
                        if (guidSet != "")
                            guidSet += string.Format(",'{0}'", guid);
                        else
                            guidSet = string.Format("'{0}'", guid);
                    }
                    IQueryFilter tmpQF = new QueryFilterClass() { WhereClause = string.Format("{0} in ({1})", cmdUpdateRecord.CollabGUID, guidSet) };

                    int repeatGUIDCount = targetFC.FeatureCount(tmpQF);
                    if (repeatGUIDCount > 0)
                    {
                        string repeatGUIDErr = "";

                        IFeatureCursor pRepeatGUIDCursor = targetFC.Search(tmpQF, true);
                        IFeature fe = null;
                        while ((fe = pRepeatGUIDCursor.NextFeature()) != null)
                        {
                            string guid = fe.get_Value(guidIndex).ToString();
                            if (repeatGUIDErr == "")
                            {
                                repeatGUIDErr = string.Format("导入选中要素失败,以下guid已存在于目标图层：\n({0})", guid);
                            }
                            else
                            {
                                repeatGUIDErr += string.Format(" ,({0})", guid);
                            }

                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pRepeatGUIDCursor);

                        MessageBox.Show(repeatGUIDErr);
                        return;
                        
                    }

                    
                }
            }
            #endregion

            #region 粘贴
            m_Application.EngineEditor.StartOperation();

            try
            {
                var pCursor = targetFC.Insert(true);

                List<int> newFeOIDList = new List<int>();
                foreach (var fe in selTempFeas)
                {
                    var fb = targetFC.CreateFeatureBuffer();

                    //几何赋值
                    IGeometry shape = fe.ShapeCopy;
                    if ((targetFC as IGeoDataset).SpatialReference.Name != fe.Shape.SpatialReference.Name)
                    {
                        shape.Project((targetFC as IGeoDataset).SpatialReference);
                    }
                    fb.Shape = shape;

                    //属性赋值
                    for (int i = 0; i < fb.Fields.FieldCount; i++)
                    {
                        IField pfield = fb.Fields.get_Field(i);
                        if (pfield.Type == esriFieldType.esriFieldTypeGeometry || pfield.Type == esriFieldType.esriFieldTypeOID)
                        {
                            continue;
                        }

                        if (pfield.Name == "SHAPE_Length" || pfield.Name == "SHAPE_Area")
                        {
                            continue;
                        }

                        int index = fe.Fields.FindField(pfield.Name);
                        if (index != -1 && pfield.Editable)
                        {
                            fb.set_Value(i, fe.get_Value(index));
                        }

                    }

                    if (cmdUpdateRecord.EnableUpdate)//协同
                    {
                        if (fb.Fields.FindField(cmdUpdateRecord.CollabVERSION) != -1)
                        {
                            fb.set_Value(fb.Fields.FindField(cmdUpdateRecord.CollabVERSION), cmdUpdateRecord.NewState);
                        }
                        if (fb.Fields.FindField(cmdUpdateRecord.CollabOPUSER) != -1)
                        {
                            fb.set_Value(fb.Fields.FindField(cmdUpdateRecord.CollabOPUSER), System.Environment.MachineName);
                        }
                    }

                    //新增要素
                    int oid = int.Parse(pCursor.InsertFeature(fb).ToString());
                    newFeOIDList.Add(oid);
                }
                pCursor.Flush();

                System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);

                m_Application.EngineEditor.StopOperation("临时数据导入");

                //清理原始选中要素
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                m_Application.MapControl.Map.ClearSelection();

                //选中新导入的要素
                foreach (var feOID in newFeOIDList)
                {
                    IFeature newFe = targetFC.GetFeature(feOID);

                    m_Application.MapControl.Map.SelectFeature(targetFeatureLayer, newFe);
                }
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                m_Application.EngineEditor.AbortOperation();
            }

            #endregion

            
        }
    }
}
