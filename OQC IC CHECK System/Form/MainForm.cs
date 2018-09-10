using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Advantech.Motion;
using static HalconCCD.EnumValue;

namespace OQC_IC_CHECK_System
{
    public partial class MainForm : Form
    {
        #region 窗体参数
        private float X; private float Y;
        #endregion

        private string version = string.Empty;//当前软件版本
        MyFunction myfunction = new MyFunction();
        Logs log = Logs.LogsT();
        private System.Collections.Concurrent.ConcurrentQueue<TextInfo> ShowLog = new System.Collections.Concurrent.ConcurrentQueue<TextInfo>();//软件界面右下方的日志
        private readonly Color DefaultTextColor = SystemColors.ControlLightLight;//文字默认颜色

        /// <summary>
        /// 启动信号【启动按钮触发，出现异常或者手动停止时关闭信号】
        /// </summary>
        private ManualResetEventSlim StartSignal = new ManualResetEventSlim(false);
        /// <summary>
        /// IC解析完成
        /// </summary>
        private AutoResetEvent ICAssistComplete = new AutoResetEvent(false);
        /// <summary>
        /// 托盘到IC拍照位置信号
        /// </summary>
        private AutoResetEvent ICBoardArriveCCD = new AutoResetEvent(false);
        /// <summary>
        /// IC轴到上料位置
        /// </summary>
        private AutoResetEvent ICFeedArrive = new AutoResetEvent(false);
        /// <summary>
        /// PCS轴到上料位置 
        /// </summary>
        private AutoResetEvent PCSFeedArrive = new AutoResetEvent(false);

        /// <summary>
        /// PCS解析完成信号
        /// </summary>
        private AutoResetEvent PCSPhotoComplete = new AutoResetEvent(false);
        /// <summary>
        /// PCS检查机允许下料信号
        /// </summary>
        private AutoResetEvent PCSAllowDropSignal = new AutoResetEvent(false);
        /// <summary>
        /// 单反检查工位到位信号
        /// </summary>
        private AutoResetEvent PCSBoardArriveCCD = new AutoResetEvent(false);
        /// <summary>
        /// 空闲时间
        /// </summary>
        private Stopwatch IdleTime = new Stopwatch();
        /// <summary>
        /// 光源开启时间
        /// </summary>
        private Stopwatch LightOpenTime = new Stopwatch();
        /// <summary>
        /// 按钮-确认
        /// </summary>
        private bool confirm = false;
        /// <summary>
        /// 判断PCS是否已经连接
        /// </summary>
        private bool PCSConnected = false;
        /// <summary>
        /// 末次下料
        /// </summary>
        private bool LastBoard = false;

        public MainForm()
        {
            InitializeComponent();

            GlobalVar.CCD = new HalconCCD.CCDShow();
            InitialControls();
            GlobalVar.gl_IntPtr_MainWindow = this.Handle;
            this.Resize += new EventHandler(Form_Resize);

            X = this.Width;
            Y = this.Height;
            setTag(this);

        }

        private void InitialControls()
        {
            InitCCD();
            panel_CCD.Controls.Add(GlobalVar.CCD);
            GlobalVar.CCD.Dock = DockStyle.Fill;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Font = new Font("Microsoft YaHei", 18F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(134)));

            this.statusStrip1.Location = new System.Drawing.Point(0, 1044);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1920, 36);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "MachineStatus";
            this.statusStrip1.BackColor = Color.FromArgb(0, 192, 192);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
        }

        private void InitCCD()
        {
            try
            {
                GlobalVar.CCD.CameraDefine = "ICTest";
                GlobalVar.CCD.ConnectMode = GlobalVar.CCDMode;
                GlobalVar.CCD.OpenCamera();
            }
            catch (Exception ex)
            {
                log.AddERRORLOG("初始化相机失败:" + ex.StackTrace);
                MsgBoxAlarm(ex.Message + "\r\n请检查相机配置，然后重新开启软件", false);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            label_Info.Text = GlobalVar.Product;
            SetLabelText(label_BoardCount, GlobalVar.BoardCount.ToString("0"));//更新托盘数量
            SetLabelText(label_Yeild_Board, "0%");
            if (GlobalVar.BoardCount != 0) SetLabelText(label_Yeild_Total, ((Convert.ToDouble(GlobalVar.BoardCount * GlobalVar.ICCount * 2 - GlobalVar.ICFailCount) / Convert.ToDouble(GlobalVar.BoardCount * GlobalVar.ICCount * 2)) * 100).ToString("0.00") + "%");
            else SetLabelText(label_Yeild_Total, "0%");
            this.version = myfunction.GetVersion();


            AddEvent();
            OpenThread();

            this.IdleTime.Start();

           // UpdateMachineStatus();取消刷新位置信息--2018.8.29 [lqz]
            GlobalVar.Machine.ForceAlarmUpdate();
            AllowModbusRun(true);

            //   GlobalVar.AxisPCI.AddAxisIntoGroup(GlobalVar.AxisX.LinkIndex);
            //  GlobalVar.AxisPCI.AddAxisIntoGroup(GlobalVar.AxisY.LinkIndex);
            softVersion.Text = string.Format("Ver:{0}", this.version);
        }

        /// <summary>
        /// 允许上下料机运行
        /// </summary>
        /// <param name="able"></param>
        private static void AllowModbusRun(bool able)
        {
            GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.ManualToAuto, true);//上料机自动运行
            GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.ManualToAuto, true);//下料机自动运行
            GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.AllowRun, able);//上料机允许作业
            GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.AllowRun, able);//下料机允许作业
        }

        ToolStripLabel AxisXStatus = new ToolStripLabel();
        ToolStripLabel AxisYStatus = new ToolStripLabel();
        ToolStripLabel AxisAStatus = new ToolStripLabel();
        ToolStripLabel AxisCStatus = new ToolStripLabel();
        ToolStripLabel AxisDStatus = new ToolStripLabel();
        ToolStripLabel AxisBStatus = new ToolStripLabel();
        private void UpdateMachineStatus()
        {
            try
            {
                if (this.statusStrip1.InvokeRequired)
                {
                    this.statusStrip1.BeginInvoke(new Action(delegate
                    {
                        UpdateMachineStatus();
                    }));
                }
                else
                {
                    this.statusStrip1.Items.Clear();

                    List<ToolStripItem> ss = new List<ToolStripItem>();

                    ss.Add(new ToolStripSeparator());
                    AxisXStatus = new ToolStripLabel(string.Format("X轴:{0}mm", GlobalVar.AxisPCI.Position_X.ToString("#0.000")));
                    ss.Add(AxisXStatus);
                    ss.Add(new ToolStripSeparator());
                    AxisYStatus = new ToolStripLabel(string.Format("Y轴:{0}mm", GlobalVar.AxisPCI.Position_Y.ToString("#0.000")));
                    ss.Add(AxisYStatus);
                    ss.Add(new ToolStripSeparator());
                    AxisAStatus = new ToolStripLabel(string.Format("上料:{0}mm", GlobalVar.AxisPCI.Position_A.ToString("#0.000")));
                    ss.Add(AxisAStatus);
                    ss.Add(new ToolStripSeparator());
                    AxisCStatus = new ToolStripLabel(string.Format("IC轴:{0}mm", GlobalVar.AxisPCI.Position_C.ToString("#0.000")));
                    ss.Add(AxisCStatus);
                    ss.Add(new ToolStripSeparator());
                    AxisDStatus = new ToolStripLabel(string.Format("单反轴:{0}mm", GlobalVar.AxisPCI.Position_D.ToString("#0.000")));
                    ss.Add(AxisDStatus);
                    ss.Add(new ToolStripSeparator());
                    AxisBStatus = new ToolStripLabel(string.Format("z", GlobalVar.AxisPCI.Position_B.ToString("#0.000")));
                    ss.Add(AxisCStatus);
                    ss.Add(new ToolStripSeparator());
                    ToolStripStatusLabel tlspring = new ToolStripStatusLabel();
                    tlspring.Spring = true;
                    ss.Add(tlspring);
                    ss.Add(new ToolStripStatusLabel(string.Format("Ver:{0}", version)));

                    this.statusStrip1.Items.AddRange(ss.ToArray());
                }
            }
            catch (Exception ex)
            {
                AddLogStr("更新机台状态异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 增加事件
        /// </summary>
        private void AddEvent()
        {
            GlobalVar.Machine.Event_AlarmUpdate += new EntireMachine.dele_AlarmUpdate(Machine_Event_AlarmUpdate);
            GlobalVar.Machine.Event_Pause += new EntireMachine.dele_Pause(Machine_Event_Pause);

            if (GlobalVar.AxisPCI != null)
            {
                GlobalVar.AxisPCI.Event_UpdatePositionX += AxisPCI_Event_UpdatePositionX;
                GlobalVar.AxisPCI.Event_UpdatePositionY += AxisPCI_Event_UpdatePositionY;
                GlobalVar.AxisPCI.Event_UpdatePositionA += AxisPCI_Event_UpdatePositionA;
                GlobalVar.AxisPCI.Event_UpdatePositionC += AxisPCI_Event_UpdatePositionC;
                GlobalVar.AxisPCI.Event_UpdatePositionD += AxisPCI_Event_UpdatePositionD;
                GlobalVar.AxisPCI.Event_UpdatePositionB += AxisPCI_Event_UpdatePositionB;
                GlobalVar.AxisPCI.Event_MotionMsg += new PCI1285_E.dele_MotionMsg(AxisPCI_Event_MotionMsg);
                GlobalVar.AxisPCI.Event_EMGStop += new PCI1285_E.dele_EMGStop(AxisPCI_EventEMGStop);
                //GlobalVar.AxisPCI.Tag_Buzzer.Event_Trigger += new SignalChangeMonitor.dele_Trigger(Tag_Buzzer_Event_Trigger);
                GlobalVar.AxisPCI.Tag_ICBoardArrived.Event_Trigger += new SignalChangeMonitor.dele_Trigger(Tag_ICBoardArrived_Event_Trigger);
                // GlobalVar.AxisPCI.Tag_ICBoardOut.Event_Trigger += new SignalChangeMonitor.dele_Trigger(Tag_ICBoardOut_Event_Trigger);
                // GlobalVar.AxisPCI.Tag_FPCBoardArrived.Event_Trigger += new SignalChangeMonitor.dele_Trigger(Tag_FPCBoardArrived_Event_Trigger);
                //GlobalVar.AxisPCI.Tag_FPCBoardOut.Event_Trigger += new SignalChangeMonitor.dele_Trigger(Tag_FPCBoardOut_Event_Trigger);

                GlobalVar.AxisPCI.IO_X_Alarm.Event_Trigger += IO_X_Alarm_Event_Trigger;
                GlobalVar.AxisPCI.IO_X_LimtP.Event_Trigger += IO_X_LimtP_Event_Trigger;
                GlobalVar.AxisPCI.IO_X_LimtN.Event_Trigger += IO_X_LimtN_Event_Trigger;
                GlobalVar.AxisPCI.IO_Y_Alarm.Event_Trigger += IO_Y_Alarm_Event_Trigger;
                GlobalVar.AxisPCI.IO_Y_LimtP.Event_Trigger += IO_Y_LimtP_Event_Trigger;
                GlobalVar.AxisPCI.IO_Y_LimtN.Event_Trigger += IO_Y_LimtN_Event_Trigger;
                GlobalVar.AxisPCI.IO_A_Alarm.Event_Trigger += IO_A_Alarm_Event_Trigger;
                GlobalVar.AxisPCI.IO_A_LimtP.Event_Trigger += IO_A_LimtP_Event_Trigger;
                GlobalVar.AxisPCI.IO_A_LimtN.Event_Trigger += IO_A_LimtN_Event_Trigger;
                GlobalVar.AxisPCI.IO_B_Alarm.Event_Trigger += IO_B_Alarm_Event_Trigger;
                GlobalVar.AxisPCI.IO_B_LimtP.Event_Trigger += IO_B_LimtP_Event_Trigger;
                GlobalVar.AxisPCI.IO_B_LimtN.Event_Trigger += IO_B_LimtN_Event_Trigger;
                GlobalVar.AxisPCI.IO_C_Alarm.Event_Trigger += IO_C_Alarm_Event_Trigger;
                GlobalVar.AxisPCI.IO_C_LimtP.Event_Trigger += IO_C_LimtP_Event_Trigger;
                GlobalVar.AxisPCI.IO_C_LimtN.Event_Trigger += IO_C_LimtN_Event_Trigger;
                GlobalVar.AxisPCI.IO_D_Alarm.Event_Trigger += IO_D_Alarm_Event_Trigger;
                GlobalVar.AxisPCI.IO_D_LimtP.Event_Trigger += IO_D_LimtP_Event_Trigger;
                GlobalVar.AxisPCI.IO_D_LimtN.Event_Trigger += IO_D_LimtN_Event_Trigger;


                GlobalVar.AxisPCI.Tag_Lock1.Event_Trigger += Tag_Lock1_Event_Trigger;
                GlobalVar.AxisPCI.Tag_Lock2.Event_Trigger += Tag_Lock2_Event_Trigger;
                GlobalVar.AxisPCI.Tag_LockBefore.Event_Trigger += Tag_LockBefore_Event_Trigger;

                GlobalVar.AxisPCI.Tag_LightSensor.Event_Trigger += Tag_LightSensor_Event_Trigger;
                GlobalVar.AxisPCI.Tag_FeedLeftCheck.Event_MonitorTimeout += Tag_LoadLeftCheck_Event_MonitorTimeout;
                GlobalVar.AxisPCI.Tag_FeedRightCheck.Event_MonitorTimeout += Tag_LoadRightCheck_Event_MonitorTimeout;
                GlobalVar.AxisPCI.Tag_DropCheck.Event_MonitorTimeout += Tag_DropCheck_Event_MonitorTimeout;
                GlobalVar.AxisPCI.Tag_PCSCheck1.Event_MonitorTimeout += Tag_PCSCheck1_Event_MonitorTimeout;
                GlobalVar.AxisPCI.Tag_PCSCheck2.Event_MonitorTimeout += Tag_PCSCheck1_Event_MonitorTimeout;
                GlobalVar.AxisPCI.Tag_PCSCheck3.Event_MonitorTimeout += Tag_PCSCheck1_Event_MonitorTimeout;
                GlobalVar.AxisPCI.Tag_PCSCheck4.Event_MonitorTimeout += Tag_PCSCheck1_Event_MonitorTimeout;

                GlobalVar.AxisPCI.Tag_CylinderFeed.Event_MonitorTimeout += Tag_CylinderFeed_Event_MonitorTimeout;
                GlobalVar.AxisPCI.Tag_CylinderPCS.Event_MonitorTimeout += Tag_CylinderPCS_Event_MonitorTimeout;
                GlobalVar.AxisPCI.Tag_CylinderDrop.Event_MonitorTimeout += Tag_CylinderDrop_Event_MonitorTimeout;

                AxisEvent(true);
            }
        }

        #region 事件

        private void Tag_PCSCheck1_Event_MonitorTimeout(bool status)
        {
            if (status)
            {
                if (!GlobalVar.Machine.Pause)
                {
                    if (!GlobalVar.ICForbiddenMode)
                    {
                        GlobalVar.AxisPCI.StopAllMove();
                        GlobalVar.Machine.Pause = true;
                        MsgBoxAlarm("PCS吸取真空异常，请复位！", true);
                    }
                }
            }
        }

        private void Tag_DropCheck_Event_MonitorTimeout(bool status)
        {
            if (status)
            {
                if (!GlobalVar.Machine.Pause)
                {
                    GlobalVar.Machine.Pause = true;
                    GlobalVar.AxisPCI.StopAllMove();
                    MsgBoxAlarm("下料轴吸取真空异常，请复位！", true);
                }
            }
        }

        private void Tag_LoadRightCheck_Event_MonitorTimeout(bool status)
        {
            if (status)
            {
                if (!GlobalVar.Machine.Pause)
                {
                    GlobalVar.AxisPCI.StopAllMove();
                    GlobalVar.Machine.Pause = true;
                    MsgBoxAlarm("上料轴右吸取真空异常，请复位！", true);
                }
            }
        }

        private void Tag_LoadLeftCheck_Event_MonitorTimeout(bool status)
        {
            if (status)
            {
                if (!GlobalVar.Machine.Pause)
                {
                    GlobalVar.AxisPCI.StopAllMove();
                    GlobalVar.Machine.Pause = true;
                    MsgBoxAlarm("上料轴左吸取真空异常，请复位！", true);
                }

            }
        }

        private void Tag_LightSensor_Event_Trigger(bool status)
        {
            if (!status)
            {
                AddLogStr("正常作业中禁止人员操作,请勿阻挡光栅", true);
                if ((!GlobalVar.Machine.Pause) && GlobalVar.IsLightSensorWorking)
                {
                    GlobalVar.AxisPCI.StopAllMove();
                    GlobalVar.Machine.Pause = true;
                    MsgBoxAlarm("正常作业中禁止人员操作，请取出托盘重新开始！", true);
                }
            }
        }


        private void Machine_Event_Pause(bool IsPause)
        {
            SetBtnEnable(this.btn_LockBig, IsPause);
        }

        private void AxisPCI_Event_UpdatePositionX(double position)
        {
            AxisXStatus.Text = string.Format("X轴:{0}mm", position.ToString("#0.000"));
        }

        private void AxisPCI_Event_UpdatePositionY(double position)
        {
            AxisYStatus.Text = string.Format("Y轴:{0}mm", position.ToString("#0.000"));
        }

        private void AxisPCI_Event_UpdatePositionA(double position)
        {
            AxisAStatus.Text = string.Format("上料轴:{0}mm", position.ToString("#0.000"));
        }

        private void AxisPCI_Event_UpdatePositionC(double position)
        {
            AxisCStatus.Text = string.Format("IC轴:{0}mm", position.ToString("#0.000"));
        }

        private void AxisPCI_Event_UpdatePositionD(double position)
        {
            AxisDStatus.Text = string.Format("单反轴:{0}mm", position.ToString("#0.000"));
        }

        private void AxisPCI_Event_UpdatePositionB(double position)
        {
            AxisBStatus.Text = string.Format("下料轴:{0}mm", position.ToString("#0.000"));
        }

        private void AxisPCI_EventEMGStop()
        {
            if (GlobalVar.EMGSTOP)
            {
                AddLogStr("紧急停止按下", true, Color.Red);
                SetLabelText(label_Status, "急停按下");
                SetLabelColor(label_Status, Color.Black, Color.Red);
                SetGroupboxColor(groupBoxEx_Status, Color.Black);
                SetBtnImage(btn_Run, Properties.Resources.Btn_Pause);
                GlobalVar.Machine.Pause = true;
                AlarmLight(0, true, true);
            }
            else
            {
                AddLogStr("紧急停止松开", false, Color.LightGreen);
                AlarmLight(1, true);
                Reset();
            }
        }

        private void AxisPCI_Event_MotionMsg(string str, bool iserr)
        {
            AddLogStr(str, iserr, iserr ? Color.Red : Color.White);
        }

        private void Machine_Event_AlarmUpdate()
        {
            if (this.listView_AlarmFrequency.InvokeRequired)
            {
                this.listView_AlarmFrequency.BeginInvoke(new Action(delegate { Machine_Event_AlarmUpdate(); }));
            }
            else
            {
                this.listView_AlarmFrequency.BeginUpdate();
                this.listView_AlarmFrequency.Clear();
                this.listView_AlarmFrequency.Columns.Add("异常信息", 195, HorizontalAlignment.Center);
                this.listView_AlarmFrequency.Columns.Add("次数", 45, HorizontalAlignment.Center);
                List<ListViewItem> lsitem = new List<ListViewItem>();

                var dicSort = from objDic in GlobalVar.Machine.AlarmRecord orderby objDic.Value descending select objDic;//Dictonary排序（降序） 如果需要升序  descending 去掉即可
                foreach (KeyValuePair<string, int> kvp in dicSort)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = kvp.Key;
                    item.SubItems.Add(kvp.Value.ToString());
                    lsitem.Add(item);
                }
                this.listView_AlarmFrequency.Items.AddRange(lsitem.ToArray());
                this.listView_AlarmFrequency.EndUpdate();

                myfunction.SaveAlarmRecord();//保存文件
            }
        }

        private void IO_X_Alarm_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("X轴报警");
        }

        private void IO_X_LimtP_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("X轴到达正限位");
        }

        private void IO_X_LimtN_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("X轴到达负限位");
        }

        private void IO_Y_Alarm_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("Y轴报警");
        }

        private void IO_Y_LimtP_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("Y轴到达正限位");
        }

        private void IO_Y_LimtN_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("Y轴到达负限位");
        }

        private void IO_A_Alarm_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("上料轴报警");
        }

        private void IO_A_LimtP_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("上料轴到达正限位");
        }

        private void IO_A_LimtN_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("上料轴到达负限位");
        }

        private void IO_B_Alarm_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("下料轴报警");
        }

        private void IO_B_LimtP_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("下料轴到达正限位");
        }

        private void IO_B_LimtN_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("下料轴到达负限位");
        }

        private void IO_C_Alarm_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("IC轴报警");
        }

        private void IO_C_LimtP_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("IC轴到达正限位");
        }

        private void IO_C_LimtN_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("IC轴到达负限位");
        }

        private void IO_D_Alarm_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("单反轴报警");
        }

        private void IO_D_LimtP_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("单反轴到达正限位");
        }

        private void IO_D_LimtN_Event_Trigger(bool status)
        {
            if (status && !GlobalVar.Machine.Reset)
                AddAlarm("单反轴到达负限位");
        }
        private void Tag_ICBoardArrived_Event_Trigger(bool status)
        {
        }


        List<Dictionary<int, string>> result_list = new List<Dictionary<int, string>>();//存储结果

        private void ICPhotoAndAnalasit()
        {
            try
            {
                AddLogStr("托盘到达IC拍照位置，开始拍照解析IC条码");
                GlobalVar.IC_Barcode_Dic.Clear();
                Dictionary<int, string> result = new Dictionary<int, string>();
                GlobalVar.BoardCount++;//托盘数量加1
                myfunction.WriteIniString(GlobalVar.gl_inisection_Sheet, GlobalVar.gl_iniKey_BoardCount, GlobalVar.BoardCount.ToString("0"));
                SetLabelText(label_BoardCount, GlobalVar.BoardCount.ToString("0"));//更新托盘数量
                Stopwatch ICtime = new Stopwatch();
                m_current_num = 0;
                GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.LightControlSTB1, true);//open光源
                ICStart = true;
                LightOpenTime.Start();
                ICtime.Start();
                GlobalVar.CCD.PlayerOnce();
                GlobalVar.CCD.StartWork(GlobalVar.ICPointList.Count);
                for (int i = 0; i < GlobalVar.ICPointList.Count; i++)
                {
                    double dis_X, dis_Y;
                    dis_X = GlobalVar.ICPointList[i].Pos_X;
                    dis_Y = GlobalVar.ICPointList[i].Pos_Y;
                    CheckStatus();
                    GlobalVar.AxisPCI.FixPointMotion(dis_X, dis_Y, false);
                    GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisX.LinkIndex);
                    GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisY.LinkIndex);
                    //CheckAxisPosition(GlobalVar.AxisX.LinkIndex);//检查X轴位置--2018.8.10 
                    //CheckAxisPosition(GlobalVar.AxisY.LinkIndex);//检查Y轴位置--2018.8.10
                    Thread.Sleep(100);
                    AddLogStr("第" + (i + 1) + "次拍照:X:" + dis_X.ToString("0.000") + "Y:" + dis_Y.ToString("0.000"));
                    #region ic拍照解析流程
                    if (GlobalVar.CCD.Status != CCDStatus.Offline && GlobalVar.CCD.Status != CCDStatus.Online)
                    {
                        AddLogStr("相机未准备好!", true, Color.Red);//取消报警[2018.08.01]
                        while (GlobalVar.CCD.Status != CCDStatus.Offline && GlobalVar.CCD.Status != CCDStatus.Online)
                        {
                            RestartCCD();
                            Thread.Sleep(10);
                        }
                        CheckStatus();
                    }

                    string result_Str = "";
                    result_Str = GlobalVar.CCD.GrabImage_Working(i, GlobalVar.PictureSavePath, false);
                    if (result_Str == null)
                        result_Str = "null|null";

                    log.AddCommLOG("第" + (i + 1) + "次拍照,条码:" + result_Str);
                    string[] result_arr = result_Str.Split('|');
                    //判断条码长度
                    if (result_arr[0].Length < 10) result_arr[0] = "null";
                    if (result_arr[1].Length < 10) result_arr[1] = "null";
                    if ((i / GlobalVar.IC_Rows) % 2 == 0)
                    {
                        ////保存结果
                        //GlobalVar.IC_Barcode_Dic.Add(2 * i, result_arr[1]);
                        //GlobalVar.IC_Barcode_Dic.Add(2 * i + 1, result_arr[0]);
                        if (result.Keys.Contains((2 * i))) result.Remove((2 * i));
                        if (result.Keys.Contains((2 * i + 1))) result.Remove((2 * i + 1));
                        result.Add(2 * i, result_arr[1]);
                        result.Add(2 * i + 1, result_arr[0]);
                    }
                    else
                    {
                        //保存结果
                        //GlobalVar.IC_Barcode_Dic.Add(2 * i, result_arr[0]);
                        //GlobalVar.IC_Barcode_Dic.Add(2 * i + 1, result_arr[1]);
                        if (result.Keys.Contains((2 * i))) result.Remove((2 * i));
                        if (result.Keys.Contains((2 * i + 1))) result.Remove((2 * i + 1));
                        result.Add(2 * i, result_arr[0]);
                        result.Add(2 * i + 1, result_arr[1]);
                    }
                    #endregion
                }
                GlobalVar.CCD.EndWork();
                ICStart = false;
                int failCount = 0;
                ICBarcode_Err = false;
                foreach (int i in result.Keys)
                {
                    string index = (i + 1).ToString("00");
                    if (result[i] == "null")
                    {
                        index += ";";
                        GlobalVar.ICFailCount++;
                        failCount++;
                        ICBarcode_Err = true;
                    }
                }
                myfunction.WriteIniString(GlobalVar.gl_inisection_Sheet, GlobalVar.gl_iniKey_ICFailCount, GlobalVar.ICFailCount.ToString("0"));

                SetLabelText(label_Yeild_Total, ((Convert.ToDouble(GlobalVar.BoardCount * GlobalVar.ICCount * 2 - GlobalVar.ICFailCount) / Convert.ToDouble(GlobalVar.BoardCount * GlobalVar.ICCount * 2)) * 100).ToString("0.00") + "%");
                SetLabelText(label_Yeild_Board, ((Convert.ToDouble(GlobalVar.ICCount * 2 - failCount) / Convert.ToDouble(GlobalVar.ICCount * 2)) * 100).ToString("0.00") + "%");

                //GlobalVar.AxisPCI.SetProp_GPSpeed(GlobalVar.m_GPValue_VelHigh_low, GlobalVar.m_GPValue_VelLow_low, GlobalVar.m_GPValue_Acc_low, GlobalVar.m_GPValue_Dec_low);
                SetLabelText(this.label_ICTime, ICtime.Elapsed.TotalSeconds.ToString("F1") + " S");
                ICtime.Reset();
                AddLogStr("IC拍照解析完成");
                result_list.Add(result);//添加测试数据
            }
            catch (Exception ex)
            {
                AddLogStr(ex.Message, true, Color.Red);

                if (ex is ResetMachineErr || ex is PauseMachineErr) return;
                if (ex is ErrReset)
                {
                    MsgBoxAlarm(ex.Message, ((ErrReset)ex).NeedReset);
                    return;
                }
                MsgBoxAlarm(ex.Message, true);
            }
        }



        //private void Tag_ICBoardOut_Event_Trigger(bool status)
        //{
        //    #region 发送IC条码给单反检查机
        //    string barcode_Result = "";
        //    foreach (int i in GlobalVar.IC_Barcode_Dic.Keys)
        //    {
        //        string index = i.ToString("00");
        //        if (GlobalVar.IC_Barcode_Dic[i] == "null") index += ",";
        //        else index += GlobalVar.IC_Barcode_Dic[i] + ",";
        //        barcode_Result += index;
        //    }

        //    AddLogStr("发送IC条码给单反检查程序");
        //    //发送给单反检查机
        //    GlobalVar.PCS_Port.SendMsg(barcode_Result);
        //    #endregion
        //}

        //private void Tag_FPCBoardArrived_Event_Trigger(bool status)
        //{
        //    #region 通知单反检查机到板
        //    AddLogStr("通知单反检查程序托盘到位");
        //    UpdateAction(3);//更新界面


        //    AddLogStr("单反检查程序开始检查条码");
        //    #endregion
        //}

        //private void Tag_FPCBoardOut_Event_Trigger(bool status)
        //{
        //    #region 测试完成，下料
        //    AddLogStr("单反检查完成");
        //    UpdateAction(4);//更新界面

        //    AddLogStr("需要开启投影仪，手动操作");

        //    AddLogStr("不开投影仪，自动下料");
        //    #endregion
        //}


        private void Tag_Lock1_Event_Trigger(bool status)
        {
            SetBtnImage(this.btn_LockBig, myfunction.GetLockBigDIStatus() ? Properties.Resources.Btn_ON : Properties.Resources.Btn_OFF);

            if (!GlobalVar.Machine.Pause)
            {
                GlobalVar.AxisPCI.StopAllEMGMove();
                MsgBoxAlarm("后门大锁打开", true);
            }
        }

        private void Tag_Lock2_Event_Trigger(bool status)
        {
            SetBtnImage(this.btn_LockIn2, myfunction.GetLock2DIStatus() ? Properties.Resources.Btn_ON : Properties.Resources.Btn_OFF);

            if (!GlobalVar.Machine.Pause)
            {
                GlobalVar.AxisPCI.StopAllEMGMove();
                MsgBoxAlarm("后门小锁打开", true);
            }
        }

        private void Tag_LockBefore_Event_Trigger(bool status)
        {
            SetBtnImage(this.btn_LockBig, status ? Properties.Resources.Btn_ON : Properties.Resources.Btn_OFF);
            if (!GlobalVar.Machine.Pause)
            {
                GlobalVar.AxisPCI.StopAllEMGMove();
                MsgBoxAlarm("前门大锁打开", true);
            }
        }

        /// <summary>
        /// 光栅阻拦报警
        /// </summary>
        /// <param name="errstr"></param>
        /// <param name="NeedReset"></param>
        private void MsgBoxPause(string errstr, bool NeedReset)
        {
            try
            {
                if (string.IsNullOrEmpty(errstr.ToString()) || AlarmBOXShow)
                {
                    AddLogStr("报警已经解除！", false, Color.BlueViolet);
                    return;//无异常或者异常框已经显示，则不显示PLC的异常框
                }
                AlarmLight(0, true, true);//亮红灯
                Thread thd = new Thread(new ThreadStart(delegate
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(delegate
                        {
                            AlarmBOXShow = true;
                            bool reset = MsgBoxPop(errstr.ToString(), Color.Peru, MessageBoxButtons.YesNo);
                            if (reset) Reset();
                            else ContinueMove();
                            AlarmBOXShow = false;
                        }));
                    }
                    else log.AddERRORLOG("线程 更新 界面 异常,Main thread  ");
                }));
                thd.Name = "暂停弹框";
                thd.IsBackground = true;
                thd.Start();
            }
            catch (Exception ex)
            {
                log.AddERRORLOG("暂停弹框异常:" + ex.Message + ex.StackTrace);
            }
        }

        private void ContinueMove()
        {
            AddLogStr("不复位 ，继续运行");
            if (!(GlobalVar.AxisPCI.Tag_LockBefore.CurrentValue)) //门未关，禁止复位
            {
                MsgBox("前门未锁，禁止复位\r\n请上锁前门，然后再复位", "提示", Color.Red);
                return;
            }
            if (!(GlobalVar.AxisPCI.Tag_Lock1.CurrentValue) || !(GlobalVar.AxisPCI.Tag_Lock2.CurrentValue)) //门未关，禁止复位
            {
                MsgBox("后门未锁，禁止复位\r\n请锁后门门，然后再复位", "提示", Color.Red);
                return;
            }
            for (int i = 0; i < GlobalVar.AxisPCI.AxisCount; i++)
            {
                if (GlobalVar.AxisPCI.GetAxisState(i) == AxisState.STA_AX_ERROR_STOP)
                    GlobalVar.AxisPCI.ClearAxisError(i);//清除所有轴的错误
            }
            SetBtnImage(this.btn_Run, Properties.Resources.Btn_Run);
            if (GlobalVar.AxisPCI.Position_A != GlobalVar.AxisPCI.Target_A)
                GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisA.LinkIndex, true, GlobalVar.AxisPCI.Target_A * GlobalVar.ServCMDRate, false);
            if (GlobalVar.AxisPCI.Position_B != GlobalVar.AxisPCI.Target_B)
                GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisB.LinkIndex, true, GlobalVar.AxisPCI.Target_B * GlobalVar.ServCMDRate, false);
            if (GlobalVar.AxisPCI.Position_C != GlobalVar.AxisPCI.Target_C)
                GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisC.LinkIndex, true, GlobalVar.AxisPCI.Target_C * GlobalVar.MotorRate, false);
            if (GlobalVar.AxisPCI.Position_D != GlobalVar.AxisPCI.Target_D)
                GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisD.LinkIndex, true, GlobalVar.AxisPCI.Target_D * GlobalVar.MotorRate, false);
            if (GlobalVar.AxisPCI.Position_X != GlobalVar.AxisPCI.Target_X || GlobalVar.AxisPCI.Position_Y != GlobalVar.AxisPCI.Target_Y)
                GlobalVar.AxisPCI.FixPointMotion(GlobalVar.AxisPCI.Target_X, GlobalVar.AxisPCI.Target_Y, false);
            GlobalVar.AxisPCI.WaitAllMoveFinished();
            GlobalVar.Machine.Pause = false;
        }

        private bool AlarmBOXShow = false;
        /// <summary>
        /// 报警弹框
        /// </summary>
        /// <param name="errstr">报警字符串</param>
        /// <param name="NeedReset">是否需要复位</param>
        private void MsgBoxAlarm(string errstr, bool NeedReset)
        {
            try
            {
                if (string.IsNullOrEmpty(errstr.ToString()) || AlarmBOXShow)
                {
                    AddLogStr("报警已经解除！", false, Color.BlueViolet);
                    return;//无异常或者异常框已经显示，则不显示PLC的异常框
                }
                AlarmLight(0, true, true);//亮红灯
                Thread thd = new Thread(new ThreadStart(delegate
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(delegate
                        {
                            AlarmBOXShow = true;
                            MsgBoxPop(errstr.ToString(), Color.Peru, MessageBoxButtons.OK);
                            //GlobalVar.c_Modbus.AddMsgList(GlobalVar.c_Modbus.Coils.AlarmRelease, true);


                            if (NeedReset) Reset();
                            AlarmBOXShow = false;
                            //if (!MsgBox.IsShow) ErgodicPLCErr();
                        }));
                    }
                    else log.AddERRORLOG("线程 更新 界面 异常,Main thread  ");
                }));
                thd.Name = "异常弹框";
                thd.IsBackground = true;
                thd.Start();
            }
            catch (Exception ex)
            {
                log.AddERRORLOG("异常弹框异常:" + ex.Message + ex.StackTrace);
            }
        }
        private void Tag_CylinderFeed_Event_MonitorTimeout(bool Up)
        {
            if (Up)//上限报警
            {
                if (!myfunction.GetCylinderFeedDOStatus())//气缸上顶
                {
                    GlobalVar.Machine.CylinderAlarm = true;
                    AddAlarm(string.Format("上料轴吸取气缸  {0} 到位 异常", Up ? "上" : "下"));
                }
            }
            else//下限报警
            {
                if (myfunction.GetCylinderFeedDOStatus())//气缸下降
                {
                    GlobalVar.Machine.CylinderAlarm = true;
                    AddAlarm(string.Format("上料轴吸取气缸  {0} 到位 异常", Up ? "上" : "下"));
                }
            }
        }

        private void Tag_CylinderPCS_Event_MonitorTimeout(bool Up)
        {
            if (!GlobalVar.ICForbiddenMode)//【IC】屏蔽不检查
            {
                if (Up)//上限报警
                {
                    if (!myfunction.GetCylinderPCSDOStatus())//气缸上顶
                    {
                        GlobalVar.Machine.CylinderAlarm = true;
                        AddAlarm(string.Format("PCS吸取气缸  {0} 到位 异常", Up ? "上" : "下"));
                    }
                }
                else//下限报警
                {
                    if (myfunction.GetCylinderPCSDOStatus())//气缸下降
                    {
                        GlobalVar.Machine.CylinderAlarm = true;
                        AddAlarm(string.Format("PCS吸取气缸  {0} 到位 异常", Up ? "上" : "下"));
                    }
                }
            }
        }

        private void Tag_CylinderDrop_Event_MonitorTimeout(bool Up)
        {
            if (Up)//上限报警
            {
                if (!myfunction.GetCylinderDropDOStatus())//气缸上顶
                {
                    GlobalVar.Machine.CylinderAlarm = true;
                    AddAlarm(string.Format("下料轴吸取气缸  {0} 到位 异常", Up ? "上" : "下"));
                }
            }
            else//下限报警
            {
                if (myfunction.GetCylinderDropDOStatus())//气缸下降
                {
                    GlobalVar.Machine.CylinderAlarm = true;
                    AddAlarm(string.Format("下料轴吸取气缸  {0} 到位 异常", Up ? "上" : "下"));
                }
            }
        }

        private void AxisEvent(bool Add)
        {
            if (Add)
            {
                GlobalVar.AxisPCI.Tag_SetStart.Event_Trigger += new SignalChangeMonitor.dele_Trigger(AxisPCI_EventStartBtn);
                //GlobalVar.AxisPCI.Tag_BoardArrived.Event_Trigger += new SignalChangeMonitor.dele_Trigger(AxisPCI_EventBoardArrived);
                GlobalVar.AxisPCI.Tag_Reset.Event_Trigger += new SignalChangeMonitor.dele_Trigger(AxisPCI_Event_Reset);
            }
            else
            {
                GlobalVar.AxisPCI.Tag_SetStart.Event_Trigger -= AxisPCI_EventStartBtn;
                //GlobalVar.AxisPCI.Tag_BoardArrived.Event_Trigger -= AxisPCI_EventBoardArrived;
                GlobalVar.AxisPCI.Tag_Reset.Event_Trigger -= AxisPCI_Event_Reset;
            }
        }

        private void AxisPCI_Event_Reset(bool status)
        {
            if (!status) return;//信号由高变低不触发
            Reset(true);
        }

        private void AxisPCI_EventStartBtn(bool status)
        {
            if (!status) return;//信号由高变低不触发
                                //MyFunction.SendMessage(GlobalVar.gl_IntPtr_MsgWindow, GlobalVar.WM_MsgAlarmDisable, (IntPtr)0, (IntPtr)0);

            if (GlobalVar.Machine.Pause)
            {
                AddLogStr("启动无效，软件处于停止中···", true, Color.OrangeRed);
                return;
            }

            if (!PCSConnected)
            {
                AddLogStr("PCS检查机禁止作业，请允许作业!");
                MsgBoxAlarm("PCS检查机禁止作业，请允许作业!", false);
                return;
            }
            //按下启动键，设置为正常速度
            GlobalVar.AxisPCI.SetProp_GPSpeed(GlobalVar.m_GPValue_RunVelHigh_move, GlobalVar.m_GPValue_RunVelLow_move,
                    GlobalVar.m_GPValue_RunAcc_move, GlobalVar.m_GPValue_RunDec_move);
            StartSignal.Set();//收到测试信号
            SetLabelText(label_Status, "作业中···");
            //SetLabelPressStart(false);
            UpdateAction(0);

            AlarmLight(2, true);//亮绿灯
            //  if (GlobalVar.AxisPCI.Tag_ICBoardArrived.CurrentValue) Tag_ICBoardArrived_Event_Trigger(true);//进板信号已经存在，则触发
        }
        #endregion
        /// <summary>
        /// 提示按下启动键
        /// </summary>
        /// <param name="IsShow"></param>
        //private void SetLabelPressStart(bool IsShow)
        //{
        //    if (this.label_PressStart.InvokeRequired)
        //    {
        //        this.label_PressStart.BeginInvoke(new MethodInvoker(delegate { SetLabelPressStart(IsShow); }));
        //    }
        //    else
        //    {
        //        this.label_PressStart.Visible = IsShow;
        //        //this.label_PressStart.Size = new Size(800, 600);
        //        this.label_PressStart.Font = new Font("微软雅黑", 50F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(134)));
        //    }
        //}


        /// <summary>
        /// 增加报警
        /// </summary>
        /// <param name="errstr">报警字符串</param>
        private void AddAlarm(string errstr)
        {
            //AlarmLight(0, true, true);
            AddLogStr(errstr, true, Color.PaleVioletRed);

            if (!GlobalVar.Machine.AlarmRecord.ContainsKey(errstr)) GlobalVar.Machine.AlarmRecord.Add(errstr, 1);
            else GlobalVar.Machine.AlarmRecord[errstr]++;
            GlobalVar.Machine.ForceAlarmUpdate();
        }
        private void OpenThread()
        {
            Thread thd_Procedure = new Thread(AutoProcedure_Feed);
            thd_Procedure.IsBackground = true;
            thd_Procedure.Name = "自动流程-上料";
            thd_Procedure.Start();

            Thread thd_Procedure_IC = new Thread(AutoProcedure_IC);
            thd_Procedure_IC.IsBackground = true;
            thd_Procedure_IC.Name = "自动流程-IC解析";
            thd_Procedure_IC.Start();

            Thread thd_Procedure_PCS = new Thread(AutoProcedure_PCS);
            thd_Procedure_PCS.IsBackground = true;
            thd_Procedure_PCS.Name = "自动流程-PCS条码解析";
            thd_Procedure_PCS.Start();

            Thread Thd_RefreshLog = new Thread(UpdateLog);
            Thd_RefreshLog.IsBackground = true;
            Thd_RefreshLog.Name = "刷新软件界面日志线程";
            Thd_RefreshLog.Start();

            Thread Thd_Alarm = new Thread(Thd_AlarmMonitor);
            Thd_Alarm.IsBackground = true;
            Thd_Alarm.Name = "异常检测线程";
            Thd_Alarm.Start();

            Thread Thd_OtherOperation = new Thread(OtherOperation);
            Thd_OtherOperation.IsBackground = true;
            Thd_OtherOperation.Name = "其他操作线程";
            Thd_OtherOperation.Start();

            Thread Thd_MonitoringAxis = new Thread(MonitoringAxis);
            Thd_MonitoringAxis.IsBackground = true;
            Thd_MonitoringAxis.Name = "监控轴运动线程";
            Thd_MonitoringAxis.Start();

            Thread Thd_LightCheck = new Thread(LightCheck);
            Thd_LightCheck.IsBackground = true;
            Thd_LightCheck.Name = "检测光源开启时间线程";
            Thd_LightCheck.Start();

            Thread Thd_Modbus = new Thread(Modbus_Check);
            Thd_Modbus.IsBackground = true;
            Thd_Modbus.Name = "上下料机modbus通信 线程";
            Thd_Modbus.Start();

            Thread Thd_FeedRightCheck = new Thread(FeedRightCheck);
            Thd_FeedRightCheck.IsBackground = true;
            Thd_FeedRightCheck.Name = "上料轴右吸取真空检查线程";
            //Thd_FeedRightCheck.Start();
            Thread Thd_FeedLeftCheck = new Thread(FeedLeftCheck);
            Thd_FeedLeftCheck.IsBackground = true;
            Thd_FeedLeftCheck.Name = "上料轴左吸取真空检查线程";
            //Thd_FeedLeftCheck.Start();
            Thread Thd_DropCheck = new Thread(DropCheck);
            Thd_DropCheck.IsBackground = true;
            Thd_DropCheck.Name = "下料轴吸取真空检查线程";
            //Thd_DropCheck.Start();
            Thread Thd_PCSCheck = new Thread(PCSCheck);
            Thd_PCSCheck.IsBackground = true;
            Thd_PCSCheck.Name = "PCS轴吸取真空检查线程";
            //Thd_PCSCheck.Start();
        }

        private void FeedRightCheck()
        {
            while (!GlobalVar.SoftWareShutDown)
            {
                try
                {
                    if (!GlobalVar.Machine.Pause)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (GlobalVar.AxisPCI.Tag_FeedRightCheck.CurrentValue) break;
                            else Thread.Sleep(1000);
                        }
                        if (!GlobalVar.AxisPCI.Tag_FeedRightCheck.CurrentValue)
                        {
                            bool signal = false;
                            if (GlobalVar.AxisPCI.GetSingleDO(GlobalVar.AxisPCI.CylinderRightUpper, ref signal))
                            {
                                if (signal)//右吸取开启
                                {
                                    GlobalVar.AxisPCI.StopAllEMGMove();
                                    throw new Exception("上料轴右吸取真空异常!");
                                }
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    AddLogStr(ex.Message);
                    if (ex is ResetMachineErr || ex is PauseMachineErr) continue;
                    if (ex is ErrReset)
                    {
                        MsgBoxAlarm(ex.Message, ((ErrReset)ex).NeedReset);
                        continue;
                    }
                    MsgBoxAlarm(ex.Message, false);
                }
            }
        }
        private void FeedLeftCheck()
        {
            while (!GlobalVar.SoftWareShutDown)
            {
                try
                {
                    if (!GlobalVar.Machine.Pause)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (GlobalVar.AxisPCI.Tag_FeedLeftCheck.CurrentValue) break;
                            else Thread.Sleep(1000);
                        }
                        if (!GlobalVar.AxisPCI.Tag_FeedLeftCheck.CurrentValue)
                        {
                            bool signal = false;
                            if (GlobalVar.AxisPCI.GetSingleDO(GlobalVar.AxisPCI.CylinderLeftUpper, ref signal))
                            {
                                if (signal)//左吸取开启
                                {
                                    GlobalVar.AxisPCI.StopAllEMGMove();
                                    throw new Exception("上料轴左吸取真空异常!");
                                }
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    AddLogStr(ex.Message);
                    if (ex is ResetMachineErr || ex is PauseMachineErr) continue;
                    if (ex is ErrReset)
                    {
                        MsgBoxAlarm(ex.Message, ((ErrReset)ex).NeedReset);
                        continue;
                    }
                    MsgBoxAlarm(ex.Message, false);
                }
            }
        }
        private void DropCheck()
        {
            while (!GlobalVar.SoftWareShutDown)
            {
                try
                {
                    if (!GlobalVar.Machine.Pause)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (GlobalVar.AxisPCI.Tag_DropCheck.CurrentValue) break;
                            else Thread.Sleep(1000);
                        }
                        if (!GlobalVar.AxisPCI.Tag_DropCheck.CurrentValue)
                        {
                            bool signal = false;
                            if (GlobalVar.AxisPCI.GetSingleDO(GlobalVar.AxisPCI.CylinderDropUpper, ref signal))
                            {
                                if (signal)//下料吸取开启
                                {
                                    GlobalVar.AxisPCI.StopAllEMGMove();
                                    throw new Exception("下料轴吸取真空异常!");
                                }
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    AddLogStr(ex.Message);
                    if (ex is ResetMachineErr || ex is PauseMachineErr) continue;
                    if (ex is ErrReset)
                    {
                        MsgBoxAlarm(ex.Message, ((ErrReset)ex).NeedReset);
                        continue;
                    }
                    MsgBoxAlarm(ex.Message, false);
                }
            }
        }

        private void PCSCheck()
        {
            while (!GlobalVar.SoftWareShutDown)
            {
                try
                {
                    if (!GlobalVar.Machine.Pause)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if ((GlobalVar.AxisPCI.Tag_PCSCheck1.CurrentValue
                     && GlobalVar.AxisPCI.Tag_PCSCheck2.CurrentValue
                     && GlobalVar.AxisPCI.Tag_PCSCheck3.CurrentValue
                     && GlobalVar.AxisPCI.Tag_PCSCheck4.CurrentValue))
                                break;
                            else
                                Thread.Sleep(1000);
                        }
                        if (!(GlobalVar.AxisPCI.Tag_PCSCheck1.CurrentValue
                      && GlobalVar.AxisPCI.Tag_PCSCheck2.CurrentValue
                      && GlobalVar.AxisPCI.Tag_PCSCheck3.CurrentValue
                      && GlobalVar.AxisPCI.Tag_PCSCheck4.CurrentValue))
                        {
                            bool signal = false;
                            if (GlobalVar.AxisPCI.GetSingleDO(GlobalVar.AxisPCI.CylinderPCSUpper, ref signal))
                            {
                                if (signal)//PCS吸取开启
                                {
                                    GlobalVar.AxisPCI.StopAllEMGMove();
                                    throw new Exception("PCS轴吸取真空异常!");
                                }
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    AddLogStr(ex.Message);
                    if (ex is ResetMachineErr || ex is PauseMachineErr) continue;
                    if (ex is ErrReset)
                    {
                        MsgBoxAlarm(ex.Message, ((ErrReset)ex).NeedReset);
                        continue;
                    }
                    MsgBoxAlarm(ex.Message, false);
                }
            }
        }

        private void Modbus_Check()
        {
            while (!GlobalVar.SoftWareShutDown)
            {
                Thread.Sleep(1000);
                try
                {
                    if (!GlobalVar.SoftWareShutDown)
                    {
                        GlobalVar.Feed_Modbus.ReadCoil();
                        GlobalVar.Drop_Modbus.ReadCoil();
                        this.BeginInvoke(new Action(() =>
                                            {
                                                if (CheckCoil(GlobalVar.Feed_Modbus.Coils.AllowRun) == 0)
                                                {
                                                    label_Feed.Text = "允许作业";
                                                    label_Feed.BackColor = Color.Green;
                                                    label_Err_Feed.Text = "无异常";
                                                    label_Err_Feed.BackColor = Color.Green;
                                                }
                                                else
                                                {
                                                    label_Feed.Text = "禁止作业";
                                                    label_Feed.BackColor = Color.Gray;
                                                }
                                                if (CheckCoil(GlobalVar.Feed_Modbus.Coils.BoardException) == 1)
                                                {
                                                    label_Err_Feed.Text = "有异常";
                                                    label_Err_Feed.BackColor = Color.Red;
                                                    if (CheckCoil(GlobalVar.Feed_Modbus.Coils.EMG) == 1)
                                                    {
                                                        richTextBox_Feed.AppendText("\r\n" + DateTime.Now.ToString("HH:mm:ss") + "-->上料机急停中...");
                                                        MsgBoxAlarm("上料机急停中...", false);
                                                    }
                                                    if (CheckCoil(GlobalVar.Feed_Modbus.Coils.Reset) == 1)
                                                    {
                                                        richTextBox_Feed.AppendText("\r\n" + DateTime.Now.ToString("HH:mm:ss") + "-->上料机需要复位");
                                                        MsgBoxAlarm("上料机需要复位", false);
                                                    }
                                                    if (CheckCoil(GlobalVar.Feed_Modbus.Coils.RunTime) == 1) richTextBox_Feed.AppendText("\r\n" + DateTime.Now.ToString("HH:mm:ss") + "-->上料机未启动");
                                                    if (CheckCoil(GlobalVar.Feed_Modbus.Coils.Lock) == 1) richTextBox_Feed.AppendText("\r\n" + DateTime.Now.ToString("HH:mm:ss") + "-->上料机门禁异常");
                                                    if (CheckCoil(GlobalVar.Feed_Modbus.Coils.ORG) == 1) richTextBox_Feed.AppendText("\r\n" + DateTime.Now.ToString(" HH:mm:ss") + "-->上料机原点信号异常");
                                                    if (CheckCoil(GlobalVar.Feed_Modbus.Coils.LMT) == 1) richTextBox_Feed.AppendText("\r\n" + DateTime.Now.ToString("HH:mm:ss") + "-->上料机正限位异常");
                                                    if (CheckCoil(GlobalVar.Feed_Modbus.Coils.BoardCheck) == 1) richTextBox_Feed.AppendText("\r\n" + DateTime.Now.ToString(" HH:mm:ss") + "-->上料机无托盘");
                                                    if (CheckCoil(GlobalVar.Feed_Modbus.Coils.BoardArrival) == 1) richTextBox_Feed.AppendText("\r\n" + DateTime.Now.ToString(" HH:mm:ss") + "-->上料机无托盘");
                                                }
                                                else
                                                {
                                                    label_Err_Feed.Text = "无异常";
                                                    label_Err_Feed.BackColor = Color.Green;
                                                }
                                                if (CheckCoil(GlobalVar.Drop_Modbus.Coils.AllowRun) == 0)
                                                {
                                                    label_Drop.Text = "允许作业";
                                                    label_Drop.BackColor = Color.Green;
                                                    label_Err_Drop.Text = "无异常";
                                                    label_Err_Drop.BackColor = Color.Green;
                                                }
                                                else
                                                {
                                                    label_Drop.Text = "禁止作业";
                                                    label_Drop.BackColor = Color.Gray;
                                                }
                                                if (CheckCoil(GlobalVar.Drop_Modbus.Coils.BoardException) == 1)
                                                {
                                                    label_Err_Drop.Text = "有异常";
                                                    label_Err_Drop.BackColor = Color.Red;
                                                    if (CheckCoil(GlobalVar.Drop_Modbus.Coils.EMG) == 1)
                                                    {
                                                        richTextBox_Drop.AppendText("\r\n" + DateTime.Now.ToString(" HH:mm:ss") + "-->下料机急停中...");
                                                        MsgBoxAlarm("下料机急停中...", false);
                                                    }
                                                    if (CheckCoil(GlobalVar.Drop_Modbus.Coils.Reset) == 1)
                                                    {
                                                        richTextBox_Drop.AppendText("\r\n" + DateTime.Now.ToString("HH:mm:ss") + "-->下料机需要复位");
                                                        MsgBoxAlarm("下料机需要复位...", false);
                                                    }
                                                    if (CheckCoil(GlobalVar.Drop_Modbus.Coils.RunTime) == 1) richTextBox_Drop.AppendText("\r\n" + DateTime.Now.ToString(" HH:mm:ss") + "-->下料机未启动");
                                                    if (CheckCoil(GlobalVar.Drop_Modbus.Coils.Lock) == 1) richTextBox_Drop.AppendText("\r\n" + DateTime.Now.ToString("HH:mm:ss") + "-->下料机门禁异常");
                                                    if (CheckCoil(GlobalVar.Drop_Modbus.Coils.ORG) == 1) richTextBox_Drop.AppendText("\r\n" + DateTime.Now.ToString("HH:mm:ss") + "-->下料机原点信号异常");
                                                    if (CheckCoil(GlobalVar.Drop_Modbus.Coils.LMT) == 1) richTextBox_Drop.AppendText("\r\n" + DateTime.Now.ToString("HH:mm:ss") + "-->下料机正限位异常");
                                                    if (CheckCoil(GlobalVar.Drop_Modbus.Coils.BoardCheck) == 1) richTextBox_Drop.AppendText("\r\n" + DateTime.Now.ToString("HH:mm:ss") + "-->下料机无托盘");
                                                    if (CheckCoil(GlobalVar.Drop_Modbus.Coils.BoardArrival) == 1) richTextBox_Drop.AppendText("\r\n" + DateTime.Now.ToString("HH:mm:ss") + "-->下料机无托盘");
                                                }
                                                else
                                                {
                                                    label_Err_Drop.Text = "无异常";
                                                    label_Err_Drop.BackColor = Color.Green;
                                                }
                                                richTextBox_Feed.ScrollToCaret();
                                                richTextBox_Drop.ScrollToCaret();
                                            }));
                    }

                }
                catch (Exception ex)
                {
                    AddLogStr(ex.Message, true, Color.Red);
                    if (ex is ResetMachineErr || ex is PauseMachineErr) continue;
                    if (ex is ErrReset)
                    {
                        MsgBoxAlarm(ex.Message, ((ErrReset)ex).NeedReset);
                        continue;
                    }
                }
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
        /// <summary>
        /// 检测光源线程
        /// </summary>
        private void LightCheck()
        {
            while (!GlobalVar.SoftWareShutDown)
            {
                Thread.Sleep(100);
                try
                {
                    if (LightOpenTime.Elapsed.TotalMinutes > 5 && !ICStart)//光源开启5分钟以上
                    {
                        GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.LightControlSTB1, false);//关闭光源
                        LightOpenTime.Stop();
                    }
                }
                catch (Exception ex)
                {
                    AddLogStr(ex.Message, true, Color.Red);

                    if (ex is ResetMachineErr || ex is PauseMachineErr) continue;
                    if (ex is ErrReset)
                    {
                        MsgBoxAlarm(ex.Message, ((ErrReset)ex).NeedReset);
                        continue;
                    }
                    MsgBoxAlarm(ex.Message, true);
                }
            }

        }

        bool allowRun = false;//允许作业
        bool feedSignal = false;//上料信号
        bool feedSucker = false;//上料吸取
        bool feedMotion = false;//上料运动
        bool feedBlow = false;//上料放托盘
        bool ic_PCSMotion = false;//CD轴运动


        /// <summary>
        /// 上料自动流程
        /// </summary>
        private void AutoProcedure_Feed()
        {
            while (!GlobalVar.SoftWareShutDown)
            {
                try
                {
                    Stopwatch watch = new Stopwatch();
                    Thread.Sleep(100);
                    AddLogStr("等待启动按钮按下");
                    StartSignal.Wait();//启动按键是否触发                  
                    AddLogStr("获得启动信号");
                    CheckStatus();

                    #region 等待允许作业
                    AddLogStr("等待PCS检查机允许作业");
                    while (!PCSConnected)
                    {
                        CheckStatus();
                        Thread.Sleep(10);
                    }
                    SetLabelText(label_Status, "作业中...");
                    AddLogStr("等待上料机托盘到位");
                    GlobalVar.Feed_Modbus.ReadCoil();
                    while ((!GlobalVar.Feed_Modbus.Coils.BoardReady.Value))
                    {
                        CheckStatus();
                        if (LastBoard) break;
                        GlobalVar.Feed_Modbus.ReadCoil();
                        //如果上料机无托盘，启动末次下料
                        if ((CheckCoil(GlobalVar.Feed_Modbus.Coils.BoardCheck) == 1) && !IsFirstMove)
                        {
                            LastBoard = true;//上料机无托盘，末次下料
                            AddLogStr("上料机无托盘，末次下料");
                            break;
                        }
                        Thread.Sleep(150);
                    }
                    CheckStatus();
                    while (GlobalVar.Machine.Pause)
                    {
                        Thread.Sleep(10);
                    }
                    CheckStatus();
                    allowRun = true;//该步骤已经走过
                    #endregion

                    #region 上料信号
                    AddLogStr("上料机托盘到位,等待IC轴到上料位");
                    ICFeedWait = false;
                    ICFeedArrive.WaitOne();//等待IC轴到上料位
                    ICFeedWait = true;
                    CheckStatus();
                    feedSignal = true;//该步骤走过
                    #endregion

                    #region 上料轴吸取--上料
                    AddLogStr("IC轴到达上料位,开始吸取制品");
                    GlobalVar.AxisPCI.SetAxisRunSpeed();//设置无制品运行速度
                    CheckStatus();
                    if (IsFirstMove)//第一次吸取动作
                        GlobalVar.AxisPCI.SuckerMotion(1, true);//吸料
                    else if (LastBoard)//末次下料
                        GlobalVar.AxisPCI.SuckerMotion(2, true);
                    else
                        GlobalVar.AxisPCI.SuckerMotion(4, true);

                    if (!(GlobalVar.Feed_Modbus.SendMsg(GlobalVar.Feed_Modbus.Coils.CommitSignal, true)))
                        GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.CommitSignal, true);//通知上料机吸取完成

                    AddLogStr("通知上料机吸取完成");
                    GlobalVar.AxisPCI.SetAxisOperateSpeed();//设置有制品运行速度
                    CheckStatus();
                    feedSucker = true;//该步骤走过
                    #endregion

                    #region 上料轴上料运动
                    while (GlobalVar.FeedMoveForbidden)//等待A轴启用
                    {
                        CheckStatus();
                        Thread.Sleep(50);
                    }
                    GlobalVar.DropMoveForbidden = true;//禁用B轴
                    AddLogStr("上料轴允许运动，运动到上料位置");
                    CheckStatus();
                    UpdateAction(1);//更新界面
                    watch.Start();
                    //运动到IC待料位置
                    GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisA.LinkIndex, true, GlobalVar.Point_FeedRight * GlobalVar.ServCMDRate, false);
                    GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisA.LinkIndex);//等待轴运动完成
                    //CheckAxisPosition(GlobalVar.AxisA.LinkIndex);//检查A轴位置--2018.8.10
                    watch.Stop();
                    AddLogStr("上料CT：" + watch.Elapsed.TotalSeconds.ToString("0.000"));
                    feedMotion = true;//该步骤结束
                    #endregion

                    #region  上料轴放置托盘
                    AddLogStr("等待PCS轴到上料位置");
                    CheckStatus();
                    if (!IsFirstMove) PCSFeedArrive.WaitOne();//等待PCS轴到上料位置
                    CheckStatus();
                    AddLogStr("放置制品");
                    if (IsFirstMove)//第一次吸取动作
                        GlobalVar.AxisPCI.SuckerMotion(1, false);//放料
                    else if (LastBoard)//末次下料
                        GlobalVar.AxisPCI.SuckerMotion(2, false);
                    else
                        GlobalVar.AxisPCI.SuckerMotion(4, false);

                    GlobalVar.AxisPCI.SetAxisRunSpeed();//设置无制品运行速度
                    CheckStatus();
                    feedBlow = true;
                    #endregion

                    #region C、D轴上料动作
                    while (!DropComplete)
                    {
                        if (IsFirstMove) break;
                        Thread.Sleep(10);
                        CheckStatus();
                    }
                    DropComplete = false;
                    watch.Restart();
                    if (!LastBoard && !GlobalVar.ICForbiddenMode) GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisC.LinkIndex, true, GlobalVar.Point_ICPhotoPosition * GlobalVar.MotorRate, false);//运动到IC拍照位置
                    if (!IsFirstMove) GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisD.LinkIndex, true, GlobalVar.Point_PCSPhotoPosition * GlobalVar.MotorRate, false);//运动到PCS解析位置
                    GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisA.LinkIndex, true, GlobalVar.Point_FeedLeft * GlobalVar.ServCMDRate, false);//运动到托盘上料位置                   
                    if (!GlobalVar.ICForbiddenMode)
                    {
                        GlobalVar.AxisPCI.MovetoRefPoint();//X,Y运动到参考位置
                        GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisC.LinkIndex);
                        //CheckAxisPosition(GlobalVar.AxisC.LinkIndex);//检查C轴位置--2018.8.10
                    }
                    watch.Stop();
                    AddLogStr("IC轴上料CT：" + watch.Elapsed.TotalSeconds.ToString("0.000"));
                    AddLogStr("上料轴上料完成");
                    if (!LastBoard) ICBoardArriveCCD.Set();//ic到解析位置信号
                    UpdateAction(2);//更新界面
                    GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisD.LinkIndex);

                    //CheckAxisPosition(GlobalVar.AxisD.LinkIndex);//检查D轴位置--2018.8.10

                    if (!IsFirstMove)
                    {
                        AddLogStr("PCS轴托盘上料完成,查询是否允许作业");
                        while (!PCSConnected)//禁止作业
                        {
                            CheckStatus();
                            Thread.Sleep(10);
                        }
                        PCSBoardArriveCCD.Set();//PCS到解析位置信号
                        if (GlobalVar.ICForbiddenMode)
                        {
                            AddLogStr("通知PCS检查机拍照,等待检查机拍照完成");
                            //与PCS检查机的通信--拍照位置
                            string str = "S01";
                            str = "?" + str + "#" + myfunction.CRC8(str) + "\n";
                            Thread.Sleep(100);
                            GlobalVar.PCS_Port.SendMsg(str);
                        }
                    }
                    GlobalVar.DropMoveForbidden = false;//启用B轴
                    CheckStatus();
                    GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisA.LinkIndex);//等待A轴到位
                    //CheckAxisPosition(GlobalVar.AxisA.LinkIndex);//检查A轴位置--2018.8.10
                    if (!IsFirstMove) GlobalVar.FeedMoveForbidden = true;//禁用A轴
                    CheckStatus();
                    if (IsFirstMove)//第一次吸取动作
                    {
                        PCSFeedArrive.Set();//如果是第一次动作，将等待PCS轴到上料位置信号完成，以免卡住
                        IsFirstMove = false;
                    }
                    if (LastBoard)//末次下料
                    {
                        IsFirstMove = true;
                        LastBoard = false;
                        ICFeedArrive.Set();
                        this.BeginInvoke(new Action(() => { btn_LastBoard.Enabled = true; }));
                        AddLogStr("末次下料结束");
                    }
                    CheckStatus();
                    ic_PCSMotion = true;
                    #endregion
                }
                catch (Exception ex)
                {
                    AddLogStr(ex.Message, true, Color.Red);

                    if (ex is ResetMachineErr || ex is PauseMachineErr) continue;
                    if (ex is ErrReset)
                    {
                        MsgBoxAlarm(ex.Message, ((ErrReset)ex).NeedReset);
                        continue;
                    }
                    MsgBoxAlarm(ex.Message, true);
                }
                finally
                {
                    //循环结束后将所有标志置为false
                    if (allowRun && feedSignal && feedSucker && feedMotion && feedBlow && ic_PCSMotion)
                    {
                        allowRun = false;//允许作业
                        feedSignal = false;//上料信号
                        feedSucker = false;//上料吸取
                        feedMotion = false;//上料运动
                        feedBlow = false;//上料放托盘
                        ic_PCSMotion = false;//CD轴运动
                    }
                    log.AddCommLOG("上料自动线程结束");
                }
            }
            log.AddERRORLOG("上料自动线程异常");
        }

        bool checkCamera = false;//检查相机
        bool icArriveSignal = false;//IC轴到位
        bool icAnalasit = false;//拍照流程
        bool icFeed = false;//IC轴回上料位

        /// <summary>
        /// IC拍照流程
        /// </summary>
        private void AutoProcedure_IC()
        {
            while (!GlobalVar.SoftWareShutDown)
            {
                try
                {
                    Thread.Sleep(100);
                    StartSignal.Wait();//启动按键是否触发

                    if (!GlobalVar.ICForbiddenMode)
                    {
                        #region 检查相机
                        if (GlobalVar.CCD.Status != CCDStatus.Offline && GlobalVar.CCD.Status != CCDStatus.Online)
                        {
                            AddLogStr("相机未准备好!", true, Color.Red);
                            while (GlobalVar.CCD.Status != CCDStatus.Offline && GlobalVar.CCD.Status != CCDStatus.Online)
                            {
                                CheckStatus();
                                RestartCCD();
                                Thread.Sleep(10);
                            }
                        }

                        GlobalVar.CCD.PlayerOnce();//抓取一次图像，避免后续的显示不是完整图
                        CheckStatus();
                        checkCamera = true;

                        #endregion

                        #region IC轴到位信号
                        AddLogStr("等待托盘到IC位置");
                        ICBoardArriveCCD.WaitOne();//等待IC托盘到解析位置信号
                        CheckStatus();
                        icArriveSignal = true;
                        #endregion

                        #region 拍照流程
                        ICPhotoAndAnalasit();//拍照流程
                        CheckStatus();
                        icAnalasit = true;
                        #endregion

                        #region 解析完成,IC轴回上料位
                        CheckStatus();
                        ICAssistComplete.Set();//IC解析完成信号
                        GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisC.LinkIndex, true, GlobalVar.Point_ICFeed * GlobalVar.MotorRate, false);//IC轴到上料位置
                                                                                                                                        //IC轴复位
                        GlobalVar.AxisPCI.Home(GlobalVar.AxisX.LinkIndex);
                        GlobalVar.AxisPCI.Home(GlobalVar.AxisY.LinkIndex);
                        GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisC.LinkIndex);//等待IC轴运动完成
                        CheckStatus();
                        AddLogStr("IC轴回到上料位置");
                        ICFeedArrive.Set();//IC到上料到位信号
                        GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisX.LinkIndex);
                        GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisY.LinkIndex);
                        //CheckAxisPosition(GlobalVar.AxisX.LinkIndex);//检查X轴位置--2018.8.10 
                        //CheckAxisPosition(GlobalVar.AxisY.LinkIndex);//检查Y轴位置--2018.8.10
                        //CheckAxisPosition(GlobalVar.AxisC.LinkIndex);//检查C轴位置--2018.8.10
                        CheckStatus();
                        icFeed = true;
                        #endregion
                    }
                    else
                    {
                        GlobalVar.BoardCount++;//托盘数量加1
                        myfunction.WriteIniString(GlobalVar.gl_inisection_Sheet, GlobalVar.gl_iniKey_BoardCount, GlobalVar.BoardCount.ToString("0"));
                        SetLabelText(label_BoardCount, GlobalVar.BoardCount.ToString("0"));//更新托盘数量
                        ICBoardArriveCCD.WaitOne();//等待IC托盘到解析位置信号
                        CheckStatus();
                        AddLogStr("IC解析屏蔽，不拍照");
                        ICFeedArrive.Set();//IC到上料到位信号
                        icArriveSignal = true;
                    }
                }
                catch (Exception ex)
                {
                    AddLogStr(ex.Message, true, Color.Red);

                    if (ex is ResetMachineErr || ex is PauseMachineErr) continue;
                    if (ex is ErrReset)
                    {
                        MsgBoxAlarm(ex.Message, ((ErrReset)ex).NeedReset);
                        continue;
                    }
                    MsgBoxAlarm(ex.Message, true);
                }
                finally
                {
                    //将步骤标识置为false
                    if (checkCamera && icArriveSignal && icAnalasit && icFeed)
                    {
                        checkCamera = false;//检查相机
                        icArriveSignal = false;//IC轴到位
                        icAnalasit = false;//拍照流程
                        icFeed = false;//IC轴回上料位
                    }
                    log.AddCommLOG("IC拍照自动线程结束");
                }
            }
            log.AddERRORLOG("IC拍照自动线程异常");
        }

        /// <summary>
        /// 重新启动相机
        /// </summary>
        private void RestartCCD()
        {
            GlobalVar.CCD.CloseCamera();
            Thread.Sleep(150);
            GlobalVar.CCD.OpenCamera();
        }
        bool PCSArriveSignal = false;//PCS到位信号
        bool PCSSucker = false;//PCS吸取
        bool PCSWait = false;//等待位置
        bool sendBarcode = false;//发送条码数据
        bool PCSBlow = false;//放置PCS制品
        bool PCSFeedBack = false;//回上料位置
        bool PCSBack = false;//发送回归到位信号
        bool PCSNGBack = false;//判断结果
        bool PCSDrop = false;//下料轴下料
        bool DownSignal = false;//通知下料机下料

        /// <summary>
        /// 下料自动流程
        /// </summary>
        private void  AutoProcedure_PCS()
        {
            Stopwatch TestTime = new Stopwatch();//总CT
            while (!GlobalVar.SoftWareShutDown)
            {
                try
                {
                    Thread.Sleep(100);
                    StartSignal.Wait();//启动按键是否触发

                    CheckStatus();
                    #region PCS到位
                    AddLogStr("等待PCS上料托盘到位");
                    PCSBoardArriveCCD.WaitOne();//等待PCS到位信号
                    UpdateAction(3);//更新界面
                    CheckStatus();
                    PCSArriveSignal = true;
                    #endregion

                    #region PCS气缸吸取制品 --开始作业
                    while (!PCSConnected)//等待连接PCS检查机
                    {
                        CheckStatus();
                        Thread.Sleep(10);
                    }
                    AddLogStr("PCS轴到拍照位置");
                    //if (!GlobalVar.ICForbiddenMode)
                    //{
                    //    CheckStatus();
                    //    AddLogStr("开始吸取制品");
                    //    GlobalVar.AxisPCI.SuckerMotion(5, true);//PCS气缸吸取制品
                    //}
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.PCSLightControl, true);//打开PCS光源
                    CheckStatus();
                    PCSSucker = true;
                    #endregion

                    #region PCS轴到等待位置--开始作业
                    //if (!GlobalVar.ICForbiddenMode)
                    //{
                    //    CheckStatus();
                    //    AddLogStr("PCS轴到拍照等待位置");
                    //    GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisD.LinkIndex, true, GlobalVar.Point_PCSWaitPosition * GlobalVar.MotorRate, false);//PCS轴运动到等待位置
                    //}
                    while (GlobalVar.DropMoveForbidden)//等待启用B轴
                    {
                        CheckStatus();
                        Thread.Sleep(10);
                    }
                    CheckStatus();
                    AddLogStr("PCS作业，禁用A轴");
                    GlobalVar.FeedMoveForbidden = true;//禁用A轴
                    GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisB.LinkIndex, true, GlobalVar.Point_DropLeft * GlobalVar.ServCMDRate, false);//B轴到PCS吸取位置
                    CheckStatus();
                    GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisB.LinkIndex);//等待B轴运动完成
                    if (!GlobalVar.ICForbiddenMode) GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisD.LinkIndex);//等待PCS轴运动完成
                    //CheckAxisPosition(GlobalVar.AxisB.LinkIndex);//检查B轴位置--2018.8.10
                    //CheckAxisPosition(GlobalVar.AxisD.LinkIndex);//检查D轴位置--2018.8.10
                    CheckStatus();
                    PCSWait = true;
                    #endregion

                    #region 发送数据给检查机
                    if (!GlobalVar.ICForbiddenMode)
                    {
                        CheckStatus();
                        AddLogStr("发送条码数据给检查机");
                        SendICBarcode();//到位后发送IC条码数据
                        while (!dataSendComplete)
                        {
                            CheckStatus();
                            Thread.Sleep(10);
                        }
                        dataSendComplete = false;
                    }
                    photocomplete = true;
                    if (!GlobalVar.ICForbiddenMode)
                    {
                        AddLogStr("通知PCS检查机拍照,等待检查机拍照完成");
                        //与PCS检查机的通信--拍照位置
                        string str = "W01";
                        str = "?" + str + "#" + myfunction.CRC8(str) + "\n";
                        //Thread.Sleep(1000);
                        GlobalVar.PCS_Port.SendMsg(str);
                        AddLogStr("发送IC条码数据给PCS检查机");
                    }
                    CheckStatus();
                    sendBarcode = true;
                    #endregion

                    #region PCS气缸放置制品
                    AddLogStr("等待检查机拍照完成");
                    while (!PCSPhoto)
                    {
                        CheckStatus();
                        Thread.Sleep(50);
                    }
                    PCSPhoto = false;
                    photocomplete = false;
                    AddLogStr("检查机拍照完成,PCS轴回下料位");
                    //if (!GlobalVar.ICForbiddenMode)
                    //{
                    //    CheckStatus();
                    //    GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisD.LinkIndex, true, GlobalVar.Point_PCSPhotoPosition * GlobalVar.MotorRate, false);//PCS轴运动到拍照位置
                    //    GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisD.LinkIndex);//等待PCS轴运动完成
                    //    //CheckAxisPosition(GlobalVar.AxisD.LinkIndex);//检查D轴位置--2018.8.10
                    //    CheckStatus();
                    //    //GlobalVar.AxisPCI.SuckerMotion(5, false);//PCS气缸抬起
                    //    //CheckStatus();
                    //}
                    PCSBlow = true;
                    #endregion

                    #region PCS轴回上料位置
                    CheckStatus();
                    GlobalVar.AxisPCI.SetAxisRunSpeed();
                    GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisD.LinkIndex, true, GlobalVar.Point_PCSFeed * GlobalVar.MotorRate, false);//PCS轴运动到上料位置
                    GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisD.LinkIndex);
                    //CheckAxisPosition(GlobalVar.AxisD.LinkIndex);//检查D轴位置--2018.8.10
                    CheckStatus();
                    PCSFeedArrive.Set();
                    GlobalVar.IsLightSensorWorking = false;
                    CheckStatus();
                    PCSFeedBack = true;
                    #endregion

                    #region PCS 发送数据 --回归到位 
                    //与PCS检查机的通信-回归位置
                    string msg = "D";
                    string strs = "?" + msg + "#" + myfunction.CRC8(msg) + "\n";
                    GlobalVar.PCS_Port.SendMsg(strs);
                    CheckStatus();
                    AddLogStr("PCS轴到下料位，等待检查机下料信号");
                    CheckStatus();
                    PCSBack = true;
                    #endregion

                    #region 结果判断，PCS轴回下料位
                    DropedBoard = true;//到达下料位
                    UpdateAction(4);//更新界面
                    while (!PCSResult)
                    {
                        CheckStatus();
                        Thread.Sleep(50);
                    }
                    PCSResult = false;
                    if (PCSNG)
                    {
                        AddLogStr("PCS测试NG，下料轴回下料位");
                        CheckStatus();
                        //GlobalVar.AxisPCI.SetAxisRunSpeed();//设置无制品运行速度
                        GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisB.LinkIndex, true, GlobalVar.Point_DropRight * GlobalVar.ServCMDRate, false);//B轴运动到下料位置                    
                        GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisB.LinkIndex);//等待B轴运动完成
                        //CheckAxisPosition(GlobalVar.AxisB.LinkIndex);//检查B轴位置--2018.8.10
                        PCSNG = false;
                    }
                    CheckStatus();
                    PCSNGBack = true;
                    #endregion

                    #region PCS轴下料
                    while (!PCSComplete)
                    {
                        CheckStatus();
                        Thread.Sleep(50);
                    }
                    PCSComplete = false;
                    AddLogStr("PCS检查完成，等待处理");
                    if (!PCSFoebideDrop)
                    {
                        //等待PCS给下料信号
                        while (!DropSignal)
                        {
                            CheckStatus();
                            Thread.Sleep(10);
                        }
                        DropSignal = false;
                        DropedBoard = false;//接收到下料信号后值为false，避免重复信号
                        AddLogStr("检查机允许下料");
                        GlobalVar.IsLightSensorWorking = true;
                        CheckStatus();
                        GlobalVar.AxisPCI.SuckerMotion(3, true);
                        CheckStatus();
                        GlobalVar.AxisPCI.SetAxisOperateSpeed();//设置有制品运行速度
                        GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisB.LinkIndex, true, GlobalVar.Point_DropRight * GlobalVar.ServCMDRate, false);//B轴运动到下料位置                    
                        GlobalVar.FeedMoveForbidden = false;//启用A轴
                        GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisB.LinkIndex);//等待B轴运动完成
                        GlobalVar.DropMoveForbidden = true;//禁用B轴
                                                                                           // CheckAxisPosition(GlobalVar.AxisB.LinkIndex);//检查B轴位置--2018.8.10
                        GlobalVar.AxisPCI.SetAxisRunSpeed();//设置无制品运行速度
                        CheckStatus();
                        AddLogStr("等待下料机准备完成");
                        GlobalVar.Drop_Modbus.ReadCoil();
                        while (!GlobalVar.Drop_Modbus.Coils.BoardReady.Value)
                        {
                            CheckStatus();
                            GlobalVar.Drop_Modbus.ReadCoil();
                            Thread.Sleep(150);
                        }
                        GlobalVar.AxisPCI.SuckerMotion(3, false);
                        DropComplete = true;
                        AddLogStr("下料轴下料完成");
                        if (!(GlobalVar.Drop_Modbus.SendMsg(GlobalVar.Drop_Modbus.Coils.CommitSignal, true)))
                            GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.CommitSignal, true);//下料完成，信号置为0//通知下料机下料完成

                        AddLogStr("通知下料轴下料完成");
                    }
                    else
                    {
                        if (!GlobalVar.ICForbiddenMode)
                        {
                            AddLogStr("禁止下料轴下料!");
                            CheckStatus();
                            GlobalVar.FeedMoveForbidden = false;//启用A轴
                            GlobalVar.IsLightSensorWorking = true;//启用光栅
                            PCSFoebideDrop = false;
                        }
                        else//【IC屏蔽】
                        {
                            if (PCSConnected)
                            {
                                AddLogStr("PCS检查NG，开始二次照合");
                                CheckStatus();
                                GlobalVar.IsLightSensorWorking = true;
                                GlobalVar.AxisPCI.SetAxisRunSpeed();
                                GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisD.LinkIndex, true, GlobalVar.Point_PCSPhotoPosition * GlobalVar.MotorRate, false);
                                GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisD.LinkIndex);
                                PCSBoardArriveCCD.Set();
                                AddLogStr("通知PCS检查机二次照合,等待检查机拍照完成");
                                //与PCS检查机的通信--拍照位置  
                                string str = "S01";
                                str = "?" + str + "#" + myfunction.CRC8(str) + "\n";
                                Thread.Sleep(100);
                                GlobalVar.PCS_Port.SendMsg(str);
                                PCSFoebideDrop = false;
                                continue;
                            }
                        }
                    }
                    TestTime.Stop();//停止计时
                    SetLabelText(this.label_RunTime, TestTime.Elapsed.TotalSeconds.ToString("0.0"));
                    TestTime.Restart();//计时复位
                    CheckStatus();
                    PCSDrop = true;
                    #endregion

                    #region 通知下料机下料
                    if (!PCSConnected)
                    {
                        AddLogStr("通知下料机下料");
                        if (!(GlobalVar.Drop_Modbus.SendMsg(GlobalVar.Drop_Modbus.Coils.AxisMoveDown, true)))
                            GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.AxisMoveDown, true);//下料完成，信号置为0//通知下料机下料完成
                    }
                    CheckStatus();
                    DownSignal = true;
                    #endregion

                }
                catch (Exception ex)
                {
                    AddLogStr(ex.Message, true, Color.Red);
                    if (ex is ResetMachineErr || ex is PauseMachineErr) continue;
                    if (ex is ErrReset)
                    {
                        MsgBoxAlarm(ex.Message, ((ErrReset)ex).NeedReset);
                        continue;
                    }
                    MsgBoxAlarm(ex.Message, true);
                }
                finally
                {
                    //将信号标识置为false
                    if (PCSArriveSignal && PCSSucker && PCSWait && sendBarcode && PCSFeedBack && PCSBlow && PCSBack && PCSNGBack && PCSDrop && DownSignal)
                    {
                        PCSArriveSignal = false;//PCS到位信号
                        PCSSucker = false;//PCS吸取
                        PCSWait = false;//等待位置
                        sendBarcode = false;//发送条码数据
                        PCSBlow = false;//放置PCS制品
                        PCSFeedBack = false;//回上料位置
                        PCSBack = false;//发送回归到位信号
                        PCSNGBack = false;//判断结果
                        PCSDrop = false;//下料轴下料
                        DownSignal = false;//通知下料机下料
                    }
                    log.AddCommLOG("PCS检查自动流程结束");
                }
            }
            log.AddERRORLOG("PCS检查自动流程异常");
        }
        /// <summary>
        /// 发送IC条码结果
        /// </summary>
        private void SendICBarcode()
        {
            Dictionary<int, string> result = result_list[0];
            #region 发送数据给PCS检查机
            string barcode_Result = "F";
            foreach (int i in result.Keys)
            {
                string index = (i + 1).ToString("00");
                if (result[i] == "null")
                {
                    index += ";";
                }
                else index += result[i] + ";";
                barcode_Result += index;
            }
            string message = "?" + barcode_Result + "#" + myfunction.CRC8(barcode_Result) + "\n";
            GlobalVar.PCS_Port.SendMsg(message);//发送条码信息给PCS检查机
            #endregion
            result_list.RemoveAt(0);
        }

        /// <summary>
        /// 监控轴运动【防撞线程】
        /// </summary>
        private void MonitoringAxis()
        {
            while (!GlobalVar.SoftWareShutDown)
            {
                try
                {
                    StartSignal.Wait();//启动键是否触发
                    if (GlobalVar.AxisPCI.Position_A > GlobalVar.FeedSaveDistance && GlobalVar.AxisPCI.Position_B < GlobalVar.DropSaveDistance)
                    {
                        GlobalVar.AxisPCI.StopAllEMGMove();
                        GlobalVar.Machine.Pause = true;
                        AddLogStr("禁止上下料轴同时在安全区域内运动,禁用A轴");
                        GlobalVar.AxisPCI.StopEMGMove(GlobalVar.AxisA.LinkIndex);//停止A轴
                        GlobalVar.AxisPCI.StopEMGMove(GlobalVar.AxisB.LinkIndex);//停止B轴
                        GlobalVar.FeedMoveForbidden = true;//禁用A轴
                        log.AddCommLOG("防撞线程操作，将B轴移动到安全位置");
                        GlobalVar.AxisPCI.ClearAxisError(GlobalVar.AxisB.LinkIndex);//解除B轴的错误
                        GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisB.LinkIndex, true, GlobalVar.Point_DropRight * GlobalVar.ServCMDRate, false);//移动B轴到安全位置
                        GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisB.LinkIndex);
                        AddLogStr("禁止上下料轴同时在安全区域内运动，请复位！");
                        Reset();
                    }
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    AddLogStr(ex.Message, true, Color.Red);

                    if (ex is ResetMachineErr || ex is PauseMachineErr) continue;
                    if (ex is ErrReset)
                    {
                        MsgBoxAlarm(ex.Message, ((ErrReset)ex).NeedReset);
                        continue;
                    }
                    MsgBoxAlarm(ex.Message, true);
                }
            }
        }

        /// <summary>
        /// IC工位托盘计数
        /// </summary>
        private int IC_BoardCount = 0;

        /// <summary>
        /// PCS工位托盘计数
        /// </summary>
        private int PCS_BoardCount = 0;



        /// <summary>
        /// 下料轴移动到安全区域
        /// </summary>
        private void WaitCAxisMove()
        {
            GlobalVar.AxisPCI.StopMove(GlobalVar.AxisB.LinkIndex);//停止移动下料轴
            GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisB.LinkIndex, true, (GlobalVar.Point_DropLeft) * GlobalVar.ServCMDRate, false);//移动到安全位置[下料位置]
            GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisB.LinkIndex);
        }
        /// <summary>
        /// 上料轴移动到安全区域
        /// </summary>
        private void WaitUAxisMove()
        {
            GlobalVar.AxisPCI.StopMove(GlobalVar.AxisA.LinkIndex);
            GlobalVar.AxisPCI.MoveDIR(GlobalVar.AxisA.LinkIndex, true, (GlobalVar.Point_FeedLeft) * GlobalVar.ServCMDRate, false);//移动到安全区域【上料位置】
            GlobalVar.AxisPCI.WaitSigleMoveFinished(GlobalVar.AxisA.LinkIndex);
        }

        /// <summary>
        /// 检查输入信号【返回信号状态】
        /// </summary>
        /// <param name="signal">信号</param>
        /// <param name="WaitTime">等待时间，单位：10毫秒</param>
        /// <returns></returns>
        private bool CheckDI(BoardSignalDefinition signal, int WaitTime)
        {
            bool result = false;
            for (int i = 0; i < WaitTime; i++)
            {
                if (GlobalVar.AxisPCI.GetSingleDI(signal, ref result))
                {
                    if (result) break;
                }
                Thread.Sleep(10);
            }
            return result;
        }

        /// <summary>
        /// 检查是否已经复位
        /// </summary>
        private void CheckStatus()
        {
            //if(GlobalVar.EMGSTOP)
            if (GlobalVar.Machine.Pause)
            {
                if (GlobalVar.Machine.CylinderAlarm) throw new Exception("气缸报警");
                if (GlobalVar.Machine.Reset) throw new ResetMachineErr("机台复位");
                if (GlobalVar.Machine.Pause) throw new PauseMachineErr("机台暂停");
            }
            //暂停
            while (GlobalVar.Machine.Pause)
            {
                if (GlobalVar.Machine.Reset) break;
                if (GlobalVar.Machine.CylinderAlarm) throw new Exception("气缸报警");
                Thread.Sleep(10);
            }

        }
        /// <summary>
        ///检测轴位置
        /// </summary>
        private void CheckAxisPosition(int axisNum)
        {
            if (GlobalVar.AxisPCI.GetAxisState(axisNum) == AxisState.STA_AX_READY)
            {
                if (axisNum == GlobalVar.AxisA.LinkIndex)
                {
                    if (GlobalVar.AxisPCI.Position_A != GlobalVar.AxisPCI.Target_A) throw new Exception("A轴位置异常");
                }
                if (axisNum == GlobalVar.AxisB.LinkIndex)
                {
                    if (GlobalVar.AxisPCI.Position_B != GlobalVar.AxisPCI.Target_B) throw new Exception("B轴位置异常");
                }
                if (axisNum == GlobalVar.AxisC.LinkIndex)
                {
                    if (GlobalVar.AxisPCI.Position_C != GlobalVar.AxisPCI.Target_C) throw new Exception("C轴位置异常");
                }
                if (axisNum == GlobalVar.AxisD.LinkIndex)
                {
                    if (GlobalVar.AxisPCI.Position_D != GlobalVar.AxisPCI.Target_D) throw new Exception("D轴位置异常");
                }
                if (axisNum == GlobalVar.AxisX.LinkIndex)
                {
                    if (GlobalVar.AxisPCI.Position_X != GlobalVar.AxisPCI.Target_X) throw new Exception("X轴位置异常");
                }
                if (axisNum == GlobalVar.AxisY.LinkIndex)
                {
                    if (GlobalVar.AxisPCI.Position_Y != GlobalVar.AxisPCI.Target_Y) throw new Exception("Y轴位置异常");
                }
            }
        }

        private void OtherOperation()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => { OtherOperation(); }));
                }
                else
                {
                    Thread.Sleep(300);
#if !DEBUG
                log.AddCommLOG(string.Format("重命名桌面快捷方式结果:{0}\t重命名启动快捷方式结果:{1}",
                         myfunction.RenameDesktopLnk("IC&PCS检查机"),
                         myfunction.RenameStartupLnk("IC&PCS检查机")));
#endif

                    InitMachine();//根据相关信号 初始化界面
                    GlobalVar.PCS_Port = new SerialPortModbus(GlobalVar.PCS_COM);//初始化串口通信
                    GlobalVar.PCS_Port.OpenPCSPort();//打开串口
                    Reset();
                    DeleteImage();
                }
            }
            catch (Exception ex)
            {
                AddLogStr(ex.Message, true, Color.Red);

                if (ex is ResetMachineErr || ex is PauseMachineErr) return; ;
                if (ex is ErrReset)
                {
                    MsgBoxAlarm(ex.Message, ((ErrReset)ex).NeedReset);
                    return;
                }
                MsgBoxAlarm(ex.Message, true);
            }
        }

        /// <summary>
        /// 删除图片
        /// </summary>
        private void DeleteImage()
        {
            Thread thd = new Thread(new ThreadStart(delegate
            {
                List<DirectoryInfo> dirs = new List<DirectoryInfo>();
                string[] Folders = Directory.GetDirectories(GlobalVar.PictureSavePath);
                foreach (string folder in Folders)
                {
                    DirectoryInfo dir = new DirectoryInfo(folder);
                    TimeSpan span = new TimeSpan();
                    span = DateTime.Now - dir.CreationTime;
                    if (span.TotalDays > 30 * 5)
                    {
                        dirs.Add(dir);
                    }
                }
                for (int j = dirs.Count - 1; j >= 0; j--)
                {
                    Directory.Delete(dirs[j].FullName);//删除
                }
            }));
            thd.IsBackground = true;
            thd.Name = "删除图片线程";
            thd.Start();
        }

        /// <summary>
        /// 机台复位
        /// </summary>
        /// <param name="Press">是否为实体按键 按下触发</param>
        private void Reset(bool Press = false)
        {
            if (!Press)
            {
                if (!(GlobalVar.AxisPCI.Tag_LockBefore.CurrentValue)) //门未关，禁止复位
                {
                    AddLogStr("前门未锁，禁止复位\r\n请上锁前门，然后再复位");
                    MsgBox("前门未锁，禁止复位\r\n请上锁前门，然后再复位", "提示", Color.Red);
                    return;
                }
                if (!(GlobalVar.AxisPCI.Tag_Lock1.CurrentValue) || !(GlobalVar.AxisPCI.Tag_Lock2.CurrentValue)) //门未关，禁止复位
                {
                    AddLogStr("后门未锁，禁止复位\r\n请锁后门，然后再复位");
                    MsgBox("后门未锁，禁止复位\r\n请锁后门，然后再复位", "提示", Color.Red);
                    return;
                }
                if (!MsgBox("机台复位", "提示", Color.LightGreen)) return;
            }

            UpdateAction(0);
            SetAxisStatus();
            Thread thd = new Thread(ResetFunction);
            thd.IsBackground = true;
            thd.Name = "复位线程";
            thd.Start();
        }

        /// <summary>
        /// 发送OEE数据
        /// </summary>
        /// <param name="str"></param>
        private void sendOEEMessage(string str)
        {
            str = "C"+str;
            str = "?" + str + "#" + myfunction.CRC8(str) + "\n";
            AddLogStr("上传OEE数据:"+str);
            GlobalVar.OeeSent = false;
            GlobalVar.PCS_Port.SendMsg(str);
            for(int i = 0; i < 3; i++)
            {
                if (GlobalVar.OeeSent)
                {
                    AddLogStr("OEE数据上传成功");
                    return;
                }
                else GlobalVar.PCS_Port.SendMsg(str);
                Thread.Sleep(100);
            }
            AddLogStr("OEE数据上传失败");
        }



        /// <summary>
        /// 更新状态栏中的轴状态
        /// </summary>
        private void SetAxisStatus()
        {
            if (!GlobalVar.Machine.Pause)
            {
                lb_AxisA.Text = "上料轴:正常作业";
                lb_AxisB.Text = "下料轴:正常作业";
                lb_AxisC.Text = "IC搬运轴:正常作业";
                lb_AxisD.Text = "PCS搬运轴:正常作业";
            }
            if(GlobalVar.resetComplete)
            {
                lb_AxisA.Text = "上料轴:复位完成";
                lb_AxisB.Text = "下料轴:复位完成";
                lb_AxisC.Text = "IC搬运轴:复位完成";
                lb_AxisD.Text = "PCS搬运轴:复位完成";
            }
            if (GlobalVar.Machine.Reset)
            {
                lb_AxisA.Text = "上料轴:复位中";
                lb_AxisB.Text = "下料轴:复位中";
                lb_AxisC.Text = "IC搬运轴:复位中";
                lb_AxisD.Text = "PCS搬运轴:复位中";
            }            
        }

        private bool BoardOut = true;//载板是否已经出去
        private int m_current_num = 0;//记录当前获得的图片序号，一张测完后重置
                                      /// <summary>
                                      /// 用于判断是否是第一次吸取动作【开机或复位后第一次】
                                      /// </summary>
        public bool IsFirstMove = true;
        /// <summary>
        /// 用于判断是否是IC拍照中
        /// </summary>
        private bool ICStart = false;
        /// <summary>
        /// 用于判断是否收到拍照完成信号
        /// </summary>
        private bool PCSPhoto = false;

        private bool photocomplete = false;
        /// <summary>
        /// 用于判断是否允许下料信号
        /// </summary>
        private bool DropSignal = false;
        /// <summary>
        /// 用于判断下料载板是否是当前载板
        /// </summary>
        private int DropBoardCount = 0;
        /// <summary>
        /// 用于记录下料托盘是否是当前载板
        /// </summary>
        public bool DropedBoard = false;
        /// <summary>
        /// PCS轴禁止作业
        /// </summary>
        public bool ForbideWork = false;
        /// <summary>
        /// PCS轴再次拍照
        /// </summary>
        private bool PhotoAgain = false;
        /// <summary>
        /// PCS操作完成，下料或不下料
        /// </summary>
        private bool PCSComplete = false;
        /// <summary>
        /// PCS轴禁止下料
        /// </summary>
        private bool PCSFoebideDrop = false;

        private bool ICFeedWait = false;
        /// <summary>
        /// 用于判断IC条码是否为空
        /// </summary>
        private bool ICBarcode_Err = false;
        /// <summary>
        /// 用于判断是否接收到结果
        /// </summary>
        private bool PCSResult = false;
        /// <summary>
        /// 用于判断当前结果是否有NG
        /// </summary>
        private bool PCSNG = false;
        private bool dataSendComplete = false;
        /// <summary>
        /// 用于判断当前托盘是否放到下料机里
        /// </summary>
        private bool DropComplete = true;

        /// <summary>
        /// 机台复位的具体方法
        /// </summary>
        private void ResetFunction()
        {
            try
            {
                GlobalVar.Machine.Reset = true;
                GlobalVar.Machine.Pause = true;
                result_list.Clear();
                PCSComplete = false;
                PCSFoebideDrop = false;
                ICFeedWait = false;
                SetFlowLayoutPanelEnable(false);//禁用左侧按钮
                SetBtnEnable(this.btn_Run, false);
                SetBtnImage(this.btn_Run, Properties.Resources.Btn_Pause);
                this.StartSignal.Reset();
                this.ICBoardArriveCCD.Set();
                this.PCSBoardArriveCCD.Set();

                //this.ICAssistComplete.Set();
                ICFeedArrive.Reset();//复位IC轴上料到位信号
                this.PCSFeedArrive.Reset();//复位PCS轴上料到位信号
                this.PCSAllowDropSignal.Reset();//复位PCS允许下料信号
                this.PCSPhotoComplete.Reset();//复位PCS拍照完成信号
                AllowModbusRun(false);
                MsgBox("请确认机台内是否存在托盘！", "清料", Color.Orange);//提示清料
                SetLabelText(label_Status, "复位中···");
                SetLabelColor(label_Status, Color.AliceBlue, Color.Gray);
                SetGroupboxColor(groupBoxEx_Status, Color.AliceBlue);

                AlarmLight(1, true);//亮黄灯
                ResetDO();//复位输出信号
                GlobalVar.AxisPCI.StopAllMove();//停止所有轴的运动
                for (int i = 0; i < GlobalVar.AxisPCI.AxisCount; i++)
                {
                    GlobalVar.AxisPCI.ClearAxisError(i);//清除所有轴的错误
                }
                GlobalVar.AxisPCI.ResetServ();//开始复位
                Thread.Sleep(500);
                AddLogStr("等待轴运动完成");
                while (!GlobalVar.resetComplete) { Thread.Sleep(10); }
                GlobalVar.AxisPCI.WaitAllMoveFinished();//等待轴复位完成，主要用于提示【待机中】
                //复位完成时关闭置载板到位信号，避免启动后 载板信号未关闭导致流程继续
                this.ICBoardArriveCCD.Reset();
                this.PCSBoardArriveCCD.Reset();
                this.ICAssistComplete.Reset();

                this.ICFeedArrive.Set();//IC轴上料到位信号完成
                AddLogStr("IC轴上料到位信号完成");
                this.PCSFeedArrive.Set();//pcs轴上料到位信 号完成
                this.PCSAllowDropSignal.Set();//PCS允许下料信号完成
                this.PCSPhotoComplete.Set();//PCS拍照信号完成
                AllowModbusRun(true);
                GlobalVar.Feed_Modbus.AddMsgList(GlobalVar.Feed_Modbus.Coils.ResetComplete, true);//复位后给升降信号
                GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.ResetComplete, true);//复位后给升降信号
                GlobalVar.AxisPCI.WaitAllMoveFinished();//等待轴复位完成，主要用于提示【待机中】
                GlobalVar.FeedMoveForbidden = false;//A轴禁用取消
                IsFirstMove = true;
                LastBoard = false;
                GlobalVar.Machine.Reset = false;
                SetBtnEnable(this.btn_Run, true);
                SetFlowLayoutPanelEnable(true);//启用左侧按钮
                this.BeginInvoke(new Action(() => { this.btn_LastBoard.Enabled = true; }));
                SetLabelText(label_Status, "待机中···");
                SetLabelColor(label_Status, Color.DeepSkyBlue, Color.Lime);
                SetGroupboxColor(groupBoxEx_Status, Color.DeepSkyBlue);
                if (ICFeedWait)
                {
                    this.ICFeedArrive.Set();//IC轴上料到位信号完成
                    AddLogStr("IC信号被使用，重置");
                }
                ResetSignal();
            }
            catch (Exception ex)
            {
                MsgBoxAlarm("机台复位异常：" + ex.Message, true);
            }
        }

        private void ResetSignal()
        {
            allowRun = false;//允许作业
            feedSignal = false;//上料信号
            feedSucker = false;//上料吸取
            feedMotion = false;//上料运动
            feedBlow = false;//上料放托盘
            ic_PCSMotion = false;//CD轴运动
            checkCamera = false;//检查相机
            icArriveSignal = false;//IC轴到位
            icAnalasit = false;//拍照流程
            icFeed = false;//IC轴回上料位
            PCSArriveSignal = false;//PCS到位信号
            PCSSucker = false;//PCS吸取
            PCSWait = false;//等待位置
            sendBarcode = false;//发送条码数据
            PCSBlow = false;//放置PCS制品
            PCSFeedBack = false;//回上料位置
            PCSBack = false;//发送回归到位信号
            PCSNGBack = false;//判断结果
            PCSDrop = false;//下料轴下料
            DownSignal = false;//通知下料机下料
        }



        /// <summary>
        /// 弹框【OK或者Cancel，确认输入信号】
        /// </summary>
        /// <param name="Msg">消息</param>
        /// <param name="define">判断的信号</param>
        /// <param name="value">判断信号的值</param>
        /// <param name="Title">标题</param>
        /// <param name="btn">按钮</param>
        /// <returns></returns>
        private DialogResult MsgBoxDIPop(string Msg, BoardSignalDefinition define, bool value, string Title = "提示", MessageBoxButtons btn = MessageBoxButtons.YesNo)
        {
            AddLogStr(Title + Msg, false, Color.LimeGreen);
            MsgBox box;
            switch (btn)
            {
                case MessageBoxButtons.YesNo:
                case MessageBoxButtons.OK:
                    box = new MsgBox(define, value, btn);
                    break;
                default:
                    return DialogResult.Cancel;
            }
            box.Title = Title;
            box.ShowText = Msg;
            box.SetBackColor = Color.LimeGreen;
            return box.ShowDialog();
        }


        /// <summary>
        /// 弹框【OK或者Cancel】
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="backcolor">背景色</param>
        /// <returns></returns>
        private bool MsgBoxPop(string text, Color backcolor, MessageBoxButtons btn)
        {
            using (MsgBox box = new MsgBox(btn))
            {
                box.Title = "提示";
                box.ShowText = text;
                box.SetBackColor = backcolor;
                box.ShowInTaskbar = true;
                if (box.ShowDialog() == DialogResult.OK) return true;
                else return false;
            }
        }

        //复位输出信号
        private void ResetDO()
        {
            GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.LightControlSTB1, false);//光源关闭
            GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.LightControlSTB2, false);//光源关闭
            GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.CylinderFeed, false);//上料轴气缸
            GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.CylinderDrop, false);//下料轴气缸
            GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.CylinderPCS, false);//pcs轴气缸
        }

        /// <summary>
        /// 设置Label的颜色色
        /// </summary>
        /// <param name="lb"></param>
        /// <param name="backcolor">背景色</param>
        /// <param name="forecolor">字体颜色</param>
        private void SetLabelColor(Label lb, Color backcolor, Color forecolor)
        {
            if (lb.InvokeRequired)
            {
                lb.BeginInvoke(new Action(() => { SetLabelColor(lb, backcolor, forecolor); }));
            }
            else
            {
                lb.BackColor = backcolor;
                lb.ForeColor = forecolor;
            }
        }

        /// <summary>
        /// 设置Groupbox的背景颜色
        /// </summary>
        /// <param name="gb"></param>
        /// <param name="backcolor">背景色</param>
        /// <param name="forecolor">字体颜色</param>
        private void SetGroupboxColor(GroupBoxEx gb, Color backcolor)
        {
            if (gb.InvokeRequired)
            {
                gb.BeginInvoke(new Action(() => { SetGroupboxColor(gb, backcolor); }));
            }
            else
            {
                gb.BackColor = backcolor;
            }
        }


        /// <summary>
        /// 设置Label的文本
        /// </summary>
        /// <param name="lb"></param>
        /// <param name="text"></param>
        private void SetLabelText(Label lb, string text)
        {
            if (lb.InvokeRequired)
            {
                lb.BeginInvoke(new Action(() => { SetLabelText(lb, text); }));
            }
            else lb.Text = text;
        }

        private void SetFlowLayoutPanelEnable(bool _Enable)
        {
            if (this.splitContainer_All.Panel1.InvokeRequired)
            {
                this.splitContainer_All.Panel1.Invoke(new Action(delegate { SetFlowLayoutPanelEnable(_Enable); }));
            }
            else
            {
                foreach (Control item in this.splitContainer_All.Panel1.Controls)
                {
                    item.Enabled = _Enable;
                }
            }
        }

        /// <summary>
        /// 界面下方的动作信号显示
        /// </summary>
        /// <param name="Index">0:全部不显示；1：上料；2：IC拍照；3：单反检查；4：下料</param>
        private void UpdateAction(int Index)
        {
            Bitmap boardin_pic = null;
            Bitmap photo_pic = null;
            Bitmap ink_pic = null;
            Bitmap boardout_pic = null;
            Bitmap progress_pic = null;
            switch (Index)
            {
                case 1:
                    boardin_pic = Properties.Resources.LightGreen_Back;
                    photo_pic = Properties.Resources.LightRed_Back;
                    ink_pic = Properties.Resources.LightRed_Back;
                    boardout_pic = Properties.Resources.LightRed_Back;
                    progress_pic = Properties.Resources.Action_BoardIn;
                    break;
                case 2:
                    boardin_pic = Properties.Resources.LightRed_Back;
                    photo_pic = Properties.Resources.LightGreen_Back;
                    ink_pic = Properties.Resources.LightRed_Back;
                    boardout_pic = Properties.Resources.LightRed_Back;
                    progress_pic = Properties.Resources.Action_Ink;
                    break;
                case 3:
                    boardin_pic = Properties.Resources.LightRed_Back;
                    photo_pic = Properties.Resources.LightRed_Back;
                    ink_pic = Properties.Resources.LightGreen_Back;
                    boardout_pic = Properties.Resources.LightRed_Back;
                    progress_pic = Properties.Resources.Action_Ink;
                    break;
                case 4:
                    boardin_pic = Properties.Resources.LightRed_Back;
                    photo_pic = Properties.Resources.LightRed_Back;
                    ink_pic = Properties.Resources.LightRed_Back;
                    boardout_pic = Properties.Resources.LightGreen_Back;
                    progress_pic = Properties.Resources.Action_BoardOut;
                    break;
                default:
                    boardin_pic = Properties.Resources.LightRed_Back;
                    photo_pic = Properties.Resources.LightRed_Back;
                    ink_pic = Properties.Resources.LightRed_Back;
                    boardout_pic = Properties.Resources.LightRed_Back;
                    progress_pic = Properties.Resources.Action_WaitBoard;
                    break;
            }
            this.ioStatus_BoardIn.StatusImage = boardin_pic;
            this.ioStatus_IC.StatusImage = photo_pic;
            this.ioStatus_FPS.StatusImage = ink_pic;
            this.ioStatus_BoardOut.StatusImage = boardout_pic;
            //this.label_PressStart.Image = progress_pic;

        }

        /// <summary>
        /// 获取各个信号的状态，初始化软件界面
        /// </summary>
        private void InitMachine()
        {
            try
            {
                SetBtnImage(this.btn_Light, myfunction.GetLightDOStatus() ? Properties.Resources.Light_ON : Properties.Resources.Light_Off);
                SetBtnImage(this.btn_LockIn2, myfunction.GetLock2DIStatus() ? Properties.Resources.Btn_ON : Properties.Resources.Btn_OFF);
                SetBtnImage(this.btn_LockBig, myfunction.GetLockBigDIStatus() ? Properties.Resources.Btn_ON : Properties.Resources.Btn_OFF);
            }
            catch (Exception ex)
            {
                AddLogStr(ex.Message, true, Color.Red);
            }
        }

        /// <summary>
        /// 检测异常
        /// </summary>
        private void Thd_AlarmMonitor()
        {
            do
            {
                try
                {
                    if (GlobalVar.RunningMode == RunMode.NotMain)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    //未投入使用
                }
                catch (Exception ex)
                {
                    log.AddERRORLOG("异常检测线程异常：" + ex.Message);
                    Thread.Sleep(1000);
                }
                finally
                {
                    Thread.Sleep(300);
                }
            }
            while (!GlobalVar.SoftWareShutDown);
        }

        #region 窗口切换
        private void btn_IC_Click(object sender, EventArgs e)
        {
            ShowFrame(ClickBtn.IC, true);
        }

        private void btn_Axis_Click(object sender, EventArgs e)
        {
            ShowFrame(ClickBtn.Axis, true);
        }

        private void btn_Para_Click(object sender, EventArgs e)
        {
            ShowFrame(ClickBtn.Location, true);
        }

        private void btn_System_Click(object sender, EventArgs e)
        {
            ShowFrame(ClickBtn.System, true);
        }

        /// <summary>
        /// 显示窗体
        /// </summary>
        /// <param name="cb">按钮</param>
        /// <param name="NeedPassword">是否需要密码</param>
        private void ShowFrame(ClickBtn cb, bool NeedPassword = false)
        {
            if (!GlobalVar.Machine.Pause)//机台未暂停，禁止切换
            {
                AddLogStr("机台运行中，禁止切换至其他界面！", true, Color.Red);
                return;
            }

            if (NeedPassword && !Password()) return;
            Frame form = null;
            try
            {
                switch (cb)
                {
                    case ClickBtn.Location:
                        form = new ParaForm();
                        break;
                    case ClickBtn.Axis:
                        form = new AxisForm();
                        break;
                    case ClickBtn.IC:
                        form = new ICForm();
                        break;
                    case ClickBtn.System:
                        form = new SystemForm();
                        break;
                    default:
                        //默认窗口，测试用···
                        form = new Frame();
                        break;
                }

                form.TopLevel = false;
                form.Parent = this;
                form.MdiParent = this.MdiParent;
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ShowFrame Err:" + ex.Message);
            }
            finally
            {
                if (cb == ClickBtn.Location)
                {
                    this.panel_CCD.Controls.Clear();
                    this.panel_CCD.Controls.Add(GlobalVar.CCD);
                }
            }
        }


        /// <summary>
        /// 输入密码
        /// </summary>
        private bool Password()
        {
            try
            {
                if ((DateTime.Now - GlobalVar.LastEnterPassword).TotalMinutes > 3)
                {
                    using (Keyboard form = new Keyboard(true))
                    {
                        form.WindowState = FormWindowState.Maximized;
                        form.TopLevel = false;
                        form.Parent = this;
                        form.MdiParent = this.MdiParent;
                        if (form.ShowDialog() != DialogResult.OK) return false;
                    }
                }
                GlobalVar.LastEnterPassword = DateTime.Now;
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

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

        #region 显示日志部分
        /// <summary>
        /// 添加软件右下方日志
        /// </summary>
        /// <param name="str">信息</param>
        /// <param name="isError">是否为异常信息</param>
        /// <param name="color">右下方显示的字体的颜色</param>
        private void AddLogStr(string str, bool isError = false, Color? color = null)
        {
            try
            {
                if (!isError) log.AddCommLOG(str);
                else log.AddERRORLOG(str);
                str = "\r\n" + DateTime.Now.ToString("HH:mm:ss:fff") + "\t" + str;
                color = color ?? DefaultTextColor;
                Color m_color = (Color)color;
                ShowLog.Enqueue(new TextInfo(str, m_color));
            }
            catch
            {
                Console.WriteLine("添加日志异常");
            }
        }

        /// <summary>
        /// 更新显示动作界面
        /// </summary>
        private void UpdateLog()
        {
            while (!GlobalVar.SoftWareShutDown)
            {
                try
                {
                    if (ShowLog.Count > 0)
                    {
                        StringBuilder logstr = new StringBuilder();
                        Color color = DefaultTextColor;
                        int addcount = ShowLog.Count;
                        for (int i = 0; i < addcount; i++)
                        {
                            TextInfo ti;
                            if (!ShowLog.TryDequeue(out ti)) continue;
                            if (ti.TextColor == DefaultTextColor)
                            {
                                logstr.Append(ti.Text);
                            }
                            else
                            {
                                if (logstr.Length == 0)
                                {
                                    logstr.Append(ti.Text);
                                    color = ti.TextColor;
                                }
                                break;
                            }
                        }
                        if (this.richTextBox_ShowStr.InvokeRequired)
                        {
                            this.richTextBox_ShowStr.Invoke(new Action(() => { ShowStr(logstr.ToString(), color); }));
                        }
                        else ShowStr(logstr.ToString(), color);
                    }
                    Thread.Sleep(10);
                }
                catch { }
            }
        }

        private void ShowStr(string str, Color color)
        {
            const int ClearLength = 280;//删除判断条件
            const int RemoveLength = 40;//删除的前多少行
            if (richTextBox_ShowStr.Lines.Length > ClearLength)
            {
                string[] lines = richTextBox_ShowStr.Lines;
                string[] NewLines;
                int length = 0;
                for (int i = RemoveLength; i >= 0; i--)
                {
                    DateTime time;
                    if (lines[i].Length >= 8 && DateTime.TryParse(lines[i].Substring(0, 8), out time))
                    {
                        length = i;
                        break;
                    }
                }
                NewLines = new string[lines.Length - length];
                Array.Copy(lines, length, NewLines, 0, lines.Length - length);
                this.richTextBox_ShowStr.Lines = NewLines;
            }

            int nSelectStart = richTextBox_ShowStr.TextLength;

            this.richTextBox_ShowStr.AppendText(str);

            int nLength = richTextBox_ShowStr.TextLength - 1;
            richTextBox_ShowStr.Select(nSelectStart, nLength);
            richTextBox_ShowStr.SelectionColor = color;

            this.richTextBox_ShowStr.ScrollToCaret();
            richTextBox_ShowStr.Select(richTextBox_ShowStr.TextLength, 0);
        }
        #endregion


        //protected override void WndProc(ref Message m)
        //{
        //    try
        //    {
        //        switch (m.Msg)
        //        {
        //            case GlobalVar.WM_FixedMotion:
        //                try
        //                {
        //                    if (GlobalVar.AxisPCI.CheckAxisInMoving())
        //                    {
        //                        MsgBox("设备(轴)为Not Ready状态，请停止后检查再作業！", "警告", Color.Orange);
        //                        return;
        //                    }
        //                    string pos_x = Marshal.PtrToStringAnsi(m.WParam);
        //                    string pos_y = Marshal.PtrToStringAnsi(m.LParam);
        //                    double dis_X = (GlobalVar.gl_Ref_Point_CADPos.Pos_X - float.Parse(pos_x)) * -1; //机械原点在左上,X坐标需要取反
        //                    double dis_Y = GlobalVar.gl_Ref_Point_CADPos.Pos_Y - float.Parse(pos_y);
        //                    double x = dis_X;
        //                    double y = dis_Y;
        //                    GlobalVar.AxisPCI.FixPointMotion(x, y);
        //                }
        //                catch (Exception ex)
        //                {
        //                    MsgBox("定点运动异常：" + ex.Message, "异常", Color.Red);
        //                }
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    catch
        //    {
        //    }
        //    base.WndProc(ref m);
        //}
        /// <summary>
        /// 弹框【OK或者Cancel】
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="backcolor">背景色</param>
        /// <returns></returns>
        private bool MsgBox(string message, string title, Color color)
        {
            using (MsgBox box = new MsgBox(MessageBoxButtons.OK))
            {
                box.Title = title;
                box.ShowText = message;
                box.SetBackColor = color;
                if (box.ShowDialog() == DialogResult.OK) return true;
                else return false;
            }
        }

        private void btn_Load_Click(object sender, EventArgs e)
        {
            try
            {
                FileOption SelectForm = new FileOption();
                SelectForm.TopLevel = false;
                SelectForm.Parent = this;
                SelectForm.MdiParent = this.MdiParent;
                if (SelectForm.ShowDialog() != DialogResult.OK) return;

                InitCCD();//重新初始化相机配置
            }
            catch (Exception ex)
            {
                MsgBoxAlarm(ex.Message, false);
            }
        }

        private void btn_Buzzer_Click(object sender, EventArgs e)
        {
            if (GlobalVar.Machine.BuzzerIsSound)//(GetAlarm())
            {
                GlobalVar.Machine.BuzzerAllowSound = false;
                SetBtnImage(this.btn_Buzzer, Properties.Resources.NoSound);
            }
            else
            {
                GlobalVar.Machine.BuzzerAllowSound = true;
                SetBtnImage(this.btn_Buzzer, Properties.Resources.Sound);
            }
            GlobalVar.Machine.BuzzerIsSound = !GlobalVar.Machine.BuzzerIsSound;//蜂鸣器是否叫 取反
        }

        private void btn_LockBefore_Click(object sender, EventArgs e)
        {
            try
            {
                if (myfunction.GetLockBackStatus())//判断大锁的状态
                {
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.Lock, false);
                    SetBtnImage(this.btn_LockBig, Properties.Resources.Btn_OFF);
                }
                else
                {
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.Lock, true);
                    SetBtnImage(this.btn_LockBig, Properties.Resources.Btn_ON);
                }
            }
            catch (Exception ex)
            {
                AddLogStr("锁变更状态失败：" + ex.Message, true, Color.Red);
            }
        }

        private void btn_LockBack_Click(object sender, EventArgs e)
        {
            try
            {
                if (myfunction.GetLockBigDOStatus())//只判断前门大锁的状态
                {
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.Lock, false);
                }
                else
                {
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.Lock, true);
                }
            }
            catch (Exception ex)
            {
                AddLogStr("锁变更状态失败：" + ex.Message, true, Color.Red);

            }
        }

        private void btn_Run_Click(object sender, EventArgs e)
        {
            if (!myfunction.GetLockBigDIStatus())
            {
                AddLogStr("门未锁，禁止运行", true, Color.Red);
                return;
            }
            if (GlobalVar.Machine.Pause)
            {
                GlobalVar.Machine.Pause = false;
                this.btn_Run.Image = Properties.Resources.Btn_Run;
                SetLabelText(label_Status, "作业中···");
                AddLogStr("请按下启动按钮！", false, Color.LightBlue);
                AllowModbusRun(true);//modbus允许作业
                                     //SetLabelPressStart(true);
            }
            else
            {
                GlobalVar.Machine.Pause = true;//置载板到位信号，需要用到此变量为True
                this.btn_Run.Image = Properties.Resources.Btn_Pause;
                SetLabelText(label_Status, "机台停止");
                AddLogStr("机台暂停中···", false, Color.LightBlue);
                this.StartSignal.Reset();
                AllowModbusRun(false);//modbus禁止作业
                GlobalVar.CCD.EndWork();
                //SetLabelPressStart(false);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (GlobalVar.AxisX.ServerOn) GlobalVar.AxisPCI.ServerOn(GlobalVar.AxisX, false);
            if (GlobalVar.AxisY.ServerOn) GlobalVar.AxisPCI.ServerOn(GlobalVar.AxisY, false);
            if (GlobalVar.AxisA.ServerOn) GlobalVar.AxisPCI.ServerOn(GlobalVar.AxisA, false);
            if (GlobalVar.AxisB.ServerOn) GlobalVar.AxisPCI.ServerOn(GlobalVar.AxisB, false);
            AlarmLight(1, true);//亮黄灯
            GlobalVar.AxisPCI.CloseBoard();
        }

        /// <summary>
        /// 报警灯【0：红灯；1：黄灯；2：绿灯】
        /// </summary>
        /// <param name="Index">序号（多个灯同时亮时，序号异或）</param>
        /// <param name="On">开启/关闭</param>
        /// <param name="Buzzer">蜂鸣器叫</param>
        private void AlarmLight(byte Index, bool On, bool Buzzer = false)
        {
            BoardSignalDefinition bsd;
            switch (Index)
            {
                case 0://红灯
                    bsd = GlobalVar.AxisPCI.AlarmLight_Red;
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.AlarmLight_Red, On);
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.AlarmLight_Yellow, false);
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.AlarmLight_Green, false);
                    break;
                case 1://黄灯
                    bsd = GlobalVar.AxisPCI.AlarmLight_Yellow;
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.AlarmLight_Red, false);
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.AlarmLight_Yellow, On);
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.AlarmLight_Green, false);
                    break;
                case 2://绿灯
                    bsd = GlobalVar.AxisPCI.AlarmLight_Green;
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.AlarmLight_Red, false);
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.AlarmLight_Yellow, false);
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.AlarmLight_Green, On);
                    break;
                default:
                    return;
            }
            if (!Buzzer)
            {
                GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.AlarmLight_Buzzer, false);
                GlobalVar.Machine.BuzzerAllowSound = false;
                return;
            }
            if (!GlobalVar.Machine.BuzzerAllowSound && (DateTime.Now - GlobalVar.Machine.LastDisableBuzzer).TotalSeconds < 3) return;//禁音3秒内的报警，不响

            GlobalVar.Machine.BuzzerAllowSound = true;
            GlobalVar.Machine.BuzzerIsSound = true;
            SetBtnImage(this.btn_Buzzer, Properties.Resources.Sound);
            Thread thd_buzzer = new Thread(new ThreadStart(delegate
            {
                try
                {
                    while (GlobalVar.Machine.BuzzerAllowSound && !GlobalVar.SoftWareShutDown)
                    {
                        if (GetBuzzerStatus())
                        {
                            GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.AlarmLight_Buzzer, false);
                            GlobalVar.AxisPCI.SetDO(bsd, false);
                        }
                        else
                        {
                            GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.AlarmLight_Buzzer, true);
                            GlobalVar.AxisPCI.SetDO(bsd, true);
                        }
                        Thread.Sleep(500);
                    }
                }
                catch { }
                finally
                {
                    GlobalVar.AxisPCI.SetDO(bsd, true);
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.AlarmLight_Buzzer, false);
                }
            }));
            thd_buzzer.IsBackground = true;
            thd_buzzer.Name = "蜂鸣器 线程";
            thd_buzzer.Start();
        }

        private void btn_Light_Click(object sender, EventArgs e)
        {
            try
            {
                if (myfunction.GetLightDOStatus())//判断照明灯的状态
                {
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.Light, false);
                    SetBtnImage(this.btn_Light, Properties.Resources.Light_Off);
                }
                else
                {
                    SetBtnImage(this.btn_Light, Properties.Resources.Light_ON);
                    GlobalVar.AxisPCI.SetDO(GlobalVar.AxisPCI.Light, true);
                }
            }
            catch (Exception ex)
            {
                AddLogStr("照明灯变更状态失败：" + ex.Message, true, Color.Red);
            }
        }

        /// <summary>
        /// 获取蜂鸣器的状态
        /// </summary>
        /// <returns></returns>
        private bool GetBuzzerStatus()
        {
            bool buzzer = false;
            if (GlobalVar.AxisPCI.GetSingleDO(GlobalVar.AxisPCI.AlarmLight_Buzzer, ref buzzer))
            {
                return buzzer;
            }
            else throw new Exception("获取蜂鸣器状态DO失败");
        }
        /// <summary>
        /// 设置Btn的可用性
        /// </summary>
        private void SetBtnEnable(Button btn, bool enable)
        {
            if (btn.InvokeRequired)
            {
                btn.BeginInvoke(new Action(delegate { btn.Enabled = enable; }));
            }
            else btn.Enabled = enable;
        }
        /// <summary>
        /// 设置Btn的可用性
        /// </summary>
        private void SetBtnEnable(PictureBox btn, bool enable)
        {
            if (btn.InvokeRequired)
            {
                btn.BeginInvoke(new Action(delegate { btn.Enabled = enable; }));
            }
            else btn.Enabled = enable;
        }
        /// <summary>
        /// 设置按钮的背景图
        /// </summary>
        private void SetBtnImage(Button btn, Bitmap bitmap)
        {
            if (btn.InvokeRequired)
            {
                btn.BeginInvoke(new Action(delegate { btn.Image = bitmap; }));
            }
            else btn.Image = bitmap;
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

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case GlobalVar.WM_PCSConnection:
                    AddLogStr("PCS检查机连接成功，允许作业");
                    PCSConnected = true;
                    break;
                case GlobalVar.WM_PCSAcceptData:
                    AddLogStr("发送IC条码数据到PCS检查机成功");
                    dataSendComplete = true;
                    break;
                case GlobalVar.WM_PCSCheckEMG:
                    AddAlarm("PCS检查机异常，请处理！");//PCS检查机异常，无需复位
                    GlobalVar.IsLightSensorWorking = false;
                    break;
                case GlobalVar.WM_PCSCheckEMG_Reset:
                    GlobalVar.IsLightSensorWorking = true;
                    AddLogStr("PCS检查机异常已解除");
                    AlarmLight(3, true);
                    break;
                case GlobalVar.WM_PCSAllowDropBoard:
                    if (DropedBoard && (!GlobalVar.Machine.Pause))//只有到达下料位置后接收到的下料信号作用
                    {
                        PCSComplete = true;
                        PCSFoebideDrop = false;
                        DropSignal = true;
                    }
                    AddLogStr("PCS检查机检查完成，下料机下料");
                    DropBoardCount++;
                    break;
                case GlobalVar.WM_PCSStartCheck:
                    AddLogStr("PCS检查机开始拍照");
                    if (photocomplete && (!GlobalVar.Machine.Pause))
                        PCSPhoto = true;

                    break;
                case GlobalVar.WM_PCSForbideDrop:
                    AddLogStr("PCS检查异常，禁止下料");
                    if (DropedBoard && (!GlobalVar.Machine.Pause))
                    {
                        PCSComplete = true;
                        PCSFoebideDrop = true;
                    }
                    break;
                case GlobalVar.WM_PCSForbideWork:
                    AddLogStr("PCS轴禁止作业！");
                    PCSConnected = false;
                    SetLabelText(label_Status, "禁止作业");
                    //下料机下料
                    //if (!(GlobalVar.Drop_Modbus.SendMsg(GlobalVar.Drop_Modbus.Coils.AxisMoveDown, true)))
                    //    GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.AxisMoveDown, true);//下料完成，信号置为0//通知下料机下料完成
                    break;
                case GlobalVar.WM_PCSNG:
                    AddLogStr("PCS检查异常，不搬运");
                    if (DropedBoard && (!GlobalVar.Machine.Pause))
                    {
                        PCSResult = true;
                        PCSNG = true;
                    }
                    break;
                case GlobalVar.WM_PCSResult:
                    AddLogStr("PCS检查PASS,搬运");
                    if (DropedBoard && (!GlobalVar.Machine.Pause))
                    {
                        PCSResult = true;
                        PCSNG = false;
                        PCSComplete = true;
                        PCSFoebideDrop = false;
                        DropSignal = true;
                    }
                    break;
                case GlobalVar.WM_PCSArrive:
                    AddLogStr("【IC屏蔽】托盘到拍照位信号发送成功");
                    break;
            }
            base.WndProc(ref m);
        }

        private void btn_LastBoard_Click(object sender, EventArgs e)
        {
            AddLogStr("开始末次下料");
            LastBoard = true;
            IsFirstMove = false;
            btn_LastBoard.Enabled = false;
        }

        private void btn_DropBoard_Click(object sender, EventArgs e)
        {
            if (!(GlobalVar.Drop_Modbus.SendMsg(GlobalVar.Drop_Modbus.Coils.AxisMoveDown, true)))
                GlobalVar.Drop_Modbus.AddMsgList(GlobalVar.Drop_Modbus.Coils.AxisMoveDown, true);//下料完成，信号置为0//通知下料机下料完成
        }
    }
}
