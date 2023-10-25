using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 地名拼音赋值：对拼音字段为空的地名点进行赋值
    /// 处理AB-AJ加空格分节  BA-BB去空格不分节
    /// </summary>
    public class AGNPPinYinCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
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

            string classFN = m_Application.TemplateManager.getFieldAliasName("CLASS", agnpFC.AliasName);
            int classIndex = agnpFC.FindField(classFN);
            if (classIndex == -1)
            {
                MessageBox.Show(string.Format("图层【{0}】中没有找到字段【{1}】!", feLayer.Name, classFN));
                return;
            }
            #endregion

            try
            {
                m_Application.EngineEditor.StartOperation();

                using (var wo = m_Application.SetBusy())
                {
                    #region 为空的pinyin字段赋值
                    {
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = string.Format("({0} is null or {0} = '')", pinyinFN);
                        if (agnpFC.HasCollabField())
                            qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;
                        IFeatureCursor feCursor = agnpFC.Search(qf, false);
                        IFeature fe = null;

                        while ((fe = feCursor.NextFeature()) != null)
                        {
                            wo.SetText(string.Format("正在为图层【{0}】的要素【{1}】赋值......", lyrName, fe.OID));

                            string name = fe.get_Value(nameIndex).ToString();
                            string py = PinYinConvert.ToPinYin(name.Trim(), "生");

                            if (py == "")
                                continue;

                            //首字母大写  
                            string py2 = PinYinConvert.PinyinHeadUpper(py);                            
                            fe.set_Value(pinyinIndex, py2);
                            fe.Store();
                            
                        }
                        Marshal.ReleaseComObject(feCursor);
                    }
                    #endregion

                    #region AB-AJ尾部分节
                    {
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = string.Format("({0} <= 'AJ')", classFN);
                        if (agnpFC.HasCollabField())
                            qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;
                        IFeatureCursor feCursor = agnpFC.Search(qf, false);
                        IFeature fe = null;

                        while ((fe = feCursor.NextFeature()) != null)
                        {
                            string cls = fe.get_Value(classIndex).ToString();
                            string py = fe.get_Value(pinyinIndex).ToString();
                            string name = fe.get_Value(nameIndex).ToString();
                            wo.SetText(string.Format("正在为图层【{0}】CLASS【{1}】的要素【{2}】处理空格......", lyrName, cls, fe.OID));

                            string py2 = "";
                            if (cls == "AB")
                            {
                                if (name.EndsWith("省"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "省"); }
                                else if (name.EndsWith("自治区"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "自治区"); }
                                else if (name.EndsWith("市"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "市"); }
                                //else { throw (new MyException("后缀错误" + lyrName + " " + fe.OID)); }
                            }
                            else if (cls == "AC")
                            {

                                if (name.EndsWith("自治州"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "自治州"); }
                                else if (name.EndsWith("盟"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "盟"); }
                                else if (name.EndsWith("地区"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "地区"); }
                                //else { throw (new MyException("后缀错误" + lyrName + " " + fe.OID)); }
                            }
                            else if (cls == "AD")
                            {
                                if (name.EndsWith("市"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "市"); }
                                //else { throw (new MyException("后缀错误" + lyrName + " " + fe.OID)); }
                            }
                            else if (cls == "AE")
                            {
                                if (name.EndsWith("市"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "市"); }
                                //else { throw (new MyException("后缀错误" + lyrName + " " + fe.OID)); }
                            }
                            else if (cls == "AF")
                            {
                                if (name.EndsWith("自治县"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "自治县"); }
                                else if (name.EndsWith("县"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "县"); }
                                else if (name.EndsWith("自治旗"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "自治旗"); }
                                else if (name.EndsWith("旗"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "旗"); }
                                else if (name.EndsWith("高新区"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "高新区"); }
                                else if (name.EndsWith("开发区"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "开发区"); }
                                else if (name.EndsWith("区"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "区"); }
                                //else { throw (new MyException("后缀错误" + lyrName + " " + fe.OID)); }
                            }
                            else if (cls == "AH")
                            {
                                if (name.EndsWith("街道"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "街道"); }
                                //else { throw (new MyException("后缀错误" + lyrName + " " + fe.OID)); }
                            }
                            else if (cls == "AI")
                            {
                                if (name.EndsWith("镇"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "镇"); }
                                //else { throw (new MyException("后缀错误" + lyrName + " " + fe.OID)); }
                            }
                            else if (cls == "AJ")
                            {
                                if (name.EndsWith("民族乡"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "民族乡"); }
                                if (name.EndsWith("乡"))
                                { py2 = PinYinConvert.PinyinAddSpace(py, "乡"); }
                                //else { throw (new MyException("后缀错误" + lyrName + " " + fe.OID)); }
                            }
                            if (py2 != "" && py != py2)
                            {
                                fe.set_Value(pinyinIndex, py2);
                                fe.Store();
                            }
                        }

                        Marshal.ReleaseComObject(feCursor);
                    }
                    #endregion

                    #region BA-BB去空格
                    {
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = string.Format("({0} = 'BA' or {0} = 'BB') and {1} LIKE '% %'", classFN, nameFN);
                        if (agnpFC.HasCollabField())
                            qf.WhereClause += " and " + cmdUpdateRecord.CurFeatureFilter;
                        IFeatureCursor feCursor = agnpFC.Search(qf, false);
                        IFeature fe = null;

                        while ((fe = feCursor.NextFeature()) != null)
                        {
                            string cls = fe.get_Value(classIndex).ToString();
                            string py = fe.get_Value(pinyinIndex).ToString();

                            wo.SetText(string.Format("正在为图层【{0}】CLASS【{1}】的要素【{2}】处理空格......", lyrName, cls, fe.OID));

                            string py2 = PinYinConvert.PinyinRemoveSpace(py);
                            if (py != py2)
                            {
                                fe.set_Value(pinyinIndex, py2);
                                fe.Store();
                            }
                        }
                        Marshal.ReleaseComObject(feCursor);
                    }
                    #endregion                    
                }

                m_Application.EngineEditor.StopOperation("地名拼音赋值");
                MessageBox.Show("PINYIN赋值完成");
            }
            catch (Exception ex)
            {
                m_Application.EngineEditor.AbortOperation();

                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(String.Format("PINYIN赋值失败:{0}",ex.Message));
            }
        }
    }
    class MyException : Exception
    {
        public MyException(string msg) : base(msg) { }
    }
}
