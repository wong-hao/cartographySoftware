namespace SMGI.Plugin.DCDProcess.DataProcess
{
    partial class FrmRiverClass
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
            this.cmbHydlLayerSelect = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btCancel = new DevExpress.XtraEditors.SimpleButton();
            this.btOK = new DevExpress.XtraEditors.SimpleButton();
            this.SuspendLayout();
            // 
            // cmbHydlLayerSelect
            // 
            this.cmbHydlLayerSelect.FormattingEnabled = true;
            this.cmbHydlLayerSelect.Location = new System.Drawing.Point(92, 25);
            this.cmbHydlLayerSelect.Name = "cmbHydlLayerSelect";
            this.cmbHydlLayerSelect.Size = new System.Drawing.Size(189, 22);
            this.cmbHydlLayerSelect.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 14);
            this.label1.TabIndex = 7;
            this.label1.Text = "HYDL图层";
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(193, 73);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 33);
            this.btCancel.TabIndex = 23;
            this.btCancel.Text = "取消";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(65, 73);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(75, 33);
            this.btOK.TabIndex = 22;
            this.btOK.Text = "确定";
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // FrmRiverClass
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(302, 130);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.cmbHydlLayerSelect);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "FrmRiverClass";
            this.Text = "河流分级设置";
            this.Load += new System.EventHandler(this.FrmRiverClass_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbHydlLayerSelect;
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraEditors.SimpleButton btCancel;
        private DevExpress.XtraEditors.SimpleButton btOK;
    }
}