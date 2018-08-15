using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OQC_IC_CHECK_System
{
    public partial class Frame : Form
    {
        private float X; private float Y;

        /// <summary>
        /// 线程是否允许运行
        /// </summary>
        protected bool Thd_Run = true;//线程是否运行
        /// <summary>
        /// 更新窗体数据的定时器
        /// </summary>
        protected Timer WindowRefresh = new Timer();
        MyFunction myfunction = new MyFunction();
        private bool isOpen=false;

        public Frame()
        {
            InitializeComponent();

            X = this.Width;
            Y = this.Height;
            setTag(this);


            this.Disposed += new EventHandler(AxisInterface_Disposed);

            WindowRefresh.Interval = GlobalVar.TimerInterval;
        }

        private void AxisInterface_Disposed(object sender, EventArgs e)
        {
            GlobalVar.AxisPCI.Tag_LockBefore.Event_Trigger -= Tag_LockBefore_Event_Trigger;
            this.WindowRefresh.Stop();
        }

        private void btn_Return_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Frame_Load(object sender, EventArgs e)
        {
            foreach (var item in this.splitContainer1.Panel1.Controls)//如果包含选择框，则将右下方的提示框显示出来
            {
                if (item is Either)
                {
                    this.panel_Explain.Visible = true;
                    break;
                }
            }
            InitMachine();
            if (GlobalVar.AxisPCI != null)
            {
                GlobalVar.AxisPCI.Tag_LockBefore.Event_Trigger += Tag_LockBefore_Event_Trigger;
            }

        }

        private void Tag_LockBefore_Event_Trigger(bool status)
        {
            SetBtnImage(this.btn_LockBefore, myfunction.GetLockBeforeDIStatus() ? Properties.Resources.Btn_ON : Properties.Resources.Btn_OFF);
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
        private void InitMachine()
        {
            try
            {
                if (GlobalVar.AxisPCI != null)
                {
                    SetBtnImage(this.btn_Light, myfunction.GetLightDOStatus() ? Properties.Resources.Light_ON : Properties.Resources.Light_Off);

                    SetBtnImage(this.btn_LockBefore, myfunction.GetLockBeforeDIStatus() ? Properties.Resources.Btn_ON : Properties.Resources.Btn_OFF);
                }
            }
            catch (Exception ex)
            {
                MsgBox("Frame InitMachine异常:" + ex.Message, Color.Red, MessageBoxButtons.OK);
            }
        }


        /// <summary>
        /// 设置按钮的背景图
        /// </summary>
        private void SetBtnImage(PictureBox btn, Bitmap bitmap)
        {
            if (btn.InvokeRequired)
            {
                btn.BeginInvoke(new Action(delegate { btn.Image = bitmap; }));
            }
            else btn.Image = bitmap;
        }
        /// <summary>
        /// 开始刷新
        /// </summary>
        protected void StartRefresh()
        {
            this.WindowRefresh.Start();
        }

        /// <summary>
        /// 重置刷新，先停止然后开始刷新
        /// </summary>
        protected void ReStartRefresh()
        {
            this.WindowRefresh.Stop();
            this.WindowRefresh.Start();
        }

        /// <summary>
        /// 停止刷新
        /// </summary>
        protected void StopRefresh()
        {
            this.WindowRefresh.Stop();
        }

        /// <summary>
        /// 弹框【确认或者取消】
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="backcolor">背景色</param>
        /// <returns></returns>
        protected bool MsgBox(string text, Color backcolor, MessageBoxButtons btn)
        {
            using (MsgBox box = new MsgBox(btn))
            {
                box.Title = "提示";
                box.ShowText = text;
                box.SetBackColor = backcolor;
                if (box.ShowDialog() == DialogResult.OK) return true;
                else return false;
            }
        }

        private void Frame_FormClosed(object sender, FormClosedEventArgs e)
        {
            Thd_Run = false;
        }

        private void Frame_Activated(object sender, EventArgs e)
        {
            StartRefresh();
            this.btn_Return.Focus();//聚焦此按钮，避免聚焦到其他控件
        }

        private void Frame_Deactivate(object sender, EventArgs e)
        {
            StopRefresh();
        }

        private void btn_LockBefore_Click(object sender, EventArgs e)
        {
            try
            {
                if (myfunction.GetLockBackStatus())//只判断前门大锁的状态
                {
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.Lock, false);
                    SetBtnImage(this.btn_LockBefore, Properties.Resources.Btn_OFF);
                }
                else
                {
                    SetBtnImage(this.btn_LockBefore, Properties.Resources.Btn_ON);
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.Lock, true);
                }
            }
            catch (Exception ex)
            {
                MsgBox("锁变更状态失败：" + ex.Message, Color.Red, MessageBoxButtons.OK);

            }
        }

        private void btn_Buzzer_Click(object sender, EventArgs e)
        {
            if (GlobalVar.Machine.BuzzerAllowSound)
            {
                GlobalVar.Machine.BuzzerAllowSound = false;
                GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.AlarmLight_Buzzer, false);
                SetBtnImage(this.btn_Buzzer, Properties.Resources.NoSound);
            }
            else
            {
                GlobalVar.Machine.BuzzerAllowSound = true;
                GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.AlarmLight_Buzzer, true);
                SetBtnImage(this.btn_Buzzer, Properties.Resources.Sound);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!isOpen)//只判断前门大锁的状态
                {
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.Light, true);
                    SetBtnImage(this.btn_Light, Properties.Resources.Light_ON);
                    isOpen = true;
                }
                else
                {
                    SetBtnImage(this.btn_Light, Properties.Resources.Light_Off);
                    isOpen = false;
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.Light, false);
                }
            }
            catch (Exception ex)
            {
                MsgBox("照明更状态失败：" + ex.Message, Color.Red, MessageBoxButtons.OK);

            }

        }
    }
}
