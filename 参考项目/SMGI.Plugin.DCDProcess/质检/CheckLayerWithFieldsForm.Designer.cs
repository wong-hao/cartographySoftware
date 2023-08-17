namespace SMGI.Plugin.DCDProcess
{
    partial class CheckLayerWithFieldsForm
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
            this.cbLayerName = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkFieldList = new System.Windows.Forms.CheckedListBox();
            this.tbOutFilePath = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnOutputPath = new System.Windows.Forms.Button();
            this.btnUnselAll = new System.Windows.Forms.Button();
            this.btnSelAll = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 360);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(390, 31);
            this.panel1.TabIndex = 16;
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 21;
            this.label1.Text = "图层名";
            // 
            // cbLayerName
            // 
            this.cbLayerName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbLayerName.FormattingEnabled = true;
            this.cbLayerName.Location = new System.Drawing.Point(14, 24);
            this.cbLayerName.Name = "cbLayerName";
            this.cbLayerName.Size = new System.Drawing.Size(361, 20);
            this.cbLayerName.TabIndex = 22;
            this.cbLayerName.SelectedIndexChanged += new System.EventHandler(this.cbLayerName_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 23;
            this.label2.Text = "待检查字段";
            // 
            // chkFieldList
            // 
            this.chkFieldList.CheckOnClick = true;
            this.chkFieldList.ColumnWidth = 165;
            this.chkFieldList.FormattingEnabled = true;
            this.chkFieldList.Location = new System.Drawing.Point(12, 72);
            this.chkFieldList.MultiColumn = true;
            this.chkFieldList.Name = "chkFieldList";
            this.chkFieldList.Size = new System.Drawing.Size(363, 196);
            this.chkFieldList.TabIndex = 24;
            // 
            // tbOutFilePath
            // 
            this.tbOutFilePath.Location = new System.Drawing.Point(12, 324);
            this.tbOutFilePath.Name = "tbOutFilePath";
            this.tbOutFilePath.ReadOnly = true;
            this.tbOutFilePath.Size = new System.Drawing.Size(316, 21);
            this.tbOutFilePath.TabIndex = 34;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 309);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 33;
            this.label5.Text = "输出路径";
            // 
            // btnOutputPath
            // 
            this.btnOutputPath.Location = new System.Drawing.Point(336, 324);
            this.btnOutputPath.Name = "btnOutputPath";
            this.btnOutputPath.Size = new System.Drawing.Size(39, 23);
            this.btnOutputPath.TabIndex = 32;
            this.btnOutputPath.Text = "选择";
            this.btnOutputPath.UseVisualStyleBackColor = true;
            this.btnOutputPath.Click += new System.EventHandler(this.btnOutputPath_Click);
            // 
            // btnUnselAll
            // 
            this.btnUnselAll.Location = new System.Drawing.Point(93, 274);
            this.btnUnselAll.Name = "btnUnselAll";
            this.btnUnselAll.Size = new System.Drawing.Size(75, 23);
            this.btnUnselAll.TabIndex = 36;
            this.btnUnselAll.Text = "取消全选";
            this.btnUnselAll.UseVisualStyleBackColor = true;
            this.btnUnselAll.Click += new System.EventHandler(this.btnUnselAll_Click);
            // 
            // btnSelAll
            // 
            this.btnSelAll.Location = new System.Drawing.Point(12, 274);
            this.btnSelAll.Name = "btnSelAll";
            this.btnSelAll.Size = new System.Drawing.Size(75, 23);
            this.btnSelAll.TabIndex = 35;
            this.btnSelAll.Text = "全选";
            this.btnSelAll.UseVisualStyleBackColor = true;
            this.btnSelAll.Click += new System.EventHandler(this.btnSelAll_Click);
            // 
            // CheckLayerWithFieldsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 391);
            this.Controls.Add(this.btnUnselAll);
            this.Controls.Add(this.btnSelAll);
            this.Controls.Add(this.tbOutFilePath);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnOutputPath);
            this.Controls.Add(this.chkFieldList);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbLayerName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CheckLayerWithFieldsForm";
            this.Text = "CheckLayerWithFieldsForm";
            this.Load += new System.EventHandler(this.CheckLayerWithFieldsForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbLayerName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckedListBox chkFieldList;
        private System.Windows.Forms.TextBox tbOutFilePath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnOutputPath;
        private System.Windows.Forms.Button btnUnselAll;
        private System.Windows.Forms.Button btnSelAll;
    }
}