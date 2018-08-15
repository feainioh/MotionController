namespace OQC_IC_CHECK_System
{
    partial class Load
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
            this.lb_Close = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(70)))), ((int)(((byte)(116)))));
            this.panel1.Controls.Add(this.lb_Close);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(554, 354);
            this.panel1.TabIndex = 0;
            // 
            // lb_Close
            // 
            this.lb_Close.BackColor = System.Drawing.Color.Transparent;
            this.lb_Close.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lb_Close.Image = global::OQC_IC_CHECK_System.Properties.Resources.close_1;
            this.lb_Close.Location = new System.Drawing.Point(514, 1);
            this.lb_Close.Name = "lb_Close";
            this.lb_Close.Size = new System.Drawing.Size(39, 37);
            this.lb_Close.TabIndex = 3;
            this.lb_Close.Click += new System.EventHandler(this.lb_Close_Click);
            this.lb_Close.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lb_Close_MouseDown);
            this.lb_Close.MouseEnter += new System.EventHandler(this.lb_Close_MouseEnter);
            this.lb_Close.MouseLeave += new System.EventHandler(this.lb_Close_MouseLeave);
            this.lb_Close.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lb_Close_MouseUp);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(70)))), ((int)(((byte)(116)))));
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Image = global::OQC_IC_CHECK_System.Properties.Resources.Loading;
            this.label1.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(554, 354);
            this.label1.TabIndex = 2;
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // Load
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 354);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Load";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Load";
            this.Load += new System.EventHandler(this.Load_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lb_Close;
    }
}