using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using System.Data;
using System.IO;
namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 检查道路附属设施线（LFCL）与道路套合关系的检查
    /// 铁路桥（450305）：检查其与LRRL中的要素是否套合；
    /// 公路桥（450306）：检查其与LRDL中的要素是否套合，同时检查其LGB赋值的合法性；
    /// 铁路公路两用桥(450307）：检查其与LRDL、LRRL中的要素是否套合，同时检查其LGB赋值的合法性；
    /// 火车隧道（450601）：检查其与LRRL中的要素是否套合；
    /// 汽车隧道（450602）：检查其与LRDL中的要素是否套合，同时检查其LGB赋值的合法性。
    /// </summary>
    public class CheckLFCL2RoadRegisterHNCmd : SMGI.Common.SMGICommand
    {
        private List<Tuple<string, string, string, string>> tts = new List<Tuple<string, string, string, string>>(); //配置信息表（单行）
        public static readonly StringBuilder _sbResult = new StringBuilder();
        public static ISpatialReference srf;
        private static List<ErrLineOverLineRelation> ErrLineOverLineList = new List<ErrLineOverLineRelation>();

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
            ErrLineOverLineList.Clear();
            string lfclLyrName = "LFCL";
            IGeoFeatureLayer lfclLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && (((l as IGeoFeatureLayer).FeatureClass as IDataset).Name.ToUpper() == lfclLyrName);
            })).FirstOrDefault() as IGeoFeatureLayer;
            if (lfclLyr == null)
            {
                MessageBox.Show(string.Format("当前工作空间中没有找到图层【{0}】!", lfclLyrName));
                return;
            }
            IFeatureClass lfclFC = lfclLyr.FeatureClass;

            string lrdlLyrName = "LRDL";
            IGeoFeatureLayer lrdlLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && (((l as IGeoFeatureLayer).FeatureClass as IDataset).Name.ToUpper() == lrdlLyrName);
            })).FirstOrDefault() as IGeoFeatureLayer;
            if (lrdlLyr == null)
            {
                MessageBox.Show(string.Format("当前工作空间中没有找到图层【{0}】!", lrdlLyrName));
                return;
            }
            IFeatureClass lrdlFC = lrdlLyr.FeatureClass;

            //LGB检查规则表
            string roadMDBPath = m_Application.Template.Root + "\\质检\\质检内容配置.mdb";
            string referenceScale = string.Empty;
            if (m_Application.MapControl.Map.ReferenceScale >= 50000)
            {
                referenceScale = "5W";
            }            
            else
            {
                MessageBox.Show(string.Format("未找到当前设置的参考比例尺【{0}】对应的规则表!", m_Application.MapControl.Map.ReferenceScale));
                return;
            }
            string tableName = "桥隧LGB赋值检查_" + referenceScale;
            var lgbRuleTable = DCDHelper.ReadToDataTable(roadMDBPath, tableName);
            if (lgbRuleTable == null)
            {
                return;
            }

            #region 读取配置
            //读取检查配置文件
            ReadConfig();           
            #endregion           

            //执行检查
            Progress progWindow = new Progress();
            progWindow.Show();

            progWindow.lbInfo.Text = "桥隧LGB赋值检查......";
            System.Windows.Forms.Application.DoEvents();

            IWorkspace workspace = m_Application.Workspace.EsriWorkspace;
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
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
                        System.Diagnostics.Process.Start(strs[0]);
                    }
                }
            }

           
        }

        //读取质检内容配置表
        private void ReadConfig()
        {
            tts.Clear();
            string dbPath = GApplication.Application.Template.Root + @"\质检\质检内容配置.mdb";
            string tableName = "桥隧LGB赋值检查_5W";
            DataTable ruleDataTable = DCDHelper.ReadToDataTable(dbPath, tableName);
            if (ruleDataTable == null)
            {
                return;
            }
            for (int i = 0; i < ruleDataTable.Rows.Count; i++)
            {
                string lfclSQL = (ruleDataTable.Rows[i]["lfclSQL"]).ToString();
                string lrdlSQL = (ruleDataTable.Rows[i]["lrdlSQL"]).ToString();               
                string bridgeLGB = (ruleDataTable.Rows[i]["BridgeLGB"]).ToString();
                string beizhu = (ruleDataTable.Rows[i]["说明"]).ToString();
                Tuple<string, string, string, string> tt = new Tuple<string, string, string, string>(lfclSQL, lrdlSQL, bridgeLGB, beizhu);
                tts.Add(tt);
            }
        }

        //检查
        public ResultMessage Check(IFeatureWorkspace featureWorkspace, List<Tuple< string, string, string, string>> tts, Progress progWindow = null)
        {
            foreach (var tt in tts)
            {
                string lflcSQL = tt.Item1;
                string lrdlSQL = tt.Item2;
                int bridgeLGB = Int32.Parse(tt.Item3);               
                string beizhu = tt.Item4;
                IFeatureClass lfclFC = null;
                IFeatureClass lrdlFC = null;
                try
                {
                   lfclFC = featureWorkspace.OpenFeatureClass("LFCL");
                   lrdlFC = featureWorkspace.OpenFeatureClass("LRDL");
                }
                catch (Exception ex)
                {
                    return new ResultMessage { stat = ResultState.Failed, msg = ex.Message };
                }

                if (lfclFC == null || lrdlFC == null)
                {
                    return new ResultMessage { stat = ResultState.Failed, msg = String.Format("{0} {1} 无效图层", "LFCL", "LRDL") };
                }

                ISpatialReference plSSRF = (lfclFC as IGeoDataset).SpatialReference;
                if (srf == null)
                {
                    srf = plSSRF;
                }
                string aliasName = lfclFC.AliasName;
                try
                {
                    IQueryFilter lfclQF = new QueryFilterClass();
                    lfclQF.WhereClause = lfclFC.HasCollabField() ? lflcSQL + " and " + cmdUpdateRecord.CurFeatureFilter : lflcSQL;
                    IFeatureCursor lfclFeatureCursor = lfclFC.Search(lfclQF, false);
                    IFeature lfclCheckFeature = null;
                    int lgbLFCLindex = lfclFC.Fields.FindField("LGB");
                    while ((lfclCheckFeature = lfclFeatureCursor.NextFeature()) != null)
                    {
                        String ptIDstr = lfclCheckFeature.OID.ToString();

                        //pwindow                        
                        int lfclLGB = -1;
                        Int32.TryParse(lfclCheckFeature.get_Value(lgbLFCLindex).ToString(),out lfclLGB);
                        ISpatialFilter lrdlFilter = new SpatialFilterClass();
                        lrdlFilter.Geometry = lfclCheckFeature.ShapeCopy;
                        lrdlFilter.GeometryField = lfclFC.ShapeFieldName;
                        lrdlFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelWithin ;//空间关系
                        lrdlFilter.WhereClause = lrdlFC.HasCollabField() ? lrdlSQL + " and " + cmdUpdateRecord.CurFeatureFilter : lrdlSQL;

                        IFeature lrdlFeature = null;
                        IFeatureCursor lrdlFeatureCursor = lrdlFC.Search(lrdlFilter, false);
                        
                        while ((lrdlFeature = lrdlFeatureCursor.NextFeature()) != null)
                        {
                            if (lfclLGB != bridgeLGB)
                            {
                                string str = ptIDstr.Trim() + ",LFCL," + beizhu + "\r\n";
                                IPolyline pl = lfclCheckFeature.ShapeCopy as IPolyline;
                                ErrLineOverLineRelation errLineOverLine = new ErrLineOverLineRelation()
                                {
                                    PlSSLayerName = "LFCL",
                                    PlSSID = ptIDstr,
                                    RelLayerName = "LRDL",
                                    RelName = "LGB错误", //空间关系名
                                    Info = beizhu,
                                    Pl = pl
                                };
                                ErrLineOverLineList.Add(errLineOverLine);
                                _sbResult.Append(str);
                                break;
                            }
                            
                        }
                        Marshal.ReleaseComObject(lrdlFeatureCursor);                       

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
                string txtFile = dir + "\\桥隧LGB赋值检查.txt";
                var rusltMessage = TextFileWriter.SaveFile(txtFile, _sbResult.ToString(), autoOver);
                if (rusltMessage.stat != ResultState.Ok) return rusltMessage;

                //保存shp文件
                var shpWriter = new ShapeFileWriter();
                var shpdir = dir + "\\errShp";
                if (!Directory.Exists(shpdir))
                    Directory.CreateDirectory(shpdir);

                var fieldDic = new Dictionary<string, int>() { { "检查项", 40 }, { "设施图层", 10 }, { "设施ID", 10 }, { "关联线图层", 10 }, { "说明", 60 } };

                string shpFile = shpdir + "\\桥隧LGB赋值检查";
                shpWriter.createErrorResutSHPFile(shpFile, srf, esriGeometryType.esriGeometryPolyline, fieldDic, autoOver);

                foreach (ErrLineOverLineRelation errLineOverLine in ErrLineOverLineList)
                {
                    IPolyline pl = errLineOverLine.Pl;
                    String lyrName = errLineOverLine.PlSSLayerName;
                    shpWriter.addErrorGeometry(pl, new Dictionary<string, string>()
                    {
                        { "检查项", "桥隧LGB赋值检查" }, 
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
}
