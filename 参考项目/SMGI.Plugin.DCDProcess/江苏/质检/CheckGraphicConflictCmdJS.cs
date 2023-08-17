using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using System.Data;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using System.Runtime.InteropServices;
using System.IO;
using SMGI.Plugin.GeneralEdit;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 探测线要素间的制图（线宽）冲突，返回有冲突的地方
    /// 详见需求文档《【20181225回复】追加需求（质检工具等）_董先敏.docx》
    /// </summary>
    public class CheckGraphicConflictCmdJS : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null;
            }
        }

        public override void OnClick()
        {
            if (m_Application.MapControl.Map.ReferenceScale == 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;
            }

            CheckGraphicConflictFormJS frm = new CheckGraphicConflictFormJS();
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            //读取配置表，获取需检查的内容
            Dictionary<KeyValuePair<string, string>, double> feType2Width = new Dictionary<KeyValuePair<string, string>, double>();//Dictionary<KeyValuePair<要素类名称,过滤条件>, 线宽>
            string mdbPath = m_Application.Template.Root + "\\质检\\质检内容配置.mdb";
            if (!System.IO.File.Exists(mdbPath))
            {
                MessageBox.Show(string.Format("未找到配置文件:{0}!", mdbPath));
                return;
            }
            string tabaleName = "要素制图冲突检查_";
            if (m_Application.MapControl.Map.ReferenceScale == 1000000)
            {
                tabaleName += "100W";
            }
            else if (m_Application.MapControl.Map.ReferenceScale == 500000)
            {
                tabaleName += "50W";
            }
            else if (m_Application.MapControl.Map.ReferenceScale == 250000)
            {
                tabaleName += "25W";
            }
            else if (m_Application.MapControl.Map.ReferenceScale == 100000)
            {
                tabaleName += "10W";
            }
            else if (m_Application.MapControl.Map.ReferenceScale == 50000)
            {
                tabaleName += "5W";
            }
            else
            {
                MessageBox.Show(string.Format("未找到当前设置的参考比例尺【{0}】对应的检查规则表!", m_Application.MapControl.Map.ReferenceScale));
                return;
            }
            DataTable dataTable = DCDHelper.ReadMDBTable(mdbPath, tabaleName);
            if (dataTable == null)
            {
                MessageBox.Show(string.Format("配置文件【{0}】中未找到表【{1}】！", mdbPath, tabaleName));
                return;
            }
            if (dataTable.Rows.Count == 0)
            {
                MessageBox.Show(string.Format("规则表中的规则为空，没有有效的检查规则！"));
                return;
            }

            foreach (DataRow dr in dataTable.Rows)
            {
                string fcName = dr["FCName"].ToString();
                string filterString = dr["FilterString"].ToString();
                double lineWidth = 0;
                double.TryParse(dr["LineWidth"].ToString(), out lineWidth);

                KeyValuePair<string, string> kv = new KeyValuePair<string, string>(fcName, filterString);
                feType2Width.Add(kv, lineWidth);
            }

            string outputFileName = OutputSetup.GetDir() + string.Format("\\图形冲突检查_{0}.shp", m_Application.MapControl.Map.ReferenceScale);


            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                err = DoCheck(outputFileName, feType2Width, m_Application.MapControl.Map.ReferenceScale, frm.GraphicMinDistance, wo);
            }

            if (err == "")
            {
                if (File.Exists(outputFileName))
                {
                    IFeatureClass errFC = CheckHelper.OpenSHPFile(outputFileName);

                    if (MessageBox.Show("是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        CheckHelper.AddTempLayerToMap(m_Application.Workspace.LayerManager.Map, errFC);
                }
                else
                {
                    MessageBox.Show("检查完毕,未发现图形冲突！");
                }
            }
            else
            {
                MessageBox.Show(err);
            }
        }

        public static string DoCheck(string resultSHPFileName, Dictionary<KeyValuePair<string, string>, double> feType2Width, double mapScale, double graphicMinDistance = 0.28, WaitOperation wo = null)
        {
            string err = "";

            string fullPath = DCDHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
            IWorkspace ws = DCDHelper.createTempWorkspace(fullPath);
            IFeatureWorkspace fws = ws as IFeatureWorkspace;
            IFeatureClass temp_fc = null;
            int orgFCNameIndex = -1;
            int orgfidIndx = -1;
            IFeatureClassLoad fcload = null ;
            try
            {
                List<CheckResultInfo> checkResultList = new List<CheckResultInfo>();
                ISpatialReference sr = null;

                #region 添加需参与检查的要素到临时要素类中
                Dictionary<string, Dictionary<int, double>> fcName2FIDBuffer = new Dictionary<string, Dictionary<int, double>>();//Dictionary<要素类名, Dictionary<要素编号, 缓冲距离>>
                Dictionary<string, IFeatureClass> fcName2FeatureClass = new Dictionary<string, IFeatureClass>();
                int dissolvepolgonFID = -1;//融合后的面要素FID
                foreach (var kv in feType2Width)
                {
                    string fcName = kv.Key.Key;
                    string filter = kv.Key.Value;
                    double lineWidth = kv.Value;

                    IFeatureClass fc = null;
                    #region 创建临时要素类，获取本次参与要素类信息
                    if (fcName2FeatureClass.ContainsKey(fcName))
                    {
                        fc = fcName2FeatureClass[fcName];
                    }
                    else
                    {
                        var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                        {
                            return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name == fcName);
                        })).ToArray();
                        if (lyrs.Count() == 0)
                            continue;
                        IGeoFeatureLayer geoFeLayer = lyrs.First() as IGeoFeatureLayer;
                        if (geoFeLayer == null)
                            continue;
                        fc = geoFeLayer.FeatureClass;

                        fcName2FeatureClass.Add(fcName, fc);
                        if (sr == null)
                        {
                            sr = (fc as IGeoDataset).SpatialReference;

                            //创建一个临时面要素类
                            if (wo != null)
                                wo.SetText(string.Format("正在创建临时要素类......"));

                            temp_fc = CreateFeatureClass(fws, "temp_GraphicConflict", sr, esriGeometryType.esriGeometryPolygon, ref orgFCNameIndex, ref orgfidIndx);
                            fcload = temp_fc as IFeatureClassLoad;
                            fcload.LoadOnlyMode = true;
                        }
                    }
                    #endregion

                    #region 要素缓冲，插入到临时要素类中
                    IFeatureCursor newFeCursor = temp_fc.Insert(true);

                    IQueryFilter qf = new QueryFilterClass();
                    if (fc.HasCollabField())
                    {
                        qf.WhereClause = string.Format("({0}) and ", filter) + cmdUpdateRecord.CurFeatureFilter;
                    }
                    else
                    {
                        qf.WhereClause = filter;
                    }

                    double bufferDistance = (lineWidth + graphicMinDistance) * 0.5 * 0.001 * mapScale;

                    if (fc.ShapeType == esriGeometryType.esriGeometryPolyline)
                    {
                        #region 线要素类
                        IFeatureCursor feCursor = fc.Search(qf, true);
                        IFeature fe = null;
                        while ((fe = feCursor.NextFeature()) != null)
                        {
                            if (wo != null)
                                wo.SetText(string.Format("正在从要素类【{0}】中添加要素【{1}】......", fcName, fe.OID));

                            if (fe.Shape == null || (fe.Shape as IPolyline).Length < bufferDistance)
                                continue;//微小要素，忽略

                            IFeatureBuffer newFeBuffer = temp_fc.CreateFeatureBuffer();

                            //收集该缓冲面的相关信息
                            if (fcName2FIDBuffer.ContainsKey(fcName))
                            {
                                if (!fcName2FIDBuffer[fcName].ContainsKey(fe.OID))
                                {
                                    fcName2FIDBuffer[fcName].Add(fe.OID, bufferDistance * 2.0);
                                }
                                else
                                {
                                    continue;//同一要素满足N个条件（被符号化为多个符号，原则上不应该出现，若出现，则以第一次符号的情形符号为准）
                                }

                            }
                            else
                            {
                                Dictionary<int, double> fid2Buffer = new Dictionary<int, double>();
                                fid2Buffer.Add(fe.OID, bufferDistance * 2.0);

                                fcName2FIDBuffer.Add(fcName, fid2Buffer);
                            }

                            IGeometry bufferShape = null;
                            #region 创建缓冲面
                            IBufferConstruction bfConstrut = new BufferConstructionClass();

                            //1.圆头缓冲
                            //bufferShape = bfConstrut.Buffer(fe.Shape, bufferDistance);

                            //2.平头缓冲
                            (bfConstrut as IBufferConstructionProperties).EndOption = esriBufferConstructionEndEnum.esriBufferFlat;
                            IEnumGeometry enumGeo = new GeometryBagClass();
                            (enumGeo as IGeometryCollection).AddGeometry(fe.Shape);
                            IGeometryCollection outputBuffer = new GeometryBagClass();
                            bfConstrut.ConstructBuffers(enumGeo, bufferDistance, outputBuffer);
                            for (int i = 0; i < outputBuffer.GeometryCount; i++)
                            {
                                IGeometry geo = outputBuffer.get_Geometry(i);
                                if (bufferShape == null)
                                {
                                    bufferShape = geo;
                                }
                                else
                                {
                                    bufferShape = (bufferShape as ITopologicalOperator).Union(geo);
                                }
                            }
                            #endregion



                            //几何赋值
                            newFeBuffer.Shape = bufferShape;
                            //属性赋值
                            newFeBuffer.set_Value(orgFCNameIndex, fc.AliasName);
                            newFeBuffer.set_Value(orgfidIndx, fe.OID);

                            newFeCursor.InsertFeature(newFeBuffer);
                        }
                        newFeCursor.Flush();

                        Marshal.ReleaseComObject(feCursor);
                        Marshal.ReleaseComObject(newFeCursor);
                        #endregion
                    }
                    else if (fc.ShapeType == esriGeometryType.esriGeometryPolygon)
                    {
                        IFeatureClass temp_plgFC = null;
                        try
                        {
                            #region 临时数据库，将符合条件的面进行融合处理(排除共线的面要素之间的误报)
                            Geoprocessor gp = new Geoprocessor();
                            gp.OverwriteOutput = true;
                            gp.SetEnvironmentValue("workspace", ws.PathName);

                            ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer makeFeatureLayer = new ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer();
                            makeFeatureLayer.in_features = fc;
                            makeFeatureLayer.out_layer = fc.AliasName + "_Layer";
                            SMGI.Common.Helper.ExecuteGPTool(gp, makeFeatureLayer, null);

                            ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute selectLayerByAttribute = new ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute();
                            selectLayerByAttribute.in_layer_or_view = fc.AliasName + "_Layer";
                            selectLayerByAttribute.where_clause = qf.WhereClause;
                            SMGI.Common.Helper.ExecuteGPTool(gp, selectLayerByAttribute, null);

                            Dissolve diss = new Dissolve();
                            diss.in_features = fc.AliasName + "_Layer";
                            diss.out_feature_class = fc.AliasName + "_Layer_diss";
                            diss.multi_part = "SINGLE_PART";
                            SMGI.Common.Helper.ExecuteGPTool(gp, diss, null);
                            temp_plgFC = (ws as IFeatureWorkspace).OpenFeatureClass(fc.AliasName + "_Layer_diss");//融合后的临时要素类
                            #endregion

                            #region 临时面要素类
                            IFeatureCursor feCursor = temp_plgFC.Search(null, true);
                            IFeature fe = null;
                            dissolvepolgonFID--;
                            while ((fe = feCursor.NextFeature()) != null)
                            {
                                if (wo != null)
                                    wo.SetText(string.Format("正在从要素类【{0}】中添加要素......", fcName));

                                if (fe.Shape == null || (fe.Shape as IPolygon).Length < bufferDistance)
                                    continue;//微小要素，忽略

                                IFeatureBuffer newFeBuffer = temp_fc.CreateFeatureBuffer();

                                //收集该缓冲面的相关信息
                                if (fcName2FIDBuffer.ContainsKey(fcName))
                                {
                                    if (!fcName2FIDBuffer[fcName].ContainsKey(dissolvepolgonFID))
                                    {
                                        fcName2FIDBuffer[fcName].Add(dissolvepolgonFID, bufferDistance * 2.0);
                                    }
                                    

                                }
                                else
                                {
                                    Dictionary<int, double> fid2Buffer = new Dictionary<int, double>();
                                    fid2Buffer.Add(dissolvepolgonFID, bufferDistance * 2.0);

                                    fcName2FIDBuffer.Add(fcName, fid2Buffer);
                                }

                                IGeometry bufferShape = null;
                                #region 创建缓冲面
                                IBufferConstruction bfConstrut = new BufferConstructionClass();

                                //1.圆头缓冲
                                //bufferShape = bfConstrut.Buffer(fe.Shape, bufferDistance);

                                //2.平头缓冲
                                (bfConstrut as IBufferConstructionProperties).EndOption = esriBufferConstructionEndEnum.esriBufferFlat;
                                IEnumGeometry enumGeo = new GeometryBagClass();
                                (enumGeo as IGeometryCollection).AddGeometry((fe.Shape as ITopologicalOperator).Boundary as IPolyline);//面边界
                                IGeometryCollection outputBuffer = new GeometryBagClass();
                                bfConstrut.ConstructBuffers(enumGeo, bufferDistance, outputBuffer);
                                for (int i = 0; i < outputBuffer.GeometryCount; i++)
                                {
                                    IGeometry geo = outputBuffer.get_Geometry(i);
                                    if (bufferShape == null)
                                    {
                                        bufferShape = geo;
                                    }
                                    else
                                    {
                                        bufferShape = (bufferShape as ITopologicalOperator).Union(geo);
                                    }
                                }
                                #endregion

                                //几何赋值
                                newFeBuffer.Shape = bufferShape;
                                //属性赋值
                                newFeBuffer.set_Value(orgFCNameIndex, fc.AliasName);
                                newFeBuffer.set_Value(orgfidIndx, dissolvepolgonFID);

                                newFeCursor.InsertFeature(newFeBuffer);
                            }
                            newFeCursor.Flush();

                            Marshal.ReleaseComObject(feCursor);
                            Marshal.ReleaseComObject(newFeCursor);
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            if (temp_plgFC != null)
                            {
                                (temp_plgFC as IDataset).Delete();
                            }
                        }
                    }
                    #endregion
                }

                if (fcload != null)
                {
                    fcload.LoadOnlyMode = false;
                    fcload = null;
                }
                #endregion

                #region 拓扑分析
                if (wo != null)
                    wo.SetText(string.Format("正在进行拓扑分析......"));

                Dictionary<IPolygon, KeyValuePair<int, int>> errInfo = CheckHelper.AreaNoOverlap(temp_fc, null);
                foreach (var item in errInfo)
                {
                    IFeature tempFe1 = temp_fc.GetFeature(item.Value.Key);
                    IFeature tempFe2 = temp_fc.GetFeature(item.Value.Value);

                    CheckResultInfo cri = new CheckResultInfo();
                    cri.Shape = item.Key;
                    cri.FCName1 = tempFe1.get_Value(orgFCNameIndex).ToString();
                    cri.FID1 = int.Parse(tempFe1.get_Value(orgfidIndx).ToString());
                    cri.FCName2 = tempFe2.get_Value(orgFCNameIndex).ToString();
                    cri.FID2 = int.Parse(tempFe2.get_Value(orgfidIndx).ToString());

                    checkResultList.Add(cri);

                    Marshal.ReleaseComObject(tempFe1);
                    Marshal.ReleaseComObject(tempFe2);

                }
                #endregion


                if (checkResultList.Count > 0)
                {
                    if (wo != null)
                        wo.SetText("正在输出检查结果......");

                    ShapeFileWriter resultFile = null;
                    foreach (var item in checkResultList)
                    {
                        //结果筛选
                        if (true)
                        {
                            #region 方法1:提取多边形中心线，根据中心线的长度剔除可能的误报项(速度很慢)
                            //double maxBufferVal = fcName2FIDBuffer[item.FCName1][item.FID1] > fcName2FIDBuffer[item.FCName2][item.FID2] ? fcName2FIDBuffer[item.FCName1][item.FID1] : fcName2FIDBuffer[item.FCName2][item.FID2];
                            //double shapeLen = getPolygonCenterLineLen(item.Shape);
                            //if (shapeLen <= maxBufferVal)//多边形的长度小于最大缓冲距离，则忽略该冲突
                            //    continue;
                            #endregion

                            #region 方法2:根据缓冲面几何的周长剔除可能的误报项
                            double bufferLen = fcName2FIDBuffer[item.FCName1][item.FID1] * 2 + fcName2FIDBuffer[item.FCName2][item.FID2] * 2;
                            bufferLen = bufferLen * 1.5;//考虑到面几何可能存在小褶皱
                            if(item.Shape.Length < bufferLen)//短小的压盖面，则忽略该冲突
                                continue;

                            #endregion
                        }


                        if (resultFile == null)
                        {
                            //新建结果文件
                            resultFile = new ShapeFileWriter();
                            Dictionary<string, int> fieldName2Len = new Dictionary<string, int>();
                            fieldName2Len.Add("图层名1", 16);
                            fieldName2Len.Add("要素编号1", 16);
                            fieldName2Len.Add("图层名2", 16);
                            fieldName2Len.Add("要素编号2", 16);
                            fieldName2Len.Add("检查项", 32);

                            resultFile.createErrorResutSHPFile(resultSHPFileName, sr, esriGeometryType.esriGeometryPolygon, fieldName2Len);
                        }

                        Dictionary<string, string> fieldName2FieldValue = new Dictionary<string, string>();
                        fieldName2FieldValue.Add("图层名1", item.FCName1);
                        if(item.FID1 > 0)//非临时融合面要素
                            fieldName2FieldValue.Add("要素编号1", item.FID1.ToString());
                        fieldName2FieldValue.Add("图层名2", item.FCName2);
                        if (item.FID2 > 0)//非临时融合面要素
                            fieldName2FieldValue.Add("要素编号2", item.FID2.ToString());
                        fieldName2FieldValue.Add("检查项", "图形冲突检查");

                        resultFile.addErrorGeometry(item.Shape, fieldName2FieldValue);
                    }

                    //保存结果文件
                    if(resultFile != null)
                        resultFile.saveErrorResutSHPFile();
                }


            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                err = ex.Message;
            }
            finally
            {
                if (fcload != null)
                {
                    fcload.LoadOnlyMode = false;
                }

                if (temp_fc != null)
                {
                    (temp_fc as IDataset).Delete();
                }
            }

            return err;
        }

        /// <summary>
        /// 创建临时要素类
        /// </summary>
        /// <param name="fws"></param>
        /// <param name="fcName"></param>
        /// <param name="sr"></param>
        /// <param name="geoType"></param>
        /// <param name="orgFCNameIndex"></param>
        /// <param name="orgfidIndx"></param>
        /// <returns></returns>
        public static IFeatureClass CreateFeatureClass(IFeatureWorkspace fws, string fcName, ISpatialReference sr, esriGeometryType geoType, ref int orgFCNameIndex, ref int orgfidIndx)
        {
            if ((fws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fcName))
            {
                IFeatureClass fc = fws.OpenFeatureClass(fcName);
                (fc as IDataset).Delete();
            }

            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
            IFieldsEdit target_fields = ocDescription.RequiredFields as IFieldsEdit;

            //新增几何所属原要素类名称字段
            IField newField = new FieldClass();
            IFieldEdit newFieldEdit = (IFieldEdit)newField;
            newFieldEdit.Name_2 = "org_fcname";
            newFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            newFieldEdit.Length_2 = 16;
            target_fields.AddField(newField);
            //新增几何所属原要素的OID
            newField = new FieldClass();
            newFieldEdit = (IFieldEdit)newField;
            newFieldEdit.Name_2 = "org_fid";
            newFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            target_fields.AddField(newField);


            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = (IWorkspace)fws;
            fieldChecker.Validate(target_fields, out enumFieldError, out validatedFields);

            int shapeFieldIndex = target_fields.FindField(fcDescription.ShapeFieldName);
            IField Shapefield = target_fields.get_Field(shapeFieldIndex);
            IGeometryDef geometryDef = Shapefield.GeometryDef;
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = geoType;
            geometryDefEdit.SpatialReference_2 = sr;

            IFeatureClass newFC = fws.CreateFeatureClass(fcName, target_fields, ocDescription.InstanceCLSID,
                ocDescription.ClassExtensionCLSID, esriFeatureType.esriFTSimple, fcDescription.ShapeFieldName, "");

            orgFCNameIndex = newFC.FindField("org_fcname");
            orgfidIndx = newFC.FindField("org_fid");

            return newFC;

        }

        /// <summary>
        /// 获取几何面的最长中轴线长度
        /// </summary>
        /// <param name="plgShape"></param>
        /// <returns></returns>
        public static double getPolygonCenterLineLen(IPolygon plgShape)
        {
            double len = 0;
            if (plgShape == null || plgShape.IsEmpty)
                return len;

            var clh = new CenterLineHelper();
            var cl = clh.Create(plgShape).Line;
            var gc = (IGeometryCollection)cl;
            for (var i = 0; i < gc.GeometryCount; i++)
            {
                var pl = new PolylineClass();
                pl.AddGeometry(gc.Geometry[i]);

                len += pl.Length;
            }

            return len;
        }

        public class CheckResultInfo
        {
            public string FCName1 { get; set; }
            public int FID1 { get; set; }

            public string FCName2 { get; set; }
            public int FID2 { get; set; }

            public IPolygon Shape { get; set; }

        }
    }
}
