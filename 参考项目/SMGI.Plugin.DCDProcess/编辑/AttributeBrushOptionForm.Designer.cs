namespace SMGI.Plugin.DCDProcess
{
    partial class AttributeBrushOptionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem("");
            this.cbSelectAllFN = new System.Windows.Forms.CheckBox();
            this.lvExlucdeFN = new System.Windows.Forms.ListView();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lvDefaultFN = new System.Windows.Forms.ListView();
            this.btnAddExlucdeFN = new System.Windows.Forms.Button();
            this.btnDelExlucdeFN = new System.Windows.Forms.Button();
            this.btnDelDefaultFN = new System.Windows.Forms.Button();
            this.btnAddDefaultFN = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbSelectAllFN
            // 
            this.cbSelectAllFN.AutoSize = true;
            this.cbSelectAllFN.Location = new System.Drawing.Point(12, 467);
            this.cbSelectAllFN.Name = "cbSelectAllFN";
            this.cbSelectAllFN.Size = new System.Drawing.Size(216, 16);
            this.cbSelectAllFN.TabIndex = 0;
            this.cbSelectAllFN.Text = "默认勾选所有字段（例外字段除外）";
            this.cbSelectAllFN.UseVisualStyleBackColor = true;
            // 
            // lvExlucdeFN
            // 
            this.lvExlucdeFN.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lvExlucdeFN.FullRowSelect = true;
            this.lvExlucdeFN.GridLines = true;
            this.lvExlucdeFN.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4});
            this.lvExlucdeFN.Location = new System.Drawing.Point(13, 23);
            this.lvExlucdeFN.Name = "lvExlucdeFN";
            this.lvExlucdeFN.Size = new System.Drawing.Size(205, 198);
            this.lvExlucdeFN.TabIndex = 106;
            this.lvExlucdeFN.UseCompatibleStateImageBehavior = false;
            this.lvExlucdeFN.View = System.Windows.Forms.View.List;
            this.lvExlucdeFN.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvExlucdeFN_ItemSelectionChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 12);
            this.label2.TabIndex = 105;
            this.label2.Text = "例外字段（不选）";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 238);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 105;
            this.label1.Text = "例外字段（选择）";
            // 
            // lvDefaultFN
            // 
            this.lvDefaultFN.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
            this.lvDefaultFN.FullRowSelect = true;
            this.lvDefaultFN.GridLines = true;
            this.lvDefaultFN.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem5,
            listViewItem6,
            listViewItem7,
            listViewItem8});
            this.lvDefaultFN.Location = new System.Drawing.Point(14, 253);
            this.lvDefaultFN.Name = "lvDefaultFN";
            this.lvDefaultFN.Size = new System.Drawing.Size(205, 198);
            this.lvDefaultFN.TabIndex = 106;
            this.lvDefaultFN.UseCompatibleStateImageBehavior = false;
            this.lvDefaultFN.View = System.Windows.Forms.View.List;
            this.lvDefaultFN.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvDefaultFN_ItemSelectionChanged);
            // 
            // btnAddExlucdeFN
            // 
            this.btnAddExlucdeFN.Location = new System.Drawing.Point(223, 23);
            this.btnAddExlucdeFN.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddExlucdeFN.Name = "btnAddExlucdeFN";
            this.btnAddExlucdeFN.Size = new System.Drawing.Size(43, 22);
            this.btnAddExlucdeFN.TabIndex = 108;
            this.btnAddExlucdeFN.Text = "增加";
            this.btnAddExlucdeFN.UseVisualStyleBackColor = true;
            this.btnAddExlucdeFN.Click += new System.EventHandler(this.btnAddExlucdeFN_Click);
            // 
            // btnDelExlucdeFN
            // 
            this.btnDelExlucdeFN.Enabled = false;
            this.btnDelExlucdeFN.Location = new System.Drawing.Point(225, 49);
            this.btnDelExlucdeFN.Margin = new System.Windows.Forms.Padding(2);
            this.btnDelExlucdeFN.Name = "btnDelExlucdeFN";
            this.btnDelExlucdeFN.Size = new System.Drawing.Size(41, 22);
            this.btnDelExlucdeFN.TabIndex = 107;
            this.btnDelExlucdeFN.Text = "移除";
            this.btnDelExlucdeFN.UseVisualStyleBackColor = true;
            this.btnDelExlucdeFN.Click += new System.EventHandler(this.btnDelExlucdeFN_Click);
            // 
            // btnDelDefaultFN
            // 
            this.btnDelDefaultFN.Enabled = false;
            this.btnDelDefaultFN.Location = new System.Drawing.Point(225, 280);
            this.btnDelDefaultFN.Margin = new System.Windows.Forms.Padding(2);
            this.btnDelDefaultFN.Name = "btnDelDefaultFN";
            this.btnDelDefaultFN.Size = new System.Drawing.Size(41, 22);
            this.btnDelDefaultFN.TabIndex = 107;
            this.btnDelDefaultFN.Text = "移除";
            this.btnDelDefaultFN.UseVisualStyleBackColor = true;
            this.btnDelDefaultFN.Click += new System.EventHandler(this.btnDelDefaultFN_Click);
            // 
            // btnAddDefaultFN
            // 
            this.btnAddDefaultFN.Location = new System.Drawing.Point(223, 254);
            this.btnAddDefaultFN.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddDefaultFN.Name = "btnAddDefaultFN";
            this.btnAddDefaultFN.Size = new System.Drawing.Size(43, 22);
            this.btnAddDefaultFN.TabIndex = 108;
            this.btnAddDefaultFN.Text = "增加";
            this.btnAddDefaultFN.UseVisualStyleBackColor = true;
            this.btnAddDefaultFN.Click += new System.EventHandler(this.btnAddDefaultFN_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 500);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(278, 31);
            this.panel1.TabIndex = 109;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(137, 4);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(64, 23);
            this.btOK.TabIndex = 7;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(201, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(210, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "字段名";
            this.columnHeader1.Width = 205;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "字段名";
            this.columnHeader2.Width = 205;
            // 
            // AttributeBrushOptionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(278, 531);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnAddDefaultFN);
            this.Controls.Add(this.btnAddExlucdeFN);
            this.Controls.Add(this.btnDelDefaultFN);
            this.Controls.Add(this.btnDelExlucdeFN);
            this.Controls.Add(this.lvDefaultFN);
            this.Controls.Add(this.lvExlucdeFN);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbSelectAllFN);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AttributeBrushOptionForm";
            this.Text = "属性刷设置";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbSelectAllFN;
        private System.Windows.Forms.ListView lvExlucdeFN;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView lvDefaultFN;
        private System.Windows.Forms.Button btnAddExlucdeFN;
        private System.Windows.Forms.Button btnDelExlucdeFN;
        private System.Windows.Forms.Button btnDelDefaultFN;
        private System.Windows.Forms.Button btnAddDefaultFN;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
    }
}