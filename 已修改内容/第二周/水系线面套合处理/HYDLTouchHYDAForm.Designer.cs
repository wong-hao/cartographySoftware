namespace SMGI.Plugin.CollaborativeWorkWithAccount.工具.水系线面套合处理
{
    partial class HYDLTouchHYDAForm
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
            this.roadLayerNameCombox = new System.Windows.Forms.ComboBox();
            this.directoryEntry1 = new System.DirectoryServices.DirectoryEntry();
            this.areaLayerNameCombox = new System.Windows.Forms.ComboBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // roadLayerNameCombox
            // 
            this.roadLayerNameCombox.FormattingEnabled = true;
            this.roadLayerNameCombox.Location = new System.Drawing.Point(124, 24);
            this.roadLayerNameCombox.Name = "roadLayerNameCombox";
            this.roadLayerNameCombox.Size = new System.Drawing.Size(121, 20);
            this.roadLayerNameCombox.TabIndex = 0;
            this.roadLayerNameCombox.SelectedIndexChanged += new System.EventHandler(this.RoadLayerNameCombox_SelectedIndexChanged);
            // 
            // areaLayerNameCombox
            // 
            this.areaLayerNameCombox.FormattingEnabled = true;
            this.areaLayerNameCombox.Location = new System.Drawing.Point(124, 66);
            this.areaLayerNameCombox.Name = "areaLayerNameCombox";
            this.areaLayerNameCombox.Size = new System.Drawing.Size(121, 20);
            this.areaLayerNameCombox.TabIndex = 1;
            this.areaLayerNameCombox.SelectedIndexChanged += new System.EventHandler(this.AreaLayerNameCombox_SelectedIndexChanged);
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(17, 108);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 2;
            this.buttonOk.Text = "确定";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(124, 108);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "选择水系线图层";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "选择水系面图层";
            // 
            // HYDLTouchHYDAForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(260, 150);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.areaLayerNameCombox);
            this.Controls.Add(this.roadLayerNameCombox);
            this.Name = "HYDLTouchHYDAForm";
            this.Text = "图层选择器";
            this.Load += new System.EventHandler(this.HYDLTouchHYDAForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox roadLayerNameCombox;
        private System.DirectoryServices.DirectoryEntry directoryEntry1;
        private System.Windows.Forms.ComboBox areaLayerNameCombox;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;

    }
}