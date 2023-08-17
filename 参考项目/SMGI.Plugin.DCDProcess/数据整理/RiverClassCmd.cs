using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Controls;
using SMGI.Common;
using SMGI.Common.Algrithm;

namespace SMGI.Plugin.DCDProcess.DataProcess
{
    //河流选取等级赋值（字段名：GRADE2）
    //参考配置表：规则配置.mdb\河流选取等级赋值_5W
    //2022.3.3 修改 张怀相
    //功能修改：
    //  1、启用编辑后才可用
    public class RiverClassCmd : SMGI.Common.SMGICommand
    {
        private IMap pMap = null;
        private IActiveView pActiveView = null;

        private IWorkspace pworkspace;
        const int MaxLevel = 10;
        string referenceScale = "";
        
        public RiverClassCmd()
        {

            m_caption = "河流选取等级赋值";
            m_category = "数据整理";
            m_toolTip = "根据规则对自然河流进行选取等级（GRADE2）赋值";
        }
        
        public override bool Enabled
        {
            get
            {
                return m_Application != null
                    && m_Application.Workspace != null
                    && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            if (m_Application.MapControl.Map.ReferenceScale == 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;
            }
           
            if (m_Application.MapControl.Map.ReferenceScale == 1000000)
            {
                referenceScale = "100W";
            }
            else if (m_Application.MapControl.Map.ReferenceScale == 500000)
            {
                referenceScale = "50W";
            }
            else if (m_Application.MapControl.Map.ReferenceScale == 250000)
            {
                referenceScale = "25W";
            }
            else if (m_Application.MapControl.Map.ReferenceScale == 100000)
            {
                referenceScale = "10W";
            }
            else if (m_Application.MapControl.Map.ReferenceScale == 50000)
            {
                referenceScale = "5W";
            }
            else if (m_Application.MapControl.Map.ReferenceScale == 10000)
            {
                referenceScale = "1W";
            }
            else
            {
                MessageBox.Show(string.Format("未找到当前设置的参考比例尺【{0}】对应的规则表!", m_Application.MapControl.Map.ReferenceScale));
                return;
            }



            pMap = m_Application.ActiveView as IMap;
            pActiveView = m_Application.ActiveView;
            IFeatureClass fc;
            
            IMap map = pMap;

            ILayer hydlLayer = null;
            FrmRiverClass hydroValueSet = new FrmRiverClass(pMap);
            if (hydroValueSet.ShowDialog() == DialogResult.OK)
            {
                hydlLayer = hydroValueSet.hydlLayer;
            }
            else
            {
                return;
            }

            if (hydlLayer == null)
            {
                return;
            }
            fc = (hydlLayer as IFeatureLayer).FeatureClass;

            //读取几何分级表li
            Dictionary<double, int> len2Grade = readRiverGradeDB();
            if (len2Grade.Count == 0)
            {
                MessageBox.Show(string.Format("当前比例尺【{0}】对应的规则表内容为空!", m_Application.MapControl.Map.ReferenceScale));
                return;
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
             
            using (WaitOperation wo = GApplication.Application.SetBusy())
            {
                HydroAlgorithm.HydroGraph hydroGraph = new HydroAlgorithm.HydroGraph();

                IQueryFilter pQueryFilter = new QueryFilterClass();
                pQueryFilter.WhereClause = "hgb<210400";//自然河流（必须在水系结构线的HGB赋值后进行）
                IFeatureCursor pFeatureCursor = (hydlLayer as IFeatureLayer).Search(pQueryFilter, true);
                int feaCount = (hydlLayer as IFeatureLayer).FeatureClass.FeatureCount(pQueryFilter);
                IFeature pFeature = null;
                while ((pFeature = pFeatureCursor.NextFeature()) != null)
                {
                    wo.SetText("正在读取数据【" + pFeature.OID.ToString() + "】......");
                    hydroGraph.Add(pFeature);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);

                wo.SetText("正在获取Path列表......");
                var hydroPaths = hydroGraph.BuildHydroPaths(wo);

                wo.SetText("正在计算河流总长度......");
                double allHydroLength = 0;
                foreach (var path in hydroPaths)
                {
                    path.CalLength();
                    allHydroLength += path.Lenght;
                }

                wo.SetText("正在对河流长度排序......");
                hydroPaths.Sort();

                //double leftLength = allHydroLength;
                //int currentLevel = MaxLevel;
                //double currentLevelLength = allHydroLength * (Math.Sqrt((double)currentLevel / ((double)MaxLevel + 1.0)));

                //li
                int currentLevel = len2Grade.First().Value;
                double currentLevelLength = len2Grade.First().Key;
                //下一级别
                var kv = len2Grade.First(o => o.Key > currentLevelLength);
                int currentLevel2 = kv.Value;
                double currentLevelLength2 = kv.Key;

                foreach (var path in hydroPaths)
                {
                    //li
                    if (path.Lenght > currentLevelLength2)
                    {
                        currentLevel = currentLevel2;
                        currentLevelLength = currentLevelLength2;

                        if (currentLevelLength < len2Grade.Last().Key)
                        {
                            kv = len2Grade.First(o => o.Key > currentLevelLength);
                            currentLevel2 = kv.Value;
                            currentLevelLength2 = kv.Key;
                        }
                        else
                        {
                            currentLevelLength2 = double.MaxValue;
                        }


                    }

                    if (path.Lenght >= currentLevelLength)
                    {
                        foreach (var edge in path.Edges)
                        {
                            IFeature currentFeature = edge.Feature;
                            wo.SetText("正在处理要素【" + currentFeature.OID.ToString() + "】/【" + feaCount.ToString() + "】...");
                            currentFeature.set_Value(currentFeature.Fields.FindField("grade2"), currentLevel);
                            currentFeature.set_Value(currentFeature.Fields.FindField("pathid"), path.Index);
                            currentFeature.Store();
                            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(currentFeature);
                        }

                        //leftLength -= path.Lenght;

                        //if (leftLength < currentLevelLength)
                        //{
                        //    currentLevel--;
                        //    currentLevelLength = allHydroLength * (Math.Sqrt((double)currentLevel / ((double)MaxLevel + 1.0)));

                        //}


                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);
                wo.Dispose();
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
            
            MessageBox.Show("分级完毕！用时：\n" + elapsedTime);


        }

        /// <summary>
        /// 按长度值从小到大排序
        /// </summary>
        /// <returns></returns>
        public Dictionary<double, int> readRiverGradeDB()
        {
            Dictionary<double, int> len2Grade = new Dictionary<double, int>();


            string mdbFileName = GApplication.Application.Template.Root + @"\规则配置.mdb";
            string tableName = "河流选取等级赋值_" + referenceScale;
            DataTable dt = DCDHelper.ReadToDataTable(mdbFileName, tableName);
            if (dt == null)
            {
                return len2Grade;
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                for (int j = 0; j < dr.Table.Columns.Count; j++)
                {
                    object val = dr[j];
                    if (val == null || Convert.IsDBNull(val))
                        dr[j] = 0;
                }

                double len = 0;
                int grade = 0;
                int.TryParse(dr["等级"].ToString().Trim(), out grade);
                double.TryParse(dr["长度"].ToString().Trim(), out len);

                len2Grade.Add(len, grade);
            }

            //按长度值从小到大排序
            return len2Grade.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
        }

    }
}
