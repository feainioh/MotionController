using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using PylonLiveView;
using PylonLiveView;

namespace PylonLiveView
{
    public partial class OBJ_TipPoint: UserControl 
    {
        public int m_old_SeqValue = 0;//記錄TipPoint順序在被修改前的值(用於排序調整)
        MyFunctions myfunc = new MyFunctions();
        public OBJ_TipPoint()
        {
            InitializeComponent();
            comboBox_AngleSymbol.SelectedIndex = 0;
            comboBox_ZSymbol.SelectedIndex = 0;
            comboBox_angle.SelectedIndex = 0;
            comboBox_lineSequence.SelectedIndex = 0;
        }

        //线序  --0：表示正序，1表示反序
        public string _LineSequence
        {
            get 
            {
                return (comboBox_lineSequence.SelectedIndex == 0) ? "0" : "1";
            }
            set
            {
                try
                {
                    comboBox_lineSequence.SelectedIndex = Convert.ToInt32(value);
                }
                catch (System.Exception ex)
                {
                    comboBox_lineSequence.SelectedIndex = 0;
                }
            }
        }

        public string _tipIndex
        {
            get
            {
                try
                {
                    return groupBox1.Text.Substring(3);
                }
                catch { return ""; }
            }
            set
            {
                groupBox1.Text += value;
            }
        }

        public string _Pos_X
        {
            get 
            {
                return this.label_posX.Text.Replace("X:", "").Trim();
            }
            set
            {
                this.label_posX.Text = "X:" + value;
            }
        }

        public string _Pos_Y
        {
            get
            {
                return this.label_posY.Text.Replace("Y:", "").Trim();
            }
            set
            {
                this.label_posY.Text = "Y:" + value;
            }
        }

        //public string _Pos_Z
        //{
        //    get
        //    {
        //        return this.label_posZ.Text.Replace("Pos_Z:", "").Trim();
        //    }
        //    set
        //    {
        //        this.label_posZ.Text = "Pos_Z: " + value;
        //    }
        //}

        public string _Symbol_angle
        {
            get 
            {
                return (comboBox_AngleSymbol.SelectedIndex == 0)? "0":"1";
            }
        }

        public string _AngleValue
        {
            get 
            {
                //return numericUpDown_Angle.Value.ToString("000000").Trim();
                return Convert.ToInt32(comboBox_angle.Text).ToString("000000").Trim();
            }
            set 
            {
                comboBox_angle.Text = value;
            }
        }

        public string _Symbol_Z
        {
            get
            {
                return (comboBox_ZSymbol.SelectedIndex == 0) ? "0" : "1";
            }
        }

        public string _Value_Z
        {
            get
            {
                return (double.Parse(textBox_ZValue.Text.Trim()) * GlobalVar.gl_PixelDistance).ToString("000000");
            }
        }

        //因為會修改old_seqValue，所以只允許在外部程序修改，內部直接修改numbericbox
        public string _TPSequence
        {
            get { return numericUpDown_Seque.Value.ToString(); }
            set
            {
                m_old_SeqValue = Convert.ToInt32(value);
                numericUpDown_Seque.Value = Convert.ToInt32(value);
            }
        }

        private void textBox_ZValue_Leave(object sender, EventArgs e)
        {
            Regex _Reg = new Regex(@"^\d+(.\d+)?$");//正整數

            if (!_Reg.IsMatch(textBox_ZValue.Text.ToString()))
            {
                MessageBox.Show("字符串輸入不符規則，請重新輸入(非字母數值)！","提示");
                textBox_ZValue.Text = "0.00";
            }

            if (double.Parse(textBox_ZValue.Text.Trim()) > 1000)
            {
                MessageBox.Show("數值輸入超過最大範圍，請重新設定！", "提示");
                textBox_ZValue.Text = "0.00";
            }
        }


        private void button_accept_Click(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToInt32(numericUpDown_Seque.Value) == m_old_SeqValue)
                { 
                    return;
                }
                int num_new = Convert.ToInt32(numericUpDown_Seque.Value);
                //一定要追溯到obj_dwgdirect祖父类 ==>发送信息

                //ATFunction.SendMessage(Global.GlobalVar.gl_IntPtr_ObjDWGDirect,
                //    Global.GlobalVar.WM_SendTPSeqChanged,
                //    (IntPtr)m_old_SeqValue,     //修改前的序号
                //    (IntPtr)num_new);    //修改后的序号   
            }
            catch { }
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            numericUpDown_Seque.Value = m_old_SeqValue;
        }

        private void comboBox_angle_SelectedIndexChanged(object sender, EventArgs e)
        {
            //try
            //{
            //    for (int i = 0; i < GlobalVar.gl_List_PointInfo.Count; i++)
            //    {
            //        if ((GlobalVar.gl_List_PointInfo[i] as SPoint).Point_name.ToUpper() == _tipIndex.ToUpper())
            //        {
            //            (GlobalVar.gl_List_PointInfo[i] as SPoint).Angle_deflection = double.Parse(comboBox_angle.Text.Trim());
            //        }
            //    }
            //}
            //catch { }
        }

        private void numericUpDown_Seque_ValueChanged(object sender, EventArgs e)
        {
            //if (numericUpDown_Seque.Value > GlobalVar.gl_List_PointInfo.Count)
            //{
            //    numericUpDown_Seque.Value = GlobalVar.gl_List_PointInfo.Count;
            //}
        }

        private void comboBox_lineSequence_SelectedIndexChanged(object sender, EventArgs e)
        {
            //try
            //{
            //    for (int i = 0; i < GlobalVar.gl_List_PointInfo.Count; i++)
            //    {
            //        if ((GlobalVar.gl_List_PointInfo[i] as SPoint).Point_name.ToUpper() == _tipIndex.ToUpper())
            //        {
            //            (GlobalVar.gl_List_PointInfo[i] as SPoint).Line_sequence = comboBox_lineSequence.SelectedIndex;
            //        }
            //    }
            //}
            //catch { }
        }

        private void button_move_Click(object sender, EventArgs e)
        {
            IntPtr ptr_pos_X = Marshal.StringToHGlobalAnsi(_Pos_X);
            IntPtr ptr_pos_Y = Marshal.StringToHGlobalAnsi(_Pos_Y);
            MyFunctions.SendMessage(GlobalVar.gl_IntPtr_MainWindow, GlobalVar.WM_FixedMotion, ptr_pos_X, ptr_pos_Y);
        }
    }
}