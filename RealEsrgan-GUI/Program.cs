using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealEsrgan_GUI
{
    internal static class Program
    {
        static Mutex singleInstance = new Mutex(true, "RealEsrganUpscale_SingleInstance");
        public const int WM_COPYDATA = 0x004A;

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData; 
            public IntPtr lpData;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (!singleInstance.WaitOne(TimeSpan.Zero, true))
            {
                SendArgsToRunningInstance(args);
                return;
            }

            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(args));
        }

        private static void SendArgsToRunningInstance(string[] args)
        {
            var processes = Process.GetProcessesByName("RealEsrgan-GUI");
            if (processes.Length > 0)
            {
                IntPtr hwnd = processes[0].MainWindowHandle;
                if (hwnd == IntPtr.Zero)
                {
                    processes[0].WaitForInputIdle();
                    hwnd = processes[0].MainWindowHandle;
                }
                string argString = string.Join("|", args); 
                COPYDATASTRUCT cds;
                cds.dwData = IntPtr.Zero;
                cds.cbData = (argString.Length + 1) * 2;
                cds.lpData = Marshal.StringToHGlobalUni(argString);
                SendMessage(hwnd, WM_COPYDATA, IntPtr.Zero, ref cds);
                Marshal.FreeHGlobal(cds.lpData);
            }
        }
    }
}
