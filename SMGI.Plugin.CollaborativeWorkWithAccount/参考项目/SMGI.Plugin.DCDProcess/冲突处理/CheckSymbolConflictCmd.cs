using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.CartographyTools;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataSourcesFile;
using System.IO;

namespace SMGI.Plugin.DCDProcess
{
    public class CheckSymbolConflictCmd : SMGICommand
    {
         public CheckSymbolConflictCmd()
        {
            m_caption = "符号冲突检查";
            m_toolTip = "检查指定目标图层要素符号与其它相关图层符号之间的冲突情况";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;

            }
        }

        public override void OnClick()
        {
            if (m_Application.MapControl.Map.ReferenceScale <= 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;
            }

            CheckSymbolConflictForm frm = new CheckSymbolConflictForm(m_Application.MapControl.Map.ReferenceScale);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string outputFileName = frm.OutputPath + string.Format("\\符号冲突检查_{0}_{1}.shp", frm.ObjFeatureLayer.Name, DateTime.Now.ToString("yyMMdd_HHmm"));

            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                err = DoCheck(outputFileName, frm.ObjFeatureLayer, frm.ConnFeatureLayer, frm.ReferenceScale, wo);
            }

            if (err == "")
            {
                if (File.Exists(outputFileName))
                {
                    if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName);
                        CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
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

        public static string DoCheck(string resultSHPFileName, IFeatureLayer objFeatureLayer, IFeatureLayer connFeatureLayer, double refScale, WaitOperation wo = null)
        {
            string err = "";

            if (objFeatureLayer == null || connFeatureLayer == null)
                return err;

            IFeatureClass tempFC = null;
            Geoprocessor gp = null;
            try
            {
                if (wo != null)
                    wo.SetText(string.Format("正在创建临时数据库......"));
                string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
                IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);
                IFeatureWorkspace fws = ws as IFeatureWorkspace;

                if (wo != null)
                    wo.SetText(string.Format("正在进行冲突检测......"));
                gp = new Geoprocessor();
                gp.OverwriteOutput = true;
                gp.SetEnvironmentValue("workspace", ws.PathName);
                gp.SetEnvironmentValue("referenceScale", refScale);

                DetectGraphicConflict detectGraphicConflict = new DetectGraphicConflict(); 
                detectGraphicConflict.in_features = objFeatureLayer;
                detectGraphicConflict.conflict_features = connFeatureLayer;
                detectGraphicConflict.out_feature_class = string.Format("{0}_DetectGraphicConflict", objFeatureLayer.Name);

                SMGI.Common.Helper.ExecuteGPTool(gp, detectGraphicConflict, null);

                tempFC = (ws as IFeatureWorkspace).OpenFeatureClass(string.Format("{0}_DetectGraphicConflict", objFeatureLayer.Name));


                CheckHelper.DeleteShapeFile(resultSHPFileName);
                if (tempFC != null && tempFC.FeatureCount(null) > 0)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在导出检查结果......"));
                    CheckHelper.ExportFeatureClassToShapefile(tempFC, resultSHPFileName);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                err = ex.Message;               
            }
            finally
            {
                if (tempFC != null)
                {
                    (tempFC as IDataset).Delete();
                    tempFC = null;
                }
            }

            return err;
        }

        
    }
}
