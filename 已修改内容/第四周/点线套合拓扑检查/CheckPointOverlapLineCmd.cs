using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using System.Data;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geometry;
using SMGI.Plugin.DCDProcess;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public class CheckPointOverlapLineCmd : SMGI.Common.SMGICommand
    {
        private List<Tuple<string, string, string, string, string>> tts = new List<Tuple<string, string, string, string, string>>(); //配置信息表（单行）

        public CheckPointOverlapLineCmd()
        {
            //tts.Add(new Tuple<string, string, string, string, string>("LFCP", "GB=410301", "LRRL", "GB in (410104,410105)", "火车站在铁路线上"));
            //tts.Add(new Tuple<string, string, string, string, string>("LFCP", "GB=410302", "LRRL", "GB in (410106,410107)", "高铁站在高铁线上"));
            //tts.Add(new Tuple<string, string, string, string, string>("LFCP", "GB=450101", "LRRL", "GB=430101", "地铁站在地铁线上"));
            //tts.Add(new Tuple<string, string, string, string, string>("LFCP", "GB=450102", "LRRL", "GB in (430102,430103)", "城铁、磁悬浮站在城铁、磁悬浮线上"));
            //tts.Add(new Tuple<string, string, string, string, string>("LFCP", "GB=450106", "LRDL", "GB in (420901,420902)", "收费站在高速公路上"));
            //tts.Add(new Tuple<string, string, string, string, string>("LFCP", "GB=450308", "LRDL", "GB in (420901,420902)", "互通在高速公路上"));
        }

        private DataTable ruleDataTable = null;

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;
            }
        }

        public override void OnClick()
        {
            //前置条件检查：已设置参考比例尺
            if (m_Application.MapControl.Map.ReferenceScale == 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;
            }
            IWorkspace workspace = m_Application.Workspace.EsriWorkspace;
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;

            //读取检查配置文件
            ReadConfig();

            if (ruleDataTable.Rows.Count == 0)
            {
                MessageBox.Show("质检内容配置表不存在或内容为空！");
                return;
            }

            //执行检查
            Progress progWindow = new Progress();
            progWindow.Show();

            progWindow.lbInfo.Text = "点线套合拓扑检查......";
            System.Windows.Forms.Application.DoEvents();

            var resultMessage = CheckPointOverlapLineCheck.Check(featureWorkspace, tts);

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
                resultMessage = CheckPointOverlapLineCheck.SaveResult(OutputSetup.GetDir());
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
                        //System.Diagnostics.Process.Start(strs[0]);
                    }
                }
            }
            CheckPointOverlapLineCheck.Clear();

        }

        //读取质检内容配置表
        private void ReadConfig()
        {
            tts.Clear();
            string dbPath = GApplication.Application.Template.Root + @"\质检\质检内容配置.mdb";
            string tableName = "点线套合拓扑检查";
            ruleDataTable = DCDHelper.ReadToDataTable(dbPath, tableName);
            if (ruleDataTable == null)
            {
                return;
            }
            for (int i = 0; i < ruleDataTable.Rows.Count; i++)
            {
                string ptName = (ruleDataTable.Rows[i]["点层名称"]).ToString();
                string ptSQL = (ruleDataTable.Rows[i]["点层条件"]).ToString();
                string relName = (ruleDataTable.Rows[i]["关联层名称"]).ToString();
                string relSQL = (ruleDataTable.Rows[i]["关联层条件"]).ToString();
                string beizhu = (ruleDataTable.Rows[i]["备注"]).ToString();
                Tuple<string, string, string, string, string> tt = new Tuple<string, string, string, string, string>(ptName, ptSQL, relName, relSQL, beizhu);
                tts.Add(tt);
            }
        }

    }
}
