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

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 空几何检查
    /// </summary>
    public class CheckNullGeoCmd : SMGICommand
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
            var frm = new CheckLayerSelectForm(m_Application, true, true, true);
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Text = "空几何检查";

            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string outputFileName;
            if (frm.CheckFeatureLayerList.Count > 1)
            {
                outputFileName = OutputSetup.GetDir() + string.Format("\\{0}.txt", frm.Text);
            }
            else
            {
                outputFileName = OutputSetup.GetDir() + string.Format("\\{0}_{1}.txt", frm.Text, frm.CheckFeatureLayerList.First().Name);
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

                err = DoCheck(outputFileName, fcList, wo);
            }

            if (err == "")
            {
                if (File.Exists(outputFileName))
                {
                    if (MessageBox.Show("是否打开检查结果文档？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(outputFileName);
                    }
                }
                else
                {
                    MessageBox.Show("检查完毕，没有发现空几何要素！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }

        }
        
        /// <summary>
        /// 空几何检查
        /// </summary>
        /// <param name="resultTxtFileName"></param>
        /// <param name="fcList"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public static string DoCheck(string resultTxtFileName, List<IFeatureClass> fcList, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                FileStream resultFS = null;
                StreamWriter resultSW = null;

                foreach (var fc in fcList)
                {
                    IQueryFilter qf = new QueryFilterClass();
                    if (fc.HasCollabField())
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                    if (wo != null)
                        wo.SetText(string.Format("正在对要素类【{0}】进行空几何检查......", fc.AliasName));

                    //核查该图层的空几何要素
                    List<int> errOIDList = CheckNullGeo(fc, qf, wo);
                    if (errOIDList.Count > 0)
                    {
                        if (resultSW == null)
                        {
                            //建立结果文件
                            resultFS = System.IO.File.Open(resultTxtFileName, System.IO.FileMode.Create);
                            resultSW = new StreamWriter(resultFS, Encoding.Default);
                            resultSW.WriteLine(string.Format("空几何检查结果："));
                        }

                        //写入结果文件
                        string oidListString = "";
                        foreach (var oid in errOIDList)
                        {
                            if(oidListString == "")
                            {
                                oidListString = oid.ToString();
                            }
                            else
                            {
                                oidListString += string.Format(",{0}",oid);
                            }
                        }
                        resultSW.WriteLine(string.Format("【{0}】:{1}", fc.AliasName, oidListString));
                    }

                }

                //保存结果文件
                if (resultSW != null)
                {
                    resultSW.Flush();
                    resultFS.Close();
                }
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

        public static List<int> CheckNullGeo(IFeatureClass fc, IQueryFilter qf, WaitOperation wo = null)
        {
            List<int> result = new List<int>();

            IFeatureCursor feCursor = fc.Search(qf, true);
            IFeature fe;
            while ((fe = feCursor.NextFeature()) != null)
            {
                if (wo != null)
                    wo.SetText(string.Format("正在检查要素类【{0}】中的要素【{1}】......", fc.AliasName, fe.OID));

                if (fe.Shape == null || fe.Shape.IsEmpty)
                {
                    result.Add(fe.OID);
                }
            }
            Marshal.ReleaseComObject(feCursor);

            return result;
        }
    }
}
