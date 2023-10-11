using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public class AttributeBrushTool: SMGITool
    {
        IFeature _selFe;
        AttributeBrushToolForm _infoForm;

        ControlsSelectFeaturesToolClass _currentTool;

        /// <summary>
        /// 进行属性的刷制
        /// </summary>
        public AttributeBrushTool() 
        {
            m_caption = "属性刷";
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "Brush.cur"));
            m_category = "编辑工具";
            m_toolTip = "将选择的要素属性（协同字段和FEAID除外）复制到另一个要素上（开启编辑且只选中一个要素时激活）";

            NeedSnap = false;

            _selFe = null;
            _currentTool = new ControlsSelectFeaturesToolClass();
        }

        public override void setApplication(GApplication app)
        {
            base.setApplication(app);

            _currentTool.OnCreate(m_Application.MapControl.Object);

        }

        public override bool Enabled
        {
            get
            {
                if (m_Application == null || m_Application.Workspace == null || m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing)
                    return false;

                if (_infoForm != null && _infoForm.SelFeStruct != null)
                    return true;

                if (m_Application.MapControl.Map.SelectionCount != 1)
                    return false;

                return true;

            }
        }

        public override void OnClick()
        {
            _currentTool.OnClick();

            _infoForm = null;
            _selFe = null;

            IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
            mapEnumFeature.Reset();
            _selFe = mapEnumFeature.Next();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);

            if (null == _infoForm || _infoForm.IsDisposed)
            {
                _infoForm = new AttributeBrushToolForm(_selFe);
                _infoForm.frmClosed += new AttributeBrushToolForm.FrmClosedEventHandler(clear);
            }
            //打开提示框
            if (cmdUpdateRecord.EnableAttributeBrushToolForm)
            {
                _infoForm.Show();
                //showInfoForm();
            }
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            _currentTool.OnMouseDown(button, shift, x, y);
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            _currentTool.OnMouseMove(button, shift, x, y);
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            _currentTool.OnMouseUp(button, shift, x, y);

            //打开信息框
            //showInfoForm();//弹窗仅在工具启用时打开，避免重复

            if (_infoForm == null || _infoForm.SelFeStruct == null)
                return;

            string fcName = _infoForm.SelFeStruct.FCName;
            IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
            mapEnumFeature.Reset();

            List<IFeature> objFeList = new List<IFeature>();
            IFeature objFe = null;
            while ((objFe = mapEnumFeature.Next()) != null)
            {
                objFeList.Add(objFe);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);

            if (objFeList.Count() == 0)
                return;

            int n = 0; //记录修改个数
            m_Application.EngineEditor.StartOperation(); 
            try
            {
                foreach (var fe in objFeList)
                {
                    bool bModify = false;
                    for (int i = 0; i < fe.Fields.FieldCount; i++)
                    {
                        IField field = fe.Fields.get_Field(i);

                        if (!field.Editable)
                            continue;

                        if (!_infoForm.SelFeStruct.FieldInfo.ContainsKey(field.Name) || _infoForm.SelFeStruct.FieldInfo[field.Name] != field.Type)
                            continue;


                        if (!_infoForm.SelFNList.Contains(field.Name.ToUpper()))
                            continue;//未选择

                        fe.set_Value(i, _infoForm.SelFeStruct.FieldValue[field.Name]);
                        bModify = true;

                    }
                    if (bModify)
                    {
                        fe.Store();
                        n++;
                    }
                }
                //m_Application.ActiveView.FocusMap.ClearSelection();
                if (n > 0)
                {
                    m_Application.EngineEditor.StopOperation("属性刷属性");
                    m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
                }
                else
                {
                    m_Application.EngineEditor.AbortOperation();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                m_Application.EngineEditor.AbortOperation();
            }
        }

        public override void Refresh(int hdc)
        {
            _currentTool.Refresh(hdc);
        }

        public override bool Deactivate()
        {
            _selFe = null;

            //关闭提示框
            if (_infoForm != null)
            {
                _infoForm.Close();
                _infoForm = null;
            }

            return _currentTool.Deactivate();
        }

        /// <summary>
        /// 窗体关闭时的响应函数
        /// </summary>
        private void clear()
        {

        }

        /// <summary>
        /// 显示信息框
        /// </summary>
        private void showInfoForm()
        {
            if (null == _infoForm || _infoForm.IsDisposed)
            {
                _infoForm.Show();
            }
            else
            {
                _infoForm.Activate();
            }
        }
        
    }

    //记录窗口的位置，通过数据绑定
    public class ABTool2 : INotifyPropertyChanged
    {
        private System.Drawing.Point pt = new System.Drawing.Point(100, 100);

        public System.Drawing.Point FrmLocation
        {
            get { return pt; }
            set
            {
                pt = value;
                NotifyPropertyChanged(() => pt);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged<T>(Expression<Func<T>> property)
        {
            if (property == null)
                return;
            var memberExpression = property.Body as MemberExpression;
            if (memberExpression == null)
                return;
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
        }

    }
    public static class DataBindings
    {
        public static ABTool2 abtool2 = new ABTool2();
    }

}
