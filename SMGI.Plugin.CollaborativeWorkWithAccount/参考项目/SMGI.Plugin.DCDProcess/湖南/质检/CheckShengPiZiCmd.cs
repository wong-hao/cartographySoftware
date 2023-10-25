using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using System.IO;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.DCDProcess
{
    public class CheckShengPiZiCmd : SMGI.Common.SMGICommand
    {
        public static List<ErrPtSPZ> ErrPtSPZList = new List<ErrPtSPZ>();
        public static ISpatialReference srf;
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;
            }
        }
        public override void OnClick()
        {
            string lyrName = "AGNP";

            #region 获取图层和字段信息
            var feLayer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
                        ((l as IGeoFeatureLayer).Name.Trim().ToUpper() == lyrName)).FirstOrDefault() as IFeatureLayer;
            if (feLayer == null)
            {
                MessageBox.Show(string.Format("当前工作空间中没有找到图层【{0}】!", lyrName));
                return;
            }

            var agnpFC = feLayer.FeatureClass;

            ISpatialReference ptSRF = (agnpFC as IGeoDataset).SpatialReference;
            if (srf == null)
            {
                srf = ptSRF;
            }

            string nameFN = m_Application.TemplateManager.getFieldAliasName("Name", agnpFC.AliasName);
            int nameIndex = agnpFC.FindField(nameFN);
            if (nameIndex == -1)
            {
                MessageBox.Show(string.Format("图层【{0}】中没有找到字段【{1}】!", feLayer.Name, nameFN));
                return;
            }

            string pinyinFN = m_Application.TemplateManager.getFieldAliasName("PinYin", agnpFC.AliasName);
            int pinyinIndex = agnpFC.FindField(pinyinFN);
            if (pinyinIndex == -1)
            {
                MessageBox.Show(string.Format("图层【{0}】中没有找到字段【{1}】!", feLayer.Name, pinyinFN));
                return;
            }
            
            #endregion

            ErrPtSPZList.Clear();

            try
            {
                #region 开始检查
                using (var wo = m_Application.SetBusy())
                {
                    IQueryFilter ptQF = new QueryFilterClass();
                    string ptSQL = String.Format("{0} is not null and {0}<>'' ", nameFN);

                    ptQF.WhereClause = agnpFC.HasCollabField() ? ptSQL + " and " + cmdUpdateRecord.CurFeatureFilter : ptSQL;
                    IFeatureCursor ptFeatureCursor = agnpFC.Search(ptQF, false);
                    IFeature ptCheckFeature = null;
                    while ((ptCheckFeature = ptFeatureCursor.NextFeature()) != null)
                    {
                        string name = ptCheckFeature.get_Value(nameIndex).ToString();
                        string pinyin = ptCheckFeature.get_Value(pinyinIndex).ToString();
                        char spz = PinYinConvert.GetSPZ(name);
                        wo.SetText("正在处理要素【" + ptCheckFeature.OID + "】.......");
                        if (spz != ' ')
                        {
                            IPoint pt = ptCheckFeature.ShapeCopy as IPoint;
                            ErrPtSPZ spzPt = new ErrPtSPZ()
                            {
                                PtLayerName = lyrName,
                                PtID = ptCheckFeature.OID.ToString(),
                                attName = name,
                                attPinyin = pinyin,
                                Info = String.Format("有生僻字/特殊字符:{0}", spz),
                                Pt = pt
                            };
                            ErrPtSPZList.Add(spzPt);
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(string.Format("检查失败:{0}", ex.Message));
            }

            
            if (ErrPtSPZList.Count > 0)
            {
                #region 保存到shp
                var shpWriter = new ShapeFileWriter();
                var shpdir = OutputSetup.GetDir() + "\\errShp";
                if (!Directory.Exists(shpdir))
                    Directory.CreateDirectory(shpdir);

                var fieldDic = new Dictionary<string, int>() { { "图层", 10 }, { "点ID", 10 }, { "NAME", 40 }, { "Pinyin", 80 }, { "说明", 60 } };

                string shpFile = shpdir + "\\生僻字检查";
                shpWriter.createErrorResutSHPFile(shpFile, srf, esriGeometryType.esriGeometryPoint, fieldDic, true);

                foreach (ErrPtSPZ errPtSPZ in ErrPtSPZList)
                {
                    IPoint pt = errPtSPZ.Pt;
                    String PtLayerName = errPtSPZ.PtLayerName;
                    shpWriter.addErrorGeometry(pt, new Dictionary<string, string>()
                    {
                        { "图层", errPtSPZ.PtLayerName }, 
                        { "点ID", errPtSPZ.PtID }, 
                        { "NAME", errPtSPZ.attName },
                        { "PinYin", errPtSPZ.attPinyin },
                        { "说明", errPtSPZ.Info }
                    });
                }

                shpWriter.saveErrorResutSHPFile();
                #endregion

                #region 提示加载
                if (MessageBox.Show("检查完成！是否加载检查结果数据到地图？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var strs = shpFile;
                    CheckHelper.AddTempLayerFromSHPFile(m_Application.Workspace.LayerManager.Map, shpFile);                    
                }
                #endregion
            }
            
        }
    }

    public class ErrPtSPZ
    {
        public string PtLayerName;
        public string PtID;
        public string attName;
        public string attPinyin;
        public string Info;
        public IPoint Pt;

    }
}
