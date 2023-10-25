namespace SMGI.Plugin.DCDProcess
{
    partial class LayerSelectWithFiedsForm
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkedFieldNames = new System.Windows.Forms.CheckedListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.clbLayerList = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chkShp = new System.Windows.Forms.CheckBox();
            this.btnShape = new System.Windows.Forms.Button();
            this.shapetxt = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(109, 259);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 32);
            this.btnOK.TabIndex = 19;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(276, 259);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 32);
            this.btnCancel.TabIndex = 18;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkedFieldNames);
            this.groupBox1.Location = new System.Drawing.Point(12, 126);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(374, 129);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "参数设置";
            // 
            // checkedFieldNames
            // 
            this.checkedFieldNames.CheckOnClick = true;
            this.checkedFieldNames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedFieldNames.FormattingEnabled = true;
            this.checkedFieldNames.Location = new System.Drawing.Point(3, 17);
            this.checkedFieldNames.MultiColumn = true;
            this.checkedFieldNames.Name = "checkedFieldNames";
            this.checkedFieldNames.Size = new System.Drawing.Size(368, 109);
            this.checkedFieldNames.TabIndex = 25;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.clbLayerList);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(12, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(409, 54);
            this.groupBox2.TabIndex = 25;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "图层选取";
            // 
            // clbLayerList
            // 
            this.clbLayerList.FormattingEnabled = true;
            this.clbLayerList.Location = new System.Drawing.Point(125, 22);
            this.clbLayerList.Name = "clbLayerList";
            this.clbLayerList.Size = new System.Drawing.Size(183, 20);
            this.clbLayerList.TabIndex = 1;
            this.clbLayerList.SelectedIndexChanged += new System.EventHandler(this.clbLayerList_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(52, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "图层名称：";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chkShp);
            this.groupBox3.Controls.Add(this.btnShape);
            this.groupBox3.Controls.Add(this.shapetxt);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(12, 66);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(406, 55);
            this.groupBox3.TabIndex = 26;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "作业区范围文件";
            // 
            // chkShp
            // 
            this.chkShp.AutoSize = true;
            this.chkShp.Location = new System.Drawing.Point(54, 27);
            this.chkShp.Name = "chkShp";
            this.chkShp.Size = new System.Drawing.Size(15, 14);
            this.chkShp.TabIndex = 3;
            this.chkShp.UseVisualStyleBackColor = true;
            this.chkShp.CheckedChanged += new System.EventHandler(this.chkShp_CheckedChanged);
            // 
            // btnShape
            // 
            this.btnShape.Enabled = false;
            this.btnShape.Location = new System.Drawing.Point(341, 20);
            this.btnShape.Name = "btnShape";
            this.btnShape.Size = new System.Drawing.Size(49, 23);
            this.btnShape.TabIndex = 2;
            this.btnShape.Text = "打开";
            this.btnShape.UseVisualStyleBackColor = true;
            this.btnShape.Click += new System.EventHandler(this.btnShape_Click);
            // 
            // shapetxt
            // 
            this.shapetxt.Enabled = false;
            this.shapetxt.Location = new System.Drawing.Point(141, 22);
            this.shapetxt.Name = "shapetxt";
            this.shapetxt.Size = new System.Drawing.Size(195, 21);
            this.shapetxt.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Enabled = false;
            this.label1.Location = new System.Drawing.Point(73, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "shape文件：";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(389, 162);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(38, 25);
            this.button1.TabIndex = 27;
            this.button1.Text = "全选";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(389, 206);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(38, 25);
            this.button2.TabIndex = 27;
            this.button2.Text = "清空";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // LayerSelectWithFiedsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 303);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "LayerSelectWithFiedsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "图层选择器";
            this.Load += new System.EventHandler(this.LayerSelectForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.ComboBox clbLayerList;
        private System.Windows.Forms.CheckedListBox checkedFieldNames;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnShape;
        private System.Windows.Forms.TextBox shapetxt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkShp;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}