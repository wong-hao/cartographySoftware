using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.DCDProcess
{
    public partial class SnapEnvConfigForm : Form
    {
        public int SnapTolerance { get; set; }

        private GApplication _app;

        private ISnappingEnvironment _snapEnv;

        public SnapEnvConfigForm(GApplication app)
        {
            InitializeComponent();

            _app = app;
        }

        private void FrmSnapEnvConfig_Load(object sender, EventArgs e)
        {
            IHookHelper hookHelper = new HookHelperClass();
            hookHelper.Hook = _app.MapControl.Object;

            IExtensionManager extensionManager = (hookHelper as IHookHelper2).ExtensionManager;
            if (extensionManager != null)
            {
                UID guid = new UIDClass();
                guid.Value = "{E07B4C52-C894-4558-B8D4-D4050018D1DA}"; //Snapping extension.
                IExtension extension = extensionManager.FindExtension(guid);
                _snapEnv = extension as ISnappingEnvironment;
                
                //更新控件
                tbTolerance.Text = _snapEnv.Tolerance.ToString();
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            bool b = true;
            try
            {
                SnapTolerance = int.Parse(tbTolerance.Text);
            }
            catch(Exception ex)
            {
                b = false;
            }

            if (!b || SnapTolerance <= 0)
            {
                MessageBox.Show("请指定一个大于0的容差值");
                return;
            }
            
            //更新容差
            _snapEnv.Tolerance = SnapTolerance;

            DialogResult = DialogResult.OK;
        }

        
    }
}
