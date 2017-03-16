using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace ItemSetEditor
{
#if DEBUG
    public static class Log
    {
        private static StreamWriter file;

        private static string timestamp => "[" + DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss", CultureInfo.GetCultureInfo("en-US")) + "] ";

        private static string[] levels = new string[] { "[ERROR]", "[WARNING]", "[INFO]" };
        public static int Level { get; set; } = 2;

        public static void Init()
        {
            file = new StreamWriter("ItemSetEditor\\log\\" + DateTime.Now.ToString("yyyy_MM_dd", CultureInfo.GetCultureInfo("en-US")) + ".txt", false);
        }

        [Conditional("DEBUG")]
        public static void Info(string message) { Write(2, timestamp + message); }
        public static void Warning(string message) { Write(1, timestamp + message); }
        public static void Error(string message) { Write(0, timestamp + message); }
        private static void Write(int level, string message)
        {
            if (level > Level || level < 0 || level >= levels.Length || file == null)
                return;

            message = levels[level] + message;

            file.WriteLine(message);
            file.Flush();
            Console.WriteLine(message);
        }

        public static void Close()
        {
            file.Close();
            file = null;
        }
    }
#endif
}
