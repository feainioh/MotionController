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
    public partial class Either : UserControl
    {
        #region 属性窗口
        /// <summary>
        /// 标题
        /// </summary>
        [Browsable(true),Description("标题栏的字符串")]
        public string Title
        {
            get { return this.label_Title.Text; }
            set 
            {
                this.label_Title.Text = value;
            }
        }

        /// <summary>
        /// 左边按键的名称
        /// </summary>
        [Browsable(true), Description("左边按键名称")]
        public string BtnLeftText
        {
            get { return this.btn_left.Text; }
            set
            {
                this.btn_left.Text = value;
            }
        }

        /// <summary>
        /// 右边按键的名称
        /// </summary>
        [Browsable(true), Description("右边按键名称")]
        public string BtnRightText
        {
            get { return this.btn_right.Text; }
            set
            {
                this.btn_right.Text = value;
            }
        }

        private bool m_LeftPress = true;
        /// <summary>
        /// 左边是否按下
        /// </summary>
        [Browsable(true),Description("左边的按键是否按下")]
        public bool LeftPress
        {
            get { return m_LeftPress; }
            set 
            {
                this.m_LeftPress = value;
                ChangeBackColor(this.m_LeftPress);
            }
        }

        public delegate void dele_LeftRight(LeftRightSide lr);
        /// <summary>
        /// 按键
        /// </summary>
        [Description("二选一按钮的点击事件")]
        public event dele_LeftRight Event_BtnClick;
        #endregion

        /// <summary>
        /// 二选一窗口
        /// </summary>
        public Either()
        {
            InitializeComponent();
        }

        private void Either_Load(object sender, EventArgs e)
        {

        }

        private void btn_left_Click(object sender, EventArgs e)
        {
            ChangeBackColor(true);

            if (Event_BtnClick != null) Event_BtnClick(LeftRightSide.Left);
        }

        private void btn_right_Click(object sender, EventArgs e)
        {
            ChangeBackColor(false);

            if (Event_BtnClick != null) Event_BtnClick(LeftRightSide.Right);
        }

        /// <summary>
        /// 改变按钮的背景色
        /// </summary>
        /// <param name="left">是否为左边高亮</param>
        private void ChangeBackColor(bool left)
        {
            if (left)
            {
                this.btn_left.BackColor = Color.LimeGreen;
                this.btn_right.BackColor = SystemColors.ControlLightLight;
            }
            else
            {
                this.btn_left.BackColor = SystemColors.ControlLightLight;
                this.btn_right.BackColor = Color.LimeGreen;
            }
        }
    }
}
