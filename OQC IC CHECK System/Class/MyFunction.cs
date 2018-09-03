using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DWGdirect.DatabaseServices;

namespace OQC_IC_CHECK_System
{
    public class MyFunction
    {
        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(
                           IntPtr hwnd,
                           int wMsg,
                           IntPtr wParam,
                           IntPtr lParam);

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key,
            string val, string filePath);

        /// <summary>
        ///  从配置文件中读出整个Section的内容 
        /// </summary>
        ///  <param name="section">INI文件中的段落</param>
        ///  <param name="lpReturn">返回的数据数组</param>
        ///  <param name="nSize">返回数据的缓冲区长度</param>
        ///  <param name="strFileName">INI文件的完整的路径(包含文件名)</param>
        ///  <returns></returns>
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileSection(string section,
                byte[] lpReturn, int nSize, string strFileName);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);

        private Logs log = Logs.LogsT();



        #region 获取软件版本号
        public string GetVersion()
        {
            string NowVersion = "V1.0";
            object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyFileVersionAttribute), false);
            if (attributes.Length > 0)
            {
                if (attributes.Length > 0)
                {
                    NowVersion = ((System.Reflection.AssemblyFileVersionAttribute)attributes[0]).Version;
                }
            }
            return NowVersion;
        }

        #endregion

        //配置文件的路径
        private string GetConfigIniPath(string FileName)
        {
            string dllpath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            dllpath = dllpath.Substring(8, dllpath.Length - 8);    // 8是 file:// 的长度
            char sep = System.IO.Path.DirectorySeparatorChar;
            return System.IO.Path.GetDirectoryName(dllpath) + sep + "Config" + sep + FileName;
        }
        //配置文件的路径
        internal string GetProductIniPath(string FileName)
        {
            string dllpath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            dllpath = dllpath.Substring(8, dllpath.Length - 8);    // 8是 file:// 的长度
            char sep = System.IO.Path.DirectorySeparatorChar;
            return System.IO.Path.GetDirectoryName(dllpath) + sep + "Product" + sep + FileName;
        }

        public void clearHistoryData()
        {
            GlobalVar.Ref_Point_CADPos = new SPoint();
            GlobalVar.gl_List_PointInfo.clearList();
        }
        /// <summary>
        /// 加载CAD文件 
        /// </summary>
        ///<param name = "product" > 品目名 </param >
        internal void LoadCADFile(string product)
        {
            string ProductFile = GetProductIniPath(product + ".dwg");
            try
            {
                DWGdirect.Runtime.SystemObjects.DynamicLinker.LoadApp("GripPoints", false, false);
                DWGdirect.Runtime.SystemObjects.DynamicLinker.LoadApp("PlotSettingsValidator", false, false);
                HostApplicationServices.Current = new HostAppServ(GlobalVar.m_dwgdirectServices);
                Database m_database = new Database(false, false);
                clearHistoryData();
                //加载DWG
                m_database.ReadDwgFile(ProductFile, FileOpenMode.OpenForReadAndAllShare, false, "");
                GlobalVar.CADPointList.clearList();
                HostApplicationServices.WorkingDatabase = m_database;
                //遍历-根据实体名称获得目标点信息
                GetEntitiesInModelSpace();
                GlobalVar.CADPointList.Sort();
                //m_dwgdirectServices.Dispose();
            }
            catch (Exception ex)
            {
                log.AddERRORLOG("加载CAD文件异常：" + ex.Message);
            }
        }

        private void GetEntitiesInModelSpace()
        {
            try
            {
                using (Transaction transaction = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                {
                    BlockTable blockTable =
                        (BlockTable)transaction.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead);

                    BlockTableRecord blockTableRecord = (BlockTableRecord)transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                    foreach (ObjectId objID in blockTableRecord)
                    {
                        Entity ent = (Entity)objID.GetObject(OpenMode.ForRead);

                        if ((ent as DWGdirect.DatabaseServices.BlockReference) == null) continue;
                        string entityname = (ent as DWGdirect.DatabaseServices.BlockReference).Name.ToUpper();
                        //// 增加载板大小判断，根据读取坐标差值来计算 [10/12/2017 617004]
                        //if (entityname.Contains("MARKPOINT"))
                        //{
                        //    Mark_Pos_X = float.Parse((ent as DWGdirect.DatabaseServices.BlockReference).Position.X.ToString("0.00"));
                        //}
                        //if (entityname.Contains("REFPOINT"))
                        //{
                        //    Ref_Pos_X = float.Parse((ent as DWGdirect.DatabaseServices.BlockReference).Position.X.ToString("0.00")); 
                        //}
                        if (entityname.ToUpper().IndexOf("TIPPOINT") >= 0)
                        { }
                        //-----早期的TIPPOINT，没有多个MIC,名称仅为TIPPOING00n，后期修改为TIPPOINT_n，n为FLOWID
                        if (entityname.ToUpper().IndexOf(GlobalVar.gl_str_TipPoint) >= 0)
                        {
                            ent.UpgradeOpen();
                            ent.Color = DWGdirect.Colors.Color.FromColor(Color.Red);
                            ent.ColorIndex = 100;
                            ent.DowngradeOpen();
                            //ent.EntityColor = new DWGdirect.Colors.EntityColor(0, 255, 0);
                            SPoint SP = new SPoint();
                            SP.Point_name = (ent as DWGdirect.DatabaseServices.BlockReference).Name;
                            SP.PointNumber = Convert.ToInt32(entityname.Replace("TIPPOINT", "").Substring(0, 4));  //TIPPOINT0xxx 
                            SP.Pos_X = float.Parse((ent as DWGdirect.DatabaseServices.BlockReference).Position.X.ToString("0.00"));
                            SP.Pos_Y = float.Parse((ent as DWGdirect.DatabaseServices.BlockReference).Position.Y.ToString("0.00"));
                            SP.Pos_Z = float.Parse((ent as DWGdirect.DatabaseServices.BlockReference).Position.Z.ToString("0.00"));

                            GlobalVar.CADPointList.addPoint(SP);
                        }
                        else if (entityname.ToUpper() == GlobalVar.gl_str_RefPoint)
                        {
                            GlobalVar.Ref_Point_CADPos.Point_name = (ent as DWGdirect.DatabaseServices.BlockReference).Name;
                            GlobalVar.Ref_Point_CADPos.Pos_X = float.Parse((ent as DWGdirect.DatabaseServices.BlockReference).Position.X.ToString("0.00"));
                            GlobalVar.Ref_Point_CADPos.Pos_Y = float.Parse((ent as DWGdirect.DatabaseServices.BlockReference).Position.Y.ToString("0.00"));
                            GlobalVar.Ref_Point_CADPos.Pos_Z = float.Parse((ent as DWGdirect.DatabaseServices.BlockReference).Position.Z.ToString("0.00"));


                        }
                        else if (entityname.ToUpper() == GlobalVar.gl_str_ScrrenRefPoint)
                        {
                            GlobalVar.point_ScrrenRefPoint.Pos_X = float.Parse((ent as DWGdirect.DatabaseServices.BlockReference).Position.X.ToString("0.00"));
                            GlobalVar.point_ScrrenRefPoint.Pos_Y = float.Parse((ent as DWGdirect.DatabaseServices.BlockReference).Position.Y.ToString("0.00"));
                            GlobalVar.point_ScrrenRefPoint.Pos_Z = float.Parse((ent as DWGdirect.DatabaseServices.BlockReference).Position.Z.ToString("0.00"));
                        }
                        else if (entityname.ToUpper() == GlobalVar.gl_str_MARKPoint)
                        {
                            GlobalVar.point_CalPos.Pos_X = float.Parse((ent as DWGdirect.DatabaseServices.BlockReference).Position.X.ToString("0.00"));
                            GlobalVar.point_CalPos.Pos_Y = float.Parse((ent as DWGdirect.DatabaseServices.BlockReference).Position.Y.ToString("0.00"));
                            GlobalVar.point_CalPos.Pos_Z = float.Parse((ent as DWGdirect.DatabaseServices.BlockReference).Position.Z.ToString("0.00"));
                            GlobalVar.gl_point_CalPosRef.Pos_X = GlobalVar.point_CalPos.Pos_X;
                            GlobalVar.gl_point_CalPosRef.Pos_Y = GlobalVar.point_CalPos.Pos_Y;
                            GlobalVar.gl_point_CalPosRef.Pos_Z = GlobalVar.point_CalPos.Pos_Z;
                        }
                    }
                    transaction.Commit();
                }
            }
            catch { throw new System.Exception("CAD文档加载错误!"); }
        }

        /// <summary>
        /// 字符串反转
        /// </summary>
        /// <param name="str">需要反转的字符串</param>
        /// <returns></returns>
        internal static string StrReverse(string str)
        {
            char[] c = str.ToCharArray();
            Array.Reverse(c);
            return new string(c);
        }


        /// <summary>
        /// 读取config文件
        /// </summary>
        internal void ReadInitConfig()
        {
            string IniFile = GetConfigIniPath("Config.ini");

            string product = string.Empty;
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Product, GlobalVar.gl_iniKey_Product, ref product);
            GlobalVar.Product = product;

            ReadIni_Value(IniFile,GlobalVar.gl_inisection_CCD,GlobalVar.gl_iniKey_SavePath, ref GlobalVar.PictureSavePath);


            string count = string.Empty;
            ReadIni_Value(IniFile,GlobalVar.gl_inisection_Sheet,GlobalVar.gl_iniKey_BoardCount,ref count);
            GlobalVar.BoardCount = Convert.ToInt32(count);
            ReadIni_Value(IniFile,GlobalVar.gl_inisection_Sheet,GlobalVar.gl_iniKey_ICFailCount,ref count);
            GlobalVar.ICFailCount = Convert.ToInt32(count);


            string link = string.Empty;
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_SoftWare, GlobalVar.gl_iniKey_LinkX, ref link);
            GlobalVar.AxisX.LinkIndex = Convert.ToUInt16(link);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_SoftWare, GlobalVar.gl_iniKey_LinkY, ref link);
            GlobalVar.AxisY.LinkIndex = Convert.ToUInt16(link);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_SoftWare, GlobalVar.gl_iniKey_LinkA, ref link);
            GlobalVar.AxisA.LinkIndex = Convert.ToUInt16(link);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_SoftWare, GlobalVar.gl_iniKey_LinkB, ref link);
            GlobalVar.AxisB.LinkIndex = Convert.ToUInt16(link);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_SoftWare, GlobalVar.gl_iniKey_LinkC, ref link);
            GlobalVar.AxisC.LinkIndex = Convert.ToUInt16(link);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_SoftWare, GlobalVar.gl_iniKey_LinkD, ref link);
            GlobalVar.AxisD.LinkIndex = Convert.ToUInt16(link);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_SoftWare, GlobalVar.gl_iniKey_LinkZ, ref link);
            GlobalVar.AxisZ.LinkIndex = Convert.ToUInt16(link);

            string ZPosition = string.Empty;
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_SoftWare, GlobalVar.gl_iniKey_ZPosition, ref ZPosition);
            GlobalVar.ZPosition_Read = Convert.ToDouble(ZPosition);
            
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_CCD, GlobalVar.gl_iniKey_CCDMode, ref GlobalVar.CCDMode);

            string mode = string.Empty;
            ReadIni_Value(IniFile,GlobalVar.gl_inisection_CCD,GlobalVar.gl_iniKey_ICRunMatrix,ref mode);
            GlobalVar.ICRunMatrix = Convert.ToBoolean(mode);
            ReadIni_Value(IniFile,GlobalVar.gl_inisection_CCD,GlobalVar.gl_iniKey_ICForbidden,ref mode);
            GlobalVar.ICForbiddenMode = Convert.ToBoolean(mode);

            ReadIni_Value(IniFile, GlobalVar.gl_inisection_COM, GlobalVar.gl_iniKey_PcsCOM, ref GlobalVar.PCS_COM);

            string position = string.Empty;
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_RefX, ref position);
            GlobalVar.Ref_Point_AxisX = Convert.ToDouble(position);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_RefY, ref position);
            GlobalVar.Ref_Point_AxisY = Convert.ToDouble(position);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_EndX, ref position);
            GlobalVar.End_Point_AxisX = Convert.ToDouble(position);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_EndY, ref position);
            GlobalVar.End_Point_AxisY = Convert.ToDouble(position);


            ReadIni_Value(IniFile,GlobalVar.gl_inisection_Axis,GlobalVar.gl_iniKey_ICXInterval,ref position);
            GlobalVar.Point_ICXInterval = Convert.ToDouble(position);
            ReadIni_Value(IniFile,GlobalVar.gl_inisection_Axis,GlobalVar.gl_iniKey_ICYInterval,ref position);
            GlobalVar.Point_ICYInterval = Convert.ToDouble(position);
            ReadIni_Value(IniFile,GlobalVar.gl_inisection_Axis,GlobalVar.gl_iniKey_IC_Columns,ref position);
            GlobalVar.IC_Columns = Convert.ToInt32(position);
            ReadIni_Value(IniFile,GlobalVar.gl_inisection_Axis,GlobalVar.gl_iniKey_IC_Rows,ref position);
            GlobalVar.IC_Rows = Convert.ToInt32(position);
            string time = string.Empty;
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_CylinderSuctionWaitTime, ref time);
            GlobalVar.CylinderSuctionWaitTime = Convert.ToInt16(time);
            ReadIni_Value(IniFile,GlobalVar.gl_inisection_Axis,GlobalVar.gl_iniKey_CylinderBlowWaitTime,ref time);
            GlobalVar.CylinderBlowWaitTime = Convert.ToInt16(time);

            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_FeedLeft, ref position);
            GlobalVar.Point_FeedLeft = Convert.ToDouble(position);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_FeedRight, ref position);
            GlobalVar.Point_FeedRight = Convert.ToDouble(position);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_DropLeft, ref position);
            GlobalVar.Point_DropLeft = Convert.ToDouble(position);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_DropRight, ref position);
            GlobalVar.Point_DropRight = Convert.ToDouble(position);
            ReadIni_Value(IniFile,GlobalVar.gl_inisection_Axis,GlobalVar.gl_iniKey_ICFeed,ref position);
            GlobalVar.Point_ICFeed = Convert.ToDouble(position);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_ICPhotoPosition, ref position);
            GlobalVar.Point_ICPhotoPosition = Convert.ToDouble(position);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_PCSFeed, ref position);
            GlobalVar.Point_PCSFeed= Convert.ToDouble(position);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_PCSPhotoPosition, ref position);
            GlobalVar.Point_PCSPhotoPosition = Convert.ToDouble(position);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_PCSWaitPosition, ref position);
            GlobalVar.Point_PCSWaitPosition = Convert.ToDouble(position);
            ReadIni_Value(IniFile,GlobalVar.gl_inisection_Axis,GlobalVar.gl_iniKey_FeedSaveDistance,ref position);
            GlobalVar.FeedSaveDistance = Convert.ToDouble(position);
            ReadIni_Value(IniFile,GlobalVar.gl_inisection_Axis,GlobalVar.gl_iniKey_DropSaveDistance,ref position);
            GlobalVar.DropSaveDistance = Convert.ToDouble(position);

            string speed = string.Empty;
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_HomeSpeed, ref speed);
            GlobalVar.HomeSpeed = Convert.ToDouble(speed);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_RefSpeedHigh, ref speed);
            GlobalVar.RefHighVel = Convert.ToDouble(speed);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_RefSpeedLow, ref speed);
            GlobalVar.RefLowVel = Convert.ToDouble(speed);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_RefAcc, ref speed);
            GlobalVar.RefAccVel = Convert.ToDouble(speed);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_RefDcc, ref speed);
            GlobalVar.RefDccVel = Convert.ToDouble(speed);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_RunSpeedHigh, ref speed);
            GlobalVar.RunHighVel = Convert.ToDouble(speed);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_RunSpeedLow, ref speed);
            GlobalVar.RunLowVel = Convert.ToDouble(speed);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_RunAcc, ref speed);
            GlobalVar.RunAccVel = Convert.ToDouble(speed);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_RunDcc, ref speed);
            GlobalVar.RunDccVel = Convert.ToDouble(speed);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_RunHighVel_Motor, ref speed);
            GlobalVar.RunHighVel_Motor = Convert.ToDouble(speed);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_RunLowVel_Motor, ref speed);
            GlobalVar.RunLowVel_Motor = Convert.ToDouble(speed);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_RunAccVel_Motor, ref speed);
            GlobalVar.RunAccVel_Motor = Convert.ToDouble(speed);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_RunDccVel_Motor, ref speed);
            GlobalVar.RunDccVel_Motor = Convert.ToDouble(speed);
            ReadIni_Value(IniFile,GlobalVar.gl_inisection_Axis,GlobalVar.gl_iniKey_RunSpeedHigh_Operate,ref speed);
            GlobalVar.RunHighVel_Operate = Convert.ToDouble(speed);
            ReadIni_Value(IniFile,GlobalVar.gl_inisection_Axis,GlobalVar.gl_iniKey_RunSpeedLow_Operate,ref speed);
            GlobalVar.RunLowVel_Operate = Convert.ToDouble(speed);
            ReadIni_Value(IniFile,GlobalVar.gl_inisection_Axis,GlobalVar.gl_iniKey_RunAcc_Operate,ref speed);
            GlobalVar.RunAccVel_Operate = Convert.ToDouble(speed);
            ReadIni_Value(IniFile,GlobalVar.gl_inisection_Axis,GlobalVar.gl_iniKey_RunDcc_Operate,ref speed);
            GlobalVar.RunDccVel_Operate = Convert.ToDouble(speed);

            string motion = string.Empty;
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_GPRunVelHigh, ref motion);
            GlobalVar.m_GPValue_RunVelHigh_move = Convert.ToUInt32(motion);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_GPRunVelLow, ref motion);
            GlobalVar.m_GPValue_RunVelLow_move = Convert.ToUInt32(motion);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_GPRunAcc, ref motion);
            GlobalVar.m_GPValue_RunAcc_move = Convert.ToUInt32(motion);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Axis, GlobalVar.gl_iniKey_GPRunDec, ref motion);
            GlobalVar.m_GPValue_RunDec_move = Convert.ToUInt32(motion);

            string ip = string.Empty;
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Ip, GlobalVar.gl_iniKey_FeedIP, ref ip);
            GlobalVar.FeedIP = ip;
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Ip, GlobalVar.gl_iniKey_DropIP, ref ip);
            GlobalVar.DropIP = ip;

            string LoadSpeed = string.Empty;
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Para, GlobalVar.gl_iniKey_FeedSpeed, ref LoadSpeed);
            GlobalVar.BoardFeedSpeed = Convert.ToDouble(LoadSpeed);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Para, GlobalVar.gl_iniKey_FeedAcc, ref LoadSpeed);
            GlobalVar.BoardFeedAcc = Convert.ToDouble(LoadSpeed);
            string LoadPosition = string.Empty;
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Para, GlobalVar.gl_iniKey_UPPostion, ref LoadPosition);
            GlobalVar.UpBoardFeedPosition = Convert.ToDouble(LoadPosition);
            ReadIni_Value(IniFile, GlobalVar.gl_inisection_Para, GlobalVar.gl_iniKey_DropPostion, ref LoadPosition);
            GlobalVar.DropBoardFeedPosition = Convert.ToDouble(LoadPosition);

        }

        public void LoadICPointList()
        {
            GlobalVar.ICPointList.Clear();
            if (GlobalVar.ICRunMatrix)//使用矩阵运动模式
            {
                GlobalVar.IC_Columns = 1+Math.Abs(Convert.ToInt32((GlobalVar.Ref_Point_AxisX - GlobalVar.End_Point_AxisX) / GlobalVar.Point_ICXInterval));
                GlobalVar.IC_Rows=1+Math.Abs( Convert.ToInt32((GlobalVar.Ref_Point_AxisY-GlobalVar.End_Point_AxisY)/GlobalVar.Point_ICYInterval));
                GlobalVar.ICCount = GlobalVar.IC_Columns * GlobalVar.IC_Rows;
                #region 
                //for(int i = 0; i < GlobalVar.ICCount; i++)
                //{
                //    if(i<4)
                //    {
                //        double x = GlobalVar.Ref_Point_AxisX;
                //        double y = GlobalVar.Ref_Point_AxisY + i * GlobalVar.Point_ICYInterval;
                //        SPoint sp = new SPoint();
                //        sp.Pos_X = x;
                //        sp.Pos_Y = y;
                //        GlobalVar.ICPointList.Add(sp);
                //    }
                //    if (i > 3 && i < 8)
                //    {
                //        double x = GlobalVar.Ref_Point_AxisX - GlobalVar.Point_ICXInterval;
                //        double y = GlobalVar.Ref_Point_AxisY + (7-i) * GlobalVar.Point_ICYInterval;
                //        SPoint sp = new SPoint();
                //        sp.Pos_X = x;
                //        sp.Pos_Y = y;
                //        GlobalVar.ICPointList.Add(sp);
                //    }
                //    if (i > 7&&i<12)
                //    {
                //        double x = GlobalVar.Ref_Point_AxisX - GlobalVar.Point_ICXInterval * 2;
                //        double y = GlobalVar.Ref_Point_AxisY + (i - 8) * GlobalVar.Point_ICYInterval;
                //        SPoint sp = new SPoint();
                //        sp.Pos_X = x;
                //        sp.Pos_Y = y;
                //        GlobalVar.ICPointList.Add(sp);
                //    }
                //    if (i > 11)
                //    {
                //        double x = GlobalVar.Ref_Point_AxisX - GlobalVar.Point_ICXInterval * 3;
                //        double y = GlobalVar.Ref_Point_AxisY + (15-i) * GlobalVar.Point_ICYInterval;
                //        SPoint sp = new SPoint();
                //        sp.Pos_X = x;
                //        sp.Pos_Y = y;
                //        GlobalVar.ICPointList.Add(sp);
                //    }
                //}
                #endregion
                for(int i = 0; i < GlobalVar.IC_Columns;i++)
                {
                        double x = GlobalVar.Ref_Point_AxisX+i*GlobalVar.Point_ICXInterval;
                    for(int j = 0; j < GlobalVar.IC_Rows; j++)
                    {
                        double y;
                        int index = i * GlobalVar.IC_Rows+j;
                        int temp = i % 2;
                        if (temp != 0) y = GlobalVar.Ref_Point_AxisY + (GlobalVar.IC_Rows- index % GlobalVar.IC_Rows-1) * GlobalVar.Point_ICYInterval;
                        else y = GlobalVar.Ref_Point_AxisY + j * GlobalVar.Point_ICYInterval;
                        SPoint sp = new SPoint();
                        sp.Pos_X = x;
                        sp.Pos_Y = y;
                        GlobalVar.ICPointList.Add(sp);
                    }
                }

            }
            else//使用配置文件模式
            {
                string config = GlobalVar.Product + ".ini";
                string fileName = GetProductIniPath(config);
                #region 读取配置文件
                #endregion
            }
        }
        /// <summary>
        /// 保存报警频次记录
        /// </summary>
        internal void SaveAlarmRecord()
        {
            Logs log = Logs.LogsT();
            try
            {
                string file = AppDomain.CurrentDomain.BaseDirectory + @"\Config\AlarmRecord.ini";
                StringBuilder content = new StringBuilder();
                foreach (KeyValuePair<string, int> item in GlobalVar.Machine.AlarmRecord)
                {
                    content.AppendLine(string.Format("{0},{1}", item.Key, item.Value));
                }
                File.WriteAllText(file, content.ToString());//覆盖原有文件
            }
            catch (Exception ex)
            {
                log.AddERRORLOG("读取报警频次记录异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 写入配置文件
        /// </summary>
        /// <param name="section">Section范围</param>
        /// <param name="key">Key关键字</param>
        /// <param name="value">值</param>
        public void WriteIniString(string section, string key, string value)
        {
            string iniPath = GetConfigIniPath("Config.ini");
            WritePrivateProfileString(section, key, value, iniPath);
        }
        //配置文件的读取
        private bool GetIniString(string iniPath, string section, string key, out string value)
        {
            StringBuilder sb = new StringBuilder(1024);
            GetPrivateProfileString(section, key, "", sb, 1024, iniPath);
            value = sb.ToString();
            if (value.Length > 0)
                return true;
            else
                return false;
        }
        private void ReadIni_Value(string path, string section, string Key, ref string param, bool IgnoreErr = false)
        {
            string value = string.Empty;
            if (GetIniString(path, section, Key, out value))
            {
                param = value;
            }
            else
            {
                if (!IgnoreErr) throw new Exception(string.Format("{0} {1}\t 参数读取失败", path, Key));
            }
        }
        /// <summary>
        /// 读取报警频次记录
        /// </summary>
        internal void ReadAlarmRecord()
        {
            Logs log = Logs.LogsT();
            try
            {
                string file = AppDomain.CurrentDomain.BaseDirectory + @"\Config\AlarmRecord.ini";
                string[] content = File.ReadAllLines(file);
                GlobalVar.Machine.AlarmRecord.Clear();
                foreach (string str in content)
                {
                    string[] text = str.Split(',');
                    if (text.Length != 2) continue;
                    GlobalVar.Machine.AlarmRecord.Add(text[0], Convert.ToInt32(text[1]));
                }
            }
            catch (Exception ex)
            {
                log.AddERRORLOG("读取报警频次记录异常:" + ex.Message);
            }
        }
        #region 门锁状态读取

        /// <summary>
        /// 获取前门锁的状态DI
        /// </summary>
        /// <returns></returns>
        internal bool GetLockBeforeDIStatus()
        {
            //勿使用此方法，避免无法获取DI异常
            //return GlobalVar.AxisPCI.Tag_Lock1.CurrentValue && GlobalVar.AxisPCI.Tag_Lock2.CurrentValue;

            bool lockbefore = false;
            if (GlobalVar.AxisPCI.GetSingleDI(GlobalVar.AxisPCI.LockInBefore, ref lockbefore))
            {
                return lockbefore;
            }
            else throw new Exception("获取前门锁状态DI失败");
        }

        /// <summary>
        /// 获取后门锁2的状态DO
        /// </summary>
        /// <returns></returns>
        internal bool GetLock2Status()
        {
            bool lock2 = false;
            if (GlobalVar.AxisPCI.GetSingleDO(GlobalVar.AxisPCI.Lock, ref lock2))
            {
                return lock2;
            }
            else throw new Exception("获取后门锁2状态DO失败");
        }

        /// <summary>
        /// 获取后门锁1的状态DO
        /// </summary>
        /// <returns></returns>
        internal bool GetLock1Status()
        {
            bool lock1 = false;
            bool lock2 = false;
            if (GlobalVar.AxisPCI.GetSingleDO(GlobalVar.AxisPCI.Lock, ref lock1))
            {
                return lock1 && lock2;
            }
            else throw new Exception("获取后门锁1状态DO失败");
        }
        /// <summary>
        /// 获取后门锁2的状态DO
        /// </summary>
        /// <returns></returns>
        internal bool GetLock2DIStatus()
        {
            bool lock2 = false;
            if (GlobalVar.AxisPCI.GetSingleDI(GlobalVar.AxisPCI.LockIn2, ref lock2))
            {
                return  lock2;
            }
            else throw new Exception("获取后门锁2状态DI失败");
        }
        /// <summary>
        /// 获取后门锁DO状态
        /// </summary>
        /// <returns></returns>
        internal bool GetLockBackStatus()
        {
            bool lock1 = false;
            if (GlobalVar.AxisPCI.GetSingleDO(GlobalVar.AxisPCI.Lock, ref lock1))
            {
                return lock1;
            }
            else throw new Exception("获取后门锁状态DO失败");
        }


        /// <summary>
        /// 获取安全门锁的状态DO
        /// </summary>
        /// <returns></returns>
        internal bool GetLockBigDOStatus()
        {
            bool lockback = false;
            if (GlobalVar.AxisPCI.GetSingleDO(GlobalVar.AxisPCI.Lock, ref lockback))
            {
                return lockback;
            }
            else throw new Exception("获取安全门锁状态DO失败");
        }
        /// <summary>
        /// 获取照明灯的状态DO
        /// </summary>
        /// <returns></returns>
        internal bool GetLightDOStatus()
        {
            bool light = false;
            if (GlobalVar.AxisPCI.GetSingleDO(GlobalVar.AxisPCI.Light, ref light))
                return light;
            else throw new Exception("获取照明灯状态DO失败");
        }
        /// <summary>
        /// 获取上料轴气缸的状态DO
        /// </summary>
        /// <returns></returns>
        internal bool GetCylinderFeedDOStatus()
        {
            bool feed = false;
            if (GlobalVar.AxisPCI.GetSingleDO(GlobalVar.AxisPCI.CylinderFeed, ref feed))
                return feed;
            else throw new Exception("获取上料轴气缸状态DO失败");
        }
        /// <summary>
        /// 获取下料轴气缸的状态DO
        /// </summary>
        /// <returns></returns>
        internal bool GetCylinderDropDOStatus()
        {
            bool drop = false;
            if (GlobalVar.AxisPCI.GetSingleDO(GlobalVar.AxisPCI.CylinderDrop, ref drop))
                return drop;
            else throw new Exception("获取下料轴状态DO失败");
        }
        /// <summary>
        /// 获取PCS轴气缸的状态DO
        /// </summary>
        /// <returns></returns>
        internal bool GetCylinderPCSDOStatus()
        {
            bool pcs = false;
            if (GlobalVar.AxisPCI.GetSingleDO(GlobalVar.AxisPCI.CylinderPCS, ref pcs))
                return pcs;
            else throw new Exception("获取PCS轴状态DO失败");
        }

        /// <summary>
        /// 获取大锁的状态DI
        /// </summary>
        /// <returns></returns>
        internal bool GetLockBigDIStatus()
        {
            bool lock1 = false;
            bool lock2 = false;
            if (GlobalVar.AxisPCI.GetSingleDI(GlobalVar.AxisPCI.LockIn1, ref lock1) && GlobalVar.AxisPCI.GetSingleDI(GlobalVar.AxisPCI.LockInBefore, ref lock2))
            {
                return lock1 && lock2;
            }
            else throw new Exception("获取大锁状态DI失败");
        }

        #endregion



        //CRC8位校验
        public string CRC8(string str)
        {
            byte[] buffer = System.Text.Encoding.Default.GetBytes(str);
            short crc = 0;
            for (int j = 0; j < buffer.Length; j++)
            {
                crc ^= (Int16)(buffer[j] << 8);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) > 0)
                    {
                        crc = (Int16)((crc << 1) ^ 0x1021);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }
            return string.Format(Convert.ToString(crc, 16).ToUpper().PadLeft(4, '0'), "0000");
        }
    }
}
