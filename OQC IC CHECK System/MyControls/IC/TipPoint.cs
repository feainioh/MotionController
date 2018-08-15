using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace OQC_IC_CHECK_System
{
    public partial class TipPoint : UserControl
    {
        #region 控件属性
        private string m_TipPointName;
        /// <summary>
        /// 点名称
        /// </summary>
        public string TipPointName
        {
            set
            {
                m_TipPointName = value;
                this.lb_Index.Text = m_TipPointName;
            }
            get { return m_TipPointName; }
        }

        private string m_pointX;
        /// <summary>
        /// 点X坐标
        /// </summary>
        public string Point_X
        {
            get { return m_pointX; }
            set
            {
                m_pointX = value;
                this.label_X.Text = m_pointX;
            }
        }
        private string m_pointY;
        /// <summary>
        /// 点Y坐标
        /// </summary>
        public string Point_Y
        {
            get { return m_pointY; }
            set
            {
                m_pointY = value;
                this.label_Y.Text = m_pointY;
            }
        }

        #endregion
        public TipPoint()
        {
            InitializeComponent();
        }

        private void btn_Motion_Click(object sender, EventArgs e)
        {
            IntPtr ptr_pos_X = Marshal.StringToHGlobalAnsi(Point_X);
            IntPtr ptr_pos_Y = Marshal.StringToHGlobalAnsi(Point_Y);
            MyFunction.SendMessage(GlobalVar.gl_IntPtr_MainWindow, GlobalVar.WM_FixedMotion, ptr_pos_X, ptr_pos_Y);
        }
    }
}
