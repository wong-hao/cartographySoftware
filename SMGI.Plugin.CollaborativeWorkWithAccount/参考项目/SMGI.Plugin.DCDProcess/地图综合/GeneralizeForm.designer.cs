namespace SMGI.Plugin.DCDProcess
{
    partial class GeneralizeForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.BendComboBox = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.clbLayerList = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.numSmoothTol = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.SmoothComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.numSimpTol = new System.Windows.Forms.NumericUpDown();
            this.smoothGroup = new System.Windows.Forms.GroupBox();
            this.cbEnableSmooth = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnVerify = new System.Windows.Forms.Button();
            this.rtbSQLText = new System.Windows.Forms.RichTextBox();
            this.objFCPanel = new System.Windows.Forms.Panel();
            this.filterPanel = new System.Windows.Forms.Panel();
            this.otherPanel = new System.Windows.Forms.Panel();
            this.cmbReferScale = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.numSmoothTol)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSimpTol)).BeginInit();
            this.smoothGroup.SuspendLayout();
            this.objFCPanel.SuspendLayout();
            this.filterPanel.SuspendLayout();
            this.otherPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "简化算法";
            // 
            // BendComboBox
            // 
            this.BendComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BendComboBox.FormattingEnabled = true;
            this.BendComboBox.Location = new System.Drawing.Point(14, 78);
            this.BendComboBox.Name = "BendComboBox";
            this.BendComboBox.Size = new System.Drawing.Size(294, 20);
            this.BendComboBox.TabIndex = 3;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(12, 9);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(65, 12);
            this.label14.TabIndex = 3;
            this.label14.Text = "参考比例尺";
            // 
            // clbLayerList
            // 
            this.clbLayerList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.clbLayerList.FormattingEnabled = true;
            this.clbLayerList.Location = new System.Drawing.Point(14, 24);
            this.clbLayerList.Name = "clbLayerList";
            this.clbLayerList.Size = new System.Drawing.Size(294, 20);
            this.clbLayerList.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "目标图层";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(140, 130);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(29, 12);
            this.label12.TabIndex = 23;
            this.label12.Text = "毫米";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(12, 112);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(95, 12);
            this.label13.TabIndex = 21;
            this.label13.Text = "化简容差(图面）";
            // 
            // numSmoothTol
            // 
            this.numSmoothTol.DecimalPlaces = 2;
            this.numSmoothTol.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numSmoothTol.Location = new System.Drawing.Point(18, 89);
            this.numSmoothTol.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numSmoothTol.Name = "numSmoothTol";
            this.numSmoothTol.Size = new System.Drawing.Size(120, 21);
            this.numSmoothTol.TabIndex = 31;
            this.numSmoothTol.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numSmoothTol.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(17, 74);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(101, 12);
            this.label7.TabIndex = 21;
            this.label7.Text = "平滑容差（图面）";
            // 
            // SmoothComboBox
            // 
            this.SmoothComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SmoothComboBox.FormattingEnabled = true;
            this.SmoothComboBox.Location = new System.Drawing.Point(19, 38);
            this.SmoothComboBox.Name = "SmoothComboBox";
            this.SmoothComboBox.Size = new System.Drawing.Size(271, 20);
            this.SmoothComboBox.TabIndex = 3;
            this.SmoothComboBox.SelectedIndexChanged += new System.EventHandler(this.SmoothComboBox_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(144, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 23;
            this.label3.Text = "毫米";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(17, 23);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 12);
            this.label8.TabIndex = 1;
            this.label8.Text = "平滑算法：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "1：";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 489);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(324, 31);
            this.panel1.TabIndex = 30;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(183, 4);
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
            this.panel2.Location = new System.Drawing.Point(247, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(256, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // numSimpTol
            // 
            this.numSimpTol.DecimalPlaces = 2;
            this.numSimpTol.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numSimpTol.Location = new System.Drawing.Point(14, 128);
            this.numSimpTol.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numSimpTol.Name = "numSimpTol";
            this.numSimpTol.Size = new System.Drawing.Size(120, 21);
            this.numSimpTol.TabIndex = 31;
            this.numSimpTol.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numSimpTol.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // smoothGroup
            // 
            this.smoothGroup.Controls.Add(this.numSmoothTol);
            this.smoothGroup.Controls.Add(this.label7);
            this.smoothGroup.Controls.Add(this.SmoothComboBox);
            this.smoothGroup.Controls.Add(this.label3);
            this.smoothGroup.Controls.Add(this.label8);
            this.smoothGroup.Location = new System.Drawing.Point(12, 188);
            this.smoothGroup.Name = "smoothGroup";
            this.smoothGroup.Size = new System.Drawing.Size(296, 125);
            this.smoothGroup.TabIndex = 28;
            this.smoothGroup.TabStop = false;
            this.smoothGroup.Text = "平滑参数";
            // 
            // cbEnableSmooth
            // 
            this.cbEnableSmooth.AutoSize = true;
            this.cbEnableSmooth.Checked = true;
            this.cbEnableSmooth.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbEnableSmooth.Location = new System.Drawing.Point(12, 166);
            this.cbEnableSmooth.Name = "cbEnableSmooth";
            this.cbEnableSmooth.Size = new System.Drawing.Size(96, 16);
            this.cbEnableSmooth.TabIndex = 29;
            this.cbEnableSmooth.Text = "开启光滑处理";
            this.cbEnableSmooth.UseVisualStyleBackColor = true;
            this.cbEnableSmooth.CheckedChanged += new System.EventHandler(this.cbEnableSmooth_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "定义查询(可选)";
            // 
            // btnVerify
            // 
            this.btnVerify.Location = new System.Drawing.Point(265, 18);
            this.btnVerify.Name = "btnVerify";
            this.btnVerify.Size = new System.Drawing.Size(43, 23);
            this.btnVerify.TabIndex = 129;
            this.btnVerify.Text = "验证";
            this.btnVerify.UseVisualStyleBackColor = true;
            this.btnVerify.Click += new System.EventHandler(this.btnVerify_Click);
            // 
            // rtbSQLText
            // 
            this.rtbSQLText.Location = new System.Drawing.Point(14, 20);
            this.rtbSQLText.Name = "rtbSQLText";
            this.rtbSQLText.Size = new System.Drawing.Size(245, 65);
            this.rtbSQLText.TabIndex = 128;
            this.rtbSQLText.Text = "";
            // 
            // objFCPanel
            // 
            this.objFCPanel.Controls.Add(this.label5);
            this.objFCPanel.Controls.Add(this.clbLayerList);
            this.objFCPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.objFCPanel.Location = new System.Drawing.Point(0, 0);
            this.objFCPanel.Name = "objFCPanel";
            this.objFCPanel.Size = new System.Drawing.Size(324, 58);
            this.objFCPanel.TabIndex = 130;
            // 
            // filterPanel
            // 
            this.filterPanel.Controls.Add(this.label4);
            this.filterPanel.Controls.Add(this.rtbSQLText);
            this.filterPanel.Controls.Add(this.btnVerify);
            this.filterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.filterPanel.Location = new System.Drawing.Point(0, 58);
            this.filterPanel.Name = "filterPanel";
            this.filterPanel.Size = new System.Drawing.Size(324, 91);
            this.filterPanel.TabIndex = 131;
            // 
            // otherPanel
            // 
            this.otherPanel.Controls.Add(this.cmbReferScale);
            this.otherPanel.Controls.Add(this.label14);
            this.otherPanel.Controls.Add(this.label1);
            this.otherPanel.Controls.Add(this.BendComboBox);
            this.otherPanel.Controls.Add(this.label2);
            this.otherPanel.Controls.Add(this.numSimpTol);
            this.otherPanel.Controls.Add(this.label13);
            this.otherPanel.Controls.Add(this.smoothGroup);
            this.otherPanel.Controls.Add(this.cbEnableSmooth);
            this.otherPanel.Controls.Add(this.label12);
            this.otherPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.otherPanel.Location = new System.Drawing.Point(0, 149);
            this.otherPanel.Name = "otherPanel";
            this.otherPanel.Size = new System.Drawing.Size(324, 323);
            this.otherPanel.TabIndex = 132;
            // 
            // cmbReferScale
            // 
            this.cmbReferScale.FormattingEnabled = true;
            this.cmbReferScale.Items.AddRange(new object[] {
            "10000",
            "25000",
            "50000",
            "100000",
            "250000",
            "500000",
            "1000000"});
            this.cmbReferScale.Location = new System.Drawing.Point(31, 27);
            this.cmbReferScale.Name = "cmbReferScale";
            this.cmbReferScale.Size = new System.Drawing.Size(174, 20);
            this.cmbReferScale.TabIndex = 32;
            this.cmbReferScale.Text = "0";
            // 
            // GeneralizeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 520);
            this.Controls.Add(this.otherPanel);
            this.Controls.Add(this.filterPanel);
            this.Controls.Add(this.objFCPanel);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GeneralizeForm";
            this.Text = "化简参数设置";
            ((System.ComponentModel.ISupportInitialize)(this.numSmoothTol)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numSimpTol)).EndInit();
            this.smoothGroup.ResumeLayout(false);
            this.smoothGroup.PerformLayout();
            this.objFCPanel.ResumeLayout(false);
            this.objFCPanel.PerformLayout();
            this.filterPanel.ResumeLayout(false);
            this.filterPanel.PerformLayout();
            this.otherPanel.ResumeLayout(false);
            this.otherPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.ComboBox clbLayerList;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.ComboBox BendComboBox;
        public System.Windows.Forms.ComboBox SmoothComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.NumericUpDown numSimpTol;
        private System.Windows.Forms.NumericUpDown numSmoothTol;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox smoothGroup;
        private System.Windows.Forms.CheckBox cbEnableSmooth;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnVerify;
        private System.Windows.Forms.RichTextBox rtbSQLText;
        private System.Windows.Forms.Panel objFCPanel;
        private System.Windows.Forms.Panel filterPanel;
        private System.Windows.Forms.Panel otherPanel;
        private System.Windows.Forms.ComboBox cmbReferScale;
    }
}