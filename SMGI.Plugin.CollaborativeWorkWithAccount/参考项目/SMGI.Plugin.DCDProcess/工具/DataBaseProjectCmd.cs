using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using System.IO;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using ESRI.ArcGIS.DataSourcesRaster;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 该工具用于对输入的地理数据库（数据库内部所有要素类必须具有空间参考）进行批量投影变换：
    /// @LZ：2021
    /// </summary>
    public class DataBaseProjectCmd : SMGI.Common.SMGICommand
    {
        public DataBaseProjectCmd()
        {
            m_caption = "数据库投影";
            m_message = "对对具有相同空间参考的地理数据库进行批量投影变换";

        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null;
            }
        }

        public override void OnClick()
        {
            var frm = new DataBaseProjectForm();
            frm.StartPosition = FormStartPosition.CenterParent;

            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                if (frm.EnableTransParams)
                {
                    err = DatabaseProject(frm.SourceWSList, frm.OutputSpatialReference, frm.DBNameDuffix, frm.TransParams, frm.OutputPath, wo);
                }
                else
                {
                    err = DatabaseProject(frm.SourceWSList, frm.OutputSpatialReference, frm.DBNameDuffix, null, frm.OutputPath, wo);
                }
            }

            if (err == "")
            {
                MessageBox.Show("数据库投影完成！");
            }
            else
            {
                MessageBox.Show(err);
            }

        }

        private static string DatabaseProject(List<IWorkspace> sourceWSList, ISpatialReference objSR, string nameDuffix, double[] transParams, string outputPath, WaitOperation wo)
        {
            string err = "";

            Geoprocessor gp = new Geoprocessor();
            gp.OverwriteOutput = true;

            int num = 0;
            foreach (var item in sourceWSList)
            {
                if (wo != null)
                    wo.SetText(string.Format("正在对数据库【{0}】进行投影变换，第{1}个/共{2}个......", item.PathName, ++num, sourceWSList.Count));

                string wsPath = item.PathName;

                string databasename = System.IO.Path.GetFileNameWithoutExtension(wsPath);
                string projectDBFullName;
                if (nameDuffix == "")
                {
                    projectDBFullName = System.IO.Path.Combine(outputPath, System.IO.Path.GetFileName(wsPath));
                }
                else
                {
                    projectDBFullName = System.IO.Path.Combine(outputPath, string.Format("{0}_{1}", databasename, nameDuffix)) + System.IO.Path.GetExtension(wsPath);
                }

                try
                {
                    #region 1.创建空数据库
                    if (item.WorkspaceFactory.ToString().Contains("FileGDBWorkspaceFactory"))
                    {
                        //删除已存在文件
                        if (Directory.Exists(projectDBFullName))
                        {
                            Directory.Delete(projectDBFullName, true);
                        }
                    }
                    else if (item.WorkspaceFactory.ToString().Contains("AccessWorkspaceFactory"))
                    {
                        //删除已存在文件
                        if (File.Exists(projectDBFullName))
                        {
                            File.Delete(projectDBFullName);
                        }
                    }

                    IWorkspaceName workspaceName = item.WorkspaceFactory.Create(outputPath, System.IO.Path.GetFileNameWithoutExtension(projectDBFullName), null, 0);//创建数据库
                    #endregion

                    #region 2.数据库投影
                    IEnumDataset sourceEnumDataset = item.get_Datasets(esriDatasetType.esriDTAny);
                    sourceEnumDataset.Reset();
                    IDataset sourceDataset = null;
                    while ((sourceDataset = sourceEnumDataset.Next()) != null)
                    {
                        if (sourceDataset is IFeatureDataset || sourceDataset is IFeatureClass)//要素数据集、要素类
                        {
                            gp.SetEnvironmentValue("workspace", projectDBFullName);

                            Project project = new Project();

                            project.in_dataset = sourceDataset;
                            project.out_coor_system = objSR;
                            project.out_dataset = sourceDataset.Name;
                            if (transParams != null && transParams.Count() == 7)//包含地理变换
                            {
                                ICoordinateFrameTransformation trans = new CoordinateFrameTransformationClass();
                                trans.PutParameters(transParams[0], transParams[1], transParams[2], transParams[3], transParams[4], transParams[5], transParams[6]);
                                trans.PutSpatialReferences((sourceDataset as IGeoDataset).SpatialReference, objSR);
                                trans.Name = "Custom GeoTran";

                                project.transform_method = trans;
                            }

                            SMGI.Common.Helper.ExecuteGPTool(gp, project, null);
                        }
                        else if (sourceDataset is IRasterDataset)//栅格数据集
                        {
                            //暂不处理
                        }
                        else if (sourceDataset is IRasterCatalog)
                        {
                            //暂不处理
                        }
                        else if (sourceDataset is IMosaicDataset)
                        {
                            //暂不处理
                        }
                        else
                        {
                            //暂不处理
                        }
                    }
                    Marshal.ReleaseComObject(sourceEnumDataset);
                    Marshal.ReleaseComObject(item);
                    #endregion
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                    System.Diagnostics.Trace.WriteLine(ex.Source);

                    if(err == "")
                        err = string.Format("数据库【{0}】投影时失败：{1}!", wsPath, ex.Message);
                    else
                        err += string.Format("\n数据库【{0}】投影时失败：{1}!", wsPath, ex.Message);
                }
                finally
                {
                    Marshal.ReleaseComObject(item);
                }

                GC.Collect();
            }

            return err;
        }



    }
}
