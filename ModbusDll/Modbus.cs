using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO.Ports;
using System.IO;
using System.Threading;


namespace ModbusDll
{
    public class Modbus
    {
        public enum emMsgFormat : int
        {
            Decimal = 10,
            Hex = 16,
        }
        public enum emFunction : byte
        {
            ReadCoil = 1,
            ReadDiscreteInputs = 2,
            ReadHoldReg = 3,
            ReadInputReg = 4,
            WriteSingleCoil = 5,
            WriteSingleHoldReg = 6,
            WriteMultiCoils = 15,
            WriteMultiHoldReg = 16,
        }
        public enum emMsgType:int
        {
            Send = 0,
            Recv,
            Info,
            Error,
        }
        public enum emCommMode : int
        {
            NoConnection = 0,
            TCP = 1,
            RTU = 2,
            ASCII = 3,
            
        }
        #region 事件定义
        //接收到数据的事件
        public delegate void delegate_DataReceive(OutputModule output);
        public event delegate_DataReceive event_DataReceive;
        //发送或接到通信消息时的事件，用于通信记录的显示
        /// <summary>
        ///  参数：nMsgType 0，发送的消息；1，接收的消息；3，通信报错；
        /// </summary>
        public delegate void dele_MessageText(string str, emMsgType nMsgType);
        public event dele_MessageText event_MessageText;
        #endregion
        #region 常量定义
        #region 功能码定义
        public const byte byREAD_COIL = 1;
        public const byte byREAD_DISCRETE_INPUTS = 2;
        public const byte byREAD_HOLDING_REG = 3;
        public const byte byREAD_INPUT_REG = 4;
        public const byte byWRITE_SINGLE_COIL = 5;
        public const byte byWRITE_SINGLE_HOLDING_REG = 6;
        public const byte byWRITE_MULTI_COILS = 15;
        public const byte byWRITE_MULTI_HOLDING_REG = 16;
        #endregion
        #endregion
        #region 属性
        public bool m_bConnected
        {
            get
            {
                switch (m_nRunMode)
                {
                    case emCommMode.RTU:
                        if (m_SerialPort != null)
                        {
                            return m_SerialPort.IsOpen;
                        }
                        else return false;
                    case emCommMode.TCP:
                        if (m_socketTcp != null)
                        {
                            return m_socketTcp.Connected;
                        }
                        else return false;
                    default:
                        return false;
                }
            }
        }
        public emCommMode CommMode
        {
            get
            {
                return m_nRunMode;
            }
        }
        public emMsgFormat MsgForm
        {
            set { m_nMsgFormat = value;}
            get { return m_nMsgFormat; }
        }
        #endregion
        #region 成员变量定义
        #region public
        public int m_nReSendTimes = 1;//TCP消息重发次数
        public int m_nReConnectTimes = 1;//TCP重连次数
        public int m_nSendTimeOut = 1000;
        public int m_nRecvTimeOut = 1000;
        public int m_nMsgIntervalTm = 200;
        #endregion
        Thread m_thd_SendMsg = null;
        AutoResetEvent m_Event_SendMsg = new AutoResetEvent(false);
        private Queue<InputModule> m_InputQueue = new Queue<InputModule> { };
        private emMsgFormat m_nMsgFormat = emMsgFormat.Hex;
        OutputModule m_output = new OutputModule();
        private byte[] m_byTCPDataRecv = new byte[14];//null;
        private byte[] m_byRtuDataRecv = new byte[14];
        private IPAddress m_IP;
        private int m_nPort = 502;
        private Socket m_socketTcp;
        private string ConnectInfo;//连接信息
        AutoResetEvent client_ConnectDone = new AutoResetEvent(false);     //连接等待
        public bool isTcpConnected = false;//判断TCP是否连接
        private SerialPort m_SerialPort = null;
        private emCommMode m_nRunMode = (int)emCommMode.NoConnection;
        private int m_nTCPCount = 0;//用于TCP消息计数
        Thread thd_TcpConnect = null;
        private bool m_bRTUSync = false;//用于同步收发消息
        #endregion
        #region TCP
        #region Construction
        public Modbus(IPAddress _ip, int port)
        {
            m_nRunMode = emCommMode.TCP;
            m_IP = _ip;
            m_nPort = port;
            if (!TCPconnect(_ip, m_nPort))
            {
                throw new Exception("网络连接失败！");
            }
            m_thd_SendMsg = new Thread(new ThreadStart(ThdProcSendMessage));
        }
        #endregion
        /// <summary>
        ///   TCP连接
        /// </summary>
        private bool TCPconnect(IPAddress ip, int nPort = 502)
        {
            AutoResetEvent eventConn = new AutoResetEvent(false);
            thd_TcpConnect = new Thread(new ThreadStart(
                delegate
                {
                    int nCount = 1;
                    do
                    {
                        try
                        {
                            if (event_MessageText != null)
                            {
                                event_MessageText(string.Format("第{0}次连接...", nCount), emMsgType.Info);
                            }
//                            IPAddress _ip;
//                             if (IPAddress.TryParse(strIP, out _ip) == false)
//                             {
//                                 IPHostEntry hst = Dns.GetHostEntry(strIP);
//                                 strIP = hst.AddressList[0].ToString();
//                             }
                            m_socketTcp = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                            m_socketTcp.BeginConnect(new IPEndPoint(ip, nPort),new AsyncCallback(ConnectCallback),m_socketTcp);
                            if (client_ConnectDone.WaitOne(1000, false))
                            {
                                if (!isTcpConnected) throw new Exception(ConnectInfo);
                            }
                            else
                            {
                                m_socketTcp.Close();
                                throw new TimeoutException("TimeOut Exception");
                            }

                            m_socketTcp.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, m_nSendTimeOut);
                            m_socketTcp.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, m_nRecvTimeOut);
                            m_socketTcp.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, 1);
                            if (event_MessageText != null)
                            {
                                event_MessageText("连接成功！", emMsgType.Info);
                            }
                            eventConn.Set();
                            return;
                        }
                        catch (Exception)
                        {
                            if (m_socketTcp != null)
                            {
                                m_socketTcp.Dispose();
                                m_socketTcp = null;
                            }
                            continue;
                        }
                    }
                    while (++nCount <= m_nReConnectTimes);
                    if (event_MessageText != null)
                    {
                        event_MessageText("连接失败！", emMsgType.Error);
                    }
                    eventConn.Set();
                    return;
                }));
            thd_TcpConnect.IsBackground = true;
            thd_TcpConnect.Start();
            eventConn.WaitOne();
            return m_bConnected;
        }
        
        /// <summary>
        /// socket连接返回函数
        /// </summary>
        /// <param name="ar">表示异步操作的状态</param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // 获取socket连接实例
                System.Net.Sockets.Socket client = (System.Net.Sockets.Socket)ar.AsyncState;
                // 完成连接过程.
                client.EndConnect(ar);
                // 得到连接成功信息
                ConnectInfo = "连接成功！";
                isTcpConnected = true;
                // 置位连接完成标志
                client_ConnectDone.Set();
            }
            catch (Exception e)
            {
                // 得到连接失败信息
                ConnectInfo = e.ToString();
                // 结束连接
                client_ConnectDone.Reset();
            }
        }

        /// <summary>
        ///  TCP下读取数据组帧 
        /// </summary>
        private byte[] CreateReadMsg_TCP(InputModule cmdMsg)
        {
            byte[] byMsg = new byte[12];
            if (m_nTCPCount++ == 65535)
            {
                m_nTCPCount = 0;
            }
            //事务标识符
            byte[] byCount = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)m_nTCPCount));
            byMsg[0] = byCount[0];
            byMsg[1] = byCount[1];
            //协议标识符
            byMsg[2] = 0;
            byMsg[3] = 0;
            //长度
            byte[] byLength = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)6));
            byMsg[4] = byLength[0];
            byMsg[5] = byLength[1];
            //SlaveID
            byMsg[6] = cmdMsg.bySlaveID;
            //功能码 1 
            byMsg[7] = cmdMsg.byFunction;
            //数据 N
            byte[] byAddr = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)cmdMsg.nStartAddr));
            byMsg[8] = byAddr[0];
            byMsg[9] = byAddr[1];
            byte[] byDataLength = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)cmdMsg.nDataLength));
            byMsg[10] = byDataLength[0];
            byMsg[11] = byDataLength[1];
            //定义接收buffer大小
            SetRecvBufSize(ref m_byTCPDataRecv, cmdMsg);
            return byMsg;
        }
        /// <summary>
        ///  TCP下写入数据组帧 
        /// </summary>
        private byte[] CreateWriteMsg_TCP(InputModule cmdMsg)
        {
            byte[] byMsg = null;
            int nWriteDataIndex = 0;
            if (cmdMsg.byFunction >= byWRITE_MULTI_COILS)
            {
                byMsg = new byte[10 + cmdMsg.byWriteData.Length + 3];
            }
            else
            {
                byMsg = new byte[10 + cmdMsg.byWriteData.Length];
            }
            if (m_nTCPCount++ == 65535)
            {
                m_nTCPCount = 0;
            }
            //事务标识符
            byte[] byCount = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)m_nTCPCount));
            byMsg[0] = byCount[0];
            byMsg[1] = byCount[1];
            //协议标识符
            byMsg[2] = 0;
            byMsg[3] = 0;
            //长度
            byte[] byLength = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)(byMsg.Length - 6)));
            byMsg[4] = byLength[0];
            byMsg[5] = byLength[1];

            byMsg[6] = cmdMsg.bySlaveID;
            byMsg[7] = cmdMsg.byFunction;
            byte[] byAddr = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)cmdMsg.nStartAddr));
            byMsg[8] = byAddr[0];
            byMsg[9] = byAddr[1];
            nWriteDataIndex = 9 + 1;
            if (cmdMsg.byFunction >= byWRITE_MULTI_COILS)
            {
                byte[] _cnt = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)cmdMsg.nDataLength));
                byMsg[10] = _cnt[0];			// Number of bytes
                byMsg[11] = _cnt[1];			// Number of bytes
                byMsg[12] = Convert.ToByte(cmdMsg.byWriteData.Length);
                nWriteDataIndex = 12 + 1;
            }
            Array.Copy(cmdMsg.byWriteData, 0, byMsg, nWriteDataIndex, cmdMsg.byWriteData.Length);
            //定义接收buffer大小
            SetRecvBufSize(ref m_byTCPDataRecv, cmdMsg);
            return byMsg;
        }
        #region TCP异步
        //private string SendMessage_TCP_Async(byte[] bySend)
        //{
        //    try
        //    {
        //        SocketError socketErr;
        //        m_socketTcp.BeginSend(bySend, 0, bySend.Length, SocketFlags.None, out socketErr, new AsyncCallback(OnSend), null);
        //        if (event_MessageText != null)
        //        {
        //            //string str = BitConverter.ToString(bySend);
        //            event_MessageText(BytesToStr(bySend), emMsgType.Send);
        //        }
        //        m_socketTcp.BeginReceive(m_byTCPDataRecv, 0, m_byTCPDataRecv.Length, SocketFlags.None, new AsyncCallback(OnReceive), m_socketTcp);
        //    }
        //    catch (Exception ex)
        //    {       
        //        return ex.Message;
        //    }
        //    return null;
        //}
        //private void OnSend(IAsyncResult result) 
        //{
        //    if (result.IsCompleted == false)
        //    {
        //        if (event_MessageText != null)
        //        {
        //            event_MessageText("发送失败!", emMsgType.Error);
        //        }
        //    }
        //}
        //private void OnReceive(IAsyncResult result)
        //{
        //    if (result.IsCompleted == false)
        //    {
        //        if (event_MessageText != null)
        //        {
        //            event_MessageText("接收失败!", emMsgType.Error);
        //        }
        //    }
        //    else
        //    {
        //        m_output.byRecvData = m_byTCPDataRecv;
        //        if (event_DataReceive != null)
        //        {
        //            event_DataReceive(m_output);
        //        }
        //        if (event_MessageText != null)
        //        {
        //            //string str = BitConverter.ToString(m_byTCPDataRecv);
        //            event_MessageText(BytesToStr(m_byTCPDataRecv), emMsgType.Recv);
        //        }
        //        m_Event_SendMsg.Set();
        //    }
        //}
        #endregion
        /// <summary>
        ///   TCP 同步发送数据
        /// </summary>
        private OutputModule SendMessage_TCP_Sync(byte[] bySend, bool bIsSync = false)
        {
            OutputModule output = null;
            if (m_socketTcp == null)
            {
                if (event_MessageText != null)
                {
                    event_MessageText("检测到套接字处于关闭状态,正在尝试重连...", emMsgType.Error);
                }
                if (!TCPconnect(m_IP, m_nPort))
                {
                    if (event_MessageText != null)
                    {
                        event_MessageText("重连失败！无法发送消息！", emMsgType.Error);
                    }
                    return output;
                }
            }
            int nCount = 1;
            //string strRet = null;
            do 
            {
                try
                {
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + "\t发送:" + BitConverter.ToString(bySend));
                    m_socketTcp.Send(bySend);
                    if (event_MessageText != null)
                    {
                        event_MessageText(BytesToStr(bySend), emMsgType.Send);
                    }
                    m_socketTcp.Receive(m_byTCPDataRecv);
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + "\t接收:" + BitConverter.ToString(m_byTCPDataRecv) + "\r\n");
                    if (bySend[7] != m_byTCPDataRecv[7])
                    {
                        Console.WriteLine("异常时间点："+DateTime.Now.ToString("HH:mm:ss:fff"));
                        throw new FunctionErr(string.Format("功能码不一致 发送功能码:{0} 接收功能码:{1}", bySend[7], m_byTCPDataRecv[7]));
                    }
                    m_output.byFunction = m_byTCPDataRecv[7];
                    if ((m_output.byFunction == (byte)emFunction.ReadCoil)
                        || (m_output.byFunction == (byte)emFunction.ReadDiscreteInputs)
                        || (m_output.byFunction == (byte)emFunction.ReadHoldReg)
                        || (m_output.byFunction == (byte)emFunction.ReadInputReg)
                        )
                    {
                        m_output.nDataLength = (int)m_byTCPDataRecv[8];
                    }
                    m_output.byRecvData = m_byTCPDataRecv;
                    if ((event_DataReceive != null) && (!bIsSync))
                    {
                        event_DataReceive(m_output);
                    }
                    if (event_MessageText != null)
                    {
                        event_MessageText(BytesToStr(m_byTCPDataRecv), emMsgType.Recv);
                    }
                    output = m_output;
                    break;
                }
                #region ObjectDisposedException
                catch (ObjectDisposedException ex)//socket 关闭
                {
                    if (event_MessageText != null)
                    {
                        event_MessageText(ex.Message, emMsgType.Error);
                    }
                    if (m_socketTcp != null)
                    {
                        m_socketTcp.Dispose();
                        m_socketTcp = null;
                    }
                    if (TCPconnect(m_IP, m_nPort))
                    {
                        continue;
                    }
                    else
                    {
                        if (event_MessageText != null)
                        {
                            event_MessageText("消息发送失败，已断开连接！", emMsgType.Error);
                        }
                        break;
                    }
                }
                #endregion
                #region ArgumentNullException
                catch (ArgumentNullException ex)//buffer为空
                {
                    if (event_MessageText != null)
                    {
                        event_MessageText(ex.Message, emMsgType.Error);
                    }
                    break;
                }
                #endregion
                #region FunctionException
                catch (FunctionErr fe)
                {
                    if (event_MessageText != null)
                    {
                        event_MessageText(fe.Message, emMsgType.Error);
                    }
                }
                #endregion
                #region SocketException
                catch (SocketException ex)//访问socket出错
                {
                    if(event_MessageText != null)
                    {
                        event_MessageText(ex.Message, emMsgType.Error);
                    }
                    if (m_socketTcp != null)
                    {
                        m_socketTcp.Dispose();
                        m_socketTcp = null;
                    }
                    if (isTcpConnected && TCPconnect(m_IP, m_nPort))
                    {
                        continue;
                    }
                    else
                    {
                        if (event_MessageText != null)
                        {
                            event_MessageText("消息发送失败！已断开连接！", emMsgType.Error);
                        }
                        break;
                    }
                }
                #endregion
                #region ArgumentOutOfRangeException
                catch (ArgumentOutOfRangeException ex)//越界
                {
                    if (event_MessageText != null)
                    {
                        event_MessageText(ex.Message, emMsgType.Error);
                    }
                    break;
                }
                #endregion
            } 
            while (++nCount <= m_nReSendTimes);
            return output;
        }
        #endregion
        #region SerialPort 
        #region Construction
        public Modbus(string strPortName, int nBaudRate, int nDatabits, Parity parity, StopBits stopBits)
        {
            if (m_SerialPort != null)
            {
                m_SerialPort.Dispose();
                m_SerialPort = null;
            }
            m_SerialPort = new SerialPort(strPortName, nBaudRate, parity, nDatabits, stopBits);
            m_nRunMode = emCommMode.RTU;
            try
            {
                if (!SPconnect())
                {
                    throw new Exception("串口打开失败！");
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            m_thd_SendMsg = new Thread(new ThreadStart(ThdProcSendMessage));
        }
        #endregion
        private bool SPconnect()
        {
            m_SerialPort.Open();
            if (m_SerialPort.IsOpen)
            {
                m_SerialPort.ReadTimeout = m_nRecvTimeOut;
                m_SerialPort.WriteTimeout = m_nSendTimeOut;
                m_SerialPort.DataReceived += new SerialDataReceivedEventHandler(m_SerialPort_DataReceived);
                return true;
            }
            else
            {
                return false;
            }      
        }
        /// <summary>
        ///   RTU方式读取数据组帧
        /// </summary>
        private byte[] CreateReadHeader_RTU(InputModule cmdMsg)
        {
            byte[] byMsg = new byte[8];
            byMsg[0] = cmdMsg.bySlaveID;
            byMsg[1] = cmdMsg.byFunction;
            byte[] byAddr = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)cmdMsg.nStartAddr));
            byMsg[2] = byAddr[0];
            byMsg[3] = byAddr[1];
            byte[] byDataLength = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)cmdMsg.nDataLength));
            byMsg[4] = byDataLength[0];
            byMsg[5] = byDataLength[1];
            byte[] CRC = CRC16(byMsg);
            byMsg[byMsg.Length - 2] = CRC[0];
            byMsg[byMsg.Length - 1] = CRC[1];
            //定义接收buffer大小
            SetRecvBufSize(ref m_byRtuDataRecv, cmdMsg);
            return byMsg;
        }
        /// <summary>
        ///   RTU方式写入数据组帧
        /// </summary>
        private byte[] CreateWritrHeader_RTU(InputModule cmdMsg)
        {
            byte[] byMsg = null;
            int nWriteDataIndex = 0;
            if (cmdMsg.byFunction >= byWRITE_MULTI_COILS)
            {
                byMsg = new byte[6 + cmdMsg.byWriteData.Length + 3];
            }
            else
            {
                byMsg = new byte[6 + cmdMsg.byWriteData.Length];
            }
            byMsg[0] = cmdMsg.bySlaveID;
            byMsg[1] = cmdMsg.byFunction;
            byte[] byAddr = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)cmdMsg.nStartAddr));
            byMsg[2] = byAddr[0];
            byMsg[3] = byAddr[1];
            nWriteDataIndex = 3 + 1;
            if (cmdMsg.byFunction >= byWRITE_MULTI_COILS)
            {
                byte[] _cnt = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)cmdMsg.nDataLength));
                byMsg[4] = _cnt[0];			// Number of bytes
                byMsg[5] = _cnt[1];			// Number of bytes
                byMsg[6] = Convert.ToByte(cmdMsg.byWriteData.Length);
                nWriteDataIndex = 6 + 1;
            }
            Array.Copy(cmdMsg.byWriteData, 0, byMsg, nWriteDataIndex, cmdMsg.byWriteData.Length);
            byte[] CRC = CRC16(byMsg);
            byMsg[byMsg.Length - 2] = CRC[0];
            byMsg[byMsg.Length - 1] = CRC[1];
            //定义接收buffer大小
            SetRecvBufSize(ref m_byRtuDataRecv, cmdMsg);
            return byMsg;
        }

        /// <summary>
        /// 加入程序执行过程时间检测，避免进入死循环
        /// </summary>
        /// <param name="_time"></param>
        /// <returns></returns>
        public static bool timeSpanOutCheck(DateTime _time)
        {
            TimeSpan sp = DateTime.Now.Subtract(_time);
            if (sp.TotalMilliseconds > 2000) //
            { return true; }
            return false;
        }

        private OutputModule SendMessage_SP(byte[] byWriteData, bool bIsSync = false)
        {
            try
            {
                m_bRTUSync = bIsSync;
                m_SerialPort.DiscardOutBuffer();
                m_SerialPort.DiscardInBuffer();
                m_SerialPort.Write(byWriteData, 0, byWriteData.Length);
                if (event_MessageText != null)
                {
                    event_MessageText(BytesToStr(byWriteData), emMsgType.Send);
                }
                if (bIsSync)
                {
                    DateTime timeStart = DateTime.Now;
                    while (true)
                    {
                        Thread.Sleep(10);
                        if (m_output.byRecvData != null)
                            return m_output;
                        if (timeSpanOutCheck(timeStart))
                            return null;
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (event_MessageText != null)
                {
                    event_MessageText(ex.Message, emMsgType.Error);
                }
            }
            return null;
        }
        private void m_SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (m_SerialPort.BytesToRead <= 0)
                    return;

                for (int i = 0; i < m_byRtuDataRecv.Length; i++)
                {
                    try
                    {
                        m_byRtuDataRecv[i] = (byte)m_SerialPort.ReadByte();
                    }
                    catch (TimeoutException)
                    {
                        break;
                    }
                    catch (InvalidOperationException ex)
                    {
                        if (event_MessageText != null)
                        {
                            event_MessageText(ex.Message, emMsgType.Error);
                        }
                        break;
                    }
                }
                try
                {
                    if (!CheckCRC(m_byRtuDataRecv))
                    {
                        if (event_MessageText != null)
                        {
                            event_MessageText("消息校验出错！", emMsgType.Error);
                        }
                        m_SerialPort.DiscardInBuffer();
                        return;
                    }
                    //if (event_DataReceive != null)
                    //{
                    m_output.byFunction = m_byRtuDataRecv[1];
                    if ((m_output.byFunction == (byte)emFunction.ReadCoil)
                        || (m_output.byFunction == (byte)emFunction.ReadDiscreteInputs)
                        || (m_output.byFunction == (byte)emFunction.ReadHoldReg)
                        || (m_output.byFunction == (byte)emFunction.ReadInputReg)
                        )
                    {
                        m_output.nDataLength = m_byRtuDataRecv[3];
                    }
                    m_output.byRecvData = m_byRtuDataRecv;
                    if (event_DataReceive != null && (!m_bRTUSync))
                    {
                        event_DataReceive(m_output);
                    }
                    //}
                    if (event_MessageText != null)
                    {
                        event_MessageText(BytesToStr(m_byRtuDataRecv), emMsgType.Recv);
                    }
                }
                catch (Exception ex)
                {
                    if (event_MessageText != null)
                    {
                        event_MessageText("消息校验异常：" + ex.ToString(), emMsgType.Error);
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                if (event_MessageText != null)
                {
                    event_MessageText("SerialPort_DataReceived异常：" + ex.ToString(), emMsgType.Error);
                }
            }
        }
        #endregion
        #region 内部调用
        private void ThdProcSendMessage()
        {
            while(m_InputQueue.Count > 0)
            {
                Send(m_InputQueue.Dequeue());
                m_Event_SendMsg.WaitOne();
                if (!m_bConnected)
                {
                    break;
                }
                Thread.Sleep(m_nMsgIntervalTm);
            }
        }
        /// <summary>
        ///   
        /// </summary>
        private OutputModule Send(InputModule input, bool bIsRet = false)
        {
            m_output.nStartAddr = input.nStartAddr;
            m_output.bySlaveID = input.bySlaveID;
            m_output.nDataLength = input.nDataLength;
            m_output.byRecvData = null;
            OutputModule output = null;
            #region 功能码检测
            bool bReadOrWriteReg;
            if ((input.byFunction == byREAD_COIL)
                || (input.byFunction == byREAD_DISCRETE_INPUTS)
                || (input.byFunction == byREAD_HOLDING_REG)
                || (input.byFunction == byREAD_INPUT_REG))
            {
                bReadOrWriteReg = true;
            }
            else if ((input.byFunction == byWRITE_SINGLE_COIL)
                || (input.byFunction == byWRITE_MULTI_HOLDING_REG)
                || (input.byFunction == byWRITE_MULTI_COILS)
                || (input.byFunction == byWRITE_SINGLE_HOLDING_REG))
            {
                bReadOrWriteReg = false;
            }
            else
            {
                if (event_MessageText != null)
                {
                    event_MessageText("检测到不支持的功能码！", emMsgType.Error);
                }
                if (!bIsRet)
                {
                    m_Event_SendMsg.Set();
                }
                return output;
            }
                #endregion
            try
            {
                switch (m_nRunMode)
                {   
                    #region RTU
                case emCommMode.RTU:
                    if (bReadOrWriteReg)
                    {
                        output = SendMessage_SP(CreateReadHeader_RTU(input), bIsRet);
                    }
                    else
                    {
                        output = SendMessage_SP(CreateWritrHeader_RTU(input), bIsRet);
                    }
                    break;
                #endregion
                    #region TCP
                case emCommMode.TCP:
                    if (bReadOrWriteReg)
                    {
                        output = SendMessage_TCP_Sync(CreateReadMsg_TCP(input), bIsRet);
                    }
                    else
                    {
                        output = SendMessage_TCP_Sync(CreateWriteMsg_TCP(input), bIsRet);
                    }
                        break;
                #endregion
                    default:
                        break;
                }
            }
            catch (System.Exception ex)
            {
                if (event_MessageText != null)
                {
                    event_MessageText(ex.Message, emMsgType.Error);
                }
            }
            if (!bIsRet)
            {
                m_Event_SendMsg.Set();
            }
            return output;
        }
        private bool CheckCRC(byte[] byMsg)
        {
            int nEndIndex = byMsg.Length - 1;
            while (byMsg[nEndIndex] == 0 || (byMsg[nEndIndex] == '\0'))
            {
                nEndIndex--;
            }
            byte[] byCRC = new byte[2];
            byCRC[1] = byMsg[nEndIndex];
            byCRC[0] = byMsg[nEndIndex - 1];
            byte[] byTmp = new byte[nEndIndex+1];
            Array.Copy(byMsg, 0, byTmp, 0, byTmp.Length);
            byte[] byTmp2 = CRC16(byTmp);
            if ((byCRC[0] == byTmp2[0]) && (byCRC[1] == byTmp2[1]))
            {
                return true;
            }
            return false;
        }
        private void SetRecvBufSize(ref byte[] byArr, InputModule input)
        {
            if (byArr != null)
            {
                Array.Clear(byArr, 0, byArr.Length);
                byArr = null;
            }
            int nHead = 20, nCRC = 0;
            switch (m_nRunMode)
            {
                case emCommMode.TCP:
                        nHead = 8;
                        break;
                case emCommMode.RTU:
                        nHead = 2;
                        nCRC = 2;
                        break;
                case emCommMode.ASCII:
                        break;
                default:
                    break;

            }
            if ((input.byFunction == byREAD_COIL)||(input.byFunction == byREAD_DISCRETE_INPUTS))
            {
                int nCount, nTemp = input.nDataLength;
                nCount = ((nTemp % 8) == 0) ? (nTemp / 8) : ((nTemp - (nTemp%8))/8+1);
                byArr = new byte[nHead + 1 + nCount + nCRC];
            }
            else if ((input.byFunction == byREAD_HOLDING_REG)
                || (input.byFunction == byREAD_INPUT_REG))
            {
                byArr = new byte[nHead + 1 +input.nDataLength * 2 + nCRC];
            }
            else if ((input.byFunction == byWRITE_SINGLE_COIL)
                ||(input.byFunction == byWRITE_MULTI_HOLDING_REG)
                ||(input.byFunction == byWRITE_MULTI_COILS)
                ||(input.byFunction == byWRITE_SINGLE_HOLDING_REG))
            {
                byArr = new byte[nHead + 4 + nCRC];
            }
        }
        private string BytesToStr(byte[] by)
        {
            string strRet = null;
            switch(m_nMsgFormat)
            {
                case emMsgFormat.Hex:
                    strRet = BitConverter.ToString(by);
                    strRet = strRet.Replace('-', ' ');
                    break;
                case emMsgFormat.Decimal:
                    for (int i = 0; i < by.Length; i++ )
                    {
                        strRet += Convert.ToString(by[i], 10) +" ";
                    }
                    break;
                default:
                    break;
            }
            return strRet;
        }
        #endregion
        #region 供外部调用
        //断开连接
        public void Disconnect()
        {
            switch (m_nRunMode)
            {
                case emCommMode.RTU:
                    if (m_SerialPort != null)
                    {
                        m_SerialPort.Close();
                    }
                    break;
                case emCommMode.TCP:
                    if (thd_TcpConnect != null && thd_TcpConnect.IsAlive)
                    {
                        thd_TcpConnect.Abort();
                    }
                    if(m_socketTcp != null)
                    {
                        m_socketTcp.Disconnect(true);
                    }
                    isTcpConnected = false;
                    break;
                default:
                    break;
            }
        }
        //连接
        public bool Connect()
        {
            switch(m_nRunMode)
            {
                case emCommMode.RTU:
                    return SPconnect();
                case emCommMode.TCP:
                    return TCPconnect(m_IP, m_nPort);
                default:
                    return false;
            }
        }
        //发送消息
        public void SendMessage(InputModule input)
        {
            m_InputQueue.Enqueue(input);
            if (!m_thd_SendMsg.IsAlive)
            {
                m_thd_SendMsg = new Thread(new ThreadStart(ThdProcSendMessage));
                m_thd_SendMsg.IsBackground = true;
                m_thd_SendMsg.Start();
            }
        }
        private object LockMsg = new object();
        public OutputModule SendMessage_Sync(InputModule input)
        {
            lock (LockMsg)
            {
                return Send(input, true);
            }
        }
        // CRC校验
        public byte[] CRC16(byte[] byBuffer)
        {
            ushort CRCFull = 0xFFFF;
            byte CRCHigh = 0xFF, CRCLow = 0xFF;
            char CRCLSB;
            byte[] CRC = new byte[2];
            for (int i = 0; i < (byBuffer.Length) - 2; i++)
            {
                CRCFull = (ushort)(CRCFull ^ byBuffer[i]);

                for (int j = 0; j < 8; j++)
                {
                    CRCLSB = (char)(CRCFull & 0x0001);
                    CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

                    if (CRCLSB == 1)
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                }
            }
            CRC[1] = CRCHigh = (byte)((CRCFull >> 8) & 0xFF);
            CRC[0] = CRCLow = (byte)(CRCFull & 0xFF);
            return CRC;
        }
        public string Dispose()
        {
            try
            {
                switch (m_nRunMode)
                {
                    case emCommMode.TCP:
                        {
                            if (m_socketTcp != null)
                            {
                                if (m_socketTcp.Connected)
                                {
                                    try
                                    {
                                        m_socketTcp.Shutdown(SocketShutdown.Both);
                                        m_socketTcp.Disconnect(true);
                                    }
                                    catch (System.Exception ex)
                                    {
                                        throw ex;
                                    }

                                }
                                m_socketTcp.Dispose();
                                m_socketTcp = null;
                            }
                        }
                        break;
                    case emCommMode.RTU:
                        {
                            if ((m_SerialPort != null) && (m_SerialPort.IsOpen))
                            {
                                m_SerialPort.Close();
                            }
                            m_SerialPort.Dispose();
                            m_SerialPort = null;
                        }
                        break;
                    default:
                        break;
                }
                if (m_thd_SendMsg.IsAlive)
                {
                    m_thd_SendMsg.Abort();
                }
            }
            catch (System.Exception ex)
            {
                return ex.Message;
            }
            return null;
        }
        #endregion
        ~Modbus()
        {
            //Dispose();
        }

        #region CRC 数组定义
        static byte[] aucCRCHi = new byte[]{
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40
        };

        static byte[] aucCRCLo = new byte[]{
            0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7,
            0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E,
            0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09, 0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9,
            0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC,
            0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
            0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32,
            0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D,
            0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A, 0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 
            0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF,
            0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
            0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1,
            0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4,
            0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F, 0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 
            0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA,
            0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
            0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0,
            0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97,
            0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C, 0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E,
            0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89,
            0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
            0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83,
            0x41, 0x81, 0x80, 0x40
        };
        #endregion
        public byte[] SlaveParaCRC16(byte[] by)
        {
            byte ucCRCHi = 0xFF, ucCRCLo = 0xFF;
            int nIndex = 0, nLength = by.Length;
            int i = 0;
            while(nLength-- > 0)
            {
                nIndex = ucCRCLo ^by[i++];
                ucCRCLo = (byte)(ucCRCHi ^ aucCRCHi[nIndex]);
                ucCRCHi = aucCRCLo[nIndex];
            }
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)(ucCRCHi << 8 | ucCRCLo)));
        }

    }
    public class InputModule
    {
       public byte bySlaveID;
       public int nStartAddr;
       public byte byFunction;
       public int nDataLength;
       public byte[] byWriteData;
    }
    public class OutputModule 
    {
        public byte bySlaveID;
        public int nStartAddr;
        public byte byFunction;
        public int nDataLength;
        public byte[] byRecvData;
    }
    public class IEEE754Converter
    {
        /// <summary>
        ///   将4个byte组成的数组所表示的无符号32位整型转成对应的浮点型
        /// </summary>
        public static float U32ToFloat(byte[] by)
        {
            UInt32 u32 = (UInt32)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(by, 0));
            float f = 0;
            unsafe
            {
                f = (*((float*)&u32));
            }
            return f;
        }
        /// <summary>
        ///  将浮点型转成无符号32位整型，返回其对应的4个byte数组表示形式
        /// </summary>
        public static byte[] FloatToU32(float f)
        {
            UInt32 u32 = 0;
            unsafe
            {
                u32 = (*((UInt32 *)&f));
            }
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)u32));
        }
    }

    /// <summary>
    /// PLC的浮点数 转换
    /// </summary>
    public class PLCFloatConverter
    {
        /// <summary>
        /// 将byte数组中的两个字节转成32位有符号整型，再转换成主机字节序
        /// </summary>
        /// <param name="bys"></param>
        /// <param name="nStartIndex"></param>
        /// <param name="DecimalDigit">小数位数</param>
        /// <returns></returns>
        public static float NetToHostOrder32(byte[] bys, int nStartIndex = 0, double DecimalDigit = 1)
        {
            byte[] byCov = new byte[4];
            byCov[0] = bys[nStartIndex + 2];
            byCov[1] = bys[nStartIndex + 3];
            byCov[2] = bys[nStartIndex];
            byCov[3] = bys[nStartIndex + 1];
            int n = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(byCov, 0));
            return (float)(n * DecimalDigit);
        }


        /// <summary>
        /// 将32位有符号整型转换成网络字节序的byte数组
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static byte[] HostToNetOrder32(int Value)
        {
            byte[] bys = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Value));
            //每两个byte前后互换
            byte[] byRet = new byte[4];
            byRet[0] = bys[2];
            byRet[1] = bys[3];
            byRet[2] = bys[0];
            byRet[3] = bys[1];
            return byRet;
        }
    }
}
