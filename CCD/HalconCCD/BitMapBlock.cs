using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace HalconCCD
{
    public partial class BitMapBlock : UserControl
    {
        //图片位置
        private string m_ImageIndex = string.Empty;
        [Category("自定义"), Browsable(true), Description("图片位置")]
        public string ImageIndex
        {
            get { return m_ImageIndex; }
            set
            {
                this.m_ImageIndex = value;
                //this.label_Title.Text = this.m_CameraName;
            }
        }
        MyFunction myfunction = new MyFunction();

        public BitMapBlock()
        {
            InitializeComponent();

        }

        private void pb_image_Click(object sender, EventArgs e)
        {
            if (this.pb_image.Image != null)
            {
                if (ImageIndex != "")
                {
                    string filename = ImageIndex + ".bmp";
                    string filePath = GlobalVar.TestData+filename;
                    Process.Start(filePath);
                }
            }
        }

        private void BitMapBlock_Load(object sender, EventArgs e)
        {
            label_Index.Text = m_ImageIndex;
        }

        public void LoadImage()
        {
            try
            {
                int index = int.Parse(ImageIndex);//图片序号
                if (GlobalVar.CCD_Image.Values != null && GlobalVar.CCD_Result.Values != null)
                    this.BeginInvoke(new Action(() =>
                    {
                        this.pb_image.Image = GlobalVar.CCD_Image[index];//加载图片
                                if (GlobalVar.CCD_Result[index] == 1)
                        {
                            this.pb_image.Borders.TopColor = Color.Green;
                            this.pb_image.Borders.RightColor = Color.Green;
                            this.pb_image.Borders.LeftColor = Color.Green;
                            this.pb_image.Borders.BottomColor = Color.Green;
                        }
                        else if (GlobalVar.CCD_Result[index] == 2)
                        {
                            this.pb_image.Borders.TopColor = Color.Red;
                            this.pb_image.Borders.RightColor = Color.Red;
                            this.pb_image.Borders.LeftColor = Color.Red;
                            this.pb_image.Borders.BottomColor = Color.Red;
                        }
                    }));


            }
            catch { }

        }

        internal void Reset()
        {
            this.pb_image.Image = null;
            this.pb_image.Borders.TopColor = Color.Moccasin;
            this.pb_image.Borders.RightColor = Color.Moccasin;
            this.pb_image.Borders.LeftColor = Color.Moccasin;
            this.pb_image.Borders.BottomColor = Color.Moccasin;
        }
    }
}
