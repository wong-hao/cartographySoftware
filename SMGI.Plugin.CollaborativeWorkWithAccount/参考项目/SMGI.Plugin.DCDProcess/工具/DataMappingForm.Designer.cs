namespace SMGI.Plugin.DCDProcess
{
    partial class DataMappingForm
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
            this.btnTemplate = new System.Windows.Forms.Button();
            this.btnSource = new System.Windows.Forms.Button();
            this.tbTemplateDB = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbSourceDB = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbOutDB = new System.Windows.Forms.TextBox();
            this.btnOutput = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.cmbMappingRuleTable = new System.Windows.Forms.ComboBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnTemplate
            // 
            this.btnTemplate.Location = new System.Drawing.Point(388, 79);
            this.btnTemplate.Name = "btnTemplate";
            this.btnTemplate.Size = new System.Drawing.Size(61, 23);
            this.btnTemplate.TabIndex = 19;
            this.btnTemplate.Text = "选择";
            this.btnTemplate.UseVisualStyleBackColor = true;
            this.btnTemplate.Click += new System.EventHandler(this.btnTemplate_Click);
            // 
            // btnSource
            // 
            this.btnSource.Location = new System.Drawing.Point(388, 25);
            this.btnSource.Name = "btnSource";
            this.btnSource.Size = new System.Drawing.Size(61, 23);
            this.btnSource.TabIndex = 20;
            this.btnSource.Text = "选择";
            this.btnSource.UseVisualStyleBackColor = true;
            this.btnSource.Click += new System.EventHandler(this.btnSource_Click);
            // 
            // tbTemplateDB
            // 
            this.tbTemplateDB.Location = new System.Drawing.Point(4, 79);
            this.tbTemplateDB.Name = "tbTemplateDB";
            this.tbTemplateDB.ReadOnly = true;
            this.tbTemplateDB.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tbTemplateDB.Size = new System.Drawing.Size(378, 21);
            this.tbTemplateDB.TabIndex = 18;
            this.tbTemplateDB.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 12);
            this.label2.TabIndex = 15;
            this.label2.Text = "标准结构模板数据库";
            // 
            // tbSourceDB
            // 
            this.tbSourceDB.Location = new System.Drawing.Point(4, 25);
            this.tbSourceDB.Name = "tbSourceDB";
            this.tbSourceDB.ReadOnly = true;
            this.tbSourceDB.Size = new System.Drawing.Size(378, 21);
            this.tbSourceDB.TabIndex = 17;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 16;
            this.label1.Text = "源数据库";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(2, 121);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 12);
            this.label5.TabIndex = 27;
            this.label5.Text = "数据映射规则表";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 170);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 27;
            this.label3.Text = "输出数据库";
            // 
            // tbOutDB
            // 
            this.tbOutDB.Location = new System.Drawing.Point(4, 185);
            this.tbOutDB.Name = "tbOutDB";
            this.tbOutDB.ReadOnly = true;
            this.tbOutDB.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tbOutDB.Size = new System.Drawing.Size(378, 21);
            this.tbOutDB.TabIndex = 18;
            this.tbOutDB.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnOutput
            // 
            this.btnOutput.Location = new System.Drawing.Point(388, 183);
            this.btnOutput.Name = "btnOutput";
            this.btnOutput.Size = new System.Drawing.Size(61, 23);
            this.btnOutput.TabIndex = 19;
            this.btnOutput.Text = "选择";
            this.btnOutput.UseVisualStyleBackColor = true;
            this.btnOutput.Click += new System.EventHandler(this.btnOutput_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 229);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(457, 31);
            this.panel1.TabIndex = 34;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(316, 4);
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
            this.panel2.Location = new System.Drawing.Point(380, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(389, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // cmbMappingRuleTable
            // 
            this.cmbMappingRuleTable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMappingRuleTable.FormattingEnabled = true;
            this.cmbMappingRuleTable.Location = new System.Drawing.Point(4, 137);
            this.cmbMappingRuleTable.Name = "cmbMappingRuleTable";
            this.cmbMappingRuleTable.Size = new System.Drawing.Size(441, 20);
            this.cmbMappingRuleTable.TabIndex = 35;
            // 
            // DataMappingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 260);
            this.Controls.Add(this.cmbMappingRuleTable);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnOutput);
            this.Controls.Add(this.btnTemplate);
            this.Controls.Add(this.btnSource);
            this.Controls.Add(this.tbOutDB);
            this.Controls.Add(this.tbTemplateDB);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbSourceDB);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DataMappingForm";
            this.Text = "数据映射对话框";
            this.Load += new System.EventHandler(this.DataMappingForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnTemplate;
        private System.Windows.Forms.Button btnSource;
        private System.Windows.Forms.TextBox tbTemplateDB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbSourceDB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbOutDB;
        private System.Windows.Forms.Button btnOutput;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.ComboBox cmbMappingRuleTable;
    }
}