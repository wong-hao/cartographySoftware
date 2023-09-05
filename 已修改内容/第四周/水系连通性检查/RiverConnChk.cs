using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public class RiverConnChk
    {
        public RiverConnChk()
        {
        }


        public string DoCheck(string resultSHPFileName, IFeatureClass riverFC, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                ShapeFileWriter resultFile = null;

                IQueryFilter qf = new QueryFilterClass();
                string wc = string.Format("1 = 1");
                // if (riverFC.HasCollabField())//协同逻辑删除要素不参与质检
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

                    bool bConn = IsConn(riverFC, fe);

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
                        fieldName2FieldValue.Add("检查项", "水系连通性检查");
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
        /// <returns></returns>
        public bool IsConn(IFeatureClass riverFC, IFeature riverFe)
        {
            bool bConn = true;

            IPolyline riverGeo = riverFe.Shape as IPolyline;

            //起点
            {
                #region 起点
                IGeometry pGeo = riverGeo.FromPoint;

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

                    IPolyline pl2 = f.Shape as IPolyline;
                    IProximityOperator ProxiOP = (pGeo) as IProximityOperator;
                    if (ProxiOP.ReturnDistance(pl2.FromPoint) < 0.0001 
                        || ProxiOP.ReturnDistance(pl2.ToPoint) < 0.0001)//水系要素端点和另一水系要素端点相连
                    {
                        bExitMatchGradeRiver = true;
                    }
                }
                Marshal.ReleaseComObject(pCursor);

                if (!bExitMatchGradeRiver)
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

                    IPolyline pl2 = f.Shape as IPolyline;
                    IProximityOperator ProxiOP = (pGeo) as IProximityOperator;
                    if (ProxiOP.ReturnDistance(pl2.FromPoint) < 0.0001
                        || ProxiOP.ReturnDistance(pl2.ToPoint) < 0.0001)//水系要素端点和另一水系要素端点相连
                    {
                        bExitMatchGradeRiver = true;
                    }
                }
                Marshal.ReleaseComObject(pCursor);

                if (!bExitMatchGradeRiver)
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
