using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 协同作业操作记录
    /// </summary>
    public class cmdUpdateRecord : SMGI.Common.SMGICommand
    {
        public static bool EnableUpdate { get; set; }//未开启更新时，与在arcmap中编辑保持一致
        public static string UserName { get; set; }
        public static string Vers { get; set; }

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

        private IEngineEditor editor;

        ITable recordTable;
        string layerName;
        IWorkspace workspace;
        IFeatureWorkspace featureWorkspace;
        private bool hasExedCreateOper = false;
        private string create_fid = "";
        private List<string> delete_fid = new List<string>();

        CmdUpdateDataFilter cmdDF = null;
        SnapEnvConfigCmd cmdSnapConfig = null;


        //协同字段索引号
        int guidIndex = -1;
        int verIndex = -1;
        int delIndex = -1;
        int userIndex = -1;

        //数据更新状态（CollVERSION）
        public const int NewState = -1;//本地新增要素状态值
        public const int EditState = -2;//从服务器下载数据的编辑状态
        public const string DelStateText = "是";//要素的删除状态文本{同时版本号为负数}

        public static string CollabGUID
        {
            get;
            private set;
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
        public static string CollabOPUSER
        {
            get;
            private set;
        }

        public cmdUpdateRecord()
        {
            m_caption = "UpdateRecord";

            Vers = DateTime.Now.Year.ToString();
            UserName = System.Environment.MachineName;
        }


        #region Overridden Class Methods
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);

            CollabGUID = m_Application.TemplateManager.getFieldAliasName("SMGIGUID");
            CollabVERSION = m_Application.TemplateManager.getFieldAliasName("SMGIVERSION");
            CollabDELSTATE = m_Application.TemplateManager.getFieldAliasName("SMGIDEL");
            CollabOPUSER = m_Application.TemplateManager.getFieldAliasName("SMGIOPUSER");


            editor = app.EngineEditor;

            var root = app.Template.Root;
            var content = app.Template.Content;
            var dataEle = content.Element("DataUpdate5W");
            bool IsOpenUpdate = Convert.ToBoolean(dataEle.Value);
            if (IsOpenUpdate)
            {
                EnableUpdate = true;
            }
           (editor as IEngineEditEvents_Event).OnCreateFeature += new IEngineEditEvents_OnCreateFeatureEventHandler(UApplication_OnCreateFeature);
           (editor as IEngineEditEvents_Event).OnChangeFeature += new IEngineEditEvents_OnChangeFeatureEventHandler(UApplication_OnChangeFeature);
           (editor as IEngineEditEvents_Event).OnDeleteFeature += new IEngineEditEvents_OnDeleteFeatureEventHandler(UApplication_OnDeleteFeature);
           (editor as IEngineEditEvents_Event).OnSelectionChanged += new IEngineEditEvents_OnSelectionChangedEventHandler(UApplication_OnSelectionChanged);
           (editor as IEngineEditEvents_Event).OnStartEditing += new IEngineEditEvents_OnStartEditingEventHandler(UApplication_OnStartEditing);
            
            //5万更新
           AxToolbarControl toolBar = (app.MainForm as ISMGIMainForm).MapHBar;
           if (toolBar != null)
           {
               cmdSnapConfig  = new SnapEnvConfigCmd(app);
               cmdDF = new CmdUpdateDataFilter(app);

               toolBar.AddItem(cmdSnapConfig, 0, -1, false, 0, esriCommandStyles.esriCommandStyleIconOnly);
               //toolBar.AddItem(cmdConfig, 0, -1, true, 0, esriCommandStyles.esriCommandStyleIconOnly);//向toolbarcontrol中增加自定义工具选项
               toolBar.AddItem(cmdDF, 0, -1, true, 0, esriCommandStyles.esriCommandStyleIconOnly);//向toolbarcontrol中增加自定义工具选项
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
            workspace = m_Application.Workspace.EsriWorkspace;
            featureWorkspace = (IFeatureWorkspace)workspace;
            IFeature fe = obj as ESRI.ArcGIS.Geodatabase.IFeature;
            if (fe == null)
            {
                return;
            }
            try
            {
                IFeatureClass spFC = featureWorkspace.OpenFeatureClass(fe.Class.AliasName);
            }
            catch (Exception ex)
            {
                return;
            }

            IFeatureClass fc = fe.Class as IFeatureClass;
            if (!fc.HasCollabField())
                return;////要素类没有协同字段

            guidIndex = fc.FindField(cmdUpdateRecord.CollabGUID);
            verIndex = fc.FindField(cmdUpdateRecord.CollabVERSION);
            delIndex = fc.FindField(cmdUpdateRecord.CollabDELSTATE);
            userIndex = fc.FindField(cmdUpdateRecord.CollabOPUSER);


            #region 实时记录要素编辑状态
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

            

            if (!EnableUpdate)//未开启更新，不修改状态
            {
                return;
            }

            //协调过程中可以删除
            if (m_Application.Workspace.IsCollaborativing)
            {
                return;
            }

            int collver;
            int.TryParse(fe.get_Value(verIndex).ToString(), out collver);
            if (collver == NewState)//本地新增要素，不记录删除状态，直接删除
            {
                return;
            }

            if (collver > m_Application.Workspace.LocalBaseVersion)//协调数据
            {
                return;
            }

            //协同作业
            fe.set_Value(userIndex, UserName);//记录操作者
            fe.set_Value(verIndex, EditState);//删除视为一种特殊的编辑操作
            fe.set_Value(delIndex, DelStateText);//标记删除状态


            //增加要素副本，记录删除状态
            var cursor = fc.Insert(true);
            cursor.InsertFeature(fe as ESRI.ArcGIS.Geodatabase.IFeatureBuffer);
            cursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
        }

        public void UApplication_OnChangeFeature(ESRI.ArcGIS.Geodatabase.IObject obj)
        {
            workspace = m_Application.Workspace.EsriWorkspace;
            featureWorkspace = (IFeatureWorkspace)workspace;

            IFeature fe = obj as ESRI.ArcGIS.Geodatabase.IFeature;
            if (fe == null)
            {
                return;
            }
            try
            {
                IFeatureClass spFC = featureWorkspace.OpenFeatureClass(fe.Class.AliasName);
            }
           catch (Exception ex)
            {
                return;
            }

            IFeatureClass fc = fe.Class as IFeatureClass;
            if (!fc.HasCollabField())
                return;////要素类没有协同字段

            guidIndex = fc.FindField(cmdUpdateRecord.CollabGUID);
            verIndex = fc.FindField(cmdUpdateRecord.CollabVERSION);
            delIndex = fc.FindField(cmdUpdateRecord.CollabDELSTATE);
            userIndex = fc.FindField(cmdUpdateRecord.CollabOPUSER);


            #region  实时记录编辑状态
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
                        delete_fid.Clear();
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

            if (!EnableUpdate)//未开启更新，不修改状态
            {
                return;
            }

            //如果正处于协调状态则不作修改
            if (m_Application.Workspace.IsCollaborativing)
            {
                return;
            }

            int collver;
            int.TryParse(fe.get_Value(verIndex).ToString(), out collver);
            if (collver > m_Application.Workspace.LocalBaseVersion)//协调数据
            {
                return;
            }

            //协同作业
            if (collver >= 0)
            {
                fe.set_Value(verIndex, EditState);
            }
            fe.set_Value(userIndex, UserName);//记录操作者
        }

        public void UApplication_OnCreateFeature(ESRI.ArcGIS.Geodatabase.IObject obj)
        {
            workspace = m_Application.Workspace.EsriWorkspace;
            featureWorkspace = (IFeatureWorkspace)workspace;
            IFeature fe = obj as ESRI.ArcGIS.Geodatabase.IFeature;
            if (fe == null)
            {
                return;
            }
            try
            {
                IFeatureClass spFC = featureWorkspace.OpenFeatureClass(fe.Class.AliasName);
            }
            catch (Exception ex)
            {
                return;
            }

            IFeatureClass fc = fe.Class as IFeatureClass;
            if (!fc.HasCollabField())
                return;////要素类没有协同字段

            guidIndex = fc.FindField(cmdUpdateRecord.CollabGUID);
            verIndex = fc.FindField(cmdUpdateRecord.CollabVERSION);
            delIndex = fc.FindField(cmdUpdateRecord.CollabDELSTATE);
            userIndex = fc.FindField(cmdUpdateRecord.CollabOPUSER);


            string newGUID = Guid.NewGuid().ToString();
            int state = NewState;

            int collver;
            int.TryParse(fe.get_Value(verIndex).ToString(), out collver);
            string guid = fe.get_Value(guidIndex).ToString();
            if (collver > m_Application.Workspace.LocalBaseVersion && !string.IsNullOrEmpty(guid))
            {
                //复制协调数据操作产生的新建要素操作,不改变guid
                newGUID = guid;

                state = EditState;
            }

            #region 实时记录状态表
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


            if (!EnableUpdate)//未开启更新，不修改状态
            {
                return;
            }

            //如果正处于协调状态则不记录新增状态
            if (m_Application.Workspace.IsCollaborativing)
            {
                return;
            }

            //协同作业
            fe.set_Value(userIndex, UserName);//记录操作者
            fe.set_Value(verIndex, state);
            fe.set_Value(guidIndex, newGUID);
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

        void UApplication_OnStartEditing()
        {
            if (!EnableUpdate)
            {
                if (System.Windows.Forms.MessageBox.Show("没有开启更新工具，是否开启？", "提示", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
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
            if(!(ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, "SMGILocalState"))
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

        #endregion
    }

    public static class FeatureClassExtensions
    {
        /// <summary>
        /// 判断要素类是否存在协同字段
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        public static bool HasCollabField(this IFeatureClass fc)
        {
            if (fc.FindField(cmdUpdateRecord.CollabGUID) == -1 || fc.FindField(cmdUpdateRecord.CollabVERSION) == -1 || 
                fc.FindField(cmdUpdateRecord.CollabDELSTATE) == -1 || fc.FindField(cmdUpdateRecord.CollabOPUSER) == -1)
            {
                return false;//要素类没有协同字段
            }

            return true;
        }
    }
}
