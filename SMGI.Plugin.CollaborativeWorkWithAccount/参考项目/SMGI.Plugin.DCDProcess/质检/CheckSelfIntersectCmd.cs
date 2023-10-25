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
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 自相交检查
    /// </summary>
    public class CheckSelfIntersectCmd : SMGICommand
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
            var frm = new CheckLayerSelectForm(m_Application, false, true, true);
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Text = "自相交检查";

            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string outputFileName_line, outputFileName_point;
            if (frm.CheckFeatureLayerList.Count > 1)
            {
                outputFileName_line = OutputSetup.GetDir() + string.Format("\\{0}_线.shp", frm.Text);
                outputFileName_point = OutputSetup.GetDir() + string.Format("\\{0}_点.shp", frm.Text);
            }
            else
            {
                outputFileName_line = OutputSetup.GetDir() + string.Format("\\{0}_线_{1}.shp", frm.Text, frm.CheckFeatureLayerList.First().Name);
                outputFileName_point = OutputSetup.GetDir() + string.Format("\\{0}_点_{1}.shp", frm.Text, frm.CheckFeatureLayerList.First().Name);
            }


            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                List<IFeatureClass> fcList = new List<IFeatureClass>();
                foreach (var layer in frm.CheckFeatureLayerList)
                {
                    IFeatureClass fc = layer.FeatureClass;
                    if(!fcList.Contains(fc))
                        fcList.Add(fc);
                }

                err = DoCheck(outputFileName_line, outputFileName_point, fcList, wo);
            }

            if (err == "")
            {
                if (File.Exists(outputFileName_line) || File.Exists(outputFileName_point))
                {
                    if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        if (File.Exists(outputFileName_line))
                        {
                            IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName_line);
                            CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                        }

                        if (File.Exists(outputFileName_point))
                        {
                            IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName_point);
                            CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("检查完毕！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }

        }
        
        /// <summary>
        /// 自相交检查
        /// </summary>
        /// <param name="resultLineSHPFileName"></param>
        /// <param name="resultPointSHPFileName"></param>
        /// <param name="fcList"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public static string DoCheck(string resultLineSHPFileName, string resultPointSHPFileName, List<IFeatureClass> fcList, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                ShapeFileWriter resultLineFile = null;
                ShapeFileWriter resultPointFile = null;

                foreach (var fc in fcList)
                {
                    if (fc.ShapeType != esriGeometryType.esriGeometryPolyline && fc.ShapeType != esriGeometryType.esriGeometryPolygon)
                        continue;

                    IQueryFilter qf = new QueryFilterClass();
                    if (fc.HasCollabField())
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                    if (wo != null)
                        wo.SetText(string.Format("正在对要素类【{0}】进行自相交检查......", fc.AliasName));

                    //核查该图层的重叠点
                    Dictionary<IGeometry, int> errList = null;
                    if (fc.ShapeType == esriGeometryType.esriGeometryPolyline)
                    {
                        errList = CheckHelper.LineNoSelfIntersect(fc, qf);
                    }
                    else
                    {
                        //要素转线
                        IFeatureClass tempFC = null;
                        try
                        {
                            string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
                            IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);

                            ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = new Geoprocessor();
                            gp.OverwriteOutput = true;
                            gp.SetEnvironmentValue("workspace", ws.PathName);

                            PolygonToLine p2l = new PolygonToLine();
                            p2l.in_features = fc;
                            p2l.out_feature_class = fc.AliasName + "_FeatureToLine";
                            p2l.neighbor_option = "IGNORE_NEIGHBORS";
                            SMGI.Common.Helper.ExecuteGPTool(gp, p2l, null);

                            tempFC = (ws as IFeatureWorkspace).OpenFeatureClass(fc.AliasName + "_FeatureToLine");
                            int origFIDIndex = tempFC.FindField("ORIG_FID");

                            errList = CheckHelper.LineNoSelfIntersect(tempFC, qf);
                            for (int i = 0; i < errList.Count; ++i)
                            {
                                IGeometry geo = errList.ElementAt(i).Key;
                                int id = errList.ElementAt(i).Value;

                                IFeature f = tempFC.GetFeature(id);
                                errList[geo] = int.Parse(f.get_Value(origFIDIndex).ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            if (tempFC != null)
                            {
                                (tempFC as IDataset).Delete();
                                tempFC = null;
                            }
                        }
                    }

                    if (errList.Count > 0)
                    {
                        //写入结果文件
                        foreach (var kv in errList)
                        {
                            if (kv.Key.GeometryType == esriGeometryType.esriGeometryPoint)
                            {
                                if (resultPointFile == null)
                                {
                                    //建立结果文件
                                    resultPointFile = new ShapeFileWriter();
                                    Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                                    fieldName2Len.Add("图层名", 20);
                                    fieldName2Len.Add("要素编号", 32);
                                    fieldName2Len.Add("检查项", 32);
                                    resultPointFile.createErrorResutSHPFile(resultPointSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPoint, fieldName2Len);
                                }

                                Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                fieldName2FieldValue.Add("图层名", fc.AliasName);
                                fieldName2FieldValue.Add("要素编号", kv.Value.ToString());
                                fieldName2FieldValue.Add("检查项", "自相交检查");


                                resultPointFile.addErrorGeometry(kv.Key, fieldName2FieldValue);
                            }
                            else if (kv.Key.GeometryType == esriGeometryType.esriGeometryPolyline)
                            {
                                if (resultLineFile == null)
                                {
                                    //建立结果文件
                                    resultLineFile = new ShapeFileWriter();
                                    Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                                    fieldName2Len.Add("图层名", 20);
                                    fieldName2Len.Add("要素编号", 32);
                                    fieldName2Len.Add("检查项", 32);
                                    resultLineFile.createErrorResutSHPFile(resultLineSHPFileName, (fc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolyline, fieldName2Len);
                                }

                                Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                                fieldName2FieldValue.Add("图层名", fc.AliasName);
                                fieldName2FieldValue.Add("要素编号", kv.Value.ToString());
                                fieldName2FieldValue.Add("检查项", "自相交检查");


                                resultLineFile.addErrorGeometry(kv.Key, fieldName2FieldValue);
                            }

                            
                        }
                    }

                }

                //保存结果文件
                if (resultPointFile != null)
                    resultPointFile.saveErrorResutSHPFile();
                if (resultLineFile != null)
                    resultLineFile.saveErrorResutSHPFile();
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
