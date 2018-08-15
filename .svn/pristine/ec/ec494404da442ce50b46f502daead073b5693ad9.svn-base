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
    public partial class Axis_DI : UserControl
    {
        private Timer WindowRefresh = new Timer();
        private IOStatus[] AllLabelDI_Project;
        private IOStatus[] AllLabelDI_Normal;
        public Axis_DI()
        {
            InitializeComponent();
            #region 
            AllLabelDI_Project = new IOStatus[] {
                                      IO_DI0_0,IO_DI0_1,IO_DI0_2,IO_DI0_3,
                                      IO_DI1_0,IO_DI1_1,IO_DI1_2,IO_DI1_3,
                                      IO_DI2_0,IO_DI2_1,IO_DI2_2,IO_DI2_3,
                                      IO_DI3_0,IO_DI3_1,IO_DI3_2,IO_DI3_3,
                                      IO_DI4_0,IO_DI4_1,IO_DI4_2,IO_DI4_3,
                                      IO_DI5_0,IO_DI5_1,IO_DI5_2,IO_DI5_3,
                                      IO_DI6_0,IO_DI6_1,IO_DI6_2,IO_DI6_3,
                                      IO_DI7_0,IO_DI7_1,IO_DI7_2,IO_DI7_3,};

            AllLabelDI_Normal = new IOStatus[] {
                                      IO_DI0_0_,IO_DI0_1_,IO_DI0_2_,IO_DI0_3_,
                                      IO_DI1_0_,IO_DI1_1_,IO_DI1_2_,IO_DI1_3_,
                                      IO_DI2_0_,IO_DI2_1_,IO_DI2_2_,IO_DI2_3_,
                                      IO_DI3_0_,IO_DI3_1_,IO_DI3_2_,IO_DI3_3_,
                                      IO_DI4_0_,IO_DI4_1_,IO_DI4_2_,IO_DI4_3_,
                                      IO_DI5_0_,IO_DI5_1_,IO_DI5_2_,IO_DI5_3_,
                                      IO_DI6_0_,IO_DI6_1_,IO_DI6_2_,IO_DI6_3_,
                                      IO_DI7_0_,IO_DI7_1_,IO_DI7_2_,IO_DI7_3_,};
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

        private void Axis_DI_Load(object sender, EventArgs e)
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
                    GetAllDI_Normal();
                    break;
                case 1:
                    GetAllDI_Project();
                    break;
            }
            this.ResumeLayout(false);
        }

        #region 获取所有的输入
        private void GetAllDI_Project()
        {
            bool di0, di1, di2, di3;
            di0 = di1 = di2 = di3 = false;
            for (int i = 0; i < GlobalVar.AxisPCI.AxisCount; i++)
            {
                int startindex = i * 4;
                if (GlobalVar.AxisPCI.GetDI(i, ref di0, ref di1, ref di2, ref di3))
                {
                    AllLabelDI_Project[startindex].StatusImage = di0 ? Properties.Resources.LightGreen_Back : Properties.Resources.LightRed_Back;
                    AllLabelDI_Project[startindex + 1].StatusImage = di1 ? Properties.Resources.LightGreen_Back : Properties.Resources.LightRed_Back;
                    AllLabelDI_Project[startindex + 2].StatusImage = di2 ? Properties.Resources.LightGreen_Back : Properties.Resources.LightRed_Back;
                    AllLabelDI_Project[startindex + 3].StatusImage = di3 ? Properties.Resources.LightGreen_Back : Properties.Resources.LightRed_Back;
                }
                else
                {
                    AllLabelDI_Project[startindex].StatusImage = Properties.Resources.LightGray_Back;
                    AllLabelDI_Project[startindex + 1].StatusImage = Properties.Resources.LightGray_Back;
                    AllLabelDI_Project[startindex + 2].StatusImage = Properties.Resources.LightGray_Back;
                    AllLabelDI_Project[startindex + 3].StatusImage = Properties.Resources.LightGray_Back;
                }
            }
        }

        private void GetAllDI_Normal()
        {
            bool di0, di1, di2, di3;
            di0 = di1 = di2 = di3 = false;
            for (int i = 0; i < GlobalVar.AxisPCI.AxisCount; i++)
            {
                int startindex = i * 4;
                if (GlobalVar.AxisPCI.GetDI(i, ref di0, ref di1, ref di2, ref di3))
                {
                    AllLabelDI_Normal[startindex].StatusImage = di0 ? Properties.Resources.LightGreen_Back : Properties.Resources.LightRed_Back;
                    AllLabelDI_Normal[startindex + 1].StatusImage = di1 ? Properties.Resources.LightGreen_Back : Properties.Resources.LightRed_Back;
                    AllLabelDI_Normal[startindex + 2].StatusImage = di2 ? Properties.Resources.LightGreen_Back : Properties.Resources.LightRed_Back;
                    AllLabelDI_Normal[startindex + 3].StatusImage = di3 ? Properties.Resources.LightGreen_Back : Properties.Resources.LightRed_Back;
                }
                else
                {
                    AllLabelDI_Normal[startindex].StatusImage = Properties.Resources.LightGray_Back;
                    AllLabelDI_Normal[startindex + 1].StatusImage = Properties.Resources.LightGray_Back;
                    AllLabelDI_Normal[startindex + 2].StatusImage = Properties.Resources.LightGray_Back;
                    AllLabelDI_Normal[startindex + 3].StatusImage = Properties.Resources.LightGray_Back;
                }
            }
        }
        #endregion
    }
}
