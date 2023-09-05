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
using System.Data;
using ESRI.ArcGIS.DataSourcesGDB;
using SMGI.Plugin.DCDProcess.GX;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public class CheckReservoirNameCheckCmd : SMGICommand
    {
        public CheckReservoirNameCheckCmd()
        {
            m_category = "DataCheck";
            m_caption = "水库名称一致性检查";
            m_message = "检查点状水库的名称是否与其对应面状水库的名称是否一致";
            m_toolTip = "";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;
            }
        }


        Dictionary<string, string> gbsDic = new Dictionary<string, string>();
        public static String pointLyrName = "点状水库";
        public static String areaLryName = "面状水域";
        public String checkField;

        public override void OnClick()
        {
            CheckReservoirNameCheckForm frm = new CheckReservoirNameCheckForm();
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            checkField = frm.checkField;

            gbsDic["240000"] = "";

            var hydlLyr = m_Application.Workspace.LayerManager.GetLayer((l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == pointLyrName);
            })).FirstOrDefault();
            if (hydlLyr == null)
            {
                MessageBox.Show("缺少" + pointLyrName + "要素类！");
                return;
            }
            IFeatureClass fc = (hydlLyr as IFeatureLayer).FeatureClass;

            var hydaLyr = m_Application.Workspace.LayerManager.GetLayer((l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == areaLryName);
            })).FirstOrDefault();
            if (hydaLyr == null)
            {
                MessageBox.Show("缺少" + areaLryName + "要素类！");
                return;
            }
            IFeatureClass fchyda = (hydaLyr as IFeatureLayer).FeatureClass;

            if ((fc.FindField(checkField) == -1) || (fchyda.FindField(checkField) == -1))
            {
                MessageBox.Show("需要检查的名称字段" + checkField + "有误！");
                return;
            }

            string outputFileName = frm.ResultOutputFilePath + string.Format("\\水库名称一致性检查.shp");
            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                err = DoCheck(outputFileName, fc, fchyda, wo);

                if (err == "")
                {
                    if (File.Exists(outputFileName))
                    {
                        IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName);

                        if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            CheckHelper.AddTempLayerToMap(m_Application.ActiveView.FocusMap, errFC);
                    }
                    else
                    {
                        MessageBox.Show("检查完毕，没有发现不一致性！");
                    }
                }
                else
                {
                    MessageBox.Show(err);
                }
            }
        }

        public string DoCheck(string resultSHPFileName, IFeatureClass fc, IFeatureClass fchyda, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                Dictionary<int, string> errsDic = new Dictionary<int, string>();

                IQueryFilter qf = new QueryFilterClass();
                ISpatialFilter sf = new SpatialFilter();
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelWithin;

                foreach (var kv in gbsDic)
                {
                    qf.WhereClause = "GB = " + kv.Key;
                    int countToltal = fc.FeatureCount(qf);
                    IFeature fe;
                    IFeatureCursor cursor = fc.Search(qf, true);
                    int count = 0;
                    while ((fe = cursor.NextFeature()) != null)
                    {
                        count++;
                        wo.SetText("正在检查【" + count + "/" + countToltal + "】");

                        // 获取点的 checkField 值
                        string value = fe.get_Value(fc.FindField(checkField)).ToString();

                        // 构造空间查询，以查找包含当前点的面要素
                        sf.Geometry = fe.Shape;
                        sf.WhereClause = "TYPE = '水库'";

                        // 在面要素中搜索符合空间条件的面要素
                        IFeature polygonFeature;
                        IFeatureCursor polygonCursor = fchyda.Search(sf, true);
                        while ((polygonFeature = polygonCursor.NextFeature()) != null)
                        {
                            // 获取面要素的 checkField 值
                            string polygonValue = polygonFeature.get_Value(fc.FindField(checkField)).ToString();

                            // 检查点的 checkField 和面的 checkField 是否不同，如果不同则记录错误信息
                            if (value != polygonValue)
                            {
                                errsDic[fe.OID] = "点要素：" + fe.OID + " 与面要素：" + polygonFeature.OID + " 的字段【" + checkField + "】不一致";
                            }
                        }
                        Marshal.ReleaseComObject(polygonCursor);
                    }
                    Marshal.ReleaseComObject(cursor);
                }

                if (errsDic.Count > 0)
                {
                    if (wo != null)
                        wo.SetText("正在输出检查结果......");

                    //新建结果文件
                    ShapeFileWriter resultFile = new ShapeFileWriter();
                    Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                    fieldName2Len.Add("图层名", 50);
                    fieldName2Len.Add("要素编号", 50);
                    fieldName2Len.Add("检查项", 50);
                    fieldName2Len.Add("错误信息", 50);
                    resultFile.createErrorResutSHPFile(resultSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPoint, fieldName2Len);

                    foreach (var item in errsDic)
                    {
                        IFeature fe = fc.GetFeature(item.Key);

                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("图层名", fc.AliasName);
                        fieldName2FieldValue.Add("要素编号", fe.OID.ToString());
                        fieldName2FieldValue.Add("检查项", "水库名称一致性检查");
                        fieldName2FieldValue.Add("错误信息", item.Value);
                        resultFile.addErrorGeometry(fe.ShapeCopy, fieldName2FieldValue);
                    }

                    //保存结果文件
                    resultFile.saveErrorResutSHPFile();
                }

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
