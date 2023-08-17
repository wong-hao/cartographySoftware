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
using SMGI.Plugin.DCDProcess.GX;

namespace SMGI.Plugin.DCDProcess
{
    public class CheckRoadDataRankClassCmd : SMGICommand
    {
        public CheckRoadDataRankClassCmd()
        {

            m_category = "DataCheck";
            m_caption = "道路选取等级赋值检查";
            m_message = "根据道路的LGB属性值，检查道路选取等级赋值情况";
            m_toolTip = "根据道路的LGB属性值，检查道路选取等级赋值情况";

        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;

            }
        }
        public override void OnClick()
        {
            string lyrName = "LRDL";
            var layer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
                        ((l as IGeoFeatureLayer).Name.Trim().ToUpper() == lyrName)).FirstOrDefault() as IFeatureLayer;
            if (layer == null)
            {
                MessageBox.Show(string.Format("当前工作空间中没有找到图层【{0}】!", lyrName));
                return;
            }
            var lrdlFC = layer.FeatureClass;
            if (m_Application.MapControl.Map.ReferenceScale != 10000)
            {
                MessageBox.Show("检查目前只支持1：10000数据!");
                return;
            }
            
            string gradeFN = "GRADE";
            int gradeIndex = lrdlFC.Fields.FindField(gradeFN);
            if (gradeIndex == -1)
            {
                MessageBox.Show(string.Format("图层【{0}】中没有找到字段【{1}】!", lyrName, gradeFN));
                return;
            }
            CheckRoadDataRankClassForm frm = new CheckRoadDataRankClassForm();
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            string outputFileName = frm.OutputPath + string.Format("\\道路选取等级赋值检查.shp");

            //读取配置表，获取需检查的内容
            Dictionary<string, List<string>> filter2LGBList = new Dictionary<string, List<string>>();
            string mdbPath = GApplication.ExePath + @"\Template\质检\质检内容配置.mdb";
            if (!System.IO.File.Exists(mdbPath))
            {
                MessageBox.Show(string.Format("未找到配置文件:{0}!", mdbPath));
                return;
            }
            DataTable dataTable = DCDHelper.ReadToDataTable(mdbPath, "道路选取等级赋值检查");
            if (dataTable == null)
            {
                return;
            }

            foreach (DataRow dr in dataTable.Rows)
            {
                string level = dr["LEVEL"].ToString();
                string gb = dr["LGB"].ToString();
                string[] lgbList = gb.Split('、');
                foreach (var g in lgbList)
                {
                    if (!filter2LGBList.ContainsKey(g))
                    {
                        filter2LGBList.Add(g, level.Split(',').ToList());
                    }
                }
                
            }
            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                err = DoCheck(outputFileName, lrdlFC, filter2LGBList, wo);
            }

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
                    MessageBox.Show("检查完毕！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }
        }


        public static string DoCheck(string resultSHPFileName, IFeatureClass lrdlFC, Dictionary<string, List<string>> filter2LGBList, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                List<int> errOIDList = new List<int>();

                string lgbFN = "LGB";
                int lgbIndex = lrdlFC.FindField(lgbFN);
                int gradeIndex = lrdlFC.FindField("GRADE2");
                IFeatureCursor feCursor = lrdlFC.Search(null, true);
                IFeature fel = null;
                while ((fel = feCursor.NextFeature()) != null)
                {
                    if (wo != null)
                        wo.SetText("正在检查道路【" + fel.OID.ToString() + "】的选取等级与LGB匹配情况......");

                    string lgb = fel.get_Value(lgbIndex).ToString();
                    string GRADE = fel.get_Value(gradeIndex).ToString();
                    if (filter2LGBList.Keys.Contains(lgb))
                    {
                        if (!filter2LGBList[lgb].Contains(GRADE))
                        {
                            errOIDList.Add(fel.OID);
                        }
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);


                if (errOIDList.Count > 0)
                {
                    if (wo != null)
                        wo.SetText("正在输出检查结果......");

                    //新建结果文件
                    ShapeFileWriter resultFile = new ShapeFileWriter();
                    Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                    fieldName2Len.Add("图层名", 16);
                    fieldName2Len.Add("要素编号", 16);
                    fieldName2Len.Add("说明", 32);
                    fieldName2Len.Add("检查项", 32);

                    resultFile.createErrorResutSHPFile(resultSHPFileName, (lrdlFC as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);

                    foreach (var item in errOIDList)
                    {
                        IFeature fe = lrdlFC.GetFeature(item);

                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("图层名", lrdlFC.AliasName);
                        fieldName2FieldValue.Add("要素编号", fe.OID.ToString());
                        fieldName2FieldValue.Add("说明", "道路选取等级与LGB属性值不匹配");
                        fieldName2FieldValue.Add("检查项", "道路选取等级赋值检查");

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
