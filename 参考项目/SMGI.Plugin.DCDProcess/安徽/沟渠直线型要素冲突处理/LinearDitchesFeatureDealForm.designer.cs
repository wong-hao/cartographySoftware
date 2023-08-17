namespace SMGI.Plugin.DCDProcess
{
    partial class LinearDitchesFeatureDealForm
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
            this.Close = new System.Windows.Forms.Button();
            this.OktBtn = new System.Windows.Forms.Button();
            this.btRight = new System.Windows.Forms.Button();
            this.btLeft = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.MoveDistanceText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Close
            // 
            this.Close.Location = new System.Drawing.Point(182, 214);
            this.Close.Name = "Close";
            this.Close.Size = new System.Drawing.Size(51, 25);
            this.Close.TabIndex = 132;
            this.Close.Text = "取消";
            this.Close.UseVisualStyleBackColor = true;
            this.Close.Click += new System.EventHandler(this.Close_Click);
            // 
            // OktBtn
            // 
            this.OktBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OktBtn.Location = new System.Drawing.Point(57, 214);
            this.OktBtn.Name = "OktBtn";
            this.OktBtn.Size = new System.Drawing.Size(51, 25);
            this.OktBtn.TabIndex = 131;
            this.OktBtn.Text = "确定";
            this.OktBtn.UseVisualStyleBackColor = true;
            this.OktBtn.Click += new System.EventHandler(this.OktBtn_Click);
            // 
            // btRight
            // 
            this.btRight.ImageIndex = 2;
            this.btRight.Location = new System.Drawing.Point(201, 95);
            this.btRight.Name = "btRight";
            this.btRight.Size = new System.Drawing.Size(42, 42);
            this.btRight.TabIndex = 134;
            this.btRight.Text = "线路右偏";
            this.btRight.UseVisualStyleBackColor = true;
            this.btRight.Click += new System.EventHandler(this.btRight_Click);
            // 
            // btLeft
            // 
            this.btLeft.ImageIndex = 1;
            this.btLeft.Location = new System.Drawing.Point(48, 95);
            this.btLeft.Name = "btLeft";
            this.btLeft.Size = new System.Drawing.Size(42, 42);
            this.btLeft.TabIndex = 133;
            this.btLeft.Text = "线路左偏";
            this.btLeft.UseVisualStyleBackColor = true;
            this.btLeft.Click += new System.EventHandler(this.btLeft_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(207, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 136;
            this.label1.Text = "mm";
            // 
            // MoveDistanceText
            // 
            this.MoveDistanceText.Location = new System.Drawing.Point(131, 21);
            this.MoveDistanceText.Name = "MoveDistanceText";
            this.MoveDistanceText.Size = new System.Drawing.Size(70, 21);
            this.MoveDistanceText.TabIndex = 135;
            this.MoveDistanceText.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(60, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 137;
            this.label2.Text = "移动距离：";
            // 
            // LinearDitchesFeatureDealForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.MoveDistanceText);
            this.Controls.Add(this.btRight);
            this.Controls.Add(this.btLeft);
            this.Controls.Add(this.Close);
            this.Controls.Add(this.OktBtn);
            this.Name = "LinearDitchesFeatureDealForm";
            this.Text = "沟渠直线型要素冲突处理";
            this.Load += new System.EventHandler(this.LinearDitchesFeatureDealForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Close;
        private System.Windows.Forms.Button OktBtn;
        private System.Windows.Forms.Button btRight;
        private System.Windows.Forms.Button btLeft;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox MoveDistanceText;
        private System.Windows.Forms.Label label2;
    }
}