namespace SMGI.Plugin.DCDProcess
{
    partial class RasterRenderForm
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
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.gbRender = new System.Windows.Forms.GroupBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnNoDataColor = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.BackGroundPan = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.btnBGColor = new System.Windows.Forms.Button();
            this.tbColorB = new System.Windows.Forms.TextBox();
            this.tbColorG = new System.Windows.Forms.TextBox();
            this.tbColorR = new System.Windows.Forms.TextBox();
            this.cbBackground = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.gbRender.SuspendLayout();
            this.panel3.SuspendLayout();
            this.BackGroundPan.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 113);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5);
            this.panel1.Size = new System.Drawing.Size(390, 36);
            this.panel1.TabIndex = 3;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(225, 5);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(75, 26);
            this.btOK.TabIndex = 7;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(300, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(10, 26);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(310, 5);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 26);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // gbRender
            // 
            this.gbRender.Controls.Add(this.panel3);
            this.gbRender.Controls.Add(this.BackGroundPan);
            this.gbRender.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbRender.Location = new System.Drawing.Point(0, 0);
            this.gbRender.Name = "gbRender";
            this.gbRender.Size = new System.Drawing.Size(390, 94);
            this.gbRender.TabIndex = 4;
            this.gbRender.TabStop = false;
            this.gbRender.Text = "渲染";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnNoDataColor);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(3, 52);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(384, 36);
            this.panel3.TabIndex = 10;
            // 
            // btnNoDataColor
            // 
            this.btnNoDataColor.BackColor = System.Drawing.SystemColors.Control;
            this.btnNoDataColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNoDataColor.Location = new System.Drawing.Point(324, 8);
            this.btnNoDataColor.Name = "btnNoDataColor";
            this.btnNoDataColor.Size = new System.Drawing.Size(52, 23);
            this.btnNoDataColor.TabIndex = 11;
            this.btnNoDataColor.UseVisualStyleBackColor = false;
            this.btnNoDataColor.Click += new System.EventHandler(this.btnNoDataColor_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(229, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 12;
            this.label1.Text = "显示NoData值为";
            // 
            // BackGroundPan
            // 
            this.BackGroundPan.Controls.Add(this.label2);
            this.BackGroundPan.Controls.Add(this.btnBGColor);
            this.BackGroundPan.Controls.Add(this.tbColorB);
            this.BackGroundPan.Controls.Add(this.tbColorG);
            this.BackGroundPan.Controls.Add(this.tbColorR);
            this.BackGroundPan.Controls.Add(this.cbBackground);
            this.BackGroundPan.Dock = System.Windows.Forms.DockStyle.Top;
            this.BackGroundPan.Location = new System.Drawing.Point(3, 17);
            this.BackGroundPan.Name = "BackGroundPan";
            this.BackGroundPan.Size = new System.Drawing.Size(384, 35);
            this.BackGroundPan.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(301, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "为";
            // 
            // btnBGColor
            // 
            this.btnBGColor.BackColor = System.Drawing.SystemColors.Control;
            this.btnBGColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBGColor.Location = new System.Drawing.Point(324, 4);
            this.btnBGColor.Name = "btnBGColor";
            this.btnBGColor.Size = new System.Drawing.Size(52, 23);
            this.btnBGColor.TabIndex = 2;
            this.btnBGColor.UseVisualStyleBackColor = false;
            this.btnBGColor.Click += new System.EventHandler(this.btnBGColor_Click);
            // 
            // tbColorB
            // 
            this.tbColorB.Location = new System.Drawing.Point(247, 4);
            this.tbColorB.Name = "tbColorB";
            this.tbColorB.Size = new System.Drawing.Size(42, 21);
            this.tbColorB.TabIndex = 1;
            // 
            // tbColorG
            // 
            this.tbColorG.Location = new System.Drawing.Point(199, 3);
            this.tbColorG.Name = "tbColorG";
            this.tbColorG.Size = new System.Drawing.Size(42, 21);
            this.tbColorG.TabIndex = 1;
            // 
            // tbColorR
            // 
            this.tbColorR.Location = new System.Drawing.Point(151, 2);
            this.tbColorR.Name = "tbColorR";
            this.tbColorR.Size = new System.Drawing.Size(42, 21);
            this.tbColorR.TabIndex = 1;
            // 
            // cbBackground
            // 
            this.cbBackground.AutoSize = true;
            this.cbBackground.Location = new System.Drawing.Point(13, 4);
            this.cbBackground.Name = "cbBackground";
            this.cbBackground.Size = new System.Drawing.Size(132, 16);
            this.cbBackground.TabIndex = 0;
            this.cbBackground.Text = "显示背景值(R,G,B):";
            this.cbBackground.UseVisualStyleBackColor = true;
            this.cbBackground.CheckedChanged += new System.EventHandler(this.cbBackground_CheckedChanged);
            // 
            // RasterRenderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 149);
            this.Controls.Add(this.gbRender);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RasterRenderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "图层属性";
            this.panel1.ResumeLayout(false);
            this.gbRender.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.BackGroundPan.ResumeLayout(false);
            this.BackGroundPan.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.GroupBox gbRender;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnNoDataColor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel BackGroundPan;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnBGColor;
        private System.Windows.Forms.TextBox tbColorB;
        private System.Windows.Forms.TextBox tbColorG;
        private System.Windows.Forms.TextBox tbColorR;
        private System.Windows.Forms.CheckBox cbBackground;
    }
}