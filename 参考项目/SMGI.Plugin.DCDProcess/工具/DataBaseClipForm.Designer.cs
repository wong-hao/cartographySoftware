namespace SMGI.Plugin.DCDProcess
{
    partial class DataBaseClipForm
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbGDBFilePath = new System.Windows.Forms.TextBox();
            this.tbOutputPath = new System.Windows.Forms.TextBox();
            this.tbClipPolygonShpFileName = new System.Windows.Forms.TextBox();
            this.btnGDB = new System.Windows.Forms.Button();
            this.btOutputPath = new System.Windows.Forms.Button();
            this.btClip = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.cbSuffixFN = new System.Windows.Forms.ComboBox();
            this.CBDataBaseType = new System.Windows.Forms.ComboBox();
            this.cbNullFeatureClass = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btOK);
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Controls.Add(this.btCancel);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 308);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(4);
            this.panel2.Size = new System.Drawing.Size(526, 31);
            this.panel2.TabIndex = 15;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(385, 4);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(64, 23);
            this.btOK.TabIndex = 7;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // panel3
            // 
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(449, 4);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(9, 23);
            this.panel3.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(458, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 218);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 37;
            this.label2.Text = "输出路径";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 21;
            this.label3.Text = "目标地理数据库";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 114);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 21;
            this.label5.Text = "分割面要素类";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 20;
            this.label1.Text = "目标数据库类型";
            // 
            // tbGDBFilePath
            // 
            this.tbGDBFilePath.Location = new System.Drawing.Point(8, 79);
            this.tbGDBFilePath.Name = "tbGDBFilePath";
            this.tbGDBFilePath.ReadOnly = true;
            this.tbGDBFilePath.Size = new System.Drawing.Size(422, 21);
            this.tbGDBFilePath.TabIndex = 22;
            // 
            // tbOutputPath
            // 
            this.tbOutputPath.Location = new System.Drawing.Point(8, 235);
            this.tbOutputPath.Name = "tbOutputPath";
            this.tbOutputPath.ReadOnly = true;
            this.tbOutputPath.Size = new System.Drawing.Size(422, 21);
            this.tbOutputPath.TabIndex = 24;
            // 
            // tbClipPolygonShpFileName
            // 
            this.tbClipPolygonShpFileName.Location = new System.Drawing.Point(8, 130);
            this.tbClipPolygonShpFileName.Name = "tbClipPolygonShpFileName";
            this.tbClipPolygonShpFileName.ReadOnly = true;
            this.tbClipPolygonShpFileName.Size = new System.Drawing.Size(422, 21);
            this.tbClipPolygonShpFileName.TabIndex = 23;
            // 
            // btnGDB
            // 
            this.btnGDB.Location = new System.Drawing.Point(436, 77);
            this.btnGDB.Name = "btnGDB";
            this.btnGDB.Size = new System.Drawing.Size(61, 23);
            this.btnGDB.TabIndex = 25;
            this.btnGDB.Text = "打开";
            this.btnGDB.UseVisualStyleBackColor = true;
            this.btnGDB.Click += new System.EventHandler(this.btnGDB_Click);
            // 
            // btOutputPath
            // 
            this.btOutputPath.Location = new System.Drawing.Point(436, 233);
            this.btOutputPath.Name = "btOutputPath";
            this.btOutputPath.Size = new System.Drawing.Size(61, 23);
            this.btOutputPath.TabIndex = 27;
            this.btOutputPath.Text = "打开";
            this.btOutputPath.UseVisualStyleBackColor = true;
            this.btOutputPath.Click += new System.EventHandler(this.btOutputPath_Click);
            // 
            // btClip
            // 
            this.btClip.Location = new System.Drawing.Point(436, 128);
            this.btClip.Name = "btClip";
            this.btClip.Size = new System.Drawing.Size(61, 23);
            this.btClip.TabIndex = 26;
            this.btClip.Text = "打开";
            this.btClip.UseVisualStyleBackColor = true;
            this.btClip.Click += new System.EventHandler(this.btClip_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 167);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 12);
            this.label4.TabIndex = 37;
            this.label4.Text = "分割（分组）字段";
            // 
            // cbSuffixFN
            // 
            this.cbSuffixFN.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSuffixFN.FormattingEnabled = true;
            this.cbSuffixFN.Location = new System.Drawing.Point(8, 183);
            this.cbSuffixFN.Name = "cbSuffixFN";
            this.cbSuffixFN.Size = new System.Drawing.Size(489, 20);
            this.cbSuffixFN.TabIndex = 38;
            // 
            // CBDataBaseType
            // 
            this.CBDataBaseType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBDataBaseType.FormattingEnabled = true;
            this.CBDataBaseType.Items.AddRange(new object[] {
            "文件地理数据库(GDB)",
            "个人地理数据库(MDB)"});
            this.CBDataBaseType.Location = new System.Drawing.Point(8, 33);
            this.CBDataBaseType.Name = "CBDataBaseType";
            this.CBDataBaseType.Size = new System.Drawing.Size(489, 20);
            this.CBDataBaseType.TabIndex = 21;
            // 
            // cbNullFeatureClass
            // 
            this.cbNullFeatureClass.AutoSize = true;
            this.cbNullFeatureClass.Checked = true;
            this.cbNullFeatureClass.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbNullFeatureClass.Location = new System.Drawing.Point(8, 267);
            this.cbNullFeatureClass.Name = "cbNullFeatureClass";
            this.cbNullFeatureClass.Size = new System.Drawing.Size(96, 16);
            this.cbNullFeatureClass.TabIndex = 39;
            this.cbNullFeatureClass.Text = "保留空要素类";
            this.cbNullFeatureClass.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbNullFeatureClass);
            this.groupBox1.Controls.Add(this.CBDataBaseType);
            this.groupBox1.Controls.Add(this.cbSuffixFN);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.btClip);
            this.groupBox1.Controls.Add(this.btOutputPath);
            this.groupBox1.Controls.Add(this.btnGDB);
            this.groupBox1.Controls.Add(this.tbClipPolygonShpFileName);
            this.groupBox1.Controls.Add(this.tbOutputPath);
            this.groupBox1.Controls.Add(this.tbGDBFilePath);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Location = new System.Drawing.Point(7, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(507, 290);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            // 
            // DataBaseClipForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(526, 339);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DataBaseClipForm";
            this.Text = "数据库分割";
            this.Load += new System.EventHandler(this.DataBaseClipForm_Load);
            this.panel2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbGDBFilePath;
        private System.Windows.Forms.TextBox tbOutputPath;
        private System.Windows.Forms.TextBox tbClipPolygonShpFileName;
        private System.Windows.Forms.Button btnGDB;
        private System.Windows.Forms.Button btOutputPath;
        private System.Windows.Forms.Button btClip;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbSuffixFN;
        private System.Windows.Forms.ComboBox CBDataBaseType;
        private System.Windows.Forms.CheckBox cbNullFeatureClass;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}

