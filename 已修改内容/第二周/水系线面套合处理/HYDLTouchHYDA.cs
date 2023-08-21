using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.CartographyTools;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataManagementTools;
using System.IO;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 维护河流线与水系结构线与面装水系的关系(GB < 240200(溢洪道))
    /// </summary>
    public class HYDLTouchHYDA
    {
        private const double _pseudoDistance = 0.001;//米

        //临时数据库
        private IWorkspace _ws;
        IFeatureClass _tempFC;
        IFeatureClass _dissolveFC;


        public HYDLTouchHYDA()
        {
            _ws = null;
            _tempFC = null;
            _dissolveFC = null;

        }

        /// <summary>
        /// 1.建立临时面图层，融合符合要求的面要素（消除邻接水系面的公共面）
        /// 1.遍历所有的水系线要素；
        /// 2.求与该水系线有相交的所有面要素；
        /// 3.求该水系线与其相交面的所有交点;
        /// 4.对该水系面在所有交点处打断，并找出不合理线（没有别任何水系面完全包含的水系结构线，或位于水系面内的非水系结构线）
        /// 5.处理不合理线：若该线另一端点处仅与一个线要素线邻，则将该要素合并到相邻线要素，否则由作业人员判断并处理.
        /// 说明：该方法不检测完全包含在水系面内的非水系结构线，也不检测完全相离与水系面内的水系架构线
        /// </summary>
        /// <param name="hydaFC"></param>
        /// <param name="hydlFC"></param>
        /// <param name="resultSHPFileName"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public string Process(IFeatureClass hydaFC, IFeatureClass hydlFC, string resultSHPFileName = "", WaitOperation wo = null)
        {
            string err = "";

            
            try
            {
                ShapeFileWriter resultFile = null;
                if (resultSHPFileName != "")
                {
                    resultFile = new ShapeFileWriter();
                    Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                    fieldName2Len.Add("原始水系线OID", 20);
                    fieldName2Len.Add("水系线OID", 20);
                    fieldName2Len.Add("处理情况", 40);
                    resultFile.createErrorResutSHPFile(resultSHPFileName, (hydaFC as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryMultipoint, fieldName2Len);
                }

                int gbIndex = hydlFC.FindField("GB");
                if (gbIndex == -1)
                {
                    err = string.Format("水系线图层【{0}】中缺少GB字段！", hydlFC.AliasName);
                    return err;
                }

                string filter = "GB < 240200";//溢洪道和沼泽等不参与
                if (hydlFC.FindField(cmdUpdateRecord.CollabVERSION) != -1)
                    filter += string.Format(" and ({0} <> {1} or {2} is null)", cmdUpdateRecord.CollabVERSION, cmdUpdateRecord.DeleteState, cmdUpdateRecord.CollabVERSION);

                #region 建立水系面的临时图层
                //创建临时面要素类
                if (wo != null)
                    wo.SetText(string.Format("【{0}】正在创建临时类...", hydaFC.AliasName + "_temp"));

                string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
                _ws = DCDHelper.createTempWorkspace(fullPath);
                IFeatureWorkspace fws = _ws as IFeatureWorkspace;
                _tempFC = DCDHelper.CreateFeatureClassStructToWorkspace(fws, hydaFC, hydaFC.AliasName + "_temp");
                DCDHelper.CopyFeaturesToFeatureClass(hydaFC, new QueryFilterClass { WhereClause = filter }, _tempFC, false);

                //融合图层所有面要素（消除面之间的公共边）
                Geoprocessor gp = new Geoprocessor();
                gp.OverwriteOutput = true;

                Dissolve diss = new Dissolve();
                diss.in_features = _tempFC;
                diss.out_feature_class = _ws.PathName + "\\" + hydaFC.AliasName + "_temp_融合";
                diss.multi_part = "SINGLE_PART";
                gp.Execute(diss, null);

                //打开融合后的临时数据
                _dissolveFC = fws.OpenFeatureClass(hydaFC.AliasName + "_temp_融合");
                #endregion

                //遍历HYDL,获取所有需检测的要素OID集合
                List<int> hydlOIDList = new List<int>();
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = filter;
                IFeatureCursor hydlCursor = hydlFC.Search(qf, true);
                IFeature hydlFe = null;
                while ((hydlFe = hydlCursor.NextFeature()) != null)
                {
                    hydlOIDList.Add(hydlFe.OID);
                }
                Marshal.ReleaseComObject(hydlCursor);

                long mem = 0;
                long maxMem = 1024 * 1024 * 700; //字节   默认初始字节为900
                for(int i = 0; i < hydlOIDList.Count; ++i)//遍历并处理线状水系
                {
                    hydlFe = hydlFC.GetFeature(hydlOIDList[i]);

                    if (wo != null)
                        wo.SetText(string.Format("正在处理线状水系【{0}】......", hydlFe.OID));

                    long hydlFeGB;
                    long.TryParse(hydlFe.get_Value(gbIndex).ToString(), out hydlFeGB);
                    IPolyline hydlShape = hydlFe.Shape as IPolyline;

                    #region 求水系线相交的所有面要素
                    List<IFeature> interHydaFeList = new List<IFeature>();

                    ISpatialFilter pSpatialFilter = new SpatialFilterClass();
                    pSpatialFilter.Geometry = hydlShape;
                    pSpatialFilter.GeometryField = "SHAPE";
                    pSpatialFilter.WhereClause = "";
                    pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    IFeatureCursor hydaCursor = _dissolveFC.Search(pSpatialFilter, false);
                    IFeature hydaFe = null;
                    while ((hydaFe = hydaCursor.NextFeature()) != null)
                    {
                        interHydaFeList.Add(hydaFe);
                    }
                    Marshal.ReleaseComObject(hydaCursor);
                    #endregion

                    #region 求该水系线与水系面的所有交点
                    List<IPoint> interPoints = new List<IPoint>();

                    //求取该水系线与水系面的交点
                    foreach (var interHydaFe in interHydaFeList)
                    {
                        ITopologicalOperator2 pTopo = (ITopologicalOperator2)interHydaFe.Shape;
                        pTopo.IsKnownSimple_2 = false;
                        pTopo.Simplify();

                        IGeometry interGeo = pTopo.Intersect(hydlShape, esriGeometryDimension.esriGeometry0Dimension);
                        if (null == interGeo || true == interGeo.IsEmpty)
                            continue;

                        //排除端点
                        IPointCollection pPointColl = (IPointCollection)interGeo;
                        for (int j = 0; j < pPointColl.PointCount; ++j)
                        {
                            IPoint pt = pPointColl.get_Point(j);
                            IProximityOperator ProxiOP = pt as IProximityOperator;

                            //相交点是否是该要素的端点，若是该要素的端点，则跳过
                            if (ProxiOP.ReturnDistance(hydlShape.FromPoint) < _pseudoDistance * 10 || ProxiOP.ReturnDistance(hydlShape.ToPoint) < _pseudoDistance * 10)
                                continue;

                            interPoints.Add(pt);//非端点的交点
                        }
                    }

                    if (interPoints.Count == 0)
                        continue;

                    #endregion

                    #region 打断线要素,找到非法要素集合
                    List<IFeature> nonlicetFeList = new List<IFeature>();

                    //打断要素
                    List<IFeature> feList = LineBreak(hydlFe, interPoints);

                    //找不合法要素
                    foreach (var item in feList)
                    {
                        if (hydlFeGB == 210400)//水系结构线
                        {
                            //判断该段水系结构线是否包含在某一面内
                            bool bWithinPolygon = false;
                            foreach (var interHydaFe in interHydaFeList)
                            {
                                IRelationalOperator ro = interHydaFe.Shape as IRelationalOperator;
                                if (ro.Contains(item.Shape))
                                {
                                    //水系结构线包含于一个水系面内
                                    bWithinPolygon = true;
                                    break;
                                }
                            }

                            if (!bWithinPolygon)//非法要素
                            {
                                nonlicetFeList.Add(item);
                            }
                        }
                        else//非水系结构线
                        {
                            //判断该段非水系结构线是否在所有水系面外
                            foreach (var interHydaFe in interHydaFeList)
                            {
                                IRelationalOperator ro = interHydaFe.Shape as IRelationalOperator;
                                if (ro.Contains(item.Shape))
                                {
                                    nonlicetFeList.Add(item);

                                    break;
                                }
                            }
                        }
                    }

                    if (nonlicetFeList.Count == 0)
                        continue;
                    #endregion

                    #region 处理非法要素
                    foreach (var nonlicetFe in nonlicetFeList)
                    {
                        IPoint otherPt = null;
                        IPoint interPt = null;
                        IPolyline pl = nonlicetFe.Shape as IPolyline;
                        bool bFromPoint = false;
                        bool bToPoint = false;
                        foreach (var pt in interPoints)
                        {
                            if (pt.Compare(pl.FromPoint) == 0)
                            {
                                bFromPoint = true;
                            }

                            if (pt.Compare(pl.ToPoint) == 0)
                            {
                                bToPoint = true;
                            }
                        }

                        if (bFromPoint)
                        {
                            if (bToPoint)
                            {
                                //该非法要素两端都是交点
                                if (hydlFeGB != 210400)//非水系结构线的不合法要素，则说明其被一个水系面所包含，可修改其GB为水系结构线
                                {
                                    nonlicetFe.set_Value(gbIndex, 210400);
                                    nonlicetFe.Store();


                                    //记录处理情况
                                    if (resultFile != null)
                                    {
                                        IPointCollection errGeo = new MultipointClass();
                                        errGeo.AddPoint(pl.FromPoint);
                                        
                                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                        fieldName2FieldValue.Add("原始水系线OID", string.Format("{0}", hydlFe.OID));
                                        fieldName2FieldValue.Add("水系线OID", string.Format("{0}", nonlicetFe.OID));
                                        fieldName2FieldValue.Add("处理情况", string.Format("{0}", "已修改属性（GB）"));

                                        resultFile.addErrorGeometry(errGeo as IGeometry, fieldName2FieldValue);
                                    }
                                }
                                else//水系结构线的不合法要素,则说明该要素不被任何一个面要素所包含，而两端又都是交点，此种情况需要作业人员手动去做处理
                                {
                                    //记录该要素，交由作业人员判断处理
                                    if (resultFile != null)
                                    {
                                        IPointCollection errGeo = new MultipointClass();
                                        errGeo.AddPoint(pl.FromPoint);

                                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                        fieldName2FieldValue.Add("原始水系线OID", string.Format("{0}", hydlFe.OID));
                                        fieldName2FieldValue.Add("水系线OID", string.Format("{0}", nonlicetFe.OID));
                                        fieldName2FieldValue.Add("处理情况", string.Format("{0}", "未修改属性，两端要素的属性没有参考意义"));

                                        resultFile.addErrorGeometry(errGeo as IGeometry, fieldName2FieldValue);
                                    }
                                }

                                continue;

                            }
                            else
                            {
                                interPt = pl.FromPoint;
                                otherPt = pl.ToPoint;
                            }
                             
                        }
                        else//至少可以保证其中一个端点是交点
                        {
                            otherPt = pl.FromPoint;
                            interPt = pl.ToPoint;
                        }

                        //非法要素的另一端是否有其他要素
                        ISpatialFilter pFilter = new SpatialFilterClass();
                        pFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        pFilter.WhereClause = filter + string.Format("and OBJECTID <> {0}", nonlicetFe.OID);
                        pFilter.GeometryField = hydlFC.ShapeFieldName;
                        pFilter.Geometry = otherPt;

                        List<IFeature> touchesFeList = new List<IFeature>();
                        IFeatureCursor pInterCursor = hydlFC.Search(pFilter, false);
                        IFeature interFe = null;
                        while ((interFe = pInterCursor.NextFeature()) != null)
                        {
                            IPolyline interLine = interFe.Shape as IPolyline;
                            ESRI.ArcGIS.Geometry.IPoint interFrom = interLine.FromPoint;
                            ESRI.ArcGIS.Geometry.IPoint interTo = interLine.ToPoint;

                            IProximityOperator ProxiOP = otherPt as IProximityOperator;
                            if (ProxiOP.ReturnDistance(interFrom) < _pseudoDistance || ProxiOP.ReturnDistance(interTo) < _pseudoDistance)
                            {
                                touchesFeList.Add(interFe);
                            }
                        }
                        Marshal.ReleaseComObject(pInterCursor);

                        if (touchesFeList.Count == 1)
                        {
                            //合并要素
                            (touchesFeList[0].Shape as ITopologicalOperator2).Simplify();
                            (nonlicetFe.Shape as ITopologicalOperator2).Simplify();
                            touchesFeList[0].Shape = (touchesFeList[0].Shape as ITopologicalOperator).Union(nonlicetFe.Shape);
                            if (hydlFC.FindField(cmdUpdateRecord.CollabVERSION) != -1)
                            {
                                int idx = touchesFeList[0].Fields.FindField(cmdUpdateRecord.CollabVERSION);
                                int state = Int32.Parse(touchesFeList[0].get_Value(idx).ToSafeString());
                                if (state != cmdUpdateRecord.NewState)
                                    touchesFeList[0].set_Value(idx, cmdUpdateRecord.EditState);
                            }                            
                            touchesFeList[0].Store();
                            nonlicetFe.Delete();

                            //记录处理情况
                            if (resultFile != null)
                            {
                                IPointCollection errGeo = new MultipointClass();
                                errGeo.AddPoint(interPt);

                                Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                fieldName2FieldValue.Add("原始水系线OID", string.Format("{0}", hydlFe.OID));
                                fieldName2FieldValue.Add("水系线OID", string.Format("{0}", touchesFeList[0].OID));
                                fieldName2FieldValue.Add("处理情况", string.Format("{0}", "已做融合处理"));

                                resultFile.addErrorGeometry(errGeo as IGeometry, fieldName2FieldValue);
                            }
                        }
                        else if (touchesFeList.Count > 1)
                        {
                            //记录该要素，交由作业人员判断处理
                            if (resultFile != null)
                            {
                                IPointCollection errGeo = new MultipointClass();
                                errGeo.AddPoint(interPt);

                                Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                fieldName2FieldValue.Add("原始水系线OID", string.Format("{0}", hydlFe.OID));
                                fieldName2FieldValue.Add("水系线OID", string.Format("{0}", nonlicetFe.OID));
                                fieldName2FieldValue.Add("处理情况", string.Format("{0}", "未做融合处理，线在端点处有多条相邻要素"));

                                resultFile.addErrorGeometry(errGeo as IGeometry, fieldName2FieldValue);
                            }

                        }
                    }
                    #endregion

                    //手动释放内存
                    mem = Environment.WorkingSet;
                    if (mem > maxMem)
                    {
                        GC.Collect();
                    }

                }

                resultFile.saveErrorResutSHPFile();
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                err = ex.Message;
            }
            finally
            {
                DeleteTempFeatureClass();
            }

            return err;
        }

        private void DeleteTempFeatureClass()
        {
            if (_tempFC != null)
            {
                (_tempFC as IDataset).Delete();

                _tempFC = null;
            }

            if (_dissolveFC != null)
            {
                (_dissolveFC as IDataset).Delete();

                _dissolveFC = null;
            }

        }

        private List<IFeature> LineBreak(IFeature sourceFe, List<IPoint> interPoints)
        {
            List<IFeature> result = new List<IFeature>();

            List<IFeature> SplitedFeas = new List<IFeature>();
            SplitedFeas.Add(sourceFe);
            for (int i = 0; i < interPoints.Count; ++i)
            {
                foreach (var f in SplitedFeas)
                {
                    bool bCoola = true;
                    #region 协同
                    if (f.Fields.FindField(cmdUpdateRecord.CollabGUID) == -1)
                    {
                        bCoola = false;
                    }

                    string collGUID = "";
                    if (bCoola)
                    {
                        collGUID = f.get_Value(f.Fields.FindField(cmdUpdateRecord.CollabGUID)).ToString();
                    }

                    int smgiver = 0;
                    if (bCoola)
                    {
                        int.TryParse(f.get_Value(f.Fields.FindField(cmdUpdateRecord.CollabVERSION)).ToString(), out smgiver);
                    }

                    if (bCoola)
                    {
                        f.set_Value(f.Fields.FindField(cmdUpdateRecord.CollabVERSION), cmdUpdateRecord.NewState);//直接删除的标志
                    }
                    #endregion

                    try
                    {
                        IPoint interPt = interPoints[i];

                        ITopologicalOperator2 pTopo = (ITopologicalOperator2)f.Shape;
                        pTopo.IsKnownSimple_2 = false;
                        pTopo.Simplify();
                        IGeometry InterGeo = pTopo.Intersect(interPt, esriGeometryDimension.esriGeometry0Dimension);
                        if (null == InterGeo || true == InterGeo.IsEmpty)
                            continue;

                        ISet pFeatureSet = (f as IFeatureEdit).Split(interPt);
                        if (pFeatureSet != null)
                        {
                            pFeatureSet.Reset();

                            List<IFeature> flist = new List<IFeature>();
                            int maxIndex = -1;
                            double maxLen = 0;
                            while (true)
                            {
                                IFeature fe = pFeatureSet.Next() as IFeature;
                                if (fe == null)
                                {
                                    break;
                                }

                                if ((fe.Shape as IPolyline).Length > maxLen)
                                {
                                    maxLen = (fe.Shape as IPolyline).Length;
                                    maxIndex = flist.Count();
                                }
                                flist.Add(fe);

                                //新增要素
                                SplitedFeas.Add(fe);

                                //结果要素
                                result.Add(fe);

                            }

                            #region 协同，长的一段保持原GUID
                            if (cmdUpdateRecord.EnableUpdate)
                            {
                                for (int k = 0; k < flist.Count(); ++k)
                                {
                                    if (maxIndex == k)
                                    {
                                        if (smgiver >= 0 || smgiver == cmdUpdateRecord.EditState)
                                        {
                                            if (bCoola)
                                            {
                                                flist[k].set_Value(flist[k].Fields.FindField(cmdUpdateRecord.CollabVERSION), cmdUpdateRecord.EditState);
                                            }
                                        }

                                        if (bCoola)
                                        {
                                            flist[k].set_Value(flist[k].Fields.FindField(cmdUpdateRecord.CollabGUID), collGUID);//默认由最大的新要素继承原要素的collGUID
                                        }

                                        flist[k].Store();

                                        break;
                                    }
                                }
                            }
                            #endregion

                            //移除原要素
                            SplitedFeas.Remove(f);
                            if(result.Count > 0)//非原始要素（原始要素没有加入到result列表）
                                result.Remove(f);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.Message);
                        System.Diagnostics.Trace.WriteLine(ex.Source);
                        System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                        string err = ex.Message;
                    }

                }
            }


            return result;
        }
    }
}
