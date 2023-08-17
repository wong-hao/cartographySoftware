namespace SMGI.Plugin.DCDProcess
{
    partial class MultiFeatureOverlapRuleSetForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cb_sourceLayer = new System.Windows.Forms.ComboBox();
            this.cb_rule = new System.Windows.Forms.ComboBox();
            this.cb_targetLayer = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btn_sql1 = new System.Windows.Forms.Button();
            this.txt_sql1 = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btn_sql2 = new System.Windows.Forms.Button();
            this.txt_sql2 = new System.Windows.Forms.TextBox();
            this.btn_OK = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "图层：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 107);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "规则：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "图层：";
            // 
            // cb_sourceLayer
            // 
            this.cb_sourceLayer.FormattingEnabled = true;
            this.cb_sourceLayer.Location = new System.Drawing.Point(14, 47);
            this.cb_sourceLayer.Name = "cb_sourceLayer";
            this.cb_sourceLayer.Size = new System.Drawing.Size(154, 20);
            this.cb_sourceLayer.TabIndex = 3;
            // 
            // cb_rule
            // 
            this.cb_rule.FormattingEnabled = true;
            this.cb_rule.Location = new System.Drawing.Point(12, 131);
            this.cb_rule.Name = "cb_rule";
            this.cb_rule.Size = new System.Drawing.Size(139, 20);
            this.cb_rule.TabIndex = 4;
            // 
            // cb_targetLayer
            // 
            this.cb_targetLayer.FormattingEnabled = true;
            this.cb_targetLayer.Location = new System.Drawing.Point(10, 52);
            this.cb_targetLayer.Name = "cb_targetLayer";
            this.cb_targetLayer.Size = new System.Drawing.Size(154, 20);
            this.cb_targetLayer.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(203, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "SQL:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(201, 27);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 7;
            this.label5.Text = "SQL:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_sql1);
            this.groupBox1.Controls.Add(this.txt_sql1);
            this.groupBox1.Location = new System.Drawing.Point(2, 1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(502, 86);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "源要素图层";
            // 
            // btn_sql1
            // 
            this.btn_sql1.Location = new System.Drawing.Point(346, 7);
            this.btn_sql1.Name = "btn_sql1";
            this.btn_sql1.Size = new System.Drawing.Size(75, 23);
            this.btn_sql1.TabIndex = 12;
            this.btn_sql1.Text = "查询构建器";
            this.btn_sql1.UseVisualStyleBackColor = true;
            this.btn_sql1.Click += new System.EventHandler(this.SQL_OnClick);
            // 
            // txt_sql1
            // 
            this.txt_sql1.Location = new System.Drawing.Point(203, 36);
            this.txt_sql1.Multiline = true;
            this.txt_sql1.Name = "txt_sql1";
            this.txt_sql1.Size = new System.Drawing.Size(229, 46);
            this.txt_sql1.TabIndex = 10;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btn_sql2);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.txt_sql2);
            this.groupBox2.Controls.Add(this.cb_targetLayer);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(2, 165);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(502, 94);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "目标要素图层";
            // 
            // btn_sql2
            // 
            this.btn_sql2.Location = new System.Drawing.Point(346, 13);
            this.btn_sql2.Name = "btn_sql2";
            this.btn_sql2.Size = new System.Drawing.Size(75, 23);
            this.btn_sql2.TabIndex = 13;
            this.btn_sql2.Text = "查询构建器";
            this.btn_sql2.UseVisualStyleBackColor = true;
            this.btn_sql2.Click += new System.EventHandler(this.SQL_OnClick);
            // 
            // txt_sql2
            // 
            this.txt_sql2.Location = new System.Drawing.Point(203, 42);
            this.txt_sql2.Multiline = true;
            this.txt_sql2.Name = "txt_sql2";
            this.txt_sql2.Size = new System.Drawing.Size(229, 46);
            this.txt_sql2.TabIndex = 11;
            // 
            // btn_OK
            // 
            this.btn_OK.Location = new System.Drawing.Point(348, 281);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(75, 23);
            this.btn_OK.TabIndex = 10;
            this.btn_OK.Text = "确定";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(429, 281);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Cancel.TabIndex = 11;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // MultiFeatureOverlapRuleSetForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(517, 316);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cb_rule);
            this.Controls.Add(this.cb_sourceLayer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MultiFeatureOverlapRuleSetForm";
            this.Text = "添加规则";
            this.Load += new System.EventHandler(this.MultiFeatureOverlapRuleSetForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cb_sourceLayer;
        private System.Windows.Forms.ComboBox cb_rule;
        private System.Windows.Forms.ComboBox cb_targetLayer;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txt_sql1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txt_sql2;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_sql1;
        private System.Windows.Forms.Button btn_sql2;
    }
}