namespace SMGI.Plugin.DCDProcess
{
    partial class PolygonGapAdjustmentForm
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
            this.tbBufferValue = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbLayerNames = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.bufferRangeBtn = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "缓冲距离：";
            // 
            // tbBufferValue
            // 
            this.tbBufferValue.Location = new System.Drawing.Point(14, 24);
            this.tbBufferValue.Name = "tbBufferValue";
            this.tbBufferValue.Size = new System.Drawing.Size(191, 21);
            this.tbBufferValue.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(211, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "米";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "目标面图层：";
            // 
            // cbLayerNames
            // 
            this.cbLayerNames.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbLayerNames.FormattingEnabled = true;
            this.cbLayerNames.Location = new System.Drawing.Point(14, 73);
            this.cbLayerNames.Name = "cbLayerNames";
            this.cbLayerNames.Size = new System.Drawing.Size(214, 20);
            this.cbLayerNames.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.bufferRangeBtn);
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 126);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(240, 31);
            this.panel1.TabIndex = 12;
            // 
            // bufferRangeBtn
            // 
            this.bufferRangeBtn.Dock = System.Windows.Forms.DockStyle.Right;
            this.bufferRangeBtn.Location = new System.Drawing.Point(26, 4);
            this.bufferRangeBtn.Name = "bufferRangeBtn";
            this.bufferRangeBtn.Size = new System.Drawing.Size(64, 23);
            this.bufferRangeBtn.TabIndex = 14;
            this.bufferRangeBtn.Text = "缓冲预览";
            this.bufferRangeBtn.UseVisualStyleBackColor = true;
            this.bufferRangeBtn.Click += new System.EventHandler(this.bufferRangeBtn_Click);
            // 
            // panel3
            // 
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(90, 4);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(9, 23);
            this.panel3.TabIndex = 7;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(99, 4);
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
            this.panel2.Location = new System.Drawing.Point(163, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(172, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // PolygonGapAdjustmentForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(240, 157);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.cbLayerNames);
            this.Controls.Add(this.tbBufferValue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PolygonGapAdjustmentForm";
            this.Text = "面状间距快速调整";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PolygonGapAdjustmentForm_FormClosed);
            this.Load += new System.EventHandler(this.PolygonGapAdjustmentForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbBufferValue;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbLayerNames;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button bufferRangeBtn;
        private System.Windows.Forms.Panel panel3;
    }
}