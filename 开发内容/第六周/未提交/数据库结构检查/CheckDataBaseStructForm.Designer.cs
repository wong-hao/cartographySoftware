namespace SMGI.Plugin.CollaborativeWorkWithAccount
{
    partial class CheckDataBaseStructForm
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
            this.btTemplate = new System.Windows.Forms.Button();
            this.btObj = new System.Windows.Forms.Button();
            this.tbTemplateDB = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbObjDB = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbOutFilePath = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btOutputPath = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 174);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(485, 31);
            this.panel1.TabIndex = 8;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(344, 4);
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
            this.panel2.Location = new System.Drawing.Point(408, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(417, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // btTemplate
            // 
            this.btTemplate.Location = new System.Drawing.Point(398, 79);
            this.btTemplate.Name = "btTemplate";
            this.btTemplate.Size = new System.Drawing.Size(75, 23);
            this.btTemplate.TabIndex = 13;
            this.btTemplate.Text = "选择";
            this.btTemplate.UseVisualStyleBackColor = true;
            this.btTemplate.Click += new System.EventHandler(this.btTemplate_Click);
            // 
            // btObj
            // 
            this.btObj.Location = new System.Drawing.Point(398, 25);
            this.btObj.Name = "btObj";
            this.btObj.Size = new System.Drawing.Size(75, 23);
            this.btObj.TabIndex = 14;
            this.btObj.Text = "选择";
            this.btObj.UseVisualStyleBackColor = true;
            this.btObj.Click += new System.EventHandler(this.btObj_Click);
            // 
            // tbTemplateDB
            // 
            this.tbTemplateDB.Location = new System.Drawing.Point(14, 79);
            this.tbTemplateDB.Name = "tbTemplateDB";
            this.tbTemplateDB.ReadOnly = true;
            this.tbTemplateDB.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tbTemplateDB.Size = new System.Drawing.Size(378, 21);
            this.tbTemplateDB.TabIndex = 12;
            this.tbTemplateDB.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "模板数据库";
            // 
            // tbObjDB
            // 
            this.tbObjDB.Location = new System.Drawing.Point(14, 25);
            this.tbObjDB.Name = "tbObjDB";
            this.tbObjDB.ReadOnly = true;
            this.tbObjDB.Size = new System.Drawing.Size(378, 21);
            this.tbObjDB.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 10;
            this.label1.Text = "目标数据库";
            // 
            // tbOutFilePath
            // 
            this.tbOutFilePath.Location = new System.Drawing.Point(14, 136);
            this.tbOutFilePath.Name = "tbOutFilePath";
            this.tbOutFilePath.ReadOnly = true;
            this.tbOutFilePath.Size = new System.Drawing.Size(378, 21);
            this.tbOutFilePath.TabIndex = 27;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 121);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 12);
            this.label5.TabIndex = 26;
            this.label5.Text = "检查结果输出路径";
            // 
            // btOutputPath
            // 
            this.btOutputPath.Location = new System.Drawing.Point(398, 134);
            this.btOutputPath.Name = "btOutputPath";
            this.btOutputPath.Size = new System.Drawing.Size(75, 23);
            this.btOutputPath.TabIndex = 13;
            this.btOutputPath.Text = "选择";
            this.btOutputPath.UseVisualStyleBackColor = true;
            this.btOutputPath.Click += new System.EventHandler(this.btOutputPath_Click);
            // 
            // CheckDataBaseStructForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 205);
            this.Controls.Add(this.tbOutFilePath);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btOutputPath);
            this.Controls.Add(this.btTemplate);
            this.Controls.Add(this.btObj);
            this.Controls.Add(this.tbTemplateDB);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbObjDB);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CheckDataBaseStructForm";
            this.Text = "数据库结构检查";
            this.Load += new System.EventHandler(this.CheckDataBaseStructForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btTemplate;
        private System.Windows.Forms.Button btObj;
        private System.Windows.Forms.TextBox tbTemplateDB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbObjDB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbOutFilePath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btOutputPath;
    }
}