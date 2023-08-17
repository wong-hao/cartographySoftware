using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 数据映射：基于映射规则表，将源数据库中指定要素映射到标准结构数据库中对应的要素类中
    /// </summary>
    public class DataMappingCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null;
            }
        }

        public override void OnClick()
        {
            DataMappingForm frm = new DataMappingForm();
            if (DialogResult.OK != frm.ShowDialog())
                return;

            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();

                    wo.SetText("正在打开源数据库和模板数据库......");
                    IWorkspace sourceWorkspace = wsFactory.OpenFromFile(frm.SourceDataBase, 0);
                    Dictionary<string, IFeatureClass> sourceFCName2FC = DCDHelper.GetAllFeatureClassFromWorkspace(sourceWorkspace as IFeatureWorkspace);
                    IWorkspace tempWorkspace = wsFactory.OpenFromFile(frm.TemplateDataBase, 0);
                    Dictionary<string, IFeatureClass> templateFCName2FC = DCDHelper.GetAllFeatureClassFromWorkspace(tempWorkspace as IFeatureWorkspace);
                    Marshal.ReleaseComObject(wsFactory);

                    #region 检查映射规则是否支持
                    wo.SetText("正在检查映射规则......");
                    foreach (var item in frm.DataMappingRuleList)
                    {
                        string sourceFCName = item.SourceFCName;
                        string filter = item.SQLFilter;
                        string objFCName = item.ObjFCName;

                        if (!sourceFCName2FC.ContainsKey(sourceFCName))
                        {
                            MessageBox.Show(string.Format("源数据库中找不到规则表中的要素类【{0}】！", sourceFCName));
                            return;
                        }

                        if (!templateFCName2FC.ContainsKey(objFCName))
                        {
                            MessageBox.Show(string.Format("模板数据库中找不到规则表中的要素类【{0}】！", objFCName));
                            return;
                        }

                        switch (sourceFCName2FC[sourceFCName].ShapeType)
                        {
                            case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon:
                                {
                                    if (templateFCName2FC[objFCName].ShapeType != ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon &&
                                        templateFCName2FC[objFCName].ShapeType != ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline &&
                                        templateFCName2FC[objFCName].ShapeType != ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint)
                                    {
                                        MessageBox.Show(string.Format("类型不匹配，不支持源数据库中要素类【{0}】向目标数据库中要素类【{1}】的数据映射！", sourceFCName, objFCName));
                                        return;
                                    }

                                    break;
                                }
                            case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline:
                                {
                                    if (templateFCName2FC[objFCName].ShapeType != ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline &&
                                        templateFCName2FC[objFCName].ShapeType != ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint)
                                    {
                                        MessageBox.Show(string.Format("类型不匹配，不支持源数据库中要素类【{0}】向目标数据库中要素类【{1}】的数据映射！", sourceFCName, objFCName));
                                        return;
                                    }

                                    break;
                                }
                            case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
                                {
                                    if (templateFCName2FC[objFCName].ShapeType != ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint)
                                    {
                                        MessageBox.Show(string.Format("类型不匹配，不支持源数据库中要素类【{0}】向目标数据库中要素类【{1}】的数据映射！", sourceFCName, objFCName));
                                        return;
                                    }

                                    break;
                                }
                            default:
                                {
                                    MessageBox.Show(string.Format("不支持源数据库中要素类【{0}】的数据类型映射！", sourceFCName));
                                    return;
                                }
                        }
                    }
                    #endregion

                    #region 新建数据库
                    wo.SetText("正在基于标准结构模板数据库，创建输出数据库文件......");
                    Dictionary<string, IFeatureClass> objFCName2FC = DCDHelper.CreateGDBStruct(tempWorkspace, frm.OutputDataBase);
                    #endregion

                    #region 数据映射
                    foreach (var item in frm.DataMappingRuleList)
                    {
                        string sourceFCName = item.SourceFCName;
                        string filter = item.SQLFilter;
                        string objFCName = item.ObjFCName;

                        if (!sourceFCName2FC.ContainsKey(sourceFCName) || !objFCName2FC.ContainsKey(objFCName))
                            continue;

                        IFeatureClass sourceFC = sourceFCName2FC[sourceFCName];
                        IFeatureClass objFC = objFCName2FC[objFCName];

                        wo.SetText(string.Format("正在对源数据库的要素类【{0}】中满足【{1}】条件的要素进行数据映射......", sourceFCName, filter));
                        if (sourceFC.ShapeType == objFC.ShapeType)
                        {
                            //相同几何类型要素映射
                            IQueryFilter qf = new QueryFilterClass() { WhereClause = filter };
                            //bool bSuc = DCDHelper.AppendToFeatureClass(sourceFC, qf, objFC, item.ObjFN2SourceFN);
                            bool bSuc = DCDHelper.AppendToFeatureClass2(sourceFC, qf, objFC, item.ObjFN2SourceFN, item.objFN2Value);
                            if (!bSuc)
                                return;
                        }
                        else
                        {
                            //不同几何类型间的要素映射
                            IQueryFilter qf = new QueryFilterClass() { WhereClause = filter };
                            //bool bSuc = DCDHelper.AppendToFeatureClass(sourceFC, qf, objFC, item.ObjFN2SourceFN, true);
                            bool bSuc = DCDHelper.AppendToFeatureClass2(sourceFC, qf, objFC, item.ObjFN2SourceFN, item.objFN2Value,true);
                            if (!bSuc)
                                return;
                        }
                    }
                    #endregion
                }

                MessageBox.Show("数据映射完成！");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                System.Diagnostics.Trace.WriteLine(ex.Source);

                MessageBox.Show(ex.Message);
            }
        }

    }
}
