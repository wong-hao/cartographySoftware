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
    /// <summary>
    /// 点拓扑检查
    /// </summary>
    public class TopoPointRelationCheck
    {
        readonly List<IPoint> _erpPoints = new List<IPoint>();
        private ISpatialReference _spatialReference;
        readonly StringBuilder _sbResult = new StringBuilder();
        private string _curlyrname = "";
        string Content;
        //OutHelp error;
        Progress progWindow = null;
        //    errorGeoDataFile = null;
        //TxtRecordFile errorTxtFile = null;
        List<string> checkedpointlayers = new List<string>();
        List<string> checkedobjectlayers = new List<string>();
        string relation;
        IFeatureWorkspace featureWorkspace;
        //GApplication ap;
        string polylinefeatureclass;
        string obfeatureclass;
        public TopoPointRelationCheck(IFeatureWorkspace _featureWorkspace, Progress prog)
        {
            //error = new OutHelp(_ap);
            this.progWindow = prog;
            //this.errorTxtFile = txtRecord;
            //checkedpointlayers = _checkedpointlayers;
            //checkedobjectlayers = _checkedobjectlayers;
            //relation = _relation;
            //Content = _Content;
            featureWorkspace = _featureWorkspace;
            //ap = _ap;
            //polylinefeatureclass = _polylinefeatureclass;
            //obfeatureclass = _obfeatureclass;
        }
        /// <summary>
        /// 检查结果条数
        /// </summary>
        public int Count
        {
            get;
            private set;
        }
        /// <summary>
        /// 执行检查
        /// </summary>
        /// <param name="pfcls">待检查要素类</param>
        /// <returns></returns>
        public ResultMessage Check()
        {

            IFeatureClass lfcpfeatureclass = null;
            IFeatureClass lrrlFeatureclass=null;
            IFeatureClass lrdlFeatureclass = null;
            IFeatureClass lfclFeatureclass = null;
            try
                {
                    lfcpfeatureclass = featureWorkspace.OpenFeatureClass("LFCP");
                    lrrlFeatureclass = featureWorkspace.OpenFeatureClass("LRRL");
                    lrdlFeatureclass = featureWorkspace.OpenFeatureClass("LRDL");
                    lfclFeatureclass = featureWorkspace.OpenFeatureClass("LFCL");
                }
            catch (Exception ex)
            { 
                return new ResultMessage { stat = ResultState.Failed, msg = ex.Message };
            }
            _spatialReference = (lfcpfeatureclass as IGeoDataset).SpatialReference;
            _curlyrname = lfcpfeatureclass.AliasName;
            _sbResult.Clear();
            _erpPoints.Clear();
            try
            {
                    #region LFCP层火车站410301必须在铁路上
                IQueryFilter nf = new QueryFilterClass();
                    if (lfcpfeatureclass.HasCollabField())
                    {
                        nf.WhereClause = "(GB = 410301) and " + cmdUpdateRecord.CurFeatureFilter;
                    }
                    else
                    {
                        nf.WhereClause = "(GB = 410301)";
                    }
                    IFeatureCursor pFeatureCursor = lfcpfeatureclass.Search(nf , false);
                    IFeature CheckFeature = null;
                    int gbindex1 = lfcpfeatureclass.Fields.FindField("GB");
                    while ((CheckFeature = pFeatureCursor.NextFeature()) != null)
                    {
                        #region
                        progWindow.lbInfo.Text = "正在处理图层【" + (lfcpfeatureclass as IDataset).Name + "】中的要素" + CheckFeature.OID.ToString() + "......";
                        System.Windows.Forms.Application.DoEvents();
                        ISpatialFilter pFilter = new SpatialFilterClass();
                        pFilter.Geometry = CheckFeature.ShapeCopy;
                        pFilter.GeometryField = lfcpfeatureclass.ShapeFieldName;
                        pFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                        IFeature fea = null;
                        IFeatureCursor FeatureCursor = lrrlFeatureclass.Search(pFilter, false);
                        bool flag = true;
                        //int gbindex = obbfeatureclass.Fields.FindField("GB");
                        while ((fea = FeatureCursor.NextFeature()) != null)
                        {
                           
                                flag = false;
                                break;
                            
                        } 
                        Marshal.ReleaseComObject(FeatureCursor);
                        if (flag)
                        {
                                string str = CheckFeature.OID.ToString().Trim() + "," + (lfcpfeatureclass as IDataset).Name + "," + "火车站必须在铁路上" + "\r\n";
                                _erpPoints.Add(CheckFeature.ShapeCopy as IPoint);
                                _sbResult.Append(str);

                        }
                        #endregion
                    }
                    Marshal.ReleaseComObject(pFeatureCursor);
                #endregion
                    #region 山隘440500 道路或者铁路上
                    IQueryFilter nf2 = new QueryFilterClass();
                    if (lfcpfeatureclass.HasCollabField())
                    {
                        nf2.WhereClause = "(GB = 440500) and " + cmdUpdateRecord.CurFeatureFilter;
                    }
                    else
                    {
                        nf2.WhereClause = "(GB = 440500)";
                    }
                    IFeatureCursor pFeatureCursor3 = lfcpfeatureclass.Search( nf2, false);
                    IFeature CheckFeature3 = null;
                    while ((CheckFeature3 = pFeatureCursor3.NextFeature()) != null)
                    {
                        #region
                        progWindow.lbInfo.Text = "正在处理图层【" + (lfcpfeatureclass as IDataset).Name + "】中的要素" + CheckFeature3.OID.ToString() + "......";
                        System.Windows.Forms.Application.DoEvents();
                        ISpatialFilter pFilter = new SpatialFilterClass();
                        pFilter.Geometry = CheckFeature3.ShapeCopy;
                        pFilter.GeometryField = lfcpfeatureclass.ShapeFieldName;
                        pFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                        IFeature fea = null;
                        IFeatureCursor FeatureCursor = lrdlFeatureclass.Search(pFilter, false);
                        bool flag1 = true;bool flag2 = true;
                       
                        while ((fea = FeatureCursor.NextFeature()) != null)
                        {
                                flag1 = false;
                                break;
                            
                        } Marshal.ReleaseComObject(FeatureCursor);
                        IFeature fea2 = null;
                        IFeatureCursor FeatureCursor2 = lrrlFeatureclass.Search(pFilter, false);
                        while ((fea2 = FeatureCursor2.NextFeature()) != null)
                        {
                                flag2 = false;
                                break;
                            
                        }
                        Marshal.ReleaseComObject(FeatureCursor2);
                        if (flag1 == true && flag2 == true)
                        {
                                string str = CheckFeature3.OID.ToString().Trim() + "," + (lfcpfeatureclass as IDataset).Name + "," + "山隘必须在道路或者铁路上" + "\r\n";
                                _erpPoints.Add(CheckFeature3.ShapeCopy as IPoint);
                                _sbResult.Append(str);

                        }
                        #endregion
                    }
                    Marshal.ReleaseComObject(pFeatureCursor3);
                    #endregion
                    #region 服务区450105 在高速路上
                    IQueryFilter nf3 = new QueryFilterClass();

                    if (lfcpfeatureclass.HasCollabField())
                    {
                        nf3.WhereClause = "(GB = 450105) and " + cmdUpdateRecord.CurFeatureFilter;
                    }
                    else
                    {
                        nf3.WhereClause = "(GB = 450105)";
                    }

                    IFeatureCursor pFeatureCursor2 = lfcpfeatureclass.Search(nf3 , false);
                    IFeature CheckFeature2 = null;
                 
                    while ((CheckFeature2 = pFeatureCursor2.NextFeature()) != null)
                    {
                        #region
                        progWindow.lbInfo.Text = "正在处理图层【" + (lfcpfeatureclass as IDataset).Name + "】中的要素" + CheckFeature2.OID.ToString() + "......";
                        System.Windows.Forms.Application.DoEvents();
                        ISpatialFilter pFilter = new SpatialFilterClass();
                        pFilter.Geometry = CheckFeature2.ShapeCopy;
                        pFilter.GeometryField = lfcpfeatureclass.ShapeFieldName;
                        pFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                        IFeature fea = null;
                        IFeatureCursor FeatureCursor = lrdlFeatureclass.Search(pFilter, false);
                        bool flag = true;
                        int lgbindex = lrdlFeatureclass.Fields.FindField("LGB");
                        while ((fea = FeatureCursor.NextFeature()) != null)
                        {
                            string lgb = fea.get_Value(lgbindex).ToString();//好接口
                            if (lgb=="420901" || lgb=="420902")
                            {
                                flag = false;
                                break;
                            }
                        } 
                        Marshal.ReleaseComObject(FeatureCursor);
                        if (flag)
                        {
                                string str = CheckFeature2.OID.ToString().Trim() + "," + (lfcpfeatureclass as IDataset).Name + "," + "服务区必须在高速路上" + "\r\n";
                                _erpPoints.Add(CheckFeature2.ShapeCopy as IPoint);
                                _sbResult.Append(str);

                        }
                        #endregion
                    }
                    Marshal.ReleaseComObject(pFeatureCursor2);
                    #endregion
                    #region 收费站450106必须在高速路上
                    IQueryFilter nf4 = new QueryFilterClass();

                    if (lfcpfeatureclass.HasCollabField())
                    {
                        nf4.WhereClause = "(GB = 450106) and " + cmdUpdateRecord.CurFeatureFilter;
                    }
                    else
                    {
                        nf4.WhereClause = "(GB = 450106)";
                    }
                    IFeatureCursor pFeatureCursor4 = lfcpfeatureclass.Search(nf4 , false);
                    IFeature CheckFeature4 = null;
                    while ((CheckFeature4 = pFeatureCursor4.NextFeature()) != null)
                    {
                        #region
                        progWindow.lbInfo.Text = "正在处理图层【" + (lfcpfeatureclass as IDataset).Name + "】中的要素" + CheckFeature4.OID.ToString() + "......";
                        System.Windows.Forms.Application.DoEvents();
                        ISpatialFilter pFilter = new SpatialFilterClass();
                        pFilter.Geometry = CheckFeature4.ShapeCopy;
                        pFilter.GeometryField = lfcpfeatureclass.ShapeFieldName;
                        pFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                        int ct = lrdlFeatureclass.FeatureCount(pFilter);
                        int ct1 = lfclFeatureclass.FeatureCount(pFilter);
                        bool flag = true;
                         //兼容1万数据
                        if (GApplication.Application.MapControl.Map.ReferenceScale == 10000)
                        {
                            if (ct > 0||ct1>0)
                                flag = false;
                        }
                        else
                        {
                            IFeature fea = null;
                            IFeatureCursor FeatureCursor = lrdlFeatureclass.Search(pFilter, false);
                           
                            int lgbindex = lrdlFeatureclass.Fields.FindField("LGB");
                            while ((fea = FeatureCursor.NextFeature()) != null)
                            {
                                string lgb = fea.get_Value(lgbindex).ToString();//好接口

                                if (lgb == "420901" || lgb == "420902")
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            Marshal.ReleaseComObject(FeatureCursor);
                        }
                        if (flag)
                        {
                            string str = CheckFeature4.OID.ToString().Trim() + "," + (lfcpfeatureclass as IDataset).Name + "," + "收费站必须在高速路上" + "\r\n";
                            _erpPoints.Add(CheckFeature4.ShapeCopy as IPoint);
                            _sbResult.Append(str);

                        }
                        #endregion
                    }
                    Marshal.ReleaseComObject(pFeatureCursor4);
                    #endregion
                    #region 互通立交桥LFCP=450308必须在高速路上
                    nf4 = new QueryFilterClass();

                    if (lfcpfeatureclass.HasCollabField())
                    {
                        nf4.WhereClause = "(GB = 450308) and " + cmdUpdateRecord.CurFeatureFilter;
                    }
                    else
                    {
                        nf4.WhereClause = "(GB = 450308)";
                    }
                    pFeatureCursor4 = lfcpfeatureclass.Search(nf4, false);
                    while ((CheckFeature4 = pFeatureCursor4.NextFeature()) != null)
                    {
                        #region
                        progWindow.lbInfo.Text = "正在处理图层【" + (lfcpfeatureclass as IDataset).Name + "】中的要素" + CheckFeature4.OID.ToString() + "......";
                        System.Windows.Forms.Application.DoEvents();
                        ISpatialFilter pFilter = new SpatialFilterClass();
                        pFilter.Geometry = CheckFeature4.ShapeCopy;
                        pFilter.GeometryField = lfcpfeatureclass.ShapeFieldName;
                        pFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                        int ct = lrdlFeatureclass.FeatureCount(pFilter);
                        int ct1 = lfclFeatureclass.FeatureCount(pFilter);
                        bool flag = true;
                        //兼容1万数据
                        if (GApplication.Application.MapControl.Map.ReferenceScale == 10000)
                        {
                            if (ct > 0 || ct1 > 0)
                                flag = false;
                        }
                        else
                        {
                            IFeature fea = null;
                            IFeatureCursor FeatureCursor = lrdlFeatureclass.Search(pFilter, false);

                            int lgbindex = lrdlFeatureclass.Fields.FindField("LGB");
                            while ((fea = FeatureCursor.NextFeature()) != null)
                            {
                                string lgb = fea.get_Value(lgbindex).ToString();//好接口

                                if (lgb == "420901" || lgb == "420902")
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            Marshal.ReleaseComObject(FeatureCursor);
                        }
                        if (flag)
                        {
                            string str = CheckFeature4.OID.ToString().Trim() + "," + (lfcpfeatureclass as IDataset).Name + "," + "收费站必须在高速路上" + "\r\n";
                            _erpPoints.Add(CheckFeature4.ShapeCopy as IPoint);
                            _sbResult.Append(str);

                        }
                        #endregion
                    }
                    Marshal.ReleaseComObject(pFeatureCursor4);
                    #endregion
                    #region 轻轨站（LFCP层GB:450102）必须位于轻轨（LRRL层GB:430102）上
                    IQueryFilter  qf = new QueryFilterClass();

                    if (lfcpfeatureclass.HasCollabField())
                    {
                        qf.WhereClause = "(GB = 450102) and " + cmdUpdateRecord.CurFeatureFilter;
                    }
                    else
                    {
                        qf.WhereClause = "GB = 450102";
                    }
                    IFeatureCursor lfcpFeCursor = lfcpfeatureclass.Search(qf, true);
                    IFeature lfcpFe = null;
                    while ((lfcpFe = lfcpFeCursor.NextFeature()) != null)
                    {
                        progWindow.lbInfo.Text = "正在处理图层【" + (lfcpfeatureclass as IDataset).Name + "】中的要素" + lfcpFe.OID.ToString() + "......";

                        System.Windows.Forms.Application.DoEvents();

                        bool flag = true;
                        int gbindex = lrrlFeatureclass.Fields.FindField("GB");

                        ISpatialFilter sf = new SpatialFilterClass();
                        sf.Geometry = lfcpFe.ShapeCopy;
                        sf.GeometryField = lfcpfeatureclass.ShapeFieldName;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        sf.WhereClause = "GB = 430102";

                        IFeatureCursor lrrlFeCursor = lrrlFeatureclass.Search(sf, true);
                        IFeature lrrlFe = null;
                        while ((lrrlFe = lrrlFeCursor.NextFeature()) != null)
                        {
                            flag = false;
                            break;
                        }
                        Marshal.ReleaseComObject(lrrlFeCursor);

                        if (flag)
                        {
                            string str = lfcpFe.OID.ToString().Trim() + "," + (lfcpfeatureclass as IDataset).Name + "," + "轻轨站必须在轻轨线上" + "\r\n";
                            _erpPoints.Add(lfcpFe.ShapeCopy as IPoint);
                            _sbResult.Append(str);

                        }
                    }
                    Marshal.ReleaseComObject(lfcpFeCursor);
                    #endregion
                    #region  地铁（GB=450101）及轻轨站（GB=450102）必须捕捉在地铁LRRL线(430101)
                    qf = new QueryFilterClass();

                    if (lfcpfeatureclass.HasCollabField())
                    {
                        qf.WhereClause = "(GB = 450102 or GB = 450101) and " + cmdUpdateRecord.CurFeatureFilter;
                    }
                    else
                    {
                        qf.WhereClause = "(GB = 450102 or GB = 450101)";
                    }
                    lfcpFeCursor = lfcpfeatureclass.Search(qf, true);
                    lfcpFe = null;
                    while ((lfcpFe = lfcpFeCursor.NextFeature()) != null)
                    {
                        progWindow.lbInfo.Text = "正在处理图层【" + (lfcpfeatureclass as IDataset).Name + "】中的要素" + lfcpFe.OID.ToString() + "......";

                        System.Windows.Forms.Application.DoEvents();

                        bool flag = true;
                        int gbindex = lrrlFeatureclass.Fields.FindField("GB");

                        ISpatialFilter sf = new SpatialFilterClass();
                        sf.Geometry = lfcpFe.ShapeCopy;
                        sf.GeometryField = lfcpfeatureclass.ShapeFieldName;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        sf.WhereClause = "GB = 430101";

                        IFeatureCursor lrrlFeCursor = lrrlFeatureclass.Search(sf, true);
                        IFeature lrrlFe = null;
                        while ((lrrlFe = lrrlFeCursor.NextFeature()) != null)
                        {
                            flag = false;
                            break;
                        }
                        Marshal.ReleaseComObject(lrrlFeCursor);

                        if (flag)
                        {
                            string str = lfcpFe.OID.ToString().Trim() + "," + (lfcpfeatureclass as IDataset).Name + "," + "地铁及轻轨站必须捕捉在地铁线" + "\r\n";
                            _erpPoints.Add(lfcpFe.ShapeCopy as IPoint);
                            _sbResult.Append(str);
                        }
                    }
                    Marshal.ReleaseComObject(lfcpFeCursor);
                    #endregion
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    return new ResultMessage { stat = ResultState.Failed, msg = ex.Message };
                }
                Count = _erpPoints.Count;

                return new ResultMessage { stat = ResultState.Ok };
                } 
        /// <summary>
        /// 获取质检报告文本
        /// </summary>
        /// <returns></returns>
        public string GetReport()
        {
            return _sbResult.ToString();
        }

        /// <summary>
        /// 保存质检结果 +2
        /// </summary>
        /// <param name="dir">输出目录</param>
        /// <param name="autoOver"></param>
        /// <returns></returns>
        public ResultMessage SaveResult(string dir, bool autoOver = false, bool toGdb = false)
        {
            //保存shp文件
            if (_erpPoints.Count > 0)
            {

                //if (toGdb)
                //{
                //    SaveToGDB();
                //    return new ResultMessage { stat = ResultState.Ok };
                //}
                var tf = dir + "\\点拓扑检查" + _curlyrname + ".txt";
                var r = TextFileWriter.SaveFile(tf, _sbResult.ToString(), autoOver);

                if (r.stat != ResultState.Ok) return r;

                var shpWriter = new ShapeFileWriter();

                var shpdir = dir + "\\errShp";
                if (!Directory.Exists(shpdir))
                    Directory.CreateDirectory(shpdir);

                var fdic = new Dictionary<string, int>() { { "检查项", 40 }, { "图层名", 20 }, { "说明", 40 } };

                var sf = shpdir + "\\点拓扑检查" + _curlyrname;
                shpWriter.createErrorResutSHPFile(sf, _spatialReference,
                    esriGeometryType.esriGeometryPoint, null, autoOver);

                for (int i = 0; i < _erpPoints.Count; i++)//dicList
                {
                    shpWriter.addErrorGeometry(_erpPoints[i], new Dictionary<string, string>()
                    {
                        { "检查项", "点拓扑检查" },{ "图层名", _curlyrname }, { "说明", "未穿越要素点" }
                    });
                }

                shpWriter.saveErrorResutSHPFile();

                r.info = new[] { tf, sf };
                return r;
            }

            return new ResultMessage { stat = ResultState.Cancel, msg = "检查出0条" };
        }


        private void SaveToGDB()
        {
            var fcls = ResultGDBWriter.GetPointFC(_spatialReference);
            var cursor = fcls.Insert(true);
            var n = 0;
            for (var t = 0; t < _erpPoints.Count; t++)
            {
                ResultGDBWriter.addErrorGeometry(fcls, cursor, _erpPoints[t],
                    new Dictionary<string, string> { { "检查内容", "点拓扑检查" }, { "图层", _curlyrname }, { "错误描述", "未穿越要素点" } });
                n++;
                if (n > 5000)
                {
                    cursor.Flush();
                    n = 0;
                }

            }
            cursor.Flush();
            Marshal.ReleaseComObject(cursor);
            Marshal.ReleaseComObject(fcls);
        }


        public void Dispose()
        {
            _erpPoints.Clear();
            _sbResult.Clear();
            _spatialReference = null;
        }

       
    }
}
