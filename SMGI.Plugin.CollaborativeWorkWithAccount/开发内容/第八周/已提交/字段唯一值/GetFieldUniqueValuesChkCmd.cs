using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using SMGI.Plugin.DCDProcess;


namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 悬挂点检查（端点离线，点不在线上打断的情况：点所在线可能相离，也可能相交）
    /// </summary>
    public class GetFieldUniqueValuesChkCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;
            }
        }

        public override void OnClick()
        {
            string outputFileName = OutputSetup.GetDir() + string.Format("\\字段唯一值_{0}.txt", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));

            //图层选择、最大距离对话框
            var layerSelectForm = new GetFieldUniqueValuesChkCheckLayerWithFieldsForm(m_Application)
            {
            };

            
            if (layerSelectForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            //检查图层
            List<string> _fieldNameList = new List<string>();
            IFeatureClass featureClass = null;
            
            featureClass = layerSelectForm.CheckFeatureClass;
            _fieldNameList = layerSelectForm.FieldNameList;


            using (var wo = m_Application.SetBusy())
            {
                FileStream resultFS = null;
                StreamWriter resultSW = null;

                foreach (string fieldName in _fieldNameList)
                {
                    // 获取字段索引
                    int fieldIndex = featureClass.Fields.FindField(fieldName);

                    // 创建一个字典来存储唯一值
                    Dictionary<string, bool> uniqueValues = new Dictionary<string, bool>();

                    // 使用游标遍历要素并获取唯一值
                    IFeatureCursor cursor = featureClass.Search(null, true);
                    IFeature feature = cursor.NextFeature();
                    while (feature != null)
                    {
                        // 获取字段值
                        object value = feature.get_Value(fieldIndex);

                        // 将字段值转换为字符串
                        string strValue = value != null ? value.ToString() : "";

                        // 将唯一值添加到字典中
                        if (!uniqueValues.ContainsKey(strValue) && !String.IsNullOrEmpty(strValue))
                        {
                            uniqueValues.Add(strValue, true);
                        }

                        // 获取下一个要素
                        feature = cursor.NextFeature();
                    }

                    if (resultSW == null)
                    {
                        //建立结果文件
                        resultFS = File.Open(outputFileName, System.IO.FileMode.Create);
                        resultSW = new StreamWriter(resultFS, Encoding.Default);
                    }

                    resultSW.WriteLine(string.Format("【图层" + featureClass.AliasName + "的字段" + fieldName + "】:"));

                    //写入结果文件
                    foreach (string uniqueValue in uniqueValues.Keys)
                    {
                        string errString = uniqueValue;
                        resultSW.WriteLine(string.Format(errString));
                    }
                }

                //保存结果文件
                if (resultSW != null)
                {
                    resultSW.Flush();
                    resultFS.Close();
                }

                if (File.Exists(outputFileName))
                {
                    if (MessageBox.Show("是否打开检查结果文档？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(outputFileName);
                    }
                }
                else
                {
                    MessageBox.Show("检查完毕，图层" + featureClass.AliasName + "的所选字段均为空！");
                }
            }
        }
    }
}
