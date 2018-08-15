using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OQC_IC_CHECK_System
{ 
    /// <summary>
    /// 控件的标题位于左侧
    /// </summary>
    public class TabControlTF : System.Windows.Forms.TabControl
    {
        private bool IsFirstDraw = true;
        private Dictionary<int, string> tabtext = new Dictionary<int, string>();//每个tabpage对应的标题
        public TabControlTF()
        {
            InitializeComponent();
            TabSet();
        }

        /// <summary>
        /// 设定控件绘制模式
        /// </summary>
        private void TabSet()
        {
            this.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.Alignment = TabAlignment.Top;
            this.SizeMode = TabSizeMode.Fixed;
            this.Multiline = true;
            this.ItemSize = new Size(50, 210);
        }


        /// <summary>
        /// 重绘控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabLeft_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            RectangleF tRectangleF = GetTabRect(e.Index);
            if (e.State == DrawItemState.Selected)//填充颜色
            {
                SolidBrush b = new SolidBrush(Color.CornflowerBlue);
                Rectangle rect = new Rectangle((int)tRectangleF.X, (int)tRectangleF.Y, (int)tRectangleF.Width, (int)tRectangleF.Height);
                g.FillRectangles(b, new Rectangle[] { rect });
            }
            Font font = new Font("微软雅黑", 23.0f);
            SolidBrush brush;
            if (e.State == DrawItemState.Selected) brush = new SolidBrush(Color.LimeGreen);
            else brush = new SolidBrush(Color.Black);
            StringFormat sf = new StringFormat();//封装文本布局信息 
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Near;
            if (IsFirstDraw)
            {
                g.DrawString(this.Controls[e.Index].Text, font, brush, tRectangleF, sf);
                int num = 0;
                foreach (TabPage item in this.TabPages)
                {
                    tabtext.Add(num++, item.Text);
                }
                IsFirstDraw = false;
            }
            else g.DrawString(this.tabtext[e.Index], font, brush, tRectangleF, sf);
            //Console.WriteLine("{2}\tIndex:{0}\tText:{1}\t{3}", e.Index, this.Controls[e.Index].Text, DateTime.Now.ToString("HH:mm:ss"), e.State);//临时的输出日志
        }


        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabLeft_DrawItem);
            this.ResumeLayout(false);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var parms = base.CreateParams;
                parms.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN
                return parms;
            }
        }
    }
}
