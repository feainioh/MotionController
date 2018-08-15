using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace OQC_IC_CHECK_System
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool CreateNew = false;
            using (System.Threading.Mutex mutex = new System.Threading.Mutex(true, Application.ProductName, out CreateNew))
            {
                if (CreateNew) Application.Run(new Load());
                else
                {
                    MessageBox.Show("程序尚未完全退出，或者已经在运行中···");
                    System.Threading.Thread.Sleep(1000);
                    System.Environment.Exit(1);
                }
            }

        }
    }
}
