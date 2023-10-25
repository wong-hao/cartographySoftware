namespace SMGI.Plugin.DCDProcess
{
    partial class CheckPropertiesConsistencyOfSameFeatureForm
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
            this.cbLayerName = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbObjFieldName = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkReferFieldList = new System.Windows.Forms.CheckedListBox();
            this.tbOutFilePath = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnOutputPath = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.cbExcept = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbLayerName
            // 
            this.cbLayerName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbLayerName.FormattingEnabled = true;
            this.cbLayerName.Location = new System.Drawing.Point(14, 24);
            this.cbLayerName.Name = "cbLayerName";
            this.cbLayerName.Size = new System.Drawing.Size(361, 20);
            this.cbLayerName.TabIndex = 24;
            this.cbLayerName.SelectedIndexChanged += new System.EventHandler(this.cbLayerName_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 23;
            this.label1.Text = "待检查图层名";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 23;
            this.label2.Text = "待检查字段";
            // 
            // cbObjFieldName
            // 
            this.cbObjFieldName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbObjFieldName.FormattingEnabled = true;
            this.cbObjFieldName.Location = new System.Drawing.Point(14, 73);
            this.cbObjFieldName.Name = "cbObjFieldName";
            this.cbObjFieldName.Size = new System.Drawing.Size(159, 20);
            this.cbObjFieldName.TabIndex = 24;
            this.cbObjFieldName.SelectedIndexChanged += new System.EventHandler(this.cbObjFieldName_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 25;
            this.label3.Text = "参考字段";
            // 
            // chkReferFieldList
            // 
            this.chkReferFieldList.CheckOnClick = true;
            this.chkReferFieldList.ColumnWidth = 165;
            this.chkReferFieldList.FormattingEnabled = true;
            this.chkReferFieldList.Location = new System.Drawing.Point(12, 126);
            this.chkReferFieldList.MultiColumn = true;
            this.chkReferFieldList.Name = "chkReferFieldList";
            this.chkReferFieldList.Size = new System.Drawing.Size(363, 164);
            this.chkReferFieldList.TabIndex = 26;
            // 
            // tbOutFilePath
            // 
            this.tbOutFilePath.Location = new System.Drawing.Point(14, 349);
            this.tbOutFilePath.Name = "tbOutFilePath";
            this.tbOutFilePath.ReadOnly = true;
            this.tbOutFilePath.Size = new System.Drawing.Size(316, 21);
            this.tbOutFilePath.TabIndex = 37;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 334);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 36;
            this.label5.Text = "输出路径";
            // 
            // btnOutputPath
            // 
            this.btnOutputPath.Location = new System.Drawing.Point(338, 349);
            this.btnOutputPath.Name = "btnOutputPath";
            this.btnOutputPath.Size = new System.Drawing.Size(39, 23);
            this.btnOutputPath.TabIndex = 35;
            this.btnOutputPath.Text = "选择";
            this.btnOutputPath.UseVisualStyleBackColor = true;
            this.btnOutputPath.Click += new System.EventHandler(this.btnOutputPath_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 391);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(390, 31);
            this.panel1.TabIndex = 38;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(249, 4);
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
            this.panel2.Location = new System.Drawing.Point(313, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(322, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // cbExcept
            // 
            this.cbExcept.AutoSize = true;
            this.cbExcept.Checked = true;
            this.cbExcept.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbExcept.Location = new System.Drawing.Point(14, 297);
            this.cbExcept.Name = "cbExcept";
            this.cbExcept.Size = new System.Drawing.Size(300, 16);
            this.cbExcept.TabIndex = 39;
            this.cbExcept.Text = "排除参考字段属性值包含空值或空字符串的要素分组";
            this.cbExcept.UseVisualStyleBackColor = true;
            // 
            // CheckPropertiesConsistencyOfSameFeatureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 422);
            this.Controls.Add(this.cbExcept);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tbOutFilePath);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnOutputPath);
            this.Controls.Add(this.chkReferFieldList);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbObjFieldName);
            this.Controls.Add(this.cbLayerName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CheckPropertiesConsistencyOfSameFeatureForm";
            this.Text = "要素属性一致性检查";
            this.Load += new System.EventHandler(this.PropertiesConsistencyOfSameFeatureForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbLayerName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbObjFieldName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckedListBox chkReferFieldList;
        private System.Windows.Forms.TextBox tbOutFilePath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnOutputPath;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.CheckBox cbExcept;
    }
}