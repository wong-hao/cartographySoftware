namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    partial class CheckRoadAndRiverSymbolConflictForm
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
            this.cbObjectLayer = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnFilePath = new System.Windows.Forms.Button();
            this.tbOutFilePath = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tbScale = new System.Windows.Forms.TextBox();
            this.cbConnLayer = new System.Windows.Forms.ComboBox();
            this.tbMinDistance = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 40;
            this.label1.Text = "目标图层";
            // 
            // cbObjectLayer
            // 
            this.cbObjectLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbObjectLayer.FormattingEnabled = true;
            this.cbObjectLayer.Location = new System.Drawing.Point(14, 24);
            this.cbObjectLayer.Name = "cbObjectLayer";
            this.cbObjectLayer.Size = new System.Drawing.Size(382, 20);
            this.cbObjectLayer.TabIndex = 41;
            this.cbObjectLayer.SelectedIndexChanged += new System.EventHandler(this.cbObjectLayer_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 40;
            this.label2.Text = "相关图层";
            // 
            // btnFilePath
            // 
            this.btnFilePath.Location = new System.Drawing.Point(335, 239);
            this.btnFilePath.Name = "btnFilePath";
            this.btnFilePath.Size = new System.Drawing.Size(61, 23);
            this.btnFilePath.TabIndex = 47;
            this.btnFilePath.Text = "浏览";
            this.btnFilePath.UseVisualStyleBackColor = true;
            this.btnFilePath.Click += new System.EventHandler(this.btnFilePath_Click);
            // 
            // tbOutFilePath
            // 
            this.tbOutFilePath.Location = new System.Drawing.Point(12, 239);
            this.tbOutFilePath.Name = "tbOutFilePath";
            this.tbOutFilePath.ReadOnly = true;
            this.tbOutFilePath.Size = new System.Drawing.Size(317, 21);
            this.tbOutFilePath.TabIndex = 46;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 224);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 45;
            this.label5.Text = "输出路径";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 276);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(411, 31);
            this.panel1.TabIndex = 48;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(270, 4);
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
            this.panel2.Location = new System.Drawing.Point(334, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(343, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 118);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 45;
            this.label3.Text = "参考比例尺";
            // 
            // tbScale
            // 
            this.tbScale.Location = new System.Drawing.Point(14, 134);
            this.tbScale.Name = "tbScale";
            this.tbScale.ReadOnly = true;
            this.tbScale.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tbScale.Size = new System.Drawing.Size(382, 21);
            this.tbScale.TabIndex = 49;
            this.tbScale.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // cbConnLayer
            // 
            this.cbConnLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbConnLayer.FormattingEnabled = true;
            this.cbConnLayer.Location = new System.Drawing.Point(12, 75);
            this.cbConnLayer.Name = "cbConnLayer";
            this.cbConnLayer.Size = new System.Drawing.Size(382, 20);
            this.cbConnLayer.TabIndex = 41;
            this.cbConnLayer.SelectedIndexChanged += new System.EventHandler(this.cbObjectLayer_SelectedIndexChanged);
            // 
            // tbMinDistance
            // 
            this.tbMinDistance.Location = new System.Drawing.Point(72, 182);
            this.tbMinDistance.Name = "tbMinDistance";
            this.tbMinDistance.Size = new System.Drawing.Size(136, 21);
            this.tbMinDistance.TabIndex = 52;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(214, 185);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(11, 12);
            this.label6.TabIndex = 51;
            this.label6.Text = "m";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 185);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 50;
            this.label4.Text = "最小间隔:";
            // 
            // CheckRoadAndRiverSymbolConflictForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(411, 307);
            this.Controls.Add(this.tbMinDistance);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbScale);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnFilePath);
            this.Controls.Add(this.tbOutFilePath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cbConnLayer);
            this.Controls.Add(this.cbObjectLayer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CheckRoadAndRiverSymbolConflictForm";
            this.Text = "道路与水系符号冲突检查";
            this.Load += new System.EventHandler(this.CheckRoadAndRiverSymbolConflictForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbObjectLayer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnFilePath;
        private System.Windows.Forms.TextBox tbOutFilePath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbScale;
        private System.Windows.Forms.ComboBox cbConnLayer;
        private System.Windows.Forms.TextBox tbMinDistance;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
    }
}