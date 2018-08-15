using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace OQC_IC_CHECK_System
{
    /// <summary> 
    /// 十六进制   字符串   数组  转换   操作
    /// </summary>
    class ModbusTool
    {
        //<summary>Convert a string of hex digits (ex: E4 CA B2) to a byte array. </summary>  
        ///<param name="s">The string containing the hex digits (with or without spaces).</param>  
        ///<returns>Returns an array of bytes.</returns>  
        internal static byte[] HexStringToByteArray(string s)
        {
            s = s.Replace("   ", " ");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        #region 写入Modbus
        /// <summary>
        /// 数组内相邻两位交换（奇数的数组补齐0）
        /// </summary>
        /// <param name="array">数组</param>
        /// <returns></returns>
        internal static byte[] ExchangeByte(byte[] array)
        {
            byte[] NewArray;
            if (array.Length % 2 != 0)
            {
                NewArray = new byte[array.Length + 1];
                for (int i = 0; i < array.Length; i++)
                {
                    NewArray[i] = array[i];
                }
            }
            else NewArray = array;
            List<byte> ls = new List<byte>();
            for (int i = 0; i < NewArray.Length; i++)
            {
                if (i % 2 == 0) ls.Add(NewArray[i + 1]);
                else ls.Add(NewArray[i - 1]);
            }
            return ls.ToArray();
        }

        /// <summary>
        /// 字符串转为数组，然后一个字节转为一个Word，左侧补齐0
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static byte[] OneWord(string str)
        {
            byte[] StrByte = Encoding.Default.GetBytes(str);//首先获取字符串转换的字节数组
            List<byte> Word = new List<byte>();
            for (int i = 0; i < StrByte.Length; i++)
            {
                Word.AddRange(new byte[] { 0, StrByte[i] });
            }
            return Word.ToArray();
        }

        /// <summary>
        /// 数字转为数组，整数存储为两个个Word，左侧补齐0
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static byte[] TwoWord(int Num)
        {
            int value = Num;// Convert.ToInt(textbox1.Text.Trim());
            int hValue = (value >> 8) & 0xFF;
            int lValue = value & 0xFF;
            byte[] arr = new byte[] { (byte)hValue, (byte)lValue };

            string HexStr = string.Format("{0:X8}", Num);
            string HighStr = HexStr.Substring(0, 4);
            string LowStr = HexStr.Substring(4, 4);

            return CombineByteArray(HexStringToByteArray(LowStr), HexStringToByteArray(HighStr));
        }

        /// <summary>
        /// 数字转为数组，整数存储为一个Word，左侧补齐0
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static byte[] OneWord(int Num)
        {
            string HexStr = string.Format("{0:X4}", Num);

            return HexStringToByteArray(HexStr);
        }
        #endregion

        #region 读取Modbus
        /// <summary>
        /// 字节数组转换为Int
        /// </summary>
        /// <param name="rev">待转换的数组</param>
        /// <param name="Addr">地址</param>
        /// <param name="Size">长度</param>
        /// <returns></returns>
        internal static int WordToInt(byte [] rev,int Addr,int Size)
        {
            Size *= 2;
            Addr *= 2;
            byte[] byCopy = new byte[Size];
            int Int_Value;
            switch (Size)
            {
                case 2:
                    byCopy[0] = rev[Addr];
                    byCopy[1] = rev[Addr + 1];
                    Int_Value = NetToHostOrder16(byCopy, 0);
                        
                        //Convert.ToInt32(string.Format("{0}{1}",
                        //                        rev[Addr].ToString("X2"), rev[Addr + 1].ToString("X2")), 16);
                    break;
                case 4:
                    byCopy[0] = rev[Addr];
                    byCopy[1] = rev[Addr + 1];
                    byCopy[2] = rev[Addr + 2];
                    byCopy[3] = rev[Addr + 3];

                    //高位在前，低位在后
                    Int_Value = NetToHostOrder32(byCopy, 0);
                        
                        //Convert.ToInt32(string.Format("{0}{1}{2}{3}",
                        //                        rev[Addr].ToString("X2"), rev[Addr + 1].ToString("X2"),
                        //                        rev[Addr + 2].ToString("X2"), rev[Addr + 3].ToString("X2")), 16);
                    break;
                default: throw new ArgumentOutOfRangeException("解析Modbus，超出范围");
            }

            return Int_Value;
        }

        /// <summary>
        /// 两个字节代表一个字符(一个word存一个字节)，数组相邻两个位置的值交换
        /// </summary>
        /// <param name="arraybyte">需要转换的数组</param>
        /// <param name="start">需要转换的数组的起始位置</param>
        /// <param name="length">长度</param>
        internal static byte[] WordOne(byte[] arraybyte, int start, int length)
        {
            byte[] bigendian = new byte[length];
            for (int i = 0, j = 0; i < length; i++)
            {
                bigendian[i] = arraybyte[start + j];
                j += 2;
            }
            return bigendian;
        }

        /// <summary>
        /// 一个字节代表一个字符(一个word存两个个字节)，数组相邻两个位置的值交换
        /// </summary>
        /// <param name="arraybyte">需要转换的数组</param>
        /// <param name="start">需要转换的数组的起始位置</param>
        /// <param name="length">长度</param>
        internal static byte[] WordTwo(byte[] arraybyte, int start, int length)
        {
            byte[] NewByte = new byte[length];
            for (int i = 0; i < length; i++)
            {
                if (i % 2 == 0) NewByte[i] = arraybyte[start + i + 1];
                else NewByte[i] = arraybyte[start + i - 1];
            }
            return NewByte;
        }
        #endregion

        /// <summary>
        /// 选择数组的某一部分
        /// </summary>
        /// <param name="array">原数组</param>
        /// <param name="start">起始位</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        internal static byte[] ByteArraySelect(byte[] array, int start, int length)
        {
            List<byte> ls = new List<byte>();
            for (int i = 0; i < length; i++)
            {
                ls.Add(array[start + i]);
            }
            return ls.ToArray();
        }

        /// <summary>
        /// 补齐数组
        /// </summary>
        /// <param name="array">原数组</param>
        /// <param name="Length">补齐后的数组长度</param>
        /// <returns></returns>
        internal static byte[] ByteArrayPadRight(byte[] array, int Length)
        {
            List<byte> NewArray = new List<byte>();
            NewArray.AddRange(array);
            for (int i = array.Length; i < Length; i++)
            {
                NewArray.Add(0);
            }
            return NewArray.ToArray();
        }

        /// <summary>
        /// 合并字节数组
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static byte[] CombineByteArray(params byte[][] args)
        {
            List<byte> ls = new List<byte>();
            foreach (var item in args)
            {
                ls.AddRange(item);
            }
            return ls.ToArray();
        }

        /// <summary>
        /// 将byte数组中的两个字节转成16位有符号整型，再转换成主机字节序
        /// </summary>
        /// <param name="bys"></param>
        /// <param name="nStartIndex"></param>
        /// <returns></returns>
        public static int NetToHostOrder16(byte[] bys, int nStartIndex)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bys, nStartIndex));
        }

        /// <summary>
        /// 将16位有符号整型转换成网络字节序的byte数组
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static byte[] HostToNetOrder16(short Value)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Value));
        }

        /// <summary>
        /// 将byte数组中的两个字节转成32位有符号整型，再转换成主机字节序
        /// </summary>
        /// <param name="bys"></param>
        /// <param name="nStartIndex"></param>
        /// <returns></returns>
        public static int NetToHostOrder32(byte[] bys, int nStartIndex)
        {
            #region 高地位转换
            byte[] byCov = new byte[4];
            byCov[0] = bys[2];
            byCov[1] = bys[3];
            byCov[2] = bys[0];
            byCov[3] = bys[1];
            int n = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(byCov, nStartIndex));
            #endregion
            return n;
        }

        /// <summary>
        /// 将32位有符号整型转换成网络字节序的byte数组
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static byte[] HostToNetOrder32(int Value)
        {
            byte[] bys = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Value));
            #region 高地位转换
            //每两个byte前后互换
            byte[] byRet = new byte[4];
            byRet[0] = bys[2];
            byRet[1] = bys[3];
            byRet[2] = bys[0];
            byRet[3] = bys[1];
            return byRet;
            #endregion
        }

    }
}
