using System;
using System.Threading;
using System.Windows.Forms;

namespace GameLogin
{
    internal static class Program
    {
        private const string MutexName = "Local\\CQYH_Launcher_SingleInstance";

        [STAThread]
        private static void Main()
        {
            using var mutex = new Mutex(true, MutexName, out var isNew);
            if (!isNew)
            {
                // Another instance is already running
                return;
            }

            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MainForm());
        }
    }
}
