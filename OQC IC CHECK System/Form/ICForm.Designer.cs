namespace OQC_IC_CHECK_System
{
    partial class ICForm
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
            this.tabControlTF_IC = new OQC_IC_CHECK_System.TabControlTF();
            this.tabPage_CAD = new System.Windows.Forms.TabPage();
            this.splitContainer_CAD = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControlTF_IC.SuspendLayout();
            this.tabPage_CAD.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_CAD)).BeginInit();
            this.splitContainer_CAD.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControlTF_IC);
            // 
            // tabControlTF_IC
            // 
            this.tabControlTF_IC.Controls.Add(this.tabPage_CAD);
            this.tabControlTF_IC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlTF_IC.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControlTF_IC.ItemSize = new System.Drawing.Size(650, 80);
            this.tabControlTF_IC.Location = new System.Drawing.Point(0, 0);
            this.tabControlTF_IC.Multiline = true;
            this.tabControlTF_IC.Name = "tabControlTF_IC";
            this.tabControlTF_IC.SelectedIndex = 0;
            this.tabControlTF_IC.Size = new System.Drawing.Size(1600, 781);
            this.tabControlTF_IC.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControlTF_IC.TabIndex = 0;
            // 
            // tabPage_CAD
            // 
            this.tabPage_CAD.Controls.Add(this.splitContainer_CAD);
            this.tabPage_CAD.Location = new System.Drawing.Point(4, 84);
            this.tabPage_CAD.Name = "tabPage_CAD";
            this.tabPage_CAD.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_CAD.Size = new System.Drawing.Size(1592, 693);
            this.tabPage_CAD.TabIndex = 0;
            this.tabPage_CAD.Text = "CAD视图";
            this.tabPage_CAD.UseVisualStyleBackColor = true;
            // 
            // splitContainer_CAD
            // 
            this.splitContainer_CAD.BackColor = System.Drawing.Color.Silver;
            this.splitContainer_CAD.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_CAD.Location = new System.Drawing.Point(3, 3);
            this.splitContainer_CAD.Name = "splitContainer_CAD";
            // 
            // splitContainer_CAD.Panel1
            // 
            this.splitContainer_CAD.Panel1.AutoScroll = true;
            this.splitContainer_CAD.Size = new System.Drawing.Size(1586, 687);
            this.splitContainer_CAD.SplitterDistance = 199;
            this.splitContainer_CAD.TabIndex = 0;
            // 
            // ICForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1600, 900);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "ICForm";
            this.Text = "ICForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ICForm_FormClosed);
            this.Load += new System.EventHandler(this.ICForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControlTF_IC.ResumeLayout(false);
            this.tabPage_CAD.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_CAD)).EndInit();
            this.splitContainer_CAD.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TabControlTF tabControlTF_IC;
        private System.Windows.Forms.TabPage tabPage_CAD;
        private System.Windows.Forms.SplitContainer splitContainer_CAD;
    }
}