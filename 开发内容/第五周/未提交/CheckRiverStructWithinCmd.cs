using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 检查水系结构线是否合理
    /// </summary>
    public class CheckRiverStructWithinCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null;
            }
        }

        public String roadLyrName;
        public static String areaLryName = "面状水域";

        public override void OnClick()
        {
            CheckRiverStructFrm frm = new CheckRiverStructFrm();
            if (frm.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            roadLyrName = frm.roadLyrName;

            IGeoFeatureLayer hydlLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == roadLyrName);
            })).ToArray().First() as IGeoFeatureLayer;
            if (hydlLyr == null)
            {
                MessageBox.Show("缺少" + roadLyrName + "要素类！");
                return;
            }
            IFeatureClass hydlFC = hydlLyr.FeatureClass;

            IGeoFeatureLayer hydaLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == areaLryName);
            })).ToArray().First() as IGeoFeatureLayer;
            if (hydaLyr == null)
            {
                MessageBox.Show("缺少" + areaLryName + "要素类！");
                return;
            }
            IFeatureClass hydaFC = hydaLyr.FeatureClass;


            string outPutFileName = OutputSetup.GetDir() + string.Format("\\水系结构线检查包含.shp");


            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                err = DoCheck(outPutFileName, hydlFC, hydaFC, wo);
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

        /// <summary>
        /// 检查水系结构线位置是否合理：
        /// 1.水系结构线不在水系面内。
        /// </summary>
        /// <param name="resultSHPFileName"></param>
        /// <param name="hydlFC"></param>
        /// <param name="hydaFC"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public static string DoCheck(string resultSHPFileName, IFeatureClass hydlFC, IFeatureClass hydaFC, WaitOperation wo = null)
        {
            string err = "";

            int hgbIndex = hydlFC.FindField("GB");
            if (hgbIndex == -1)
            {
                err = string.Format("要素类【{0}】中没有找到GB字段！", hydlFC.AliasName);
                return err;
            }

            int hydaGBIndex = hydaFC.FindField("GB");
            if (hydaGBIndex == -1)
            {
                err = string.Format("要素类【{0}】中没有找到GB字段！", hydaFC.AliasName);
                return err;
            }

            try
            {
                Dictionary<int, string> oid2ErrInfo = new Dictionary<int, string>();

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "GB = 210000 or GB = 220000";
                // if (hydlFC.FindField(cmdUpdateRecord.CollabVERSION) != -1)
                qf.WhereClause += string.Format(" and ({0} <> {1} or {2} is null)", ServerDataInitializeCommand.CollabVERSION, cmdUpdateRecord.DeleteState, ServerDataInitializeCommand.CollabVERSION);
                IFeatureCursor feCursor = hydlFC.Search(qf, true);
                IFeature f;
                while ((f = feCursor.NextFeature()) != null)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在检查水系结构线【{0}】......", f.OID));
                    #region 检查不在任何水系面内的水系结构线

                    ISpatialFilter sf = new SpatialFilter();
                    sf.Geometry = f.Shape;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelWithin;
                    if (hydaFC.FindField(ServerDataInitializeCommand.CollabVERSION) != -1)
                        sf.WhereClause = "TYPE = '双线河流'or TYPE = '水库'or TYPE = '湖泊池塘'";
                    sf.WhereClause = string.Format("({0} <> {1} or {2} is null)", ServerDataInitializeCommand.CollabVERSION, cmdUpdateRecord.DeleteState, ServerDataInitializeCommand.CollabVERSION);
                    if (hydaFC.FeatureCount(sf) == 0)//该水系结构线不包含在任一水系面内
                    {
                        oid2ErrInfo.Add(f.OID, "水系结构线不包含在任一水系面内");

                        continue;
                    }

                    #endregion


                }
                Marshal.ReleaseComObject(feCursor);

                if (wo != null)
                    wo.SetText("正在输出检查结果......");

                #region 输出结果
                //新建结果文件
                ShapeFileWriter resultFile = new ShapeFileWriter();
                Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                fieldName2Len.Add("图层名", 20);
                fieldName2Len.Add("编号", 20);
                fieldName2Len.Add("说明", 40);
                fieldName2Len.Add("检查项", 20);
                resultFile.createErrorResutSHPFile(resultSHPFileName, (hydlFC as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);

                //输出内容
                if (oid2ErrInfo.Count > 0)
                {
                    foreach (var item in oid2ErrInfo)
                    {
                        IFeature fe = hydlFC.GetFeature(item.Key);

                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("图层名", hydlFC.AliasName);
                        fieldName2FieldValue.Add("编号", item.Key.ToString());
                        fieldName2FieldValue.Add("说明", item.Value.ToString());
                        fieldName2FieldValue.Add("检查项", "水系结构线检查");

                        resultFile.addErrorGeometry(fe.Shape, fieldName2FieldValue);
                    }
                }

                //保存结果文件
                resultFile.saveErrorResutSHPFile();
                #endregion
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
