using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geometry;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessor;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataManagementTools;
using SMGI.Common;

namespace SMGI.Plugin.DCDProcess
{
    public partial class DataBaseClipForm : Form
    {
        /// <summary>
        /// 待裁切的目标数据库
        /// </summary>
        public IFeatureWorkspace SourceWS
        {
            get;
            protected set;
        }

        /// <summary>
        /// 待裁切数据库的类型（gdb、mdb）
        /// </summary>
        public string DBDuffix
        {
            get;
            protected set;
        }

        /// <summary>
        /// 裁切面数据库
        /// </summary>
        public IFeatureClass ClipFC
        {
            get;
            protected set;
        }

        /// <summary>
        /// 裁切字段
        /// </summary>
        public string ClipFN
        {
            get
            {
                return cbSuffixFN.Text;
            }
        }

        /// <summary>
        /// 是否保留空要素类
        /// </summary>
        public bool NeedSaveNullFC
        {
            get
            {
                return cbNullFeatureClass.Checked;
            }
        }

        /// <summary>
        /// 输出路径
        /// </summary>
        public string OutputPath
        {
            get
            {
                return tbOutputPath.Text;
            }
        }
    
        public DataBaseClipForm()
        {
            InitializeComponent();
        }

        private void DataBaseClipForm_Load(object sender, EventArgs e)
        {
            if (CBDataBaseType.Items.Count > 0)
            {
                CBDataBaseType.SelectedIndex = 0;
            }
        }

        private void btnGDB_Click(object sender, EventArgs e)
        {
            if (CBDataBaseType.Text.Contains("文件"))
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "选择GDB工程文件夹";
                fbd.ShowNewFolderButton = false;

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
                    if (!wsFactory.IsWorkspace(fbd.SelectedPath))
                    {
                        MessageBox.Show("不是有效的文件地理数据库!");
                        return;
                    }

                    using (WaitOperation wo = GApplication.Application.SetBusy())
                    {
                        wo.SetText(string.Format("正在加载数据......"));
                        SourceWS = wsFactory.OpenFromFile(fbd.SelectedPath, 0) as IFeatureWorkspace;

                        DBDuffix = "gdb";
                        tbGDBFilePath.Text = fbd.SelectedPath;

                    }
                }

            }
            else if (CBDataBaseType.Text.Contains("个人"))
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.RestoreDirectory = true;
                dlg.Title = "选择个人地理数据库";
                dlg.Filter = "MDB数据库(*.mdb)|*.mdb"; //过滤文件类型
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    IWorkspaceFactory wsFactory = new AccessWorkspaceFactoryClass();
                    if (!wsFactory.IsWorkspace(dlg.FileName))
                    {
                        MessageBox.Show("不是有效的个人地理数据库!");
                        return;
                    }

                    using (WaitOperation wo = GApplication.Application.SetBusy())
                    {
                        wo.SetText(string.Format("正在加载数据......"));
                        SourceWS = wsFactory.OpenFromFile(dlg.FileName, 0) as IFeatureWorkspace;

                        DBDuffix = "mdb";
                        tbGDBFilePath.Text = dlg.FileName;

                    }
                }
            }
            else
            {
                MessageBox.Show("不支持该数据库类型！");
            }
        }

        private void btClip_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "选择一个面要素类";
            dlg.AddExtension = true;
            dlg.DefaultExt = "shp";
            dlg.Filter = "选择文件|*.shp";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                #region 读取面要素类
                IWorkspaceFactory wsFactory = new ShapefileWorkspaceFactory();
                IFeatureWorkspace fws = wsFactory.OpenFromFile(System.IO.Path.GetDirectoryName(dlg.FileName), 0) as IFeatureWorkspace;
                ClipFC = fws.OpenFeatureClass(System.IO.Path.GetFileName(dlg.FileName));
                if (ClipFC.ShapeType != esriGeometryType.esriGeometryPolygon)
                {
                    ClipFC = null;
                    MessageBox.Show("所选择的裁切要素类不是一个有效的面要素类!");
                    return;
                }

                if (ClipFC.FeatureCount(null) == 0)
                {
                    ClipFC = null;
                    MessageBox.Show("所选择的裁切要素类为空!");
                    return;
                }

                tbClipPolygonShpFileName.Text = dlg.FileName;
                #endregion

                #region 更新后缀字段名
                cbSuffixFN.Items.Clear();
                for (int i = 0; i < ClipFC.Fields.FieldCount; ++i)
                {
                    var field = ClipFC.Fields.get_Field(i);
                    if (field.Type != esriFieldType.esriFieldTypeString && field.Type != esriFieldType.esriFieldTypeDouble
                        && field.Type != esriFieldType.esriFieldTypeInteger && field.Type != esriFieldType.esriFieldTypeOID
                        && field.Type != esriFieldType.esriFieldTypeSingle && field.Type != esriFieldType.esriFieldTypeSmallInteger)
                        continue;//非数字、文本类型字段直接跳过

                    cbSuffixFN.Items.Add(field.Name);
                }

                #endregion
            }
        }

        private void btOutputPath_Click(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            if (tbGDBFilePath.Text != "")
                fd.SelectedPath = System.IO.Path.GetDirectoryName(tbGDBFilePath.Text);
            if (fd.ShowDialog() == DialogResult.OK && fd.SelectedPath.Length > 0)
            {
                tbOutputPath.Text = fd.SelectedPath;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            #region 判断必填参数是否已经填写
            if (SourceWS == null)
            {
                MessageBox.Show("请指定目标地理数据库！");
                return;
            }
            if (ClipFC == null)
            {
                MessageBox.Show("请指定分割面要素类！");
                return;
            }
            if (cbSuffixFN.Text == "")
            {
                MessageBox.Show("请指定分割字段！");
                return;
            }
            if (tbOutputPath.Text == "")
            {
                MessageBox.Show("请指定输出路径！");
                return;
            }
            #endregion

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        

    }
}