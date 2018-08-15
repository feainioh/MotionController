using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace OQC_IC_CHECK_System
{
    public partial class BoardController : UserControl
    {
        #region 属性
        private bool _upLoad = false;
        /// <summary>
        /// 上料托盘是否到位【允许吸料】
        /// </summary>
        internal bool UpLoadArrive
        {
            get { return _upLoad; }
            set { _upLoad = value; }
        }

        private bool _downLoad = false;
        /// <summary>
        /// 下料托盘到位【允许放料】
        /// </summary>
        internal bool DownLoadArrive
        {
            get { return _downLoad; }
            set { _downLoad = value; }
        }

        private bool _feedMoved = false;
        /// <summary>
        /// 上料吸取完成
        /// </summary>
        internal bool FeedMoved
        {
            get { return _feedMoved; }
            set { _feedMoved = value; }
        }

        private bool _dropMoved = false;
        /// <summary>
        /// 下料放置完成
        /// </summary>
        internal bool DropMoved
        {
            get { return _dropMoved; }
            set { _dropMoved = value; }
        }
        #endregion
        MyFunction myfunction = new MyFunction();
        Logs log = Logs.LogsT();
        public BoardController()
        {
            InitializeComponent();
            InitControll();
        }

        private void InitControll()
        {
            textBox_UpIP.Text = GlobalVar.FeedIP;
            tb_Speed_Up.Text = GlobalVar.BoardFeedSpeed.ToString("0.00");
            tb_Acc_Up.Text = GlobalVar.BoardFeedAcc.ToString("0.00");
            tb_Position_Up.Text = GlobalVar.UpBoardFeedPosition.ToString("0.00");
            textBox_DownIP.Text = GlobalVar.DropIP;
            tb_Speed_Down.Text = GlobalVar.BoardFeedSpeed.ToString("0.00");
            tb_Acc_Down.Text = GlobalVar.BoardFeedAcc.ToString("0.00");
            tb_Position_Down.Text = GlobalVar.UpBoardFeedPosition.ToString("0.00");
            InitLabels();
            SetEnable(false);
        }

        private void InitLabels()
        {
            InitUpLabels();
            InitDownLabels();
        }

        private void InitUpLabels()
        {
            lb_L_UP.Text = "无异常";
            lb_H_UP.Text = "无异常";
            lb_LMT_UP.Text = "无异常";
            lb_ORG_UP.Text = "无异常";
            lb_Lock_UP.Text = "无异常";
            lb_EMG_UP.Text = "无异常";
            lb_Reset_UP.Text = "无需复位";
            lb_Run_UP.Text = "无异常";
        }

        private void InitDownLabels()
        {
            lb_L_Down.Text = "无异常";
            lb_H_Down.Text = "无异常";
            lb_LMT_Down.Text = "无异常";
            lb_ORG_Down.Text = "无异常";
            lb_Lock_Down.Text = "无异常";
            lb_EMG_Down.Text = "无异常";
            lb_Reset_Down.Text = "无需复位";
            lb_Run_Down.Text = "无异常";
        }

        private void SetEnable(bool v)
        {
            SetDownEnable(v);
            SetUpEnable(v);
        }

        private void SetUpEnable(bool v)
        {
            textBox_UpIP.Enabled = v;
            tb_Speed_Up.Enabled = v;
            tb_Acc_Up.Enabled = v;
            tb_Position_Up.Enabled = v;
            btn_Update_Up.Visible = v;
        }

        private void SetDownEnable(bool v)
        {
            textBox_DownIP.Enabled = v;
            tb_Speed_Down.Enabled = v;
            tb_Acc_Down.Enabled = v;
            tb_Position_Down.Enabled = v;
            btn_Update_Down.Visible = v;
        }

        /// <summary>
        /// 上料机自动运行
        /// </summary>
        private bool UpLoadRun = false;
        /// <summary>
        /// 下料机自动运行
        /// </summary>
        private bool UnderLoadRun = false;

        private void BoardController_Load(object sender, EventArgs e)
        {
            OpenThread();
        }

        public void StartWork()
        {
            try
            {
                checkBox_FeedBoardAuto.Checked = true;
                checkBox_DropBoardAuto.Checked = true;
            }
            catch (Exception ex)
            {
                log.AddERRORLOG("初始化通信异常:" + ex.Message);
            }

        }




        private void OpenThread()
        {
            Thread Thd_FeedLoad = new Thread(Feed_Check)
            {
                IsBackground = true,
                Name = "上料机MODBUS通信线程"
            };
            Thd_FeedLoad.Start();

            Thread Thd_DropLoad = new Thread(Drop_Check)
            {
                IsBackground = true,
                Name = "下料机MODBUS通信线程"
            };
            Thd_DropLoad.Start();
        }

        private void Drop_Check()
        {
            while (!GlobalVar.SoftWareShutDown)
            {
                Thread.Sleep(100);
                try
                {
                    // if (_dropMoved && !GlobalVar.Drop_Modbus.Coils.CommitSignal.Value) GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.CommitSignal, true);//下料完成，信号置为0
                    if (CheckCoil(GlobalVar.Drop_Modbus.Coils.BoardReady) == 0)//下料准备好
                    {
                        _downLoad = true;
                        this.BeginInvoke(new Action(() =>
                        {
                            lb_EMG_Down.Text = "未急停";
                            lb_Reset_Down.Text = "不需要复位";
                            lb_Run_Down.Text = "启动";
                            lb_Lock_Down.Text = "无异常";
                            lb_ORG_Down.Text = "无异常";
                            lb_LMT_Down.Text = "无异常";
                            lb_H_Down.Text = "有托盘";
                            lb_L_Down.Text = "有托盘";
                        }));
                    }
                    else
                    {
                        _downLoad = false;
                        this.BeginInvoke(new Action(() =>
                        {
                            if (CheckCoil(GlobalVar.Drop_Modbus.Coils.EMG) == 1) lb_EMG_Down.Text = "急停中...";
                            if (CheckCoil(GlobalVar.Drop_Modbus.Coils.Reset) == 1) lb_Reset_Down.Text = "需要复位";
                            if (CheckCoil(GlobalVar.Drop_Modbus.Coils.RunTime) == 1) lb_Run_Down.Text = "未启动";
                            if (CheckCoil(GlobalVar.Drop_Modbus.Coils.Lock) == 1) lb_Lock_Down.Text = "异常";
                            if (CheckCoil(GlobalVar.Drop_Modbus.Coils.ORG) == 1) lb_ORG_Down.Text = "异常";
                            if (CheckCoil(GlobalVar.Drop_Modbus.Coils.LMT) == 1) lb_LMT_Down.Text = "异常";
                            if (CheckCoil(GlobalVar.Drop_Modbus.Coils.BoardCheck) == 1) lb_H_Down.Text = "无托盘";
                            if (CheckCoil(GlobalVar.Drop_Modbus.Coils.BoardArrival) == 1) lb_L_Down.Text = "无托盘";
                        }));
                    }

                }
                catch { }
            }
        }

        ///上料机线程
        private void Feed_Check()
        {
            while (!GlobalVar.SoftWareShutDown)
            {
                Thread.Sleep(200);
                try
                {
                    // if (_feedMoved) GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.CommitSignal, true);//上料完成，信号置为0
                    if (CheckCoil(GlobalVar.Feed_Modbus.Coils.BoardReady) == 0)//下料准备好
                    {
                        _upLoad = true;
                        this.BeginInvoke(new Action(() =>
                        {
                            lb_EMG_UP.Text = "未急停";
                            lb_Reset_UP.Text = "不需要复位";
                            lb_Run_UP.Text = "启动";
                            lb_Lock_UP.Text = "无异常";
                            lb_ORG_UP.Text = "无异常";
                            lb_LMT_UP.Text = "无异常";
                            lb_H_UP.Text = "有托盘";
                            lb_L_UP.Text = "有托盘";
                        }));
                    }
                    else
                    {
                        _upLoad = false;
                        this.BeginInvoke(new Action(() =>
                        {
                            if (CheckCoil(GlobalVar.Feed_Modbus.Coils.EMG) == 1) lb_EMG_UP.Text = "急停中...";
                            if (CheckCoil(GlobalVar.Feed_Modbus.Coils.Reset) == 1) lb_Reset_UP.Text = "需要复位";
                            if (CheckCoil(GlobalVar.Feed_Modbus.Coils.RunTime) == 1) lb_Run_UP.Text = "未启动";
                            if (CheckCoil(GlobalVar.Feed_Modbus.Coils.Lock) == 1) lb_Lock_UP.Text = "异常";
                            if (CheckCoil(GlobalVar.Feed_Modbus.Coils.ORG) == 1) lb_ORG_UP.Text = "异常";
                            if (CheckCoil(GlobalVar.Feed_Modbus.Coils.LMT) == 1) lb_LMT_UP.Text = "异常";
                            if (CheckCoil(GlobalVar.Feed_Modbus.Coils.BoardCheck) == 1) lb_H_UP.Text = "无托盘";
                            if (CheckCoil(GlobalVar.Feed_Modbus.Coils.BoardArrival) == 1) lb_L_UP.Text = "无托盘";
                        }));
                    }
                }
                catch { }

            }
        }
        /// <summary>
        /// 检查线圈的值 【1：False；0：True】
        /// </summary>
        /// <param name="coil">判断的线圈</param>
        private int CheckCoil(Coil coil)
        {
            return coil.Value ? 0 : 1;
        }
        #region 作业状态
        private void checkBox_UpBoardAuto_Click(object sender, EventArgs e)
        {
            if (checkBox_FeedBoardAuto.Checked)
            {
                checkBox_FeedBoardAuto.Checked = false;
                UpLoadRun = false;
            }
            else
            {
                checkBox_FeedBoardAuto.Checked = true;
                UpLoadRun = true;
            }
        }

        private void checkBox_DownBoardAuto_Click(object sender, EventArgs e)
        {
            if (checkBox_DropBoardAuto.Checked)
            {
                checkBox_DropBoardAuto.Checked = false;
                UnderLoadRun = false;
            }
            else
            {
                checkBox_DropBoardAuto.Checked = true;
                UnderLoadRun = true;
            }
        }

        private void checkBox_DownBoardAuto_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkBox_DropBoardAuto.Checked)
                {
                    UnderLoadRun = true;
                    SetDownEnable(false);

                    GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.ManualToAuto, true);
                    GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.AllowRun, true);
                }
                else
                {
                    SetDownEnable(true);
                    UnderLoadRun = false;

                    GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.ManualToAuto, false);
                    GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.AllowRun, false);
                }
            }
            catch (Exception ex)
            {
                log.AddERRORLOG("下料机切换运行模式异常:" + ex.Message);
            }
        }

        private void checkBox_UpBoardAuto_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkBox_FeedBoardAuto.Checked)
                {
                    UpLoadRun = true;
                    SetUpEnable(false);
                    GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.ManualToAuto, true);
                    GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.AllowRun, true);
                }
                else
                {
                    SetUpEnable(true);
                    UpLoadRun = false;
                    GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.ManualToAuto, false);
                    GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.AllowRun, false);
                }
            }
            catch (Exception ex)
            {
                log.AddERRORLOG("上料机切换运行模式异常:" + ex.Message);
            }
        }
        #endregion

        private void btn_Update_Up_Click(object sender, EventArgs e)
        {
            GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.HoldingRegisters.BoardSpeed, int.Parse(tb_Speed_Up.Text));//速度
            GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.HoldingRegisters.BoardAcc, int.Parse(tb_Acc_Up.Text));//加速度
            GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.HoldingRegisters.BoardToPosition, int.Parse(tb_Position_Up.Text));//位置

            GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.UpdatePara, false);//更新参数

            myfunction.WriteIniString(GlobalVar.gl_inisection_Para, GlobalVar.gl_iniKey_FeedSpeed, tb_Speed_Up.Text);
            myfunction.WriteIniString(GlobalVar.gl_inisection_Para, GlobalVar.gl_iniKey_FeedAcc, tb_Acc_Up.Text);
            myfunction.WriteIniString(GlobalVar.gl_inisection_Para, GlobalVar.gl_iniKey_UPPostion, tb_Position_Up.Text);
        }

        private void btn_Update_Down_Click(object sender, EventArgs e)
        {
            GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.HoldingRegisters.BoardSpeed, int.Parse(tb_Speed_Down.Text));//速度
            GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.HoldingRegisters.BoardAcc, int.Parse(tb_Acc_Down.Text));//加速度
            GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.HoldingRegisters.BoardToPosition, int.Parse(tb_Position_Down.Text));//位置

            GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.UpdatePara, false);//更新参数

            myfunction.WriteIniString(GlobalVar.gl_inisection_Para, GlobalVar.gl_iniKey_FeedSpeed, tb_Speed_Down.Text);
            myfunction.WriteIniString(GlobalVar.gl_inisection_Para, GlobalVar.gl_iniKey_FeedAcc, tb_Acc_Down.Text);
            myfunction.WriteIniString(GlobalVar.gl_inisection_Para, GlobalVar.gl_iniKey_DropPostion, tb_Position_Down.Text);
        }

        #region 按钮事件
        private void btn_Up_UpLoad_MouseDown(object sender, MouseEventArgs e)
        {
            GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.BoardUpJOG, false);
        }

        private void btn_Up_UpLoad_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.BoardUpJOG, true);
        }

        private void btn_Down_UpLoad_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.BoardUnderJOG, true);
        }

        private void btn_Down_UpLoad_MouseDown(object sender, MouseEventArgs e)
        {
            GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.BoardUnderJOG, false);
        }

        private void btn_ORG_UpLoad_Click(object sender, EventArgs e)
        {

        }

        private void btn_PTP_UpLoad_Click(object sender, EventArgs e)
        {
            GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.BoardToPosition, false);
        }

        private void btn_Cline_UpLoad_Click(object sender, EventArgs e)
        {
            GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.BoardCline, false);
        }

        private void btn_Up_DownLoad_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.BoardUpJOG, true);
        }

        private void btn_Up_DownLoad_MouseDown(object sender, MouseEventArgs e)
        {
            GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.BoardUpJOG, false);
        }

        private void btn_Down_DownLoad_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.BoardUnderJOG, true);
        }

        private void btn_Down_DownLoad_MouseDown(object sender, MouseEventArgs e)
        {
            GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.BoardUnderJOG, false);
        }

        private void btn_ORG_DownLoad_Click(object sender, EventArgs e)
        {
            GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.BoardToORG, false);
        }

        private void btn_PTP_DownLoad_Click(object sender, EventArgs e)
        {
            GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.BoardToPosition, false);
        }

        private void btn_Cline_DownLoad_Click(object sender, EventArgs e)
        {
            GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.BoardCline, false);
        }
        private void btn_ORG_UpLoad_MouseDown(object sender, MouseEventArgs e)
        {
            GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.BoardToORG, false);
        }
        private void btn_ORG_UpLoad_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.BoardToORG, true);
        }

        #endregion


    }
}
