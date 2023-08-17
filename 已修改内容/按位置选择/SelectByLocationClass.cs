using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    /// <summary>
    /// 使用要素在另一图层中的位置来选择要素
    /// </summary>
    public class SelectByLocationClass : SMGI.Common.SMGICommand
    {
        public SelectByLocationClass()
        {
            m_caption = "按位置选择";
            m_toolTip = "按位置选择";
            m_category = "数据";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;
            }
        }
        public override void OnClick() 
        {
            SelectByLocationForm mPolygonOverloapPolygonForm0 = new SelectByLocationForm(m_Application);
            mPolygonOverloapPolygonForm0.Show();
        }
    }
}
