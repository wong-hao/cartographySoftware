using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using System.Collections.Generic;
using System.Windows.Forms;
namespace SMGI.Plugin.DCDProcess.DataProcess
{
    /// <summary>
    /// 检查字段存在的空格
    /// </summary>
    public class FieldBackspaceCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            var frm = new FrmBackspace(m_Application);
            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                bool res = DoProcess(frm.ProcFeatureClass, frm.fieldNameList);

                if (res)
                {
                    MessageBox.Show("处理完毕！");
                }
            }

        }

        /// <summary>
        /// 执行处理
        /// </summary>
        /// <param name="fcls">要素类</param>
        /// <param name="fieldNames">字段集合（都字符类型字段）</param>
        /// <returns></returns>
        public bool DoProcess(IFeatureClass fcls, List<string> fieldNames)
        {
            var qfilter = new QueryFilterClass();
            var str = "";
            var fziList = new List<int>();
            for (var i = 0; i < fieldNames.Count; i++)
            {
                if (i > 0) str += " or ";
                var fname = fieldNames[i];
                str += fname + " LIKE '%　%' or " + fname + " like '% %' ";
                var idx = fcls.Fields.FindField(fieldNames[i]);
                if (-1 != idx)
                    fziList.Add(idx);
            }

            qfilter.WhereClause = str;
            var pCursor = fcls.Search(qfilter, false);
            IFeature pFea = null;

            while (null != (pFea = pCursor.NextFeature()))
            {
                foreach (var i in fziList)
                {
                    pFea.Value[i] = pFea.Value[i].ToString().Replace(" ", "").Replace("　", "");

                }
                pFea.Store();
            }


            return true;
        }
    }
}
