using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.DCDProcess
{
    [Guid("A566C3CC-5B29-437B-A40B-C2C0D6F1D92E")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("SMGI.Plugin.CollaborativeWork.CmdUpdateDataFilter")]
    public sealed class CmdUpdateDataFilter : BaseCommand, IToolControl
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Register(regKey);
            ControlsCommands.Register(regKey);
        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Unregister(regKey);
            ControlsCommands.Unregister(regKey);
        }
        #endregion
        #endregion

        private Panel groupPan;
        private ComboBox operateCB;//要素操作状态组合框
        SMGI.Common.GApplication _app;
        public CmdUpdateDataFilter(GApplication app)
        {
            _app = app;

            _app.WorkspaceOpened += new EventHandler(workspaceOpened);

            m_caption = "当前显示状态";
            m_message = "选择当前显示状态";
            m_toolTip = "选择当前显示状态";
            m_category = "显示状态";

            groupPan = new Panel();
            groupPan.Size = new System.Drawing.Size(200, 25);

            //根据要素的操作状态控制显示与否
            operateCB = new ComboBox();
            operateCB.DropDownStyle = ComboBoxStyle.DropDownList;
            operateCB.FlatStyle = FlatStyle.Popup;
            operateCB.Size = new System.Drawing.Size(96, 24);
            operateCB.Items.Add("显示所有");
            operateCB.Items.Add("显示当前");
            operateCB.Items.Add("显示已改");
            operateCB.Items.Add("显示增加");
            operateCB.Items.Add("显示修改");
            operateCB.Items.Add("显示删除");
            operateCB.SelectedIndex = 0;
            operateCB.Parent = groupPan;
            operateCB.Dock = DockStyle.Left;
            operateCB.SelectedIndexChanged += new EventHandler(cb_SelectedIndexChanged);

        }

        #region Overridden Class Methods
        public override void OnCreate(object hook)
        {
            // TODO:  Add other initialization code
        }

        public override void OnClick()
        {
            // TODO:  Add other initialization code
        }

        public override bool Enabled
        {
            get
            {
                return _app != null && _app.Workspace != null;
            }
        }
        #endregion

        #region IToolControl 成员
        //控件句柄
        public int hWnd
        {
            get
            {
                return groupPan.Handle.ToInt32();
            }
        }
        //控件是否可拖动
        public bool OnDrop(esriCmdBarType barType)
        {
            if (esriCmdBarType.esriCmdBarTypeToolbar == barType)
                return true;
            else
                return false;
        }
        //通知控件是否聚焦
        public void OnFocus(ICompletionNotify complete)
        {
            //
        }
        #endregion

        //
        private void cb_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateFeatureLayerDefinition();
        }

        public void updateFeatureLayerDefinition()
        {
            var view = _app.ActiveView;
            var map = view.FocusMap;

            var ls = map.get_Layers();
            var layer = ls.Next();


            for (; layer != null; layer = ls.Next())
            {
                if (!(layer is ESRI.ArcGIS.Carto.IFeatureLayer))
                    continue;

                if ((layer as IFeatureLayer).FeatureClass == null)
                    continue;

                if (((layer as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName != _app.Workspace.EsriWorkspace.PathName)
                {
                    //临时数据
                    continue;
                }                  

                string queryExpression = string.Empty;

                switch (operateCB.SelectedIndex)
                {
                    case 0://显示所有
                        queryExpression = string.Empty;
                        break;
                    case 1://显示当前
                        queryExpression = cmdUpdateRecord.CurFeatureFilter;
                        break;
                    case 2://显示已改
                        queryExpression = string.Format("{0} < 0", cmdUpdateRecord.CollabVERSION);
                        break;
                    case 3://显示增加
                        queryExpression = string.Format("{0} = {1} and ({2} is null or {2} <> '{3}')", cmdUpdateRecord.CollabVERSION, cmdUpdateRecord.NewState,cmdUpdateRecord.CollabDELSTATE, cmdUpdateRecord.DelStateText);//排除删除要素
                        break;
                    case 4://显示修改
                        queryExpression = string.Format("{0} = {1} and ({2} is null or {2} <> '{3}')", cmdUpdateRecord.CollabVERSION, cmdUpdateRecord.EditState,cmdUpdateRecord.CollabDELSTATE, cmdUpdateRecord.DelStateText);//排除删除要素
                        break;
                    case 5://显示删除
                        queryExpression = string.Format("{0} < 0 and {1} = '{2}'", cmdUpdateRecord.CollabVERSION, cmdUpdateRecord.CollabDELSTATE, cmdUpdateRecord.DelStateText);
                        break;
                    default:
                        break;
                }

                var fd = layer as ESRI.ArcGIS.Carto.IFeatureLayerDefinition;
                fd.DefinitionExpression = queryExpression;
            }

            groupPan.Focus();

            view.Refresh();
        }


        public void workspaceOpened(object sender, EventArgs e)
        {
            operateCB.SelectedIndex = 1;
        }
    }
}
