

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.DCDProcess
{
    public class DatabaseMergeCmd : SMGI.Common.SMGICommand
    {
        public DatabaseMergeCmd()
        {
            m_caption = "数据库合并";
        }
             
        public override bool Enabled
        {
            get
            {
                return true;
            }
        }

        public override void OnClick()
        {
            DatabaseMergeForm frm = new DatabaseMergeForm(m_Application);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;


            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    wo.SetText("正在新建输出数据库文件......");
                    #region 根据参考数据库模板，新建输出数据库文件
                    //读取数据模板
                    IWorkspaceFactory tempWSFactory = new FileGDBWorkspaceFactoryClass();
                    IWorkspace tempWorkspace = tempWSFactory.OpenFromFile(frm.DataBaseTemplate, 0);

                    //新建输出数据库
                    Dictionary<string, IFeatureClass> fcName2FC = DCDHelper.CreateGDBStruct(tempWorkspace, System.IO.Path.Combine(frm.OutputPath, frm.OutputGDBName));

                    Marshal.ReleaseComObject(tempWSFactory);
                    #endregion


                    #region 依次遍历待合并数据库，将数据集导入到输出数据库
                    int count = 0;
                    foreach (var sourceFileName in frm.SourceDBFileNameList)
                    {
                        count++;

                        IWorkspaceFactory srcWF = null;
                        if (sourceFileName.ToLower().EndsWith(".gdb"))
                        {
                            srcWF = new FileGDBWorkspaceFactoryClass();
                        }
                        else if (sourceFileName.ToLower().EndsWith(".mdb"))
                        {
                            srcWF = new AccessWorkspaceFactoryClass();
                        }

                        if (srcWF == null)
                            continue;

                        #region 导入要素到输出数据库中对应的要素类中
                        IWorkspace sourceWorkspace = srcWF.OpenFromFile(sourceFileName, 0);
                        IEnumDataset sourceEnumDataset = sourceWorkspace.get_Datasets(esriDatasetType.esriDTAny);
                        sourceEnumDataset.Reset();
                        IDataset sourceDataset = null;
                        while ((sourceDataset = sourceEnumDataset.Next()) != null)
                        {
                            if (sourceDataset is IFeatureDataset)//要素数据集
                            {
                                //遍历子要素类
                                IFeatureDataset sourceFeatureDataset = sourceDataset as IFeatureDataset;
                                IEnumDataset subSourceEnumDataset = sourceFeatureDataset.Subsets;
                                subSourceEnumDataset.Reset();
                                IDataset subSourceDataset = null;
                                while ((subSourceDataset = subSourceEnumDataset.Next()) != null)
                                {
                                    if (subSourceDataset is IFeatureClass)//要素类
                                    {
                                        if(!fcName2FC.ContainsKey(subSourceDataset.Name))
                                            continue;//输出数据库中不包含该要素类

                                        IFeatureClass fc = subSourceDataset as IFeatureClass;
                                        IFeatureClass targetFC = fcName2FC[subSourceDataset.Name];

                                        //导入要素
                                        wo.SetText(string.Format("({0}/{1}):正在导入数据库【{2}】中的要素类【{3}】......",
                                            count, frm.SourceDBFileNameList.Count, sourceFileName, subSourceDataset.Name));
                                        bool bSuc = DCDHelper.AppendToFeatureClass(fc, null, targetFC, null);
                                        if (!bSuc)
                                            return;
                                    }
                                }
                                Marshal.ReleaseComObject(subSourceEnumDataset);
                            }
                            else if (sourceDataset is IFeatureClass)//要素类
                            {
                                if (!fcName2FC.ContainsKey(sourceDataset.Name))
                                    continue;//输出数据库中不包含该要素类

                                IFeatureClass fc = sourceDataset as IFeatureClass;
                                IFeatureClass targetFC = fcName2FC[sourceDataset.Name]; ;

                                //导入要素
                                wo.SetText(string.Format("({0}/{1}):正在导入数据库【{2}】中的要素类【{3}】......",
                                    count, frm.SourceDBFileNameList.Count, sourceFileName, sourceDataset.Name));
                                bool bSuc = DCDHelper.AppendToFeatureClass(fc, null, targetFC, null);
                                if (!bSuc)
                                    return;
                            }
                        }
                        Marshal.ReleaseComObject(sourceEnumDataset);
                        #endregion
                    }
                    #endregion
                }

                MessageBox.Show("操作完成！");
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
