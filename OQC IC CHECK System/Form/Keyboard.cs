using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace OQC_IC_CHECK_System
{
    public partial class Keyboard : Form
    {  
        #region 窗体参数
        private float X; private float Y;
        #endregion

        #region/*鼠标拖动*/
        internal static int WM_NCHITTEST = 0x84;
        internal static IntPtr HTCLIENT = (IntPtr)0x1;
        internal static IntPtr HTCAPTION = (IntPtr)0x2;
        internal static int WM_NCLBUTTONDBLCLK = 0x00A3;
        #endregion

        /// <summary>
        /// 双精度的小数的位数
        /// </summary>
        private int m_DecimalDigits = 1;
        /// <summary>
        /// 双精度的范围
        /// </summary>
        private Range_Double DoubleValue;
        /// <summary>
        /// 整型的范围
        /// </summary>
        private Range_Double IntValue;

        private DataType m_DataType = DataType.None;
        /// <summary>
        /// 数据类型
        /// </summary>
        internal DataType Data_Type { get { return m_DataType; } }
        /// <summary>
        /// 实际值
        /// </summary>
        internal object RealValue = 0;

        private Logs log = Logs.LogsT();


        Thread Thd_beep;//Beep线程
        /// <summary>
        /// 等待beep响声
        /// </summary>
        private AutoResetEvent BeepStart = new AutoResetEvent(false);
        private int BeepNum = -1;

        /// <summary>
        /// 默认的构造函数 作为密码输入框 字体将随窗体变化
        /// </summary>
        public Keyboard(bool Password)
        {
            InitializeComponent(); 
            
            this.Resize += new EventHandler(Form_Resize);

            X = this.Width;
            Y = this.Height;
            setTag(this);

            if (Password)
            {
                this.textBox_Password.PasswordChar = '*';
                m_DataType = DataType.StringType_Password;
            }
            else
            {
                m_DataType = DataType.StringType; 
            }
        }

        public Keyboard(Point location, int Max, int Min)
        {
            InitializeComponent();
            this.Location = location;
            IntValue = new Range_Double(Max, Min);
            m_DataType = DataType.IntType;
        }

        public Keyboard(Point location, double Max, double Min,int DecimalDigits = 1)
        {
            InitializeComponent();
            this.Location = location;
            DoubleValue = new Range_Double(Max, Min);
            this.m_DecimalDigits = DecimalDigits;
            m_DataType = DataType.DoubleType;
        }

        #region 字体大小随窗体变化
        private void setTag(Control cons)
        {
            foreach (Control con in cons.Controls)
            {
                con.Tag = con.Width + ":" + con.Height + ":" + con.Left + ":" + con.Top + ":" + con.Font.Size;
                if (con.Controls.Count > 0)
                    setTag(con);
            }
        }
        private void setControls(float newx, float newy, Control cons)
        {
            foreach (Control con in cons.Controls)
            {
                string[] mytag = con.Tag.ToString().Split(new char[] { ':' });
                float a = Convert.ToSingle(mytag[0]) * newx;
                con.Width = (int)a;
                a = Convert.ToSingle(mytag[1]) * newy;
                con.Height = (int)(a);
                a = Convert.ToSingle(mytag[2]) * newx;
                con.Left = (int)(a);
                a = Convert.ToSingle(mytag[3]) * newy;
                con.Top = (int)(a);
                Single currentSize = Convert.ToSingle(mytag[4]) * newy;
                con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                if (con.Controls.Count > 0)
                {
                    setControls(newx, newy, con);
                }
            }
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            // throw new Exception("The method or operation is not implemented.");
            float newx = (this.Width) / X;
            //  float newy = (this.Height - this.statusStrip1.Height) / (Y - y);
            float newy = this.Height / Y;
            setControls(newx, newy, this);
            //this.Text = this.Width.ToString() +" "+ this.Height.ToString();

        }
        #endregion

        private void Keyboard_Load(object sender, EventArgs e)
        {
            if (IntValue != null)
            {
                this.label_Max.Text = string.Format("最大值:{0}", IntValue.MAX.ToString());
                this.label_Min.Text = string.Format("最小值:{0}", IntValue.MIN.ToString());
            }
            else if (DoubleValue != null)
            {
                this.label_Max.Text = string.Format("最大值:{0}", DoubleValue.MAX.ToString());
                this.label_Min.Text = string.Format("最小值:{0}", DoubleValue.MIN.ToString());

            }
            else
            {
                this.label_Max.Text = string.Format("");
                this.label_Min.Text = string.Format("");
            }

            this.Activate();
            this.textBox_Password.Focus();

            Thd_beep = new Thread(Beep);
            Thd_beep.IsBackground = true;
            Thd_beep.Start();

            int x, y, dist;
            x = this.Location.X;
            y = this.Location.Y;
            dist = Screen.PrimaryScreen.WorkingArea.Width - x - this.Width;
            if (dist < 0) x += dist;
            dist = Screen.PrimaryScreen.WorkingArea.Height - y - this.Height;
            if (dist < 0) y += dist;
            this.Location = new Point(x, y);
        }

        private void Beep()
        {
            while (true)
            {
                BeepStart.Reset();
                if (BeepStart.WaitOne())
                {
                    Console.Beep(100 * BeepNum, 200);
                }
            }
        }

        private void textBox_PasswordKeyWord(string str)
        {
            try
            {
                textBox_Password.Focus();
                SendKeys.Send(str);
                int Num = 0;
                if (int.TryParse(str, out Num)) Num++;
                else Num = 1;
                BeepNum = Num;
                BeepStart.Set();
            }
            catch { }
        }

        private void btn_Num_Click(object sender, EventArgs e)
        {
            textBox_PasswordKeyWord(((ImageButton)sender).Text);
        }

        private void btn_Del_Click(object sender, EventArgs e)
        {
            textBox_PasswordKeyWord("{BACKSPACE}");
        }

        private void btn_Negative_Click(object sender, EventArgs e)
        {
            textBox_PasswordKeyWord("{-}");
        }

        private void btn_Confirm_Click(object sender, EventArgs e)
        {
            try
            {
                string Text = this.textBox_Password.Text;
                switch (m_DataType)
                {
                    case DataType.IntType:
                        int Intvalue = 0;
                        if (int.TryParse(Text, out Intvalue))
                        {
                            if (Intvalue > this.IntValue.MAX || Intvalue < this.IntValue.MIN) throw new Exception("数字超出范围");
                            this.RealValue = Intvalue;
                        }
                        else
                        {
                            throw new Exception("数据类型不满足");
                        }
                        break;
                    case DataType.DoubleType:

                        double dobvalue = 0;
                        if (double.TryParse(Text, out dobvalue))
                        {
                            if (dobvalue > this.DoubleValue.MAX || dobvalue < this.DoubleValue.MIN) throw new Exception("数字超出范围");
                            if (Text.Contains("."))
                            {
                                Text = Text.Substring(Text.IndexOf(".") + 1);
                                if (Text.Length > this.m_DecimalDigits) throw new Exception(string.Format("小数位数超过{0}位", this.m_DecimalDigits));
                            }

                            this.RealValue = dobvalue;
                        }
                        else
                        {
                            throw new Exception("数据类型不满足");
                        }
                        break;
                    case DataType.StringType_Password:
                        if (Text != "2018") throw new Exception("密码错误！");

                        this.RealValue = Text;
                        break;
                    case DataType.StringType:
                        this.RealValue = Text;
                        break;
                }
                this.DialogResult = DialogResult.OK;
            }
            catch(Exception ex)
            {
                ErrMsgBox(ex.Message);
            }
        }

        private void btn_Cancle_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// 异常弹框
        /// </summary>
        /// <param name="errmsg">异常内容</param>
        /// <param name="title">异常标题</param>
        /// <returns></returns>
        private DialogResult ErrMsgBox(string errmsg, string title = "异常")
        {
            log.AddERRORLOG(errmsg);
            MsgBox box = new MsgBox();
            box.Title = title;
            box.ShowText = errmsg;
            return box.ShowDialog();
        }

        private void Keyboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Thd_beep != null && Thd_beep.IsAlive) Thd_beep.Abort();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCLBUTTONDBLCLK)
            {
                return;
            }
            if (Data_Type != DataType.StringType_Password && m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);
                if (m.Result == HTCLIENT)
                {
                    m.HWnd = this.Handle;
                    m.Result = HTCAPTION;
                }
                return;
            }
            base.WndProc(ref m);
        }
    }
}
