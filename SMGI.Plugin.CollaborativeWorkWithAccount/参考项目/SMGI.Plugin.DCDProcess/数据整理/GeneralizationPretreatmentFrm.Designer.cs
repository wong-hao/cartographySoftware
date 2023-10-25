namespace SMGI.Plugin.DCDProcess
{
    partial class GeneralizationPretreatmentFrm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btRiverFieldNone = new System.Windows.Forms.Button();
            this.btRiverFieldAll = new System.Windows.Forms.Button();
            this.chkriverField = new System.Windows.Forms.CheckedListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chkDelGrade2list = new System.Windows.Forms.CheckedListBox();
            this.cbObjScale = new System.Windows.Forms.ComboBox();
            this.btOrgin = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.tbOriginDB = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkDelGradeList = new System.Windows.Forms.CheckedListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btUnSelAll = new System.Windows.Forms.Button();
            this.clbSimplifiedFCName = new System.Windows.Forms.CheckedListBox();
            this.btSelAll = new System.Windows.Forms.Button();
            this.label20 = new System.Windows.Forms.Label();
            this.txtSmooth = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.txtBend = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.chkRoadField = new System.Windows.Forms.CheckedListBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.btRoadFieldNone = new System.Windows.Forms.Button();
            this.btRoadFieldAll = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 750);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(519, 31);
            this.panel1.TabIndex = 9;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(378, 4);
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
            this.panel2.Location = new System.Drawing.Point(442, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(451, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btRiverFieldNone);
            this.groupBox1.Controls.Add(this.btRiverFieldAll);
            this.groupBox1.Controls.Add(this.chkriverField);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.chkDelGrade2list);
            this.groupBox1.Location = new System.Drawing.Point(14, 112);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(499, 224);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "水系处理";
            // 
            // btRiverFieldNone
            // 
            this.btRiverFieldNone.Location = new System.Drawing.Point(453, 65);
            this.btRiverFieldNone.Name = "btRiverFieldNone";
            this.btRiverFieldNone.Size = new System.Drawing.Size(38, 25);
            this.btRiverFieldNone.TabIndex = 29;
            this.btRiverFieldNone.Text = "清空";
            this.btRiverFieldNone.UseVisualStyleBackColor = true;
            this.btRiverFieldNone.Click += new System.EventHandler(this.btRiverFieldNone_Click);
            // 
            // btRiverFieldAll
            // 
            this.btRiverFieldAll.Location = new System.Drawing.Point(453, 34);
            this.btRiverFieldAll.Name = "btRiverFieldAll";
            this.btRiverFieldAll.Size = new System.Drawing.Size(38, 25);
            this.btRiverFieldAll.TabIndex = 28;
            this.btRiverFieldAll.Text = "全选";
            this.btRiverFieldAll.UseVisualStyleBackColor = true;
            this.btRiverFieldAll.Click += new System.EventHandler(this.btRiverFieldAll_Click);
            // 
            // chkriverField
            // 
            this.chkriverField.CheckOnClick = true;
            this.chkriverField.FormattingEnabled = true;
            this.chkriverField.Location = new System.Drawing.Point(193, 34);
            this.chkriverField.MultiColumn = true;
            this.chkriverField.Name = "chkriverField";
            this.chkriverField.Size = new System.Drawing.Size(254, 180);
            this.chkriverField.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(191, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(137, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "水系伪节点处理标识字段";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "预删除的低等级水系";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 12);
            this.label2.TabIndex = 2;
            // 
            // chkDelGrade2list
            // 
            this.chkDelGrade2list.CheckOnClick = true;
            this.chkDelGrade2list.FormattingEnabled = true;
            this.chkDelGrade2list.Location = new System.Drawing.Point(9, 34);
            this.chkDelGrade2list.MultiColumn = true;
            this.chkDelGrade2list.Name = "chkDelGrade2list";
            this.chkDelGrade2list.Size = new System.Drawing.Size(129, 180);
            this.chkDelGrade2list.TabIndex = 1;
            this.chkDelGrade2list.SelectedIndexChanged += new System.EventHandler(this.chkDelGrade2list_SelectedIndexChanged);
            // 
            // cbObjScale
            // 
            this.cbObjScale.FormattingEnabled = true;
            this.cbObjScale.Items.AddRange(new object[] {
            "100000",
            "250000",
            "500000",
            "1000000"});
            this.cbObjScale.Location = new System.Drawing.Point(14, 76);
            this.cbObjScale.Name = "cbObjScale";
            this.cbObjScale.Size = new System.Drawing.Size(491, 20);
            this.cbObjScale.TabIndex = 24;
            // 
            // btOrgin
            // 
            this.btOrgin.Location = new System.Drawing.Point(430, 24);
            this.btOrgin.Name = "btOrgin";
            this.btOrgin.Size = new System.Drawing.Size(75, 23);
            this.btOrgin.TabIndex = 23;
            this.btOrgin.Text = "选择";
            this.btOrgin.UseVisualStyleBackColor = true;
            this.btOrgin.Click += new System.EventHandler(this.btOrgin_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 61);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 20;
            this.label6.Text = "缩编后比例尺";
            // 
            // tbOriginDB
            // 
            this.tbOriginDB.Location = new System.Drawing.Point(14, 26);
            this.tbOriginDB.Name = "tbOriginDB";
            this.tbOriginDB.ReadOnly = true;
            this.tbOriginDB.Size = new System.Drawing.Size(410, 21);
            this.tbOriginDB.TabIndex = 22;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 12);
            this.label1.TabIndex = 21;
            this.label1.Text = "大比例尺成果数据库";
            // 
            // chkDelGradeList
            // 
            this.chkDelGradeList.CheckOnClick = true;
            this.chkDelGradeList.FormattingEnabled = true;
            this.chkDelGradeList.Location = new System.Drawing.Point(9, 34);
            this.chkDelGradeList.MultiColumn = true;
            this.chkDelGradeList.Name = "chkDelGradeList";
            this.chkDelGradeList.Size = new System.Drawing.Size(129, 180);
            this.chkDelGradeList.TabIndex = 1;
            this.chkDelGradeList.SelectedIndexChanged += new System.EventHandler(this.chkDelGradeList_SelectedIndexChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btUnSelAll);
            this.groupBox3.Controls.Add(this.clbSimplifiedFCName);
            this.groupBox3.Controls.Add(this.btSelAll);
            this.groupBox3.Controls.Add(this.label20);
            this.groupBox3.Controls.Add(this.txtSmooth);
            this.groupBox3.Controls.Add(this.label21);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.txtBend);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Location = new System.Drawing.Point(12, 582);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(499, 162);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "线化简";
            // 
            // btUnSelAll
            // 
            this.btUnSelAll.Location = new System.Drawing.Point(453, 78);
            this.btUnSelAll.Name = "btUnSelAll";
            this.btUnSelAll.Size = new System.Drawing.Size(38, 25);
            this.btUnSelAll.TabIndex = 29;
            this.btUnSelAll.Text = "清空";
            this.btUnSelAll.UseVisualStyleBackColor = true;
            this.btUnSelAll.Click += new System.EventHandler(this.btUnSelAll_Click);
            // 
            // clbSimplifiedFCName
            // 
            this.clbSimplifiedFCName.CheckOnClick = true;
            this.clbSimplifiedFCName.FormattingEnabled = true;
            this.clbSimplifiedFCName.Location = new System.Drawing.Point(14, 47);
            this.clbSimplifiedFCName.MultiColumn = true;
            this.clbSimplifiedFCName.Name = "clbSimplifiedFCName";
            this.clbSimplifiedFCName.Size = new System.Drawing.Size(433, 100);
            this.clbSimplifiedFCName.TabIndex = 33;
            // 
            // btSelAll
            // 
            this.btSelAll.Location = new System.Drawing.Point(453, 47);
            this.btSelAll.Name = "btSelAll";
            this.btSelAll.Size = new System.Drawing.Size(38, 25);
            this.btSelAll.TabIndex = 28;
            this.btSelAll.Text = "全选";
            this.btSelAll.UseVisualStyleBackColor = true;
            this.btSelAll.Click += new System.EventHandler(this.btSelAll_Click);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(430, 22);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(29, 12);
            this.label20.TabIndex = 32;
            this.label20.Text = "毫米";
            // 
            // txtSmooth
            // 
            this.txtSmooth.AcceptsReturn = true;
            this.txtSmooth.Location = new System.Drawing.Point(336, 19);
            this.txtSmooth.Name = "txtSmooth";
            this.txtSmooth.Size = new System.Drawing.Size(88, 21);
            this.txtSmooth.TabIndex = 31;
            this.txtSmooth.Text = "2";
            this.txtSmooth.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(277, 22);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(53, 12);
            this.label21.TabIndex = 30;
            this.label21.Text = "平滑参数";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(211, 22);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(29, 12);
            this.label12.TabIndex = 29;
            this.label12.Text = "毫米";
            // 
            // txtBend
            // 
            this.txtBend.Location = new System.Drawing.Point(117, 19);
            this.txtBend.Name = "txtBend";
            this.txtBend.Size = new System.Drawing.Size(88, 21);
            this.txtBend.TabIndex = 28;
            this.txtBend.Text = "1";
            this.txtBend.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(12, 22);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(101, 12);
            this.label13.TabIndex = 27;
            this.label13.Text = "弯曲化简宽度图上";
            // 
            // chkRoadField
            // 
            this.chkRoadField.CheckOnClick = true;
            this.chkRoadField.FormattingEnabled = true;
            this.chkRoadField.Location = new System.Drawing.Point(193, 34);
            this.chkRoadField.MultiColumn = true;
            this.chkRoadField.Name = "chkRoadField";
            this.chkRoadField.Size = new System.Drawing.Size(254, 180);
            this.chkRoadField.TabIndex = 1;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.btRoadFieldNone);
            this.groupBox6.Controls.Add(this.btRoadFieldAll);
            this.groupBox6.Controls.Add(this.chkRoadField);
            this.groupBox6.Controls.Add(this.chkDelGradeList);
            this.groupBox6.Controls.Add(this.label5);
            this.groupBox6.Controls.Add(this.label7);
            this.groupBox6.Controls.Add(this.label8);
            this.groupBox6.Location = new System.Drawing.Point(14, 348);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(499, 225);
            this.groupBox6.TabIndex = 10;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "道路处理";
            // 
            // btRoadFieldNone
            // 
            this.btRoadFieldNone.Location = new System.Drawing.Point(453, 65);
            this.btRoadFieldNone.Name = "btRoadFieldNone";
            this.btRoadFieldNone.Size = new System.Drawing.Size(38, 25);
            this.btRoadFieldNone.TabIndex = 29;
            this.btRoadFieldNone.Text = "清空";
            this.btRoadFieldNone.UseVisualStyleBackColor = true;
            this.btRoadFieldNone.Click += new System.EventHandler(this.btRoadFieldNone_Click);
            // 
            // btRoadFieldAll
            // 
            this.btRoadFieldAll.Location = new System.Drawing.Point(453, 34);
            this.btRoadFieldAll.Name = "btRoadFieldAll";
            this.btRoadFieldAll.Size = new System.Drawing.Size(38, 25);
            this.btRoadFieldAll.TabIndex = 28;
            this.btRoadFieldAll.Text = "全选";
            this.btRoadFieldAll.UseVisualStyleBackColor = true;
            this.btRoadFieldAll.Click += new System.EventHandler(this.btRoadFieldAll_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(191, 17);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(137, 12);
            this.label5.TabIndex = 3;
            this.label5.Text = "道路伪节点处理标识字段";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 17);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(113, 12);
            this.label7.TabIndex = 3;
            this.label7.Text = "预删除的低等级道路";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 17);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(0, 12);
            this.label8.TabIndex = 2;
            // 
            // GeneralizationPretreatmentFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 781);
            this.Controls.Add(this.cbObjScale);
            this.Controls.Add(this.btOrgin);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbOriginDB);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GeneralizationPretreatmentFrm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "缩编预处理";
            this.Load += new System.EventHandler(this.GeneralizationPretreatmentFrm_Load);
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cbObjScale;
        private System.Windows.Forms.Button btOrgin;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbOriginDB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox chkDelGrade2list;
        private System.Windows.Forms.CheckedListBox chkDelGradeList;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label20;
        public System.Windows.Forms.TextBox txtSmooth;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label12;
        public System.Windows.Forms.TextBox txtBend;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckedListBox chkRoadField;
        private System.Windows.Forms.CheckedListBox chkriverField;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btRiverFieldNone;
        private System.Windows.Forms.Button btRiverFieldAll;
        private System.Windows.Forms.Button btRoadFieldNone;
        private System.Windows.Forms.Button btRoadFieldAll;
        private System.Windows.Forms.Button btUnSelAll;
        private System.Windows.Forms.CheckedListBox clbSimplifiedFCName;
        private System.Windows.Forms.Button btSelAll;
    }
}