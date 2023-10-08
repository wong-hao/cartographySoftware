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

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    public class LineBreakTool : SMGITool
    {
        private IFeature _breakFeature;

        public LineBreakTool()
        {
            m_caption = "线打断";
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "修线.cur"));
            NeedSnap = false;
            _breakFeature = null;
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing;
            }
        }

        private ILayer layer;
        public override void OnClick()
        {
            var map = m_Application.ActiveView.FocusMap;
            var selection = map.FeatureSelection;
            if (map.SelectionCount != 1)
            {
                MessageBox.Show("请选中单条可编辑的线要素!");
                return;
            }


            IEnumFeature selectEnumFeature = (selection as MapSelection) as IEnumFeature;
            selectEnumFeature.Reset();
            IFeature fe = selectEnumFeature.Next();

            if (fe.Shape.GeometryType != esriGeometryType.esriGeometryPolyline)
            {
                MessageBox.Show("请选中一条线要素");
                return;
            }

            string fn = fe.Class.AliasName.ToUpper();
            layer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass == fe.Class)).FirstOrDefault();
            if (!(m_Application.EngineEditor as IEngineEditLayers).IsEditable(layer as IFeatureLayer))
            {
                MessageBox.Show("请选中一条可编辑的线要素");
                return;
            }

            NeedSnap = true;
            _breakFeature = fe;
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
                return;

            if (_breakFeature == null)
                return;

            //获取当前点
            IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            ISnappingResult snapResult = m_snapper.Snap(currentMouseCoords);
            if (snapResult == null)
            {
                MessageBox.Show("请将打断点置于捕捉容差范围内！");
                return;
            }
            currentMouseCoords = snapResult.Location;

            m_Application.EngineEditor.StartOperation();
            try
            {
                string StaCod = _breakFeature.get_Value(_breakFeature.Fields.FindField(ServerDataInitializeCommand.CollabSTACOD)).ToString();

                bool bCoola = true;
                #region 协同
                if (_breakFeature.Fields.FindField(ServerDataInitializeCommand.CollabGUID) == -1)
                {
                    bCoola = false;
                }

                string smgiGUID = "";
                if (bCoola)
                {
                    smgiGUID = _breakFeature.get_Value(_breakFeature.Fields.FindField(ServerDataInitializeCommand.CollabGUID)).ToString();
                }

                int smgiver = 0;
                if (bCoola)
                {
                    string STACOD = _breakFeature.get_Value(_breakFeature.Fields.FindField(ServerDataInitializeCommand.CollabSTACOD)).ToString();
                    MessageBox.Show("STACOD: " + STACOD.ToString());
                    if (STACOD == "删除")
                    {
                        MessageBox.Show("该要素为删除要素，不支持该操作！");
                        return;
                    }
                }

                if (bCoola)
                {
                    _breakFeature.set_Value(_breakFeature.Fields.FindField(ServerDataInitializeCommand.CollabVERSION), cmdUpdateRecord.NewState);//201605，直接删除的标志
                }
                #endregion

                ISet pFeatureSet = (_breakFeature as IFeatureEdit).Split(currentMouseCoords);
                if (pFeatureSet != null)//201605
                {
                    pFeatureSet.Reset();

                    List<IFeature> flist = new List<IFeature>();
                    int maxIndex = -1;
                    double maxLen = 0;
                    while (true)
                    {
                        IFeature fe = pFeatureSet.Next() as IFeature;
                        if (fe == null)
                        {
                            break;
                        }

                        if ((fe.Shape as IPolyline).Length > maxLen)
                        {
                            maxLen = (fe.Shape as IPolyline).Length;
                            maxIndex = flist.Count();
                        }
                        flist.Add(fe);

                    }

                    #region 协同，长的一段保持原GUID
                    if (cmdUpdateRecord.EnableUpdate)
                    {
                        for (int k = 0; k < flist.Count(); ++k)
                        {
                            if (maxIndex == k)
                            {
                                m_Application.ActiveView.FocusMap.SelectFeature(layer, flist[k]);
                                if (StaCod == "原始" || StaCod == "修改")
                                {
                                    flist[k].set_Value(flist[k].Fields.FindField(ServerDataInitializeCommand.CollabSTACOD), "修改");//201605
                                }

                                if (smgiver >= 0 || smgiver == cmdUpdateRecord.EditState)//服务器中下载下来的要素，在打断等操作时，最大部分的协同版本号应该修改（这样才能更新服务器）2017.06.27
                                {
                                    if (bCoola)
                                    {
                                        flist[k].set_Value(flist[k].Fields.FindField(ServerDataInitializeCommand.CollabVERSION), cmdUpdateRecord.EditState);
                                    }
                                }

                                if (bCoola)
                                {
                                    flist[k].set_Value(flist[k].Fields.FindField(ServerDataInitializeCommand.CollabGUID), smgiGUID);//默认由最大的新要素继承原要素的smgiGUID
                                }

                                flist[k].Store();
                                
                                break;
                            }
                        }
                    }
                    #endregion
                }

                m_Application.EngineEditor.StopOperation("线打断");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                m_Application.EngineEditor.AbortOperation();
            }

            NeedSnap = false;
            _breakFeature = null;
            m_Application.MapControl.Refresh();

        }

    }
}