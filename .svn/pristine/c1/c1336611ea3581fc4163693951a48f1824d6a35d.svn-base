using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OQC_IC_CHECK_System
{
    public partial class SystemForm : Frame
    {
        public SystemForm()
        {
            InitializeComponent();
        }

        private void SystemForm_Load(object sender, EventArgs e)
        {
            MyFunction myfunction = new MyFunction();
            this.label_Version.Text = "软件版本:" + myfunction.GetVersion();
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            if (!MsgBox("确定关闭软件？", Color.Blue, MessageBoxButtons.OKCancel)) return;

            GlobalVar.SoftWareShutDown = true;
            GlobalVar.PCS_Port.ClosePCSPort();//关闭串口
            Application.ExitThread();
            Application.Exit();
        }

        private void btn_MinBox_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            MyFunction.SendMessage(GlobalVar.gl_IntPtr_MainWindow, GlobalVar.WM_Mininize, (IntPtr)0, (IntPtr)0);
        }

        private void btn_ShutDown_Click(object sender, EventArgs e)
        {
            if (!MsgBox("确定关机？", Color.Blue, MessageBoxButtons.OKCancel)) return;

            RunCMD("shutdown -s -t 0");
        }

        private void btn_ReStart_Click(object sender, EventArgs e)
        {
            if (!MsgBox("确定重启电脑？", Color.Blue, MessageBoxButtons.OKCancel)) return;

            RunCMD("shutdown -r -t 0");
        }

        private void RunCMD(string cmdorder)
        {
            Process myProcess = new Process();
            myProcess.StartInfo.FileName = "cmd.exe";
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardInput = true;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.StartInfo.RedirectStandardError = true;
            myProcess.StartInfo.CreateNoWindow = true; myProcess.Start();
            myProcess.StandardInput.WriteLine(cmdorder);
        }



        private void btn_Set_Click(object sender, EventArgs e)
        {
            ShowFrame(ClickBtn.SoftWareSet, true);
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
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 显示窗体
        /// </summary>
        /// <param name="cb">按钮</param>
        /// <param name="NeedPassword">是否需要密码</param>
        private void ShowFrame(ClickBtn cb, bool NeedPassword = false)
        {
            if (NeedPassword && !Password()) return;

            try
            {
                Frame form = null;
                switch (cb)
                {
                    case ClickBtn.SoftWareSet:
                        form = new SetForm();
                        break;
                    case ClickBtn.CCDPara:
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

            }
        }

    }
}
