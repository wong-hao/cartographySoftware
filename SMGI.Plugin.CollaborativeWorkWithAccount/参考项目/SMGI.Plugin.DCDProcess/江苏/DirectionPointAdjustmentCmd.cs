using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using System.Data;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 有向点方向调整
    /// 详细参见需求文档《收费站ANGLE赋值20180408.docx》
    /// </summary>
    public class DirectionPointAdjustmentCmd : SMGI.Common.SMGICommand
    {
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
            string dbPath = m_Application.Template.Root + @"\规则配置.mdb";
            string tableName = "有向点规则";
            DataTable ruleDataTable = DCDHelper.ReadToDataTable(dbPath, tableName);
            if (ruleDataTable == null)
            {
                return;
            }

            int num = 0;
            try
            {
                m_Application.EngineEditor.StartOperation();


                using (WaitOperation wo = GApplication.Application.SetBusy())
                {
                    IQueryFilter qf = new QueryFilterClass();
                    IFeatureClass fc = null;//有向点要素类
                    IFeatureCursor feCursor = null;
                    IFeature fe = null;

                    IFeatureClass conFC = null;//关联要素类


                    for (int i = 0; i < ruleDataTable.Rows.Count; i++)
                    {
                        string lyrName = ruleDataTable.Rows[i]["目标图层"].ToString().Trim().ToUpper();
                        string filter = ruleDataTable.Rows[i]["目标要素条件"].ToString().Trim();
                        string angleFN = ruleDataTable.Rows[i]["字段名"].ToString().Trim();
                        double angle = 0;
                        double.TryParse(ruleDataTable.Rows[i]["角度值"].ToString().Trim(), out angle);

                        string conLyrName = ruleDataTable.Rows[i]["关联图层"].ToString().Trim().ToUpper();
                        string conFilter = ruleDataTable.Rows[i]["关联要素条件"].ToString().Trim();
                        

                        var feLayer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
                            ((l as IGeoFeatureLayer).Name.Trim().ToUpper() == lyrName)).FirstOrDefault() as IFeatureLayer;
                        if (feLayer == null)
                        {
                            MessageBox.Show(string.Format("当前工作空间中没有找到图层【{0}】!", lyrName));
                            return;
                        }
                        fc = feLayer.FeatureClass;

                        angleFN = m_Application.TemplateManager.getFieldAliasName(angleFN, fc.AliasName);
                        int angleIndex = fc.FindField(angleFN);
                        if (angleIndex == -1)
                        {
                            MessageBox.Show(string.Format("图层【{0}】中没有找到字段【{1}】!", lyrName, angleFN));
                            return;
                        }

                        feLayer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
                            ((l as IGeoFeatureLayer).Name.Trim().ToUpper() == conLyrName)).FirstOrDefault() as IFeatureLayer;
                        if (feLayer == null)
                        {
                            MessageBox.Show(string.Format("当前工作空间中没有找到图层【{0}】!", conLyrName));
                            return;
                        }
                        conFC = feLayer.FeatureClass;


                        qf.WhereClause = filter;
                        feCursor = fc.Search(qf, false);
                        while ((fe = feCursor.NextFeature()) != null)
                        {
                            wo.SetText("正在处理要素【" + fe.OID + "】.......");

                            IRelationalOperator ro = fe.Shape as IRelationalOperator;

                            SpatialFilterClass sf = new SpatialFilterClass();
                            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                            sf.Geometry = fe.Shape;
                            sf.WhereClause = conFilter;
                            IFeatureCursor conFeCursor = conFC.Search(sf, true);
                            IFeature conFe = null;
                            if ((conFe = conFeCursor.NextFeature()) != null)
                            {
                                ISegmentCollection segs = conFe.Shape as ISegmentCollection;
                                if (segs == null)
                                    continue;

                                ILine lineDir = null;

                                for (int s = 0; s < segs.SegmentCount; s++)
                                {
                                    ILine line = segs.get_Segment(s) as ILine;
                                    if (line == null || line.IsEmpty)
                                        continue;

                                    ISegmentCollection gc = new PolylineClass();
                                    gc.AddSegment(segs.get_Segment(s));
                                    if (!ro.Disjoint(gc as IGeometry))
                                    {
                                        lineDir = line;
                                        break;
                                    }
                                }

                                if (lineDir != null)
                                {
                                    fe.set_Value(angleIndex, DCDHelper.GetAngle(lineDir, angle));
                                    fe.Store();

                                    num++;
                                }
                                
                            }
                            Marshal.ReleaseComObject(conFeCursor);
                        }
                        Marshal.ReleaseComObject(feCursor);
                    }
                }

                m_Application.EngineEditor.StopOperation("有向点方向调整");

                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);

                MessageBox.Show(string.Format("更改了{0}条要素", num));
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
    }
}
