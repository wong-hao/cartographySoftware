namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    partial class CheckReservoirNameCheckForm
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
            this.tbOutFilePath = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnOutputPath = new System.Windows.Forms.Button();
            this.checkFieldComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 96);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(381, 31);
            this.panel1.TabIndex = 15;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(240, 4);
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
            this.panel2.Location = new System.Drawing.Point(304, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(313, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // tbOutFilePath
            // 
            this.tbOutFilePath.Location = new System.Drawing.Point(14, 59);
            this.tbOutFilePath.Name = "tbOutFilePath";
            this.tbOutFilePath.ReadOnly = true;
            this.tbOutFilePath.Size = new System.Drawing.Size(316, 21);
            this.tbOutFilePath.TabIndex = 31;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 44);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 30;
            this.label5.Text = "输出路径";
            // 
            // btnOutputPath
            // 
            this.btnOutputPath.Location = new System.Drawing.Point(336, 57);
            this.btnOutputPath.Name = "btnOutputPath";
            this.btnOutputPath.Size = new System.Drawing.Size(39, 23);
            this.btnOutputPath.TabIndex = 29;
            this.btnOutputPath.Text = "选择";
            this.btnOutputPath.UseVisualStyleBackColor = true;
            this.btnOutputPath.Click += new System.EventHandler(this.btnOutputPath_Click);
            // 
            // checkFieldComboBox
            // 
            this.checkFieldComboBox.FormattingEnabled = true;
            this.checkFieldComboBox.Location = new System.Drawing.Point(107, 15);
            this.checkFieldComboBox.Name = "checkFieldComboBox";
            this.checkFieldComboBox.Size = new System.Drawing.Size(121, 20);
            this.checkFieldComboBox.TabIndex = 48;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 47;
            this.label1.Text = "选择属性字段";
            // 
            // CheckReservoirNameCheckForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(381, 127);
            this.Controls.Add(this.checkFieldComboBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbOutFilePath);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnOutputPath);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CheckReservoirNameCheckForm";
            this.Text = "水库名称一致性检查";
            this.Load += new System.EventHandler(this.CheckReservoirNameCheckForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.TextBox tbOutFilePath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnOutputPath;
        private System.Windows.Forms.ComboBox checkFieldComboBox;
        private System.Windows.Forms.Label label1;
    }
}