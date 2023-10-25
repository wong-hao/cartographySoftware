namespace SMGI.Plugin.DCDProcess
{
    partial class LayerLabelSetForm
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
            this.btn = new System.Windows.Forms.Button();
            this.lab = new System.Windows.Forms.Label();
            this.cbb = new System.Windows.Forms.ComboBox();
            this.chb = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btn
            // 
            this.btn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn.Location = new System.Drawing.Point(161, 60);
            this.btn.Name = "btn";
            this.btn.Size = new System.Drawing.Size(60, 23);
            this.btn.TabIndex = 0;
            this.btn.Text = "确  定";
            this.btn.UseVisualStyleBackColor = true;
            this.btn.Click += new System.EventHandler(this.btn_Click);
            // 
            // lab
            // 
            this.lab.AutoSize = true;
            this.lab.Location = new System.Drawing.Point(15, 26);
            this.lab.Name = "lab";
            this.lab.Size = new System.Drawing.Size(65, 12);
            this.lab.TabIndex = 4;
            this.lab.Text = "标注属性：";
            // 
            // cbb
            // 
            this.cbb.FormattingEnabled = true;
            this.cbb.Location = new System.Drawing.Point(82, 23);
            this.cbb.Name = "cbb";
            this.cbb.Size = new System.Drawing.Size(139, 20);
            this.cbb.TabIndex = 5;
            // 
            // chb
            // 
            this.chb.AutoSize = true;
            this.chb.Location = new System.Drawing.Point(17, 64);
            this.chb.Name = "chb";
            this.chb.Size = new System.Drawing.Size(72, 16);
            this.chb.TabIndex = 6;
            this.chb.Text = "是否标注";
            this.chb.UseVisualStyleBackColor = true;
            // 
            // LayerLabelSetForm
            // 
            this.AcceptButton = this.btn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(238, 95);
            this.Controls.Add(this.chb);
            this.Controls.Add(this.cbb);
            this.Controls.Add(this.lab);
            this.Controls.Add(this.btn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "LayerLabelSetForm";
            this.Text = "标注设置";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn;
        private System.Windows.Forms.Label lab;
        private System.Windows.Forms.ComboBox cbb;
        private System.Windows.Forms.CheckBox chb;
    }
}