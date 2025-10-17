using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealEsrgan_GUI
{
    internal static class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (var singleInstance = new Mutex(true, "RealEsrganUpscale_SingleInstance", out bool createdNew))
            {
                if (!createdNew)
                {
                    // If another instance is running, forward args via pipe and exit.
                        if (args.Length == 0) return;

                    if (PipeServer.SendArgs("RealEsrganPipe", args))
                        return;

                    // fallback: no running instance found or failed to send
                    Console.WriteLine("No running instance found.");
                    return;
                }

                Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(args));
            }
        }
    }
}
