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
using SMGI.Plugin.DCDProcess;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 数据库结构检查
    /// 1.对比目标数据库与模板数据库中要素类的个数和名称是否一一对应；
    /// 2.对比目标数据库与模板数据库中对应的要素类字段结构、空间参考是否一致
    /// </summary>
    public class CheckDataBaseStructCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null;
            }
        }

        public override void OnClick()
        {
            CheckDataBaseStructForm frm = new CheckDataBaseStructForm();
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            string outPutFileName = frm.OutputPath + string.Format("\\数据库结构检查_{0}.txt", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));

            if (!CheckHelper.IsValidGeoDataBase(frm.ObjDataBase))
            {
                MessageBox.Show("目标数据库不识别！");
                return;
            }
            if (!CheckHelper.IsValidGeoDataBase(frm.TemplateDataBase))
            {
                MessageBox.Show("模板数据库不识别！");
                return;
            }

            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
                var objWS = wsFactory.OpenFromFile(frm.ObjDataBase, 0) as IFeatureWorkspace;
                var templateWS = wsFactory.OpenFromFile(frm.TemplateDataBase, 0) as IFeatureWorkspace;

                err = DoCheck(outPutFileName, objWS, templateWS, wo);

                if (err != "")
                {
                    MessageBox.Show(err);
                }
                else
                {
                    if (File.Exists(outPutFileName))
                    {
                        if (MessageBox.Show("检查完成，是否打开检查结果文档？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(outPutFileName);
                        }
                    }
                    else
                    {
                        MessageBox.Show("检查完成，未发现异常！");
                    }
                }
            }
        }


        public static string DoCheck(string outPutFileName, IFeatureWorkspace objWS, IFeatureWorkspace templateWS, WaitOperation wo = null)
        {
            string err = "";

            Dictionary<string, string> logInfo = new Dictionary<string, string>();//要素类名称2检查结果
            try
            {
                Dictionary<string, IFeatureClass> obj_fcName2FC = CheckHelper.GetAllFeatureClass(objWS);
                Dictionary<string, IFeatureClass> template_fcName2FC = CheckHelper.GetAllFeatureClass(templateWS);

                Dictionary<IFeatureClass, IFeatureClass> objFC2TemplateFC = new Dictionary<IFeatureClass, IFeatureClass>();//能找到对应关系的要素类集合

                //1.检查模板数据库中的要素类在目标数据库中是否都存在
                foreach (var item in template_fcName2FC)
                {
                    if (obj_fcName2FC.ContainsKey(item.Key))
                    {
                        objFC2TemplateFC.Add(obj_fcName2FC[item.Key], item.Value);
                    }
                    else
                    {
                        logInfo.Add(item.Key, "\t目标数据库中缺少该要素类。\r\n\r\n");
                        
                    }
                }

                //2.检查目标数据库中的要素类是否在模板数据库中能找到对应的要素类
                foreach (var item in obj_fcName2FC)
                {
                    if (!template_fcName2FC.ContainsKey(item.Key))
                    {
                        logInfo.Add(item.Key, "\t该要素类在模板数据库中不存在。\r\n\r\n");
                    }
                }

                //3.检查对应要素类的空间参考、字段结构是否一致
                foreach (var item in objFC2TemplateFC)
                {
                    string fcName = item.Key.AliasName;

                    if (wo != null)
                        wo.SetText(string.Format("正在检查要素类【{0}】的空间参考信息......", fcName));

                    // 1.空间参考检查
                    var objSR = (item.Key as IGeoDataset).SpatialReference;
                    var templateSR = (item.Value as IGeoDataset).SpatialReference;

                    string info = CompareSpatialReference(objSR, templateSR);
                    if(info != "")
                        info = string.Format("\t{0}\r\n", info);

                    // 2.要素类几何类型判断
                    if (item.Key.ShapeType != item.Value.ShapeType)
                    {
                        info += string.Format("\t几何类型不匹配。\r\n");
                    }

                    if (wo != null)
                        wo.SetText(string.Format("正在检查要素类【{0}】的字段结构信息......", item.Key.AliasName));
                    
                    // 3.字段结构检查
                    info += CompareFieldsStruct(item.Key, item.Value);
                    if (info != "")
                        logInfo.Add(fcName, string.Format("{0}\r\n", info));
                }

                // 输出结果信息
                if (logInfo.Count > 0)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在输出检查结果文件......"));

                    StringBuilder sb = new StringBuilder();
                    foreach (var item in logInfo)
                    {
                        sb.Append(string.Format("图层【{0}】:\r\n", item.Key ));
                        sb.Append(item.Value);
                    }
                    err = SaveFile(outPutFileName, sb.ToString(), true);
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
        /// 比较两空间参考是否一致
        /// </summary>
        /// <param name="objSR"></param>
        /// <param name="templateSR"></param>
        /// <returns></returns>
        public static string CompareSpatialReference(ISpatialReference objSR, ISpatialReference templateSR)
        {
            string result = "";

            IClone objClone = objSR as IClone;
            IClone templateClone = templateSR as IClone;
            if (!objClone.IsEqual(templateClone))
                result = "空间参考不匹配。";

            //ICompareCoordinateSystems compare = objSR as ICompareCoordinateSystems;
            //if (!compare.IsEqualNoVCS(templateSR))
            //    result = "空间参考不匹配！";

            return result;
        }

        /// <summary>
        /// 比较两要素类的字段结构等是否一致
        /// </summary>
        /// <param name="objFC"></param>
        /// <param name="templateFC"></param>
        /// <returns></returns>
        public static string CompareFieldsStruct(IFeatureClass objFC, IFeatureClass templateFC)
        {
            StringBuilder sb = new StringBuilder();

            var objFields = objFC.Fields;
            var templateFields = templateFC.Fields;


            //遍历模板要素类中的字段结构
            for (var i = 0; i < templateFields.FieldCount; i++)
            {
                var fd = templateFields.Field[i];
                var index = objFields.FindField(fd.Name);                
                if (index == -1)
                {
                    sb.Append(string.Format("\t字段{0}:不存在\r\n", fd.Name));

                    continue;
                }

                var objFD = objFields.Field[index];

                string info = "";
                if (fd.Name != objFD.Name)
                {
                    info += "名称大小写不匹配；";
                }
                if (fd.AliasName != objFD.AliasName)
                {
                    info += "别名不匹配；";
                }
                if (fd.Type != objFD.Type)
                {
                    info += "类型不匹配；";
                }
                if (fd.IsNullable != objFD.IsNullable)
                {
                    info += "是否可为空不匹配；";
                }
                if (fd.Length != objFD.Length)
                {
                    info += "长度不匹配；";
                }

                if (info != "")
                {
                    sb.Append(string.Format("\t字段{0}:{1}\r\n", fd.Name, info));
                }
            }

            if (templateFields.FieldCount != objFields.FieldCount)
            {
                sb.Append(string.Format("\t字段个数不匹配\r\n"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 保存文本文件
        /// </summary>
        /// <param name="filepath">文件路径</param>
        /// <param name="content">文本内容</param>
        ///  <param name="autoOver">是否自动覆盖</param>
        /// <returns></returns>
        public static string SaveFile(string filepath, string content, bool autoOver = false)
        {
            if (File.Exists(filepath) && !autoOver)
            {
                if (DialogResult.No == MessageBox.Show(string.Format("文件【{0}】已经存在！是否替换？", filepath), "", MessageBoxButtons.YesNo))
                {
                    return "检查结果文件写入失败！";
                }

                File.Delete(filepath);
            }

            try
            {
                var strm = File.OpenWrite(filepath);
                using (var wr = new StreamWriter(strm))
                {
                    wr.Write(content);
                }
                strm.Close();
            }
            catch (IOException ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return string.Format("检查结果文件写入失败:{0}", ex.Message);
            }
            
            return "";
        }
    }
}
