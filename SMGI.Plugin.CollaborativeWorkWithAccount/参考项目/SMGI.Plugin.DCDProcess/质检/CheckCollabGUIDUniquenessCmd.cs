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
using System.Data;
using ESRI.ArcGIS.DataSourcesGDB;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// GUID唯一性检查：检查数据库中所有编辑要素的GUID是否唯一
    /// </summary>
    public class CheckCollabGUIDUniquenessCmd : SMGICommand
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
            string outputFileName = OutputSetup.GetDir() + string.Format("\\GUID唯一性检查_{0}.txt", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));

            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                err = DoCheck(outputFileName, m_Application.Workspace.EsriWorkspace, wo);
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
                    MessageBox.Show("检查完毕，没有发现异常！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }
        }

        public static string DoCheck(string resultTxtFileName, IWorkspace ws, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                FileStream resultFS = null;
                StreamWriter resultSW = null;

                Dictionary<string, List<KeyValuePair<string, int>>> guid2EditFIDList = getEditedFeatureInfo(ws);
                foreach (var kv in guid2EditFIDList)
                {
                    if (kv.Value.Count == 1)
                        continue;

                    if (resultSW == null)
                    {
                        //建立结果文件
                        resultFS = System.IO.File.Open(resultTxtFileName, System.IO.FileMode.Create);
                        resultSW = new StreamWriter(resultFS, Encoding.Default);
                        resultSW.WriteLine(string.Format("GUID唯一性检查结果："));
                    }

                    //写入结果文件
                    string errString = "";
                    foreach (var item in kv.Value)
                    {
                        if (errString == "")
                        {
                            errString = string.Format("{0}|{1}", item.Key,item.Value);
                        }
                        else
                        {
                            errString += string.Format(", {0}|{1}", item.Key, item.Value);
                        }
                    }
                    resultSW.WriteLine(string.Format("【{0}】不唯一： {1}", kv.Key, errString));
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns>Dictionary<GUID,List<KeyValuePair<FCName,OID>>></returns>
        public static Dictionary<string, List<KeyValuePair<string, int>>> getEditedFeatureInfo(IWorkspace ws)
        {
            Dictionary<string, List<KeyValuePair<string, int>>> result = new Dictionary<string, List<KeyValuePair<string, int>>>();

            Dictionary<string, IFeatureClass> fcName2FC = DCDHelper.GetAllFeatureClassFromWorkspace(ws as IFeatureWorkspace);
            foreach (var kv in fcName2FC)
            {
                IFeatureClass fc = kv.Value;
                int guidIndex = fc.FindField(cmdUpdateRecord.CollabGUID);
                int verIndex = fc.FindField(cmdUpdateRecord.CollabVERSION);
                int delIndex = fc.FindField(cmdUpdateRecord.CollabDELSTATE);
                if (guidIndex == -1 || verIndex == -1 || delIndex == -1)
                    continue;

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = string.Format("{0} < 0", cmdUpdateRecord.CollabVERSION);
                IFeatureCursor fCursor = fc.Search(qf, true);

                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    string guid = f.get_Value(guidIndex).ToString();
                    if (!result.ContainsKey(guid))
                    {
                        List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
                        list.Add(new KeyValuePair<string, int>(fc.AliasName, f.OID));

                        result.Add(guid, list);
                    }
                    else
                    {
                        result[guid].Add(new KeyValuePair<string, int>(fc.AliasName, f.OID));
                    }

                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
            }

            return result;
        }
        
    }
}
