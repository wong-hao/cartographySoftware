namespace SMGI.Plugin.DCDProcess
{
    partial class DBFormatConversionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DBFormatConversionForm));
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem("");
            this.panel1 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.btnOutPutPath = new System.Windows.Forms.Button();
            this.tbOutputPath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnDel = new System.Windows.Forms.Button();
            this.btnSourceDB = new System.Windows.Forms.Button();
            this.lvDataBase = new System.Windows.Forms.ListView();
            this.CBDataBaseConvType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 303);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(542, 31);
            this.panel1.TabIndex = 92;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(401, 4);
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
            this.panel2.Location = new System.Drawing.Point(465, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(474, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // btnOutPutPath
            // 
            this.btnOutPutPath.Image = ((System.Drawing.Image)(resources.GetObject("btnOutPutPath.Image")));
            this.btnOutPutPath.Location = new System.Drawing.Point(506, 261);
            this.btnOutPutPath.Margin = new System.Windows.Forms.Padding(2);
            this.btnOutPutPath.Name = "btnOutPutPath";
            this.btnOutPutPath.Size = new System.Drawing.Size(26, 22);
            this.btnOutPutPath.TabIndex = 113;
            this.btnOutPutPath.UseVisualStyleBackColor = true;
            this.btnOutPutPath.Click += new System.EventHandler(this.btnOutPutPath_Click);
            // 
            // tbOutputPath
            // 
            this.tbOutputPath.Location = new System.Drawing.Point(12, 261);
            this.tbOutputPath.Name = "tbOutputPath";
            this.tbOutputPath.ReadOnly = true;
            this.tbOutputPath.Size = new System.Drawing.Size(489, 21);
            this.tbOutputPath.TabIndex = 112;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 246);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 111;
            this.label3.Text = "输出文件夹";
            // 
            // btnDel
            // 
            this.btnDel.Enabled = false;
            this.btnDel.Image = ((System.Drawing.Image)(resources.GetObject("btnDel.Image")));
            this.btnDel.Location = new System.Drawing.Point(506, 98);
            this.btnDel.Margin = new System.Windows.Forms.Padding(2);
            this.btnDel.Name = "btnDel";
            this.btnDel.Size = new System.Drawing.Size(26, 22);
            this.btnDel.TabIndex = 110;
            this.btnDel.UseVisualStyleBackColor = true;
            this.btnDel.Click += new System.EventHandler(this.btnDel_Click);
            // 
            // btnSourceDB
            // 
            this.btnSourceDB.Image = ((System.Drawing.Image)(resources.GetObject("btnSourceDB.Image")));
            this.btnSourceDB.Location = new System.Drawing.Point(506, 68);
            this.btnSourceDB.Margin = new System.Windows.Forms.Padding(2);
            this.btnSourceDB.Name = "btnSourceDB";
            this.btnSourceDB.Size = new System.Drawing.Size(26, 22);
            this.btnSourceDB.TabIndex = 109;
            this.btnSourceDB.UseVisualStyleBackColor = true;
            this.btnSourceDB.Click += new System.EventHandler(this.btnSourceDB_Click);
            // 
            // lvDataBase
            // 
            this.lvDataBase.FullRowSelect = true;
            this.lvDataBase.GridLines = true;
            this.lvDataBase.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvDataBase.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem5,
            listViewItem6,
            listViewItem7,
            listViewItem8});
            this.lvDataBase.Location = new System.Drawing.Point(12, 68);
            this.lvDataBase.Name = "lvDataBase";
            this.lvDataBase.Size = new System.Drawing.Size(489, 165);
            this.lvDataBase.TabIndex = 108;
            this.lvDataBase.UseCompatibleStateImageBehavior = false;
            this.lvDataBase.View = System.Windows.Forms.View.Details;
            // 
            // CBDataBaseConvType
            // 
            this.CBDataBaseConvType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBDataBaseConvType.FormattingEnabled = true;
            this.CBDataBaseConvType.Items.AddRange(new object[] {
            "MDB转GDB",
            "GDB转MDB"});
            this.CBDataBaseConvType.Location = new System.Drawing.Point(12, 21);
            this.CBDataBaseConvType.Name = "CBDataBaseConvType";
            this.CBDataBaseConvType.Size = new System.Drawing.Size(520, 20);
            this.CBDataBaseConvType.TabIndex = 107;
            this.CBDataBaseConvType.SelectedIndexChanged += new System.EventHandler(this.CBDataBaseConvType_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 12);
            this.label2.TabIndex = 105;
            this.label2.Text = "待转换地理数据库列表";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 106;
            this.label1.Text = "转换类型";
            // 
            // DBFormatConversionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 334);
            this.Controls.Add(this.btnOutPutPath);
            this.Controls.Add(this.tbOutputPath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnDel);
            this.Controls.Add(this.btnSourceDB);
            this.Controls.Add(this.lvDataBase);
            this.Controls.Add(this.CBDataBaseConvType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DBFormatConversionForm";
            this.Text = "数据库格式转换";
            this.Load += new System.EventHandler(this.DBFormatConversionForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btnOutPutPath;
        private System.Windows.Forms.TextBox tbOutputPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnDel;
        private System.Windows.Forms.Button btnSourceDB;
        private System.Windows.Forms.ListView lvDataBase;
        private System.Windows.Forms.ComboBox CBDataBaseConvType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}