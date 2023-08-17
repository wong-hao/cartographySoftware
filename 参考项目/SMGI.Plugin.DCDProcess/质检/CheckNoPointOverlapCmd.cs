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

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 点重叠检查
    /// </summary>
    public class CheckNoPointOverlapCmd : SMGICommand
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
            var frm = new CheckLayerSelectForm(m_Application, true, false, false);
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Text = "点重叠检查";

            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string outputFileName;
            if (frm.CheckFeatureLayerList.Count > 1)
            {
                outputFileName = OutputSetup.GetDir() + string.Format("\\{0}.shp", frm.Text);
            }
            else
            {
                outputFileName = OutputSetup.GetDir() + string.Format("\\{0}_{1}.shp", frm.Text, frm.CheckFeatureLayerList.First().Name);
            }


            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                List<IFeatureClass> fcList = new List<IFeatureClass>();
                foreach (var layer in frm.CheckFeatureLayerList)
                {
                    IFeatureClass fc = layer.FeatureClass;
                    if(!fcList.Contains(fc))
                        fcList.Add(fc);
                }

                err = DoCheck(outputFileName, fcList, wo);
            }

            if (err == "")
            {
                if (File.Exists(outputFileName))
                {
                    IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName);

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
        /// 点重叠检查
        /// </summary>
        /// <param name="resultSHPFileName"></param>
        /// <param name="fcList"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public static string DoCheck(string resultSHPFileName, List<IFeatureClass> fcList, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                ShapeFileWriter resultFile = null;

                foreach (var fc in fcList)
                {
                    if (fc.ShapeType != esriGeometryType.esriGeometryPoint)
                        continue;

                    IQueryFilter qf = new QueryFilterClass();
                    if (fc.HasCollabField())
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                    if (wo != null)
                        wo.SetText(string.Format("正在对要素类【{0}】进行点重叠检查......", fc.AliasName));

                    //核查该图层的重叠点
                    List<KeyValuePair<int, int>> errOIDList = CheckHelper.PointNoOverlap(fc, qf);
                    if(errOIDList.Count > 0)
                    {
                        if (resultFile == null)
                        {
                            //建立结果文件
                            resultFile = new ShapeFileWriter();
                            Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                            fieldName2Len.Add("图层名", 20);
                            fieldName2Len.Add("要素编号1", 32);
                            fieldName2Len.Add("要素编号2", 32);
                            fieldName2Len.Add("检查项", 32);
                            resultFile.createErrorResutSHPFile(resultSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPoint, fieldName2Len);
                        }

                        //写入结果文件
                        foreach (var kv in errOIDList)
                        {
                            IFeature errFe = fc.GetFeature(kv.Key);

                            Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                            fieldName2FieldValue.Add("图层名", fc.AliasName);
                            fieldName2FieldValue.Add("要素编号1", kv.Key.ToString());
                            fieldName2FieldValue.Add("要素编号2", kv.Value.ToString());
                            fieldName2FieldValue.Add("检查项", "点重叠检查");


                            resultFile.addErrorGeometry(errFe.Shape, fieldName2FieldValue);
                        }
                    }
                    
                }

                //保存结果文件
                if(resultFile != null)
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
