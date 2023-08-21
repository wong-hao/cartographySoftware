using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SMGI.Common;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Editor;
using SMGI.DxForm;
using System.Windows.Forms;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 协同作业操作记录
    /// </summary>
    public class cmdUpdateRecord : SMGI.Common.SMGICommand
    {
        public static bool EnableUpdate { get; set; }//未开启更新时，与在arcmap中编辑保持一致（制图中心要求）
        public static string UserName { get; set; }//操作者（OPUSER）信息：计算机名
        public static string Vers { get; set; }//年份信息（曾用于1：5万地形图更新项目，福建协同更新项目中已无意义）
        public static bool EnableChangeFeatureForm { get; set; }//修改要素时，是否进行弹窗

        private IEngineEditor editor;

        /// <summary>
        /// 排除删除要素的SQL语句
        /// </summary>
        public static string CurFeatureFilter
        {
            get
            {
                return string.Format("({0} is null or {0} >= 0 or {1} is null or {1} <> '{2}')", cmdUpdateRecord.CollabVERSION, cmdUpdateRecord.CollabDELSTATE, cmdUpdateRecord.DelStateText);//增加{1} <> '{2}'条件主要是为了兼容数据中协同删除状态的为空字符串的情况
            }
        }

        public static string CollabVERSION
        {
            get;
            private set;
        }

        public static string CollabDELSTATE//删除状态['是']
        {
            get;
            private set;
        }

        public const string DelStateText = "是";//要素的删除状态文本{同时版本号为负数}


        ITable recordTable;
        int guidIndex = -1;
        string layerName;
        private bool hasExedCreateOper = false;
        private string create_fid = "";
        private List<string> delete_fid = new List<string>();

        //5万更新相关成员
        CmdUpdateDataFilter cmdDF = null;
        CmdUpdateConfig cmdConfig = null;
        //cmdSnapEnvConfig cmdSnapConfig = null;
        //数据更新状态（SMGIVERSION）
        public const int NewState = -1;//本地新增要素状态值
        public const int EditState = -2;//从服务器下载数据的编辑状态
        public const int DeleteState = -2;//从服务器下载数据的删除状态


        public cmdUpdateRecord()
        {
            m_caption = "UpdateRecord";

            Vers = DateTime.Now.Year.ToString();//年份信息
            UserName = System.Environment.MachineName;//操作者（OPUSER）信息：计算机名
            EnableChangeFeatureForm = true;
        }


        #region Overridden Class Methods
        public override void setApplication(GApplication app)
        {
            if (app.Template.Caption != "含权限控制的地理信息协同作业")
            {
                //仅处理含权限控制的地理信息协同作业，其他协同在SMGI.Plugin.DataUpdate中处理
                return;
            }
            base.setApplication(app);

            editor = app.EngineEditor;

            CollabVERSION = m_Application.TemplateManager.getFieldAliasName("SMGIVERSION");

            (editor as IEngineEditEvents_Event).OnCreateFeature += new IEngineEditEvents_OnCreateFeatureEventHandler(UApplication_OnCreateFeature);
            (editor as IEngineEditEvents_Event).OnChangeFeature += new IEngineEditEvents_OnChangeFeatureEventHandler(UApplication_OnChangeFeature);
            (editor as IEngineEditEvents_Event).OnDeleteFeature += new IEngineEditEvents_OnDeleteFeatureEventHandler(UApplication_OnDeleteFeature);
            (editor as IEngineEditEvents_Event).OnSelectionChanged += new IEngineEditEvents_OnSelectionChangedEventHandler(UApplication_OnSelectionChanged);
            (editor as IEngineEditEvents_Event).OnStartEditing += new IEngineEditEvents_OnStartEditingEventHandler(UApplication_OnStartEditing);

            ////5万更新       
            AxToolbarControl toolBar = (app.MainForm as ISMGIMainForm).MapHBar;
            if (toolBar != null)
            {
                cmdConfig = new CmdUpdateConfig(app);
                cmdDF = new CmdUpdateDataFilter(app);
                toolBar.AddItem(cmdConfig, 0, -1, true, 0, esriCommandStyles.esriCommandStyleIconOnly);//向toolbarcontrol中增加自定义工具选项
                toolBar.AddItem(cmdDF);//向toolbarcontrol中增加自定义工具选项
            }
        }

        public override bool Enabled
        {
            get
            {
                return true;
            }
        }

        public override void OnClick()
        {
        }

        #endregion

        #region
        public void UApplication_OnDeleteFeature(ESRI.ArcGIS.Geodatabase.IObject obj)
        {
            var fe = obj as ESRI.ArcGIS.Geodatabase.IFeature;
            if (fe == null)
            {
                return;
            }

            #region 实时记录要素编辑状态
            var fc = obj.Class as IFeatureClass;
            if (obj.Class.AliasName != layerName)
            {
                recordTable = null;
            }

            ITable tb = GetTableByFeature(fe);
            if (tb != null)
            {
                if (guidIndex != -1)
                {
                    IRow newRow = tb.CreateRow();
                    newRow.set_Value(tb.Fields.FindField("OPTYPE"), "删除");
                    newRow.set_Value(tb.Fields.FindField("KEYGID"), fe.get_Value(guidIndex));
                    newRow.set_Value(tb.Fields.FindField("OPUSER"), UserName);
                    newRow.set_Value(tb.Fields.FindField("OPTIME"), DateTime.Now.ToString());
                    newRow.set_Value(tb.Fields.FindField("VERSID"), m_Application.Workspace.LocalBaseVersion);
                    newRow.Store();

                    delete_fid.Add(fe.get_Value(guidIndex).ToString());
                }
            }
            #endregion

            if (!EnableUpdate)//未开启更新，不修改状态（制图中心要求）
            {
                return;
            }

            //协调过程中可以删除
            if (m_Application.Workspace.IsCollaborativing)
            {
                return;
            }

            int smgiver;
            int.TryParse(fe.get_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabVERSION)).ToString(), out smgiver);
            if (smgiver == NewState)//本地新增要素，不记录删除状态，直接删除
            {
                return;
            }

            if (smgiver > m_Application.Workspace.LocalBaseVersion)//协调数据
            {
                return;
            }

            //5更
            fe.set_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabSTACOD), "删除");
            //fe.set_Value(fe.Fields.FindField("VERS"), Vers);//年份信息不赋值，date字段会赋上日期，下同

            fe.set_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabDELSTATE), ServerDataInitializeCommand.DelStateText);//ServerDataInitializeCommand.DelStateText == "是"
            //协同作业
            fe.set_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabOPUSER), UserName);//记录操作者
            fe.set_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabVERSION), DeleteState);
            //corrector有长度限制，需截取
            int fieldLenth = fe.Fields.get_Field(fe.Fields.FindField("corrector")).Length;
            if (GlobalClass.strname.Length > fieldLenth)
            {
                fe.set_Value(fe.Fields.FindField("corrector"), GlobalClass.strname.Substring(0, fieldLenth));
            }
            else
            {
                fe.set_Value(fe.Fields.FindField("corrector"), GlobalClass.strname);
            }
            fe.set_Value(fe.Fields.FindField("date"), DateTime.Now.ToString("yyyyMMdd"));
            

            //增加要素副本，记录删除状态
            var cursor = fc.Insert(true);
            cursor.InsertFeature(fe as ESRI.ArcGIS.Geodatabase.IFeatureBuffer);
            cursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
        }

        public void UApplication_OnChangeFeature(ESRI.ArcGIS.Geodatabase.IObject obj)
        {
            var fe = obj as ESRI.ArcGIS.Geodatabase.IFeature;
            if (fe == null)
            {
                return;
            }

            #region  实时记录编辑状态
            var fc = fe.Class as IFeatureClass;
            if (obj.Class.AliasName != layerName)
            {
                recordTable = null;
            }

            ITable tb = GetTableByFeature(fe);
            if (tb != null)
            {
                if (guidIndex != -1)
                {

                    IRow newRow = tb.CreateRow();
                    newRow.set_Value(tb.Fields.FindField("KEYGID"), fe.get_Value(guidIndex));


                    //判断是否为融合操作
                    if (delete_fid.Count > 0)
                    {
                        string fids = delete_fid[0];
                        for (int i = 0; i < delete_fid.Count; i++)
                        {
                            if (i >= 1)
                            {
                                fids += ",";
                                fids += delete_fid[i];
                            }
                        }
                        if (fids.Length > 50)//暂时
                            fids = "";
                        newRow.set_Value(tb.Fields.FindField("OPTYPE"), "合并");
                        newRow.set_Value(tb.Fields.FindField("RELGID"), fids);
                    }
                    else if (hasExedCreateOper)          //判断是否为打断操作
                    {
                        newRow.set_Value(tb.Fields.FindField("OPTYPE"), "打断");
                        newRow.set_Value(tb.Fields.FindField("RELGID"), create_fid);
                    }
                    else
                    {
                        newRow.set_Value(tb.Fields.FindField("OPTYPE"), "修改");
                    }

                    newRow.set_Value(tb.Fields.FindField("OPUSER"), UserName);
                    newRow.set_Value(tb.Fields.FindField("OPTIME"), DateTime.Now.ToString());
                    newRow.set_Value(tb.Fields.FindField("VERSID"), m_Application.Workspace.LocalBaseVersion);
                    newRow.Store();
                }
            }
            #endregion

            if (!EnableUpdate)//未开启更新，不修改状态（制图中心要求）
            {
                return;
            }

            //如果正处于协调状态则不作修改
            if (m_Application.Workspace.IsCollaborativing)
            {
                return;
            }

            int smgiver;
            int.TryParse(fe.get_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabVERSION)).ToString(), out smgiver);
            if (smgiver > m_Application.Workspace.LocalBaseVersion)//协调数据
            {
                return;
            }

            //5更
            string v = fe.get_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabSTACOD)).ToString();
            if (v == "原始")
            {
                fe.set_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabSTACOD), "修改");
                //  fe.set_Value(fe.Fields.FindField(CollabVERSION), EditState);
            }
           // fe.set_Value(fe.Fields.FindField("VERS"), Vers);

            //协同作业
            if (smgiver >= 0)
            {
                fe.set_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabVERSION), EditState);
            }
            fe.set_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabOPUSER), UserName);//记录操作者
            //corrector有长度限制，需截取
            int fieldLenth = fe.Fields.get_Field(fe.Fields.FindField("corrector")).Length;
            if (GlobalClass.strname.Length > fieldLenth)
            {
                fe.set_Value(fe.Fields.FindField("corrector"), GlobalClass.strname.Substring(0, fieldLenth));
            }
            else
            {
                fe.set_Value(fe.Fields.FindField("corrector"), GlobalClass.strname);
            }
            fe.set_Value(fe.Fields.FindField("date"), DateTime.Now.ToString("yyyyMMdd"));

            if (EnableChangeFeatureForm)//是否弹弹窗
            {
                OnChangeFeatureForm frm = new OnChangeFeatureForm(m_Application);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    string source = frm.source;
                    string remark = frm.remark;
                    fe.set_Value(fe.Fields.FindField("source"), source);
                    fe.set_Value(fe.Fields.FindField("remark"), remark);
                }
            }

        }

        public void UApplication_OnCreateFeature(ESRI.ArcGIS.Geodatabase.IObject obj)
        {
            var fe = obj as ESRI.ArcGIS.Geodatabase.IFeature;
            if (fe == null)
            {
                return;
            }

            string newGUID = Guid.NewGuid().ToString();
            string stacod = "增加";
            int state = NewState;

            int smgiver;
            int.TryParse(fe.get_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabVERSION)).ToString(), out smgiver);
            string guid = fe.get_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabGUID)).ToString();
            string v = fe.get_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabSTACOD)).ToString();
            if (smgiver > m_Application.Workspace.LocalBaseVersion && !string.IsNullOrEmpty(guid))
            {
                //复制协调数据操作产生的新建要素操作,不改变guid
                newGUID = guid;

                if (v == "原始" || v == "修改")
                {
                    stacod = "修改";
                }
                state = EditState;
            }

            #region 实时记录状态表
            var fc = fe.Class as IFeatureClass;
            if (obj.Class.AliasName != layerName)
            {
                recordTable = null;
            }

            ITable tb = GetTableByFeature(fe);
            if (tb != null)
            {
                if (guidIndex != -1)
                {
                    create_fid = newGUID;
                    IRow newRow = tb.CreateRow();
                    newRow.set_Value(tb.Fields.FindField("OPTYPE"), "新增");
                    newRow.set_Value(tb.Fields.FindField("KEYGID"), create_fid);
                    newRow.set_Value(tb.Fields.FindField("OPUSER"), UserName);
                    newRow.set_Value(tb.Fields.FindField("OPTIME"), DateTime.Now.ToString());
                    newRow.set_Value(tb.Fields.FindField("VERSID"), m_Application.Workspace.LocalBaseVersion);
                    newRow.Store();
                    hasExedCreateOper = true;
                }
            }
            #endregion

            if (!EnableUpdate)//未开启更新，不修改状态（制图中心要求）
            {
                return;
            }

            //如果正处于协调状态则不记录新增状态
            if (m_Application.Workspace.IsCollaborativing)
            {
                return;
            }

            //5更
            fe.set_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabSTACOD), stacod);
            //fe.set_Value(fe.Fields.FindField("VERS"), Vers);

            //协同作业
            fe.set_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabOPUSER), UserName);//记录操作者
            fe.set_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabVERSION), state);
            fe.set_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabGUID), newGUID);
            //corrector有长度限制，需截取
            int fieldLenth = fe.Fields.get_Field(fe.Fields.FindField("corrector")).Length;
            if (GlobalClass.strname.Length > fieldLenth)
            {
                fe.set_Value(fe.Fields.FindField("corrector"), GlobalClass.strname.Substring(0, fieldLenth));
            }
            else
            {
                fe.set_Value(fe.Fields.FindField("corrector"), GlobalClass.strname);
            }
            fe.set_Value(fe.Fields.FindField("date"), DateTime.Now.ToString("yyyyMMdd"));
        }

        void UApplication_OnSelectionChanged()
        {
            IMap map = m_Application.ActiveView.FocusMap;
            if (map.SelectionCount != 0)
            {
                hasExedCreateOper = false;
                delete_fid.Clear();
            }
        }

        void UApplication_OnCurrentLayerChanged()
        {
            recordTable = null;
        }

        ITable GetTableByFeature(IFeature fe)
        {
            if (fe == null)
                return null;
            if (recordTable == null)
            {
                IFeatureClass fc = fe.Class as IFeatureClass;
                guidIndex = fc.FindField(ServerDataInitializeCommand.CollabGUID);

                IDataset dt = fc as IDataset;
                layerName = dt.Name;
                IWorkspace ws = dt.Workspace;
                
                if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, "RecordTable"))
                {
                    IFeatureWorkspace fews = ws as IFeatureWorkspace;
                    recordTable = fews.OpenTable("RecordTable");
                }
            }
            return recordTable;
        }

        public int GetBaseVersID()
        {
            var ws = m_Application.Workspace.EsriWorkspace;
            if (!(ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, "SMGILocalState"))
            {
                return -1;
            }

            var localState = (ws as IFeatureWorkspace).OpenTable("SMGILocalState");
            var cusor = localState.Search(null, true);
            var f = cusor.NextRow();
            if (f != null)
            {
                return Convert.ToInt32(f.get_Value(f.Fields.FindField("BASEVERSION")));
            }
            return -1;
        }

        public bool IsBeyondLocalBaseNum(int guid)
        {
            var ws = m_Application.Workspace.EsriWorkspace;
            var localState = (ws as IFeatureWorkspace).OpenTable("SMGILocalState");
            var cusor = localState.Search(null, true);
            var f = cusor.NextRow();
            if (f != null)
            {
                int baseVersID = Convert.ToInt32(f.get_Value(f.Fields.FindField("BASEVERSION")));
                if (guid > baseVersID)
                {
                    return true;
                }
            }
            return false;
        }

        void UApplication_OnStartEditing()
        {
            if (!EnableUpdate)
            {
                if (System.Windows.Forms.MessageBox.Show("没有开启协同更新功能，是否开启？", "提示", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    EnableUpdate = true;
                }
            }

            //开启编辑后，默认为编辑状态
            if (m_Application.PluginManager.Commands.ContainsKey("SMGI.Plugin.GeneralEdit.EditSelector"))
            {
                PluginCommand cmd = m_Application.PluginManager.Commands["SMGI.Plugin.GeneralEdit.EditSelector"];
                if (cmd != null && cmd.Enabled)
                {
                    m_Application.MapControl.CurrentTool = cmd.Command as ITool;
                }
            
            }

            //设置基版本号
            m_Application.Workspace.LocalBaseVersion = GetBaseVersID();
        }

        #endregion
    }
}
