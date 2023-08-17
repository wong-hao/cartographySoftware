namespace SMGI.Plugin.DCDProcess
{
    partial class FrmMeasureResult
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
            this.label1 = new System.Windows.Forms.Label();
            this.rbArea = new System.Windows.Forms.RadioButton();
            this.rbLen = new System.Windows.Forms.RadioButton();
            this.rtbResult = new System.Windows.Forms.RichTextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.rbArea);
            this.panel1.Controls.Add(this.rbLen);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(303, 32);
            this.panel1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(126, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 12);
            this.label1.TabIndex = 1;
            // 
            // rbArea
            // 
            this.rbArea.AutoSize = true;
            this.rbArea.Location = new System.Drawing.Point(73, 9);
            this.rbArea.Name = "rbArea";
            this.rbArea.Size = new System.Drawing.Size(47, 16);
            this.rbArea.TabIndex = 0;
            this.rbArea.Text = "面积";
            this.rbArea.UseVisualStyleBackColor = true;
            // 
            // rbLen
            // 
            this.rbLen.AutoSize = true;
            this.rbLen.Checked = true;
            this.rbLen.Location = new System.Drawing.Point(11, 9);
            this.rbLen.Name = "rbLen";
            this.rbLen.Size = new System.Drawing.Size(47, 16);
            this.rbLen.TabIndex = 0;
            this.rbLen.TabStop = true;
            this.rbLen.Text = "距离";
            this.rbLen.UseVisualStyleBackColor = true;
            // 
            // rtbResult
            // 
            this.rtbResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbResult.Location = new System.Drawing.Point(0, 32);
            this.rtbResult.Name = "rtbResult";
            this.rtbResult.ReadOnly = true;
            this.rtbResult.Size = new System.Drawing.Size(303, 114);
            this.rtbResult.TabIndex = 2;
            this.rtbResult.Text = "";
            // 
            // FrmMeasureResult
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(303, 146);
            this.Controls.Add(this.rtbResult);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmMeasureResult";
            this.Text = "测量";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmMeasureResultcs_FormClosed);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RichTextBox rtbResult;
        private System.Windows.Forms.RadioButton rbArea;
        private System.Windows.Forms.RadioButton rbLen;
        private System.Windows.Forms.Label label1;

    }
}