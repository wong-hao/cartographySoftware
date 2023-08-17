namespace SMGI.Plugin.DCDProcess
{
    partial class FrmBuildingGen
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmBuildingGen));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtSQL = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtScale = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtGdbPath = new System.Windows.Forms.TextBox();
            this.btLastPrj = new System.Windows.Forms.Button();
            this.btSelectGdb = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cbOrthogonal = new System.Windows.Forms.CheckBox();
            this.tbArea = new System.Windows.Forms.TextBox();
            this.tbHole = new System.Windows.Forms.TextBox();
            this.tbDis = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btProcess = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cbConflict = new System.Windows.Forms.CheckBox();
            this.tbArea2 = new System.Windows.Forms.TextBox();
            this.tbTolerance = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.btCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtSQL);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.txtScale);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtGdbPath);
            this.groupBox1.Controls.Add(this.btLastPrj);
            this.groupBox1.Controls.Add(this.btSelectGdb);
            this.groupBox1.Location = new System.Drawing.Point(16, 15);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(640, 150);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "输入数据";
            // 
            // txtSQL
            // 
            this.txtSQL.Location = new System.Drawing.Point(171, 95);
            this.txtSQL.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtSQL.Multiline = true;
            this.txtSQL.Name = "txtSQL";
            this.txtSQL.Size = new System.Drawing.Size(460, 46);
            this.txtSQL.TabIndex = 14;
            this.txtSQL.Text = "GB is not null";
            this.txtSQL.TextChanged += new System.EventHandler(this.txtSQL_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(19, 116);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(112, 15);
            this.label8.TabIndex = 13;
            this.label8.Text = "建筑筛选条件：";
            this.label8.Click += new System.EventHandler(this.label8_Click);
            // 
            // txtScale
            // 
            this.txtScale.Location = new System.Drawing.Point(171, 58);
            this.txtScale.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtScale.Name = "txtScale";
            this.txtScale.Size = new System.Drawing.Size(165, 25);
            this.txtScale.TabIndex = 12;
            this.txtScale.Text = "50000";
            this.txtScale.TextChanged += new System.EventHandler(this.txt_scale_TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(55, 62);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(113, 15);
            this.label11.TabIndex = 11;
            this.label11.Text = "参考比例： 1：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(67, 28);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "数据库：";
            // 
            // txtGdbPath
            // 
            this.txtGdbPath.Location = new System.Drawing.Point(171, 24);
            this.txtGdbPath.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtGdbPath.Name = "txtGdbPath";
            this.txtGdbPath.Size = new System.Drawing.Size(293, 25);
            this.txtGdbPath.TabIndex = 4;
            this.txtGdbPath.TextChanged += new System.EventHandler(this.txt_scale_TextChanged);
            // 
            // btLastPrj
            // 
            this.btLastPrj.Location = new System.Drawing.Point(543, 21);
            this.btLastPrj.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btLastPrj.Name = "btLastPrj";
            this.btLastPrj.Size = new System.Drawing.Size(89, 29);
            this.btLastPrj.TabIndex = 5;
            this.btLastPrj.Text = "最近工程";
            this.btLastPrj.UseVisualStyleBackColor = true;
            this.btLastPrj.Click += new System.EventHandler(this.btlastprj_Click);
            // 
            // btSelectGdb
            // 
            this.btSelectGdb.Location = new System.Drawing.Point(475, 21);
            this.btSelectGdb.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btSelectGdb.Name = "btSelectGdb";
            this.btSelectGdb.Size = new System.Drawing.Size(61, 29);
            this.btSelectGdb.TabIndex = 5;
            this.btSelectGdb.Text = "浏览..";
            this.btSelectGdb.UseVisualStyleBackColor = true;
            this.btSelectGdb.Click += new System.EventHandler(this.btSelectGdb_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cbOrthogonal);
            this.groupBox2.Controls.Add(this.tbArea);
            this.groupBox2.Controls.Add(this.tbHole);
            this.groupBox2.Controls.Add(this.tbDis);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(16, 172);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Size = new System.Drawing.Size(640, 175);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "聚合面参数设置";
            // 
            // cbOrthogonal
            // 
            this.cbOrthogonal.AutoSize = true;
            this.cbOrthogonal.Checked = true;
            this.cbOrthogonal.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbOrthogonal.Enabled = false;
            this.cbOrthogonal.Location = new System.Drawing.Point(31, 138);
            this.cbOrthogonal.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbOrthogonal.Name = "cbOrthogonal";
            this.cbOrthogonal.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cbOrthogonal.Size = new System.Drawing.Size(119, 19);
            this.cbOrthogonal.TabIndex = 3;
            this.cbOrthogonal.Text = "保留正交形状";
            this.cbOrthogonal.UseVisualStyleBackColor = true;
            // 
            // tbArea
            // 
            this.tbArea.Location = new System.Drawing.Point(109, 70);
            this.tbArea.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbArea.Name = "tbArea";
            this.tbArea.Size = new System.Drawing.Size(176, 25);
            this.tbArea.TabIndex = 1;
            this.tbArea.Text = "2";
            // 
            // tbHole
            // 
            this.tbHole.Location = new System.Drawing.Point(109, 104);
            this.tbHole.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbHole.Name = "tbHole";
            this.tbHole.Size = new System.Drawing.Size(176, 25);
            this.tbHole.TabIndex = 1;
            this.tbHole.Text = "2";
            // 
            // tbDis
            // 
            this.tbDis.Location = new System.Drawing.Point(109, 36);
            this.tbDis.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbDis.Name = "tbDis";
            this.tbDis.Size = new System.Drawing.Size(176, 25);
            this.tbDis.TabIndex = 1;
            this.tbDis.Text = "1";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(295, 74);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(31, 15);
            this.label7.TabIndex = 0;
            this.label7.Text = "mm²";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(295, 108);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(31, 15);
            this.label5.TabIndex = 0;
            this.label5.Text = "mm²";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(31, 74);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(82, 15);
            this.label6.TabIndex = 0;
            this.label6.Text = "最小面积：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(31, 108);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 15);
            this.label4.TabIndex = 0;
            this.label4.Text = "最小孔洞：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(295, 40);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(23, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "mm";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(31, 40);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "聚合距离：";
            // 
            // btProcess
            // 
            this.btProcess.Location = new System.Drawing.Point(480, 514);
            this.btProcess.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btProcess.Name = "btProcess";
            this.btProcess.Size = new System.Drawing.Size(72, 35);
            this.btProcess.TabIndex = 1;
            this.btProcess.Text = "确定";
            this.btProcess.UseVisualStyleBackColor = true;
            this.btProcess.Click += new System.EventHandler(this.btProcess_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.cbConflict);
            this.groupBox4.Controls.Add(this.tbArea2);
            this.groupBox4.Controls.Add(this.tbTolerance);
            this.groupBox4.Controls.Add(this.label10);
            this.groupBox4.Controls.Add(this.label12);
            this.groupBox4.Controls.Add(this.label14);
            this.groupBox4.Controls.Add(this.label15);
            this.groupBox4.Location = new System.Drawing.Point(16, 355);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox4.Size = new System.Drawing.Size(640, 140);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "简化建筑物参数设置";
            // 
            // cbConflict
            // 
            this.cbConflict.AutoSize = true;
            this.cbConflict.Checked = true;
            this.cbConflict.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbConflict.Enabled = false;
            this.cbConflict.Location = new System.Drawing.Point(31, 104);
            this.cbConflict.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbConflict.Name = "cbConflict";
            this.cbConflict.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cbConflict.Size = new System.Drawing.Size(119, 19);
            this.cbConflict.TabIndex = 3;
            this.cbConflict.Text = "检查空间冲突";
            this.cbConflict.UseVisualStyleBackColor = true;
            // 
            // tbArea2
            // 
            this.tbArea2.Location = new System.Drawing.Point(109, 70);
            this.tbArea2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbArea2.Name = "tbArea2";
            this.tbArea2.Size = new System.Drawing.Size(176, 25);
            this.tbArea2.TabIndex = 1;
            this.tbArea2.Text = "2";
            // 
            // tbTolerance
            // 
            this.tbTolerance.Location = new System.Drawing.Point(109, 36);
            this.tbTolerance.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbTolerance.Name = "tbTolerance";
            this.tbTolerance.Size = new System.Drawing.Size(176, 25);
            this.tbTolerance.TabIndex = 1;
            this.tbTolerance.Text = "0.4";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(295, 74);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(31, 15);
            this.label10.TabIndex = 0;
            this.label10.Text = "mm²";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(31, 74);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(82, 15);
            this.label12.TabIndex = 0;
            this.label12.Text = "最小面积：";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(295, 40);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(23, 15);
            this.label14.TabIndex = 0;
            this.label14.Text = "mm";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(31, 40);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(82, 15);
            this.label15.TabIndex = 0;
            this.label15.Text = "简化容差：";
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(587, 514);
            this.btCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(72, 35);
            this.btCancel.TabIndex = 2;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // FrmBuildingGen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 560);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btProcess);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmBuildingGen";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "房屋建筑区提取";
            this.Load += new System.EventHandler(this.FrmLCAHouseProcess_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtGdbPath;
        private System.Windows.Forms.Button btSelectGdb;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btLastPrj;
        private System.Windows.Forms.CheckBox cbOrthogonal;
        private System.Windows.Forms.TextBox tbArea;
        private System.Windows.Forms.TextBox tbHole;
        private System.Windows.Forms.TextBox tbDis;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btProcess;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox cbConflict;
        private System.Windows.Forms.TextBox tbArea2;
        private System.Windows.Forms.TextBox tbTolerance;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtScale;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.TextBox txtSQL;
        private System.Windows.Forms.Label label8;
    }
}