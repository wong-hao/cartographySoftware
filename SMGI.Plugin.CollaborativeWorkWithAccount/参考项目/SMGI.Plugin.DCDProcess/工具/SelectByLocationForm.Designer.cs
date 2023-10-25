namespace SMGI.Plugin.DCDProcess
{
    partial class SelectByLocationForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.cmbSelectMethods = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.clbObjFCList = new System.Windows.Forms.CheckedListBox();
            this.cbOnlyShowEnableSelectFC = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbSourceFC = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbUseSlectedFeature = new System.Windows.Forms.CheckBox();
            this.lbselFeatureCount = new System.Windows.Forms.Label();
            this.cmbobjFeatureSpatialSelectMethod = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Cursor = System.Windows.Forms.Cursors.Default;
            this.label1.Font = new System.Drawing.Font("宋体", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(7, 9);
            this.label1.MaximumSize = new System.Drawing.Size(415, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(397, 26);
            this.label1.TabIndex = 116;
            this.label1.Text = "依据要素相对于源图层中的要素的位置从一个或多个目标图层中选择\r\n要素。";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Cursor = System.Windows.Forms.Cursors.Default;
            this.label2.Font = new System.Drawing.Font("宋体", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(7, 49);
            this.label2.MaximumSize = new System.Drawing.Size(415, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 117;
            this.label2.Text = "选择方法:";
            // 
            // cmbSelectMethods
            // 
            this.cmbSelectMethods.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSelectMethods.FormattingEnabled = true;
            this.cmbSelectMethods.Items.AddRange(new object[] {
            "从以下图层中选择要素",
            "添加到当前在以下图层中选择的要素",
            "移除当前在以下图层中选择的要素",
            "从当前在以下图层中选择的要素中选择"});
            this.cmbSelectMethods.Location = new System.Drawing.Point(10, 65);
            this.cmbSelectMethods.Name = "cmbSelectMethods";
            this.cmbSelectMethods.Size = new System.Drawing.Size(394, 20);
            this.cmbSelectMethods.TabIndex = 118;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Cursor = System.Windows.Forms.Cursors.Default;
            this.label3.Font = new System.Drawing.Font("宋体", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(7, 97);
            this.label3.MaximumSize = new System.Drawing.Size(415, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 119;
            this.label3.Text = "目标图层:";
            // 
            // clbObjFCList
            // 
            this.clbObjFCList.CheckOnClick = true;
            this.clbObjFCList.FormattingEnabled = true;
            this.clbObjFCList.Location = new System.Drawing.Point(10, 113);
            this.clbObjFCList.MultiColumn = true;
            this.clbObjFCList.Name = "clbObjFCList";
            this.clbObjFCList.Size = new System.Drawing.Size(394, 196);
            this.clbObjFCList.TabIndex = 120;
            this.clbObjFCList.SelectedValueChanged += new System.EventHandler(this.clbObjFCList_SelectedValueChanged);
            // 
            // cbOnlyShowEnableSelectFC
            // 
            this.cbOnlyShowEnableSelectFC.AutoSize = true;
            this.cbOnlyShowEnableSelectFC.Checked = true;
            this.cbOnlyShowEnableSelectFC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbOnlyShowEnableSelectFC.Location = new System.Drawing.Point(10, 315);
            this.cbOnlyShowEnableSelectFC.Name = "cbOnlyShowEnableSelectFC";
            this.cbOnlyShowEnableSelectFC.Size = new System.Drawing.Size(186, 16);
            this.cbOnlyShowEnableSelectFC.TabIndex = 121;
            this.cbOnlyShowEnableSelectFC.Text = "在此列表中仅显示可选图层(O)";
            this.cbOnlyShowEnableSelectFC.UseVisualStyleBackColor = true;
            this.cbOnlyShowEnableSelectFC.Visible = false;
            this.cbOnlyShowEnableSelectFC.CheckedChanged += new System.EventHandler(this.cbOnlyShowEnableSelectFC_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Cursor = System.Windows.Forms.Cursors.Default;
            this.label4.Font = new System.Drawing.Font("宋体", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(7, 343);
            this.label4.MaximumSize = new System.Drawing.Size(415, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 122;
            this.label4.Text = "源图层:";
            // 
            // cmbSourceFC
            // 
            this.cmbSourceFC.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSourceFC.FormattingEnabled = true;
            this.cmbSourceFC.Location = new System.Drawing.Point(10, 359);
            this.cmbSourceFC.Name = "cmbSourceFC";
            this.cmbSourceFC.Size = new System.Drawing.Size(394, 20);
            this.cmbSourceFC.TabIndex = 123;
            this.cmbSourceFC.SelectedIndexChanged += new System.EventHandler(this.cmbSourceFC_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Cursor = System.Windows.Forms.Cursors.Default;
            this.label5.Font = new System.Drawing.Font("宋体", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(7, 421);
            this.label5.MaximumSize = new System.Drawing.Size(415, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(183, 13);
            this.label5.TabIndex = 124;
            this.label5.Text = "目标图层要素的空间选择方法:";
            // 
            // cbUseSlectedFeature
            // 
            this.cbUseSlectedFeature.AutoSize = true;
            this.cbUseSlectedFeature.Location = new System.Drawing.Point(10, 385);
            this.cbUseSlectedFeature.Name = "cbUseSlectedFeature";
            this.cbUseSlectedFeature.Size = new System.Drawing.Size(96, 16);
            this.cbUseSlectedFeature.TabIndex = 125;
            this.cbUseSlectedFeature.Text = "使用所选要素";
            this.cbUseSlectedFeature.UseVisualStyleBackColor = true;
            // 
            // lbselFeatureCount
            // 
            this.lbselFeatureCount.AutoSize = true;
            this.lbselFeatureCount.Cursor = System.Windows.Forms.Cursors.Default;
            this.lbselFeatureCount.Font = new System.Drawing.Font("宋体", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbselFeatureCount.Location = new System.Drawing.Point(205, 388);
            this.lbselFeatureCount.MaximumSize = new System.Drawing.Size(415, 0);
            this.lbselFeatureCount.Name = "lbselFeatureCount";
            this.lbselFeatureCount.Size = new System.Drawing.Size(106, 13);
            this.lbselFeatureCount.TabIndex = 126;
            this.lbselFeatureCount.Text = "(选择了 个要素)";
            // 
            // cmbobjFeatureSpatialSelectMethod
            // 
            this.cmbobjFeatureSpatialSelectMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbobjFeatureSpatialSelectMethod.FormattingEnabled = true;
            this.cmbobjFeatureSpatialSelectMethod.Items.AddRange(new object[] {
            "与源图层要素相交",
            "完全位于源图层要素范围内",
            "完全包含源图层要素",
            "与源图层要素共线",
            "与源图层要素完全相同"});
            this.cmbobjFeatureSpatialSelectMethod.Location = new System.Drawing.Point(10, 437);
            this.cmbobjFeatureSpatialSelectMethod.Name = "cmbobjFeatureSpatialSelectMethod";
            this.cmbobjFeatureSpatialSelectMethod.Size = new System.Drawing.Size(394, 20);
            this.cmbobjFeatureSpatialSelectMethod.TabIndex = 127;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 485);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Size = new System.Drawing.Size(416, 31);
            this.panel1.TabIndex = 128;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(275, 4);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(64, 23);
            this.btOK.TabIndex = 9;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // panel3
            // 
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(339, 4);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(9, 23);
            this.panel3.TabIndex = 8;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(348, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "关闭";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // SelectByLocationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(416, 516);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.cmbobjFeatureSpatialSelectMethod);
            this.Controls.Add(this.lbselFeatureCount);
            this.Controls.Add(this.cbUseSlectedFeature);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cmbSourceFC);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbOnlyShowEnableSelectFC);
            this.Controls.Add(this.clbObjFCList);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbSelectMethods);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectByLocationForm";
            this.Text = "按位置选择";
            this.Load += new System.EventHandler(this.SelectByLocationForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbSelectMethods;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckedListBox clbObjFCList;
        private System.Windows.Forms.CheckBox cbOnlyShowEnableSelectFC;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbSourceFC;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox cbUseSlectedFeature;
        private System.Windows.Forms.Label lbselFeatureCount;
        private System.Windows.Forms.ComboBox cmbobjFeatureSpatialSelectMethod;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btCancel;
    }
}