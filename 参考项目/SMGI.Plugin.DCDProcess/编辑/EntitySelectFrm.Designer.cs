namespace SMGI.Plugin.DCDProcess
{
    partial class EntitySelectFrm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkFields = new System.Windows.Forms.CheckedListBox();
            this.cmbLayers = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkFields);
            this.groupBox1.Location = new System.Drawing.Point(14, 32);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(344, 153);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "匹配字段";
            // 
            // chkFields
            // 
            this.chkFields.CheckOnClick = true;
            this.chkFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkFields.FormattingEnabled = true;
            this.chkFields.Location = new System.Drawing.Point(3, 17);
            this.chkFields.MultiColumn = true;
            this.chkFields.Name = "chkFields";
            this.chkFields.Size = new System.Drawing.Size(338, 133);
            this.chkFields.TabIndex = 0;
            this.chkFields.SelectedValueChanged += new System.EventHandler(this.chkFields_SelectedValueChanged);
            // 
            // cmbLayers
            // 
            this.cmbLayers.FormattingEnabled = true;
            this.cmbLayers.Location = new System.Drawing.Point(71, 6);
            this.cmbLayers.Name = "cmbLayers";
            this.cmbLayers.Size = new System.Drawing.Size(287, 20);
            this.cmbLayers.TabIndex = 12;
            this.cmbLayers.SelectedIndexChanged += new System.EventHandler(this.cmbLayers_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 13;
            this.label1.Text = "当前图层";
            // 
            // EntitySelectFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(369, 207);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbLayers);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EntitySelectFrm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "实体选择设置";
            this.Load += new System.EventHandler(this.EntitySelectFrm_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckedListBox chkFields;
        private System.Windows.Forms.ComboBox cmbLayers;
        private System.Windows.Forms.Label label1;
    }
}