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

namespace SMGI.Plugin.DCDProcess
{
    public partial class RiverGrade2ConnChkFrm : Form
    {
        private IFeatureClass _riverFC;

        /// <summary>
        /// 检查的水系几何等级
        /// </summary>
        public long GradeTwo
        {
            get
            {
                return long.Parse(cbGrade2.Text);
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

        public RiverGrade2ConnChkFrm(IFeatureClass fc)
        {
            InitializeComponent();

            _riverFC = fc;
        }

        private void RiverGrade2ConnChkFrm_Load(object sender, EventArgs e)
        {
            IFeatureCursor cursor = _riverFC.Search(null, false);
            IDataStatistics datasta = new DataStatisticsClass();
            datasta.Field = "GRADE2";
            datasta.Cursor = cursor as ICursor;
            System.Collections.IEnumerator enumvartor = datasta.UniqueValues; //枚举
            int RecordCount = datasta.UniqueValueCount;
            string[] strvalue = new string[RecordCount];
            enumvartor.Reset();
            List<long> grade2List = new List<long>();
            while (enumvartor.MoveNext())
            {
                long grade = long.Parse(enumvartor.Current.ToString());
                grade2List.Add(grade);
            }
            Marshal.ReleaseComObject(cursor);

            grade2List.Sort();
            foreach (var item in grade2List)
            {
                cbGrade2.Items.Add(item.ToString());
            }
            if (grade2List.Count > 0)
                cbGrade2.SelectedIndex = 0;

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
                long grade = long.Parse(cbGrade2.Text);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show("请选择预检查的水系几何等级！");
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
