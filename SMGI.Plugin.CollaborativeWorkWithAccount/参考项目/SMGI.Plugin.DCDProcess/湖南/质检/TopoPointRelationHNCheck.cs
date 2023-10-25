using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;

namespace SMGI.Plugin.DCDProcess
{
    public static class TopoPointRelationHNCheck
    {
        public static List<ErrTopoPointRelation> ErrTopoPtList = new List<ErrTopoPointRelation>();
        public static readonly StringBuilder _sbResult = new StringBuilder();
        public static ISpatialReference srf;
   
        /// <summary>
        /// 执行检查
        /// </summary>
        /// <param name="pfcls">待检查要素类</param>
        /// <returns></returns>
        public static ResultMessage Check(IFeatureWorkspace featureWorkspace, List<Tuple<string, string, string, string, string>> tts, Progress progWindow=null)
        {

            ErrTopoPtList.Clear();
            _sbResult.Clear();
            #region 逐项检查
            foreach (var tt in tts)
            {
                string ptName = tt.Item1;
                string ptSQL = tt.Item2;
                string plName = tt.Item3;
                string plSQL = tt.Item4;
                string beizhu = tt.Item5;
                IFeatureClass ptFC = null;
                IFeatureClass plFC = null;
                try
                {
                    ptFC = featureWorkspace.OpenFeatureClass(ptName);
                    plFC = featureWorkspace.OpenFeatureClass(plName);
                }
                catch (Exception ex)
                {
                    return new ResultMessage { stat = ResultState.Failed, msg = ex.Message };
                }
                if (ptFC==null || plFC==null)
                {
                    return new ResultMessage { stat = ResultState.Failed, msg =String.Format("{0} {1} 有空图层",ptName,plName) }; 
                }

                ISpatialReference ptSRF = (ptFC as IGeoDataset).SpatialReference;
                if (srf == null)
                {
                    srf = ptSRF;
                }
                string aliasName = ptFC.AliasName;                

                try
                {
                    IQueryFilter ptQF = new QueryFilterClass();
                    ptQF.WhereClause = ptFC.HasCollabField() ? ptSQL + " and " + cmdUpdateRecord.CurFeatureFilter : ptSQL;
                    IFeatureCursor ptFeatureCursor = ptFC.Search(ptQF, false);
                    IFeature ptCheckFeature = null;
                    int gbindex1 = ptFC.Fields.FindField("GB");
                    while ((ptCheckFeature = ptFeatureCursor.NextFeature()) != null)
                    {
                        String ptIDstr = ptCheckFeature.OID.ToString();
                        
                        if (progWindow != null)
                        {
                            progWindow.lbInfo.Text = "正在处理图层【" + ptName + "】中的要素" + ptIDstr + "......";
                            System.Windows.Forms.Application.DoEvents();
                        }

                        ISpatialFilter pFilter = new SpatialFilterClass();
                        pFilter.Geometry = ptCheckFeature.ShapeCopy;
                        pFilter.GeometryField = ptFC.ShapeFieldName;
                        pFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;//空间关系
                        pFilter.WhereClause = plFC.HasCollabField() ? plSQL + " and " + cmdUpdateRecord.CurFeatureFilter : plSQL;

                        IFeature plFeature = null;
                        IFeatureCursor plFeatureCursor = plFC.Search(pFilter, false);
                        bool flag = true;                        
                        while ((plFeature = plFeatureCursor.NextFeature()) != null)
                        {
                            flag = false;
                            break;
                        }
                        Marshal.ReleaseComObject(plFeatureCursor);
                        if (flag)
                        {
                            string str = ptIDstr.Trim() + "," + ptName + "," + beizhu + "\r\n";
                            IPoint pt = ptCheckFeature.ShapeCopy as IPoint;
                            ErrTopoPointRelation errTopoRel = new ErrTopoPointRelation() { 
                                PtLayerName = ptName,
                                PtID = ptIDstr,
                                RelLayerName = plName,                                 
                                RelName = "Intersect", //空间关系名
                                Info = beizhu, 
                                Pt =pt};
                            ErrTopoPtList.Add(errTopoRel);                            
                            _sbResult.Append(str);
                        }                        
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                    return new ResultMessage { stat = ResultState.Failed, msg = ex.Message };
                }

            }
            #endregion            

            return new ResultMessage { stat = ResultState.Ok };
        }
  
        /// <summary>
        /// 获取质检报告文本
        /// </summary>
        /// <returns></returns>
        public static string GetReport()
        {
            return _sbResult.ToString();
        }

        /// <summary>
        /// 保存质检结果
        /// </summary>
        /// <param name="dir">输出目录</param>
        /// <param name="autoOver"></param>
        /// <returns></returns>
        public static ResultMessage SaveResult(string dir,  bool autoOver = false, bool toGdb = false)
        {

            if (ErrTopoPtList.Count > 0)
            {
                //保存txt文件
                string txtFile = dir + "\\点拓扑检查.txt";
                var rusltMessage = TextFileWriter.SaveFile(txtFile, _sbResult.ToString(), autoOver);
                if (rusltMessage.stat != ResultState.Ok) return rusltMessage;

                //保存shp文件
                var shpWriter = new ShapeFileWriter();
                var shpdir = dir + "\\errShp";
                if (!Directory.Exists(shpdir))
                    Directory.CreateDirectory(shpdir);

                var fieldDic = new Dictionary<string, int>() { { "检查项", 40 }, { "点图层", 10 }, { "点ID", 10 }, { "线图层", 10 }, { "说明", 60 } };
                
                string shpFile = shpdir + "\\点拓扑检查";
                shpWriter.createErrorResutSHPFile(shpFile, srf, esriGeometryType.esriGeometryPoint, fieldDic, autoOver);

                foreach(ErrTopoPointRelation errTopoPtRel in ErrTopoPtList)                
                {
                    IPoint pt = errTopoPtRel.Pt;
                    String lyrName = errTopoPtRel.PtLayerName;
                    shpWriter.addErrorGeometry(pt, new Dictionary<string, string>()
                    {
                        { "检查项", "点拓扑检查" }, 
                        { "点图层", errTopoPtRel.PtLayerName }, 
                        { "点ID", errTopoPtRel.PtID }, 
                        { "线图层", errTopoPtRel.RelLayerName }, 
                        { "说明", errTopoPtRel.Info }
                    });
                }

                shpWriter.saveErrorResutSHPFile();

                rusltMessage.info = new[] { txtFile, shpFile };
                return rusltMessage;
            }
            else
            {
                return new ResultMessage { stat = ResultState.Cancel, msg = "检查出0条" };
            }
        }

        public static void Clear()
        {
            ErrTopoPtList.Clear();
            _sbResult.Clear();
        }
    }

    public class ErrTopoPointRelation
    {
        public string PtLayerName;
        public string PtID;
        public string RelLayerName;
        public string RelName;
        public string Info;
        public IPoint Pt;

    }
}
