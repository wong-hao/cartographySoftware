using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.DCDProcess
{
    public class Lineextendedit : SMGITool
    {   
        private IEngineEditor editor;
        private ISimpleLineSymbol lineSymbol;
        private INewLineFeedback lineFeedback;
        private IFeature selFeature;
        private ToolbarMenu toolbarMenu = new ToolbarMenu();
        private bool useToPoint;

        public Lineextendedit()
        {
            m_caption = "线延续";
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "修线.cur"));
            useToPoint = true;
            toolbarMenu.AddItem(new SMGI.Plugin.DCDProcess.ChangeHead());
            toolbarMenu.AddItem(new SMGI.Plugin.DCDProcess.CancelOperate());            
        }
       
        public override void OnClick()
        {            
            editor = m_Application.EngineEditor;
            lineSymbol = GetLineSymbo();
            selFeature = GetSelectFeature();            

            var dis = m_Application.ActiveView.ScreenDisplay;           
            lineFeedback = new NewLineFeedbackClass { Display = dis, Symbol = lineSymbol as ISymbol };
            IPoint ptEnd = GetSelectFeatureToPoint(selFeature, useToPoint);
            lineFeedback.Start(ptEnd);      
      
            //用于解决在绘制feedback过程中进行地图平移出现线条混乱的问题
            m_Application.MapControl.OnAfterScreenDraw += new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);
        }

        public ISimpleLineSymbol GetLineSymbo()
        {
            //#region Create a symbol to use for feedback
            ISimpleLineSymbol lineSymbolx = new SimpleLineSymbolClass();
            IRgbColor color = new RgbColorClass();	 //red
            color.Red = 255;
            color.Green = 0;
            color.Blue = 0;
            lineSymbolx.Color = color;
            lineSymbolx.Style = esriSimpleLineStyle.esriSLSSolid;
            lineSymbolx.Width = 1.5;
            (lineSymbolx as ISymbol).ROP2 = esriRasterOpCode.esriROPNotXOrPen;//这个属性很重要
            //#endregion
            return lineSymbolx; 
        }

        private void MapControl_OnAfterScreenDraw(object sender, IMapControlEvents2_OnAfterScreenDrawEvent e)
        {
            if (lineFeedback != null)
            {
                lineFeedback.Refresh(m_Application.ActiveView.ScreenDisplay.hDC);
            }
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (selFeature == null)
                return;

            if (button == 1)//左键添加折点
            {
                if (lineFeedback == null)
                {
                    var dis = m_Application.ActiveView.ScreenDisplay;
                    lineFeedback = new NewLineFeedbackClass { Display = dis, Symbol = lineSymbol as ISymbol };
                    lineFeedback.Start(ToSnapedMapPoint(x, y));
                }
                else
                {
                    lineFeedback.AddPoint(ToSnapedMapPoint(x, y));
                }               
            }
            else if (button == 2)//右键弹出菜单
            {
                toolbarMenu.CommandPool = m_Application.MainForm.CommandPool;
                IEngineEditSketch engineEditSketch = (IEngineEditSketch)m_Application.EngineEditor;
                engineEditSketch.SetEditLocation(x, y);
                toolbarMenu.PopupMenu(x, y, m_Application.MapControl.hWnd);
                if (LineExtentCommon.cancel)
                {
                    lineFeedback = null;
                    m_Application.MapControl.Map.ClearSelection();
                    m_Application.MapControl.Refresh();
                    LineExtentCommon.cancel = false;                    
                }
                if (LineExtentCommon.switchToPoint)
                {
                    lineFeedback = null;
                    useToPoint = !useToPoint;
                    IPoint ptEnd = GetSelectFeatureToPoint(selFeature, useToPoint);
                    var dis = m_Application.ActiveView.ScreenDisplay;
                    lineFeedback = new NewLineFeedbackClass { Display = dis, Symbol = lineSymbol as ISymbol };
                    lineFeedback.Start(ptEnd);
                    m_Application.MapControl.Refresh();
                    LineExtentCommon.switchToPoint = false;
                }
            }            
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (lineFeedback != null)
            {
                lineFeedback.MoveTo(ToSnapedMapPoint(x, y));
            }
        }

        public override void OnDblClick()
        {
            IPolyline polyline = lineFeedback.Stop();            
            lineFeedback = null;
            
            if (polyline.IsEmpty)
                return;

            if (selFeature == null)
                return;

            //删除状态的要素不参与
            int versionIndex = selFeature.Fields.FindField(cmdUpdateRecord.CollabVERSION);
            int delIndex = selFeature.Fields.FindField(cmdUpdateRecord.CollabDELSTATE);
            if (versionIndex != -1 && delIndex != -1)
            {
                int ver = 0;
                int.TryParse(selFeature.get_Value(versionIndex).ToString(), out ver);
                string delState = selFeature.get_Value(delIndex).ToString();
                if (ver < 0 && delState == cmdUpdateRecord.DelStateText)
                {
                    editor.AbortOperation();
                    return;
                }
            }

            #region 编辑操作开始
            editor.StartOperation();
            try
            {
                ITopologicalOperator2 pTopo = (ITopologicalOperator2)polyline;
                pTopo.IsKnownSimple_2 = false;
                pTopo.Simplify();

                IGeometry geo = selFeature.ShapeCopy;
                ITopologicalOperator feaTopo = geo as ITopologicalOperator;
                feaTopo.Simplify(); //解决HRESULT:0x80040218
                geo = feaTopo.Union(polyline);                
                selFeature.Shape = geo;
                selFeature.Store();
                editor.StopOperation("线延续");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                MessageBox.Show(ex.Message);
                editor.AbortOperation();
            }            
            #endregion

            
            m_Application.MapControl.Map.ClearSelection();
            m_Application.MapControl.Refresh();
        }

        public override bool Deactivate()
        {
            //卸掉该事件
            m_Application.MapControl.OnAfterScreenDraw -= new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);
            return base.Deactivate();
        }

        public override bool Enabled
        {
            get
            {
                if (m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    if (m_Application.MapControl.Map.SelectionCount != 1)
                        return false;
                    IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                    mapEnumFeature.Reset();
                    IFeature feature = mapEnumFeature.Next();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);
                    if (feature != null && feature.Shape is IPolyline)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                        return true;
                    }
                }
                return false;
            }            
        }

        public IFeature GetSelectFeature()
        {
            IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
            mapEnumFeature.Reset();
            IFeature feature = mapEnumFeature.Next();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);
            if (feature != null)
            {
                if (feature.Shape is IPolyline)
                    return feature;
            }
            return null; 
        }

        public IPoint GetSelectFeatureToPoint(IFeature feature, bool useToPoint)
        {
            if (feature.Shape is IPolyline)
            {
                IPolyline pl = feature.ShapeCopy as IPolyline;
                if (useToPoint)
                    return pl.ToPoint;
                else
                    return pl.FromPoint;
            }

            return null;
        }
    }

    public class ChangeHead : SMGICommand
    {
        public ChangeHead()
        {
            m_caption = "线换头";
            m_toolTip = "线换头按钮";
            m_category = "基础编辑";            
        }
        public override void OnClick()
        {
            LineExtentCommon.switchToPoint = true;
        }
        public override bool Enabled
        {
            get
            {
                return true;
            }
        }
    }

    public class CancelOperate : SMGICommand
    {
        public CancelOperate()
        {
            m_caption = "取消";
            m_toolTip = "取消操作";
            m_category = "基础编辑";            
        }
        public override void OnClick()
        {
            LineExtentCommon.cancel = true; 
        }
        public override bool Enabled
        {
            get {
                return true;
            }
        } 
    }

    public static class LineExtentCommon
    {
        public static bool switchToPoint = false;
        public static bool cancel = false;
    }
}
