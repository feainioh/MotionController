namespace OQC_IC_CHECK_System
{
    partial class OBJ_DWGDirect
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
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panel_graphics = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label_pox_y = new System.Windows.Forms.Label();
            this.label_pox_x = new System.Windows.Forms.Label();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.panel_graphics.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_graphics
            // 
            this.panel_graphics.BackColor = System.Drawing.SystemColors.ControlText;
            this.panel_graphics.Controls.Add(this.label2);
            this.panel_graphics.Controls.Add(this.label1);
            this.panel_graphics.Controls.Add(this.label_pox_y);
            this.panel_graphics.Controls.Add(this.label_pox_x);
            this.panel_graphics.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_graphics.Location = new System.Drawing.Point(0, 0);
            this.panel_graphics.Name = "panel_graphics";
            this.panel_graphics.Size = new System.Drawing.Size(1375, 705);
            this.panel_graphics.TabIndex = 4;
            this.panel_graphics.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panel_graphics_MouseClick);
            this.panel_graphics.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel_graphics_MouseDown);
            this.panel_graphics.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel_graphics_MouseMove);
            this.panel_graphics.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel_graphics_MouseUp);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Aqua;
            this.label2.Location = new System.Drawing.Point(126, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "Y：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Aqua;
            this.label1.Location = new System.Drawing.Point(33, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "X：";
            // 
            // label_pox_y
            // 
            this.label_pox_y.AutoSize = true;
            this.label_pox_y.ForeColor = System.Drawing.Color.Aqua;
            this.label_pox_y.Location = new System.Drawing.Point(155, 27);
            this.label_pox_y.Name = "label_pox_y";
            this.label_pox_y.Size = new System.Drawing.Size(29, 12);
            this.label_pox_y.TabIndex = 5;
            this.label_pox_y.Text = "0.00";
            // 
            // label_pox_x
            // 
            this.label_pox_x.AutoSize = true;
            this.label_pox_x.ForeColor = System.Drawing.Color.Aqua;
            this.label_pox_x.Location = new System.Drawing.Point(62, 27);
            this.label_pox_x.Name = "label_pox_x";
            this.label_pox_x.Size = new System.Drawing.Size(29, 12);
            this.label_pox_x.TabIndex = 4;
            this.label_pox_x.Text = "0.00";
            // 
            // OBJ_DWGDirect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.HighlightText;
            this.Controls.Add(this.panel_graphics);
            this.Name = "OBJ_DWGDirect";
            this.Size = new System.Drawing.Size(1375, 705);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.on_control_Paint);
            this.Resize += new System.EventHandler(this.panel_graphics_Resize);
            this.panel_graphics.ResumeLayout(false);
            this.panel_graphics.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_graphics;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_pox_y;
        private System.Windows.Forms.Label label_pox_x;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;

    }
}
