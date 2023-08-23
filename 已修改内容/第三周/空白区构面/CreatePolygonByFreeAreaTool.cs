using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.DataSourcesGDB;
using SMGI.Common;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 多尺度-空白区构面（新增要素带状态）
    /// </summary>
    public  class CreatePolygonByFreeAreaTool:SMGITool
    {
        public CreatePolygonByFreeAreaTool()
        {
            m_caption = "空白区构面";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1) return;

            //获取选择图层，并查询点击位置是否存在面要素
            var toc = m_Application.TOCSelectItem;
            if (!(toc.Layer is IGeoFeatureLayer) || (toc.Layer as IGeoFeatureLayer).FeatureClass.ShapeType != esriGeometryType.esriGeometryPolygon) return;
            ISpatialFilter sf = new SpatialFilterClass { Geometry = ToSnapedMapPoint(x, y), SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects };
            var gfl = (IGeoFeatureLayer)toc.Layer;
            if (gfl.FeatureClass.FeatureCount(sf) > 0) return;

            //获取屏幕内线面要素
            var ext = ((IActiveView)m_Application.Workspace.Map).Extent;
            var gls = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && l.Visible && ((l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline || (l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon));
            
            //合并所有图层范围
            foreach (var gl in gls)
            {
                ext.Union(((IGeoDataset)gl).Extent);
            }

            //获取所有要素集合
            IGeometryCollection gc = new GeometryBagClass();
            foreach (var gl in gls)
            {
                var qs = ((IIdentify2)gl).Identify(ext, null);
                if (qs == null) continue;
                for (var i = 0; i < qs.Count; i++)
                {
                    var fo = qs.Element[i] as IFeatureIdentifyObj;
                    var rowIdentifyObject = (IRowIdentifyObject)fo;
                    if (rowIdentifyObject == null) continue;
                    var fe = (IFeature)rowIdentifyObject.Row;
                    gc.AddGeometry(fe.Shape);
                }
            }
            if (gc.GeometryCount == 0) return;

            //生成内存图层并构建面
            var ged = (IGeoDataset)gfl;
            var srt = (ISpatialReferenceTolerance)ged.SpatialReference;
            var mfc = CreatePolygonMemoryLayer(ged.SpatialReference);
            ISelectionSet sel;
            IInvalidArea are = new InvalidAreaClass();
            IFeatureConstruction feco = new FeatureConstructionClass();
            feco.AutoCompleteFromGeometries(mfc, ged.Extent, gc as IEnumGeometry, are, srt.XYTolerance, ((IDataset)gfl).Workspace, out sel);
            var fcu = mfc.Search(sf, false);
            var nfe = fcu.NextFeature();
            if (nfe == null) return;

            //将面写入图层
            m_Application.EngineEditor.StartOperation();
            var fci = gfl.FeatureClass.Insert(true);
            var fb = gfl.FeatureClass.CreateFeatureBuffer();
            fb.Shape = nfe.Shape;

            // if (gfl.FeatureClass.HasCollabField())
            {
                int idxGUID = -1;
                int idxVERS = -1;
                int idxUSER = -1;
                idxGUID = gfl.FeatureClass.FindField(ServerDataInitializeCommand.CollabGUID);
                idxVERS = gfl.FeatureClass.FindField(ServerDataInitializeCommand.CollabVERSION);
                idxUSER = gfl.FeatureClass.FindField(ServerDataInitializeCommand.CollabOPUSER);
                if (idxGUID > -1 && idxVERS > -1 && idxUSER > -1)
                {
                    fb.set_Value(idxVERS, cmdUpdateRecord.NewState);
                    fb.set_Value(idxUSER, cmdUpdateRecord.UserName);
                    fb.set_Value(idxGUID, Guid.NewGuid().ToString());
                }
            }

            if (cmdUpdateRecord.EnableChangeFeatureForm)//是否弹弹窗
            {
                OnChangeFeatureForm frm = new OnChangeFeatureForm(m_Application);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    string source = frm.source;
                    string remark = frm.remark;
                    fb.set_Value(fb.Fields.FindField("source"), source);
                    fb.set_Value(fb.Fields.FindField("remark"), remark);
                }
            }

            fci.InsertFeature(fb);
            fci.Flush();
            m_Application.EngineEditor.StopOperation("空白区构面");
            m_Application.ActiveView.Refresh();
        }

        //生成内存图层
        public IFeatureClass CreatePolygonMemoryLayer(ISpatialReference sp)
        {
            //设置字段集
            IFields fields = new FieldsClass();
            var fieldsEdit = (IFieldsEdit)fields;
            IField field = new FieldClass();
            var fieldEdit = (IFieldEdit)field;

            //创建主键
            fieldEdit.Name_2 = "FID";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            fieldsEdit.AddField(field);

            //创建图形字段
            IGeometryDef geometryDef = new GeometryDefClass();
            var geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
            geometryDefEdit.SpatialReference_2 = sp;

            field = new FieldClass();
            fieldEdit = (IFieldEdit)field;
            fieldEdit.Name_2 = "Shape";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            fieldEdit.GeometryDef_2 = geometryDef;
            fieldsEdit.AddField(field);

            IWorkspaceFactory wf = new InMemoryWorkspaceFactoryClass();
            var wn = wf.Create("", "MemoryWorkspace", null, 0);
            var na = (IName)wn;
            var fw = (IFeatureWorkspace)(na.Open()); //打开内存空间
            var featureClass = fw.CreateFeatureClass("MW", fields, null, null, esriFeatureType.esriFTSimple, "Shape", "");
            return featureClass;
        }
    }
}
