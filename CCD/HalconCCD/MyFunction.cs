using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace HalconCCD
{
    class MyFunction
    {

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);
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
            return System.IO.Path.GetDirectoryName(dllpath) + sep + "TestData" + sep + FileName;
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
        public void Read_ini()
        {
            string IniFile = GetConfigIniPath("Config.ini");

            string connectMode = string.Empty;
            ReadIni_Value(IniFile,"CCDInfo","ConnectMode",ref connectMode);
            GlobalVar.ConnectMode = connectMode;
        }
    }
}
