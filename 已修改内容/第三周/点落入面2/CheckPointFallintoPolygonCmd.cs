using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesFile;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using System.IO;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public class CheckPointFallintoPolygonCmd : SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null;
            }
        }

        public override void OnClick()
        {
            var frm = new CheckPointFallintoPolygonForm(m_Application);
            frm.StartPosition = FormStartPosition.CenterParent;

            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string outputFileName = frm.ResultOutputFilePath + string.Format("\\点落入面检查_{0}.shp", frm.PointFeatureClass.AliasName);

            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                Dictionary<IFeatureClass, string> areaFC2Filter = new Dictionary<IFeatureClass, string>();
                areaFC2Filter.Add(frm.AreaFeatureClass, frm.AreaFilterString);

                err = DoCheck(outputFileName, frm.PointFeatureClass, frm.PointFilterString, areaFC2Filter, wo);
            }

            if (err == "")
            {
                if (File.Exists(outputFileName))
                {
                    if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName);
                        CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                    }
                }
                else
                {
                    MessageBox.Show("检查完毕，没有发现非法落入面的点要素！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }

        }

        public static string DoCheck(string resultSHPFileName, IFeatureClass pointFC, string pointFilter, Dictionary<IFeatureClass, string> areaFC2Filter, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                IQueryFilter pointQF = new QueryFilterClass();
                pointQF.WhereClause = pointFilter;
                if (pointFC.FindField(ServerDataInitializeCommand.CollabVERSION) != -1)
                {
                    if (pointQF.WhereClause != "")
                        pointQF.WhereClause = string.Format("({0}) and ", pointQF.WhereClause);
                    pointQF.WhereClause += string.Format("({0} <> {1} or {2} is null)", ServerDataInitializeCommand.CollabVERSION, cmdUpdateRecord.DeleteState, ServerDataInitializeCommand.CollabVERSION);
                }

                Dictionary<IFeatureClass, IQueryFilter> areaFC2QF = new Dictionary<IFeatureClass, IQueryFilter>();
                foreach (var kv in areaFC2Filter)
                {
                    IFeatureClass areaFC = kv.Key;
                    if (areaFC.ShapeType != esriGeometryType.esriGeometryPolygon)
                        continue;

                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = kv.Value;
                    if (areaFC.FindField(ServerDataInitializeCommand.CollabVERSION) != -1)
                    {
                        if (qf.WhereClause != "")
                            qf.WhereClause = string.Format("({0}) and ", qf.WhereClause);
                        qf.WhereClause += string.Format("({0} <> {1} or {2} is null)", ServerDataInitializeCommand.CollabVERSION, cmdUpdateRecord.DeleteState, ServerDataInitializeCommand.CollabVERSION);
                    }

                    areaFC2QF.Add(areaFC, qf);
                }

                ShapeFileWriter resultFile = null;
                Dictionary<int, Dictionary<int, string>> errList = CheckPointFallintoPolygon(pointFC, pointQF, areaFC2QF, wo);
                if (errList.Count > 0)
                {
                    if (resultFile == null)
                    {
                        resultFile = new ShapeFileWriter();
                        Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                        fieldName2Len.Add("图层名", 20);
                        fieldName2Len.Add("要素编号", 32);
                        fieldName2Len.Add("说明", 64);
                        fieldName2Len.Add("检查项", 32);
                        resultFile.createErrorResutSHPFile(resultSHPFileName, (pointFC as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPoint, fieldName2Len);
                    }

                    foreach (var kv in errList)
                    {
                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("图层名", pointFC.AliasName);
                        fieldName2FieldValue.Add("要素编号", kv.Key.ToString());
                        if (kv.Value.Count > 1)
                        {
                            fieldName2FieldValue.Add("说明", string.Format("落入的非法面【{0}】个：<{1},{2}>......", kv.Value.Count, kv.Value.First().Value, kv.Value.First().Key));
                        }
                        else
                        {
                            fieldName2FieldValue.Add("说明", string.Format("落入的非法面【{0}】个：<{1},{2}>", kv.Value.Count, kv.Value.First().Value, kv.Value.First().Key));
                        }
                        fieldName2FieldValue.Add("检查项", "点落入面检查");

                        IFeature fe = pointFC.GetFeature(kv.Key);
                        resultFile.addErrorGeometry(fe.ShapeCopy, fieldName2FieldValue);
                    }
                }

                if (resultFile != null)
                    resultFile.saveErrorResutSHPFile();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                err = ex.Message;
            }

            return err;
        }

        public static Dictionary<int, Dictionary<int, string>> CheckPointFallintoPolygon(IFeatureClass pointFC, IQueryFilter pointQF, Dictionary<IFeatureClass, IQueryFilter> areaFC2QF, WaitOperation wo = null)
        {
            Dictionary<int, Dictionary<int, string>> result = new Dictionary<int, Dictionary<int, string>>();

            IFeatureCursor feCursor = pointFC.Search(pointQF, true);
            IFeature fe;
            while ((fe = feCursor.NextFeature()) != null)
            {
                if (fe.Shape == null || fe.Shape.IsEmpty)
                    continue;

                if (wo != null)
                    wo.SetText(string.Format("正在检查要素类【{0}】中的要素【{1}】......", pointFC.AliasName, fe.OID));

                foreach (var kv in areaFC2QF)
                {
                    ISpatialFilter sf = new SpatialFilter();
                    sf.Geometry = fe.Shape;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelWithin;
                    sf.WhereClause = kv.Value.WhereClause;

                    IFeatureCursor areaFeCursor = kv.Key.Search(sf, true);
                    IFeature areaFe = null;
                    while ((areaFe = areaFeCursor.NextFeature()) != null)
                    {
                        if (result.ContainsKey(fe.OID))
                        {
                            result[fe.OID].Add(areaFe.OID, kv.Key.AliasName);
                        }
                        else
                        {
                            Dictionary<int, string> oid2fcname = new Dictionary<int, string>();
                            oid2fcname.Add(areaFe.OID, kv.Key.AliasName);

                            result.Add(fe.OID, oid2fcname);
                        }
                    }
                    Marshal.ReleaseComObject(areaFeCursor);
                }

            }
            Marshal.ReleaseComObject(feCursor);

            return result;
        }
    }
}
