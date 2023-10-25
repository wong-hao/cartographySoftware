using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 检查水系附属设施线（HFCL）与道路套合关系的检查
    /// 干堤（270101）：与道路线（LRDL）严格套合
    /// 详见《吉林多尺度软件优化问题整理20180329》中的第一个需求
    /// </summary>
    public class CheckHFCL2RoadRegisterCmd : SMGI.Common.SMGICommand
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
            IGeoFeatureLayer hfclLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "HFCL");
            })).ToArray().First() as IGeoFeatureLayer;
            if (hfclLyr == null)
            {
                MessageBox.Show("缺少HFCL要素类！");
                return;
            }
            IFeatureClass hfclFC = hfclLyr.FeatureClass;

            IGeoFeatureLayer lrdlLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "LRDL");
            })).ToArray().First() as IGeoFeatureLayer;
            if (lrdlLyr == null)
            {
                MessageBox.Show("缺少LRDL要素类！");
                return;
            }
            IFeatureClass lrdlFC = lrdlLyr.FeatureClass;

            string gbFN = m_Application.TemplateManager.getFieldAliasName("GB", hfclFC.AliasName);
            int gbIndex = hfclFC.FindField(gbFN);
            if (gbIndex == -1)
            {
                MessageBox.Show(string.Format("要素类【{0}】中没有找到【{1}】字段！", hfclFC.AliasName, gbFN));
                return;
            }


            string outPutFileName = OutputSetup.GetDir() + string.Format("\\干堤与道路套合检查.shp");


            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                string gbSet = "";
                IQueryFilter qf = new QueryFilterClass();

                var gbList = new string[] { "270101" };
                foreach (var gb in gbList)
                {
                    if (gbSet != "")
                        gbSet += string.Format(",{0}", gb);
                    else
                        gbSet = string.Format("{0}", gb);
                }
                if (gbSet != "")
                    qf.WhereClause = string.Format("{0} in ({1})", gbFN, gbSet);

                if (hfclFC.HasCollabField())
                {
                    if(qf.WhereClause == "")
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                    else
                        qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;
                }

                IQueryFilter lrdlQF = new QueryFilterClass();
                lrdlQF.WhereClause = "";
                if (lrdlFC.HasCollabField())
                    lrdlQF.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                err = DoCheck(outPutFileName, hfclFC, qf, lrdlFC, lrdlQF, wo);
            }

            if (err == "")
            {
                IFeatureClass errFC = CheckHelper.OpenSHPFile(outPutFileName);
                int count = errFC.FeatureCount(null);
                if (count > 0)
                {
                    if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                }
                else
                {
                    MessageBox.Show("检查完毕！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }

        }

        public static string DoCheck(string resultSHPFileName, IFeatureClass hfclFC, IQueryFilter qf, IFeatureClass lrdlFC, IQueryFilter lrdlQF, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                //新建结果文件
                ShapeFileWriter resultFile = new ShapeFileWriter();
                Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                fieldName2Len.Add("图层名", 16);
                fieldName2Len.Add("要素编号", 16);
                fieldName2Len.Add("检查项", 32);
                fieldName2Len.Add("说明", 32);
                resultFile.createErrorResutSHPFile(resultSHPFileName, (hfclFC as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);

                Dictionary<string, List<int>> errInfo2OIDList = new Dictionary<string, List<int>>();
                List<int> errOIDList;

                if (wo != null)
                    wo.SetText("正在检查干堤的拓扑问题......");

                errOIDList = CheckHelper.LineNotCoveredByLineClass(hfclFC, qf, lrdlFC, lrdlQF);
                if (errOIDList.Count > 0)
                {
                    errInfo2OIDList.Add("干堤与道路不套合", errOIDList);
                }

                if (wo != null)
                    wo.SetText("正在输出检查结果......");
                foreach (var item in errInfo2OIDList)
                {
                    if (item.Value.Count == 0)
                        continue;

                    string oidSet = "";
                    foreach (var oid in item.Value)
                    {
                        if (oidSet != "")
                            oidSet += string.Format(",{0}", oid);
                        else
                            oidSet = string.Format("{0}", oid);
                    }

                    IFeatureCursor illegalFeCursor = hfclFC.Search(new QueryFilterClass() { WhereClause = string.Format("OBJECTID in ({0})", oidSet) }, true);
                    IFeature illegalFe = null;
                    while ((illegalFe = illegalFeCursor.NextFeature()) != null)
                    {
                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("图层名", hfclFC.AliasName);
                        fieldName2FieldValue.Add("要素编号", illegalFe.OID.ToString());
                        fieldName2FieldValue.Add("检查项", "干堤与道路套合检查");
                        fieldName2FieldValue.Add("说明", item.Key);

                        resultFile.addErrorGeometry(illegalFe.Shape, fieldName2FieldValue);

                    }
                    Marshal.ReleaseComObject(illegalFeCursor);

                }

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

        
    }
}
