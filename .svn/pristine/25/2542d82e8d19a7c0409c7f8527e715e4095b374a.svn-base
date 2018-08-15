using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Euresys.Open_eVision_1_2;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

namespace PylonLiveView
{
    public partial class DetailBlock : UserControl
    {
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key,
            string val, string filePath);

        public int flowid;   
        MatrixDecode m_decoder = new MatrixDecode();

        public Double Pos_X_CAD;
        public Double Pos_Y_CAD;
        public Double Pos_Z_CAD;

        public Bitmap m_bitmap = null;
        //Bitmap m_bitmap = new Bitmap(640, 480);
        public DetailBlock()
        {
            InitializeComponent();
        }

        public Halcon gl_halcon = new Halcon();
        public bool m_receivedPics = false;   //是否接受到图片(判断相机是否有漏帧)
        public bool m_decodeFinished = false;  //是否在解析中标志
        public bool m_TypeCheck = true;    //MIC/PROX製品類型是否有錯
        public int m_PcsNo;
        public int m_PcsNo_Mapping;
        public bool m_GoodPostion = true;   //是否为有效位置
        public bool m_result = true;    //解析結果
        public string m_resultString = "";
        public string m_sheetbarcode = "";

        public Bitmap _bitmap
        {
            get { return (Bitmap)pictureBox1.Image; }
            set 
            {
                try
                {
                    m_resultString = "";
                    m_bitmap = (Bitmap)value.Clone();
                    //m_receivedPics = true;
                    Bitmap fordisplay = (Bitmap)value.Clone();
                    if (m_GoodPostion)
                    {
                        BeginInvoke(new Action(() =>
                        {
                            pictureBox1.Image = fordisplay;
                        }));
                        //Thread thread = new Thread(backthread_decode_Halcon);
                        //thread.IsBackground = true;
                        //thread.Start(null);
                    }
                    else
                    {
                        m_result = true;
                        m_decodeFinished = true;
                        BeginInvoke(new Action(() =>
                        {
                            pictureBox1.Image = fordisplay;
                            button_result.BackColor = Color.BurlyWood;
                            button_result.Text = "無部品搭載";
                        }));
                    }
                    BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                string saveDic = GlobalVar.gl_PicsSavePath + "\\" + m_sheetbarcode;
                                if (!Directory.Exists(saveDic))
                                {
                                    Directory.CreateDirectory(saveDic);
                                }
                                if (GlobalVar.gl_saveCapturePics)
                                {
                                    try
                                    {
                                        fordisplay.Save(saveDic + "\\" + m_sheetbarcode + "_" + m_PcsNo_Mapping.ToString() + "_" + flowid.ToString() + ".bmp"
                                            , System.Drawing.Imaging.ImageFormat.Bmp);
                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            m_receivedPics = true;
                        }));
                }
                catch
                {
                    m_decodeFinished = true;
                }
            }
        }

        public void setsize(int width, int weight)
        {
            try { }
            catch (Exception e) { throw new Exception("Detail Block 设置大小异常."); }
        }

        public void setPositionDisplay(string x, string y)
        {
            lbl_pos_x.Text = x;
            lbl_pos_y.Text = y;
        }

        /*  OpeneVision 解析方式
        private void backthread_decode_OpenEVision(object obj)
        {
            try
            {
                lock (m_bitmap)
                {
                    if (m_bitmap != null)
                    {
                        if (GlobalVar.gl_inEmergence)
                        {
                            m_result = false;
                            m_decodeFinished = true;
                        }
                        Bitmap tmpbmp = (Bitmap)m_bitmap.Clone();
                        //Bitmap tmpbmp = new Bitmap(m_bitmap);
                        Thread.Sleep(20);
                        EImageBW8 EB8Image = m_decoder.ConvertBitmapToEImageBW8(tmpbmp);
                        m_resultString = m_decoder.GetDecodeStrbyEImageBW8(EB8Image);
                        Invoke(new Action(() =>
                            {
                                try
                                {
                                    if (checkStringIsLegal(m_resultString, 3)
                                        && (m_resultString.Trim() != "")
                                        && (m_resultString.Length > 5))
                                    {
                                        m_result = true;
                                        if (m_resultString.Substring(0, 3) != GlobalVar.gl_str_ProxHeadStr)
                                        {
                                            m_ProxTypeCheck = false;
                                            button_result.BackColor = Color.HotPink;
                                            //button_result.BackColor = Color.Green;
                                        }
                                        else
                                        {
                                            button_result.BackColor = Color.Green;
                                        }
                                        button_result.Text = m_resultString;
                                    }
                                    else
                                    {
                                        m_result = false;
                                        button_result.BackColor = Color.Red;
                                        button_result.Text = "解析失敗";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logWR.appendNewLogMessage("DetailBlock显示结果存储文件过程异常: \r\n" + ex.ToString());
                                }
                                m_decodeFinished = true;
                            }
                        ));
                    }
                };
            }
            catch
            {
                m_result = false;
                m_decodeFinished = true;
            }
            finally
            {
                ////存储结果数据
                //string resultDicPath = Application.StartupPath + "\\" + GlobalVar.gl_folderName_ResultSave;
                //if (!Directory.Exists(resultDicPath))
                //{ Directory.CreateDirectory(resultDicPath); }
                //string resultPath = resultDicPath + "\\" + m_sheetbarcode + ".ini";
                //WritePrivateProfileString(GlobalVar.gl_iniSection_Result, m_PcsNo_Mapping.ToString(), m_resultString , resultPath);   
            }
        }
        */

        public void backthread_decode_Halcon(int flowid)
        {
            m_result = false;
            try
            {
                bool BitmapFind = true;  //是否有查找到图片，担心没有文件生成，造成等待死循环
                string FilePath = GlobalVar.gl_PicsSavePath + "\\" + m_sheetbarcode
                    + "\\" + m_sheetbarcode + "_" + m_PcsNo_Mapping.ToString() + "_" + flowid.ToString() + ".bmp";
                DateTime datetime_start = DateTime.Now;
                for (; ; )
                {
                    if (File.Exists(FilePath)) break;
                    Thread.Sleep(100);
                    TimeSpan Ts = DateTime.Now.Subtract(datetime_start);
                    if (Ts.TotalMilliseconds > 5000)
                    {
                        BitmapFind = false;
                        break;
                    }
                }

                if (GlobalVar.gl_inEmergence)
                {
                    m_result = false;
                    m_decodeFinished = true;
                }
                if (!BitmapFind)
                {
                    Invoke(new Action(() =>
                      {
                          try
                          {
                              m_result = false;
                              button_result.BackColor = Color.Red;
                              button_result.Text = "無結果圖片";
                              logWR.appendNewLogMessage("位置" + m_PcsNo_Mapping.ToString() + "無結果圖片生成: \r\n");
                          }
                          catch (Exception ex)
                          {
                              logWR.appendNewLogMessage("DetailBlock显示结果存储文件过程异常: \r\n" + ex.ToString());
                          }
                      }));
                }
                else
                {
                    //先用HALCON解析一次
                    string result = "";
                    if (GlobalVar.gl_bUseHalcon)
                        result = gl_halcon.Decode(FilePath);
                    if ((result.Length < 6) || (!checkStringIsLegal(result, 3)))
                    { 
                        //HALCON解析失败，openevision重新解析一次
                        MatrixDecode decoder = new MatrixDecode();
                        result = decoder.GetDecodeStrbyPath(FilePath);
                    }
                    Invoke(new Action(() =>
                    {
                        try
                        {
                            try
                            {
                                if (result.Trim() != "")  //避免截取字符串异常
                                {
                                    List<ZhiPinInfo> zhipininfo = new List<ZhiPinInfo>();
                                    for (int n = 0; n < GlobalVar.gl_List_PointInfo.m_List_PointInfo.Count; n++)
                                    {
                                        if (flowid == GlobalVar.gl_List_PointInfo.m_List_PointInfo[n].FlowID)
                                        {
                                            for (int i = 0; i < GlobalVar.gl_List_PointInfo.m_List_PointInfo[n].m_list_zhipingInfo.Count; i++)
                                            {
                                                zhipininfo.Add(GlobalVar.gl_List_PointInfo.m_List_PointInfo[n].m_list_zhipingInfo[i]);
                                            }
                                        }
                                    }
                                    m_result = true; //没有用料信息，默认pass
                                    m_TypeCheck = true; //没有用料信息，默认pass
                                    //遍历GlobalVar.gl_list_MicInfo，只要有一项符合要求即判断为PASS
                                    for (int i = 0; i < zhipininfo.Count; i++)
                                    {
                                        m_result = false; //m_result只作为解析成功与否的判断
                                        ZhiPinInfo info = zhipininfo[i];
                                        if (checkStringIsLegal(result, 3)
                                        && (result.Length == info._BarcodeLength))
                                        { m_result = true; }
                                        if (result.Substring(info._StartPos, info._StartLen) != info._HeadStr)
                                        {
                                            m_TypeCheck = false;  //m_MICTypeCheck作为MIC条码合规与否的判断
                                        }
                                        else
                                        {
                                            m_TypeCheck = true;
                                        }
                                        //如果两项都OK了，就PASS
                                        if (m_result && m_TypeCheck)
                                        { break; }
                                    }
                                }
                                else
                                {
                                    m_result = false;
                                }
                            }
                            catch
                            {
                                m_result = false;
                            }
                            //结果显示
                            if (m_result && m_TypeCheck)
                            { 
                                button_result.BackColor = Color.Green;
                            }
                            else
                            { 
                                button_result.BackColor = Color.HotPink; //用料错误
                            }
                            m_resultString = result;
                            button_result.Text = result;
                            if ((result.Length > 0) && (!m_result))
                            {
                                button_result.BackColor = Color.LightGreen; //长度不符
                                button_result.Text = result;
                            }
                            if ((result.Length == 0) &&(!m_result))
                            {
                                button_result.BackColor = Color.Red;
                                button_result.Text = "解析失敗";
                                if (GlobalVar.gl_saveDecodeFailPics)
                                {
                                    File.Copy(FilePath, GlobalVar.gl_NGPicsSavePath + "\\" + Path.GetFileName(FilePath), true);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logWR.appendNewLogMessage("条码解析与DetailBlock显示结果存储文件过程异常: \r\n" + ex.ToString());
                        }
                    }
                    ));
                }
                m_decodeFinished = true;
            }
            catch (Exception ex)
            {
                m_result = false;
                m_decodeFinished = true;
                logWR.appendNewLogMessage("条码解析与DetailBlock显示结果存储文件过程异常: \r\n" + ex.ToString());
            }
            finally
            {
                ////存储结果数据
                //string resultDicPath = Application.StartupPath + "\\" + GlobalVar.gl_folderName_ResultSave;
                //if (!Directory.Exists(resultDicPath))
                //{ Directory.CreateDirectory(resultDicPath); }
                //string resultPath = resultDicPath + "\\" + m_sheetbarcode + ".ini";
                //WritePrivateProfileString(GlobalVar.gl_iniSection_Result, m_PcsNo_Mapping.ToString(), m_resultString , resultPath);   
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            //foreach (Control C in pictureBox1.Controls)
            //{
            //    if (C is Label)
            //    {
            //        Label L = (Label)C;
            //        L.Visible = false;
            //        //          e.Graphics.DrawString(L.Text, L.Font, new
            //        //SolidBrush(L.ForeColor), L.Left - cam_window.Left, L.Top - cam_window.Top);
            //        e.Graphics.DrawString(L.Text, L.Font, new
            //          SolidBrush(L.ForeColor), (pictureBox1.Width - L.Width) / 2,
            //          (pictureBox1.Height - L.Height) / 2);
            //    }
            //}
        }

        private void ToolStripMenuItem_save_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog fd = new SaveFileDialog();
                fd.Filter = "*.bmp|*.bmp";
                if (m_bitmap == null)
                {
                    MessageBox.Show("圖片為空，存儲無效!");
                    return;
                }
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    m_bitmap.Save(fd.FileName);
                }
            }
            catch { }
        }

        private void ToolStripMenuItem_magnify_Click(object sender, EventArgs e)
        {
            magnify mag = new magnify((Bitmap)m_bitmap.Clone());
            mag.ShowDialog();
        }

        public void Reset()
        {
            try
            {
                m_receivedPics = false;
                m_decodeFinished = false;
                m_GoodPostion = true;
                m_result = true;
                m_TypeCheck = true;
                BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            button_result.Text = "";
                            button_result.BackColor = Color.Gray;
                            pictureBox1.Image = null;
                        }
                        catch { }
                    }));
                m_bitmap.Dispose();
            }
            catch { }
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            magnify mag = new magnify((Bitmap)m_bitmap.Clone());
            mag.ShowDialog();
        }

        /// <summary>
        /// 检验输入是否合法
        /// </summary>
        /// <param name="str">检测字串</param>
        /// <param name="checkType">1：数字  2：英文字符  3：数字+英文字符</param>
        /// <returns></returns>
        bool checkStringIsLegal(string str, int checkType)
        {
            bool result = true;
            if (checkType == 1)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    char c = str[i];
                    result &= ((c >= 48) && (c <= 57));
                }
            }
            else if (checkType == 2)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    char c = str[i];
                    result &= (((c >= 65) && (c <= 90))
                        || ((c >= 97) && (c <= 122)));
                }
            }
            else if (checkType == 3)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    char c = str[i];
                    result &= ((((c >= 65) && (c <= 90)) || ((c >= 97) && (c <= 122)))
                        || ((c >= 48) && (c <= 57)));
                }
            }
            return result;
        }
    }
}
