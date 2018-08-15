using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Advantech.Motion;

namespace OQC_IC_CHECK_System
{
    public partial class AxisInterface : UserControl
    {
        #region 属性窗口
        protected AxisDefine m_AxisName = AxisDefine.X;
        /// <summary>
        /// 轴名称
        /// </summary>
        [Category("自定义属性"), Description("轴名称")]
        public AxisDefine AxisName
        {
            get { return m_AxisName; }
            set
            {
                this.label_AxisName.Text = string.Format("{0}轴", value.ToString());
                this.m_AxisName = value;
                switch (m_AxisName)
                {
                    case AxisDefine.X:
                        axisproperty = GlobalVar.AxisX;
                        break;
                    case AxisDefine.Y:
                        axisproperty = GlobalVar.AxisY;
                        break;
                    case AxisDefine.Z:
                        axisproperty = GlobalVar.AxisZ;
                        break;
                    case AxisDefine.A:
                        axisproperty = GlobalVar.AxisA;
                        break;
                    case AxisDefine.D:
                        axisproperty = GlobalVar.AxisD;
                        break;
                    case AxisDefine.C:
                        axisproperty = GlobalVar.AxisC;
                        break;
                    case AxisDefine.B:
                        axisproperty = GlobalVar.AxisB;
                        break;
                }
            }
        }
        #endregion

        private AxisProperty axisproperty = new AxisProperty();
        /// <summary>
        /// 轴序号
        /// </summary>
        protected int Index
        {
            get
            {
                switch (AxisName)
                {
                    case AxisDefine.X:
                        return axisproperty.LinkIndex;
                    case AxisDefine.Y:
                        return axisproperty.LinkIndex;
                    case AxisDefine.A:
                        return axisproperty.LinkIndex;
                    case AxisDefine.B:
                        return axisproperty.LinkIndex;
                    case AxisDefine.D:
                        return axisproperty.LinkIndex;
                    case AxisDefine.C:
                        return axisproperty.LinkIndex;
                    case AxisDefine.Z:
                        return axisproperty.LinkIndex;
                    default:
                        return 0;
                }
            }
        }

        /// <summary>
        /// 是否正方向
        /// </summary>
        private bool Positive = true;
        /// <summary>
        /// JOG距离
        /// </summary>
        private double movedis = 0;
        /// <summary>
        /// 倍数换算关系
        /// </summary>
        private double rate = 0;

        protected Logs log = Logs.LogsT();
        protected MyFunction myfunction = new MyFunction();


        public AxisInterface()
        {
            InitializeComponent();

            WindowRefresh.Interval = GlobalVar.TimerInterval;
            WindowRefresh.Tick += new EventHandler(WindowRefresh_Tick);

        }


        private void AxisInterface_Load(object sender, EventArgs e)
        {
            this.comboBox_MoveDirect.SelectedIndex = 0;
            this.rb_T.Checked = true;

            this.movedis = Convert.ToDouble(this.numericUpDown_MovDis.Value);
            GetRate();
            this.movedis *= this.rate;
        }

        private void GetRate()
        {
            if (axisproperty.ServerOn) this.rate = GlobalVar.ServCMDRate;//伺服的脉冲距离
            else
            {
                if (Index == GlobalVar.AxisZ.LinkIndex)
                {
                    this.rate =  GlobalVar.MotorRate;//Z轴电机的脉冲 距离
                }
                else this.rate = GlobalVar.MotorRate;//电机的脉冲距离
            }
        }
        private void WindowRefresh_Tick(object sender, EventArgs e)
        {
            if (!GlobalVar.AxisPCI.Enable) return;

            this.label_status_Axis.Text = GlobalVar.AxisPCI.GetAxisState(Index).ToString();
            GetRealPosition();//获取实时位置

            UInt32 iostatus = new UInt32();
            bool sg0, sg1, sg2, sg3;
            sg0 = sg1 = sg2 = sg3 = false;
            if (GlobalVar.AxisPCI.GetIOState(Index, ref iostatus)) GetMotionIOStatus(iostatus);
            else IOGray();
            if (GlobalVar.AxisPCI.GetDI(Index, ref sg0, ref sg1, ref sg2, ref sg3)) UpdateDI(sg0, sg1, sg2, sg3);
            else DIGray();
            if (GlobalVar.AxisPCI.GetAxisDO(Index, ref sg0, ref sg1, ref sg2, ref sg3)) UpdateDO(sg0, sg1, sg2, sg3);
            else DOGray();
            GetVelPara();
        }

        /// <summary>
        /// 更新DI信息为灰色
        /// </summary>
        private void DIGray()
        {
            this.label_DI0.Image = Properties.Resources.LightGray;
            this.label_DI1.Image = Properties.Resources.LightGray;
            this.label_DI2.Image = Properties.Resources.LightGray;
            this.label_DI3.Image = Properties.Resources.LightGray;
        }
        /// <summary>
        /// 设置DO信息图标
        /// </summary>
        private void UpdateDO(bool sg0, bool sg1, bool sg2, bool sg3)
        {
            this.label_DO0.Image = sg0 ? Properties.Resources.LightGreen : Properties.Resources.LightRed;
            this.label_DO1.Image = sg0 ? Properties.Resources.LightGreen : Properties.Resources.LightRed;
            this.label_DO2.Image = sg0 ? Properties.Resources.LightGreen : Properties.Resources.LightRed;
            this.label_DO3.Image = sg0 ? Properties.Resources.LightGreen : Properties.Resources.LightRed;
        }
        /// <summary>
        /// 更新DO信息为灰色
        /// </summary>
        private void DOGray()
        {
            this.label_DO0.Image = Properties.Resources.LightGray;
            this.label_DO1.Image = Properties.Resources.LightGray;
            this.label_DO2.Image = Properties.Resources.LightGray;
            this.label_DO3.Image = Properties.Resources.LightGray;
        }
        /// <summary>
        /// 获取轴参数
        /// </summary>
        private void GetVelPara()
        {
            double VelLow; double VelHigh; double VelAcc; double VelDec; double Jerk;
            VelLow = VelHigh = VelAcc = VelDec = Jerk = 0;
            if (GlobalVar.AxisPCI.GetAxisVelParam(Index, ref VelLow, ref VelHigh, ref VelAcc, ref VelDec, ref Jerk))
            {
                this.numericUpDown_VelLow.Value = Convert.ToDecimal((double)VelLow / this.rate);
                this.numericUpDown_VelHigh.Value = Convert.ToDecimal((double)VelHigh / this.rate);
                this.numericUpDown_Acc.Value = Convert.ToDecimal((double)VelAcc / this.rate);
                this.numericUpDown_Dec.Value = Convert.ToDecimal((double)VelDec / this.rate);
                if (Jerk == 0) this.rb_T.Checked = true;
                else this.rb_S.Checked = true;
            }
        }

        /// <summary>
        /// 设置DI信息图标
        /// </summary>
        private void UpdateDI(bool di0, bool di1, bool di2, bool di3)
        {
            this.label_DI0.Image = di0 ? Properties.Resources.LightGreen : Properties.Resources.LightRed;
            this.label_DI1.Image = di1 ? Properties.Resources.LightGreen : Properties.Resources.LightRed;
            this.label_DI2.Image = di2 ? Properties.Resources.LightGreen : Properties.Resources.LightRed;
            this.label_DI3.Image = di3 ? Properties.Resources.LightGreen : Properties.Resources.LightRed;
        }

        /// <summary>
        /// 设置IO控件为灰色
        /// </summary>
        private void IOGray()
        {
            label_Ready.Image = Properties.Resources.LightGray;
            label_Alarm.Image = Properties.Resources.LightGray;
            label_ORG.Image = Properties.Resources.LightGray;
            label_LMTP.Image = Properties.Resources.LightGray;
            label_LMTN.Image = Properties.Resources.LightGray;
        }

        /// <summary>
        /// 获取轴的IO信号
        /// </summary>
        /// <param name="IOStatus"></param>
        private void GetMotionIOStatus(uint IOStatus)
        {
            if ((IOStatus & (uint)Ax_Motion_IO.AX_MOTION_IO_RDY) > 0)//Ready
            {
                label_Ready.Image = Properties.Resources.LightGreen;
            }
            else label_Ready.Image = Properties.Resources.LightRed;

            if ((IOStatus & (uint)Ax_Motion_IO.AX_MOTION_IO_ALM) > 0)//ALM
            {
                label_Alarm.Image = Properties.Resources.LightGreen;
            }
            else label_Alarm.Image = Properties.Resources.LightRed;

            if ((IOStatus & (uint)Ax_Motion_IO.AX_MOTION_IO_ORG) > 0)//ORG
            {
                label_ORG.Image = Properties.Resources.LightGreen;
            }
            else label_ORG.Image = Properties.Resources.LightRed;

            if ((IOStatus & (uint)Ax_Motion_IO.AX_MOTION_IO_LMTP) > 0)//+EL
            {
                label_LMTP.Image = Properties.Resources.LightGreen;
            }
            else label_LMTP.Image = Properties.Resources.LightRed;

            if ((IOStatus & (uint)Ax_Motion_IO.AX_MOTION_IO_LMTN) > 0)//-EL
            {
                label_LMTN.Image = Properties.Resources.LightGreen;
            }
            else label_LMTN.Image = Properties.Resources.LightRed;
        }

        /// <summary>
        /// 获取实时位置
        /// </summary>
        private void GetRealPosition()
        {
            string position = string.Empty;
            if (Index == GlobalVar.AxisX.LinkIndex)
            {
                position = GlobalVar.AxisPCI.Position_X.ToString("#0.000") + " mm";
            }
            else if (Index == GlobalVar.AxisY.LinkIndex)
            {
                position = GlobalVar.AxisPCI.Position_Y.ToString("#0.000") + " mm";
            }
            else if (Index == GlobalVar.AxisA.LinkIndex)
            {
                position = GlobalVar.AxisPCI.Position_A.ToString("#0.000") + " mm";
            }
            else if (Index == GlobalVar.AxisA.LinkIndex)
            {
                position = GlobalVar.AxisPCI.Position_C.ToString("#0.000") + " mm";
            }
            else if (Index == GlobalVar.AxisB.LinkIndex)
            {
                position = GlobalVar.AxisPCI.Position_D.ToString("#0.000") + "mm";
            }
            else if (Index == GlobalVar.AxisB.LinkIndex)
            {
                position = GlobalVar.AxisPCI.Position_B.ToString("#0.000") + "mm";
            }
            else if (Index == GlobalVar.AxisZ.LinkIndex)
            {
                position = GlobalVar.AxisPCI.Position_Z.ToString("#0.000") + "mm";
            }
            else return;
            this.label_RealPosition.Text = position;
        }
        /// <summary>
        /// 开始刷新
        /// </summary>
        internal void StartRefresh()
        {
            this.WindowRefresh.Start();
        }

        /// <summary>
        /// 停止刷新
        /// </summary>
        internal void StopRefresh()
        {
            AxisJOG(Positive, false);
            this.WindowRefresh.Stop();
        }
        /// <summary>
        /// 开启线程执行JOG
        /// </summary>
        /// <param name="Positive">是否正方形</param>
        private void StartJOG(bool positive)
        {
            Positive = positive;
            AxisJOG(Positive, true);
        }

        /// <summary>
        /// 轴JOG运动  此处必须点动
        /// </summary>
        /// <param name="Positive">是否正向</param>
        /// <param name="Run">是否运动</param>
        private void AxisJOG(bool Positive, bool Run)
        {
            if (Index == -1) return;
            try
            {
                if (Run)
                {
                    if (Positive)
                    {
                        GlobalVar.AxisPCI.MoveDIR(Index, true, movedis, true);
                    }
                    else
                    {
                        GlobalVar.AxisPCI.MoveDIR(Index, false, movedis, true);
                    }
                }
                else
                {
                    GlobalVar.AxisPCI.StopMove(Index);
                }
            }
            catch (Exception ex)
            {
                MsgBoxPop(ex.Message, Color.Red, MessageBoxButtons.OK);
            }
        }




        /// <summary>
        /// 弹框【OK或者Cancel】
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="backcolor">背景色</param>
        /// <returns></returns>
        protected bool MsgBoxPop(string text, Color backcolor, MessageBoxButtons btn)
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
        protected override CreateParams CreateParams
        {
            get
            {
                var parms = base.CreateParams;
                parms.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN
                return parms;
            }
        }

        private void button_Home_Click(object sender, EventArgs e)
        {
            if (this.Index == -1) return;
            GlobalVar.AxisPCI.Home(this.Index);
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            if (this.Index == -1) return;
            GlobalVar.AxisPCI.StopMove(Index);
        }

        private void Button_ClearError_Click(object sender, EventArgs e)
        {
            if (this.Index == -1) return;
            GlobalVar.AxisPCI.ClearAxisError(Index);
        }

        private void Button_SetEnd_Click(object sender, EventArgs e)
        {
            this.movedis = Convert.ToDouble(this.numericUpDown_MovDis.Value);
            GetRate();
            this.movedis *= this.rate; ;
        }

        private void btn_MoveLeft_MouseDown(object sender, MouseEventArgs e)
        {
            StartJOG(false);
        }

        private void btn_MoveRight_MouseDown(object sender, MouseEventArgs e)
        {
            StartJOG(true);
        }

        private void numericUpDown_VelLow_Click(object sender, EventArgs e)
        {
            double value = 0.000d;
            if (ChangeAxisValue(sender, ref value, 3))
            {
                GlobalVar.AxisPCI.SetProp_VelLow((uint)Index, value, false);
            }
        }

        private void numericUpDown_Acc_Click(object sender, EventArgs e)
        {
            double value = 0.000d;
            if (ChangeAxisValue(sender, ref value, 3))
            {
                GlobalVar.AxisPCI.SetProp_Acc((uint)Index, value, false);
            }
        }

        private void numericUpDown_VelHigh_Click(object sender, EventArgs e)
        {
            double value = 0.000d;
            if (ChangeAxisValue(sender, ref value, 3))
            {
                GlobalVar.AxisPCI.SetProp_VelHigh((uint)Index, value, false);
            }
        }

        private void numericUpDown_Dec_Click(object sender, EventArgs e)
        {
            double value = 0.000d;
            if (ChangeAxisValue(sender, ref value, 3))
            {
                GlobalVar.AxisPCI.SetProp_Dec((uint)Index, value, false);
            }
        }

        /// <summary>
        /// 修改轴的运动参数
        /// </summary>
        /// <param name="sender">文本框</param>
        /// <param name="value">修改后的值</param>
        /// <param name="v">小数位数</param>
        /// <param name="Max">最大值</param>
        /// <param name="Min">最小值</param>
        /// <returns></returns>
        private bool ChangeAxisValue(object sender, ref double value, int v, double Max = 999d, double Min = 0d)
        {
            double d = 0d;
            if (TextClick(sender, ref d, v, Max, Min))
            {
                int Rate = 1;
                if (axisproperty.ServerOn) Rate = GlobalVar.ServCMDRate;//伺服的换算倍数
                else Rate = GlobalVar.MotorRate;

                value = d * Rate;
                return true;
            }
            else return false;
        }

        /// <summary>
        /// 文本框点击事件
        /// </summary>
        /// <param name="sender">文本框</param>
        /// <param name="d">值</param>
        /// <param name="v">小数位数</param>
        /// <param name="max">最大值</param>
        /// <param name="min">最小值</param>
        /// <returns></returns>
        private bool TextClick(object sender, ref double d, int v=1, double max=999d, double min=0d)
        {
            try
            {
                this.WindowRefresh.Stop();
                NumericUpDown tb = (NumericUpDown)sender;
                Point p1 = MousePosition;//鼠标相对于屏幕的坐标
                Point p2 = this.PointToClient(p1);//鼠标相对于窗体的坐标
                Keyboard key = new Keyboard(p1,max,min,v);
                if (key.ShowDialog() == DialogResult.OK)
                {
                    d = (double)key.RealValue;
                    return true;
                }
                else return false;
            }catch(Exception ex)
            {
                log.AddERRORLOG("输入文本异常:" + ex.Message);
                return false;
            }
            finally
            {
                this.WindowRefresh.Start();
            }
        }

        private void Button_D04_Click(object sender, EventArgs e)
        {
            SetDO(4);
        }

        private void Button_D05_Click(object sender, EventArgs e)
        {
            SetDO(5);
        }

        private void Button_D06_Click(object sender, EventArgs e)
        {
            SetDO(6);
        }

        private void Button_D07_Click(object sender, EventArgs e)
        {
            SetDO(7);
        }

        private void SetDO(ushort DOChannel)
        {
            bool result = false;
            BoardSignalDefinition signal = new BoardSignalDefinition(Index, DOChannel);
            if (GlobalVar.AxisPCI.GetSingleDO(signal, ref result))
                GlobalVar.AxisPCI.SetDO(Index, DOChannel, !result);
        }
    }
}
