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
    public partial class IOStatus : UserControl
    {
        #region 属性窗口
        /// <summary>
        /// 名称
        /// </summary>
        [Browsable(true), Description("名称")]
        public string Title
        {
            get { return this.label_Title.Text; }
            set
            {
                this.label_Title.Text = value;
            }
        }

        private bool m_BtnPress = true;
        /// <summary>
        /// 按钮是否按下
        /// </summary>
        [Browsable(true), Description("按钮是否按下")]
        public bool BtnPress
        {
            get { return m_BtnPress; }
            set
            {
                this.m_BtnPress = value;
            }
        }

        private Bitmap m_StatusImage = Properties.Resources.LightGray_Back;
        /// <summary>
        /// IO状态
        /// </summary>
        [Browsable(true), Description("显示状态")]
        public Bitmap StatusImage
        {
            get { return m_StatusImage; }
            set
            {
                this.m_StatusImage = value;
                this.BackgroundImage = value;
            }
        }

        public delegate void dele_click(IOStatus IO);
        /// <summary>
        /// 按键
        /// </summary>
        [Description("按钮的点击事件")]
        public event dele_click Event_BtnClick;
        #endregion

        /// <summary>
        /// IO指示灯窗口
        /// </summary>
        public IOStatus()
        {
            InitializeComponent();

            this.MaximumSize = this.MinimumSize = this.Size;
        }

        private void IOStatus_Load(object sender, EventArgs e)
        {

        }

        private void label_Title_Click(object sender, EventArgs e)
        {
            ClickEvent();
        }

        private void IOStatus_Click(object sender, EventArgs e)
        {
            ClickEvent();
        }

        private void ClickEvent()
        {
            if (this.Event_BtnClick != null) this.Event_BtnClick(this);
        }
    }
}
