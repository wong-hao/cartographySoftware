using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.CartographyTools;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 使用要素在另一图层中的位置来选择要素
    /// </summary>
    public partial class SelectByLocationForm : Form
    {
        List<string> laylis = new List<string>();
        IWorkspace workspace = null;
        GApplication app;
        IFeatureWorkspace featureWorkspace;
        public SelectByLocationForm(GApplication app)
        {
            this.app = app;
            InitializeComponent();
        }

        private void createHotBtn_Click(object sender, EventArgs e)
        {
            bool pand = false;
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    pand = true;
                }
            }
            if (pand == false || comboBox1.Text == "")
            {
                MessageBox.Show("参数不符合选择条件！");
                return;
            }

            //IWorkspace workspace = app.Workspace.EsriWorkspace;
            //IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
            //IFeatureClass feac = featureWorkspace.OpenFeatureClass(comboBox1.Text);
            //ISpatialFilter sf = new SpatialFilter();
            //sf.Geometry = diffGeo;
            //sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            //IFeature pFeature;
            //IFeatureCursor feacusor = feac.Search(sf, false);
            //while ((pFeature = feacusor.NextFeature()) != null)


            if (checkBox1.Checked == true)
            {
                using (var wo = app.SetBusy())
                {
                    ILayer terl = app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == (comboBox1.Text))).FirstOrDefault();
                    IFeatureLayer pFeatureLayer = terl as IFeatureLayer;
                    IFeatureSelection pFeatureSelection = pFeatureLayer as IFeatureSelection;
                    ISelectionSet pSelectionSet = pFeatureSelection.SelectionSet;
                    IFeature pFeature;
                    int j = 0;
                    //List<IFeature> featureList = new List<IFeature>();
                    if (pSelectionSet.Count > 0)
                    {
                        IEnumIDs IDs = null;
                        IDs = pSelectionSet.IDs;
                        int ID = IDs.Next();
                        while (ID > 0)
                        {
                            pFeature = pFeatureLayer.FeatureClass.GetFeature(ID);
                            j++;
                            wo.SetText("需处理总数：" + pSelectionSet.Count.ToString() + "  已处理数：" + j.ToString());
                            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                            {
                                if (checkedListBox1.GetItemChecked(i))
                                {
                                    ILayer terl1 = app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == (checkedListBox1.Items[i].ToString()))).FirstOrDefault();
                                    if (comboBox2.Text == "与原图层要素相交")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelIntersects, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                    if (comboBox2.Text == "完全位于源图层要素范围内")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelContains, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                    if (comboBox2.Text == "包含源图层要素")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelWithin, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                    if (comboBox2.Text == "与源图层要素相邻")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelTouches, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                    if (comboBox2.Text == "与源图层要素重叠")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelOverlaps, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                }
                            }
                            
                            //featureList.Add(pFeature);
                            ID = IDs.Next();
                        }
                    }
                }
            }
            else
            {
                using (var wo = app.SetBusy())
                {
                    if (comboBox2.Text == "与原图层要素相交")
                    {
                        //创建临时数据库
                        string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
                        IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);


                        ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = new Geoprocessor();
                        gp.OverwriteOutput = true;
                        gp.SetEnvironmentValue("workspace", ws.PathName);

                        IGeoProcessorResult gpResult = null;

                        IFeatureClass temp_union_fc = null; //所有相关面合并后的临时要素类（未融合）
                        IFeatureClass temp_diss_fc = null; //Dissolve后得到的临时结果要素类

                        try
                        {
                            if (wo != null)
                                wo.SetText("正在构建临时要素类......");
                            //IWorkspace workspace = app.Workspace.EsriWorkspace;
                            //IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
                            //IFeatureClass feach = featureWorkspace.OpenFeatureClass(comboBox1.Text);

                            ILayer terl = app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == (comboBox1.Text))).FirstOrDefault();
                            IFeatureLayer pFeatureLayer = terl as IFeatureLayer;
                            IFeatureClass feach = pFeatureLayer.FeatureClass;

                            #region 构建临时要素类
                            //创建临时要素类
                            temp_union_fc = DCDHelper.CreateFeatureClassStructToWorkspace(ws as IFeatureWorkspace, feach, feach.AliasName + "_temp");
                            //复制相关要素
                            //DCDHelper.CopyFeaturesToFeatureClass(targetFC, qf, temp_union_fc, false);
                            //foreach (var referFC in referFCList)
                            //{
                            DCDHelper.CopyFeaturesToFeatureClass(feach, null, temp_union_fc, false);
                            //}
                            #endregion

                            if (wo != null)
                                wo.SetText("正在修复几何......");
                            #region 修复几何
                            RepairGeometry reGeo = new RepairGeometry();
                            reGeo.in_features = temp_union_fc.AliasName;
                            gpResult = (IGeoProcessorResult)gp.Execute(reGeo, null);
                            #endregion

                            #region 分区
                            if (temp_union_fc.FeatureCount(null) > 10000)
                            {
                                CreateCartographicPartitions gPpartition = new CreateCartographicPartitions();
                                gPpartition.in_features = temp_union_fc.AliasName;
                                gPpartition.out_features = "Partitions";
                                gPpartition.feature_count = 5000;
                                gpResult = (IGeoProcessorResult)gp.Execute(gPpartition, null);

                                gp.SetEnvironmentValue("cartographicPartitions", ws.PathName + "\\Partitions");
                            }
                            #endregion

                            if (wo != null)
                                wo.SetText("正在融合要素......");

                            #region 融合
                            Dissolve diss = new Dissolve();
                            diss.in_features = temp_union_fc.AliasName;
                            diss.out_feature_class = temp_union_fc.AliasName + "_diss";
                            diss.multi_part = "SINGLE_PART";
                            gpResult = (IGeoProcessorResult)gp.Execute(diss, null);
                            temp_diss_fc = (ws as IFeatureWorkspace).OpenFeatureClass(temp_union_fc.AliasName + "_diss");
                            #endregion


                            //if (wo != null)
                            //    wo.SetText("正在检查是否为封闭空白区......");

                            //#region 提取微小孔洞到targetFC中
                            //IFeatureClassLoad pFCLoad = targetFC as IFeatureClassLoad;
                            //pFCLoad.LoadOnlyMode = true;
                            //IFeature pFeature = targetFC.CreateFeature();
                            //int gbindex = pFeature.Fields.FindField("GB");

                            //查找是否有孔洞包含点击选取点，创建要素，并将GB置为-1
                            IFeatureCursor pCursor = temp_diss_fc.Search(null, false);
                            IFeature fe = pCursor.NextFeature();
                            //int count = temp_diss_fc.FeatureCount(null);
                            while (fe != null)
                            {
                                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                                {
                                    if (checkedListBox1.GetItemChecked(i))
                                    {
                                        ILayer terl1 = app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == (checkedListBox1.Items[i].ToString()))).FirstOrDefault();
                                        if (comboBox2.Text == "与原图层要素相交")
                                        {
                                            wo.SetText("正在处理......");
                                            SelectByGeometry(terl1, fe.ShapeCopy, esriSpatialRelEnum.esriSpatialRelIntersects, esriSelectionResultEnum.esriSelectionResultAdd);
                                        }
                                    }
                                }
                                fe = pCursor.NextFeature();
                            }
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine(ex.Message);
                            System.Diagnostics.Trace.WriteLine(ex.Source);
                            System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                            //删除临时要素类
                            if (temp_union_fc != null)
                            {
                                (temp_union_fc as IDataset).Delete();
                            }
                            if (temp_diss_fc != null)
                            {
                                (temp_diss_fc as IDataset).Delete();
                            }
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            //删除临时要素类
                            if (temp_union_fc != null)
                            {
                                (temp_union_fc as IDataset).Delete();
                            }
                            if (temp_diss_fc != null)
                            {
                                (temp_diss_fc as IDataset).Delete();
                            }
                        }
                    }
                    else
                    {
                        //IWorkspace workspace = app.Workspace.EsriWorkspace;
                        //IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
                        //IFeatureClass feac = featureWorkspace.OpenFeatureClass(comboBox1.Text);

                        ILayer terl = app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == (comboBox1.Text))).FirstOrDefault();
                        IFeatureLayer pFeatureLayer = terl as IFeatureLayer;
                        IFeatureClass feac = pFeatureLayer.FeatureClass;

                        IFeature pFeature;
                        IFeatureCursor feacusor = feac.Search(null, false);
                        int j2 = 0;
                        while ((pFeature = feacusor.NextFeature()) != null)
                        {
                            j2++;
                            wo.SetText("需处理总数：" + feac.FeatureCount(null).ToString() + "  已处理数：" + j2.ToString());

                            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                            {
                                if (checkedListBox1.GetItemChecked(i))
                                {
                                    ILayer terl1 = app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == (checkedListBox1.Items[i].ToString()))).FirstOrDefault();
                                    //if (comboBox2.Text == "与原图层要素相交")
                                    //{
                                    //    SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelIntersects, esriSelectionResultEnum.esriSelectionResultAdd);
                                    //}
                                    if (comboBox2.Text == "完全位于源图层要素范围内")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelContains, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                    if (comboBox2.Text == "包含源图层要素")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelWithin, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                    if (comboBox2.Text == "与源图层要素相邻")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelTouches, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                    if (comboBox2.Text == "与源图层要素重叠")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelOverlaps, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                }
                            }

                        } Marshal.ReleaseComObject(feacusor);
                    }
                }
            }
            this.Close();
            app.ActiveView.Refresh();
            MessageBox.Show("处理完成！");
        }

        /// <summary>
        /// 选择并返回相应图层中的要素，选择方法由SelectMethod设置
        /// </summary>
        /// <param name="pLayer">需要传递一个图层实例作为参数</param>
        /// <param name="pGeometry">需要传递点、线、面等geometry参数，
        /// 作为点选、线选和框选等不同的选择方式 </param>
        /// <returns></returns>
        public static void SelectByGeometry(ILayer pLayer, IGeometry pGeometry,esriSpatialRelEnum pEsriSpatialRelEnum ,esriSelectionResultEnum selectMethod)
        {
            try
            {
                ISpatialFilter pSpatialFilter = new SpatialFilterClass();
                pSpatialFilter.Geometry = pGeometry;
                pSpatialFilter.SpatialRel = pEsriSpatialRelEnum;
                pSpatialFilter.GeometryField = "Shape";


                //List<IFeature> featureList = new List<IFeature>();
                IFeatureLayer pFeatureLayer = pLayer as IFeatureLayer;

                IFeatureSelection pFeatureSelection = pFeatureLayer as IFeatureSelection;

                //pFeatureSelection.BufferDistance=10;

                pFeatureSelection.SelectFeatures(pSpatialFilter, selectMethod, false);
                //featureList = GetFeatureList(pFeatureLayer);
                //return featureList;
            }
            catch
            {
                throw new Exception("选择要素出现错误!");
            }
        }
        private void SelectByLocationForm_Load(object sender, EventArgs e)
        {
            
            //#region 读取对象的数据组织结构（要素集、要素类）
            
            //workspace = app.Workspace.EsriWorkspace;
            //featureWorkspace = (IFeatureWorkspace)workspace;
            //IEnumDataset enumDataset = workspace.get_Datasets(esriDatasetType.esriDTAny);
            //enumDataset.Reset();
            //IDataset dataset = enumDataset.Next();
            //while (dataset != null)
            //{
            //    if (dataset is IFeatureDataset)
            //    {
            //        IEnumDataset enumSubset = dataset.Subsets;
            //        enumSubset.Reset();
            //        IDataset subsetDataset = enumSubset.Next();
            //        while (subsetDataset != null)
            //        {
            //            if (subsetDataset is IFeatureClass)
            //            {
            //                checkedListBox1.Items.Add(subsetDataset.Name.Trim());
            //                laylis.Add(subsetDataset.Name.Trim());
            //                comboBox1.Items.Add(subsetDataset.Name.Trim());
            //            }
            //            subsetDataset = enumSubset.Next();
            //        }
            //    }
            //    if (dataset is IFeatureClass)
            //    {
            //        checkedListBox1.Items.Add(dataset.Name.Trim());
            //        laylis.Add(dataset.Name.Trim());
            //        comboBox1.Items.Add(dataset.Name.Trim());
            //    }
            //    dataset = enumDataset.Next();
            //}
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(enumDataset);
            //#endregion

            var map = app.ActiveView.FocusMap as IMap;
            IEnumLayer layers = map.get_Layers(null, false);
            layers.Reset();
            ILayer layer = layers.Next();
            //int selected = 1;


            while (layer != null)
            {
                if (layer is IGroupLayer)
                {
                    ICompositeLayer pGroupLayer = (ICompositeLayer)layer;
                    for (int i = 0; i < pGroupLayer.Count; i++)
                    {
                        ILayer SubLayer = pGroupLayer.get_Layer(i);
                        if (SubLayer is IFeatureLayer)
                        {
                            //if ((SubLayer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
                            //{
                                //if (SubLayer.Name.ToUpper().StartsWith("BOUA"))
                                //{
                                //    cmbLayers.Items.Add(SubLayer.Name);
                                //}
                            //}
                            //selected = cmbLayers.Items.Count;
                            checkedListBox1.Items.Add(SubLayer.Name.ToUpper());
                            laylis.Add(SubLayer.Name.ToUpper());
                            comboBox1.Items.Add(SubLayer.Name.ToUpper());
                        }
                    }
                }
                if (layer is IFeatureLayer)
                {

                    //if ((layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
                    //{
                    //    if (layer.Name.ToUpper().StartsWith("BOUA"))
                    //    {
                    //        cmbLayers.Items.Add(layer.Name);
                    //    }
                    //}
                    checkedListBox1.Items.Add(layer.Name.ToUpper());
                    laylis.Add(layer.Name.ToUpper());
                    comboBox1.Items.Add(layer.Name.ToUpper());
                }
                layer = layers.Next();
            }

            layerComboBox.SelectedIndex = 0;
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        private void cbOnlySelect_CheckedChanged(object sender, EventArgs e)
        {
            checkBox1.Enabled = true;
            checkBox1.Checked = false;
            checkedListBox1.Items.Clear();
            if (cbOnlySelect.Checked)
            {
                foreach (var item in laylis)
                {
                    ILayer terl = app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == (item))).FirstOrDefault();
                    IFeatureLayer pFeatureLayer = terl as IFeatureLayer;
                    if (pFeatureLayer.Selectable)
                    {
                        checkedListBox1.Items.Add(item);
                    }
                }
            }
            else 
            {
                foreach (var item in laylis)
                {
                    checkedListBox1.Items.Add(item);
                }
            }
            foreach (var item in laylis)
            {
                if (!comboBox1.Items.Contains(item))
                {
                    comboBox1.Items.Add(item);
                }
            }
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //checkBox1.Enabled = true;
            //checkBox1.Checked = false;
            string xItem = checkedListBox1.SelectedItem.ToString();
            if (checkedListBox1.GetItemChecked(checkedListBox1.SelectedIndex))
            {
                ILayer terl = app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == (xItem))).FirstOrDefault();
                IFeatureLayer pFatureLayer = terl as IFeatureLayer;
                IFeatureSelection pFeatureSelection = pFatureLayer as IFeatureSelection;
                ISelectionSet pSelectionSet = pFeatureSelection.SelectionSet;
                if (pSelectionSet.Count == 0)
                {
                    comboBox1.Items.Remove(xItem);
                }
                if (checkedListBox1.Items[checkedListBox1.SelectedIndex].ToString() == comboBox1.Text)
                {
                    checkBox1.Checked = true;
                    checkBox1.Enabled = false;
                }

            }
            else
            {
                if (!comboBox1.Items.Contains(xItem))
                {
                    comboBox1.Items.Add(xItem);
                }
                if (checkedListBox1.Items[checkedListBox1.SelectedIndex].ToString() == comboBox1.Text)
                {
                    checkBox1.Enabled = true;
                    checkBox1.Checked = false;
                }
            }

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            checkBox1.Enabled = true;
            checkBox1.Checked = false;
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i) && checkedListBox1.Items[i].ToString() == comboBox1.Text)
                {
                    checkBox1.Checked = true;
                    checkBox1.Enabled = false;
                }
            }
            ILayer terl = app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == (comboBox1.Text))).FirstOrDefault();
            IFeatureLayer pFatureLayer = terl as IFeatureLayer;
            IFeatureSelection pFeatureSelection = pFatureLayer as IFeatureSelection;
            ISelectionSet pSelectionSet = pFeatureSelection.SelectionSet;
            label6.Text = "(选择了 " + pSelectionSet.Count.ToString() + " 个要素)";
            if (pSelectionSet.Count == 0)
            {
                checkBox1.Enabled = false;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool pand = false;
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    pand = true;
                }
            }
            if (pand == false || comboBox1.Text == "")
            {
                MessageBox.Show("参数不符合选择条件！");
                return;
            }

            //IWorkspace workspace = app.Workspace.EsriWorkspace;
            //IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
            //IFeatureClass feac = featureWorkspace.OpenFeatureClass(comboBox1.Text);
            //ISpatialFilter sf = new SpatialFilter();
            //sf.Geometry = diffGeo;
            //sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            //IFeature pFeature;
            //IFeatureCursor feacusor = feac.Search(sf, false);
            //while ((pFeature = feacusor.NextFeature()) != null)


            if (checkBox1.Checked == true)
            {
                using (var wo = app.SetBusy())
                {
                    ILayer terl = app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == (comboBox1.Text))).FirstOrDefault();
                    IFeatureLayer pFeatureLayer = terl as IFeatureLayer;
                    IFeatureSelection pFeatureSelection = pFeatureLayer as IFeatureSelection;
                    ISelectionSet pSelectionSet = pFeatureSelection.SelectionSet;
                    IFeature pFeature;
                    int j = 0;
                    //List<IFeature> featureList = new List<IFeature>();
                    if (pSelectionSet.Count > 0)
                    {
                        IEnumIDs IDs = null;
                        IDs = pSelectionSet.IDs;
                        int ID = IDs.Next();
                        while (ID > 0)
                        {
                            pFeature = pFeatureLayer.FeatureClass.GetFeature(ID);
                            j++;
                            wo.SetText("需处理总数：" + pSelectionSet.Count.ToString() + "  已处理数：" + j.ToString());
                            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                            {
                                if (checkedListBox1.GetItemChecked(i))
                                {
                                    ILayer terl1 = app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == (checkedListBox1.Items[i].ToString()))).FirstOrDefault();
                                    if (comboBox2.Text == "与原图层要素相交")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelIntersects, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                    if (comboBox2.Text == "完全位于源图层要素范围内")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelContains, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                    if (comboBox2.Text == "包含源图层要素")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelWithin, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                    if (comboBox2.Text == "与源图层要素相邻")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelTouches, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                    if (comboBox2.Text == "与源图层要素重叠")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelOverlaps, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                }
                            }

                            //featureList.Add(pFeature);
                            ID = IDs.Next();
                        }
                    }
                }
            }
            else
            {
                using (var wo = app.SetBusy())
                {
                    if (comboBox2.Text == "与原图层要素相交")
                    {
                        //创建临时数据库
                        string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
                        IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);


                        ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = new Geoprocessor();
                        gp.OverwriteOutput = true;
                        gp.SetEnvironmentValue("workspace", ws.PathName);

                        IGeoProcessorResult gpResult = null;

                        IFeatureClass temp_union_fc = null; //所有相关面合并后的临时要素类（未融合）
                        IFeatureClass temp_diss_fc = null; //Dissolve后得到的临时结果要素类

                        try
                        {
                            if (wo != null)
                                wo.SetText("正在构建临时要素类......");
                            //IWorkspace workspace = app.Workspace.EsriWorkspace;
                            //IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
                            //IFeatureClass feach = featureWorkspace.OpenFeatureClass(comboBox1.Text);

                            ILayer terl = app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == (comboBox1.Text))).FirstOrDefault();
                            IFeatureLayer pFeatureLayer = terl as IFeatureLayer;
                            IFeatureClass feach = pFeatureLayer.FeatureClass;

                            #region 构建临时要素类
                            //创建临时要素类
                            temp_union_fc = DCDHelper.CreateFeatureClassStructToWorkspace(ws as IFeatureWorkspace, feach, feach.AliasName + "_temp");
                            //复制相关要素
                            //DCDHelper.CopyFeaturesToFeatureClass(targetFC, qf, temp_union_fc, false);
                            //foreach (var referFC in referFCList)
                            //{
                            DCDHelper.CopyFeaturesToFeatureClass(feach, null, temp_union_fc, false);
                            //}
                            #endregion

                            if (wo != null)
                                wo.SetText("正在修复几何......");
                            #region 修复几何
                            RepairGeometry reGeo = new RepairGeometry();
                            reGeo.in_features = temp_union_fc.AliasName;
                            gpResult = (IGeoProcessorResult)gp.Execute(reGeo, null);
                            #endregion

                            #region 分区
                            if (temp_union_fc.FeatureCount(null) > 10000)
                            {
                                CreateCartographicPartitions gPpartition = new CreateCartographicPartitions();
                                gPpartition.in_features = temp_union_fc.AliasName;
                                gPpartition.out_features = "Partitions";
                                gPpartition.feature_count = 5000;
                                gpResult = (IGeoProcessorResult)gp.Execute(gPpartition, null);

                                gp.SetEnvironmentValue("cartographicPartitions", ws.PathName + "\\Partitions");
                            }
                            #endregion

                            if (wo != null)
                                wo.SetText("正在融合要素......");

                            #region 融合
                            Dissolve diss = new Dissolve();
                            diss.in_features = temp_union_fc.AliasName;
                            diss.out_feature_class = temp_union_fc.AliasName + "_diss";
                            diss.multi_part = "SINGLE_PART";
                            gpResult = (IGeoProcessorResult)gp.Execute(diss, null);
                            temp_diss_fc = (ws as IFeatureWorkspace).OpenFeatureClass(temp_union_fc.AliasName + "_diss");
                            #endregion


                            //if (wo != null)
                            //    wo.SetText("正在检查是否为封闭空白区......");

                            //#region 提取微小孔洞到targetFC中
                            //IFeatureClassLoad pFCLoad = targetFC as IFeatureClassLoad;
                            //pFCLoad.LoadOnlyMode = true;
                            //IFeature pFeature = targetFC.CreateFeature();
                            //int gbindex = pFeature.Fields.FindField("GB");

                            //查找是否有孔洞包含点击选取点，创建要素，并将GB置为-1
                            IFeatureCursor pCursor = temp_diss_fc.Search(null, false);
                            IFeature fe = pCursor.NextFeature();
                            //int count = temp_diss_fc.FeatureCount(null);
                            while (fe != null)
                            {
                                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                                {
                                    if (checkedListBox1.GetItemChecked(i))
                                    {
                                        ILayer terl1 = app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == (checkedListBox1.Items[i].ToString()))).FirstOrDefault();
                                        if (comboBox2.Text == "与原图层要素相交")
                                        {
                                            wo.SetText("正在处理......");
                                            SelectByGeometry(terl1, fe.ShapeCopy, esriSpatialRelEnum.esriSpatialRelIntersects, esriSelectionResultEnum.esriSelectionResultAdd);
                                        }
                                    }
                                }
                                fe = pCursor.NextFeature();
                            }
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine(ex.Message);
                            System.Diagnostics.Trace.WriteLine(ex.Source);
                            System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                            //删除临时要素类
                            if (temp_union_fc != null)
                            {
                                (temp_union_fc as IDataset).Delete();
                            }
                            if (temp_diss_fc != null)
                            {
                                (temp_diss_fc as IDataset).Delete();
                            }
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            //删除临时要素类
                            if (temp_union_fc != null)
                            {
                                (temp_union_fc as IDataset).Delete();
                            }
                            if (temp_diss_fc != null)
                            {
                                (temp_diss_fc as IDataset).Delete();
                            }
                        }
                    }
                    else
                    {
                        //IWorkspace workspace = app.Workspace.EsriWorkspace;
                        //IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
                        //IFeatureClass feac = featureWorkspace.OpenFeatureClass(comboBox1.Text);

                        ILayer terl = app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == (comboBox1.Text))).FirstOrDefault();
                        IFeatureLayer pFeatureLayer = terl as IFeatureLayer;
                        IFeatureClass feac = pFeatureLayer.FeatureClass;

                        IFeature pFeature;
                        IFeatureCursor feacusor = feac.Search(null, false);
                        int j2 = 0;
                        while ((pFeature = feacusor.NextFeature()) != null)
                        {
                            j2++;
                            wo.SetText("需处理总数：" + feac.FeatureCount(null).ToString() + "  已处理数：" + j2.ToString());

                            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                            {
                                if (checkedListBox1.GetItemChecked(i))
                                {
                                    ILayer terl1 = app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == (checkedListBox1.Items[i].ToString()))).FirstOrDefault();
                                    //if (comboBox2.Text == "与原图层要素相交")
                                    //{
                                    //    SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelIntersects, esriSelectionResultEnum.esriSelectionResultAdd);
                                    //}
                                    if (comboBox2.Text == "完全位于源图层要素范围内")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelContains, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                    if (comboBox2.Text == "包含源图层要素")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelWithin, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                    if (comboBox2.Text == "与源图层要素相邻")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelTouches, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                    if (comboBox2.Text == "与源图层要素重叠")
                                    {
                                        SelectByGeometry(terl1, pFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelOverlaps, esriSelectionResultEnum.esriSelectionResultAdd);
                                    }
                                }
                            }

                        } Marshal.ReleaseComObject(feacusor);
                    }
                }
            }

            app.ActiveView.Refresh();
            MessageBox.Show("处理完成！");
        }
    }
}
