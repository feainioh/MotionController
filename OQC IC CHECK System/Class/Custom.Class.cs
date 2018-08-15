using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections;

namespace OQC_IC_CHECK_System
{
    /// <summary>
    /// 整台机台状态
    /// </summary>
    internal class EntireMachine
    {
        public delegate void dele_AlarmUpdate();//报警信息更新
        public delegate void dele_Pause(bool IsPause);//机台暂停
        /// <summary>
        /// 报警信息更新
        /// </summary>
        public event dele_AlarmUpdate Event_AlarmUpdate;
        /// <summary>
        /// 机台暂停
        /// </summary>
        public event dele_Pause Event_Pause;


        private bool m_Pause = true;
        /// <summary>
        /// 机台是否是暂停状态
        /// </summary>
        internal bool Pause
        {
            set
            {
                if (m_Pause != value)
                {
                    if (this.Event_Pause != null) this.Event_Pause(value);
                }
                m_Pause = value;
            }
            get { return m_Pause; }
        }

        private bool m_Reset = false;
        /// <summary>
        /// 机台是否复位
        /// </summary>
        internal bool Reset
        {
            get { return m_Reset; }
            set
            {
                CylinderAlarm = false;//解除气缸当前报警
                m_Reset = value;
            }
        }
        /// <summary>
        /// 蜂鸣器是否在响
        /// </summary>
        internal bool BuzzerIsSound = false;
        /// <summary>
        /// 报警频次记录【不要记录软件的Exception】
        /// </summary>
        internal Dictionary<string, int> AlarmRecord = new Dictionary<string, int>();
        /// <summary>
        /// 气缸是否报警
        /// </summary>
        internal bool CylinderAlarm = false;

        private DateTime m_LastDisableBuzzer;
        /// <summary>
        /// 上次蜂鸣器静音时间【静音内的报警不响】
        /// </summary>
        internal DateTime LastDisableBuzzer { get { return m_LastDisableBuzzer; } }

        private bool m_BuzzerAllowSound = true;
        /// <summary>
        /// 蜂鸣器是否允许响
        /// </summary>
        internal bool BuzzerAllowSound
        {
            get
            {
                return m_BuzzerAllowSound;
            }
            set
            {
                m_BuzzerAllowSound = value;
                if (!m_BuzzerAllowSound) m_LastDisableBuzzer = DateTime.Now;
            }
        }

        /// <summary>
        /// 强制更新报警信息
        /// </summary>
        internal void ForceAlarmUpdate()
        {
            if (this.Event_AlarmUpdate != null) this.Event_AlarmUpdate();
        }
    }
    /// <summary>
    /// 轴信息
    /// </summary>
    internal class AxisProperty
    {
        /// <summary>
        /// 关联板卡的序号
        /// </summary>
        internal int LinkIndex = -1;
        /// <summary>
        /// 伺服是否开启
        /// </summary>
        internal bool ServerOn = false;
    }

    /// <summary>
    /// 板卡信号定义
    /// </summary>
    class BoardSignalDefinition
    {
        /// <summary>
        /// 轴编号
        /// </summary>
        public readonly int AxisNum = -1;
        /// <summary>
        /// 针脚定义
        /// </summary>
        public readonly ushort Channel = ushort.MaxValue;
        /// <summary>
        /// 是否为输出信号【允许修改】
        /// </summary>
        public readonly bool IsDO = false;
        public BoardSignalDefinition(int axisNum, ushort channel)
        {
            this.AxisNum = axisNum;
            this.Channel = channel;
            if (this.Channel >= 4) this.IsDO = true;
        }
    }

    /// <summary>
    /// 信号变更(触发事件)
    /// </summary>
    class SignalChangeMonitor
    {
        public delegate void dele_Trigger(bool status);
        public event dele_Trigger Event_Trigger;

        private readonly bool isChangeTrigger = false;//是否变化时触发
        private readonly int Trigger = 0;//触发延时
        private Stopwatch moniter = new Stopwatch();//计时
        protected bool m_CurrentValue = false;
        /// <summary>
        /// 当前值
        /// </summary>
        internal bool CurrentValue
        {
            set
            {
                if (isChangeTrigger//立即触发信号
                    && value != m_CurrentValue)//信号变化
                {
                    m_CurrentValue = value;
                    EventTrigger(m_CurrentValue);//IO触发
                    return;
                }

                if (value != m_CurrentValue)
                {
                    if (moniter.IsRunning) moniter.Reset();
                    else moniter.Restart();
                }
                m_CurrentValue = value;
                if (moniter.Elapsed.TotalMilliseconds > Trigger)
                {
                    moniter.Stop();
                    moniter.Restart();
                    EventTrigger(m_CurrentValue);//输入输出触发
                }
            }
            get { return m_CurrentValue; }
        }
        /// <summary>
        /// 无参数，代表信号变化时，立刻触发
        /// </summary>
        internal SignalChangeMonitor()
        {
            this.isChangeTrigger = true;
        }

        /// <summary>
        /// 输入输出信号
        /// </summary>
        /// <param name="trigger">持续多久触发</param>
        internal SignalChangeMonitor(int trigger)
        {
            this.Trigger = trigger;
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="status"></param>
        protected void EventTrigger(bool status)
        {
            if (this.Event_Trigger != null) this.Event_Trigger(status);
        }
    }

    /// <summary>
    /// 气缸信号监控
    /// </summary>
    class CylinderSignalMonitor : SignalChangeMonitor
    {
        internal CylinderSignalMonitor(int tigger)
        {
            timeout = tigger;
        }
        /// <summary>
        /// 当前值
        /// </summary>
        internal new bool CurrentValue
        {
            set
            {
                if (value != m_CurrentValue)//信号变化
                {
                    EventTrigger(value);//IO触发
                    MonitorInputThd(value);//监控输入信号
                }
                m_CurrentValue = value;
            }
            get { return m_CurrentValue; }
        }

        private BoardSignalDefinition UP_signal;//高电平
        private BoardSignalDefinition DOWN_signal;//低电平
                                                  /// <summary>
                                                  /// 是否需要监控输入信号
                                                  /// </summary>
        internal bool NeedMonitorInput = false;
        /// <summary>
        /// 输出关联输入信号，需要收到反馈
        /// </summary>
        /// <param name="up">信号为高时，需要判断的输入信号</param>
        /// <param name="down">信号为低时，需要判断的输入信号</param>
        internal void LinkInputSignal(BoardSignalDefinition up, BoardSignalDefinition down)
        {
            this.UP_signal = up;
            this.DOWN_signal = down;
            this.NeedMonitorInput = true;
        }
        public delegate void delet_MonitorInputOutTime(bool Up);//监控输入信号超时事件
        public event delet_MonitorInputOutTime Event_MonitorTimeout;

        int timeout = 2000;//超时1000毫秒
                           /// <summary>
                           /// 监控输入信号
                           /// </summary>
                           /// <param name="_up">是否为高电平</param>
        internal void MonitorInputThd(bool _up)
        {
            if ((_up && this.UP_signal == null) || (!_up && this.DOWN_signal == null)) return;
            Thread thd = new Thread(new ThreadStart(delegate
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                bool result = false;
                do
                {
                    if (_up)
                    {
                        if (GlobalVar.AxisPCI.GetSingleDI(this.UP_signal, ref result))
                        {
                            if (result) break;//到位则停止循环
                        }
                    }
                    else
                    {
                        if (GlobalVar.AxisPCI.GetSingleDI(this.DOWN_signal, ref result))
                        {
                            if (result) break;//到位则停止循环
                        }
                    }
                } while (sw.Elapsed.TotalMilliseconds < timeout);
                if (!result && this.Event_MonitorTimeout != null) this.Event_MonitorTimeout(_up);
            }));
            thd.IsBackground = true;
            thd.Name = "监控输入信号 线程";
            thd.Start();
        }

    }


    /// <summary>
    /// 真空信号监控
    /// </summary>
    class SuckerSignalMonitor : SignalChangeMonitor
    {
        Logs log = Logs.LogsT();
        /// <summary>
        /// 当前值
        /// </summary>
        internal new bool CurrentValue
        {
            set
            {
                if (value != m_CurrentValue)
                {
                    EventTrigger(value);//IO触发
                    MonitorInputThd(value);//监控输入信号
                }
                m_CurrentValue = value;
            }
            get { return m_CurrentValue; }
        }

        private BoardSignalDefinition UpLimit;//气缸上限
        private BoardSignalDefinition Blow_signal;//吹气
        /// <summary>
        /// 输出关联输入信号，需要收到反馈
        /// </summary>
        /// <param name="blow">信号为低时，需要判断的输入信号</param>
        internal void LinkInputSignal(BoardSignalDefinition upLimit,BoardSignalDefinition blow)
        {
            this.UpLimit = upLimit;
            this.Blow_signal = blow;
        }
        public delegate void delet_MonitorInputOutTime(bool Up);//监控输入信号超时事件
        public event delet_MonitorInputOutTime Event_MonitorTimeout;

        int timeout = 2000;//超时1000毫秒
        /// <summary>
        /// 监控输入信号
        /// </summary>
        /// <param name="_sucker">是否为高电平</param>
        internal void MonitorInputThd(bool _sucker)
        {
            if ((!_sucker && this.Blow_signal == null)) return;
            Thread thd = new Thread(new ThreadStart(delegate
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                bool result_sucker = false;
                bool result_blow = true;
                bool result_limit = false;
               // log.AddERRORLOG(this.Blow_signal.AxisNum.ToString() + this.Blow_signal.Channel.ToString() + "进入真空状态检测线程");
                do
                {
                    if (_sucker)
                    {
                        if (GlobalVar.AxisPCI.GetSingleDI(this.Blow_signal, ref result_sucker))
                        {
                            if (result_sucker)
                            {
                                if (GlobalVar.AxisPCI.GetSingleDI(this.UpLimit, ref result_limit))
                                {
                                    if (result_limit)//到达上限
                                        break;
                                }
                                //log.AddERRORLOG(this.Blow_signal.AxisNum.ToString() + this.Blow_signal.Channel.ToString() + "达到真空状态");
                            }
                        }
                    }
                    else
                    {
                        if (GlobalVar.AxisPCI.GetSingleDI(this.Blow_signal, ref result_blow))
                        {
                            if (!result_blow)
                            {
                                if (GlobalVar.AxisPCI.GetSingleDI(this.UpLimit, ref result_limit))
                                {
                                    //log.AddERRORLOG(this.Blow_signal.AxisNum.ToString() + this.Blow_signal.Channel.ToString() + "破真空状态");
                                    if(result_limit)
                                    break;//破真空则停止循环
                                }
                            }
                        }
                    }
                } while (sw.Elapsed.TotalMilliseconds < timeout);
                if (sw.Elapsed.TotalMilliseconds < timeout)
                {
                    Thread.Sleep(100);
                    GlobalVar.AxisPCI.GetSingleDI(this.Blow_signal, ref result_sucker);
                    //log.AddERRORLOG(this.Blow_signal.AxisNum.ToString() + this.Blow_signal.Channel.ToString() + "重复检测");
                }
              //  log.AddERRORLOG(this.Blow_signal.AxisNum.ToString() + this.Blow_signal.Channel.ToString() + "检测循环结束,结果:"+result_sucker.ToString()+"吸取信号:"+_sucker.ToString());
                if (!result_sucker&& _sucker && this.Event_MonitorTimeout != null)
                {
                 //   log.AddERRORLOG(this.Blow_signal.AxisNum.ToString() + this.Blow_signal.Channel.ToString() + "触发超时事件");
                    this.Event_MonitorTimeout(_sucker);
                }
                if (!_sucker && result_blow && this.Event_MonitorTimeout != null) this.Event_MonitorTimeout(_sucker);
            }));
            thd.IsBackground = true;
            thd.Name = "监控输入信号 线程";
            thd.Start();
        }

    }
    class TextInfo
    {
        /// <summary>
        /// 字符串
        /// </summary>
        public string Text;
        /// <summary>
        /// 字符串颜色
        /// </summary>
        public System.Drawing.Color TextColor;

        public TextInfo(string text, System.Drawing.Color color)
        {
            this.Text = text;
            this.TextColor = color;
        }
    }



    #region CAD点阵信息相关
    public class SPoint : Object
    {
        public int FlowID;  //从DWG中获取
        public string Point_name;
        public int PointNumber; //值保持為最新的順序號
        public double Angle_deflection = 0;   //旋轉角度，保持即時更新
        public int Line_sequence = 0;   //条码队列顺序 -- 0：正向 1：反向
        public double Pos_X = 0.0F;
        public double Pos_Y = 0.0F;
        public double Pos_Z = 0.0F;

        public void CopyPoint(SPoint sp)
        {
            this.Pos_X = sp.Pos_X;
            this.Pos_Y = sp.Pos_Y;
            this.Pos_Z = sp.Pos_Z;
        }
    }

    public class ZhiPinInfo
    {
        public string _SubName;   //ST,KNOWLES...
        public string _HeadStr;      //FFC, FF9....
        public int _BarcodeLength;
        public int _StartPos;
        public int _StartLen;  //
    }

    //一个OnePointGroup == 一个FlowID的cycle，DOCK有两个MIC就有两个CYCLE
    public class OnePointGroup
    {
        public int FlowID;   //FLOWID
        public List<SPoint> m_ListGroup = new List<SPoint>();
        public List<ZhiPinInfo> m_list_zhipingInfo = new List<ZhiPinInfo>();  //当前组用的制品，用于切换光源
        public OneGroup_Blocks m_BlockList_ByGroup = new OneGroup_Blocks();  //当前组的BLOCK集合        
    }

    //CAD点阵集合信息
    public class PointInfo
    {
        public List<OnePointGroup> m_List_PointInfo = new List<OnePointGroup>();
        public void addPoint(SPoint sp)
        {
            for (int i = 0; i < m_List_PointInfo.Count; i++)
            {
                if (m_List_PointInfo[i].FlowID == sp.FlowID)
                {
                    m_List_PointInfo[i].m_ListGroup.Add(sp); //一样Flowid的点放在一组,一个Flowid就只有一个m_List_PointInfo
                    return;
                }
            }
            //如果到这里没有被return，说明m_List_PointInfo中尚没有这个FLOWID的集合
            OnePointGroup newgroup = new OnePointGroup();
            newgroup.FlowID = sp.FlowID;
            newgroup.m_ListGroup.Add(sp);
            m_List_PointInfo.Add(newgroup);
        }

        public void clearList()
        {
            m_List_PointInfo.Clear();
        }

        public void Sort()
        {
            for (int i = 0; i < m_List_PointInfo.Count; i++)
            {
                m_List_PointInfo[i].m_ListGroup.Sort(new SPointCompare_byTipSequence());
            }
        }
    }
    public class SPointCompare_byTipSequence : IComparer<SPoint>
    {
        public int Compare(SPoint point1, SPoint point2)
        {
            return ((new CaseInsensitiveComparer()).Compare(point1.PointNumber, point2.PointNumber));
        }
    }
    public class OneGroup_Blocks
    {
        public int FlowID;
        public List<ClientBlock> m_BlockinfoList = new List<ClientBlock>();
        public bool m_DecodeFinished = false;
        public void add(ClientBlock block)
        {
            m_BlockinfoList.Add(block);
        }

    }
    #endregion


    /// <summary>
    /// 范围
    /// </summary>
    public class Range_Double
    {
        private double m_intMax, m_intMin;

        /// <summary>
        /// 获取最大值
        /// </summary>
        public double MAX { get { return m_intMax; } }
        /// <summary>
        /// 获取最小值
        /// </summary>
        public double MIN { get { return m_intMin; } }

        /// <summary>
        /// int的范围
        /// </summary>
        /// <param name="Max">最大值</param>
        /// <param name="Min">最小值</param>
        public Range_Double(double Max, double Min)
        {
            this.m_intMax = Max;
            this.m_intMin = Min;
        }
    }


}
