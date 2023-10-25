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
    /// 打褶线检查
    /// </summary>
    public class CheckWrinkleLineCmd : SMGICommand
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
            if (m_Application.MapControl.Map.ReferenceScale == 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;
            }

            var frm = new CheckWrinkleLineForm(m_Application);
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Text = "打褶线检查";

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
                    if (!fcList.Contains(fc))
                        fcList.Add(fc);
                }

                err = DoCheck(outputFileName, fcList, frm.AngleThreshold, frm.LenThreshold, m_Application.MapControl.Map.ReferenceScale, wo);
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
                    MessageBox.Show("检查完毕，没有发现打褶线！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }

        }
        
        /// <summary>
        /// 打褶线检查
        /// </summary>
        /// <param name="resultSHPFileName"></param>
        /// <param name="fcList"></param>
        /// <param name="angleThreshold"></param>
        /// <param name="LenThreshold"></param>
        /// <param name="referScale"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public static string DoCheck(string resultSHPFileName, List<IFeatureClass> fcList, double angleThreshold, double LenThreshold, double referScale, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                ShapeFileWriter resultFile = null;

                foreach (var fc in fcList)
                {
                    if (fc.ShapeType != esriGeometryType.esriGeometryPolyline)
                        continue;

                    IQueryFilter qf = new QueryFilterClass();
                    if (fc.HasCollabField())
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                    if (wo != null)
                        wo.SetText(string.Format("正在对要素类【{0}】进行打褶线检查......", fc.AliasName));

                    //核查该图层的打褶点
                    Dictionary<IPoint, int> errList = CheckWrinkleLine(fc, qf, angleThreshold, LenThreshold * referScale * 1.0e-3, wo);
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
                            resultFile.createErrorResutSHPFile(resultSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPoint, fieldName2Len);
                        }

                        //写入结果文件
                        foreach (var kv in errList)
                        {
                            Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                            fieldName2FieldValue.Add("图层名", fc.AliasName);
                            fieldName2FieldValue.Add("要素编号", kv.Value.ToString());
                            fieldName2FieldValue.Add("检查项", "打褶线检查");

                            resultFile.addErrorGeometry(kv.Key, fieldName2FieldValue);
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

        public static Dictionary<IPoint, int> CheckWrinkleLine(IFeatureClass lineFC, IQueryFilter lineQF, double angleThreshold, double LenThreshold, WaitOperation wo = null)
        {
            Dictionary<IPoint, int> result = new Dictionary<IPoint, int>();

            IFeatureCursor feCursor = lineFC.Search(lineQF, true);
            IFeature fe;
            while ((fe = feCursor.NextFeature()) != null)
            {
                if (fe.Shape == null || fe.Shape.IsEmpty)
                    continue;

                if (wo != null)
                    wo.SetText(string.Format("正在检查要素类【{0}】中的要素【{1}】......", lineFC.AliasName, fe.OID));

                //角度+深度 计算每个节点夹角和相邻线长度
                var ptColl = fe.ShapeCopy as IPointCollection;
                var lengthList = new List<double>();//np-1
                var angleList = new List<double>();//np-2
                lengthList.Add(Dist(ptColl.Point[0], ptColl.Point[1]));
                for (var i = 1; i < ptColl.PointCount - 1; i++)
                {
                    angleList.Add(Angle(ptColl.Point[i], ptColl.Point[i - 1], ptColl.Point[i + 1]));
                    lengthList.Add(Dist(ptColl.Point[i], ptColl.Point[i + 1]));
                }

                for (var i = 0; i < angleList.Count; i++)
                {
                    if (angleList[i] < angleThreshold)
                    {
                        if (lengthList[i + 1] < LenThreshold || lengthList[i] < LenThreshold)
                        {
                            result.Add(ptColl.Point[i + 1], fe.OID);
                        }
                    }
                }

            }
            Marshal.ReleaseComObject(feCursor);

            return result;
        }

        //两点距离
        public static double Dist(IPoint pt1, IPoint pt2)
        {
            var xoff = pt1.X - pt2.X;
            var yoff = pt1.Y - pt2.Y;

            return Math.Sqrt(xoff * xoff + yoff * yoff);
        }

        //返回度 cos=a*b/|a|*|b| 向量
        public static double Angle(IPoint cen, IPoint pt1, IPoint pt2)
        {
            double ma_x = pt1.X - cen.X, ma_y = pt1.Y - cen.Y;
            double mb_x = pt2.X - cen.X, mb_y = pt2.Y - cen.Y;
            double v1 = ma_x * mb_x + ma_y * mb_y;
            double ma_val = Math.Sqrt(ma_x * ma_x + ma_y * ma_y);
            double mb_val = Math.Sqrt(mb_x * mb_x + mb_y * mb_y);
            double cosM = v1 / (ma_val * mb_val);
            double angAmb = Math.Acos(cosM) * 180 / Math.PI;
            return angAmb;
        }
    }
}
