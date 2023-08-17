using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.DataSourcesGDB;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;


namespace SMGI.Plugin.DCDProcess.DataProcess
{
    //道路选取等级赋初值（字段名：GRADE2）
    //参考配置表：规则配置.mdb\道路选取等级赋初值
    //2022.3.2 修改 张怀相
    //功能修改：
    //  1、执行后加载对话框 GRADEListFrm
    //  2、对话框里设置 是否 采用 特定等级，默认采用特定等级
    //  3、从下拉列表里选择特定等级，程序只对符合该等级的河流进行GRADE2赋值
    public class RoadDataRankClass : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null 
                    && m_Application.Workspace != null 
                    && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        
        public override void OnClick()
        {  
            string templatecp = GApplication.Application.Template.Root + @"\规则配置.mdb";
            string tableName = "道路选取等级赋初值";

            //读取配置表： 道路选取等级赋初值
            DataTable ruleDataTable = DCDHelper.ReadToDataTable(templatecp, tableName);
            if (ruleDataTable == null)
            {
                //MessageBox.Show(String.Format("规则配置表：{0} 无效", tableName));
                return;
            }

            //从配置表中获取所有的等级（排除1-4级）
            List<int> ranks = new List<int>();
            for (int i = 0; i < ruleDataTable.Rows.Count; i++)
            {
                int rank = Int32.Parse((ruleDataTable.Rows[i]["GRADE2"]).ToString());
                if (rank <= 4) 
                    continue;
                if (!ranks.Contains(rank))
                { ranks.Add(rank); }
            }
            ranks.Sort();

            //设置等级选取对话框
            GradeListFrm frm = new GradeListFrm("道路选取等级GRADE2", ranks);

            if (frm.ShowDialog() == DialogResult.OK)
            {
                if (frm.OneGrade)
                {
                    DataRankHelper.GradeClass(SMGI.Common.GApplication.Application, ruleDataTable, "GRADE2", frm.Grade, true);
                }
                else
                {
                    DataRankHelper.GradeClass(SMGI.Common.GApplication.Application, ruleDataTable); 
                }
            }
        }       
    }
}
