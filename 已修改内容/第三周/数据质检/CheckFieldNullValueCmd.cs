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

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 属性空值检查
    /// </summary>
    public class CheckFieldNullValueCmd : SMGICommand
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
            var frm = new CheckLayerWithFieldsForm(m_Application, false);
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Text = "属性空值检查";

            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string outputFileName = frm.OutputPath + string.Format("\\{0}_{1}.txt", frm.Text, frm.CheckFeatureClass.AliasName);

            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                Dictionary<IFeatureClass, List<string>> fc2FieldList = new Dictionary<IFeatureClass, List<string>>();
                fc2FieldList.Add(frm.CheckFeatureClass, frm.FieldNameList);

                err = DoCheck(outputFileName, fc2FieldList, wo);
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
        /// 属性空值检查
        /// </summary>
        /// <param name="resultTxtFileName"></param>
        /// <param name="fc2FieldList"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public static string DoCheck(string resultTxtFileName, Dictionary<IFeatureClass, List<string>> fc2FieldList, WaitOperation wo = null)
        {
            string err = "";

            try
            {
                FileStream resultFS = null;
                StreamWriter resultSW = null;

                
                foreach (var kv in fc2FieldList)
                {
                    IFeatureClass fc = kv.Key;
                    List<string> fieldList = kv.Value;

                    IQueryFilter qf = new QueryFilterClass();
                    // if (fc.HasCollabField())
                        qf.WhereClause = cmdUpdateRecord.CurFeatureFilter;

                    if (wo != null)
                        wo.SetText(string.Format("正在对要素类【{0}】进行属性空值检查......", fc.AliasName));

                    Dictionary<int, List<string>> oid2FieldList = new Dictionary<int, List<string>>();
                    #region 检查
                    IFeatureCursor feCursor = fc.Search(qf, true);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        foreach (var fd in fieldList)
                        {
                            int fdIndex = fc.FindField(fd);
                            if (fdIndex == -1)
                                continue;

                            var fdValue = fe.get_Value(fdIndex);
                            if (Convert.IsDBNull(fdValue))
                            {
                                if(oid2FieldList.ContainsKey(fe.OID))
                                {
                                    oid2FieldList[fe.OID].Add(fd);
                                }
                                else
                                {
                                    List<string> fdList = new List<string>();
                                    fdList.Add(fd);

                                    oid2FieldList.Add(fe.OID, fdList);
                                }
                            }
                        }
                    }
                    Marshal.ReleaseComObject(feCursor);
                    #endregion

                    if (oid2FieldList.Count > 0)
                    {
                        if (resultSW == null)
                        {
                            //建立结果文件
                            resultFS = System.IO.File.Open(resultTxtFileName, System.IO.FileMode.Create);
                            resultSW = new StreamWriter(resultFS, Encoding.Default);
                        }

                        //写入结果文件
                        resultSW.WriteLine(string.Format("要素类【{0}】的属性空值检查结果【{1}】：", fc.AliasName, oid2FieldList.Count));
                        foreach (var item in oid2FieldList)
                        {
                            string oidErrString = "";
                            foreach (var fd in item.Value)
                            {
                                if (oidErrString == "")
                                {
                                    oidErrString = string.Format("要素{0}：{1}", item.Key, fd);
                                }
                                else
                                {
                                    oidErrString += string.Format(",{0}", fd);
                                }
                            }

                            resultSW.WriteLine(oidErrString);
                        }

                        resultSW.WriteLine(string.Format(""));//要素类之间隔一行
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

        
    }
}
