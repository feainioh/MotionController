namespace HalconCCD
{
    partial class BitMapBlock
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BitMapBlock));
            this.pb_image = new LayeredSkin.Controls.LayeredPictureBox();
            this.label_Index = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // pb_image
            // 
            this.pb_image.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pb_image.Borders.BottomColor = System.Drawing.Color.Moccasin;
            this.pb_image.Borders.BottomWidth = 3;
            this.pb_image.Borders.LeftColor = System.Drawing.Color.Moccasin;
            this.pb_image.Borders.LeftWidth = 3;
            this.pb_image.Borders.RightColor = System.Drawing.Color.Moccasin;
            this.pb_image.Borders.RightWidth = 3;
            this.pb_image.Borders.TopColor = System.Drawing.Color.Moccasin;
            this.pb_image.Borders.TopWidth = 3;
            this.pb_image.Canvas = ((System.Drawing.Bitmap)(resources.GetObject("pb_image.Canvas")));
            this.pb_image.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pb_image.Image = null;
            this.pb_image.Images = null;
            this.pb_image.Interval = 100;
            this.pb_image.Location = new System.Drawing.Point(0, 0);
            this.pb_image.MultiImageAnimation = false;
            this.pb_image.Name = "pb_image";
            this.pb_image.Size = new System.Drawing.Size(177, 140);
            this.pb_image.TabIndex = 0;
            this.pb_image.Click += new System.EventHandler(this.pb_image_Click);
            // 
            // label_Index
            // 
            this.label_Index.AutoSize = true;
            this.label_Index.BackColor = System.Drawing.Color.Transparent;
            this.label_Index.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Index.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label_Index.Location = new System.Drawing.Point(4, 3);
            this.label_Index.Name = "label_Index";
            this.label_Index.Size = new System.Drawing.Size(20, 22);
            this.label_Index.TabIndex = 1;
            this.label_Index.Text = "1";
            // 
            // BitMapBlock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.label_Index);
            this.Controls.Add(this.pb_image);
            this.Name = "BitMapBlock";
            this.Size = new System.Drawing.Size(177, 140);
            this.Load += new System.EventHandler(this.BitMapBlock_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label_Index;
        public LayeredSkin.Controls.LayeredPictureBox pb_image;
    }
}
