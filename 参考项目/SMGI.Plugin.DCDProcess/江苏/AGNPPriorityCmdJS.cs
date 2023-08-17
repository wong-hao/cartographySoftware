using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using SMGI.Common;
using System.Data;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 地名点优先级赋值工具（江苏）
    /// 根据地名点的密度和权重，对地名点的Priority字段进行赋值
    /// 详细请参考文档《20180604江苏省地图数据库生产系统--功能实现要点.docx》
    /// 修改（20190124）：详细参考需求文档《20190110修改意见.docx》
    /// </summary>
    public class AGNPPriorityCmdJS : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            string lyrName = "AGNP";
            var layer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
                        ((l as IGeoFeatureLayer).Name.Trim().ToUpper() == lyrName)).FirstOrDefault() as IFeatureLayer;
            if (layer == null)
            {
                MessageBox.Show(string.Format("当前工作空间中没有找到图层【{0}】!", lyrName));
                return;
            }
            var agnpFC = layer.FeatureClass;

            if (MessageBox.Show("是否确定重新为地名点的优先等级赋值？", "提示", MessageBoxButtons.YesNo) != DialogResult.Yes) 
                return;

            string priorityFN = m_Application.TemplateManager.getFieldAliasName("PRIORITY", agnpFC.AliasName);
            int priorityIndex = agnpFC.FindField(priorityFN);
            if (priorityIndex == -1)
            {
                MessageBox.Show(string.Format("图层【{0}】中没有找到字段【{1}】!", layer.Name, priorityFN));
                return;
            }

            string classFN = m_Application.TemplateManager.getFieldAliasName("CLASS", agnpFC.AliasName);
            int classIndex = agnpFC.FindField(classFN);
            if (classIndex == -1)
            {
                MessageBox.Show(string.Format("图层【{0}】中没有找到字段【{1}】!", layer.Name, classFN));
                return;
            }

            string dbPath = m_Application.Template.Root + @"\规则配置.mdb";
            string tableName = "地名点优先级分类权重";
            DataTable weightDataTable = DCDHelper.ReadToDataTable(dbPath, tableName);
            if (weightDataTable == null)
            {
                return;
            }

            tableName = "地名点优先级赋值";
            DataTable priorityDataTable = DCDHelper.ReadToDataTable(dbPath, tableName);
            if (priorityDataTable == null)
            {
                return;
            }

            //地名点权重映射表
            Dictionary<string, int> class2weight = new Dictionary<string, int>();
            //优先级赋值规则表
            Dictionary<int, AGNPPriorityRule> priority2Rule = new Dictionary<int, AGNPPriorityRule>();

            //权重
            for (int i = 0; i < weightDataTable.Rows.Count; i++)
            {
                string calssValue = weightDataTable.Rows[i]["分类码"].ToString();
                int weight = Convert.ToInt32(weightDataTable.Rows[i]["权重值"]);

                class2weight.Add(calssValue, weight);

            }

            //面积阀值
            for (int i = 0; i < priorityDataTable.Rows.Count; i++)
            {
                int priority = Convert.ToInt32(priorityDataTable.Rows[i]["地名点优先级等级"]);
                string allSelSQL = priorityDataTable.Rows[i]["全部选取条件"].ToString();
                string partSelSQL = priorityDataTable.Rows[i]["部分选取条件"].ToString();
                string conTinSQL = priorityDataTable.Rows[i]["参与构网条件"].ToString();
                double scale = Convert.ToDouble(priorityDataTable.Rows[i]["等级总数所占比例"]);//该等级及以上所占比例参考值（全部选取的数量不受限制）

                AGNPPriorityRule rule = new AGNPPriorityRule(allSelSQL, partSelSQL, conTinSQL, scale);

                priority2Rule.Add(priority, rule);
            }


            try
            {
                m_Application.EngineEditor.StartOperation();


                using (WaitOperation wo = GApplication.Application.SetBusy())
                {
                    PriorityAssignment(agnpFC, class2weight, priority2Rule, wo);
                }

                m_Application.EngineEditor.StopOperation("地名点优先级赋值");

                MessageBox.Show(string.Format("赋值完成")); 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                m_Application.EngineEditor.AbortOperation();

                MessageBox.Show(ex.Message);
            }

            
        }

        private void PriorityAssignment(IFeatureClass ptFC, Dictionary<string, int> class2weight,
            Dictionary<int, AGNPPriorityRule> priority2Rule, WaitOperation wo = null)
        {
            string priorityFN = m_Application.TemplateManager.getFieldAliasName("PRIORITY", ptFC.AliasName);
            int priorityIndex = ptFC.FindField(priorityFN);

            string classFN = m_Application.TemplateManager.getFieldAliasName("CLASS", ptFC.AliasName);
            int classIndex = ptFC.FindField(classFN);

            //按地名优先级降序（数值升序，级别数字越小，级别越高，1,2,3,4,5,6,7,8,9）排列，以便先为高等级地名点优先级赋值
            priority2Rule = priority2Rule.OrderBy(o => o.Key).ToDictionary(p => p.Key, o => o.Value);

            int totalCount = 0;
            //将所有地名点的优先级初始化为空值
            IFeatureCursor feCursor = ptFC.Search(null, false);
            IFeature fe = null;
            while ((fe = feCursor.NextFeature()) != null)
            {
                //重置为空值
                fe.set_Value(priorityIndex, DBNull.Value);
                fe.Store();

                totalCount++;

                System.Runtime.InteropServices.Marshal.ReleaseComObject(fe);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);

            Dictionary<int, int> oid2AssignedPriority = new Dictionary<int,int>(totalCount);//已赋值要素
            IQueryFilter qf = new QueryFilterClass();
            foreach (var kv in priority2Rule)//先高级别，后低级别
            {
                string tipText = string.Format("正在进行地名点优先等级【{0}】的赋值", kv.Key);

                int priority = kv.Key;
                AGNPPriorityRule rule = kv.Value;

                if (wo != null)
                    wo.SetText(string.Format("{0}:全部选取要素赋值......", tipText));
                #region 1.全部选取要素赋值
                qf.WhereClause = "";
                if(rule.AllSelectSQL != "")
                    qf.WhereClause = string.Format("({0})", rule.AllSelectSQL);
                if (ptFC.HasCollabField())
                {
                    if (qf.WhereClause != "")
                        qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;
                    else
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                }

                feCursor = ptFC.Search(qf, false);
                while ((fe = feCursor.NextFeature()) != null)
                {
                    if (oid2AssignedPriority.ContainsKey(fe.OID))
                        continue;//已赋较高等级，跳过

                    //全部选取要素直接赋值
                    fe.set_Value(priorityIndex, priority);
                    fe.Store();

                    oid2AssignedPriority.Add(fe.OID, priority);

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fe);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);

                #endregion

                int maxCount = (int)(totalCount * rule.Scale) + 1;
                if (oid2AssignedPriority.Count > maxCount)
                    continue;//该等级及以上的点已超出规则里的比例设置，则跳转到下一低级别的赋值

                if (wo != null)
                    wo.SetText(string.Format("{0}:部分选取要素赋值......", tipText));
                #region 2.部分选取要素赋值（通过构建Tin的方式）

                #region 2.1获取参与部分选取要素相关信息
                qf.WhereClause = "";
                if(rule.PartSelectSQL != "")
                    qf.WhereClause = string.Format("({0})", rule.PartSelectSQL);
                if (ptFC.HasCollabField())
                {
                    if (qf.WhereClause != "")
                        qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;
                    else
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                }

                Dictionary<int, string> fid2class = new Dictionary<int, string>(ptFC.FeatureCount(qf));//参与部分选取的要素及其对应的类型分类码
                feCursor = ptFC.Search(qf, true);
                while ((fe = feCursor.NextFeature()) != null)
                {
                    string classValue = fe.get_Value(classIndex).ToString();

                    fid2class.Add(fe.OID, classValue);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);
                #endregion

                if (fid2class.Count == 0)
                    continue;//没有满足条件的要素参与部分选取，则跳转到下一低级别的赋值

                if (oid2AssignedPriority.Count + fid2class.Count <= maxCount)//总数小于阈值，参与部分选取的要素直接赋值
                {
                    foreach (var kv2 in fid2class)
                    {
                        IFeature f = ptFC.GetFeature(kv2.Key);//要素

                        //等级属性赋值
                        f.set_Value(priorityIndex, priority);
                        f.Store();

                        oid2AssignedPriority.Add(f.OID, priority);

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(f);
                    }

                    continue;
                }

                TinClass tin = new TinClass();
                Dictionary<int, double> oid2Area = new Dictionary<int, double>(fid2class.Count());//每个节点影响面积(添加权重后)
                #region 2.2构建三角网,计算TIN节点密度
                qf.WhereClause = "";
                if(rule.ConstructTinSQL != "")
                    qf.WhereClause = string.Format("({0})", rule.ConstructTinSQL);
                if (ptFC.HasCollabField())
                {
                    if (qf.WhereClause != "")
                        qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;
                    else
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                }

                feCursor = ptFC.Search(qf, true);
                Dictionary<int, IPoint> FID2Point = new Dictionary<int, IPoint>();//要素ID->几何
                MultipointClass mp = new MultipointClass();
                while ((fe = feCursor.NextFeature()) != null)
                {
                    IPoint shape = fe.ShapeCopy as IPoint;
                    if (shape == null || shape.IsEmpty)
                        continue;//空几何不参与

                    FID2Point.Add(fe.OID, shape);
                    mp.AddGeometry(shape);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);

                
                tin.InitNew(mp.Envelope);
                tin.StartInMemoryEditing();

                IPointCollection pc = mp as IPointCollection;
                foreach (var item in FID2Point)
                {
                    IPoint p = item.Value;
                    p.Z = 0;

                    tin.AddPointZ(p, item.Key);//如果三角网中该点几何已经存在（数据中存在重叠点），则该点不会添加到三角网中，即不会参与后期的属性值更新
                }

                IPolygon tinDataArea = tin.GetDataArea();//TIN的数据范围
                IRelationalOperator ro = tinDataArea as IRelationalOperator;
                for (int j = 1; j <= tin.NodeCount; j++)
                {
                    ITinNode node = tin.GetNode(j);
                    if (!node.IsInsideDataArea)
                        continue;

                    IFeature f = ptFC.GetFeature(node.TagValue);//要素
                    if (!fid2class.ContainsKey(f.OID))
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(f);
                        continue;//该要素为非部分选取要素，直接跳过不赋值
                    }

                    string classValue = f.get_Value(classIndex).ToString();
                    int weight = class2weight[classValue];//节点权重

                    //求节点在范围内的影响面积(添加权重后)
                    IPolygon voronoiPolygon = node.GetVoronoiRegion(null);
                    double nodeArea = 0;//添加权重后的节点影响面积
                    if (!ro.Contains(voronoiPolygon))//节点影响范围部分超出了tin的数据范围，取两者相交的公共部分的面积
                    {
                        IPolygon interGeo = (tinDataArea as ITopologicalOperator).Intersect(voronoiPolygon as IGeometry, esriGeometryDimension.esriGeometry2Dimension) as IPolygon;
                        if (interGeo != null)
                        {
                            nodeArea = (interGeo as IArea).Area * weight;
                        }
                    }
                    else
                    {
                        nodeArea = (voronoiPolygon as IArea).Area * weight;
                    }

                    oid2Area.Add(f.OID, nodeArea);

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(f);
                }
                #endregion

                #region 2.3按影响面积降序排列,并进行优先级赋值
                oid2Area = oid2Area.OrderByDescending(o => o.Value).ToDictionary(p => p.Key, o => o.Value);
                //oid2Area = (from o in oid2Area orderby o.Value descending select o).ToDictionary(p => p.Key, p => p.Value);
                foreach (var kv2 in oid2Area)
                {
                    if (oid2AssignedPriority.Count() > maxCount)
                        break;//该等级及以上的点已超出规则里的比例设置,本级别赋值完成

                    IFeature f = ptFC.GetFeature(kv2.Key);//要素

                    //等级属性赋值
                    f.set_Value(priorityIndex, priority);
                    f.Store();

                    oid2AssignedPriority.Add(f.OID, priority);

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(f);
                }
                #endregion

                tin = null;
                fid2class = null;
                oid2Area = null;
                GC.Collect();

                #endregion
            }
        }
    }

    /// <summary>
    /// 地名点优先级选取规则
    /// </summary>
    public class AGNPPriorityRule
    {
        /// <summary>
        /// 本地名优先级别全部选取（直接赋值）的SQL条件
        /// </summary>
        public string AllSelectSQL { get; set; }

        /// <summary>
        /// 通过构建三角网及本级别的面积阈值（带权重），从满足该SQL条件的地名点中选择部分要素进行赋值
        /// </summary>
        public string PartSelectSQL { get; set; }

        /// <summary>
        /// 部分选取时，参与构建Tin的要素筛选SQL条件
        /// </summary>
        public string ConstructTinSQL { get; set; }

        /// <summary>
        /// 本级别及以上所占总数比例的参考值（全部选取的数量不受限制）
        /// </summary>
        public double Scale { get; set; }


        public AGNPPriorityRule(string allSelSQL, string partSelSQL, string conTinSQL, double scale)
        {
            AllSelectSQL = allSelSQL;
            PartSelectSQL = partSelSQL;
            ConstructTinSQL = conTinSQL;
            Scale = scale;
        }
    }

}
