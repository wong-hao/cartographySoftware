namespace SMGI.Plugin.DCDProcess
{
    partial class BOULConstructBOUAFrm
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
            this.label1 = new System.Windows.Forms.Label();
            this.cbBOUL = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbTYPE = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbBOUA = new System.Windows.Forms.ComboBox();
            this.btnShpFile = new System.Windows.Forms.Button();
            this.tbShpFileName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbPropJoin = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbProPlgFCName = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 381);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(336, 31);
            this.panel1.TabIndex = 9;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(195, 4);
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
            this.panel2.Location = new System.Drawing.Point(259, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(268, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 10;
            this.label1.Text = "境界线图层";
            // 
            // cbBOUL
            // 
            this.cbBOUL.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbBOUL.FormattingEnabled = true;
            this.cbBOUL.Location = new System.Drawing.Point(15, 29);
            this.cbBOUL.Name = "cbBOUL";
            this.cbBOUL.Size = new System.Drawing.Size(309, 20);
            this.cbBOUL.TabIndex = 11;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "境界类型";
            // 
            // cbTYPE
            // 
            this.cbTYPE.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTYPE.FormattingEnabled = true;
            this.cbTYPE.Location = new System.Drawing.Point(15, 79);
            this.cbTYPE.Name = "cbTYPE";
            this.cbTYPE.Size = new System.Drawing.Size(309, 20);
            this.cbTYPE.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 10;
            this.label3.Text = "选择输出面图层";
            // 
            // cbBOUA
            // 
            this.cbBOUA.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbBOUA.FormattingEnabled = true;
            this.cbBOUA.Location = new System.Drawing.Point(9, 36);
            this.cbBOUA.Name = "cbBOUA";
            this.cbBOUA.Size = new System.Drawing.Size(303, 20);
            this.cbBOUA.TabIndex = 11;
            this.cbBOUA.SelectedIndexChanged += new System.EventHandler(this.cbBOUA_SelectedIndexChanged);
            // 
            // btnShpFile
            // 
            this.btnShpFile.Location = new System.Drawing.Point(287, 239);
            this.btnShpFile.Name = "btnShpFile";
            this.btnShpFile.Size = new System.Drawing.Size(39, 23);
            this.btnShpFile.TabIndex = 27;
            this.btnShpFile.Text = "...";
            this.btnShpFile.UseVisualStyleBackColor = true;
            this.btnShpFile.Click += new System.EventHandler(this.btnShpFile_Click);
            // 
            // tbShpFileName
            // 
            this.tbShpFileName.Location = new System.Drawing.Point(17, 241);
            this.tbShpFileName.Name = "tbShpFileName";
            this.tbShpFileName.ReadOnly = true;
            this.tbShpFileName.Size = new System.Drawing.Size(264, 21);
            this.tbShpFileName.TabIndex = 26;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 226);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 25;
            this.label4.Text = "范围文件(可选)";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbPropJoin);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.cbProPlgFCName);
            this.groupBox1.Location = new System.Drawing.Point(17, 275);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(307, 100);
            this.groupBox1.TabIndex = 28;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "属性关联";
            // 
            // cbPropJoin
            // 
            this.cbPropJoin.AutoSize = true;
            this.cbPropJoin.Location = new System.Drawing.Point(8, 21);
            this.cbPropJoin.Name = "cbPropJoin";
            this.cbPropJoin.Size = new System.Drawing.Size(72, 16);
            this.cbPropJoin.TabIndex = 11;
            this.cbPropJoin.Text = "属性关联";
            this.cbPropJoin.UseVisualStyleBackColor = true;
            this.cbPropJoin.CheckedChanged += new System.EventHandler(this.cbPropJoin_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "属性关联图层";
            // 
            // cbProPlgFCName
            // 
            this.cbProPlgFCName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProPlgFCName.FormattingEnabled = true;
            this.cbProPlgFCName.Location = new System.Drawing.Point(6, 65);
            this.cbProPlgFCName.Name = "cbProPlgFCName";
            this.cbProPlgFCName.Size = new System.Drawing.Size(258, 20);
            this.cbProPlgFCName.TabIndex = 11;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBox1);
            this.groupBox2.Controls.Add(this.cbBOUA);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(12, 105);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(320, 118);
            this.groupBox2.TabIndex = 30;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "输出图层";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(7, 77);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(96, 16);
            this.checkBox1.TabIndex = 30;
            this.checkBox1.Text = "输出到临时层";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // BOULConstructBOUAFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(336, 412);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnShpFile);
            this.Controls.Add(this.tbShpFileName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbTYPE);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbBOUL);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BOULConstructBOUAFrm";
            this.Text = "境界线构面";
            this.Load += new System.EventHandler(this.BOULConstructBOUAFrm_Load);
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbBOUL;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbTYPE;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbBOUA;
        private System.Windows.Forms.Button btnShpFile;
        private System.Windows.Forms.TextBox tbShpFileName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cbPropJoin;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbProPlgFCName;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}