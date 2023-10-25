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
using System.Data;
using ESRI.ArcGIS.DataSourcesGDB;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// GUID新增检查：基于较大比例尺参考数据库，检查较小比例尺数据库的指定要素类中是否出现新增GUID的情况，《20190927【GUID新增检查】优化.docx》
    /// </summary>
    public class CheckNewCollabGUIDCmd : SMGICommand
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
            var frm = new CheckNewCollabGUIDForm(m_Application);
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Text = "GUID新增检查";
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string outputFileName;
            if (frm.CheckFeatureClassList.Count > 1)
            {
                outputFileName = OutputSetup.GetDir() + string.Format("\\{0}.shp", frm.Text);
            }
            else
            {
                outputFileName = OutputSetup.GetDir() + string.Format("\\{0}_{1}.shp", frm.Text, frm.CheckFeatureClassList.First().AliasName);
            }

            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                IWorkspace referWorkspace = null;//参考数据库
                #region 读取参考数据库
                wo.SetText("正在读取参考数据库......");
                var wsf = new FileGDBWorkspaceFactoryClass();
                if (!(Directory.Exists(frm.ReferGDB) && wsf.IsWorkspace(frm.ReferGDB)))
                {
                    MessageBox.Show("指定的参考数据库不合法!");
                    return;
                }
                referWorkspace = wsf.OpenFromFile(frm.ReferGDB, 0);
                #endregion

                Dictionary<IFeatureClass, IFeatureClass> fc2ReferFC = new Dictionary<IFeatureClass, IFeatureClass>();
                #region 收集与预处理待检查要素类相关信息
                wo.SetText("正在收集与预处理待检查要素类相关信息......");
                foreach (var fc in frm.CheckFeatureClassList)
                {
                    if (fc.FindField(cmdUpdateRecord.CollabGUID) == -1)
                    {
                        continue;
                    }

                    if (!(referWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fc.AliasName))
                    {
                        MessageBox.Show(string.Format("参考数据库中找不到要素类【{0}】!", fc.AliasName));
                        return;
                    }
                    IFeatureClass referFC = (referWorkspace as IFeatureWorkspace).OpenFeatureClass(fc.AliasName);

                    if (referFC.FindField(cmdUpdateRecord.CollabGUID) == -1)
                    {
                        MessageBox.Show(string.Format("参考数据库中的要素类【{0}】不存在字段【{1}】!", fc.AliasName, cmdUpdateRecord.CollabGUID));
                        return;
                    }

                    fc2ReferFC.Add(fc, referFC);
                }
                #endregion

                err = DoCheck(outputFileName, fc2ReferFC, wo);
            }

            if (err == "")
            {
                if (File.Exists(outputFileName))
                {
                    IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName);

                    if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                }
                else
                {
                    MessageBox.Show("检查完毕,没有发现异常！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }
        }

        public static string DoCheck(string resultSHPFileName, Dictionary<IFeatureClass, IFeatureClass> fc2ReferFC, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                ShapeFileWriter resultFile = null;


                foreach (var kv in fc2ReferFC)
                {
                    List<int> errOIDList = new List<int>();//错误结果

                    IFeatureClass fc = kv.Key;
                    IFeatureClass referFC = kv.Value;
                    int guidIndex = fc.FindField(cmdUpdateRecord.CollabGUID);
                    if (guidIndex == -1)
                    {
                        throw new Exception(string.Format("当前工作空间中的要素类【{0}】找不到GUID字段【{1}】", fc.AliasName, cmdUpdateRecord.CollabGUID));
                    }
                    int referGUIDIndex = referFC.FindField(cmdUpdateRecord.CollabGUID);
                    if (guidIndex == -1)
                    {
                        throw new Exception(string.Format("参考数据库中的要素类【{0}】找不到GUID字段【{1}】", referFC.AliasName, cmdUpdateRecord.CollabGUID));
                    }


                    if (wo != null)
                        wo.SetText(string.Format("正在对要素类【{0}】中的要素进行检查......", fc.AliasName));

                    #region 检查
                    //收集参考要素类中的GUID集合
                    List<string> referFCGUIDList = new List<string>();
                    IQueryFilter qf = new QueryFilterClass();
                    if (referFC.HasCollabField())
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                    IFeatureCursor referFeCursor = referFC.Search(qf, true);
                    IFeature referFe = null;
                    while ((referFe = referFeCursor.NextFeature()) != null)
                    {
                        string guid = referFe.get_Value(referGUIDIndex).ToString();

                        if(!referFCGUIDList.Contains(guid))
                            referFCGUIDList.Add(guid);
                    }
                    Marshal.ReleaseComObject(referFeCursor);

                    //遍历要素类中的所有要素类
                    qf = new QueryFilterClass();
                    if (fc.HasCollabField())
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;
                    IFeatureCursor feCursor = fc.Search(qf, true);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        string guid = fe.get_Value(guidIndex).ToString();

                        if (!referFCGUIDList.Contains(guid))
                        {
                            errOIDList.Add(fe.OID);
                        }
                    }
                    Marshal.ReleaseComObject(feCursor);
                    #endregion

                    if (errOIDList.Count > 0)
                    {
                        if (resultFile == null)
                        {
                            //新建结果文件
                            resultFile = new ShapeFileWriter();
                            Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                            fieldName2Len.Add("要素类名", 16);
                            fieldName2Len.Add("要素编号", 16);
                            fieldName2Len.Add("检查项", 32);

                            resultFile.createErrorResutSHPFile(resultSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPoint, fieldName2Len);
                        }

                        //写入结果文件
                        foreach (var item in errOIDList)
                        {
                            fe = fc.GetFeature(item);

                            Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                            fieldName2FieldValue.Add("要素类名", fc.AliasName);
                            fieldName2FieldValue.Add("要素编号", item.ToString());
                            fieldName2FieldValue.Add("检查项", "GUID新增检查");
                            IPoint geo = null;
                            try
                            {
                                if (fc.ShapeType == esriGeometryType.esriGeometryPoint)
                                {
                                    geo = fe.ShapeCopy as IPoint;
                                }
                                else if (fc.ShapeType == esriGeometryType.esriGeometryPolyline)
                                {
                                    geo = new PointClass();
                                    (fe.Shape as ICurve).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, geo);
                                }
                                else if (fc.ShapeType == esriGeometryType.esriGeometryPolygon)
                                {
                                    geo = (fe.Shape as IArea).LabelPoint;
                                }
                            }
                            catch
                            {
                                //求几何中心点失败，空几何返回
                            }
                            resultFile.addErrorGeometry(geo, fieldName2FieldValue);

                            Marshal.ReleaseComObject(fe);

                            //内存监控
                            if (Environment.WorkingSet > DCDHelper.MaxMem)
                            {
                                GC.Collect();
                            }
                        }

                    }//errOIDList.Count > 0

                }

                //保存结果文件
                if(resultFile != null)
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
        
    }
}
