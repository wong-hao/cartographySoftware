namespace SMGI.Plugin.DCDProcess
{
    partial class DataBaseProjectForm
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
            System.Windows.Forms.ListViewItem listViewItem13 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem14 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem15 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem16 = new System.Windows.Forms.ListViewItem("");
            this.lvDataBase = new System.Windows.Forms.ListView();
            this.btnSourceDB = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btnDel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.btnOutSR = new System.Windows.Forms.Button();
            this.btnOutPutPath = new System.Windows.Forms.Button();
            this.tbOutputPath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbGeoTrans = new System.Windows.Forms.CheckBox();
            this.gbGeoTrans = new System.Windows.Forms.GroupBox();
            this.tbScale = new System.Windows.Forms.TextBox();
            this.tbZRotate = new System.Windows.Forms.TextBox();
            this.tbYRotate = new System.Windows.Forms.TextBox();
            this.tbXRotate = new System.Windows.Forms.TextBox();
            this.tbZTrans = new System.Windows.Forms.TextBox();
            this.tbYTrans = new System.Windows.Forms.TextBox();
            this.tbXTrans = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.tbOutputSpatialReference = new System.Windows.Forms.TextBox();
            this.cbAddDuffix = new System.Windows.Forms.CheckBox();
            this.tbDuffix = new System.Windows.Forms.TextBox();
            this.gbGeoTrans.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvDataBase
            // 
            this.lvDataBase.FullRowSelect = true;
            this.lvDataBase.GridLines = true;
            this.lvDataBase.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvDataBase.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem13,
            listViewItem14,
            listViewItem15,
            listViewItem16});
            this.lvDataBase.Location = new System.Drawing.Point(14, 24);
            this.lvDataBase.Name = "lvDataBase";
            this.lvDataBase.Size = new System.Drawing.Size(440, 205);
            this.lvDataBase.TabIndex = 108;
            this.lvDataBase.UseCompatibleStateImageBehavior = false;
            this.lvDataBase.View = System.Windows.Forms.View.Details;
            this.lvDataBase.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvDataBase_ItemSelectionChanged);
            // 
            // btnSourceDB
            // 
            this.btnSourceDB.Location = new System.Drawing.Point(457, 24);
            this.btnSourceDB.Margin = new System.Windows.Forms.Padding(2);
            this.btnSourceDB.Name = "btnSourceDB";
            this.btnSourceDB.Size = new System.Drawing.Size(43, 22);
            this.btnSourceDB.TabIndex = 107;
            this.btnSourceDB.Text = "检索";
            this.btnSourceDB.UseVisualStyleBackColor = true;
            this.btnSourceDB.Click += new System.EventHandler(this.btnSourceDB_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 12);
            this.label2.TabIndex = 106;
            this.label2.Text = "输入地理数据库文件";
            // 
            // btnDel
            // 
            this.btnDel.Enabled = false;
            this.btnDel.Location = new System.Drawing.Point(459, 50);
            this.btnDel.Margin = new System.Windows.Forms.Padding(2);
            this.btnDel.Name = "btnDel";
            this.btnDel.Size = new System.Drawing.Size(41, 22);
            this.btnDel.TabIndex = 105;
            this.btnDel.Text = "移除";
            this.btnDel.UseVisualStyleBackColor = true;
            this.btnDel.Click += new System.EventHandler(this.btnDel_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 243);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 106;
            this.label3.Text = "输出坐标系";
            // 
            // btnOutSR
            // 
            this.btnOutSR.Location = new System.Drawing.Point(459, 259);
            this.btnOutSR.Margin = new System.Windows.Forms.Padding(2);
            this.btnOutSR.Name = "btnOutSR";
            this.btnOutSR.Size = new System.Drawing.Size(41, 22);
            this.btnOutSR.TabIndex = 110;
            this.btnOutSR.Text = "浏览";
            this.btnOutSR.UseVisualStyleBackColor = true;
            this.btnOutSR.Click += new System.EventHandler(this.btnOutSR_Click);
            // 
            // btnOutPutPath
            // 
            this.btnOutPutPath.Location = new System.Drawing.Point(459, 312);
            this.btnOutPutPath.Margin = new System.Windows.Forms.Padding(2);
            this.btnOutPutPath.Name = "btnOutPutPath";
            this.btnOutPutPath.Size = new System.Drawing.Size(41, 22);
            this.btnOutPutPath.TabIndex = 113;
            this.btnOutPutPath.Text = "浏览";
            this.btnOutPutPath.UseVisualStyleBackColor = true;
            this.btnOutPutPath.Click += new System.EventHandler(this.btnOutPutPath_Click);
            // 
            // tbOutputPath
            // 
            this.tbOutputPath.Location = new System.Drawing.Point(14, 313);
            this.tbOutputPath.Name = "tbOutputPath";
            this.tbOutputPath.ReadOnly = true;
            this.tbOutputPath.Size = new System.Drawing.Size(440, 21);
            this.tbOutputPath.TabIndex = 112;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 297);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(113, 12);
            this.label4.TabIndex = 111;
            this.label4.Text = "输出地理数据库位置";
            // 
            // cbGeoTrans
            // 
            this.cbGeoTrans.AutoSize = true;
            this.cbGeoTrans.Location = new System.Drawing.Point(14, 403);
            this.cbGeoTrans.Name = "cbGeoTrans";
            this.cbGeoTrans.Size = new System.Drawing.Size(120, 16);
            this.cbGeoTrans.TabIndex = 114;
            this.cbGeoTrans.Text = "地理变换（可选）";
            this.cbGeoTrans.UseVisualStyleBackColor = true;
            this.cbGeoTrans.CheckedChanged += new System.EventHandler(this.cbGeoTrans_CheckedChanged);
            // 
            // gbGeoTrans
            // 
            this.gbGeoTrans.Controls.Add(this.tbScale);
            this.gbGeoTrans.Controls.Add(this.tbZRotate);
            this.gbGeoTrans.Controls.Add(this.tbYRotate);
            this.gbGeoTrans.Controls.Add(this.tbXRotate);
            this.gbGeoTrans.Controls.Add(this.tbZTrans);
            this.gbGeoTrans.Controls.Add(this.tbYTrans);
            this.gbGeoTrans.Controls.Add(this.tbXTrans);
            this.gbGeoTrans.Controls.Add(this.label7);
            this.gbGeoTrans.Controls.Add(this.label6);
            this.gbGeoTrans.Controls.Add(this.label5);
            this.gbGeoTrans.Controls.Add(this.label8);
            this.gbGeoTrans.Controls.Add(this.label9);
            this.gbGeoTrans.Controls.Add(this.label10);
            this.gbGeoTrans.Controls.Add(this.label11);
            this.gbGeoTrans.Enabled = false;
            this.gbGeoTrans.Location = new System.Drawing.Point(14, 425);
            this.gbGeoTrans.Name = "gbGeoTrans";
            this.gbGeoTrans.Size = new System.Drawing.Size(440, 109);
            this.gbGeoTrans.TabIndex = 115;
            this.gbGeoTrans.TabStop = false;
            this.gbGeoTrans.Text = "变换参数";
            // 
            // tbScale
            // 
            this.tbScale.Location = new System.Drawing.Point(57, 74);
            this.tbScale.Name = "tbScale";
            this.tbScale.Size = new System.Drawing.Size(67, 21);
            this.tbScale.TabIndex = 13;
            this.tbScale.Text = "0.0";
            // 
            // tbZRotate
            // 
            this.tbZRotate.Location = new System.Drawing.Point(359, 46);
            this.tbZRotate.Name = "tbZRotate";
            this.tbZRotate.Size = new System.Drawing.Size(67, 21);
            this.tbZRotate.TabIndex = 12;
            this.tbZRotate.Text = "0.0";
            // 
            // tbYRotate
            // 
            this.tbYRotate.Location = new System.Drawing.Point(211, 46);
            this.tbYRotate.Name = "tbYRotate";
            this.tbYRotate.Size = new System.Drawing.Size(67, 21);
            this.tbYRotate.TabIndex = 11;
            this.tbYRotate.Text = "0.0";
            // 
            // tbXRotate
            // 
            this.tbXRotate.Location = new System.Drawing.Point(57, 46);
            this.tbXRotate.Name = "tbXRotate";
            this.tbXRotate.Size = new System.Drawing.Size(67, 21);
            this.tbXRotate.TabIndex = 10;
            this.tbXRotate.Text = "0.0";
            // 
            // tbZTrans
            // 
            this.tbZTrans.Location = new System.Drawing.Point(359, 18);
            this.tbZTrans.Name = "tbZTrans";
            this.tbZTrans.Size = new System.Drawing.Size(67, 21);
            this.tbZTrans.TabIndex = 9;
            this.tbZTrans.Text = "0.0";
            // 
            // tbYTrans
            // 
            this.tbYTrans.Location = new System.Drawing.Point(211, 18);
            this.tbYTrans.Name = "tbYTrans";
            this.tbYTrans.Size = new System.Drawing.Size(67, 21);
            this.tbYTrans.TabIndex = 8;
            this.tbYTrans.Text = "0.0";
            // 
            // tbXTrans
            // 
            this.tbXTrans.Location = new System.Drawing.Point(57, 19);
            this.tbXTrans.Name = "tbXTrans";
            this.tbXTrans.Size = new System.Drawing.Size(67, 21);
            this.tbXTrans.TabIndex = 7;
            this.tbXTrans.Text = "0.0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 77);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 12);
            this.label7.TabIndex = 6;
            this.label7.Text = "K缩放：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(306, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 12);
            this.label6.TabIndex = 5;
            this.label6.Text = "Z旋转：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(158, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "Y旋转：";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(9, 49);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(47, 12);
            this.label8.TabIndex = 3;
            this.label8.Text = "X旋转：";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(306, 21);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(47, 12);
            this.label9.TabIndex = 2;
            this.label9.Text = "Z平移：";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(158, 21);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(47, 12);
            this.label10.TabIndex = 1;
            this.label10.Text = "Y平移：";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(9, 21);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(47, 12);
            this.label11.TabIndex = 0;
            this.label11.Text = "X平移：";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 546);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(505, 31);
            this.panel1.TabIndex = 116;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(364, 4);
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
            this.panel2.Location = new System.Drawing.Point(428, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(437, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // tbOutputSpatialReference
            // 
            this.tbOutputSpatialReference.Location = new System.Drawing.Point(14, 260);
            this.tbOutputSpatialReference.Name = "tbOutputSpatialReference";
            this.tbOutputSpatialReference.ReadOnly = true;
            this.tbOutputSpatialReference.Size = new System.Drawing.Size(440, 21);
            this.tbOutputSpatialReference.TabIndex = 112;
            // 
            // cbAddDuffix
            // 
            this.cbAddDuffix.AutoSize = true;
            this.cbAddDuffix.Checked = true;
            this.cbAddDuffix.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAddDuffix.Location = new System.Drawing.Point(14, 350);
            this.cbAddDuffix.Name = "cbAddDuffix";
            this.cbAddDuffix.Size = new System.Drawing.Size(180, 16);
            this.cbAddDuffix.TabIndex = 114;
            this.cbAddDuffix.Text = "为投影后的数据库名添加后缀";
            this.cbAddDuffix.UseVisualStyleBackColor = true;
            this.cbAddDuffix.CheckedChanged += new System.EventHandler(this.cbAddDuffix_CheckedChanged);
            // 
            // tbDuffix
            // 
            this.tbDuffix.Location = new System.Drawing.Point(14, 373);
            this.tbDuffix.Name = "tbDuffix";
            this.tbDuffix.Size = new System.Drawing.Size(440, 21);
            this.tbDuffix.TabIndex = 117;
            this.tbDuffix.Text = "project";
            this.tbDuffix.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // DataBaseProjectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(505, 577);
            this.Controls.Add(this.tbDuffix);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.gbGeoTrans);
            this.Controls.Add(this.cbAddDuffix);
            this.Controls.Add(this.cbGeoTrans);
            this.Controls.Add(this.btnOutPutPath);
            this.Controls.Add(this.tbOutputSpatialReference);
            this.Controls.Add(this.tbOutputPath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnOutSR);
            this.Controls.Add(this.lvDataBase);
            this.Controls.Add(this.btnSourceDB);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnDel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "DataBaseProjectForm";
            this.Text = "数据库投影";
            this.Load += new System.EventHandler(this.DataBaseProjectForm_Load);
            this.gbGeoTrans.ResumeLayout(false);
            this.gbGeoTrans.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lvDataBase;
        private System.Windows.Forms.Button btnSourceDB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnDel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnOutSR;
        private System.Windows.Forms.Button btnOutPutPath;
        private System.Windows.Forms.TextBox tbOutputPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbGeoTrans;
        private System.Windows.Forms.GroupBox gbGeoTrans;
        private System.Windows.Forms.TextBox tbScale;
        private System.Windows.Forms.TextBox tbZRotate;
        private System.Windows.Forms.TextBox tbYRotate;
        private System.Windows.Forms.TextBox tbXRotate;
        private System.Windows.Forms.TextBox tbZTrans;
        private System.Windows.Forms.TextBox tbYTrans;
        private System.Windows.Forms.TextBox tbXTrans;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.TextBox tbOutputSpatialReference;
        private System.Windows.Forms.CheckBox cbAddDuffix;
        private System.Windows.Forms.TextBox tbDuffix;
    }
}