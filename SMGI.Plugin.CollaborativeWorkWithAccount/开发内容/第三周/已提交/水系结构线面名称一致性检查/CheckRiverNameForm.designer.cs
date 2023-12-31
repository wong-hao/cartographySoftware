﻿namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    partial class CheckRiverNameForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnFilePath = new System.Windows.Forms.Button();
            this.tbOutFilePath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tblen = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.roadLyrNameComboBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkFieldComboBox = new System.Windows.Forms.ComboBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tblen)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 177);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(355, 31);
            this.panel1.TabIndex = 40;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(214, 4);
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
            this.panel2.Location = new System.Drawing.Point(278, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(287, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 130);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 39;
            this.label1.Text = "输出路径";
            // 
            // btnFilePath
            // 
            this.btnFilePath.Location = new System.Drawing.Point(284, 145);
            this.btnFilePath.Name = "btnFilePath";
            this.btnFilePath.Size = new System.Drawing.Size(61, 23);
            this.btnFilePath.TabIndex = 38;
            this.btnFilePath.Text = "选择";
            this.btnFilePath.UseVisualStyleBackColor = true;
            this.btnFilePath.Click += new System.EventHandler(this.btnFilePath_Click);
            // 
            // tbOutFilePath
            // 
            this.tbOutFilePath.Location = new System.Drawing.Point(13, 145);
            this.tbOutFilePath.Name = "tbOutFilePath";
            this.tbOutFilePath.ReadOnly = true;
            this.tbOutFilePath.Size = new System.Drawing.Size(265, 21);
            this.tbOutFilePath.TabIndex = 37;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 12);
            this.label2.TabIndex = 41;
            this.label2.Text = "水系结构线长度限差";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(140, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 41;
            this.label3.Text = "米";
            // 
            // tblen
            // 
            this.tblen.Location = new System.Drawing.Point(14, 25);
            this.tblen.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.tblen.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.tblen.Name = "tblen";
            this.tblen.Size = new System.Drawing.Size(120, 21);
            this.tblen.TabIndex = 42;
            this.tblen.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.tblen.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 44;
            this.label4.Text = "选择水系线图层";
            // 
            // roadLyrNameComboBox
            // 
            this.roadLyrNameComboBox.FormattingEnabled = true;
            this.roadLyrNameComboBox.Location = new System.Drawing.Point(108, 60);
            this.roadLyrNameComboBox.Name = "roadLyrNameComboBox";
            this.roadLyrNameComboBox.Size = new System.Drawing.Size(121, 20);
            this.roadLyrNameComboBox.TabIndex = 43;
            this.roadLyrNameComboBox.SelectedIndexChanged += new System.EventHandler(this.roadLyrNameComboBox_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 97);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 45;
            this.label5.Text = "选择名称字段";
            // 
            // checkFieldComboBox
            // 
            this.checkFieldComboBox.FormattingEnabled = true;
            this.checkFieldComboBox.Location = new System.Drawing.Point(108, 94);
            this.checkFieldComboBox.Name = "checkFieldComboBox";
            this.checkFieldComboBox.Size = new System.Drawing.Size(121, 20);
            this.checkFieldComboBox.TabIndex = 46;
            this.checkFieldComboBox.SelectedIndexChanged += new System.EventHandler(this.checkFieldComboBox_SelectedIndexChanged);
            // 
            // CheckRiverNameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(355, 208);
            this.Controls.Add(this.checkFieldComboBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.roadLyrNameComboBox);
            this.Controls.Add(this.tblen);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnFilePath);
            this.Controls.Add(this.tbOutFilePath);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "CheckRiverNameForm";
            this.Text = "水系结构线线面名称一致性检查";
            this.Load += new System.EventHandler(this.CheckRiverNameForm_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tblen)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnFilePath;
        private System.Windows.Forms.TextBox tbOutFilePath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown tblen;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox roadLyrNameComboBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox checkFieldComboBox;
    }
}