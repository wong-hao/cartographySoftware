using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Data;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geoprocessing;

namespace SMGI.Plugin.DCDProcess
{
    public class PolylineGeneralizeCmd : SMGI.Common.SMGICommand
    {
        public PolylineGeneralizeCmd()
        {
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && 
                    m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            var frm = new GeneralizeForm(esriGeometryType.esriGeometryPolyline, m_Application.MapControl.Map.ReferenceScale);
            frm.Text = "线化简";

            if (frm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            bool bsuccess = false;
            using (var wo = m_Application.SetBusy())
            {
                bsuccess = Generalize((frm.ObjLayer as IFeatureLayer).FeatureClass, frm.FilterText, frm.SimplifyAlgorithm, frm.SimplifyTolerance, frm.EnableSmooth, frm.SmoothAlgorithm, frm.SmoothTolerance, true, wo);
            }

            if (bsuccess)
            {
                MessageBox.Show("处理完毕");
            }


        }


        public static bool Generalize(IFeatureClass inputFC, string filterText, string simplyAlgorithm, double simplifyTol, bool enableSmooth, string smoothAlgorithm, double smoothTol, bool needEdit, WaitOperation wo = null)
        {
            bool bSuccess = false;

            //临时工作空间
            string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
            IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);
            IFeatureWorkspace fws = ws as IFeatureWorkspace;

            IFeatureClass tempFC_Simplify = null;
            IFeatureClass tempFC_Simplify_Smooth = null;

            var gp = GApplication.Application.GPTool;
            gp.SetEnvironmentValue("workspace", (inputFC as IDataset).Workspace.PathName);
            gp.OverwriteOutput = true;
            try
            {
                //选择要素
                ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer pMakeFeatureLayer = new ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer();
                pMakeFeatureLayer.in_features = inputFC;
                pMakeFeatureLayer.out_layer = inputFC.AliasName + "_Layer";
                SMGI.Common.Helper.ExecuteGPTool(gp, pMakeFeatureLayer, null);
                ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute pSelectLayerByAttribute = new ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute();
                pSelectLayerByAttribute.in_layer_or_view = inputFC.AliasName + "_Layer";

                if (inputFC.AliasName.ToUpper() == "BOUL")//国界线不参与化简
                {
                    if (filterText == "")
                    {
                        filterText = "GB <> 620201 and GB <> 620202";
                    }
                    else
                    {
                        filterText = string.Format("({0}) and GB <> 620201 and GB <> 620202", filterText);
                    }
                }
                if (inputFC.HasCollabField())
                {
                    if (filterText == "")
                    {
                        pSelectLayerByAttribute.where_clause = cmdUpdateRecord.CurFeatureFilter;
                    }
                    else
                    {
                        pSelectLayerByAttribute.where_clause = cmdUpdateRecord.CurFeatureFilter + string.Format(" and ({0})", filterText);
                    }

                }
                else
                {
                    pSelectLayerByAttribute.where_clause = filterText;
                }

                
                SMGI.Common.Helper.ExecuteGPTool(gp, pSelectLayerByAttribute, null);

                IQueryFilter qf = new QueryFilterClass();
                if (pSelectLayerByAttribute.where_clause != null)
                    qf.WhereClause = pSelectLayerByAttribute.where_clause.ToString();

                //化简
                if (wo != null)
                    wo.SetText(string.Format("正在对要素类【{0}】进行化简......", inputFC.AliasName));
                ESRI.ArcGIS.CartographyTools.SimplifyLine simplifyLineTool = new ESRI.ArcGIS.CartographyTools.SimplifyLine();
                simplifyLineTool.in_features = inputFC.AliasName + "_Layer";
                simplifyLineTool.out_feature_class = ws.PathName + @"\" + inputFC.AliasName + "_Simplify";
                simplifyLineTool.algorithm = simplyAlgorithm;
                simplifyLineTool.tolerance = simplifyTol;
                SMGI.Common.Helper.ExecuteGPTool(gp, simplifyLineTool, null);
                tempFC_Simplify = (ws as IFeatureWorkspace).OpenFeatureClass(inputFC.AliasName + "_Simplify");

                //光滑
                if (enableSmooth)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在对要素类【{0}】进行平滑处理......", inputFC.AliasName));
                    ESRI.ArcGIS.CartographyTools.SmoothLine smoothLineTool = new ESRI.ArcGIS.CartographyTools.SmoothLine();
                    smoothLineTool.in_features = ws.PathName + @"\" + inputFC.AliasName + "_Simplify";
                    smoothLineTool.out_feature_class = ws.PathName + @"\" + tempFC_Simplify.AliasName + "_Smooth";
                    smoothLineTool.algorithm = smoothAlgorithm;
                    smoothLineTool.tolerance = smoothTol;
                    SMGI.Common.Helper.ExecuteGPTool(gp, smoothLineTool, null);

                    tempFC_Simplify_Smooth = (ws as IFeatureWorkspace).OpenFeatureClass(tempFC_Simplify.AliasName + "_Smooth");
                }


                IFeatureClass outFC = null;
                if (enableSmooth)
                {
                    outFC = tempFC_Simplify_Smooth;
                }
                else
                {
                    outFC = tempFC_Simplify;
                }

                int guidIndex = inputFC.FindField(cmdUpdateRecord.CollabGUID);
                if (needEdit && guidIndex != -1)
                {
                    //获取化简后的要素信息
                    Dictionary<string, int> guid2OID = new Dictionary<string, int>();
                    IFeatureCursor simpCursor = outFC.Search(null, true);
                    IFeature simpFe = null;
                    while ((simpFe = simpCursor.NextFeature()) != null)
                    {
                        string guid = simpFe.get_Value(guidIndex).ToString();
                        if (!guid2OID.ContainsKey(guid))
                        {
                            guid2OID.Add(guid, simpFe.OID);
                        }
                        else
                        {
                            MessageBox.Show(string.Format("目标图层中字段【{0}】的属性值{1}不唯一，处理失败！", cmdUpdateRecord.CollabGUID, guid));
                            return bSuccess;
                        }
                    }
                    Marshal.ReleaseComObject(simpCursor);


                    IWorkspaceEdit wsEdit = (inputFC as IDataset).Workspace as IWorkspaceEdit;
                    if (wsEdit.IsBeingEdited())
                    {
                        wsEdit.StartEditOperation();
                    }

                    try
                    {
                        
                        //更新要素
                        IFeatureCursor inCursor = inputFC.Search(qf, false);
                        IFeature fe = null;
                        while ((fe = inCursor.NextFeature()) != null)
                        {
                            string guid = fe.get_Value(guidIndex).ToString();

                            if (guid2OID.ContainsKey(guid))
                            {
                                int oid = guid2OID[guid];
                                simpFe = outFC.GetFeature(oid);

                                fe.Shape = simpFe.ShapeCopy;
                                fe.Store();
                            }
                            else
                            {
                                fe.Delete();
                            }

                            Marshal.ReleaseComObject(fe);
                        }
                        Marshal.ReleaseComObject(inCursor);

                        if (wsEdit.IsBeingEdited())
                        {
                            GApplication.Application.EngineEditor.StopOperation("线化简");
                        }

                        //刷新地图
                        GApplication.Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, GApplication.Application.ActiveView.Extent);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.Message);
                        System.Diagnostics.Trace.WriteLine(ex.Source);
                        System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                        if (wsEdit.IsBeingEdited())
                        {
                            GApplication.Application.EngineEditor.AbortOperation();
                        }

                        throw ex;
                    }
                }
                else
                {
                    //清空原要素类
                    (inputFC as ITable).DeleteSearchedRows(qf);

                    //将化简后的数据拷贝回原要素类中
                    DCDHelper.CopyFeaturesToFeatureClass(outFC, null, inputFC, false);
                }

                bSuccess = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                bSuccess = false;
            }
            finally
            {

                if (tempFC_Simplify != null)
                {
                    (tempFC_Simplify as IDataset).Delete();
                }

                if (tempFC_Simplify_Smooth != null)
                {
                    (tempFC_Simplify_Smooth as IDataset).Delete();
                }
            }

            return bSuccess;
        }
    }
}
