namespace SMGI.Plugin.DCDProcess
{
    partial class LayerSelectForm
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
            this.LayerNameList = new System.Windows.Forms.CheckedListBox();
            this.btnSelAll = new System.Windows.Forms.Button();
            this.btnUnSelAll = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // LayerNameList
            // 
            this.LayerNameList.CheckOnClick = true;
            this.LayerNameList.ColumnWidth = 200;
            this.LayerNameList.FormattingEnabled = true;
            this.LayerNameList.Location = new System.Drawing.Point(14, 24);
            this.LayerNameList.MultiColumn = true;
            this.LayerNameList.Name = "LayerNameList";
            this.LayerNameList.Size = new System.Drawing.Size(470, 132);
            this.LayerNameList.TabIndex = 36;
            // 
            // btnSelAll
            // 
            this.btnSelAll.Location = new System.Drawing.Point(492, 24);
            this.btnSelAll.Name = "btnSelAll";
            this.btnSelAll.Size = new System.Drawing.Size(61, 23);
            this.btnSelAll.TabIndex = 35;
            this.btnSelAll.Text = "全选";
            this.btnSelAll.UseVisualStyleBackColor = true;
            this.btnSelAll.Click += new System.EventHandler(this.btnSelAll_Click);
            // 
            // btnUnSelAll
            // 
            this.btnUnSelAll.Location = new System.Drawing.Point(492, 62);
            this.btnUnSelAll.Name = "btnUnSelAll";
            this.btnUnSelAll.Size = new System.Drawing.Size(61, 23);
            this.btnUnSelAll.TabIndex = 34;
            this.btnUnSelAll.Text = "全部取消";
            this.btnUnSelAll.UseVisualStyleBackColor = true;
            this.btnUnSelAll.Click += new System.EventHandler(this.btnUnSelAll_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 33;
            this.label2.Text = "图层列表";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 181);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(559, 31);
            this.panel1.TabIndex = 37;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(418, 4);
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
            this.panel2.Location = new System.Drawing.Point(482, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(491, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // LayerSelectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(559, 212);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.LayerNameList);
            this.Controls.Add(this.btnSelAll);
            this.Controls.Add(this.btnUnSelAll);
            this.Controls.Add(this.label2);
            this.Name = "LayerSelectForm";
            this.Text = "图层选择";
            this.Load += new System.EventHandler(this.LayerSelectForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox LayerNameList;
        private System.Windows.Forms.Button btnSelAll;
        private System.Windows.Forms.Button btnUnSelAll;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
    }
}