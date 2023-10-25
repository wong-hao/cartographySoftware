using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Collections;

namespace SMGI.Plugin.DCDProcess
{
    public partial class GeneralizationPretreatmentFrm : Form
    {
        /// <summary>
        /// 数据库名称
        /// </summary>
        public string GDBFilePath
        {
            get
            {
                return tbOriginDB.Text;
            }
        }

        /// <summary>
        /// 目标数据库比例尺
        /// </summary>
        public double Scale
        {
            get
            {
                return double.Parse(cbObjScale.Text);
            }
        }

        /// <summary>
        /// 需删除的水系SQL语句
        /// </summary>
        public string RiverDelSQLText
        {
            get
            {
                return _riverDelSQLText;
            }
        }
        private string _riverDelSQLText;

        /// <summary>
        /// 需删除的道路SQL语句
        /// </summary>
        public string RoadDelSQLText
        {
            get
            {
                return _roadDelSQLText;
            }
        }
        private string _roadDelSQLText;


        /// <summary>
        /// 简化参数
        /// </summary>
        public double BlendValue
        {
            get
            {
                return double.Parse(txtBend.Text) * 0.001 * Scale;
            }
        }

        /// <summary>
        /// 光滑参数
        /// </summary>
        public double SmoothValue
        {
            get
            {
                return double.Parse(txtSmooth.Text) * 0.001 * Scale;
            }
        }

        #region 可以把这些常量信息放置在外部配置文件中
        /// <summary>
        /// 水系要素类名称
        /// </summary>
        public string RiverFeatureClassName
        {
            get
            {
                return "HYDL";
            }
        }

        /// <summary>
        /// 水系几何等级字段名称
        /// </summary>
        public string RiverGradeFieldName
        {
            get
            {
                return GApplication.Application.TemplateManager.getFieldAliasName("GRADE2", RiverFeatureClassName);
            }
        }

        /// <summary>
        /// 河流伪节点处理标识字段名称集合
        /// </summary>
        public List<string> RiverFieldArray
        {
            get;
            set; 
        }

        

        /// <summary>
        /// 道路要素类名称
        /// </summary>
        public string RoadFeatureClassName
        {
            get
            {
                return "LRDL";
            }
        }

        /// <summary>
        /// 道路等级字段名称
        /// </summary>
        public string RoadGradeFieldName
        {
            get
            {
                return GApplication.Application.TemplateManager.getFieldAliasName("GRADE", RoadFeatureClassName);
            }
        }

        /// <summary>
        /// 道路伪节点处理标识字段名称集合
        /// </summary>
        public List<string> RoadFieldArray
        {
            get;
            set;
        }

        /// <summary>
        /// 需化简的线图层集合
        /// </summary>
        public List<string> NeedSimplifiedFCNameList
        {
            get;
            set;
        }
        #endregion

        public GeneralizationPretreatmentFrm()
        {
            InitializeComponent();

            _riverDelSQLText = "";
            _roadDelSQLText = "";
        }

        private void GeneralizationPretreatmentFrm_Load(object sender, EventArgs e)
        {

        }

        private void btOrgin_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择GDB工程文件夹";
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!GApplication.GDBFactory.IsWorkspace(fbd.SelectedPath))
                {
                    MessageBox.Show("不是有效地GDB文件");
                    return;
                }

                tbOriginDB.Text = fbd.SelectedPath;

                //获取水系、道路的等级信息
                using (var wo = GApplication.Application.SetBusy())
                {
                    IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactoryClass();
                    IFeatureWorkspace fWS = workspaceFactory.OpenFromFile(fbd.SelectedPath, 0) as IFeatureWorkspace;

                    if (!(fWS as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, RiverFeatureClassName))
                    {
                        MessageBox.Show(string.Format("数据库【{0}】中没有要素类【{1}】", fbd.SelectedPath, RiverFeatureClassName));
                        return;
                    }

                    if (!(fWS as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, RoadFeatureClassName))
                    {
                        MessageBox.Show(string.Format("数据库【{0}】中没有要素类【{1}】", fbd.SelectedPath, RoadFeatureClassName));
                        return;
                    }

                    IFeatureClass hydlFC = fWS.OpenFeatureClass(RiverFeatureClassName);
                    IFeatureClass lrdlFC = fWS.OpenFeatureClass(RoadFeatureClassName);

                    int hydlGradeIndex = hydlFC.FindField(RiverGradeFieldName);
                    if (hydlGradeIndex == -1)
                    {
                        MessageBox.Show(string.Format("要素类【{0}】中没有找到等级字段【{1}】", RiverFeatureClassName, RiverGradeFieldName));
                        return;
                    }

                    int lrdlGradeIndex = lrdlFC.FindField(RoadGradeFieldName);
                    if (lrdlGradeIndex == -1)
                    {
                        MessageBox.Show(string.Format("要素类【{0}】中没有找到等级字段【{1}】", RoadFeatureClassName, RoadGradeFieldName));
                        return;
                    }

                    List<string> fieldNames = new List<string>();

                    //初始化水系字段名称选择框
                    for (int i = 0; i < hydlFC.Fields.FieldCount; i++)
                    {
                        var field = hydlFC.Fields.get_Field(i);
                        if (field.Type == esriFieldType.esriFieldTypeGeometry || field.Type == esriFieldType.esriFieldTypeOID)
                        {
                            continue;
                        }
                        if (field.Name.ToUpper() == "SHAPE_LENGTH" || field.Name.ToUpper() == "SHAPE_AREA")
                        {
                            continue;
                        }
                        if (field.Name.ToUpper().StartsWith("SMGI"))
                        {
                            continue;
                        }

                        fieldNames.Add(field.Name);
                    }
                    chkriverField.Items.AddRange(fieldNames.ToArray());


                    fieldNames = new List<string>();

                    //初始化道路字段名称选择框
                    for (int i = 0; i < lrdlFC.Fields.FieldCount; i++)
                    {
                        var field = lrdlFC.Fields.get_Field(i);
                        if (field.Type == esriFieldType.esriFieldTypeGeometry || field.Type == esriFieldType.esriFieldTypeOID)
                        {
                            continue;
                        }
                        if (field.Name.ToUpper() == "SHAPE_LENGTH" || field.Name.ToUpper() == "SHAPE_AREA")
                        {
                            continue;
                        }
                        if (field.Name.ToUpper().StartsWith("SMGI"))
                        {
                            continue;
                        }

                        fieldNames.Add(field.Name);
                    }
                    chkRoadField.Items.AddRange(fieldNames.ToArray());


                    //初始化水系等级选择框
                    try
                    {
                        List<int> gradeList = new List<int>();

                        IQueryFilter qf = new QueryFilterClass();
                        qf.SubFields = RiverGradeFieldName;
                        IFeatureCursor feCursor = hydlFC.Search(qf, true);

                        IDataStatistics dataStati = new DataStatisticsClass();
                        dataStati.Field = RiverGradeFieldName;
                        dataStati.Cursor = feCursor as ICursor;

                        IEnumerator enumerator = dataStati.UniqueValues;
                        enumerator.Reset();
                        while (enumerator.MoveNext())
                        {
                            int grade = 0;
                            try
                            {
                                grade = int.Parse(enumerator.Current.ToString());
                            }
                            catch
                            {
                            }

                            gradeList.Add(grade);
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);

                        gradeList.Sort();
                        foreach (var item in gradeList)
                        {
                            chkDelGrade2list.Items.Add(item);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.Message);
                        System.Diagnostics.Trace.WriteLine(ex.Source);
                        System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                        MessageBox.Show(ex.Message);
                        return;
                    }

                    //初始化道路等级选择框
                    try
                    {
                        List<int> gradeList = new List<int>();

                        IQueryFilter qf = new QueryFilterClass();
                        qf.SubFields = RoadGradeFieldName;
                        IFeatureCursor feCursor = lrdlFC.Search(qf, true);

                        IDataStatistics dataStati = new DataStatisticsClass();
                        dataStati.Field = RoadGradeFieldName;
                        dataStati.Cursor = feCursor as ICursor;

                        IEnumerator enumerator = dataStati.UniqueValues;
                        enumerator.Reset();
                        while (enumerator.MoveNext())
                        {
                            int grade = 0;
                            try
                            {
                                grade = int.Parse(enumerator.Current.ToString());
                            }
                            catch
                            {
                            }

                            gradeList.Add(grade);
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);

                        gradeList.Sort();
                        foreach (var item in gradeList)
                        {
                            chkDelGradeList.Items.Add(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.Message);
                        System.Diagnostics.Trace.WriteLine(ex.Source);
                        System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                        MessageBox.Show(ex.Message);
                        return;
                    }

                    //初始化需化简的线图层选择框
                    try
                    {
                        List<string> plFCNames = new List<string>();

                        IEnumDataset pEnumDataset = (fWS as IWorkspace).get_Datasets(esriDatasetType.esriDTAny);
                        pEnumDataset.Reset();
                        IDataset pDataset = pEnumDataset.Next();
                        while (pDataset != null)
                        {
                            if (pDataset is IFeatureDataset)//要素数据集
                            {
                                IFeatureDataset pFeatureDataset = fWS.OpenFeatureDataset(pDataset.Name);
                                IEnumDataset pEnumDatasetF = pFeatureDataset.Subsets;
                                pEnumDatasetF.Reset();
                                IDataset pDatasetF = pEnumDatasetF.Next();
                                while (pDatasetF != null)
                                {
                                    if (pDatasetF is IFeatureClass)//要素类
                                    {
                                        IFeatureClass fc = fWS.OpenFeatureClass(pDatasetF.Name);
                                        if (fc != null && fc.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline)
                                            plFCNames.Add(fc.AliasName);
                                    }

                                    pDatasetF = pEnumDatasetF.Next();
                                }

                                System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDatasetF);
                            }
                            else if (pDataset is IFeatureClass)//要素类
                            {
                                IFeatureClass fc = fWS.OpenFeatureClass(pDataset.Name);
                                if (fc != null && fc.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline)
                                    plFCNames.Add(fc.AliasName);
                            }

                            pDataset = pEnumDataset.Next();

                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDataset);

                        foreach (var item in plFCNames)
                        {
                            if (item.ToUpper() == "TERL")
                                continue;

                            clbSimplifiedFCName.Items.Add(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.Message);
                        System.Diagnostics.Trace.WriteLine(ex.Source);
                        System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                        MessageBox.Show(ex.Message);
                        return;
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fWS);
                }
            }
        }

        
        private void btRiverFieldAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkriverField.Items.Count; i++)
            {
                chkriverField.SetItemChecked(i, true);
            }
        }

        private void btRiverFieldNone_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkriverField.Items.Count; i++)
            {
                chkriverField.SetItemChecked(i, false);
            }
        }

        private void btRoadFieldAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkRoadField.Items.Count; i++)
            {
                chkRoadField.SetItemChecked(i, true);
            }
        }

        private void btRoadFieldNone_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkRoadField.Items.Count; i++)
            {
                chkRoadField.SetItemChecked(i, false);
            }
        }

        private void chkDelGrade2list_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selIndex = chkDelGrade2list.SelectedIndex;

            for (var i = 0; i < chkDelGrade2list.Items.Count; i++)
            {
                if (i < selIndex)
                {
                    chkDelGrade2list.SetItemChecked(i, false);
                }
                else
                {
                    chkDelGrade2list.SetItemChecked(i, true);
                }
            }
        }

        private void chkDelGradeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selIndex = chkDelGradeList.SelectedIndex;

            for (var i = 0; i < chkDelGradeList.Items.Count; i++)
            {
                if (i < selIndex)
                {
                    chkDelGradeList.SetItemChecked(i, false);
                }
                else
                {
                    chkDelGradeList.SetItemChecked(i, true);
                }
            }
        }

        private void btSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < clbSimplifiedFCName.Items.Count; i++)
            {
                clbSimplifiedFCName.SetItemChecked(i, true);
            }
        }

        private void btUnSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < clbSimplifiedFCName.Items.Count; i++)
            {
                clbSimplifiedFCName.SetItemChecked(i, false);
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (tbOriginDB.Text == "")
            {
                MessageBox.Show("请指定一个大比例尺数据库！");
                return;
            }

            if (cbObjScale.Text == "")
            {
                MessageBox.Show("请输入缩编后数据库比例尺！");
                return;
            }
            double scale;
            double.TryParse(cbObjScale.Text, out scale);
            if (scale <= 0)
            {
                MessageBox.Show("请输入一个合法的缩编后数据库比例尺！");
                return;
            }

            foreach (var ln in chkDelGrade2list.CheckedItems)
            {
                if (_riverDelSQLText == "")
                {
                    _riverDelSQLText = RiverGradeFieldName + "=" + ln.ToString();
                }
                else
                {
                    _riverDelSQLText += " OR " + RiverGradeFieldName + "=" + ln.ToString();
                }
            }

            foreach (var ln in chkDelGradeList.CheckedItems)
            {
                if (_roadDelSQLText == "")
                {
                    _roadDelSQLText = RoadGradeFieldName + "=" + ln.ToString();
                }
                else
                {
                    _roadDelSQLText += " OR " + RoadGradeFieldName + "=" + ln.ToString();
                }
            }

            if (_riverDelSQLText == "" && _roadDelSQLText == "")
            {
                MessageBox.Show("此次操作还没有选择任何需删除的低等级河流及道路！");
                return;
            }

            RiverFieldArray = new List<string>();
            for (int i = 0; i < chkriverField.CheckedItems.Count; i++)
            {
                var item = chkriverField.CheckedItems[i];
                RiverFieldArray.Add(item.ToString());
            }
            if (_riverDelSQLText != "" && RiverFieldArray.Count() == 0)
            {
                MessageBox.Show("没有选择水系伪节点处理时的标识字段");
                return;
            }

            RoadFieldArray = new List<string>();
            for (int i = 0; i < chkRoadField.CheckedItems.Count; i++)
            {
                var item = chkRoadField.CheckedItems[i];
                RoadFieldArray.Add(item.ToString());
            }
            if (_roadDelSQLText != "" && RoadFieldArray.Count() == 0)
            {
                MessageBox.Show("没有选择道路伪节点处理时的标识字段");
                return;
            }

            double blend;
            double.TryParse(txtBend.Text, out blend);
            if (blend <= 0)
            {
                MessageBox.Show("请输入一个合法的弯曲化简参数！");
                return;
            }

            double smooth;
            double.TryParse(txtSmooth.Text, out smooth);
            if (smooth <= 0)
            {
                MessageBox.Show("请输入一个合法的平滑参数！");
                return;
            }
            
            NeedSimplifiedFCNameList = new List<string>();
            for (int i = 0; i < clbSimplifiedFCName.CheckedItems.Count; i++)
            {
                var item = clbSimplifiedFCName.CheckedItems[i];
                NeedSimplifiedFCNameList.Add(item.ToString());
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        
    }
}
