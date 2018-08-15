using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OQC_IC_CHECK_System
{
    public partial class Load : Form
    {
        private BackgroundWorker backworker = new BackgroundWorker();
        private delegate void dele_Process(string str);//显示进度的委托
        private MyFunction myfunction = new MyFunction();
        private Logs log = Logs.LogsT();

        public Load()
        {
            InitializeComponent();

            backworker.DoWork += Backworker_DoWork1;
            backworker.RunWorkerCompleted += Backworker_RunWorkerCompleted;
        }

        private void Backworker_DoWork1(object sender, DoWorkEventArgs e)
        {
            Console.Write("input:" + e.Argument);

            try
            {
                Initial();
#if !DEBUG
                //创建windows计划任务
                TaskScheduler._TASK_STATE taskstate_CleanLogs = WindowsSchedule.CreateTaskScheduler(
                    "", "CleanLogs", string.Format("{0}定期删除日志文件", "IC&PCS BARCODE检查机 PC电脑 "),
                    Environment.CurrentDirectory + @"\CleanLogFiles.exe", Environment.CurrentDirectory + @"\Log", "PT24H0M");
#endif
            }
            catch (Exception ex)
            {
                log.AddERRORLOG(ex.Message);
                MsgBox(ex.Message, Color.Red, MessageBoxButtons.OK);
                Environment.Exit(66);
            }
        }


        private void Backworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MainForm main = new MainForm();
            this.ShowInTaskbar = false;
            this.Hide();
            main.Show();
        }

        private void Initial()
        {
            try
            {
                myfunction.ReadInitConfig();//读取Config配置文件【Config.ini】
                myfunction.LoadICPointList();
            }
            catch (Exception ex) { throw new Exception("读取Config配置文件异常：" + ex.Message); }
            //不使用CAD
            //try
            //{
            //    GlobalVar.m_dwgdirectServices = new DWGdirect.Runtime.Services();
            //    myfunction.LoadCADFile(GlobalVar.Product);
            //}
            //catch (Exception ex) { throw new Exception("读取CAD文件异常：" + ex.Message); }
            try
            {
                myfunction.ReadAlarmRecord();//读取报警频次记录
            }
            catch (Exception ex) { throw new Exception("读取报警频次记录文件异常：" + ex.Message); }
            GlobalVar.AxisPCI = new PCI1285_E();//初始化板卡
            GlobalVar.AxisPCI.ServerOn(GlobalVar.AxisX, true);//开启伺服
            GlobalVar.AxisPCI.ServerOn(GlobalVar.AxisY, true);//开启伺服
            GlobalVar.AxisPCI.ServerOn(GlobalVar.AxisA, true);//开启伺服
            GlobalVar.AxisPCI.ServerOn(GlobalVar.AxisB, true);//开启伺服
            GlobalVar.AxisPCI.SetCMDPosition(GlobalVar.AxisZ.LinkIndex, GlobalVar.ZPosition_Read);//设置Z轴的脉冲，相机高度
            GlobalVar.Feed_Modbus = new CModbus(GlobalVar.FeedIP);//上料机MODBUS初始化
            GlobalVar.Drop_Modbus = new CModbus(GlobalVar.DropIP);//下料机MODBUS初始化
        }
        #region quit
        private void lb_Close_MouseDown(object sender, MouseEventArgs e)
        {
            lb_Close.BackColor = Color.Red;
            lb_Close.Image = OQC_IC_CHECK_System.Properties.Resources.close_2;

        }

        private void lb_Close_MouseUp(object sender, MouseEventArgs e)
        {
            lb_Close.BackColor = Color.Transparent;
            lb_Close.Image = OQC_IC_CHECK_System.Properties.Resources.close_1;
        }

        private void lb_Close_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }
        private void lb_Close_MouseEnter(object sender, EventArgs e)
        {
            lb_Close.BackColor = Color.Red;
        }
        private void lb_Close_MouseLeave(object sender, EventArgs e)
        {
            lb_Close.BackColor = Color.Transparent;
        }
        #endregion

        private void Load_Load(object sender, EventArgs e)
        {
            backworker.RunWorkerAsync(this);
        }
        /// <summary>
        /// 弹框【OK或者Cancel】
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="backcolor">背景色</param>
        /// <returns></returns>
        private bool MsgBox(string text, Color backcolor, MessageBoxButtons btn)
        {
            using (MsgBox box = new MsgBox(btn))
            {
                box.Title = "初始化异常";
                box.ShowText = text;
                box.SetBackColor = backcolor;
                if (box.ShowDialog() == DialogResult.OK) return true;
                else return false;
            }
        }
    }
}
