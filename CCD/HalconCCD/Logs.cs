using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.IO;
using System.Windows.Forms;

namespace HalconCCD
{
    /// <summary>
    /// 日志类（单例类）
    /// </summary>
    public sealed class Logs
    {
        /// <summary>
        /// 日志文件夹名称
        /// </summary>
        private readonly string FolderName = string.Format(@"\LOG\{0}\", "COMM_LOG");

        /// <summary>
        /// 异常日志文件夹名称
        /// </summary>
        private readonly string ErrorFolderName = string.Format(@"\LOG\{0}\", "ERROR_LOG");

        /// <summary>
        /// 保存日志的队列
        /// </summary>
        private ConcurrentQueue<string> Comm_LOGQueue = new ConcurrentQueue<string>();
        /// <summary>
        /// 保存异常日志的队列
        /// </summary>
        private ConcurrentQueue<string> ERROR_LOGQueue = new ConcurrentQueue<string>();
        /// <summary>
        /// 当前准备写入日志的队列的数量
        /// </summary>
        private int PresentComm_LOGCount = 0;
        /// <summary>
        /// 当前准备写入异常日志的队列的数量
        /// </summary>
        private int PresentERROR_LOGCount = 0;
        private bool isFirstWriteLog = true;//是否第一次写日志

        private static Logs log;
        private Logs()
        {
            Thread runWriteCommLOG = new Thread(ThreadWriteCommLOG);
            runWriteCommLOG.IsBackground = true;
            runWriteCommLOG.Start();

            Thread runWriteERRORLOG = new Thread(ThreadWriteERRORLOG);
            runWriteERRORLOG.IsBackground = true;
            runWriteERRORLOG.Start();
        }

        public static Logs LogsT()
        {
            if (log == null) log = new Logs();
            return log;
        }

        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="str">日志内容</param>
        public void AddCommLOG(string str)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\t\t";//每行开头时间
            string str_record = time + str;
            Comm_LOGQueue.Enqueue(str_record);
        }

        private void ThreadWriteCommLOG()
        {
            while (true)
            {
                if (!Comm_LOGQueue.IsEmpty)
                {
                    PresentComm_LOGCount = Comm_LOGQueue.Count;
                    WriteCommLOG();
                    PresentComm_LOGCount = 0;
                }
                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// 写日志
        /// </summary>
        private void WriteCommLOG()
        {
            try
            {
                string filename = DateTime.Now.ToString("yyyyMMddHH");//文件名称
                string filenameLastHour = DateTime.Now.AddHours(-1).ToString("yyyyMMddHH");//上一个小时的文件名称
                string dirName = Application.StartupPath + FolderName;
                if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);

                string _logfile = dirName + filename + ".log";
                string _logfileLastHour = dirName + filenameLastHour + ".log";
                FileStream FS = new FileStream(_logfile, FileMode.Append, FileAccess.Write, FileShare.Write);
                FileStream FSLastHour = new FileStream(_logfileLastHour, FileMode.Append, FileAccess.Write, FileShare.Write);
                StringBuilder writestr = new StringBuilder(PresentComm_LOGCount);
                StringBuilder writestrLastHour = new StringBuilder(PresentComm_LOGCount);
                for (int i = 0; i < PresentComm_LOGCount; i++)
                {
                    string tempstr = string.Empty; ;
                    if (Comm_LOGQueue.TryDequeue(out tempstr))
                    {
                        DateTime time;
                        if (tempstr.Length >= 19 && DateTime.TryParse(tempstr.Substring(0, 19), out time))
                        {
                            string strtime = time.ToString("yyyyMMddHH");
                            if (isFirstWriteLog)
                            {
                                StringBuilder start = new StringBuilder();
                                for (int j = 0; j < 80; j++)
                                {
                                    start.Append("*");
                                }
                                start.Insert(start.Length / 2, "程序开启");
                                tempstr = start.AppendLine().Append(tempstr).ToString();
                                isFirstWriteLog = false;
                            }
                            if (strtime == filename) writestr.AppendLine(tempstr);
                            else if (strtime == filenameLastHour) writestrLastHour.AppendLine(tempstr);
                        }
                    }
                    else break;
                }
                StreamWriter SW = new StreamWriter(FS, Encoding.Default);
                SW.Write(writestr);
                SW.Close();
                SW.Dispose();
                if (writestrLastHour.Length > 0)
                {
                    StreamWriter SWLastHour = new StreamWriter(FSLastHour, Encoding.Default);
                    SWLastHour.Write(writestrLastHour);
                    SWLastHour.Close();
                    SWLastHour.Dispose();
                }
                else
                {
                    FSLastHour.Dispose();
                    if (File.ReadAllBytes(_logfileLastHour).Length == 0) File.Delete(_logfileLastHour);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("CommLog Error:"+ex.Message+"\r\n"+ex.StackTrace);
                Console.WriteLine("\r\n！！！Logs Error\r\n" + ex.Message);
            }
        }

        /// <summary>
        /// 添加异常日志
        /// </summary>
        /// <param name="str">异常日志内容</param>
        public void AddERRORLOG(string str)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\t\t";//每行开头时间
            string str_record = time + str;
            Comm_LOGQueue.Enqueue(str_record);
            ERROR_LOGQueue.Enqueue(str_record);
        }

        private void ThreadWriteERRORLOG()
        {
            while (true)
            {
                if (!ERROR_LOGQueue.IsEmpty)
                {
                    PresentERROR_LOGCount = ERROR_LOGQueue.Count;
                    WriteERRORLOG();
                    PresentERROR_LOGCount = 0;
                }
                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// 写异常日志
        /// </summary>
        private void WriteERRORLOG()
        {
            try
            {
                string filename = DateTime.Now.ToString("yyyyMMdd");//文件名称
                string filenameLastHour = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");//前一天的文件名称
                string dirName = Application.StartupPath + ErrorFolderName;
                if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);

                string _logfile = dirName + filename + ".log";
                string _logfileLastHour = dirName + filenameLastHour + ".log";
                FileStream FS = new FileStream(_logfile, FileMode.Append, FileAccess.Write, FileShare.Write);
                FileStream FSLastHour = new FileStream(_logfileLastHour, FileMode.Append, FileAccess.Write, FileShare.Write);
                StringBuilder writestr = new StringBuilder(PresentERROR_LOGCount);
                StringBuilder writestrLastHour = new StringBuilder(PresentComm_LOGCount);
                for (int i = 0; i < PresentERROR_LOGCount; i++)
                {
                    string tempstr = string.Empty;
                    if (ERROR_LOGQueue.TryDequeue(out tempstr))
                    {
                        DateTime time;
                        if (tempstr.Length >= 10 && DateTime.TryParse(tempstr.Substring(0, 10), out time))
                        {
                            string strtime = time.ToString("yyyyMMdd");
                            if (strtime == filename) writestr.AppendLine(tempstr);
                            else if (strtime == filenameLastHour) writestrLastHour.AppendLine(tempstr);
                        }
                    }
                    else break;
                }
                StreamWriter SW = new StreamWriter(FS, Encoding.Default);
                SW.Write(writestr);
                SW.Close();
                SW.Dispose();
                if (writestrLastHour.Length > 0)
                {
                    StreamWriter SWLastHour = new StreamWriter(FSLastHour, Encoding.Default);
                    SWLastHour.Write(writestrLastHour);
                    SWLastHour.Close();
                    SWLastHour.Dispose();
                }
                else
                {
                    FSLastHour.Dispose();
                    if (File.ReadAllBytes(_logfileLastHour).Length == 0) File.Delete(_logfileLastHour);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error Error:" + ex.Message + "\r\n" + ex.StackTrace);
                Console.WriteLine("\r\n！！！Logs ERROR_LOG Error\r\n" + ex.Message);
            }
        }
    }
}
