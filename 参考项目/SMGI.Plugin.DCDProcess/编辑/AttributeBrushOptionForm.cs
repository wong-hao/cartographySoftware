using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Plugin.DCDProcess
{
    public partial class AttributeBrushOptionForm : Form
    {
        public bool SelAllFN
        {
            get
            {
                return cbSelectAllFN.Checked;
            }
        }

        public List<string> ExlucdeFNList
        {
            get
            {
                List<string> exlucdeFNList = new List<string>();

                foreach (ListViewItem lvi in lvExlucdeFN.Items)  
                {
                    exlucdeFNList.Add(lvi.Text);
                }

                return exlucdeFNList;
            }
        }

        public List<string> DefalutFNList
        {
            get
            {
                List<string> defalutFNList = new List<string>();

                foreach (ListViewItem lvi in lvDefaultFN.Items)
                {
                    defalutFNList.Add(lvi.Text);
                }

                return defalutFNList;
            }
        }

        public AttributeBrushOptionForm(bool bSelAllFN, List<string> exlucdeFNList, List<string> defalutSelFNList)
        {
            InitializeComponent();

            lvExlucdeFN.Columns.Add("");
            lvExlucdeFN.Scrollable = true;
            lvExlucdeFN.Items.Clear();

            lvDefaultFN.Columns.Add("");
            lvDefaultFN.Scrollable = true;
            lvDefaultFN.Items.Clear();


            cbSelectAllFN.Checked = bSelAllFN;
            
            foreach (var item in exlucdeFNList)
            {
                lvExlucdeFN.BeginUpdate();

                lvExlucdeFN.Items.Add(new ListViewItem(item, 0));

                lvExlucdeFN.EndUpdate();
            }

            foreach (var item in defalutSelFNList)
            {
                lvDefaultFN.BeginUpdate();

                lvDefaultFN.Items.Add(new ListViewItem(item, 0));

                lvDefaultFN.EndUpdate();
            }
        }

        private void lvExlucdeFN_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            btnDelExlucdeFN.Enabled = lvExlucdeFN.SelectedItems.Count > 0;
        }

        private void btnAddExlucdeFN_Click(object sender, EventArgs e)
        {
            AddFNForm frm = new AddFNForm();
            frm.StartPosition = FormStartPosition.CenterScreen;
            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lvExlucdeFN.BeginUpdate();

                lvExlucdeFN.Items.Add(new ListViewItem(frm.FN, 0));

                lvExlucdeFN.EndUpdate();
            }
        }

        private void btnDelExlucdeFN_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lvExlucdeFN.SelectedItems)  //选中项遍历  
            {
                lvExlucdeFN.Items.RemoveAt(lvi.Index); // 按索引移除  
            }
        }

        private void lvDefaultFN_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            btnDelDefaultFN.Enabled = lvDefaultFN.SelectedItems.Count > 0;
        }

        private void btnAddDefaultFN_Click(object sender, EventArgs e)
        {
            AddFNForm frm = new AddFNForm();
            frm.StartPosition = FormStartPosition.Manual;
            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lvDefaultFN.BeginUpdate();

                lvDefaultFN.Items.Add(new ListViewItem(frm.FN, 0));

                lvDefaultFN.EndUpdate();
            }
        }

        private void btnDelDefaultFN_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lvDefaultFN.SelectedItems)  //选中项遍历  
            {
                lvDefaultFN.Items.RemoveAt(lvi.Index); // 按索引移除  
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
