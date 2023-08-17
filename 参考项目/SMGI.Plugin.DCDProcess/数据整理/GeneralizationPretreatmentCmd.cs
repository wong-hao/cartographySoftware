using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 缩编预处理：对大比例尺成果库进行如下处理：
    /// 1.删除低等级的水系、道路；
    /// 2.对水系和道路层进行伪节点处理；
    /// 3.线化简。
    /// </summary>
    public class GeneralizationPretreatmentCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace == null;
            }
        }

        public override void OnClick()
        {
            GeneralizationPretreatmentFrm frm = new GeneralizationPretreatmentFrm();
            frm.Text = "缩编预处理";
            if (DialogResult.OK == frm.ShowDialog())
            {
                string orginDBFileName = frm.GDBFilePath;
                string riverFCName = frm.RiverFeatureClassName;
                string riverDelSqlText = frm.RiverDelSQLText;
                List<string> riverFieldArray = frm.RiverFieldArray;
                string roadFCName = frm.RoadFeatureClassName;
                string roadDelSqlText = frm.RoadDelSQLText;
                List<string> roadFieldArray = frm.RoadFieldArray;
                double blendVal = frm.BlendValue;
                double smoothVal = frm.SmoothValue;
                List<string> simplifiedFCNames = frm.NeedSimplifiedFCNameList;

                try
                {
                    using (var wo = m_Application.SetBusy())
                    {
                        wo.SetText(string.Format("正在读取数据库......"));

                        IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactoryClass();
                        IFeatureWorkspace fWS = workspaceFactory.OpenFromFile(orginDBFileName, 0) as IFeatureWorkspace;
                        IFeatureClass hydlFC = fWS.OpenFeatureClass(riverFCName);
                        IFeatureClass lrdlFC = fWS.OpenFeatureClass(roadFCName);

                        #region 低等级水系处理
                        if (riverDelSqlText != "")
                        {
                            IQueryFilter qf = new QueryFilterClass();
                            qf.WhereClause = riverDelSqlText;

                            if (hydlFC.FeatureCount(qf) > 0)
                            {
                                wo.SetText(string.Format("正在删除要素类【{0}】中的低等级要素......", hydlFC.AliasName));

                                List<IPoint> delPointList = new List<IPoint>();

                                IFeatureCursor feCursor = hydlFC.Search(qf, true);
                                IFeature fe = null;
                                while ((fe = feCursor.NextFeature()) != null)
                                {
                                    IPolyline pl = fe.Shape as IPolyline;
                                    if (pl == null || pl.IsEmpty)
                                        continue;

                                    delPointList.Add(pl.FromPoint);
                                    delPointList.Add(pl.ToPoint);
                                }
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);

                                //1.删除低等级河流
                                (hydlFC as ITable).DeleteSearchedRows(qf);

                                //2.伪节点处理(仅处理删除低等级河流后产生的伪节点)
                                wo.SetText(string.Format("正在对要素类【{0}】进行伪节点处理......", hydlFC.AliasName));
                                PseudoCmd.PseudonodesProcess(hydlFC, riverFieldArray);

                            }
                        }

                        #endregion

                        #region 低等级道路处理
                        if (roadDelSqlText != "")
                        {
                            IQueryFilter qf = new QueryFilterClass();
                            qf.WhereClause = roadDelSqlText;

                            if (lrdlFC.FeatureCount(qf) > 0)
                            {
                                wo.SetText(string.Format("正在删除要素类【{0}】中的低等级要素......", lrdlFC.AliasName));

                                List<IPoint> delPointList = new List<IPoint>();

                                IFeatureCursor feCursor = lrdlFC.Search(qf, true);
                                IFeature fe = null;
                                while ((fe = feCursor.NextFeature()) != null)
                                {
                                    IPolyline pl = fe.Shape as IPolyline;
                                    if (pl == null || pl.IsEmpty)
                                        continue;

                                    delPointList.Add(pl.FromPoint);
                                    delPointList.Add(pl.ToPoint);
                                }
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);

                                //1.删除低等级道路
                                (lrdlFC as ITable).DeleteSearchedRows(qf);

                                //2.伪节点处理(仅处理删除低等级道路后产生的伪节点)
                                wo.SetText(string.Format("正在对要素类【{0}】进行伪节点处理......", lrdlFC.AliasName));
                                PseudoCmd.PseudonodesProcess(lrdlFC, roadFieldArray);
                            }
                        }

                        #endregion

                        #region 线化简
                        foreach (var fcName in simplifiedFCNames)
                        {
                            IFeatureClass fc = fWS.OpenFeatureClass(fcName);

                            wo.SetText(string.Format("正在对要素类【{0}】中的要素进行化简......", fc.AliasName));
                            PolylineGeneralizeCmd.Generalize(fc, "", "BEND_SIMPLIFY", blendVal, true, "PAEK", smoothVal, false, wo);
                        }
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    MessageBox.Show(ex.Message);
                    return;
                }
                
                

                if (MessageBox.Show("是否加载数据库到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    IWorkspace ws = GApplication.GDBFactory.OpenFromFile(orginDBFileName, 0);
                    if (GWorkspace.IsWorkspace(ws))
                    {
                        m_Application.OpenESRIWorkspace(ws);
                    }
                    else
                    {
                        m_Application.InitESRIWorkspace(ws);
                    }
                }
            }
        }


    }
}
