using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System.IO;

namespace SMGI.Plugin.DCDProcess
{
    public class AddCalcFieldHebeiCmd : SMGI.Common.SMGICommand
    {
        public AddCalcFieldHebeiCmd()
        {
            m_caption = "数据库合并";
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
            AddCalcFieldForm frm = new AddCalcFieldForm(m_Application);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            DataTable ruleDataTable = new DataTable();
            #region 1--读取赋值配置表 ruleDataTable
            string dbPath = m_Application.Template.Root + @"\规则配置.mdb";
            string tableName = frm.ruleTableName; //分类代码 开头的表格
            try 
            {                
                ruleDataTable = DCDHelper.ReadToDataTable(dbPath, tableName);
                if (ruleDataTable == null)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                string msg = String.Format("读取表：{0}失败", tableName);
                MessageBox.Show(ex.Message+"\n"+msg);
                return;
            }
            #endregion

            #region 2--获取待添加{图层-字段名列表}关系 fcNameFields
            IDictionary<string, IList<string>> fcNameFields = new Dictionary<string, IList<string>>();
            for (int i = 0; i < ruleDataTable.Rows.Count; i++)
            {
                string lyrName = (ruleDataTable.Rows[i]["原数据图层"]).ToString();
                string fldName = (ruleDataTable.Rows[i]["分类码字段名"]).ToString();
                if (fcNameFields.ContainsKey(lyrName))
                {
                    if (fcNameFields[lyrName].Contains(fldName))
                        continue;
                    else
                    {
                        fcNameFields[lyrName].Add(fldName);
                    }
                }
                else
                {
                    IList<string> fldNames = new List<string>() { fldName };
                    fcNameFields.Add(lyrName, fldNames);
                }
            }
            #endregion


            #region 3--依次遍历处理各个数据库
            int errcount = 0;
            using (var wo = m_Application.SetBusy())
            {
                wo.SetText("开始处理......");

                string dir = OutputSetup.GetDir();
                string txtFile = dir + "\\分类代码赋值处理记录.txt";
                StreamWriter sw = new StreamWriter(txtFile);
                
                int count = 0;                
                foreach (var sourceFileName in frm.SourceDBFileNameList)
                {
                    count++;
                    string txt = String.Format("{0}-处理：{1}\r\n", count, sourceFileName);
                    wo.SetText(txt);                    
                    sw.Write(txt);
                    sw.Flush();
                    int n1=DealGDB(sourceFileName, ruleDataTable,sw);
                    if (n1 > 0)
                    {
                        errcount += n1;
                    }
                }                
                sw.Flush();
                sw.Close();
                wo.SetText("处理完成！");
            }
            if (errcount > 0)
            {
                MessageBox.Show(String.Format("操作完成——有{0}处错误，请查看日志！",errcount));
            }
            else
            {
                MessageBox.Show("操作完成——全部正确！");
            }
         
            #endregion
        }

        //函数——处理单个数据库---DealGDB
        public int DealGDB(string sourceFileName, DataTable dt,StreamWriter swr)
        {
            int n = 0;
            IWorkspaceFactory srcWF = null;
            if (sourceFileName.ToLower().EndsWith(".gdb"))
            {
                srcWF = new FileGDBWorkspaceFactoryClass();
            }
            else if (sourceFileName.ToLower().EndsWith(".mdb"))
            {
                srcWF = new AccessWorkspaceFactoryClass();
            }

            if (srcWF == null)
                return -1;

            #region 1--获取要素类列表fcs
            IList<IFeatureClass> fcs = new List<IFeatureClass>();
            IWorkspace sourceWorkspace = srcWF.OpenFromFile(sourceFileName, 0);
            IEnumDataset sourceEnumDataset = sourceWorkspace.get_Datasets(esriDatasetType.esriDTAny);
            sourceEnumDataset.Reset();
            IDataset sourceDataset = null;
            while ((sourceDataset = sourceEnumDataset.Next()) != null)
            {
                if (sourceDataset is IFeatureDataset)//要素数据集
                {
                    //遍历子要素类
                    IFeatureDataset sourceFeatureDataset = sourceDataset as IFeatureDataset;
                    IEnumDataset subSourceEnumDataset = sourceFeatureDataset.Subsets;
                    subSourceEnumDataset.Reset();
                    IDataset subSourceDataset = null;
                    while ((subSourceDataset = subSourceEnumDataset.Next()) != null)
                    {
                        if (subSourceDataset is IFeatureClass)//要素类
                        {
                            IFeatureClass fc = subSourceDataset as IFeatureClass;
                            fcs.Add(fc);                            
                        }
                    }
                    Marshal.ReleaseComObject(subSourceEnumDataset);
                }
                else if (sourceDataset is IFeatureClass)//要素类
                {
                    IFeatureClass fc2 = sourceDataset as IFeatureClass;
                    fcs.Add(fc2);                   
                }
            }
            Marshal.ReleaseComObject(sourceEnumDataset);           
            #endregion 

            #region 2--为要素类添加字段并计算
            foreach (IFeatureClass fc in fcs)
            {
                string name = (fc as FeatureDataset).Name;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string lyrName = (dt.Rows[i]["原数据图层"]).ToString();
                    string subfcName = (dt.Rows[i]["要素名称"]).ToString();
                    string sql = (dt.Rows[i]["定义查询"]).ToString();
                    string fldName = (dt.Rows[i]["分类码字段名"]).ToString();
                    string fldValue = (dt.Rows[i]["分类码属性值"]).ToString();

                    if (name == lyrName)
                    {
                        string info = FCCalcValue(fc, sql, fldName, fldValue);
                        if (info != "成功")
                        {
                            swr.Write(String.Format("\t{0}\r\n", info));
                            swr.Flush();
                            n++;
                        }
                        else
                        {
                            string txt = String.Format("处理成功 原数据图层：{0}，要素名称：{1}", name, subfcName);
                            swr.Write("\t"+txt + "\r\n");
                            swr.Flush(); 
                        }
                    }
                }               
            }
            #endregion

            return n;
        }


        //为要素类添加字段——FCAddField  
        //如果已存在该字段，          返回字段序号idx
        //如果不存在该字段，添加成功后返回字段序号idx
        //如果不存在该字段，添加失败后返回-2
        public int FCAddField(IFeatureClass fc, string fldName,esriFieldType fldType, int length=255)
        {
            int fieldIndex = fc.FindField(fldName);
            if (fieldIndex == -1)
            {
                IFieldEdit field = new FieldClass();
                field.Name_2 = fldName;
                field.Type_2 = fldType;
                
                if (fldType == esriFieldType.esriFieldTypeString)
                { field.Length_2 = length; }
                
                field.Editable_2 = true;
                field.DefaultValue_2 = "NULL";
                try
                {
                    fc.AddField(field);
                }
                catch
                {
                    //MessageBox.Show("添加字段失败："+fc.AliasName+"  "+ fldName);
                    return -2;
                }
                fieldIndex = fc.FindField(fldName);
            }
            return fieldIndex;            
        }

        //函数--按赋值表对要素类赋值——FCCalcValue
        //GB——LONG
        //CLASS——TEXT3
        //LGB——LONG
        //PAC——TEXT6
        //成功，则返回"成功"，否则返回错误信息
        public string FCCalcValue(IFeatureClass fc,string sql,string fldName,string fldValue)
        {
            string name = (fc as IDataset).Name; 
            int idx =-1;
            if (fldName.ToUpper() == "GB" || fldName.ToUpper() == "LGB") //整型
            {
                //添加字段（如果没有）         
                idx = FCAddField(fc, fldName, esriFieldType.esriFieldTypeInteger);
                if (idx == -2)
                {
                    return String.Format("处理失败：为{0} 添加字段{1} ", name, fldName);                    
                }                
                try
                {
                    //转整型有可能失败
                    var v2 = Int32.Parse(fldValue);
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = sql;
                    IFeatureCursor feCursor = fc.Search(qf, true);
                    IFeature fe = null;

                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        fe.set_Value(idx, v2);
                        fe.Store();
                    }
                }
                catch
                {
                    return String.Format("处理失败：为{0}  {1}  赋值{2}", name, fldName, fldValue);                     
                }                
            }
            else if (fldName.ToUpper() == "CLASS")
            {
                idx = FCAddField(fc, fldName, esriFieldType.esriFieldTypeString, 3);
                if (idx == -2)
                {
                    return String.Format("处理失败：为{0} 添加字段{1}", name, fldName);                   
                }
                var v2 = fldValue;
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = sql;
                IFeatureCursor feCursor = fc.Search(qf, true);
                IFeature fe = null;
                while ((fe = feCursor.NextFeature()) != null)
                {
                    fe.set_Value(idx, fldValue);
                    fe.Store();
                }
            }
            else if (fldName.ToUpper() == "PAC")
            {
                idx = FCAddField(fc, fldName, esriFieldType.esriFieldTypeString, 6);
                if (idx == -2)
                {
                    return String.Format("处理失败：为{0} 添加字段{1}", name, fldName);                   
                }
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = sql;
                IFeatureCursor feCursor = fc.Search(qf, true);
                IFeature fe = null;
                while ((fe = feCursor.NextFeature()) != null)
                {
                    fe.set_Value(idx, fldValue);
                    fe.Store();
                }
            }
            return "成功";
        }
    }
}
