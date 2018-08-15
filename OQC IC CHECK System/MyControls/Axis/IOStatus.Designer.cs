namespace OQC_IC_CHECK_System
{
    partial class IOStatus
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label_Title = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label_Title
            // 
            this.label_Title.BackColor = System.Drawing.Color.Transparent;
            this.label_Title.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Bold);
            this.label_Title.ForeColor = System.Drawing.Color.SpringGreen;
            this.label_Title.Location = new System.Drawing.Point(7, 4);
            this.label_Title.Name = "label_Title";
            this.label_Title.Size = new System.Drawing.Size(157, 72);
            this.label_Title.TabIndex = 0;
            this.label_Title.Text = "备用";
            this.label_Title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_Title.Click += new System.EventHandler(this.label_Title_Click);
            // 
            // IOStatus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::OQC_IC_CHECK_System.Properties.Resources.LightRed_Back;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Controls.Add(this.label_Title);
            this.Name = "IOStatus";
            this.Size = new System.Drawing.Size(246, 73);
            this.Load += new System.EventHandler(this.IOStatus_Load);
            this.Click += new System.EventHandler(this.IOStatus_Click);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label_Title;
    }
}
