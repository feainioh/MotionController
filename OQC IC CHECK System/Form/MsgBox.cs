using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OQC_IC_CHECK_System
{
    public partial class MsgBox : Form
    {/// <summary>
     /// 获取窗体是否已经显示
     /// </summary>
        internal static bool IsShow { get { return m_IsShow; } }
        /// <summary>
        /// 是否已经显示【已经显示，则忽略后续的】
        /// </summary>
        private static bool m_IsShow = false;
        /// <summary>
        /// 是否取消显示
        /// </summary>
        private bool Cancel = false;

        /// <summary>
        /// 显示的标题
        /// </summary>
        internal string Title
        {
            set { this.label_Title.Text = value; }
        }

        /// <summary>
        /// 显示的字符串
        /// </summary>
        internal string ShowText
        {
            set { this.label_Content.Text = value; }
        }

        internal Color SetBackColor
        {
            set { this.groupBoxEx1.BackColor = value; }
        }

        /// <summary>
        /// 检查是否已经显示，已经显示则返回取消
        /// </summary>
        private void CheckIsShow()
        {
            if (m_IsShow) this.Cancel = true; ;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ConfirmBtnText">确定按钮字符串</param>
        /// <param name="btn"></param>
        public MsgBox(string ConfirmBtnText)
        {
            InitializeComponent();

            this.btn_Confirm.Text = ConfirmBtnText;
            this.btn_Confirm.Location = new Point((this.Width - this.btn_Confirm.Width) / 2, this.btn_Confirm.Location.Y);
            this.btn_Cancel.Visible = false;

            CheckIsShow();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="BtnText">确定按钮字符串</param>
        /// <param name="CancelBtnText">取消按钮字符串</param>
        public MsgBox(string ConfirmBtnText, string CancelBtnText)
        {
            InitializeComponent();

            this.btn_Confirm.Text = ConfirmBtnText;
            this.btn_Cancel.Text = CancelBtnText;

            CheckIsShow();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="btn">按钮</param>
        public MsgBox(MessageBoxButtons btn = MessageBoxButtons.OK)
        {
            InitializeComponent();

            switch (btn)
            {
                case MessageBoxButtons.OK:
                    this.btn_Confirm.Text = "确认";
                    this.btn_Confirm.Location = new Point((this.Width - this.btn_Confirm.Width) / 2, this.btn_Confirm.Location.Y);
                    this.btn_Cancel.Visible = false;
                    break;
                case MessageBoxButtons.YesNo:

                    break;
            }

            CheckIsShow();
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="define">信号定义</param>
        /// <param name="value">判断的值</param>
        /// <param name="btn">按钮</param>
        internal MsgBox(BoardSignalDefinition define, bool value, MessageBoxButtons btn = MessageBoxButtons.OK)
        {
            InitializeComponent();

            this.BoardSignal = define;
            this.BoadrSignalValue = value;
            switch (btn)
            {
                case MessageBoxButtons.OK:
                    this.btn_Confirm.Text = "确认";
                    this.btn_Confirm.Location = new Point((this.Width - this.btn_Confirm.Width) / 2, this.btn_Confirm.Location.Y);
                    this.btn_Cancel.Visible = false;
                    break;
                case MessageBoxButtons.YesNo:

                    break;
            }

            CheckIsShow();
        }

        private void ConfirmClick()
        {
            try
            {
                if (this.BoardSignal != null)
                {
                    bool di0, di1, di2, di3;
                    di0 = di1 = di2 = di3 = false;
                    if (GlobalVar.AxisPCI.GetDI(this.BoardSignal.AxisNum, ref di0, ref di1, ref di2, ref di3))
                    {
                        bool DI = false;
                        switch (BoardSignal.Channel)
                        {
                            case 0:
                                DI = di0;
                                break;
                            case 1:
                                DI = di1;
                                break;
                            case 2:
                                DI = di2;
                                break;
                            case 3:
                                DI = di3;
                                break;
                        }
                        if (DI != this.BoadrSignalValue) return;
                    }
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex){ Logs.LogsT().AddCommLOG(ex.Message); }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
        private readonly BoardSignalDefinition BoardSignal;//需要判断的信号
        private readonly bool BoadrSignalValue;//需要判断信号的值
        private void btn_Confirm_Click(object sender, EventArgs e)
        {
            if (this.BoardSignal != null)
            {
                bool di0, di1, di2, di3;
                di0 = di1 = di2 = di3 = false;
                if (GlobalVar.AxisPCI.GetDI(this.BoardSignal.AxisNum, ref di0, ref di1, ref di2, ref di3))
                {
                    bool DI = false;
                    switch (BoardSignal.Channel)
                    {
                        case 0:
                            DI = di0;
                            break;
                        case 1:
                            DI = di1;
                            break;
                        case 2:
                            DI = di2;
                            break;
                        case 3:
                            DI = di3;
                            break;
                    }
                    if (DI != this.BoadrSignalValue) return;
                }
            }

            this.DialogResult = DialogResult.OK;
        }

        private void MsgBox_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!Cancel) m_IsShow = false;
        }

        private void MsgBox_Shown(object sender, EventArgs e)
        {
            if (Cancel) this.DialogResult = DialogResult.Cancel;
        }

        private void MsgBox_Load(object sender, EventArgs e)
        {
            m_IsShow = true;
            GlobalVar.gl_IntPtr_MsgWindow = this.Handle;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case GlobalVar.WM_MsgAlarmDisable:
                    ConfirmClick();
                    break;
            }
            base.WndProc(ref m);
        }
    }
}
