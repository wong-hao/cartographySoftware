namespace SMGI.Plugin.DCDProcess
{
    partial class PolylineEndPointProcessForm
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
            this.label5 = new System.Windows.Forms.Label();
            this.clbObjLayerList = new System.Windows.Forms.ComboBox();
            this.tbBufferValue = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.rtbSQLText = new System.Windows.Forms.RichTextBox();
            this.btnVerify = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "目标线图层";
            // 
            // clbObjLayerList
            // 
            this.clbObjLayerList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.clbObjLayerList.FormattingEnabled = true;
            this.clbObjLayerList.Location = new System.Drawing.Point(14, 24);
            this.clbObjLayerList.Name = "clbObjLayerList";
            this.clbObjLayerList.Size = new System.Drawing.Size(362, 20);
            this.clbObjLayerList.TabIndex = 5;
            // 
            // tbBufferValue
            // 
            this.tbBufferValue.Location = new System.Drawing.Point(16, 168);
            this.tbBufferValue.Name = "tbBufferValue";
            this.tbBufferValue.Size = new System.Drawing.Size(311, 21);
            this.tbBufferValue.TabIndex = 139;
            this.tbBufferValue.Text = "0";
            this.tbBufferValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 136;
            this.label3.Text = "定义查询(可选)";
            // 
            // rtbSQLText
            // 
            this.rtbSQLText.Location = new System.Drawing.Point(14, 73);
            this.rtbSQLText.Name = "rtbSQLText";
            this.rtbSQLText.Size = new System.Drawing.Size(313, 65);
            this.rtbSQLText.TabIndex = 137;
            this.rtbSQLText.Text = "";
            // 
            // btnVerify
            // 
            this.btnVerify.Location = new System.Drawing.Point(333, 71);
            this.btnVerify.Name = "btnVerify";
            this.btnVerify.Size = new System.Drawing.Size(43, 23);
            this.btnVerify.TabIndex = 138;
            this.btnVerify.Text = "验证";
            this.btnVerify.UseVisualStyleBackColor = true;
            this.btnVerify.Click += new System.EventHandler(this.btnVerify_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(333, 171);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 12);
            this.label7.TabIndex = 134;
            this.label7.Text = "米";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 152);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(101, 12);
            this.label6.TabIndex = 135;
            this.label6.Text = "距端点的距离阈值";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 211);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(391, 31);
            this.panel1.TabIndex = 140;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(250, 4);
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
            this.panel2.Location = new System.Drawing.Point(314, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(323, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // PolylineEndPointProcessForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(391, 242);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tbBufferValue);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.rtbSQLText);
            this.Controls.Add(this.btnVerify);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.clbObjLayerList);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PolylineEndPointProcessForm";
            this.Text = "线端节点密度处理";
            this.Load += new System.EventHandler(this.PolylineEndPointProcessForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.ComboBox clbObjLayerList;
        private System.Windows.Forms.TextBox tbBufferValue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RichTextBox rtbSQLText;
        private System.Windows.Forms.Button btnVerify;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
    }
}