using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace OQC_IC_CHECK_System
{
    class SerialPortModbus
    {
        Logs log = Logs.LogsT();
        MyFunction myfunction = new MyFunction();
        public SerialPort PCS_Port;
        string port_Name = null;
        private bool useHex;
        private bool useASCII;
        private bool useUTF8;
        private bool useUnicode;
        private bool useDefault=true;

        /// <summary>
        /// 串口名称
        /// </summary>
        public string PortName
        {
            get { return port_Name; }
            set { port_Name = value; }
        }

        private bool _ready = false;
        /// <summary>
        /// 检查机准备好
        /// </summary>
        public bool PortReady
        {
            get { return _ready; }
            set { _ready = value; }
        }

        public SerialPortModbus(string port)
        {
            PortName = port;
            PCS_Port = new SerialPort();
            PCS_Port.BaudRate = 115200;
            PCS_Port.DataBits = 8;
            PCS_Port.PortName = port_Name;
            PCS_Port.Parity = Parity.None;
            PCS_Port.StopBits = StopBits.One;
            PCS_Port.DtrEnable = false;
            PCS_Port.RtsEnable = false;
            PCS_Port.DataReceived += new SerialDataReceivedEventHandler(Com_DataReceived);
        }

        private void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (useDefault)
                {
                    string reDataStr = PCS_Port.ReadTo("\n\n");
                    if (reDataStr == "") return;
                    log.AddCommLOG("串口通信---->接收:" + reDataStr);
                    //处理接收到的数据
                    QueryData(reDataStr);
                }
                else
                {
                    // 开辟接收缓冲区
                    byte[] ReDatas = new byte[PCS_Port.BytesToRead];
                    //从串口读取数据
                    PCS_Port.Read(ReDatas, 0, ReDatas.Length);
                    //实现数据的解码
                    FormateData(ReDatas);
                }
            }catch(Exception ex)
            {
                MsgBox("串口通信异常!","异常",Color.Red);
            }
        }

        /// <summary>
        /// 处理串口接收的数据
        /// </summary>
        /// <param name="reDataStr"></param>
        private void QueryData(string reDataStr)
        {
            string cass = reDataStr.Substring(1, 1);
            string msg = reDataStr.Substring(1, reDataStr.IndexOf("#") - 1);//接收到的消息
            string message = string.Empty;
            switch (cass.ToUpper())
            {
                case "E"://握手-【回复握手】
                    if (!GlobalVar.SoftWareShutDown) message = "!" + msg + "#" + myfunction.CRC8(msg) + "\n";
                    SendMsg(message);
                    if(msg.Contains('1'))//允许作业
                        MyFunction.SendMessage(GlobalVar.gl_IntPtr_MainWindow, GlobalVar.WM_PCSConnection, (IntPtr)0, (IntPtr)0);//握手成功
                    if (msg.Contains('0'))//禁止作业
                        MyFunction.SendMessage(GlobalVar.gl_IntPtr_MainWindow,GlobalVar.WM_PCSForbideWork,(IntPtr)0,(IntPtr)0);
                    break;
                case "W"://通知检查机托盘到位-【检查机收到信号】
                    MyFunction.SendMessage(GlobalVar.gl_IntPtr_MainWindow, GlobalVar.WM_PCSStartCheck, (IntPtr)0, (IntPtr)0);//到位，开始检测
                    break;
                case "D"://回归到位-【暂不处理】
                    break;
                case "F"://IC测试结果发送-【检查机收到数据】
                    MyFunction.SendMessage(GlobalVar.gl_IntPtr_MainWindow, GlobalVar.WM_PCSAcceptData, (IntPtr)0, (IntPtr)0);//接收到IC条码数据
                    break;
                case "G"://异常消息，需要报警灯
                    message = "!" + msg + "#" + myfunction.CRC8(msg) + "\n";
                    SendMsg(message);//回复检查机
                    if (msg.Contains("1"))//需要报警
                    {
                        MyFunction.SendMessage(GlobalVar.gl_IntPtr_MainWindow, GlobalVar.WM_PCSCheckEMG, (IntPtr)0, (IntPtr)0);
                    }
                    if (msg.Contains("0"))//报警解除
                    {
                        MyFunction.SendMessage(GlobalVar.gl_IntPtr_MainWindow, GlobalVar.WM_PCSCheckEMG_Reset, (IntPtr)0, (IntPtr)0);
                    }
                    break;
                case "M"://允许下料机下料
                    message = "!" + msg + "#" + myfunction.CRC8(msg) + "\n";
                    SendMsg(message);//回复检查机          
                    //if (msg.Contains('0'))
                       // MyFunction.SendMessage(GlobalVar.gl_IntPtr_MainWindow, GlobalVar.WM_PCSAllowDropBoard, (IntPtr)0, (IntPtr)0);
                    if (msg.Contains('1'))
                        MyFunction.SendMessage(GlobalVar.gl_IntPtr_MainWindow, GlobalVar.WM_PCSForbideDrop, (IntPtr)0, (IntPtr)0);
                    break;
                case "L"://拍照完成
                    message = "!" + msg + "#" + myfunction.CRC8(msg) + "\n";
                    SendMsg(message);//回复检查机  
                    if (msg.Contains('1'))
                    {
                        MyFunction.SendMessage(GlobalVar.gl_IntPtr_MainWindow, GlobalVar.WM_PCSNG, (IntPtr)0, (IntPtr)0);
                    }
                    else
                    {
                        MyFunction.SendMessage(GlobalVar.gl_IntPtr_MainWindow, GlobalVar.WM_PCSResult, (IntPtr)0, (IntPtr)0);
                    }
                    break;
                case "T":
                    break;
                case "S"://IC禁用模式【不处理】
                    MyFunction.SendMessage(GlobalVar.gl_IntPtr_MainWindow, GlobalVar.WM_PCSArrive, (IntPtr)0, (IntPtr)0);
                    break;
                default:
                    MsgBox("通信异常:未设定命令！", "串口通信异常", Color.Red);
                    break;
            }
        }



        private void FormateData(byte[] reDatas)
        {
            string RecDataStr = "";
            if (useHex)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < reDatas.Length; i++)
                {
                    sb.AppendFormat("{0:x2}" + " ", reDatas[i]);
                }
                RecDataStr = sb.ToString().ToUpper();
            }
            if (useASCII)
            {
                RecDataStr = new ASCIIEncoding().GetString(reDatas);
            }
            if (useUTF8)
                RecDataStr = new UTF8Encoding().GetString(reDatas);
            if (useUnicode)
                RecDataStr = new UnicodeEncoding().GetString(reDatas);
            //处理接收到的数据
            QueryData(RecDataStr);
        }




        /// <summary>
        /// 设置串口参数
        /// </summary>
        /// <param name="port">COM口名称</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="parity">奇偶校验</param>
        /// <param name="stopbits">停止位</param>
        /// <param name="dtr">DTR是否可用</param>
        /// <param name="rts">RTS是否可用</param>
        public void SetSerialPortPara(string port, int baudRate, int dataBits, Parity parity, StopBits stopbits, bool dtr, bool rts)
        {
            PCS_Port.BaudRate = baudRate;
            PCS_Port.DataBits = dataBits;
            PCS_Port.PortName = port;
            PCS_Port.Parity = parity;
            PCS_Port.StopBits = stopbits;
            PCS_Port.DtrEnable = dtr;
            PCS_Port.RtsEnable = rts;
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        public void OpenPCSPort()
        {
            if (port_Name != null)
            {
                int i = 0;
                for (; ; )
                {
                    if (PCS_Port.IsOpen) return;
                    else
                    {
                        if (i >= 3)
                        {
                            MsgBox("打开串口超时", "串口异常", Color.Red);
                            return;
                        }
                        i++;
                        PCS_Port.Open();
                        Thread.Sleep(1000);
                    }
                }
            }
            else
                MsgBox("COM口不能为空！", "异常", Color.Red);
        }
        /// <summary>
        /// 关闭串口
        /// </summary>
        public void ClosePCSPort()
        {
            if (port_Name != null)
            {
                int i = 0;
                for (; ; )
                {
                    if (!PCS_Port.IsOpen) return;
                    else
                    {
                        if (i >= 3)
                        {
                            MsgBox("关闭串口超时", "串口异常", Color.Red);
                            return;
                        }
                        i++;
                        PCS_Port.Close();
                        PCS_Port.Dispose();
                        GC.Collect();
                        GC.CancelFullGCNotification();
                        Thread.Sleep(1000);
                    }
                }
            }
            else
                MsgBox("COM口不能为空！", "异常", Color.Red);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg">消息内容</param>
        public void SendMsg(string msg)
        {
            if (PCS_Port.IsOpen)
            {
                try
                {
                    if (msg == "") return;
                    PCS_Port.Write(msg);
                    log.AddCommLOG("串口通信---->发送:"+msg);
                }
                catch (Exception ex)
                {
                    MsgBox("串口通信异常：" + ex.Message, "异常", Color.Red);
                }
            }
            else
            {
                MsgBox("串口未打开\r\n请打开串口", "警告", Color.Red);
                OpenPCSPort();
            }
        }

        /// <summary>
        /// 设置发送读取模式
        /// </summary>
        /// <param name="mode">模式 1:Hex      2:ASCII     3:UTF-8      4:Unicode   5:string</param>
        public void setParaMode(int mode)
        {
            switch (mode)
            {
                case 1:
                    useHex = true;
                    useDefault = false;
                    useASCII = false;
                    useUTF8 = false;
                    useUnicode = false;
                    break;
                case 2:
                    useHex = false;
                    useDefault = false;
                    useASCII = true;
                    useUTF8 = false;
                    useUnicode = false;
                    break;
                case 3:
                    useHex = false;
                    useDefault = false;
                    useASCII = false;
                    useUTF8 = true;
                    useUnicode = false;
                    break;
                case 4:
                    useHex = false;
                    useDefault = false;
                    useASCII = false;
                    useUTF8 = false;
                    useUnicode = true;
                    break;
                case 5:
                    useHex = false;
                    useDefault = true;
                    useASCII = false;
                    useUTF8 = false;
                    useUnicode = false;
                    break;
                default:
                    useHex = false;
                    useDefault = true;
                    useASCII = false;
                    useUTF8 = false;
                    useUnicode = false;
                    break;
            }
        }

        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="msg">信息内容</param>
        /// <param name="mode">发送模式       1:Hex      2:ASCII     3:UTF-8      4:Unicode</param>
        public void SendData(string msg, int mode)
        {
            byte[] sendData = null;

            switch (mode)
            {
                case 1:
                    sendData = strToHexByte(msg);
                    useHex = true;
                    break;
                case 2:
                    sendData = Encoding.ASCII.GetBytes(msg);
                    useASCII = true;
                    break;
                case 3:
                    sendData = Encoding.UTF8.GetBytes(msg);
                    useUTF8 = true;
                    break;
                case 4:
                    sendData = Encoding.Unicode.GetBytes(msg);
                    useUnicode = true;
                    break;
                default:
                    sendData = strToHexByte(msg);
                    break;
            }
            if (PCS_Port.IsOpen)
            {
                try
                {
                    PCS_Port.Write(sendData, 0, sendData.Length);
                }
                catch (Exception ex)
                {
                    MsgBox("串口通信异常：" + ex.Message, "异常", Color.Red);
                }
            }
            else
            {
                MsgBox("串口未打开\r\n请打开串口", "警告", Color.Red);
                OpenPCSPort();
            }
        }
        /// <summary>
        /// 16进制编码
        /// </summary>
        /// <param name="hexString">字符串</param>
        /// <returns></returns>
        private byte[] strToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0) hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2).Replace(" ", ""), 16);
            return returnBytes;
        }






        /// <summary>
        /// 弹框【OK或者Cancel】
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="title">标题</param>
        /// <param name="backcolor">背景色</param>
        /// <returns></returns>
        private bool MsgBox(string message, string title, Color color)
        {
            using (MsgBox box = new MsgBox(MessageBoxButtons.OK))
            {
                box.Title = "串口异常";
                box.ShowText = message;
                box.SetBackColor = color;
                if (box.ShowDialog() == DialogResult.OK) return true;
                else return false;
            }
        }
    }
}
