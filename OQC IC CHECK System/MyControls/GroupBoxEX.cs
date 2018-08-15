using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace OQC_IC_CHECK_System
{
    public class GroupBoxEx : System.Windows.Forms.GroupBox
    {
        private Font _titleFont = new Font("宋体", 15, FontStyle.Regular);
        private Color _titleColor = Color.Green;
        private Color _borderColor = Color.FromArgb(23, 169, 254);
        private int _radius = 10;
        private int _tiltePos = 10;

        private const int WM_ERASEBKGND = 0x0014;
        private const int WM_PAINT = 0xF;

        public GroupBoxEx()
            : base()
        {
        }

        [DefaultValue(typeof(Color), "23, 169, 254"), Description("控件边框颜色")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                base.Invalidate();
            }
        }

        [DefaultValue(typeof(Color), "Green"), Description("标题颜色")]
        public Color TitleColor
        {
            get { return _titleColor; }
            set
            {
                _titleColor = value;
                base.Invalidate();
            }
        }

        [DefaultValue(typeof(Font), ""), Description("标题字体设置")]
        public Font TitleFont
        {
            get { return _titleFont; }
            set
            {
                _titleFont = value;
                base.Invalidate();
            }
        }


        [DefaultValue(typeof(int), "30"), Description("圆角弧度大小")]
        public int Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                base.Invalidate();
            }
        }

        [DefaultValue(typeof(int), "10"), Description("标题位置")]
        public int TiltePos
        {
            get { return _tiltePos; }
            set
            {
                _tiltePos = value;
                base.Invalidate();
            }
        }

        protected override void WndProc(ref Message m)
        {
            try
            {
                base.WndProc(ref m);
                if (m.Msg == WM_PAINT)
                {
                    if (this.Radius > 0)
                    {
                        using (Graphics g = Graphics.FromHwnd(this.Handle))
                        {
                            Rectangle r = new Rectangle();
                            r.Width = this.Width;
                            r.Height = this.Height;
                            DrawBorder(g, r, this.Radius);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DrawBorder(Graphics g, Rectangle rect, int radius)
        {
            rect.Width -= 1;
            rect.Height -= 1;


            using (Pen pen = new Pen(this.BorderColor))
            {
                g.Clear(this.BackColor);
                g.DrawString(this.Text, this.TitleFont, new SolidBrush(this.TitleColor), radius + this.TiltePos, 0);

                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                GraphicsPath path = new GraphicsPath();

                float height = g.MeasureString(this.Text, this.TitleFont).Height / 2;
                float width = g.MeasureString(this.Text, this.TitleFont).Width;

                path.AddArc(rect.X, rect.Y + height, radius, radius, 180, 90);//左上角弧线   
                path.AddLine(radius, rect.Y + height, radius + this.TiltePos, rect.Y + height);

                path.StartFigure();

                path.AddLine(radius + this.TiltePos + width, rect.Y + height, rect.Right - radius, rect.Y + height);

                path.AddArc(rect.Right - radius, rect.Y + height, radius, radius, 270, 90);//右上角弧线   
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);

                path.StartFigure();

                path.AddArc(rect.X, rect.Y + height, radius, radius, -90, -90);//左上角弧线   
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, -180, -90);


                g.DrawPath(pen, path);
            }
        }
    }  
}
