namespace SMGI.Plugin.DCDProcess
{
    partial class FrmSimplify
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
            this.btCancel = new DevExpress.XtraEditors.SimpleButton();
            this.btOK = new DevExpress.XtraEditors.SimpleButton();
            this.txtBendDeepth = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBendWidth = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(244, 99);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 33);
            this.btCancel.TabIndex = 35;
            this.btCancel.Text = "取消";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(136, 99);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(75, 33);
            this.btOK.TabIndex = 34;
            this.btOK.Text = "确定";
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // txtBendDeepth
            // 
            this.txtBendDeepth.Location = new System.Drawing.Point(124, 19);
            this.txtBendDeepth.Name = "txtBendDeepth";
            this.txtBendDeepth.Size = new System.Drawing.Size(210, 22);
            this.txtBendDeepth.TabIndex = 33;
            this.txtBendDeepth.Text = "100";
            this.txtBendDeepth.TextChanged += new System.EventHandler(this.txtBendDeepth_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 14);
            this.label2.TabIndex = 31;
            this.label2.Text = "最小开口宽度（米）";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 14);
            this.label1.TabIndex = 30;
            this.label1.Text = "最小弯曲深度(米）";
            // 
            // txtBendWidth
            // 
            this.txtBendWidth.Location = new System.Drawing.Point(124, 53);
            this.txtBendWidth.Name = "txtBendWidth";
            this.txtBendWidth.Size = new System.Drawing.Size(210, 22);
            this.txtBendWidth.TabIndex = 36;
            this.txtBendWidth.Text = "100";
            this.txtBendWidth.TextChanged += new System.EventHandler(this.txtBendWidth_TextChanged);
            // 
            // FrmSimplify
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 156);
            this.Controls.Add(this.txtBendWidth);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.txtBendDeepth);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "FrmSimplify";
            this.Text = "要素边弯曲化简设置";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FrmSimplify_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton btCancel;
        private DevExpress.XtraEditors.SimpleButton btOK;
        private System.Windows.Forms.TextBox txtBendDeepth;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBendWidth;
    }
}