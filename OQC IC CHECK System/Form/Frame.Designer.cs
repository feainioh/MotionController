namespace OQC_IC_CHECK_System
{
    partial class Frame
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btn_Return = new OQC_IC_CHECK_System.ImageButton();
            this.panel_Explain = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btn_Light = new System.Windows.Forms.PictureBox();
            this.btn_LockBefore = new System.Windows.Forms.PictureBox();
            this.btn_Buzzer = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel_Explain.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btn_Light)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btn_LockBefore)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btn_Buzzer)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btn_Light);
            this.splitContainer1.Panel2.Controls.Add(this.btn_LockBefore);
            this.splitContainer1.Panel2.Controls.Add(this.btn_Buzzer);
            this.splitContainer1.Panel2.Controls.Add(this.btn_Return);
            this.splitContainer1.Panel2.Controls.Add(this.panel_Explain);
            this.splitContainer1.Size = new System.Drawing.Size(1600, 900);
            this.splitContainer1.SplitterDistance = 781;
            this.splitContainer1.TabIndex = 1;
            // 
            // btn_Return
            // 
            this.btn_Return.BackColor = System.Drawing.Color.LimeGreen;
            this.btn_Return.Font = new System.Drawing.Font("微软雅黑", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_Return.Location = new System.Drawing.Point(3, 6);
            this.btn_Return.Name = "btn_Return";
            this.btn_Return.Size = new System.Drawing.Size(180, 105);
            this.btn_Return.TabIndex = 3;
            this.btn_Return.Text = "返回";
            this.btn_Return.UseVisualStyleBackColor = false;
            this.btn_Return.Click += new System.EventHandler(this.btn_Return_Click);
            // 
            // panel_Explain
            // 
            this.panel_Explain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_Explain.Controls.Add(this.flowLayoutPanel1);
            this.panel_Explain.Location = new System.Drawing.Point(1210, 65);
            this.panel_Explain.Name = "panel_Explain";
            this.panel_Explain.Size = new System.Drawing.Size(387, 46);
            this.panel_Explain.TabIndex = 2;
            this.panel_Explain.Visible = false;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Controls.Add(this.label4);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(387, 46);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(3, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 39);
            this.label1.TabIndex = 0;
            this.label1.Text = "按键说明";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Lime;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(142, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 39);
            this.label2.TabIndex = 0;
            this.label2.Text = "原点";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.ForeColor = System.Drawing.Color.LimeGreen;
            this.label3.Location = new System.Drawing.Point(223, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 39);
            this.label3.TabIndex = 0;
            this.label3.Text = "位置";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(304, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 39);
            this.label4.TabIndex = 0;
            this.label4.Text = "操作";
            // 
            // btn_Light
            // 
            this.btn_Light.Image = global::OQC_IC_CHECK_System.Properties.Resources.LightOFF;
            this.btn_Light.Location = new System.Drawing.Point(884, 10);
            this.btn_Light.Name = "btn_Light";
            this.btn_Light.Size = new System.Drawing.Size(170, 98);
            this.btn_Light.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btn_Light.TabIndex = 40;
            this.btn_Light.TabStop = false;
            this.btn_Light.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // btn_LockBefore
            // 
            this.btn_LockBefore.Image = global::OQC_IC_CHECK_System.Properties.Resources.Btn_ON;
            this.btn_LockBefore.Location = new System.Drawing.Point(578, 10);
            this.btn_LockBefore.Name = "btn_LockBefore";
            this.btn_LockBefore.Size = new System.Drawing.Size(178, 98);
            this.btn_LockBefore.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btn_LockBefore.TabIndex = 38;
            this.btn_LockBefore.TabStop = false;
            this.btn_LockBefore.Click += new System.EventHandler(this.btn_LockBefore_Click);
            // 
            // btn_Buzzer
            // 
            this.btn_Buzzer.Image = global::OQC_IC_CHECK_System.Properties.Resources.Sound;
            this.btn_Buzzer.Location = new System.Drawing.Point(253, 10);
            this.btn_Buzzer.Name = "btn_Buzzer";
            this.btn_Buzzer.Size = new System.Drawing.Size(178, 98);
            this.btn_Buzzer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btn_Buzzer.TabIndex = 37;
            this.btn_Buzzer.TabStop = false;
            this.btn_Buzzer.Click += new System.EventHandler(this.btn_Buzzer_Click);
            // 
            // Frame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(1600, 900);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Frame";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Frame";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Activated += new System.EventHandler(this.Frame_Activated);
            this.Deactivate += new System.EventHandler(this.Frame_Deactivate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Frame_FormClosed);
            this.Load += new System.EventHandler(this.Frame_Load);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel_Explain.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btn_Light)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btn_LockBefore)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btn_Buzzer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        protected internal System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel_Explain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private ImageButton btn_Return;
        private System.Windows.Forms.PictureBox btn_Buzzer;
        private System.Windows.Forms.PictureBox btn_LockBefore;
        private System.Windows.Forms.PictureBox btn_Light;
    }
}