using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 要素匹配@LZ,20221031
    /// 逐一遍历目标要素类中的指定要素，建立缓冲区，检索与之匹配的参考要素（关键字段如GB等属性值一致，同时参考要素类中匹配要素与之叠置的几何为单部件）的待核查属性值是否一致，并将匹配结果写入目标要素类中的临时字段中
    /// </summary>
    public class FeatureMatchCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            LineFeatureMatchForm frm = new LineFeatureMatchForm();
            if (DialogResult.OK != frm.ShowDialog())
                return;

            bool res;
            using (var wo = m_Application.SetBusy())
            {
                res = DoCheck(frm.ObjLayer.FeatureClass, frm.FilterText, frm.BufferValue, frm.ReferLayer.FeatureClass, frm.MatchFNList, frm.CheckFN,"check_","matchresult", "matchdesc",wo);
            }
            if (res)
            {
                MessageBox.Show("匹配检测完成，具体请查看目标图层！");
            }
            
        }

        public bool DoCheck(IFeatureClass objFC, string filterText, double bufferVal, IFeatureClass referFC, List<string> mathcFNList, string checkFN, string newCheckFNAffix = "check_", string newCheckResultFN = "matchresult", string newDescFN = "matchdesc", WaitOperation wo = null)
        {
            bool res = false;

            string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
            IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);
            string tempFN = "org_fid1";
            IFeatureClass temp_bufferFC = null;
            IFeatureClass temp_intersectFC = null;
            try
            {
                Geoprocessor gp = new Geoprocessor();
                gp.OverwriteOutput = true;
                gp.SetEnvironmentValue("workspace", ws.PathName);

                #region 0,在目标要素类中增加临时字段,并赋以ID
                AddField(objFC, tempFN, esriFieldType.esriFieldTypeInteger);
                CalculateField cal = new CalculateField();
                cal.in_table = (objFC as IDataset).Workspace.PathName + "\\" + (objFC as IDataset).Name;
                cal.field = tempFN;
                cal.expression = @"[OBJECTID]";
                SMGI.Common.Helper.ExecuteGPTool(gp, cal, null);
                #endregion 

                #region 1.对目标图层指定要素建立缓冲区
                if (wo != null)
                    wo.SetText("正在构建缓冲......");
                ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer makeFeatureLayer = new ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer();
                makeFeatureLayer.in_features = objFC;
                makeFeatureLayer.out_layer = objFC.AliasName + "_Layer";
                SMGI.Common.Helper.ExecuteGPTool(gp, makeFeatureLayer, null);

                ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute selectLayerByAttribute = new ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute();
                selectLayerByAttribute.in_layer_or_view = objFC.AliasName + "_Layer";
                selectLayerByAttribute.where_clause = filterText;
                SMGI.Common.Helper.ExecuteGPTool(gp, selectLayerByAttribute, null);

                ESRI.ArcGIS.AnalysisTools.Buffer buffer = new ESRI.ArcGIS.AnalysisTools.Buffer();
                buffer.buffer_distance_or_field = string.Format("{0} Meters", bufferVal);
                buffer.in_features = objFC.AliasName + "_Layer";
                buffer.out_feature_class = objFC.AliasName + "_buffer";
                SMGI.Common.Helper.ExecuteGPTool(gp, buffer, null);
                temp_bufferFC = (ws as IFeatureWorkspace).OpenFeatureClass(objFC.AliasName + "_buffer");
                #endregion

                Dictionary<int, Dictionary<int, double>> bufferFID2refFIDAndLen = new Dictionary<int,Dictionary<int,double>>();
                #region 2.缓冲要素类和参考要素类进行相交分析,并提取结果
                if (wo != null)
                    wo.SetText("正在求交分析......");
                string outFCName = objFC.AliasName + "_" + referFC.AliasName + "_IntersectAnalysis";
                int i = 0;
                while ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, outFCName))
                {
                    ++i;
                    outFCName += i.ToString();
                }


                ESRI.ArcGIS.AnalysisTools.Intersect intersectTool = new ESRI.ArcGIS.AnalysisTools.Intersect();
                IGpValueTableObject valTbl = new GpValueTableObjectClass();
                valTbl.SetColumns(2);
                object o1 = temp_bufferFC;//输入 IFeatureClass 
                object o2 = referFC;//输入 IFeatureClass 
                valTbl.AddRow(ref o1);
                valTbl.AddRow(ref o2);
                intersectTool.in_features = valTbl;
                intersectTool.out_feature_class = outFCName;
                intersectTool.join_attributes = "ONLY_FID";
                SMGI.Common.Helper.ExecuteGPTool(gp, intersectTool, null);

                //打开临时要素类
                temp_intersectFC = (ws as IFeatureWorkspace).OpenFeatureClass(outFCName);

                string bufferFIDFieldName = string.Format("FID_{0}", (temp_bufferFC as IDataset).Name);
                int bufferFIDIndex = temp_intersectFC.FindField(bufferFIDFieldName);
                string referFIDFieldName = string.Format("FID_{0}", (referFC as IDataset).Name);
                if (bufferFIDFieldName == referFIDFieldName)
                    referFIDFieldName += "_1";
                int referFIDIndex = temp_intersectFC.FindField(referFIDFieldName);

                IFeatureCursor intersectFeCursor = temp_intersectFC.Search(null, true);
                IFeature intersectFe = null;
                while ((intersectFe = intersectFeCursor.NextFeature()) != null)
                {
                    int bufferFID = int.Parse(intersectFe.get_Value(bufferFIDIndex).ToString());
                    int referFID = int.Parse(intersectFe.get_Value(referFIDIndex).ToString());

                    IPolyline pl = intersectFe.Shape as IPolyline;
                    double len = (pl != null && !pl.IsEmpty) ? pl.Length : 0;


                    if (bufferFID2refFIDAndLen.ContainsKey(bufferFID))
                    {
                        if (bufferFID2refFIDAndLen[bufferFID].ContainsKey(referFID))
                        {
                            bufferFID2refFIDAndLen[bufferFID][referFID] += len;
                        }
                        else
                        {
                            bufferFID2refFIDAndLen[bufferFID].Add(referFID, len);
                        }
                    }
                    else
                    {
                        Dictionary<int, double> fid2InterMetric = new Dictionary<int, double>();
                        fid2InterMetric.Add(referFID, len);

                        bufferFID2refFIDAndLen.Add(bufferFID, fid2InterMetric);
                    }

                }
                Marshal.ReleaseComObject(intersectFeCursor);

                #endregion

                Dictionary<int, KeyValuePair<string,string>> matchInfo = new Dictionary<int,KeyValuePair<string,string>>();
                Dictionary<int, string> matchResultInfo = new Dictionary<int, string>();
                #region 3.整理求交分析结果
                if (wo != null)
                        wo.SetText(string.Format("正在整理分析结果......"));

                //获取参考要素类的要素信息
                Dictionary<int, Dictionary<string, object>> referFID2MatchFNV = new Dictionary<int, Dictionary<string, object>>();
                Dictionary<int,object> referFID2CheckV = new Dictionary<int,object>();
                IFeatureCursor referFeCursor = referFC.Search(null, true);
                IFeature referFe = null;
                while ((referFe = referFeCursor.NextFeature()) != null)
                {
                    foreach(var item in mathcFNList)
                    {
                        object v = referFe.get_Value(referFe.Fields.FindField(item));

                        if(!referFID2MatchFNV.ContainsKey(referFe.OID))
                        {
                            Dictionary<string, object> lst = new Dictionary<string, object>();
                            lst.Add(item,v);

                            referFID2MatchFNV.Add(referFe.OID, lst);
                        }
                        else
                        {
                            referFID2MatchFNV[referFe.OID].Add(item,v);
                        }
                    }

                    referFID2CheckV.Add(referFe.OID, referFe.get_Value(referFe.Fields.FindField(checkFN)));
                    
                }
                Marshal.ReleaseComObject(referFeCursor);

                //遍历缓冲要素,整理目标要素的匹配信息
                IFeatureCursor feCursor = temp_bufferFC.Search(null, true);
                IFeature fe = null;
                int index1 = temp_bufferFC.FindField(tempFN);
                int index2 = temp_bufferFC.FindField(checkFN);
                while ((fe = feCursor.NextFeature()) != null)
                {
                    
                    int orgfid = int.Parse(fe.get_Value(index1).ToString());
                    string orgCheckValue = fe.get_Value(index2).ToString().Trim();
                    string checkFNValue = "";
                    string checkresult = "";
                    string checkDesc = "";

                    Dictionary<string, object> objMatchFNInfo = new Dictionary<string,object>();
                    foreach(var item in mathcFNList)
                    {
                        object v = fe.get_Value(fe.Fields.FindField(item));
                        objMatchFNInfo.Add(item,v);
                    }

                    Dictionary<object, List<int>> checkVal2ReferFIDList = new Dictionary<object,List<int>>();//匹配上的核查字段属性值及对应的参考要素ID
                    if(bufferFID2refFIDAndLen.ContainsKey(fe.OID))//存在与之相交的参考要素
                    {
                        Dictionary<int, double> intersectInfo = bufferFID2refFIDAndLen[fe.OID];
                        foreach(var kv in intersectInfo)
                        {
                            double objFeLen = (objFC.GetFeature(orgfid).Shape as IPolyline).Length;
                            double referFeLen = (referFC.GetFeature(kv.Key).Shape as IPolyline).Length;

                            if(referFID2CheckV.ContainsKey(kv.Key))
                            {
                                if (objFeLen > 2 * bufferVal && kv.Value <= 2 * bufferVal)//相交部分的长度小于缓冲距离，不参与匹配
                                    continue;

                                if ((kv.Value / referFeLen < 0.7) && (kv.Value / objFeLen < 0.7))//相交部分的长度小于参考要素长度的70%，且小于目标要素长度的70%，不参与匹配
                                    continue;

                                object referCheckFNInfo = referFID2CheckV[kv.Key];

                                if (mathcFNList.Count > 0)
                                {
                                    Dictionary<string, object> referMatchFNInfo = referFID2MatchFNV[kv.Key];

                                    //判断目标要素与参考要素的匹配字段属性值是否一致,不一致
                                    bool bSame = true;
                                    foreach (var kv2 in objMatchFNInfo)
                                    {
                                        object objValue = kv2.Value;

                                        foreach (var kv3 in referMatchFNInfo)
                                        {
                                            if (kv3.Key == kv2.Key && !kv2.Value.Equals(kv3.Value))
                                            {
                                                bSame = false;
                                                break;
                                            }
                                        }

                                        if (!bSame)
                                            break;
                                    }

                                    if (!bSame)
                                    {
                                        continue;
                                    }
                                }

                                //获取匹配信息
                                if(checkVal2ReferFIDList.ContainsKey(referCheckFNInfo))
                                {
                                    checkVal2ReferFIDList[referCheckFNInfo].Add(kv.Key);
                                }
                                else
                                {
                                    List<int> fidList = new List<int>();
                                    fidList.Add(kv.Key);
                                    checkVal2ReferFIDList.Add(referCheckFNInfo,fidList);
                                }
                                

                            }
                        }

                        if(checkVal2ReferFIDList.Count>0)
                        {
                            foreach(var kv4 in checkVal2ReferFIDList)
                            {
                                if(checkFNValue == "")
                                {
                                    checkFNValue = kv4.Key.ToString().Trim() == "" ? "空" : kv4.Key.ToString().Trim();
                                }
                                else
                                {
                                    checkFNValue += kv4.Key.ToString().Trim() == "" ? ",空" : "," + kv4.Key.ToString().Trim();
                                }


                                if(checkDesc == "")
                                {
                                    checkDesc = string.Format("{0}|", kv4.Key.ToString().Trim() == "" ? "空" : kv4.Key.ToString().Trim());
                                }
                                else
                                {
                                    checkDesc += string.Format(",{0}|", kv4.Key.ToString().Trim() == "" ? "空" : kv4.Key.ToString().Trim());
                                }
                                string fidListText = "";
                                foreach(var fid in kv4.Value)
                                {
                                    if(fidListText == "")
                                    {
                                        fidListText = fid.ToString();
                                    }
                                    else
                                    {
                                        fidListText += "、" + fid.ToString();
                                    }
                                    
                                }
                                checkDesc += string.Format("{0}", fidListText);
                            }
                        }
                    }
                    matchInfo.Add(orgfid, new KeyValuePair<string, string>(checkFNValue, checkDesc));


                    if (checkVal2ReferFIDList.Count == 0)
                    {
                        checkresult = "未找到匹配要素";
                    }
                    else if (checkVal2ReferFIDList.Count == 1)
                    {
                        if (checkFNValue.ToString().Trim() == orgCheckValue)
                        {
                            checkresult = "属性一致";
                        }
                        else if (checkFNValue == "")
                        {
                            checkresult = "匹配要素的属性为空";
                        }
                        else if (orgCheckValue == "")
                        {
                            checkresult = "存在1个匹配属性，且原值为空";
                        }
                        else
                        {
                            checkresult = "存在1个匹配属性，但与原值不同";
                        }
                    }
                    else 
                    {
                        checkresult = "存在多个不同的匹配属性";
                    }
                    matchResultInfo.Add(orgfid, checkresult);
                    
                }
                Marshal.ReleaseComObject(feCursor);
                #endregion


                #region 4.将结果写入目标要素类中
                if (wo != null)
                    wo.SetText(string.Format("正在写入匹配结果......"));

                int newIndex1 = AddField(objFC, newCheckFNAffix + checkFN, esriFieldType.esriFieldTypeString);
                int newIndex3 = AddField(objFC, newCheckResultFN, esriFieldType.esriFieldTypeString);
                int newIndex2 = AddField(objFC, newDescFN, esriFieldType.esriFieldTypeString);
                

                var fCursor = objFC.Update(new QueryFilterClass(){WhereClause = filterText}, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    string v1 = matchInfo[f.OID].Key;
                    string v2 = matchInfo[f.OID].Value;
                    string v3 = matchResultInfo[f.OID];

                    if (v1.Length > 120)
                        v1 = v1.Substring(0, 120) + "......";
                    if (v2.Length > 120)
                        v2 = v2.Substring(0, 120) + "......";
                    if (v3.Length > 120)
                        v3 = v3.Substring(0, 120) + "......";

                    if (matchInfo.ContainsKey(f.OID))
                    {
                        f.set_Value(newIndex1, v1);
                        f.set_Value(newIndex2, v2);
                        f.set_Value(newIndex3, v3);

                        fCursor.UpdateFeature(f);
                    }

                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                #endregion

                res = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (temp_bufferFC != null)
                {
                    (temp_bufferFC as IDataset).Delete();
                    temp_bufferFC = null;
                }

                if (temp_intersectFC != null)
                {
                    (temp_intersectFC as IDataset).Delete();
                    temp_intersectFC = null;
                }

                if(objFC.FindField(tempFN)!= -1)
                {
                    objFC.DeleteField(objFC.Fields.get_Field(objFC.FindField(tempFN)));
                }
            }

            return res;
        }

        public int AddField(IFeatureClass fc, string fldName, esriFieldType fldType, int length = 255)
        {
            int fieldIndex = fc.FindField(fldName);
            if (fieldIndex == -1)
            {
                IFieldEdit field = new FieldClass();
                field.Name_2 = fldName;
                field.Type_2 = fldType;

                if (fldType == esriFieldType.esriFieldTypeString)
                { 
                    field.Length_2 = length; 
                }
                field.Editable_2 = true;
                field.DefaultValue_2 = "NULL";

                fc.AddField(field);

                fieldIndex = fc.FindField(fldName);
            }
            return fieldIndex;
        }
    }
}
