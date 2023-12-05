using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;
using System.Threading;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Carto;

using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using System.Xml.Linq;
using System.Diagnostics;
using System.Data;
using System.Data .OleDb;


namespace SMGI.Common
{
    public class GApplication
    {
        public static IWorkspaceFactory GDBFactory = new FileGDBWorkspaceFactoryClass();
        public static IWorkspaceFactory MDBFactory = new AccessWorkspaceFactoryClass();
        public static IWorkspaceFactory ShpFactory = new ShapefileWorkspaceFactoryClass();
        public static IWorkspaceFactory MemoryFactory = new InMemoryWorkspaceFactory();
        public static IWorkspaceFactory RasterFactory = new RasterWorkspaceFactoryClass();
        //public static GeometryEnvironmentClass GeometryEnvironment = new GeometryEnvironmentClass();
        public static string ApplicationName = "";

        public static bool NoDelete = false;

        public double ConvertPixelsToMapUnits(double pixelUnits)
        {
            int pixelExtent = ActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().right
                           - ActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().left;

            double realWorldDisplayExtent = ActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds.Width;
            double sizeOfOnePixel = realWorldDisplayExtent / pixelExtent;

            return pixelUnits * sizeOfOnePixel;
        }

        public DataTable ReadToDataTable(string MDBPath, string TableName)
        {
            DataTable result = new DataTable();
            string connectionString = string.Format ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}",MDBPath );
            OleDbConnection connection = new OleDbConnection(connectionString);
            OleDbDataAdapter adapter = new OleDbDataAdapter(string.Format ( "Select * from {0}",TableName), connection);
            adapter.Fill(result);
            result.TableName = TableName;
            connection.Close();
            return result;
        }
        #region OverrideValueSet Functions
        public void OverrideValueSet(IRepresentationRule newRule, IRepresentation rep)
        {
            var ruleOrg = (rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID) as IClone).Clone() as IRepresentationRule;
            var rule = rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID);
            OverrideValueSet(rule as IGeometricEffects, newRule as IGeometricEffects, rep);
            for (int i = 0; i < rule.LayerCount; i++)
            {
                var layer = rule.Layer[i];
                var newLayer = newRule.Layer[i];
                //全局效果（几何效果集合）
                OverrideValueSet(layer as IGeometricEffects, newLayer as IGeometricEffects, rep);
                OverrideValueSet(layer as IGraphicAttributes, newLayer as IGraphicAttributes, rep);

                if (layer is IBasicMarkerSymbol)
                {
                    OverrideValueSet((layer as IBasicMarkerSymbol).MarkerPlacement as IGraphicAttributes,
                        (newLayer as IBasicMarkerSymbol).MarkerPlacement as IGraphicAttributes, rep);
                }
                else if (layer is IBasicLineSymbol)
                {
                    OverrideValueSet((layer as IBasicLineSymbol).Stroke as IGraphicAttributes,
                        (newLayer as IBasicLineSymbol).Stroke as IGraphicAttributes, rep);
                }
                else if (layer is IBasicFillSymbol)
                {
                    OverrideValueSet((layer as IBasicFillSymbol).FillPattern as IGraphicAttributes,
                        (newLayer as IBasicFillSymbol).FillPattern as IGraphicAttributes, rep);
                }
            }
            rep.RepresentationClass.RepresentationRules.set_Rule(rep.RuleID, ruleOrg);

        }
        public void OverrideValueSet(IGeometricEffects ges, IGeometricEffects newges, IRepresentation rep)
        {
            if (ges == null || newges == null || rep == null)
                return;
            for (int j = 0; j < ges.Count; j++)
            {
                var ge = ges.Element[j];
                var newga = newges.Element[j] as IGraphicAttributes;
                IGraphicAttributes ga = ge as IGraphicAttributes;
                OverrideValueSet(ga, newga, rep);
            }
        }
        public void OverrideValueSet(IGraphicAttributes ga, IGraphicAttributes newga, IRepresentation rep)
        {
            if (ga == null || newga == null || rep == null)
                return;
            for (int k = 0; k < ga.GraphicAttributeCount; k++)
            {
                var id = ga.get_ID(k);
                object obj = newga.get_Value(id);
                rep.set_Value(ga, id, obj);
            }
        }
        #endregion

        #region RuleValueSet Functions
        public void RuleValueSet(IGeometricEffects ges, IGeometricEffects newges, IRepresentation rep)
        {
            if (ges == null || newges == null || rep == null)
            {
                return;
            }
            for (int j = 0; j < ges.Count; j++)
            {
                var ge = ges.Element[j];
                var newge = newges.Element[j];
                var ga = ge as IGraphicAttributes;
                var newga = newge as IGraphicAttributes;
                RuleValueSet(ga, newga, rep);
            }
        }

        public void RuleValueSet(IGraphicAttributes ga, IGraphicAttributes newga, IRepresentation rep)
        {
            if (ga == null || newga == null || rep == null)
            {
                return;
            }
            for (int k = 0; k < ga.GraphicAttributeCount; k++)
            {
                var id = ga.get_ID(k);
                object obj = rep.get_Value(ga, id);
                newga.set_Value(id, obj);
            }
        }
        public IRepresentationRule RuleValueSet(IRepresentation rep)
        {
            var r = rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID);
            var newrule = (r as IClone).Clone() as IRepresentationRule;
            RuleValueSet(r as IGeometricEffects, newrule as IGeometricEffects, rep);
            for (int i = 0; i < newrule.LayerCount; i++)
            {
                var layer = r.Layer[i];
                var newlayer = newrule.Layer[i];
                RuleValueSet(layer as IGeometricEffects, newlayer as IGeometricEffects, rep);
                RuleValueSet(layer as IGraphicAttributes, newlayer as IGraphicAttributes, rep);
                if (layer is IBasicMarkerSymbol)
                {
                    RuleValueSet((layer as IBasicMarkerSymbol).MarkerPlacement as IGraphicAttributes,
                        (newlayer as IBasicMarkerSymbol).MarkerPlacement as IGraphicAttributes, rep);
                }
                else if (layer is IBasicLineSymbol)
                {
                    RuleValueSet((layer as IBasicLineSymbol).Stroke as IGraphicAttributes,
                        (newlayer as IBasicLineSymbol).Stroke as IGraphicAttributes, rep);
                }
                else if (layer is IBasicFillSymbol)
                {
                    RuleValueSet((layer as IBasicFillSymbol).FillPattern as IGraphicAttributes,
                       (newlayer as IBasicFillSymbol).FillPattern as IGraphicAttributes, rep);
                }
            }
            return newrule;
        }
        #endregion


        public GWorkspace Workspace
        {
            get
            {
                return mWorkspace;
            }
            internal set
            {
                // SetBusy(true);
                if (mWorkspace != null)
                {
                    mWorkspace.Close();
                    mWorkspace = null;
                    if (WorkspaceClosed != null)
                    {
                        WorkspaceClosed(this, new EventArgs());
                    }
                }
                mWorkspace = value;
                if (mWorkspace == null)
                {
                    MainForm.Title = ApplicationName;
                    this.MapControl.Map = new ESRI.ArcGIS.Carto.MapClass() { Name = "未打开" };

                    this.PageLayoutControl.PageLayout = new ESRI.ArcGIS.Carto.PageLayoutClass();
                    SynsMapWhenWorkspaceNull(MainForm.LayoutState);
                }
                else
                {
                    MainForm.Title = ApplicationName + "-" + Workspace.FullName;
                    MapControl.Map = mWorkspace.Map;
                    PageLayoutControl.PageLayout = mWorkspace.PageLayout;

                    MainForm.LayoutState = mWorkspace.LastState;
                    Active(mWorkspace.LastState);
                    //this.MainForm_MapLayoutChanged(MainForm,new LayoutChangedArgs(mWorkspace.LastState,mWorkspace.LastState));

                    if (WorkspaceOpened != null)
                    {
                        WorkspaceOpened(this, new EventArgs());
                    }
                }

                TOCControl.Refresh();
                PageLayoutControl.Refresh();
                MapControl.Refresh();
                // SetBusy(false);
            }
        }

        private void Active(LayoutState state)
        {
            if (MapControl.ActiveView.IsActive())
                MapControl.ActiveView.Deactivate();
            if (PageLayoutControl.ActiveView.IsActive())
                PageLayoutControl.ActiveView.Deactivate();
            if (state == Common.LayoutState.MapControl)
            {
                MapControl.ActiveView.Activate(MapControl.hWnd);
            }
            else
            {
                PageLayoutControl.ActiveView.Activate(PageLayoutControl.hWnd);
            }
        }
        private GWorkspace mWorkspace;

        public IConfig AppConfig { get; internal set; }

        public Parameter GParameters { get; internal set; }

        public AxMapControl MapControl { get; internal set; }
        public AxPageLayoutControl PageLayoutControl { get; internal set; }
        public IActiveView ActiveView
        {
            get
            {
                return this.LayoutState == Common.LayoutState.MapControl ? MapControl.ActiveView : PageLayoutControl.ActiveView;
            }
        }

        public AxTOCControl TOCControl { get; internal set; }

        public ParameterDialog GenParaDlg { get; internal set; }

        public IWorkspace MemoryWorkspace { get; internal set; }

        public void SetStusBar(string tips)
        {
            this.MainForm.ShowToolDes(tips);

        }

        public TemplateManager TemplateManager { get; internal set; }
        public GTemplate Template { get { return TemplateManager.Template; } }

        #region rendy
        //public StyleManager StyleMgr { get; internal set; }
        public SelectDataDlg SelectDataDialog { get; internal set; }
        //public StyleKnowledgeBase StyleKnowledgeBases { get; internal set; }
        #endregion
        //public PageLayoutForm PageLayoutForm4D { get; internal set; }

        public ClassificationAndCode ClassificationAndCode { get; internal set; }


        public LayoutState LayoutState { get; internal set; }

        /// <summary>
        /// 返回值是 IMap、ILayer、ILegendGroup、ILengendClass、Null中的一种
        /// </summary>
        public TOCSeleteItem TOCSelectItem
        {
            get
            {
                esriTOCControlItem pItem = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap pMap = null;
                ILayer pLayer = null;
                object pOther = new object();
                object pIndex = new object();
                TOCControl.GetSelectedItem(ref pItem, ref pMap, ref pLayer, ref pOther, ref pIndex);

                switch (pItem)
                {
                    case esriTOCControlItem.esriTOCControlItemHeading:
                        return new TOCSeleteItem(pItem, pMap, pLayer, pOther as ILegendGroup, null);
                    case esriTOCControlItem.esriTOCControlItemLayer:
                        return new TOCSeleteItem(pItem, pMap, pLayer, null, null);
                    case esriTOCControlItem.esriTOCControlItemLegendClass:
                        {
                            int i = (int)pIndex;
                            if (i == -1)
                                return new TOCSeleteItem(pItem, pMap, pLayer, pOther as ILegendGroup, null);
                            else
                                return new TOCSeleteItem(pItem, pMap, pLayer, pOther as ILegendGroup, (pOther as ILegendGroup).Class[(int)pIndex]);
                        }
                    case esriTOCControlItem.esriTOCControlItemMap:
                        return new TOCSeleteItem(pItem, pMap, null, null, null);
                    case esriTOCControlItem.esriTOCControlItemNone:
                    default:
                        return new TOCSeleteItem(pItem, null, null, null, null);
                }
            }
        }
        //public GBAndLayerInfo GBsAndLayerInfo { get; internal set; }

        // public Dictionary<root, int> workpapers = new Dictionary<root, int>();
        //public List<root> workpapers = new List<root>();
        public void InitESRIWorkspace(IWorkspace workspace)
        {
            this.Workspace = GWorkspace.Init(this, workspace);
            this.TemplateManager.Template.MatchCurrentWorkspace();
            this.Workspace.Save();
        }
        public void InitESRIRasterWorkspace(IWorkspace workspace, string filename)
        {
            this.Workspace = GWorkspace.InitRaster(this, workspace, filename);
            this.TemplateManager.Template.MatchCurrentWorkspace();
            this.Workspace.RasterSave();
        }
        public void OpenESRIWorkspace(IWorkspace workspace)
        {
            this.Workspace = GWorkspace.Open(this, workspace);
        }
        public void OpenWorkspace(string path)
        {
            //MainForm.LayoutState = Common.LayoutState.MapControl;
            this.Workspace = GWorkspace.Open(this, path);
        }
        public void CreateWorkspace(string path)
        {
            //MainForm.LayoutState = Common.LayoutState.MapControl;
            this.Workspace = GWorkspace.CreateWorkspace(this, path);
        }
        public void CloseWorkspace()
        {
            //MainForm.LayoutState = Common.LayoutState.MapControl;
            this.Workspace.Close();
            this.Workspace = null;
        }

        public IPluginHost MainForm { get; private set; }

        public IEngineEditor EngineEditor { get; internal set; }
        //private WaitForm waitForm;
        public ESRI.ArcGIS.Geoprocessor.Geoprocessor GPTool { get; internal set; }

        public event EventHandler WorkspaceOpened;
        public event EventHandler WorkspaceClosed;
        public event EventHandler AppExist;
        public PluginManager PluginManager;

        public static GApplication Application { get; internal set; }
        /// <summary>
        /// 简化app
        /// </summary>
        /// <param name="host"></param>
        /// <param name="initForm"></param>
        public GApplication(IPluginHost host, InitForm initForm, int unused)
        {
            this.MainForm = host;
            MainForm.Title = ApplicationName;
            MapControl = host.MapControl;
            //  string name = app.MapControl.Map.AnnotationEngine.ToString();
            //IAnnotateMap sm1 = new MaplexAnnotateMapClass();
            //MapControl.Map.AnnotationEngine = sm1;
            // app.Workspace.Map.AnnotationEngine = sm1;
            // MapControl.Map.Name = "未打开";
            MapControl.Focus();
            PageLayoutControl = host.PageLayoutControl;

            TOCControl = host.TocControl;

            mWorkspace = null;
            EngineEditor = new EngineEditorClass();
            AppConfig = new XmlConfig();
            GParameters = new Parameter();
            object para = AppConfig["_gPara"];
            if (para != null)
            {
                GParameters.LoadFromString(para.ToString());
            }
            GPTool = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();


            Application = this;
        }

        public GApplication(IPluginHost host, IInitInfo initForm, TemplateManager templateManager)
        {

            this.MainForm = host;

            MapControl = host.MapControl;
            MapControl.Map.Name = "未打开";
            MapControl.Focus();
            PageLayoutControl = host.PageLayoutControl;

            TOCControl = host.TocControl;
            this.TemplateManager = templateManager;
            this.TemplateManager.Template.Application = this;

            if (Template != null)
            {
                if (ApplicationName == "")
                {
                    MainForm.Title = Template.Caption;
                    ApplicationName = Template.Caption;
                }
                else
                {
                    MainForm.Title = Template.ClassName + "-" + ApplicationName;
                }
            }
            else
            {
                MainForm.Title = ApplicationName;
            }



            mWorkspace = null;
            EngineEditor = new EngineEditorClass();
            AppConfig = new XmlConfig();
            GParameters = new Parameter();
            object para = AppConfig["_gPara"];
            if (para != null)
            {
                GParameters.LoadFromString(para.ToString());
            }


            SelectDataDialog = new SelectDataDlg();
            GenParaDlg = new ParameterDialog(GParameters);
            initForm.Info("正在初始化批量处理工具......");
            GPTool = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();
            GPTool.OverwriteOutput = true;

            MainForm.Closing += mainform_FormClosing;

            initForm.Info("正在注册地图视图事件......");
            MapControlManager mm = new MapControlManager(this);
            initForm.Info("正在注册图层管理事件......");
            TOCEventManager tMgr = new TOCEventManager(TOCControl, this);
            initForm.Info("正在初始化缓存数据库......");
            ESRI.ArcGIS.esriSystem.IName mName = MemoryFactory.Create("", "Memorty", null, 0) as ESRI.ArcGIS.esriSystem.IName;
            MemoryWorkspace = mName.Open() as IWorkspace;

            initForm.Info("正在加载制图插件......");
            PluginManager = new PluginManager(this);

            SynsMapWhenWorkspaceNull(MainForm.LayoutState);

            MainForm.MapLayoutChanged += new EventHandler<LayoutChangedArgs>(MainForm_MapLayoutChanged);

            Application = this;

            XElement contentXEle = Template.Content;
            //根据选择的模板获取启动图片
            XElement paraXEle = contentXEle.Element("ParaTable");
            if (paraXEle != null)
            {
                bool isOpenPara = Convert.ToBoolean(paraXEle.Value);
                if (isOpenPara)
                {
                    System.Windows.Forms.Timer tm = new System.Windows.Forms.Timer();
                    tm.Tick += new EventHandler(tm_Tick);
                    tm.Interval = 200;
                    tm.Start();
                }
            }
            initForm.Hide();
        }

        void tm_Tick(object sender, EventArgs e)
        {
            (sender as System.Windows.Forms.Timer).Stop();
            MainForm.ShowChild(GenParaDlg.Handle);
        }

        public bool DoCommand(string command, XElement args = null, Action<string> messageRaisedAction = null)
        {
            return (PluginManager.Commands[command].Command as ISMGIAutomaticCommand)
                    .DoCommand(args, messageRaisedAction);
        }

        private void SynsMapWhenWorkspaceNull(LayoutState state)
        {
            if (MapControl.ActiveView.IsActive())
                MapControl.ActiveView.Deactivate();
            if (!PageLayoutControl.ActiveView.IsActive())
                PageLayoutControl.ActiveView.Activate(PageLayoutControl.hWnd);

            IMaps maps = new SMGIMaps();
            maps.Reset();
            maps.Add(MapControl.Map);
            PageLayoutControl.PageLayout.ReplaceMaps(maps);
            PageLayoutControl.ActiveView.Deactivate();
            if (state == Common.LayoutState.MapControl)
                MapControl.ActiveView.Activate(this.MapControl.hWnd);
            else
                PageLayoutControl.ActiveView.Activate(PageLayoutControl.hWnd);
        }

        void MainForm_MapLayoutChanged(object sender, LayoutChangedArgs e)
        {
            LayoutState = e.CurrentState;

            if (Workspace == null)
            {
                Active(LayoutState);
                return;
            }

            Workspace.LastState = LayoutState;
            if (LayoutState == Common.LayoutState.PageLayoutControl)
            {
                Workspace.MapExtent = MapControl.ActiveView.Extent;
                MapControl.Map = new MapClass();
                //MapControl.ActiveView.Deactivate();

                PageLayoutControl.ActiveView.Activate(PageLayoutControl.hWnd);
                TOCControl.SetBuddyControl(PageLayoutControl);

                if (Workspace.PageLayoutExtent != null)
                {
                    IMapFrame mapFrame = (PageLayoutControl.PageLayout as IGraphicsContainer).FindFrame(Workspace.Map) as IMapFrame;
                    //mapFrame.ExtentType = esriExtentTypeEnum.esriExtentScale;
                    if (mapFrame != null)
                        (mapFrame.Map as IActiveView).Extent = Workspace.PageLayoutExtent;
                }
                PageLayoutControl.ActiveView.Refresh();
                PageLayoutControl.Focus();
            }
            else
            {
                IMapFrame mapFrame = (PageLayoutControl.PageLayout as IGraphicsContainer).FindFrame(Workspace.Map) as IMapFrame;
                if (mapFrame != null)
                    Workspace.PageLayoutExtent = (mapFrame.Map as IActiveView).Extent;
                PageLayoutControl.ActiveView.Deactivate();

                MapControl.Map = Workspace.Map;
                //MapControl.ActiveView.Activate(MapControl.hWnd);
                TOCControl.SetBuddyControl(MapControl);
                MapControl.Focus();

                if (Workspace.MapExtent != null)
                    MapControl.ActiveView.Extent = Workspace.MapExtent;
            }
        }


        void mainform_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            //plugMgr.Save();
            if (EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing)
            {
                System.Windows.Forms.DialogResult r = System.Windows.Forms.MessageBox.Show("正在开启编辑，是否保存编辑？", "提示", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                if (r == System.Windows.Forms.DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    EngineEditor.StopEditing(r == System.Windows.Forms.DialogResult.Yes);
                }
            }

            if (Workspace != null)
            {
                System.Windows.Forms.DialogResult r = System.Windows.Forms.MessageBox.Show("工作区尚未保存，是否保存？", "提示", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                if (r == System.Windows.Forms.DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    if (r == System.Windows.Forms.DialogResult.Yes)
                        Workspace.Save();
                    if (abort != null)
                    {
                        abort(this);
                    }
                }
            }
            if (!e.Cancel)
            {
                AppConfig["_gPara"] = GParameters.SaveToString();
                if (AppExist != null)
                    AppExist(this, new EventArgs());
            }
        }
        internal static string RegFilePath
        {
            get
            {
                return AppDataPath + @"\ECarto";
            }
        }
        public static string AppDataPath
        {
            get
            {
                if (System.Environment.OSVersion.Version.Major <= 5)
                {
                    return GApplication.RootPath;
                }

                var dp = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var di = new System.IO.DirectoryInfo(dp);
                var ds = di.GetDirectories("SMGI");
                if (ds == null || ds.Length == 0)
                {
                    var sdi = di.CreateSubdirectory("SMGI");
                    return sdi.FullName;
                }
                else
                {
                    return ds[0].FullName;
                }
            }
        }
        public static string ExePath
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            }
        }
        public static string RootPath
        {
            get
            {
                return System.IO.Path.GetFullPath(ExePath + @"\..");
            }

        }
        public static string PluginPath
        {
            get
            {
                return RootPath + @"\Plugins";
            }
        }
        public static string DocumentPath
        {
            get
            {
                return RootPath + @"\Document";
            }
        }

        public static string TemplatePath
        {
            get
            {
                return RootPath + @"\Template";
            }
        }

        public static string ResourcePath
        {
            get
            {
                return RootPath + @"\Resource";
            }
        }
        public void StartEdit()
        {
            if (Workspace == null)
            {
                return;
            }
            if (EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
            {
                return;
            }
            EngineEditor.StartEditing(Workspace.EsriWorkspace, Workspace.Map);
            EngineEditor.EnableUndoRedo(true);
        }
        public void StopEdit(bool saveChange)
        {
            if (EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
            {
                EngineEditor.StopEditing(saveChange);
            }
        }
        public void SaveEdit()
        {
            if (EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
            {
                EngineEditor.StopEditing(true);
                StartEdit();
            }
        }

        public static void ReConnectData(IWorkspace ws, IMap map)
        {
            var layers = map.get_Layers();
            ILayer l = null;
            layers.Reset();
            IWorkspaceName wname = (ws as IDataset).FullName as IWorkspaceName;
            while ((l = layers.Next()) != null)
            {
                if (l is IDataLayer2)
                {
                    var dl = l as IDataLayer2;
                    if (dl.InWorkspace(ws))
                        continue;
                    IName name = dl.DataSourceName;
                    IDatasetName dname = name as IDatasetName;
                    dname.WorkspaceName = wname;
                    try
                    {
                        dl.Connect(name);
                    }
                    catch (Exception)
                    {

                        continue;
                    }

                }
            }
        }

        public void ShowInfomation(string message, string caption = "提示信息", int time = 5000)
        {
            InfoForm f = new InfoForm { Message = message, Caption = caption, Time = time };
            f.Show(this.MainForm as IWin32Window);
        }

        Thread thread;
        internal event Action<GApplication> abort;
        internal event Action<string> waitText;
        internal event Action<int> maxValue;
        internal event Action<int> step;
        public AutoResetEvent AutoEvent = new AutoResetEvent(false);
        public WaitOperation SetBusy()
        {

            MainForm.BusyStatus = true;
            bool inited = false;
            ThreadStart start = () =>
            {
                WaitForm wait = new WaitForm(this);
                inited = true;
                wait.ShowDialog();

                //wait.Dispose();
            };
            thread = new Thread(start);
            thread.Start();
            WaitOperation wo = new WaitOperation();
            wo.SetText = (v) =>
            {
                if (waitText != null)
                    waitText(v);
            };
            wo.SetMaxValue = (v) =>
                {
                    if (maxValue != null)
                        maxValue(v);
                };
            wo.Step = (v) =>
            {
                if (step != null)
                {
                    step(v);
                }
            };

            wo.dispose = (x) =>
            {
                //AutoEvent.WaitOne();
                if (abort != null)
                    abort(this);
                MainForm.BusyStatus = false;
            };

            //while (!inited)
            //{ 
            //}            
            return wo;

        }

        /// <summary>
        /// SDE数据库连接设置，返回IWorkspace类型
        /// </summary>
        /// <param name="sdeAddress">IP地址</param>
        /// <param name="sdeUsername">用户名</param>
        /// <param name="sdePassword">密码</param>
        /// <param name="sdeDataBaseName">数据库名称</param>
        /// <returns>返回的SDE工作空间</returns>
        public IWorkspace GetWorkspacWithSDEConnection(string sdeAddress, string sdeUsername, string sdePassword, string sdeDataBaseName)
        {
            try
            {
                IPropertySet propset = new PropertySetClass();
                propset.SetProperty("INSTANCE", "sde:postgresql:" + sdeAddress);
                propset.SetProperty("USER", sdeUsername);
                propset.SetProperty("PASSWORD", sdePassword);
                propset.SetProperty("DATABASE", sdeDataBaseName);
                propset.SetProperty("VERSION", "SDE.DEFAULT");
                IWorkspaceFactory factory = new SdeWorkspaceFactoryClass();
                IWorkspace workspace = factory.Open(propset, 0);
                return workspace;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                System.Diagnostics.Trace.WriteLine(string.Format("{0},IP-{1},U-{2},P_len-{3},DB-{4}", "SDE数据库连接失败!", sdeAddress, sdeUsername, sdePassword.Length, sdeDataBaseName));

                MessageBox.Show("SDE数据库连接失败!" + ex.Message);
            }
            return null;
        }

        /// <summary>
        /// SDE数据库连接设置，返回IWorkspace类型
        /// </summary>
        /// <param name="sdeAddress">IP地址</param>
        /// <param name="sdeUsername">用户名</param>
        /// <param name="sdePassword">密码</param>
        /// <param name="sdeDataBaseName">数据库名称</param>
        /// <returns>返回的SDE工作空间</returns>
        public IPropertySet GetPropertysetWithSDEConnection(string sdeAddress, string sdeUsername, string sdePassword, string sdeDataBaseName)
        {
            IPropertySet propset = new PropertySetClass();
            propset.SetProperty("INSTANCE", "sde:postgresql:" + sdeAddress);
            propset.SetProperty("USER", sdeUsername);
            propset.SetProperty("PASSWORD", sdePassword);
            propset.SetProperty("DATABASE", sdeDataBaseName);
            propset.SetProperty("VERSION", "SDE.DEFAULT");
            return propset;
        }

        /// <summary>
        /// 获取SDE路径
        /// </summary>
        /// <param name="path">存放连接文件的路径</param>
        /// <param name="propertySet">IPropertySet实例</param>
        /// <returns></returns>
        public string GetSdeConnectionPath(string path, IPropertySet propertySet)
        {
            string sdeName = "temp.sde";
            string sdePath = path + "\\" + sdeName;
            if (System.IO.File.Exists(sdePath))
            {
                System.IO.File.Delete(sdePath);
            }
            IWorkspaceFactory workspaceFactory = new SdeWorkspaceFactory();
            workspaceFactory.Create(path, sdeName, propertySet, 0);
            return sdePath;
        }

        /// <summary>
        /// 获取指定工作空间下的所有要素类名称和要素数据集名称
        /// </summary>
        /// <param name="targetWorkspace">目标工作空间</param>
        /// <param name="fcNames">要素类名集合</param>
        /// <param name="fdtNames">要素数据集集合</param>
        public void GetDatasetNames(IWorkspace targetWorkspace, ref List<string> fcNames, ref List<string> fdtNames)
        {
            IFeatureWorkspace targetFeatureWorkspace = (IFeatureWorkspace)targetWorkspace;
            IEnumDataset enumDataset = targetWorkspace.get_Datasets(esriDatasetType.esriDTAny);
            enumDataset.Reset();
            IDataset dataset = enumDataset.Next();
            while (dataset != null)
            {
                if (dataset is IFeatureDataset)
                {
                    fdtNames.Add(dataset.Name.Trim());
                    IEnumDataset enumSubset = dataset.Subsets;
                    enumSubset.Reset();
                    IDataset subsetDataset = enumSubset.Next();
                    while (subsetDataset != null)
                    {
                        if (subsetDataset is IFeatureClass)
                        {
                            fcNames.Add(subsetDataset.Name.Trim());
                        }
                        subsetDataset = enumSubset.Next();
                    }
                }

                if (dataset is IFeatureClass)
                {
                    fcNames.Add(dataset.Name.Trim());
                }
                dataset = enumDataset.Next();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(enumDataset);
        }


        /// <summary>
        /// 上锁
        /// </summary>
        /// <param name="objectClass">要上锁的对象</param>
        public void LockObject(ISchemaLock schemaLock)
        {
            schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);
        }

        /// <summary>
        /// 解锁
        /// </summary>
        /// <param name="objectClass">解锁的对象</param>
        public void UnLockObject(ISchemaLock schemaLock)
        {
            schemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);
        }

        /// <summary>
        /// 获取指定对象的锁信息
        /// </summary>
        /// <param name="objectClass">要获取锁的对象</param>
        /// <returns>锁的详细信息，包括用户名和锁的类型</returns>
        public Dictionary<string, esriSchemaLock> GetSchemaLocksForObjectClass(ISchemaLock schemaLock)
        {
            Dictionary<string, esriSchemaLock> dic = new Dictionary<string, esriSchemaLock>();
            IEnumSchemaLockInfo enumSchemaLockInfo = null;
            schemaLock.GetCurrentSchemaLocks(out enumSchemaLockInfo);

            ISchemaLockInfo schemaLockInfo = null;
            while ((schemaLockInfo = enumSchemaLockInfo.Next()) != null)
            {
                dic[schemaLockInfo.UserName] = schemaLockInfo.SchemaLockType;
            }
            return dic;
        }

        /// <summary>
        /// 连接远程共享目录
        /// </summary>
        /// <param name="ipAddress">远程服务器地址</param>
        /// <param name="folderName">共享文件夹名称</param>
        /// <param name="userName">用户名</param>
        /// <param name="passWord">密码</param>
        /// <returns>连接是否成功</returns>
        public bool NetShareConnectState(string ipAddress, string folderName, string userName, string passWord)
        {
            string folderPath = @"\\" + ipAddress + @"\" + folderName;
            bool Flag = false;
            Process proc = new Process();
            try
            {
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                string dosLine = @"net use " + folderPath + " /User:" + ipAddress + @"\" + userName + " " + passWord + " /PERSISTENT:YES";
                proc.StandardInput.WriteLine(dosLine);
                proc.StandardInput.WriteLine("exit");
                while (!proc.HasExited)
                {
                    proc.WaitForExit(1000);
                }

                string errormsg = proc.StandardError.ReadToEnd();
                proc.StandardError.Close();
                if (string.IsNullOrEmpty(errormsg))
                {
                    Flag = true;
                }
                else
                {
                    throw new Exception(errormsg);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }
            return Flag;
        }
    }

    public class WaitOperation : IDisposable
    {
        public Action<string> SetText;
        public Action<int> SetMaxValue;
        public Action<int> Step;

        internal Action<int> dispose;
        public void Dispose()
        {
            dispose(0);
        }
    }
}
