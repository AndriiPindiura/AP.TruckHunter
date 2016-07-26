using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Drawing;

namespace TruckHunter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ListForm());
        }
    }
    public static class CamMonitor
    {
        public delegate void CamMonitorEvent(string ip, int incam, int outcam, 
            int upcam, DateTime entertime, DateTime exittime, DateTime uptime, 
            string id1c, string truckid, string culture, string who, 
            string entry, string timint, bool refresh, Color brush, int quality);
        public static CamMonitorEvent CamMonitorEventHandler;
    }
    public static class Navigator
    {
        public delegate void NavigatorEvent(bool vector, int tab);
        public static NavigatorEvent NavigatorEventHandler;
    }
    public class Log
    {
        private static object sync = new object();
        public static void Write(Exception ex, String addinfo)
        {
            try
            {
                // Путь .\\Log
                string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
                if (!Directory.Exists(pathToLog))
                    Directory.CreateDirectory(pathToLog); // Создаем директорию, если нужно
                string filename = Path.Combine(pathToLog, string.Format("{0}_{1:dd.MM.yyy}.log",
                AppDomain.CurrentDomain.FriendlyName, DateTime.Now));
                string fullText = string.Format("[{0:dd.MM.yyy HH:mm:ss.fff}] [{1}.{2}()] {3}\r\n",
                DateTime.Now, ex.TargetSite.DeclaringType, ex.TargetSite.Name, ex.Message);
                fullText += addinfo;
                lock (sync)
                {
                    File.AppendAllText(filename, fullText, Encoding.GetEncoding("Windows-1251"));
                }
            }
            catch
            {
                // Перехватываем все и ничего не делаем
            }
        }
    }
}