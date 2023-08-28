using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using System.Data;
using SMGI.Plugin.DCDProcess;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 检查道路附属设施线（LFCL）与道路套合关系的检查
    /// 铁路桥（450305）：检查其与LRRL中的要素是否套合；
    /// 公路桥（450306）：检查其与LRDL中的要素是否套合，同时检查其LGB赋值的合法性；
    /// 铁路公路两用桥(450307）：检查其与LRDL、LRRL中的要素是否套合，同时检查其LGB赋值的合法性；
    /// 火车隧道（450601）：检查其与LRRL中的要素是否套合；
    /// 汽车隧道（450602）：检查其与LRDL中的要素是否套合，同时检查其LGB赋值的合法性。
    /// </summary>
    public class CheckLineOverlapLineCmd : SMGI.Common.SMGICommand
    {
        private List<Tuple<string, string, string, string, string>> tts = new List<Tuple<string, string, string, string, string>>(); //配置信息表（单行）
        public static readonly StringBuilder _sbResult = new StringBuilder();
        public static ISpatialReference srf;
        private static List<ErrLineOverLineRelation> ErrLineOverLineList;
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null;
            }
        }

        private DataTable ruleDataTable = null;

        public override void OnClick()
        {
            ErrLineOverLineList = new List<ErrLineOverLineRelation>();
            //LGB检查规则表
            //前置条件检查：已设置参考比例尺
            if (m_Application.MapControl.Map.ReferenceScale == 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;
            }
            IWorkspace workspace = m_Application.Workspace.EsriWorkspace;
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;

            #region 读取配置
            //读取检查配置文件
            ReadConfig();

            if (ruleDataTable.Rows.Count == 0)
            {
                MessageBox.Show("质检内容配置表不存在或内容为空！");
                return;
            }

            string outPutFileName = OutputSetup.GetDir() + string.Format("\\线线套合检查.shp");
            #endregion

            string err = "";

            //执行检查
            Progress progWindow = new Progress();
            progWindow.Show();

            progWindow.lbInfo.Text = "线线套合拓扑检查......";
            System.Windows.Forms.Application.DoEvents();

            var resultMessage = Check(featureWorkspace, tts);

            progWindow.Close();

            //检查输出与现实            
            if (resultMessage.stat != ResultState.Ok)
            {
                MessageBox.Show(resultMessage.msg);
                return;
            }
            else
            {
                //保存质检结果
                resultMessage = SaveResult(OutputSetup.GetDir());
                if (resultMessage.stat != ResultState.Ok)
                {
                    MessageBox.Show(resultMessage.msg);
                    return;
                }
                else
                {
                    //添加质检结果到图面
                    if (MessageBox.Show("检查完成！是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        var strs = (string[])resultMessage.info;
                        CheckHelper.AddTempLayerFromSHPFile(m_Application.Workspace.LayerManager.Map, strs[1]);
                        //System.Diagnostics.Process.Start(strs[0]); //加载txt
                    }
                }
            }

        }

        //读取质检内容配置表
        private void ReadConfig()
        {
            tts.Clear();
            string dbPath = GApplication.Application.Template.Root + @"\质检\质检内容配置.mdb";
            string tableName = "线线套合拓扑检查";
            ruleDataTable = DCDHelper.ReadToDataTable(dbPath, tableName);
            if (ruleDataTable == null)
            {
                return;
            }
            for (int i = 0; i < ruleDataTable.Rows.Count; i++)
            {
                string ptName = (ruleDataTable.Rows[i]["线状设施图层名称"]).ToString();
                string ptSQL = (ruleDataTable.Rows[i]["线状设施条件"]).ToString();
                string relName = (ruleDataTable.Rows[i]["相关图层名称"]).ToString();
                string relSQL = (ruleDataTable.Rows[i]["相关条件"]).ToString();
                string beizhu = (ruleDataTable.Rows[i]["说明"]).ToString();
                Tuple<string, string, string, string, string> tt = new Tuple<string, string, string, string, string>(ptName, ptSQL, relName, relSQL, beizhu);
                tts.Add(tt);
            }
        }

        public ResultMessage Check(IFeatureWorkspace featureWorkspace, List<Tuple<string, string, string, string, string>> tts, Progress progWindow = null)
        {
            foreach (var tt in tts)
            {
                string plSSName = tt.Item1;
                string plSSSQL = tt.Item2;
                string plGLName = tt.Item3;
                string plGLSQL = tt.Item4;
                string beizhu = tt.Item5;
                IFeatureClass plSSFC = null;
                IFeatureClass plGLFC = null;
                try
                {
                    plSSFC = featureWorkspace.OpenFeatureClass(plSSName);
                    plGLFC = featureWorkspace.OpenFeatureClass(plGLName);
                }
                catch (Exception ex)
                {
                    //return new ResultMessage { stat = ResultState.Failed, msg = ex.Message };
                    continue;
                }

                if (plSSFC == null || plGLFC == null)
                {
                    //return new ResultMessage { stat = ResultState.Failed, msg = String.Format("{0} {1} 有空图层", plSSName, plGLName) };
                    continue;
                }

                ISpatialReference plSSRF = (plSSFC as IGeoDataset).SpatialReference;
                if (srf == null)
                {
                    srf = plSSRF;
                }
                string aliasName = plSSFC.AliasName;
                try
                {
                    IQueryFilter ssQF = new QueryFilterClass();
                    // ssQF.WhereClause = plSSFC.HasCollabField() ? plSSSQL + " and " + cmdUpdateRecord.CurFeatureFilter : plSSSQL;
                    ssQF.WhereClause = plSSSQL + " and " + cmdUpdateRecord.CurFeatureFilter;

                    IFeatureCursor feSSCursor = plSSFC.Search(ssQF, false);
                    IFeature feSS = null;
                    int idxSSGB = plSSFC.Fields.FindField("GB");
                    while ((feSS = feSSCursor.NextFeature()) != null)
                    {
                        String ssIDstr = feSS.OID.ToString();

                        //pwindow

                        ISpatialFilter pFilter = new SpatialFilterClass();
                        pFilter.Geometry = feSS.ShapeCopy;
                        pFilter.GeometryField = plSSFC.ShapeFieldName;
                        pFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelWithin;//空间关系
                        // pFilter.WhereClause = plGLFC.HasCollabField() ? plGLSQL + " and " + cmdUpdateRecord.CurFeatureFilter : plGLSQL;
                        pFilter.WhereClause = plGLSQL + " and " + cmdUpdateRecord.CurFeatureFilter;

                        IFeature feGL = null;
                        IFeatureCursor feGLCursor = plGLFC.Search(pFilter, false);
                        bool flag = true;
                        while ((feGL = feGLCursor.NextFeature()) != null)
                        {
                            flag = false;
                            break;
                        }
                        Marshal.ReleaseComObject(feGLCursor);
                        if (flag)
                        {
                            string str = ssIDstr.Trim() + "," + plSSName + "," + beizhu + "\r\n";
                            IPolyline pl = feSS.ShapeCopy as IPolyline;
                            ErrLineOverLineRelation errLineOverLine = new ErrLineOverLineRelation()
                            {
                                PlSSLayerName = plSSName,
                                PlSSID = ssIDstr,
                                RelLayerName = plGLName,
                                RelName = "LineOverLine", //空间关系名
                                Info = beizhu,
                                Pl = pl
                            };
                            ErrLineOverLineList.Add(errLineOverLine);
                            _sbResult.Append(str);
                        }

                    }

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                }

            }
            return new ResultMessage { stat = ResultState.Ok };

        }

        public static ResultMessage SaveResult(string dir, bool autoOver = false, bool toGdb = false)
        {

            if (ErrLineOverLineList.Count > 0)
            {
                //保存txt文件
                string txtFile = dir + "\\线线套合拓扑检查.txt";
                var rusltMessage = TextFileWriter.SaveFile(txtFile, _sbResult.ToString(), autoOver);
                if (rusltMessage.stat != ResultState.Ok) return rusltMessage;

                //保存shp文件
                var shpWriter = new ShapeFileWriter();
                var shpdir = dir + "\\errShp";
                if (!Directory.Exists(shpdir))
                    Directory.CreateDirectory(shpdir);

                var fieldDic = new Dictionary<string, int>() { { "检查项", 40 }, { "设施图层", 10 }, { "设施ID", 10 }, { "关联线图层", 10 }, { "说明", 60 } };

                string shpFile = shpdir + "\\线线套合拓扑检查";
                shpWriter.createErrorResutSHPFile(shpFile, srf, esriGeometryType.esriGeometryPolyline, fieldDic, autoOver);

                foreach (ErrLineOverLineRelation errLineOverLine in ErrLineOverLineList)
                {
                    IPolyline pl = errLineOverLine.Pl;
                    String lyrName = errLineOverLine.PlSSLayerName;
                    shpWriter.addErrorGeometry(pl, new Dictionary<string, string>()
                    {
                        { "检查项", "线线套合拓扑检查" }, 
                        { "设施图层", errLineOverLine.PlSSLayerName }, 
                        { "设施ID", errLineOverLine.PlSSID }, 
                        { "关联线图层", errLineOverLine.RelLayerName }, 
                        { "说明", errLineOverLine.Info }
                    });
                }

                shpWriter.saveErrorResutSHPFile();

                rusltMessage.info = new[] { txtFile, shpFile };
                return rusltMessage;
            }
            else
            {
                return new ResultMessage { stat = ResultState.Cancel, msg = "检查出0条" };
            }
        }
    }

    //待输出错误的结构体
    public class ErrLineOverLineRelation
    {
        public string PlSSLayerName;
        public string PlSSID;
        public string RelLayerName;
        public string RelName;
        public string Info;
        public IPolyline Pl;

    }
}
