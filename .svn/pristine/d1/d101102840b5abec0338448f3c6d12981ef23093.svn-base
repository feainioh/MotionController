using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Advantech.Motion;

namespace OQC_IC_CHECK_System
{
    public partial class AxisServerOn : AxisInterface
    {
        public AxisServerOn()
        {
            InitializeComponent();
        }

        private void AxisServerOn_Load(object sender, EventArgs e)
        {
            this.WindowRefresh.Tick += WindowRefresh_Tick;
        }

        private void WindowRefresh_Tick(object sender, EventArgs e)
        {
            UInt32 iostatus = new UInt32();
            if (GlobalVar.AxisPCI.GetIOState(Index, ref iostatus)) GetMotionIOStatus(iostatus);
            else IOGray();
        }
        private void GetMotionIOStatus(uint IOStatus)
        {
            if ((IOStatus & (uint)Ax_Motion_IO.AX_MOTION_IO_SVON) > 0)//-EL
            {
                label_Servo.Image = Properties.Resources.LightGreen;
            }
           else label_Servo.Image = Properties.Resources.LightRed;
        }
        //设置IO灰色
        private void IOGray()
        {
            label_Servo.Image = Properties.Resources.LightGray;
        }
    }
}
