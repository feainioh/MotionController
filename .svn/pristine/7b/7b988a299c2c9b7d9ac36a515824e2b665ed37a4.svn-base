using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OQC_IC_CHECK_System
{
    public partial class Axis_DO : UserControl
    {
        private Timer WindowRefresh = new Timer();
        private IOStatus[] AllLabelDO_Project;
        private IOStatus[] AllLabelDO_Normal;
        private Logs log = Logs.LogsT();
        public Axis_DO()
        {
            InitializeComponent();
            #region 初始化Label数组，用于显示输入输出状态
            AllLabelDO_Project = new IOStatus[] {
                                      IO_DO0_0,IO_DO0_1,IO_DO0_2,IO_DO0_3,
                                      IO_DO1_0,IO_DO1_1,IO_DO1_2,IO_DO1_3,
                                      IO_DO2_0,IO_DO2_1,IO_DO2_2,IO_DO2_3,
                                      IO_DO3_0,IO_DO3_1,IO_DO3_3,IO_DO3_2,
                                      IO_DO4_0,IO_DO4_1,IO_DO4_2,IO_DO4_3,
                                      IO_DO5_0,IO_DO5_1,IO_DO5_2,IO_DO5_3,
                                      IO_DO6_0,IO_DO6_1,IO_DO6_2,IO_DO6_3,
                                      IO_DO7_0,IO_DO7_1,IO_DO7_2,IO_DO7_3,};

            AllLabelDO_Normal = new IOStatus[] {
                                      IO_DO0_0_,IO_DO0_1_,IO_DO0_2_,IO_DO0_3_,
                                      IO_DO1_0_,IO_DO1_1_,IO_DO1_2_,IO_DO1_3_,
                                      IO_DO2_0_,IO_DO2_1_,IO_DO2_2_,IO_DO2_3_,
                                      IO_DO3_0_,IO_DO3_1_,IO_DO3_2_,IO_DO3_3_,
                                      IO_DO4_0_,IO_DO4_1_,IO_DO4_2_,IO_DO4_3_,
                                      IO_DO5_0_,IO_DO5_1_,IO_DO5_2_,IO_DO5_3_,
                                      IO_DO6_0_,IO_DO6_1_,IO_DO6_2_,IO_DO6_3_,
                                      IO_DO7_0_,IO_DO7_1_,IO_DO7_2_,IO_DO7_3_,};
            int len = 4;
            for (int i = 0; i < AllLabelDO_Project.Length; i++)
            {
                if (i >= len * 0 && i < len * 1) AllLabelDO_Project[i].Tag = 0 + (i + 4 - len * 0).ToString();
                if (i >= len * 1 && i < len * 2) AllLabelDO_Project[i].Tag = 1 + (i + 4 - len * 1).ToString();
                if (i >= len * 2 && i < len * 3) AllLabelDO_Project[i].Tag = 2 + (i + 4 - len * 2).ToString();
                if (i >= len * 3 && i < len * 4) AllLabelDO_Project[i].Tag = 3 + (i + 4 - len * 3).ToString();
                if (i >= len * 4 && i < len * 5) AllLabelDO_Project[i].Tag = 4 + (i + 4 - len * 4).ToString();
                if (i >= len * 5 && i < len * 6) AllLabelDO_Project[i].Tag = 5 + (i + 4 - len * 5).ToString();
                if (i >= len * 6 && i < len * 7) AllLabelDO_Project[i].Tag = 6 + (i + 4 - len * 6).ToString();
                if (i >= len * 7 && i < len * 8) AllLabelDO_Project[i].Tag = 7 + (i + 4 - len * 7).ToString();
                AllLabelDO_Project[i].Event_BtnClick += Axis_DO_Event_BtnClick;
            }

            for (int i = 0; i < AllLabelDO_Normal.Length; i++)
            {
                if (i >= len * 0 && i < len * 1) AllLabelDO_Normal[i].Tag = 0 + (i + 4 - len * 0).ToString();
                if (i >= len * 1 && i < len * 2) AllLabelDO_Normal[i].Tag = 1 + (i + 4 - len * 1).ToString();
                if (i >= len * 2 && i < len * 3) AllLabelDO_Normal[i].Tag = 2 + (i + 4 - len * 2).ToString();
                if (i >= len * 3 && i < len * 4) AllLabelDO_Normal[i].Tag = 3 + (i + 4 - len * 3).ToString();
                if (i >= len * 4 && i < len * 5) AllLabelDO_Normal[i].Tag = 4 + (i + 4 - len * 4).ToString();
                if (i >= len * 5 && i < len * 6) AllLabelDO_Normal[i].Tag = 5 + (i + 4 - len * 5).ToString();
                if (i >= len * 6 && i < len * 7) AllLabelDO_Normal[i].Tag = 6 + (i + 4 - len * 6).ToString();
                if (i >= len * 7 && i < len * 8) AllLabelDO_Normal[i].Tag = 7 + (i + 4 - len * 7).ToString();
                AllLabelDO_Normal[i].Event_BtnClick += Axis_DO_Event_BtnClick;
            }
            #endregion
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
            this.WindowRefresh.Stop();
        }

        private void Axis_DO_Event_BtnClick(IOStatus IO)
        {
            try
            {
                string tag = ((IOStatus)IO).Tag.ToString();
                int axis = Convert.ToInt32(tag.Substring(0, 1));
                ushort channel =(ushort)( Convert.ToUInt16(tag.Substring(1, 1)));
                BoardSignalDefinition signal = new BoardSignalDefinition(axis, channel);
                bool result = false;
                if (GlobalVar.AxisPCI.GetSingleDO(signal, ref result))
                    GlobalVar.AxisPCI.SetDO(axis, channel, !result);
            }
            catch (Exception ex)
            {
                log.AddCommLOG("点击DO异常：" + ex.Message);
            }
        }

        private void Axis_DO_Load(object sender, EventArgs e)
        {

            WindowRefresh.Interval = 100;
            WindowRefresh.Tick += WindowRefresh_Tick;
        }

        private void WindowRefresh_Tick(object sender, EventArgs e)
        {
            this.SuspendLayout();
            switch (this.tabControlTF1.SelectedIndex)
            {
                case 0:
                    GetAllDO_Normal();
                    break;
                case 1:
                    GetAllDO_Project();
                    break;
            }
            this.ResumeLayout(false);
        }

        #region 获取所有的输出
        private void GetAllDO_Project()
        {
            bool do0, do1, do2, do3;
            do0 = do1 = do2 = do3 = false;
            for (int i = 0; i < GlobalVar.AxisPCI.AxisCount; i++)
            {
                int startindex = i * 4;
                if (GlobalVar.AxisPCI.GetAxisDO(i, ref do0, ref do1, ref do2, ref do3))
                {
                    AllLabelDO_Project[startindex].StatusImage = do0 ? Properties.Resources.LightGreen_Back : Properties.Resources.LightRed_Back;
                    AllLabelDO_Project[startindex + 1].StatusImage = do1 ? Properties.Resources.LightGreen_Back : Properties.Resources.LightRed_Back;
                    AllLabelDO_Project[startindex + 2].StatusImage = do2 ? Properties.Resources.LightGreen_Back : Properties.Resources.LightRed_Back;
                    AllLabelDO_Project[startindex + 3].StatusImage = do3 ? Properties.Resources.LightGreen_Back : Properties.Resources.LightRed_Back;
                }
                else
                {
                    AllLabelDO_Project[startindex].StatusImage = Properties.Resources.LightGray_Back;
                    AllLabelDO_Project[startindex + 1].StatusImage = Properties.Resources.LightGray_Back;
                    AllLabelDO_Project[startindex + 2].StatusImage = Properties.Resources.LightGray_Back;
                    AllLabelDO_Project[startindex + 3].StatusImage = Properties.Resources.LightGray_Back;
                }
            }
        }

        private void GetAllDO_Normal()
        {
            bool do0, do1, do2, do3;
            do0 = do1 = do2 = do3 = false;
            for (int i = 0; i < GlobalVar.AxisPCI.AxisCount; i++)
            {
                int startindex = i * 4;
                if (GlobalVar.AxisPCI.GetAxisDO(i, ref do0, ref do1, ref do2, ref do3))
                {
                    AllLabelDO_Normal[startindex].StatusImage = do0 ? Properties.Resources.LightGreen_Back : Properties.Resources.LightRed_Back;
                    AllLabelDO_Normal[startindex + 1].StatusImage = do1 ? Properties.Resources.LightGreen_Back : Properties.Resources.LightRed_Back;
                    AllLabelDO_Normal[startindex + 2].StatusImage = do2 ? Properties.Resources.LightGreen_Back : Properties.Resources.LightRed_Back;
                    AllLabelDO_Normal[startindex + 3].StatusImage = do3 ? Properties.Resources.LightGreen_Back : Properties.Resources.LightRed_Back;
                }
                else
                {
                    AllLabelDO_Normal[startindex].StatusImage = Properties.Resources.LightGray_Back;
                    AllLabelDO_Normal[startindex + 1].StatusImage = Properties.Resources.LightGray_Back;
                    AllLabelDO_Normal[startindex + 2].StatusImage = Properties.Resources.LightGray_Back;
                    AllLabelDO_Normal[startindex + 3].StatusImage = Properties.Resources.LightGray_Back;
                }
            }
        }
        #endregion
    }
}
