namespace SMGI.Plugin.DCDProcess
{
    partial class DatabaseMergeForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnDel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSourceDB = new System.Windows.Forms.Button();
            this.lvDataBase = new System.Windows.Forms.ListView();
            this.label4 = new System.Windows.Forms.Label();
            this.tbDBName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbReferGDBTemplate = new System.Windows.Forms.TextBox();
            this.btnReferGDBTemplate = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 390);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(504, 31);
            this.panel1.TabIndex = 105;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(363, 4);
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
            this.panel2.Location = new System.Drawing.Point(427, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(436, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // btnOutPutPath
            // 
            this.btnOutPutPath.Location = new System.Drawing.Point(459, 301);
            this.btnOutPutPath.Margin = new System.Windows.Forms.Padding(2);
            this.btnOutPutPath.Name = "btnOutPutPath";
            this.btnOutPutPath.Size = new System.Drawing.Size(41, 22);
            this.btnOutPutPath.TabIndex = 108;
            this.btnOutPutPath.Text = "浏览";
            this.btnOutPutPath.UseVisualStyleBackColor = true;
            this.btnOutPutPath.Click += new System.EventHandler(this.btnOutPutPath_Click);
            // 
            // tbOutputPath
            // 
            this.tbOutputPath.Location = new System.Drawing.Point(14, 302);
            this.tbOutputPath.Name = "tbOutputPath";
            this.tbOutputPath.ReadOnly = true;
            this.tbOutputPath.Size = new System.Drawing.Size(440, 21);
            this.tbOutputPath.TabIndex = 107;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 287);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 12);
            this.label1.TabIndex = 106;
            this.label1.Text = "输出文件地理数据库位置";
            // 
            // btnDel
            // 
            this.btnDel.Enabled = false;
            this.btnDel.Location = new System.Drawing.Point(459, 50);
            this.btnDel.Margin = new System.Windows.Forms.Padding(2);
            this.btnDel.Name = "btnDel";
            this.btnDel.Size = new System.Drawing.Size(41, 22);
            this.btnDel.TabIndex = 99;
            this.btnDel.Text = "移除";
            this.btnDel.UseVisualStyleBackColor = true;
            this.btnDel.Click += new System.EventHandler(this.btnDel_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 12);
            this.label2.TabIndex = 101;
            this.label2.Text = "输入地理数据库文件";
            // 
            // btnSourceDB
            // 
            this.btnSourceDB.Location = new System.Drawing.Point(457, 24);
            this.btnSourceDB.Margin = new System.Windows.Forms.Padding(2);
            this.btnSourceDB.Name = "btnSourceDB";
            this.btnSourceDB.Size = new System.Drawing.Size(43, 22);
            this.btnSourceDB.TabIndex = 103;
            this.btnSourceDB.Text = "检索";
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
            this.lvDataBase.Location = new System.Drawing.Point(14, 24);
            this.lvDataBase.Name = "lvDataBase";
            this.lvDataBase.Size = new System.Drawing.Size(440, 198);
            this.lvDataBase.TabIndex = 104;
            this.lvDataBase.UseCompatibleStateImageBehavior = false;
            this.lvDataBase.View = System.Windows.Forms.View.Details;
            this.lvDataBase.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvDataBase_ItemSelectionChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 333);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(137, 12);
            this.label4.TabIndex = 106;
            this.label4.Text = "输出文件地理数据库名称";
            // 
            // tbDBName
            // 
            this.tbDBName.Location = new System.Drawing.Point(14, 348);
            this.tbDBName.Name = "tbDBName";
            this.tbDBName.Size = new System.Drawing.Size(486, 21);
            this.tbDBName.TabIndex = 107;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 239);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(161, 12);
            this.label3.TabIndex = 106;
            this.label3.Text = "输出文件地理数据库参考模板";
            // 
            // tbReferGDBTemplate
            // 
            this.tbReferGDBTemplate.Location = new System.Drawing.Point(14, 254);
            this.tbReferGDBTemplate.Name = "tbReferGDBTemplate";
            this.tbReferGDBTemplate.ReadOnly = true;
            this.tbReferGDBTemplate.Size = new System.Drawing.Size(440, 21);
            this.tbReferGDBTemplate.TabIndex = 107;
            // 
            // btnReferGDBTemplate
            // 
            this.btnReferGDBTemplate.Location = new System.Drawing.Point(459, 253);
            this.btnReferGDBTemplate.Margin = new System.Windows.Forms.Padding(2);
            this.btnReferGDBTemplate.Name = "btnReferGDBTemplate";
            this.btnReferGDBTemplate.Size = new System.Drawing.Size(41, 22);
            this.btnReferGDBTemplate.TabIndex = 108;
            this.btnReferGDBTemplate.Text = "浏览";
            this.btnReferGDBTemplate.UseVisualStyleBackColor = true;
            this.btnReferGDBTemplate.Click += new System.EventHandler(this.btnReferGDBTemplate_Click);
            // 
            // DatabaseMergeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 421);
            this.Controls.Add(this.btnReferGDBTemplate);
            this.Controls.Add(this.btnOutPutPath);
            this.Controls.Add(this.tbDBName);
            this.Controls.Add(this.tbReferGDBTemplate);
            this.Controls.Add(this.tbOutputPath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lvDataBase);
            this.Controls.Add(this.btnSourceDB);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnDel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DatabaseMergeForm";
            this.Text = "数据库合并";
            this.Load += new System.EventHandler(this.DatabaseMergeForm_Load);
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnDel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSourceDB;
        private System.Windows.Forms.ListView lvDataBase;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbDBName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbReferGDBTemplate;
        private System.Windows.Forms.Button btnReferGDBTemplate;
    }
}