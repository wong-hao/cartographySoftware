namespace SMGI.Plugin.DCDProcess
{
    partial class CheckLayerSelectForm
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
            this.btnFilePath = new System.Windows.Forms.Button();
            this.tbOutFilePath = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chkLayerList = new System.Windows.Forms.CheckedListBox();
            this.btnSelAll = new System.Windows.Forms.Button();
            this.btnUnSelAll = new System.Windows.Forms.Button();
            this.cbBetweenLayers = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 216);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(558, 31);
            this.panel1.TabIndex = 10;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(417, 4);
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
            this.panel2.Location = new System.Drawing.Point(481, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(490, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // btnFilePath
            // 
            this.btnFilePath.Location = new System.Drawing.Point(492, 181);
            this.btnFilePath.Name = "btnFilePath";
            this.btnFilePath.Size = new System.Drawing.Size(61, 23);
            this.btnFilePath.TabIndex = 31;
            this.btnFilePath.Text = "选择";
            this.btnFilePath.UseVisualStyleBackColor = true;
            this.btnFilePath.Click += new System.EventHandler(this.btnFilePath_Click);
            // 
            // tbOutFilePath
            // 
            this.tbOutFilePath.Location = new System.Drawing.Point(84, 181);
            this.tbOutFilePath.Name = "tbOutFilePath";
            this.tbOutFilePath.ReadOnly = true;
            this.tbOutFilePath.Size = new System.Drawing.Size(400, 21);
            this.tbOutFilePath.TabIndex = 30;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 184);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 12);
            this.label5.TabIndex = 29;
            this.label5.Text = "输出路径:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 12);
            this.label2.TabIndex = 29;
            this.label2.Text = "待检查的图层列表";
            // 
            // chkLayerList
            // 
            this.chkLayerList.CheckOnClick = true;
            this.chkLayerList.ColumnWidth = 120;
            this.chkLayerList.FormattingEnabled = true;
            this.chkLayerList.Location = new System.Drawing.Point(14, 24);
            this.chkLayerList.MultiColumn = true;
            this.chkLayerList.Name = "chkLayerList";
            this.chkLayerList.Size = new System.Drawing.Size(470, 132);
            this.chkLayerList.TabIndex = 32;
            // 
            // btnSelAll
            // 
            this.btnSelAll.Location = new System.Drawing.Point(492, 24);
            this.btnSelAll.Name = "btnSelAll";
            this.btnSelAll.Size = new System.Drawing.Size(61, 23);
            this.btnSelAll.TabIndex = 31;
            this.btnSelAll.Text = "全选";
            this.btnSelAll.UseVisualStyleBackColor = true;
            this.btnSelAll.Click += new System.EventHandler(this.btnSelAll_Click);
            // 
            // btnUnSelAll
            // 
            this.btnUnSelAll.Location = new System.Drawing.Point(492, 62);
            this.btnUnSelAll.Name = "btnUnSelAll";
            this.btnUnSelAll.Size = new System.Drawing.Size(61, 23);
            this.btnUnSelAll.TabIndex = 31;
            this.btnUnSelAll.Text = "全部取消";
            this.btnUnSelAll.UseVisualStyleBackColor = true;
            this.btnUnSelAll.Click += new System.EventHandler(this.btnUnSelAll_Click);
            // 
            // cbBetweenLayers
            // 
            this.cbBetweenLayers.AutoSize = true;
            this.cbBetweenLayers.Location = new System.Drawing.Point(14, 159);
            this.cbBetweenLayers.Name = "cbBetweenLayers";
            this.cbBetweenLayers.Size = new System.Drawing.Size(84, 16);
            this.cbBetweenLayers.TabIndex = 33;
            this.cbBetweenLayers.Text = "跨图层检查";
            this.cbBetweenLayers.UseVisualStyleBackColor = true;
            this.cbBetweenLayers.Visible = false;
            // 
            // CheckLayerSelectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(558, 247);
            this.Controls.Add(this.cbBetweenLayers);
            this.Controls.Add(this.chkLayerList);
            this.Controls.Add(this.btnSelAll);
            this.Controls.Add(this.btnUnSelAll);
            this.Controls.Add(this.btnFilePath);
            this.Controls.Add(this.tbOutFilePath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CheckLayerSelectForm";
            this.Text = "CheckLayerSelectForm";
            this.Load += new System.EventHandler(this.CheckLayerSelectForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btnFilePath;
        private System.Windows.Forms.TextBox tbOutFilePath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckedListBox chkLayerList;
        private System.Windows.Forms.Button btnSelAll;
        private System.Windows.Forms.Button btnUnSelAll;
        private System.Windows.Forms.CheckBox cbBetweenLayers;
    }
}