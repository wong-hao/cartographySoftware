using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.DCDProcess
{
    public partial class CheckNewCollabGUIDForm : Form
    {
        #region 属性
        public List<IFeatureClass> CheckFeatureClassList
        {
            get
            {
                return _checkFeatureClassList;
            }
        }
        private List<IFeatureClass> _checkFeatureClassList;

        public string ReferGDB
        {
            get
            {
                return tbReferGDB.Text;
            }
        }
        #endregion

        private GApplication _app;
        public CheckNewCollabGUIDForm(GApplication app)
        {
            InitializeComponent();
            _app = app;
        }

        private void CheckNewCollabGUIDForm_Load(object sender, EventArgs e)
        {
            //检索当前工作空间的所有图层对应要素类名称
            List<string> fcNameList = new List<string>();
            chkFCList.ValueMember = "Key";
            chkFCList.DisplayMember = "Value";
            var layers = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer));
            foreach (var lyr in layers)
            {
                IFeatureLayer feLayer = lyr as IFeatureLayer;
                IFeatureClass fc = feLayer.FeatureClass;
                if (fc == null)
                    continue;//空图层

                if (fc.FindField(cmdUpdateRecord.CollabGUID) == -1)
                    continue;//不含协同GUID字段

                if ((fc as IDataset).Workspace.PathName != _app.Workspace.EsriWorkspace.PathName)
                    continue;//临时数据

                if (!fcNameList.Contains(fc.AliasName))
                {
                    chkFCList.Items.Add(new KeyValuePair<IFeatureClass, string>(fc, fc.AliasName), true);
                    fcNameList.Add(fc.AliasName);
                }
            }
        }

        private void btnSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkFCList.Items.Count; i++)
            {
                chkFCList.SetItemChecked(i, true);
            }
        }

        private void btnUnSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkFCList.Items.Count; i++)
            {
                chkFCList.SetItemChecked(i, false);
            }
            chkFCList.ClearSelected();
        }

        private void btReferGDB_Click(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            fd.Description = "选择GDB数据库";
            fd.ShowNewFolderButton = false;
            if (fd.ShowDialog() != DialogResult.OK || !fd.SelectedPath.ToLower().Trim().EndsWith(".gdb"))
                return;

            tbReferGDB.Text = fd.SelectedPath;
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (chkFCList.CheckedItems.Count == 0)
            {
                MessageBox.Show("请选择至少一个要素类进行检查！");
                return;
            }

            if (tbReferGDB.Text == "")
            {
                MessageBox.Show("请指定参考数据库！");
                return;
            }

            _checkFeatureClassList = new List<IFeatureClass>();
            foreach (var item in chkFCList.CheckedItems)
            {
                var kv = (KeyValuePair<IFeatureClass, string>)item;
                _checkFeatureClassList.Add(kv.Key);
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        
    }
}
