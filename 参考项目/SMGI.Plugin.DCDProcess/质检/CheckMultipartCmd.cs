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
    /// 多部件检查
    /// </summary>
    public class CheckMultipartCmd : SMGICommand
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
            var frm = new CheckLayerSelectForm(m_Application, false, true, true);
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Text = "多部件检查";

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
                    if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName);
                        CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                    }
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
        /// 多部件检查
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
                    if (fc.ShapeType == esriGeometryType.esriGeometryPoint)
                        continue;

                    IQueryFilter qf = new QueryFilterClass();
                    if (fc.HasCollabField())
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                    if (wo != null)
                        wo.SetText(string.Format("正在对要素类【{0}】进行多部件检查......", fc.AliasName));

                    //核查该图层的重叠点
                    Dictionary<IGeometry, int> errList = CheckMultipart(fc, qf, wo);
                    if (errList.Count > 0)
                    {
                        if (resultFile == null)
                        {
                            //建立结果文件
                            resultFile = new ShapeFileWriter();
                            Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                            fieldName2Len.Add("图层名", 20);
                            fieldName2Len.Add("要素编号", 32);
                            fieldName2Len.Add("检查项", 32);
                            resultFile.createErrorResutSHPFile(resultSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);
                        }

                        //写入结果文件
                        foreach (var kv in errList)
                        {
                            Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                            fieldName2FieldValue.Add("图层名", fc.AliasName);
                            fieldName2FieldValue.Add("要素编号", kv.Value.ToString());
                            fieldName2FieldValue.Add("检查项", "多部件检查");

                            if (kv.Key.GeometryType == esriGeometryType.esriGeometryPolyline)
                            {
                                resultFile.addErrorGeometry(kv.Key, fieldName2FieldValue);
                            }
                            else if (kv.Key.GeometryType == esriGeometryType.esriGeometryPolygon)
                            {
                                var bdy = (kv.Key as ITopologicalOperator).Boundary as IPolyline;
                                resultFile.addErrorGeometry(bdy, fieldName2FieldValue);
                            }
                        }
                    }

                }

                //保存结果文件
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

        public static Dictionary<IGeometry, int> CheckMultipart(IFeatureClass fc, IQueryFilter qf, WaitOperation wo = null)
        {
            Dictionary<IGeometry, int> result = new Dictionary<IGeometry, int>();

            IFeatureCursor feCursor = fc.Search(qf, true);
            IFeature fe;
            while ((fe = feCursor.NextFeature()) != null)
            {
                if (fe.Shape == null || fe.Shape.IsEmpty)
                    continue;

                if(wo != null)
                    wo.SetText(string.Format("正在检查要素类【{0}】中的要素【{1}】......",fc.AliasName, fe.OID));

                if (fc.ShapeType == esriGeometryType.esriGeometryPolygon)
                {
                    var gc = (IGeometryCollection)(fe.Shape as IPolygon4).ConnectedComponentBag;
                    if (gc.GeometryCount > 1)
                    {
                        result.Add(fe.ShapeCopy, fe.OID);
                    }
                }
                else if(fc.ShapeType == esriGeometryType.esriGeometryPolyline)
                {
                    var gc = fe.Shape as IGeometryCollection;
                    if (gc.GeometryCount > 1)
                    {
                        result.Add(fe.ShapeCopy, fe.OID);
                    }
                }
            }
            Marshal.ReleaseComObject(feCursor);

            return result;
        }
    }
}
