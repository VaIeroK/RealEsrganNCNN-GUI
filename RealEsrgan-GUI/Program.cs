using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
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
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (var singleInstance = new Mutex(true, "RealEsrganUpscale_SingleInstance", out bool createdNew))
            {
                if (!createdNew)
                {
                    if (args.Length == 0) return;

                    try
                    {
                        using (var client = new NamedPipeClientStream(".", "RealEsrganPipe", PipeDirection.Out))
                        {
                            int retries = 10;
                            while (retries-- > 0)
                            {
                                try
                                {
                                    client.Connect(100);
                                    break;
                                }
                                catch
                                {
                                    Thread.Sleep(50);
                                }
                            }

                            using (var writer = new StreamWriter(client) { AutoFlush = true })
                            {
                                string argString = string.Join("|", args);
                                writer.WriteLine(argString);
                            }
                        }

                    }
                    catch
                    {
                        Console.WriteLine("No running instance found.");
                    }
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
