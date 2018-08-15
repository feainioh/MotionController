using System.Drawing;

namespace OQC_IC_CHECK_System
{
    partial class SetForm
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
            this.components = new System.ComponentModel.Container();
            this.btn_ClearAlarmRecord = new OQC_IC_CHECK_System.ImageButton();
            this.btn_ClearYeild = new OQC_IC_CHECK_System.ImageButton();
            this.listView_ErrList = new System.Windows.Forms.ListView();
            this.Index = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Message = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.WindowRefresh = new System.Windows.Forms.Timer(this.components);
            this.count = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listView_ErrList);
            this.splitContainer1.Panel1.Controls.Add(this.btn_ClearYeild);
            this.splitContainer1.Panel1.Controls.Add(this.btn_ClearAlarmRecord);
            // 
            // btn_ClearAlarmRecord
            // 
            this.btn_ClearAlarmRecord.BackColor = System.Drawing.Color.Lime;
            this.btn_ClearAlarmRecord.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_ClearAlarmRecord.Location = new System.Drawing.Point(258, 102);
            this.btn_ClearAlarmRecord.Name = "btn_ClearAlarmRecord";
            this.btn_ClearAlarmRecord.Size = new System.Drawing.Size(350, 110);
            this.btn_ClearAlarmRecord.TabIndex = 0;
            this.btn_ClearAlarmRecord.Text = "清空报警记录";
            this.btn_ClearAlarmRecord.UseVisualStyleBackColor = false;
            this.btn_ClearAlarmRecord.Click += new System.EventHandler(this.btn_ClearAlarmRecord_Click);
            // 
            // btn_ClearYeild
            // 
            this.btn_ClearYeild.BackColor = System.Drawing.Color.Lime;
            this.btn_ClearYeild.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_ClearYeild.Location = new System.Drawing.Point(258, 387);
            this.btn_ClearYeild.Name = "btn_ClearYeild";
            this.btn_ClearYeild.Size = new System.Drawing.Size(350, 110);
            this.btn_ClearYeild.TabIndex = 1;
            this.btn_ClearYeild.Text = "清除统计信息";
            this.btn_ClearYeild.UseVisualStyleBackColor = false;
            // 
            // listView_ErrList
            // 
            this.listView_ErrList.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.listView_ErrList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView_ErrList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Index,
            this.Message,
            this.count});
            this.listView_ErrList.ForeColor = Color.White;
            this.listView_ErrList.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listView_ErrList.GridLines = true;
            this.listView_ErrList.Location = new System.Drawing.Point(903, 102);
            this.listView_ErrList.Name = "listView_ErrList";
            this.listView_ErrList.Size = new System.Drawing.Size(602, 700);
            this.listView_ErrList.TabIndex = 2;
            this.listView_ErrList.UseCompatibleStateImageBehavior = false;
            this.listView_ErrList.View = System.Windows.Forms.View.Details;
            // 
            // Index
            // 
            this.Index.Text = "序号";
            this.Index.Width = 80;
            // 
            // Message
            // 
            this.Message.Text = "异常信息";
            this.Message.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Message.Width = 440;
            // 
            // WindowRefresh
            // 
            this.WindowRefresh.Interval = 1000;
            // 
            // count
            // 
            this.count.Text = "频次";
            this.count.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.count.Width = 80;
            // 
            // SetForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1920, 1080);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "SetForm";
            this.Text = "SetForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SetForm_FormClosing);
            this.Load += new System.EventHandler(this.SetForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ImageButton btn_ClearAlarmRecord;
        private ImageButton btn_ClearYeild;
        private System.Windows.Forms.ListView listView_ErrList;
        private System.Windows.Forms.ColumnHeader Index;
        private System.Windows.Forms.ColumnHeader Message;
        private System.Windows.Forms.Timer WindowRefresh;
        private System.Windows.Forms.ColumnHeader count;
    }
}