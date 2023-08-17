using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using SMGI.Common;

namespace SMGI.Plugin.DCDProcess
{
    public partial class RoadGradeConnChkFrm : Form
    {
        private IFeatureClass _roadFC;

        /// <summary>
        /// 检查的道路等级
        /// </summary>
        public long Grade
        {
            get
            {
                return long.Parse(cbGrade.Text);
            }
        }

        /// <summary>
        /// 输出路径
        /// </summary>
        public string OutFilePath
        {
            get
            {
                return tbOutFilePath.Text;
            }
        }

        public RoadGradeConnChkFrm(IFeatureClass roadFC)
        {
            InitializeComponent();

            _roadFC = roadFC;
        }

        private void RoadGradeConnChkFrm_Load(object sender, EventArgs e)
        {
            IFeatureCursor cursor = _roadFC.Search(null, false);
            IDataStatistics datasta = new DataStatisticsClass();
            datasta.Field = GApplication.Application.TemplateManager.getFieldAliasName("GRADE", _roadFC.AliasName);
            datasta.Cursor = cursor as ICursor;
            System.Collections.IEnumerator enumvartor = datasta.UniqueValues; //枚举
            int RecordCount = datasta.UniqueValueCount;
            string[] strvalue = new string[RecordCount];
            enumvartor.Reset();
            List<long> gradeList = new List<long>();
            while (enumvartor.MoveNext())
            {
                long grade = long.Parse(enumvartor.Current.ToString());
                gradeList.Add(grade);
            }
            Marshal.ReleaseComObject(cursor);

            gradeList.Sort();
            foreach (var item in gradeList)
            {
                cbGrade.Items.Add(item.ToString());
            }
            if (gradeList.Count > 0)
                cbGrade.SelectedIndex = 0;

            tbOutFilePath.Text = OutputSetup.GetDir();
        }

        private void btnFilePath_Click(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK && fd.SelectedPath.Length > 0)
            {
                tbOutFilePath.Text = fd.SelectedPath;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {

            try
            {
                long grade = long.Parse(cbGrade.Text);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show("请选择预检查的道路等级！");
                return;
            }

            if (tbOutFilePath.Text == "")
            {
                MessageBox.Show("请指定输出路径！");
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
