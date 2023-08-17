using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    public class RiverGrade2ConnChk
    {
        public RiverGrade2ConnChk()
        {
        }


        public string DoCheck(string resultSHPFileName, IFeatureClass riverFC, int gradeTwoFieldIndex, long gradeTwo, WaitOperation wo = null)
        {
            string err = "";

            string gradeTwoFieldName = riverFC.Fields.get_Field(gradeTwoFieldIndex).Name;
            int pVerIndex = riverFC.FindField(cmdUpdateRecord.CollabVERSION);

            try
            {
                ShapeFileWriter resultFile = null;

                IQueryFilter qf = new QueryFilterClass();
                string wc = string.Format("{0} = {1} ", gradeTwoFieldName, gradeTwo);
                if (riverFC.HasCollabField())//协同逻辑删除要素不参与质检
                {
                    wc += " and " + cmdUpdateRecord.CurFeatureFilter;
                }
                qf.WhereClause = wc;
                IFeatureCursor pCursor = riverFC.Search(qf, true);
                IFeature fe = null;
                while ((fe = pCursor.NextFeature()) != null)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在检测要素【{0}】......", fe.OID));

                    bool bConn = IsConn(riverFC, fe, gradeTwoFieldIndex);

                    if (!bConn)//输出至结果文件
                    {
                        IPolyline pl = fe.ShapeCopy as IPolyline;

                        if (resultFile == null)
                        {
                            //建立结果文件
                            resultFile = new ShapeFileWriter();
                            Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                            fieldName2Len.Add("检查项", 20);
                            fieldName2Len.Add("图层名", 20);
                            fieldName2Len.Add("编号", 20);
                            resultFile.createErrorResutSHPFile(resultSHPFileName, (riverFC as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);
                        }

                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("检查项", "水系几何等级连通性检查");
                        fieldName2FieldValue.Add("图层名", riverFC.AliasName);
                        fieldName2FieldValue.Add("编号", string.Format("{0}", fe.OID));

                        resultFile.addErrorGeometry(pl, fieldName2FieldValue);
                    }
                }
                Marshal.ReleaseComObject(pCursor);

                //保存结果文件
                if (resultFile != null)
                {
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

        /// <summary>
        /// 检测水系几何分级等级是否连通
        /// </summary>
        /// <param name="roadFC"></param>
        /// <param name="roadFe"></param>
        /// <param name="gradeFieldIndex"></param>
        /// <returns></returns>
        public bool IsConn(IFeatureClass riverFC, IFeature riverFe, int gradeTwoFieldIndex)
        {
            bool bConn = true;

            long gradeTwo = long.Parse(riverFe.get_Value(gradeTwoFieldIndex).ToString());
            IPolyline riverGeo = riverFe.Shape as IPolyline;

            //起点
            {
                #region 起点
                IGeometry pGeo = riverGeo.FromPoint;

                bool bExitGradeRiver = false;//某一端点处是否存在其它具有Grade值的水系要素
                bool bExitMatchGradeRiver = false;//某一端点处是否存在与之匹配的水系要素

                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = pGeo;
                sf.GeometryField = "SHAPE";
                sf.WhereClause = "";
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureCursor pCursor = riverFC.Search(sf, false);
                IFeature f = null;
                while ((f = pCursor.NextFeature()) != null)
                {
                    if (f.OID == riverFe.OID)
                        continue;

                    long gradeTwo2 = -1;
                    try
                    {
                        gradeTwo2 = long.Parse(f.get_Value(gradeTwoFieldIndex).ToString());
                    }
                    catch
                    {
                        //没有赋水系几何等级，直接跳过
                        continue;
                    }

                    IPolyline pl2 = f.Shape as IPolyline;
                    IProximityOperator ProxiOP = (pGeo) as IProximityOperator;
                    if (ProxiOP.ReturnDistance(pl2.FromPoint) < 0.0001 
                        || ProxiOP.ReturnDistance(pl2.ToPoint) < 0.0001)//水系要素端点和另一水系要素端点相连
                    {
                        bExitGradeRiver = true;//存在具有Grade2的水系

                        if (gradeTwo2 <= gradeTwo)//相邻水系要素的几何等级不小于该水系要素的几何等级（等级数值不大于该要素），找到了与之端点连通的水系要素
                        {
                            bExitMatchGradeRiver = true;
                            break;
                        }
                    }
                }
                Marshal.ReleaseComObject(pCursor);

                if (bExitGradeRiver && !bExitMatchGradeRiver)
                {
                    bConn = false;
                    return bConn;
                }

                #endregion
            }

            //终点
            {
                #region 终点
                IGeometry pGeo = riverGeo.ToPoint;

                bool bExitGradeRiver = false;//某一端点处是否存在其它具有Grade值的水系要素
                bool bExitMatchGradeRiver = false;//某一端点处是否存在与之匹配的水系要素

                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = pGeo;
                sf.GeometryField = "SHAPE";
                sf.WhereClause = "";
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureCursor pCursor = riverFC.Search(sf, false);
                IFeature f = null;
                while ((f = pCursor.NextFeature()) != null)
                {
                    if (f.OID == riverFe.OID)
                        continue;

                    long gradeTwo2 = -1;
                    try
                    {
                        gradeTwo2 = long.Parse(f.get_Value(gradeTwoFieldIndex).ToString());
                    }
                    catch
                    {
                        //没有赋水系几何等级，直接跳过
                        continue;
                    }

                    IPolyline pl2 = f.Shape as IPolyline;
                    IProximityOperator ProxiOP = (pGeo) as IProximityOperator;
                    if (ProxiOP.ReturnDistance(pl2.FromPoint) < 0.0001
                        || ProxiOP.ReturnDistance(pl2.ToPoint) < 0.0001)//水系要素端点和另一水系要素端点相连
                    {
                        bExitGradeRiver = true;//存在具有Grade2的水系

                        if (gradeTwo2 <= gradeTwo)//相邻水系要素的几何等级不小于该水系要素的几何等级（等级数值不大于该要素），找到了与之端点连通的水系要素
                        {
                            bExitMatchGradeRiver = true;
                            break;
                        }
                    }
                }
                Marshal.ReleaseComObject(pCursor);

                if (bExitGradeRiver && !bExitMatchGradeRiver)
                {
                    bConn = false;
                    return bConn;
                }
                #endregion
            }

            return bConn;
        }
    }
}
