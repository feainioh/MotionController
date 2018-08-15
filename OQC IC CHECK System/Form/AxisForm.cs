using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace OQC_IC_CHECK_System
{
    public partial class AxisForm : Frame
    {
        MyFunction myfunction = new MyFunction();
        Logs log = Logs.LogsT();

        /// <summary>
        /// JOG运动 等待
        /// </summary>
        private ManualResetEventSlim JOGWait = new ManualResetEventSlim(false);
        /// <summary>
        /// 选中的轴的序号
        /// </summary>
        private int SelectAxisIndex = -1;
        /// <summary>
        /// 是否正方向
        /// </summary>
        private bool Positive = true;

        private Thread Thd_Jog;//线程运行

        public AxisForm()
        {
            InitializeComponent();
        }

        private void AxisForm_Load(object sender, EventArgs e)
        {
            #region 设置轴绑定
            //清空绑定信息
            CmbAxes_X.Items.Clear();
            CmbAxes_Y.Items.Clear();
            CmbAxes_Z.Items.Clear();
            CmbAxes_C.Items.Clear();
            CmbAxes_D.Items.Clear();
            CmbAxes_B.Items.Clear();
            CmbAxes_A.Items.Clear();

            //轴数量为0，退出
            if (GlobalVar.AxisPCI.AxisCount == 0) return;
            List<string> str = new List<string>();
            //获取所有轴
            for (int i = 0; i < GlobalVar.AxisPCI.AxisCount; i++)
            {
                str.Add(String.Format("{0:d}-Axis", i));
            }
            string[] array = str.ToArray();
            //绑定
            CmbAxes_X.Items.AddRange(array);
            CmbAxes_Y.Items.AddRange(array);
            CmbAxes_Z.Items.AddRange(array);
            CmbAxes_C.Items.AddRange(array);
            CmbAxes_D.Items.AddRange(array);
            CmbAxes_B.Items.AddRange(array);
            CmbAxes_A.Items.AddRange(array);
            //设置关联项
            CmbAxes_X.SelectedIndex = GlobalVar.AxisX.LinkIndex;
            CmbAxes_Y.SelectedIndex = GlobalVar.AxisY.LinkIndex;
            CmbAxes_Z.SelectedIndex = GlobalVar.AxisZ.LinkIndex;
            CmbAxes_C.SelectedIndex = GlobalVar.AxisA.LinkIndex;
            CmbAxes_D.SelectedIndex = GlobalVar.AxisB.LinkIndex;
            CmbAxes_B.SelectedIndex = GlobalVar.AxisB.LinkIndex;
            CmbAxes_A.SelectedIndex = GlobalVar.AxisA.LinkIndex;

            #endregion

            this.numericUpDown_GPRunVelLow.Value = GlobalVar.m_GPValue_RunVelLow_move / GlobalVar.ServCMDRate;
            this.numericUpDown_GPRunVelHigh.Value = GlobalVar.m_GPValue_RunVelHigh_move / GlobalVar.ServCMDRate;
            this.numericUpDown_GPRunAcc.Value = GlobalVar.m_GPValue_RunAcc_move / GlobalVar.ServCMDRate;
            this.numericUpDown_GPRunDec.Value = GlobalVar.m_GPValue_RunDec_move / GlobalVar.ServCMDRate;

            this.numericUpDown_RefX.Value=(decimal)GlobalVar.Ref_Point_AxisX;
            this.numericUpDown_RefY.Value=(decimal)GlobalVar.Ref_Point_AxisY;
            this.numericUpDown_LoadLeft.Value=(decimal)GlobalVar.Point_FeedLeft;
            this.numericUpDown_DownRight.Value = (decimal)GlobalVar.Point_DropRight;
            this.numericUpDown_LoadRight.Value=(decimal)GlobalVar.Point_FeedRight ;
            this.numericUpDown_DownLeft.Value=(decimal)GlobalVar.Point_DropLeft;
            this.numericUpDown_ICLoad.Value=(decimal)GlobalVar.Point_ICFeed;
            this.numericUpDown_ICPhoto.Value=(decimal)GlobalVar.Point_ICPhotoPosition;
            this.numericUpDown_PCSLoad.Value=(decimal)GlobalVar.Point_PCSFeed;
            this.numericUpDown_PCSPhoto.Value=(decimal)GlobalVar.Point_PCSPhotoPosition;
            this.numericUpDown_PCSWait.Value=(decimal)GlobalVar.Point_PCSWaitPosition;

            if (Thd_Jog == null || !Thd_Jog.IsAlive)
            {
                JOGWait.Reset();
                Thd_Jog = new Thread(Thd_JOGMOVE);
                Thd_Jog.IsBackground = true;
                Thd_Jog.Name = " JOG运动线程";
                Thd_Jog.Start();
            }
            Refresh_AxisPosition();
            this.WindowRefresh.Tick += new EventHandler(WindowRefresh_Tick);
            AddBtnEvent();

        }


        #region JOG事件
        private void AddBtnEvent()
        {
            this.btn_YPos.MouseCaptureChanged += new EventHandler(btn_JOG_MouseCaptureChanged);
            this.btn_YPos.MouseUp += btn_JOG_MouseUp;
            this.btn_YPos.MouseDown += Btn_YPos_MouseDown;

            this.btn_YNegtive.MouseCaptureChanged += btn_JOG_MouseCaptureChanged;
            this.btn_YNegtive.MouseUp += btn_JOG_MouseUp;
            this.btn_YNegtive.MouseDown += Btn_YNeg_MouseDown;

            this.btn_XPos.MouseCaptureChanged += btn_JOG_MouseCaptureChanged;
            this.btn_XPos.MouseUp += btn_JOG_MouseUp;
            this.btn_XPos.MouseDown += Btn_XPos_MouseDown;

            this.btn_XNeg.MouseCaptureChanged += btn_JOG_MouseCaptureChanged;
            this.btn_XNeg.MouseUp += btn_JOG_MouseUp;
            this.btn_XNeg.MouseDown += Btn_XNeg_MouseDown;

            this.btn_ZUp.MouseCaptureChanged += btn_JOG_MouseCaptureChanged;
            this.btn_ZUp.MouseUp += btn_JOG_MouseUp;
            this.btn_ZUp.MouseDown += Btn_ZUp_MouseDown;

            this.btn_ZDown.MouseCaptureChanged += btn_JOG_MouseCaptureChanged;
            this.btn_ZDown.MouseUp += btn_JOG_MouseUp;
            this.btn_ZDown.MouseDown += Btn_ZDown_MouseDown;

            this.btn_ICPos.MouseCaptureChanged += btn_JOG_MouseCaptureChanged;
            this.btn_ICPos.MouseUp += btn_JOG_MouseUp;
            this.btn_ICPos.MouseDown += Btn_ICPos_MouseDown;

            this.btn_ICNeg.MouseCaptureChanged += btn_JOG_MouseCaptureChanged;
            this.btn_ICNeg.MouseUp += btn_JOG_MouseUp;
            this.btn_ICNeg.MouseDown += Btn_ICNeg_MouseDown;

            this.btn_LoadRight.MouseCaptureChanged += btn_JOG_MouseCaptureChanged;
            this.btn_LoadRight.MouseUp += btn_JOG_MouseUp;
            this.btn_LoadRight.MouseDown += Btn_LoadRight_MouseDown;

            this.btn_LoadLeft.MouseCaptureChanged += btn_JOG_MouseCaptureChanged;
            this.btn_LoadLeft.MouseUp += btn_JOG_MouseUp;
            this.btn_LoadLeft.MouseDown += Btn_LoadLeft_MouseDown;

            this.btn_FPCPos.MouseCaptureChanged += btn_JOG_MouseCaptureChanged;
            this.btn_FPCPos.MouseUp += btn_JOG_MouseUp;
            this.btn_FPCPos.MouseDown += Btn_FPCPos_MouseDown;

            this.btn_FPCNeg.MouseCaptureChanged += btn_JOG_MouseCaptureChanged;
            this.btn_FPCNeg.MouseUp += btn_JOG_MouseUp;
            this.btn_FPCNeg.MouseDown += Btn_FPCNeg_MouseDown;

            this.btn_DownLeft.MouseCaptureChanged += btn_JOG_MouseCaptureChanged;
            this.btn_DownLeft.MouseUp += btn_JOG_MouseUp;
            this.btn_DownLeft.MouseDown += Btn_DownLeft_MouseDown;

            this.btn_DownRight.MouseCaptureChanged += btn_JOG_MouseCaptureChanged;
            this.btn_DownRight.MouseUp += btn_JOG_MouseUp;
            this.btn_DownRight.MouseDown += Btn_DownRight_MouseDown;
        }

        private void btn_JOG_MouseUp(object sender, MouseEventArgs e)
        {
            JOGWait.Reset();//停止JOG
        }

        private void btn_JOG_MouseCaptureChanged(object sender, EventArgs e)
        {
            JOGWait.Reset();//停止JOG
        }

        private void Btn_DownRight_MouseDown(object sender, MouseEventArgs e)
        {
            SelectAxisIndex = GlobalVar.AxisB.LinkIndex;
            StartJOG(true);
        }


        private void Btn_DownLeft_MouseDown(object sender, MouseEventArgs e)
        {
            SelectAxisIndex = GlobalVar.AxisB.LinkIndex;
            StartJOG(false);
        }

        private void Btn_FPCNeg_MouseDown(object sender, MouseEventArgs e)
        {
            SelectAxisIndex = GlobalVar.AxisD.LinkIndex;
            StartJOG(false);
        }

        private void Btn_FPCPos_MouseDown(object sender, MouseEventArgs e)
        {
            SelectAxisIndex = GlobalVar.AxisD.LinkIndex;
            StartJOG(true);
        }

        private void Btn_LoadLeft_MouseDown(object sender, MouseEventArgs e)
        {
            SelectAxisIndex = GlobalVar.AxisA.LinkIndex;
            StartJOG(false);
        }

        private void Btn_LoadRight_MouseDown(object sender, MouseEventArgs e)
        {
            SelectAxisIndex = GlobalVar.AxisA.LinkIndex;
            StartJOG(true);
        }

        private void Btn_ICNeg_MouseDown(object sender, MouseEventArgs e)
        {
            SelectAxisIndex = GlobalVar.AxisC.LinkIndex;
            StartJOG(false);
        }        

        private void Btn_ICPos_MouseDown(object sender, MouseEventArgs e)
        {
            SelectAxisIndex = GlobalVar.AxisC.LinkIndex;
            StartJOG(true);
        }

        private void Btn_ZDown_MouseDown(object sender, MouseEventArgs e)
        {
            SelectAxisIndex = GlobalVar.AxisZ.LinkIndex;
            StartJOG(true);
        }

        private void Btn_ZUp_MouseDown(object sender, MouseEventArgs e)
        {
            SelectAxisIndex = GlobalVar.AxisZ.LinkIndex;
            StartJOG(false);
        }

        private void Btn_XNeg_MouseDown(object sender, MouseEventArgs e)
        {
            SelectAxisIndex = GlobalVar.AxisX.LinkIndex;
            StartJOG(false);
        }

        private void Btn_XPos_MouseDown(object sender, MouseEventArgs e)
        {
            SelectAxisIndex = GlobalVar.AxisX.LinkIndex;
            StartJOG(true);
        }
              
        private void Btn_YNeg_MouseDown(object sender, MouseEventArgs e)
        {
            SelectAxisIndex = GlobalVar.AxisY.LinkIndex;
            StartJOG(false);
        }

        private void Btn_YPos_MouseDown(object sender, MouseEventArgs e)
        {
            SelectAxisIndex = GlobalVar.AxisY.LinkIndex;
            StartJOG(true);
        }

        /// <summary>
        /// 开启线程执行JOG
        /// </summary>
        /// <param name="Positive">是否正方向</param>
        private void StartJOG(bool positive)
        {
            Positive = positive;
            JOGWait.Set();
        }

        #endregion

        private void WindowRefresh_Tick(object sender, EventArgs e)
        {
            switch (this.tabControlNF_Axis.SelectedIndex)
            {
                case 0:
                    GetAxisPosition();
                    break;
            }
        }

        #region 轴数据刷新
        private void Refresh_AxisPosition()
        {
            this.WindowRefresh.Start();
            this.Axis_X.StopRefresh();
            this.Axis_Y.StopRefresh();
            this.Axis_A.StopRefresh();
            this.Axis_C.StopRefresh();
            this.Axis_D.StopRefresh();
            this.Axis_Z.StopRefresh();
            this.Axis_B.StopRefresh();
        }
        private void GetAxisPosition()
        {
            this.lb_Upload.Text = GlobalVar.AxisPCI.Position_A.ToString("#0.000") + " mm";
            this.lb_ICUpload.Text = GlobalVar.AxisPCI.Position_C.ToString("#0.0") + " mm";
            this.lb_IC_X.Text = GlobalVar.AxisPCI.Position_X.ToString("#0.000") + " mm";
            this.lb_IC_Y.Text = GlobalVar.AxisPCI.Position_Y.ToString("#0.000") + " mm";
            this.lb_PCSUpload.Text = GlobalVar.AxisPCI.Position_D.ToString("#0.0") + " mm";
            this.lb_Down.Text = GlobalVar.AxisPCI.Position_B.ToString("#0.000") + " mm";

        }

        /// <summary>
        /// 刷新X轴
        /// </summary>
        private void Refresh_XAxis()
        {
            this.WindowRefresh.Stop();
            this.Axis_X.StartRefresh();
            this.Axis_Y.StopRefresh();
            this.Axis_Z.StopRefresh();
            this.Axis_A.StopRefresh();
            this.Axis_C.StopRefresh();
            this.Axis_D.StopRefresh();
            this.Axis_B.StopRefresh();
            this.Axis_DI.StopRefresh();
            this.Axis_DO.StopRefresh();
        }

        /// <summary>
        /// 刷新Y轴
        /// </summary>
        private void Refersh_YAxis()
        {
            this.WindowRefresh.Stop();
            this.Axis_X.StopRefresh();
            this.Axis_Y.StartRefresh();
            this.Axis_Z.StopRefresh();
            this.Axis_A.StopRefresh();
            this.Axis_C.StopRefresh();
            this.Axis_D.StopRefresh();
            this.Axis_B.StopRefresh();
            this.Axis_DI.StopRefresh();
            this.Axis_DO.StopRefresh();
        }
        /// <summary>
        /// 刷新Z轴
        /// </summary>
        private void Refresh_ZAxis()
        {
            this.WindowRefresh.Stop();
            this.Axis_X.StopRefresh();
            this.Axis_Y.StopRefresh();
            this.Axis_Z.StartRefresh();
            this.Axis_A.StopRefresh();
            this.Axis_C.StopRefresh();
            this.Axis_D.StopRefresh();
            this.Axis_B.StopRefresh();
            this.Axis_DI.StopRefresh();
            this.Axis_DO.StopRefresh();
        }
        /// <summary>
        /// 刷新A轴
        /// </summary>
        private void Refresh_AAxis()
        {
            this.WindowRefresh.Stop();
            this.Axis_X.StopRefresh();
            this.Axis_Y.StopRefresh();
            this.Axis_Z.StopRefresh();
            this.Axis_A.StartRefresh();
            this.Axis_C.StopRefresh();
            this.Axis_D.StopRefresh();
            this.Axis_B.StopRefresh();
            this.Axis_DI.StopRefresh();
            this.Axis_DO.StopRefresh();
        }

        /// <summary>
        /// 刷新C轴
        /// </summary>
        private void Refresh_CAxis()
        {
            this.WindowRefresh.Stop();
            this.Axis_X.StopRefresh();
            this.Axis_Y.StopRefresh();
            this.Axis_Z.StopRefresh();
            this.Axis_A.StopRefresh();
            this.Axis_C.StartRefresh();
            this.Axis_D.StopRefresh();
            this.Axis_B.StopRefresh();
            this.Axis_DI.StopRefresh();
            this.Axis_DO.StopRefresh();
        }

        /// <summary>
        /// 刷新D轴
        /// </summary>
        private void Refresh_DAxis()
        {
            this.WindowRefresh.Stop();
            this.Axis_X.StopRefresh();
            this.Axis_Y.StopRefresh();
            this.Axis_Z.StopRefresh();
            this.Axis_A.StopRefresh();
            this.Axis_C.StopRefresh();
            this.Axis_D.StartRefresh();
            this.Axis_B.StopRefresh();
            this.Axis_DI.StopRefresh();
            this.Axis_DO.StopRefresh();
        }

        /// <summary>
        /// 刷新B轴
        /// </summary>
        private void Refresh_BAxis()
        {
            this.WindowRefresh.Stop();
            this.Axis_X.StopRefresh();
            this.Axis_Y.StopRefresh();
            this.Axis_Z.StopRefresh();
            this.Axis_A.StopRefresh();
            this.Axis_C.StopRefresh();
            this.Axis_D.StopRefresh();
            this.Axis_B.StartRefresh();
            this.Axis_DI.StopRefresh();
            this.Axis_DO.StopRefresh();
        }
        /// <summary>
        /// 刷新DI
        /// </summary>
        private void Refersh_DIAxis()
        {
            this.WindowRefresh.Stop();
            this.Axis_X.StopRefresh();
            this.Axis_Y.StopRefresh();
            this.Axis_Z.StopRefresh();
            this.Axis_A.StopRefresh();
            this.Axis_C.StopRefresh();
            this.Axis_D.StopRefresh();
            this.Axis_B.StopRefresh();
            this.Axis_DI.StartRefresh();
            this.Axis_DO.StopRefresh();
        }

        /// <summary>
        /// 刷新DI
        /// </summary>
        private void Refersh_DOAxis()
        {
            this.WindowRefresh.Stop();
            this.Axis_X.StopRefresh();
            this.Axis_Y.StopRefresh();
            this.Axis_Z.StopRefresh();
            this.Axis_A.StopRefresh();
            this.Axis_C.StopRefresh();
            this.Axis_D.StopRefresh();
            this.Axis_B.StopRefresh();
            this.Axis_DI.StopRefresh();
            this.Axis_DO.StartRefresh();
        }
        #endregion

        private void Thd_JOGMOVE()
        {
            while (Thd_Run)
            {
                try
                {
                    if (!JOGWait.IsSet)
                    {
                        AxisJOG(Positive,false);
                        Thread.Sleep(100);
                        continue;
                    }
                    JOGWait.Wait();
                    if (!Thd_Run) break;
                    AxisJOG(Positive,true);
                    Thread.Sleep(100);
                }catch { }
                finally
                {
                    if (!Thd_Run)//如果线程停止，则停止所有轴运动
                        GlobalVar.AxisPCI.StopAllMove();
                }
            }
        }

        /// <summary>
        /// JOG运动【持续运动】
        /// </summary>
        /// <param name="positive">方向 true：正向</param>
        /// <param name="Run">是否运动</param>
        private void AxisJOG(bool positive, bool Run)
        {
            if (SelectAxisIndex == -1) return;

            if (Run)
            {
                if (GlobalVar.AxisPCI.GetAxisState(SelectAxisIndex) == Advantech.Motion.AxisState.STA_AX_CONTI_MOT) return;

                GlobalVar.AxisPCI.MoveContinous(SelectAxisIndex, Positive);
            }
            else
            {
                if (GlobalVar.AxisPCI.GetAxisState(SelectAxisIndex) == Advantech.Motion.AxisState.STA_AX_READY) return;

                GlobalVar.AxisPCI.StopMove(SelectAxisIndex);
            }
        }




        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        private void AxisForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Axis_X != null) Axis_X.Dispose();
            if (Axis_Y != null) Axis_Y.Dispose();
            if (Axis_A != null) Axis_A.Dispose();
            if (Axis_C != null) Axis_C.Dispose();
            if (Axis_D != null) Axis_D.Dispose();
            if (Axis_B != null) Axis_B.Dispose();
            if (Axis_Z != null) Axis_Z.Dispose();
        }

        private void tabControlNF_Axis_Selected(object sender, TabControlEventArgs e)
        {
            switch (e.TabPageIndex)
            {
                case 0:
                    Refresh_AxisPosition();
                    break;
                case 1:
                    Refresh_AAxis();
                    break;
                case 2:
                    Refresh_CAxis();
                    break;
                case 3:
                    Refresh_DAxis();
                    break;
                case 4:
                    Refresh_BAxis();
                    break;
                case 5:
                    Refresh_XAxis();
                    break;
                case 6:
                    Refersh_YAxis();
                    break;
                case 7:
                    Refresh_ZAxis();
                    break;
                case 9:
                    Refersh_DIAxis();
                    break;
                case 10:
                    Refersh_DOAxis();
                    break;
                default:
                    Refresh_AxisPosition();
                    break;
            }
        }

        private void btn_SetGPRun_Click(object sender, EventArgs e)
        {
            try
            {
                GlobalVar.m_GPValue_RunVelLow_move = (uint)Convert.ToUInt16(this.numericUpDown_GPRunVelLow.Value) * GlobalVar.ServCMDRate;
                GlobalVar.m_GPValue_RunVelHigh_move = (uint)Convert.ToUInt16(this.numericUpDown_GPRunVelHigh.Value) * GlobalVar.ServCMDRate;
                GlobalVar.m_GPValue_RunAcc_move = (uint)Convert.ToUInt16(this.numericUpDown_GPRunAcc.Value) * GlobalVar.ServCMDRate;
                GlobalVar.m_GPValue_RunDec_move = (uint)Convert.ToUInt16(this.numericUpDown_GPRunDec.Value) * GlobalVar.ServCMDRate;

                myfunction.WriteIniString(GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_GPRunVelLow, GlobalVar.m_GPValue_RunVelLow_move.ToString());
                myfunction.WriteIniString(GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_GPRunVelHigh, GlobalVar.m_GPValue_RunVelHigh_move.ToString());
                myfunction.WriteIniString(GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_GPRunAcc, GlobalVar.m_GPValue_RunAcc_move.ToString());
                myfunction.WriteIniString(GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_GPRunDec, GlobalVar.m_GPValue_RunDec_move.ToString());
            }
            catch (Exception ex)
            {
                MsgBox(ex.Message, Color.Red, MessageBoxButtons.OK);
            }
        }

        private void btn_SetLocation_Click(object sender, EventArgs e)
        {
            try
            {
                GlobalVar.Ref_Point_AxisX = Convert.ToDouble(this.numericUpDown_RefX.Value);
                GlobalVar.Ref_Point_AxisY = Convert.ToDouble(this.numericUpDown_RefY.Value);
                GlobalVar.Point_FeedLeft = Convert.ToDouble(this.numericUpDown_LoadLeft.Value);
                GlobalVar.Point_FeedRight = Convert.ToDouble(this.numericUpDown_LoadRight.Value);
                GlobalVar.Point_DropLeft = Convert.ToDouble(this.numericUpDown_DownLeft.Value);
                GlobalVar.Point_DropRight = Convert.ToDouble(this.numericUpDown_DownRight.Value);
                GlobalVar.Point_ICFeed = Convert.ToDouble(this.numericUpDown_ICLoad.Value);
                GlobalVar.Point_ICPhotoPosition = Convert.ToDouble(this.numericUpDown_ICPhoto.Value);
                GlobalVar.Point_PCSFeed= Convert.ToDouble(this.numericUpDown_PCSLoad.Value);
                GlobalVar.Point_PCSPhotoPosition = Convert.ToDouble(this.numericUpDown_PCSPhoto.Value);
                GlobalVar.Point_PCSWaitPosition = Convert.ToDouble(this.numericUpDown_PCSWait.Value);

                myfunction.WriteIniString(GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_RefX, GlobalVar.Ref_Point_AxisX.ToString());
                myfunction.WriteIniString(GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_RefY, GlobalVar.Ref_Point_AxisY.ToString());
                myfunction.WriteIniString(GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_FeedLeft, GlobalVar.Point_FeedLeft.ToString());
                myfunction.WriteIniString(GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_FeedRight, GlobalVar.Point_FeedRight.ToString());
                myfunction.WriteIniString(GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_DropLeft, GlobalVar.Point_DropLeft.ToString());
                myfunction.WriteIniString(GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_ICFeed, GlobalVar.Point_ICFeed.ToString());
                myfunction.WriteIniString(GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_ICPhotoPosition, GlobalVar.Point_ICPhotoPosition.ToString());
                myfunction.WriteIniString(GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_PCSFeed, GlobalVar.Point_PCSFeed.ToString());
                myfunction.WriteIniString(GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_PCSPhotoPosition, GlobalVar.Point_PCSPhotoPosition.ToString());
                myfunction.WriteIniString(GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_PCSWaitPosition, GlobalVar.Point_PCSWaitPosition.ToString());
                

            }
            catch (Exception ex)
            {
                MsgBox(ex.Message, Color.Red, MessageBoxButtons.OK);
            }
        }

        private void btn_Link_Click(object sender, EventArgs e)
        {
            int SelectX, SelectY, SelectZ, SelectA, SelectC, SelectD, SelectB;
            SelectX = this.CmbAxes_X.SelectedIndex;
            SelectY = this.CmbAxes_Y.SelectedIndex;
            SelectZ = this.CmbAxes_Z.SelectedIndex;
            SelectA = this.CmbAxes_A.SelectedIndex;
            SelectC = this.CmbAxes_C.SelectedIndex;
            SelectD = this.CmbAxes_D.SelectedIndex;
            SelectB = this.CmbAxes_B.SelectedIndex;

            int[] array = new int[7] { SelectX, SelectY, SelectZ, SelectA, SelectC, SelectD, SelectB };
            for(int i = 0; i < array.Length; i++)
            {
                int index = array[i];
                for (int j = 0; j < array.Length; j++)
                {
                    if (i == j) continue;
                    if (index == array[j])
                    {
                        MsgBox("板卡不能重复指定！", Color.OrangeRed, MessageBoxButtons.OK);
                        return;
                    }
                }
            }

            GlobalVar.AxisX.LinkIndex = SelectX;
            GlobalVar.AxisY.LinkIndex = SelectY;
            GlobalVar.AxisZ.LinkIndex = SelectZ;
            GlobalVar.AxisA.LinkIndex = SelectA;
            GlobalVar.AxisC.LinkIndex = SelectC;
            GlobalVar.AxisD.LinkIndex = SelectD;
            GlobalVar.AxisB.LinkIndex = SelectB;

            myfunction.WriteIniString(GlobalVar.gl_inisection_SoftWare, GlobalVar.gl_iniKey_LinkX, GlobalVar.AxisX.LinkIndex.ToString());
            myfunction.WriteIniString(GlobalVar.gl_inisection_SoftWare, GlobalVar.gl_iniKey_LinkY, GlobalVar.AxisY.LinkIndex.ToString());
            myfunction.WriteIniString(GlobalVar.gl_inisection_SoftWare, GlobalVar.gl_iniKey_LinkA, GlobalVar.AxisA.LinkIndex.ToString());
            myfunction.WriteIniString(GlobalVar.gl_inisection_SoftWare, GlobalVar.gl_iniKey_LinkB, GlobalVar.AxisA.LinkIndex.ToString());
            myfunction.WriteIniString(GlobalVar.gl_inisection_SoftWare, GlobalVar.gl_iniKey_LinkC, GlobalVar.AxisB.LinkIndex.ToString());
            myfunction.WriteIniString(GlobalVar.gl_inisection_SoftWare, GlobalVar.gl_iniKey_LinkD, GlobalVar.AxisB.LinkIndex.ToString());
            myfunction.WriteIniString(GlobalVar.gl_inisection_SoftWare, GlobalVar.gl_iniKey_LinkZ, GlobalVar.AxisZ.LinkIndex.ToString());

        }

        private void btn_Reset_Click(object sender, EventArgs e)
        {
            Thread thd = new Thread(ResetFunction);
            thd.IsBackground = true;
            thd.Name = "复位线程";
            thd.Start();
        }

        private void ResetFunction()
        {
            try
            {
                GlobalVar.Machine.Reset = true;
                GlobalVar.Machine.Pause = true;
                //复位时置载板到位信号，避免复位后收到载板信号导致流程继续

                GlobalVar.AxisPCI.StopAllMove();//停止所有轴的运动
                for (int i = 0; i < GlobalVar.AxisPCI.AxisCount; i++)
                {
                    GlobalVar.AxisPCI.ClearAxisError(i);//清除所有轴的错误
                }
                GlobalVar.AxisPCI.ResetServ();//开始复位
                Thread.Sleep(200);
                GlobalVar.AxisPCI.WaitAllMoveFinished();//等待轴复位完成，主要用于提示【待机中】
                GlobalVar.Machine.Reset = false;
            }
            catch (Exception ex)
            {
                MsgBox("机台复位异常：" + ex.Message, Color.Red,MessageBoxButtons.OK);
            }
        }
    }
}
