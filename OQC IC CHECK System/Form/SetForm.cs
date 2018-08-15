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
    public partial class SetForm : Frame
    {
        public SetForm()
        {
            InitializeComponent();
        }

        private void btn_ClearAlarmRecord_Click(object sender, EventArgs e)
        {
            if (!MsgBox("确定清除报警记录吗？该操作不可恢复", Color.SeaShell, MessageBoxButtons.OKCancel)) return;

            GlobalVar.Machine.AlarmRecord.Clear();
            GlobalVar.Machine.ForceAlarmUpdate();
            (new MyFunction()).WriteIniString(GlobalVar.gl_inisection_Sheet,GlobalVar.gl_iniKey_BoardCount,"0");
            (new MyFunction()).WriteIniString(GlobalVar.gl_inisection_Sheet,GlobalVar.gl_iniKey_ICFailCount,"0");
            GlobalVar.BoardCount = 0;
            GlobalVar.ICFailCount = 0;
        }


        /// <summary>
        /// 弹框【确认或者取消】
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="backcolor">背景色</param>
        /// <returns></returns>
        protected bool MsgBox(string text, Color backcolor, MessageBoxButtons btn)
        {
            using (MsgBox box = new MsgBox(btn))
            {
                box.Title = "提示";
                box.ShowText = text;
                box.SetBackColor = backcolor;
                if (box.ShowDialog() == DialogResult.OK) return true;
                else return false;
            }
        }

        private void SetForm_Load(object sender, EventArgs e)
        {
            listView_ErrList.Clear();
            this.WindowRefresh.Tick += WindowRefresh_Tick;
            this.WindowRefresh.Start();
        }

        private void WindowRefresh_Tick(object sender, EventArgs e)
        {
            listView_ErrList.Clear();
            this.listView_ErrList.BeginUpdate();
            int index = 0;
            this.listView_ErrList.Columns.Add("序号", 80, HorizontalAlignment.Center);
            this.listView_ErrList.Columns.Add("异常信息", 440, HorizontalAlignment.Center);
            this.listView_ErrList.Columns.Add("次数", 80, HorizontalAlignment.Center);
            foreach (KeyValuePair<string,int> kv in GlobalVar.Machine.AlarmRecord)
            {
                index++;
                ListViewItem lvi = new ListViewItem();
                lvi.Text = index.ToString();
                lvi.SubItems.Add(kv.Key);
                lvi.SubItems.Add(kv.Value.ToString());
                this.listView_ErrList.Items.Add(lvi);
            }
            this.listView_ErrList.EndUpdate();
        }

        private void SetForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.WindowRefresh.Stop();
        }
    }
}
