
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OQC_IC_CHECK_System
{
    public partial class FileOption : Form
    {
        /// <summary>
        /// 根目录
        /// </summary>
        private string RootDIr = string.Empty;
        private string m_SelectedFile = string.Empty;
        /// <summary>
        /// 已经选择的文件
        /// </summary>
        internal string SelectedFile { get { return RootDIr + m_SelectedFile.Trim() + ".ini"; } }

        private MyFunction myfunction = new MyFunction();

        private const int TopHeight = 180;//上面的高度

        public FileOption()
        {
            InitializeComponent();
        }

        private void FileOption_Load(object sender, EventArgs e)
        {
            //SetFormSize(new Size(this.Width, TopHeight));//初次打开时，只显示上面的选项         
            Read();//读取存储的配置，列表
        }

        private void btn_SaveOption_Click(object sender, EventArgs e)
        {
            SetFormSize(new Size(this.Width, TopHeight+this.groupBoxEx_Save.Height));//初次打开时，只显示上面的选项
            this.groupBoxEx_Save.Visible = true;
            this.groupBoxEx_Read.Visible = false;
            this.textBox_SaveName.Focus();
        }

        private void btn_ReadOption_Click(object sender, EventArgs e)
        {
            SetFormSize(new Size(this.Width, TopHeight + this.groupBoxEx_Read.Height));//初次打开时，只显示上面的选项
            this.groupBoxEx_Save.Visible = false;
            this.groupBoxEx_Read.Visible = true;
            Read();//读取存储的配置，列表
        }

        private void SetFormSize(Size size)
        {
            this.Size = this.MaximumSize = this.MinimumSize = size;//设置当前窗体的值
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBox_SaveName.Text))
                throw new Exception("名称为空，无法保存");

            string temp_File = string.Format("{0}.ini", this.textBox_SaveName.Text);

            try
            {
                if (File.Exists(myfunction.GetProductIniPath(temp_File)))
                    if (!MsgBoxPop("文件已经存在，是否替换", Color.DarkSeaGreen, MessageBoxButtons.OKCancel)) return;
                
                File.Copy(myfunction.GetProductIniPath(GlobalVar.Product + ".ini"), myfunction.GetProductIniPath(temp_File), true);

                MsgBoxPop("存储成功", Color.LightBlue, MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                ErrMsgBox(ex.Message, "保存失败");
            }
        }

        /// <summary>
        /// 读取Product文件夹下的所有ini文件
        /// </summary>
        private void Read()
        {
            try
            {
                string[] files = Directory.GetFiles("Product");
                List<string> txt_list = new List<string>();
                foreach (string str in files)
                {
                    if (str.ToUpper().Contains(".ini")) txt_list.Add(str);
                }

                if (txt_list.Count == 0) return;

                RefreshListView(txt_list.ToArray());
            }
            catch (Exception ex)
            {
                ErrMsgBox(ex.Message, "读取文件异常");
            }
        }

        /// <summary>
        /// 更换数据显示
        /// </summary>
        /// <param name="files"></param>
        private void RefreshListView(string[] files)
        {
            int Length = this.listView_Files.Width;

            this.listView_Files.Clear();
            this.listView_Files.BeginUpdate();
            foreach (string str in files)
            {
                string FileName = Path.GetFileNameWithoutExtension(str);
                listView_Files.Items.Add(new ListViewItem(FileName.PadRight(Length)));
                this.RootDIr = Directory.GetDirectoryRoot(str);
            }
            this.listView_Files.EndUpdate();
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.listView_Files.SelectedItems.Count == 0) return;
                GlobalVar.Product = this.listView_Files.SelectedItems[0].Text.Trim();
                this.m_SelectedFile = GlobalVar.Product + ".ini";

                myfunction.WriteIniString(GlobalVar.gl_inisection_Product, GlobalVar.gl_iniKey_Product, GlobalVar.Product);
                //myfunction.LoadCADFile(GlobalVar.Product);

                MsgBoxPop("读取成功", Color.LightBlue, MessageBoxButtons.OK);
                this.DialogResult = DialogResult.OK;
            }
            catch(Exception ex)
            {
                ErrMsgBox(ex.Message,"读取失败");
            }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btn_Delete_Click(object sender, EventArgs e)
        {
            try
            {
                string delefile = this.listView_Files.SelectedItems[0].Text.Trim() + ".ini";
                File.Delete(myfunction.GetProductIniPath(delefile));
            }
            catch(Exception ex)
            {
                ErrMsgBox(ex.Message,"删除失败");
            }
            finally
            {
                Read();
            }
        }

        /// <summary>
        /// 弹框【OK或者Cancel】
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="backcolor">背景色</param>
        /// <returns></returns>
        private bool MsgBoxPop(string text, Color backcolor, MessageBoxButtons btn)
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

        /// <summary>
        /// 异常弹框
        /// </summary>
        /// <param name="errmsg">异常内容</param>
        /// <param name="title">异常标题</param>
        /// <returns></returns>
        private DialogResult ErrMsgBox(string errmsg, string title = "异常")
        {
            MsgBox box = new MsgBox();
            box.Title = title;
            box.ShowText = errmsg;
            return box.ShowDialog();
        }

        private void lb_Close_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void lb_Close_MouseDown(object sender, MouseEventArgs e)
        {
            lb_Close.BackColor = Color.LightYellow;
            lb_Close.Image = Properties.Resources.close_2;
        }

        private void lb_Close_MouseUp(object sender, MouseEventArgs e)
        {
            lb_Close.BackColor = Color.Transparent;
            lb_Close.Image = Properties.Resources.close_1;
        }

        private void lb_Close_MouseEnter(object sender, EventArgs e)
        {
            lb_Close.BackColor = Color.LightYellow;
        }

        private void lb_Close_MouseLeave(object sender, EventArgs e)
        {
            lb_Close.BackColor = Color.Transparent;
        }
    }
}
