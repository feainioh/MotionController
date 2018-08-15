namespace OQC_IC_CHECK_System
{
    partial class Either
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label_Title = new System.Windows.Forms.Label();
            this.btn_left = new OQC_IC_CHECK_System.ImageButton();
            this.btn_right = new OQC_IC_CHECK_System.ImageButton();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.label_Title);
            this.flowLayoutPanel1.Controls.Add(this.btn_left);
            this.flowLayoutPanel1.Controls.Add(this.btn_right);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(394, 48);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // label_Title
            // 
            this.label_Title.BackColor = System.Drawing.Color.Silver;
            this.label_Title.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Title.Location = new System.Drawing.Point(3, 0);
            this.label_Title.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.label_Title.Name = "label_Title";
            this.label_Title.Size = new System.Drawing.Size(149, 46);
            this.label_Title.TabIndex = 1;
            this.label_Title.Text = "  标题   ";
            this.label_Title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_left
            // 
            this.btn_left.BackColor = System.Drawing.Color.LimeGreen;
            this.btn_left.FlatAppearance.BorderSize = 0;
            this.btn_left.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_left.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_left.Location = new System.Drawing.Point(155, 0);
            this.btn_left.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.btn_left.Name = "btn_left";
            this.btn_left.Size = new System.Drawing.Size(118, 46);
            this.btn_left.TabIndex = 2;
            this.btn_left.Text = "上";
            this.btn_left.UseVisualStyleBackColor = false;
            this.btn_left.Click += new System.EventHandler(this.btn_left_Click);
            // 
            // btn_right
            // 
            this.btn_right.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.btn_right.FlatAppearance.BorderSize = 0;
            this.btn_right.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_right.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_right.Location = new System.Drawing.Point(273, 0);
            this.btn_right.Margin = new System.Windows.Forms.Padding(0);
            this.btn_right.Name = "btn_right";
            this.btn_right.Size = new System.Drawing.Size(118, 46);
            this.btn_right.TabIndex = 3;
            this.btn_right.Text = "下";
            this.btn_right.UseVisualStyleBackColor = false;
            this.btn_right.Click += new System.EventHandler(this.btn_right_Click);
            // 
            // Either
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "Either";
            this.Size = new System.Drawing.Size(394, 48);
            this.Load += new System.EventHandler(this.Either_Load);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        protected internal ImageButton btn_left;
        protected internal ImageButton btn_right;
        private System.Windows.Forms.Label label_Title;

    }
}
