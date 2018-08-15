using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace HalconCCD
{
    public partial class MainCCDForm : Form
    {
        public MainCCDForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //OpenFileDialog openFile = new OpenFileDialog();
            //openFile.Title = "图片";
            //openFile.Filter = "所有文件(*.*)|*.*";
            ccdShow1.StartWork(16);
            FolderBrowserDialog openFile = new FolderBrowserDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                string names = openFile.SelectedPath;
                string[] files = Directory.GetFiles(names);
                int i = 0;
                foreach (string name in files)
                {
                    Stopwatch time = new Stopwatch();
                    time.Start();
                    i++;
                    Image image = Image.FromFile(name);
                    Bitmap bitmap = new Bitmap(image);
                    HObject image1 = ccdShow1.HImageConvertFromBitmap(bitmap);
                    ccdShow1.DirectShowImage(image1);
                    string barcode;
                    ccdShow1.HandleBarcode(image1, i,"E:/IC",out barcode);
                    image.Dispose();
                    bitmap.Dispose();
                    image1.Dispose();
                    label_Time.Text = time.Elapsed.TotalMilliseconds.ToString();
                    label_Time.Update();
                    Thread.Sleep(500);
                }
            }
            //
            //Thread thd = new Thread(delegate ()
            //{
            //    while (true)
            //    {
            //        string folder = @"C:\Users\Administrator\Desktop\0524";
            //        string[] files = Directory.GetFiles(folder);
            //        this.BeginInvoke(new Action(() =>
            //        {
            //        foreach (string name in files)
            //        {
            //            Image image = Image.FromFile(name);
            //            Bitmap bitmap = new Bitmap(image);
            //            HObject image1 = ccdShow1.HImageConvertFromBitmap(bitmap);
            //            ccdShow1.DirectShowImage(image1);
            //            ccdShow1.HandleBarcode(image1, 1);
            //            Thread.Sleep(500);
            //        }
            //        }));
            //        Thread.Sleep(3000);
            //    }
            //});
            //thd.Start();
        }
    }
}
