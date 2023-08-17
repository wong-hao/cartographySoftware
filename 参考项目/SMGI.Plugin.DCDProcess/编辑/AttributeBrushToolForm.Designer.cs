namespace SMGI.Plugin.DCDProcess
{
    partial class AttributeBrushToolForm
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
            this.chkFieldList = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOptionSet = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chkFieldList
            // 
            this.chkFieldList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkFieldList.CheckOnClick = true;
            this.chkFieldList.ColumnWidth = 100;
            this.chkFieldList.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chkFieldList.FormattingEnabled = true;
            this.chkFieldList.Location = new System.Drawing.Point(2, 22);
            this.chkFieldList.Name = "chkFieldList";
            this.chkFieldList.Size = new System.Drawing.Size(219, 277);
            this.chkFieldList.TabIndex = 26;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 25;
            this.label2.Text = "要素类名称";
            // 
            // btnOptionSet
            // 
            this.btnOptionSet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOptionSet.Enabled = false;
            this.btnOptionSet.Location = new System.Drawing.Point(167, 308);
            this.btnOptionSet.Name = "btnOptionSet";
            this.btnOptionSet.Size = new System.Drawing.Size(54, 23);
            this.btnOptionSet.TabIndex = 27;
            this.btnOptionSet.Text = "保存";
            this.btnOptionSet.UseVisualStyleBackColor = true;
            this.btnOptionSet.Click += new System.EventHandler(this.btnOptionSet_Click);
            // 
            // AttributeBrushToolForm2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(226, 337);
            this.Controls.Add(this.btnOptionSet);
            this.Controls.Add(this.chkFieldList);
            this.Controls.Add(this.label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AttributeBrushToolForm2";
            this.Text = "属性刷选项";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AttributeBrushToolForm_FormClosed);
            this.Load += new System.EventHandler(this.AttributeBrushToolForm2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox chkFieldList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOptionSet;
    }
}