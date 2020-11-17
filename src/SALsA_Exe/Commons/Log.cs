using SALsA.LivesiteAutomation;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace SALsA.General
{

    public static class Log
    {
        public static string UID { get; private set; }
        public static StreamWriter LogStream { get; private set; }
        public static int Id = 0;
        public static string SAS { get; private set; }
        public static string StartTime { get; private set; }
        static Log()
        {
            ResetLog();
        }

        public static void ResetLog()
        {
            StartTime = DateTime.UtcNow.ToString("yyMMddTHHmmss", CultureInfo.InvariantCulture);
            UID = Utility.ShortRandom;
            MemoryStream ms = new MemoryStream();
            LogStream = new StreamWriter(ms);
        }

        public static void SetSAS(string sas)
        {
            SAS = sas;
        }

        // https://msdn.microsoft.com/en-us/magazine/ff714589.aspx
        private enum LogLevel { Online = 0x0020, Verbose = 0x0010, Information = 0x0008, Warning = 0x0004, Error = 0x002, Critical = 0x001 };

        // Forcing toString on simple objects
        public static void Verbose(object obj)
        {
            Verbose("{0}", (string)(obj.ToString()));
        }
        public static void Information(object obj)
        {
            Information("{0}", (string)(obj.ToString()));
        }
        public static void Warning(object obj)
        {
            Warning("{0}", (string)(obj.ToString()));
        }
        public static void Error(object obj)
        {
            Error("{0}", (string)(obj.ToString()));
        }
        public static void Critical(object obj)
        {
            Critical("{0}", (string)(obj.ToString()));
        }


        public static void Verbose(string ss, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            InternalLog(String.Format(CultureInfo.InvariantCulture, ss, arg), LogLevel.Verbose);
        }
        public static void Information(string ss, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.White;
            InternalLog(String.Format(CultureInfo.InvariantCulture, ss, arg), LogLevel.Information);
        }
        public static void Warning(string ss, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            InternalLog(String.Format(CultureInfo.InvariantCulture, ss, arg), LogLevel.Warning);
        }
        public static void Error(string ss, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            InternalLog(String.Format(CultureInfo.InvariantCulture, ss, arg), LogLevel.Error);
        }
        public static void Critical(string ss, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            InternalLog(String.Format(CultureInfo.InvariantCulture, ss, arg), LogLevel.Critical);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Exception(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            if (ex is AggregateException)
            {
                for (int j = 0; j < ((AggregateException)(ex)).InnerExceptions.Count; j++)
                {
                    sb.Append(String.Format("{0}---------------InnerExceptions:{1}---------------{0}", Environment.NewLine, j));
                    sb.Append(String.Format("{0}", ((AggregateException)(ex)).InnerExceptions[j]));
                }
            }
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            InternalLog(String.Format(CultureInfo.InvariantCulture, "EXCEPTION :\n{0}", ex.ToString(), sb), LogLevel.Error);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InternalLog(string ss, LogLevel lvl)
        {

            var currentTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture);
            string logLine = String.Format(CultureInfo.InvariantCulture, "{0} [ICM:{1}] {2} |> {3} <{4}>: {5}", UID, Id, currentTime, lvl, GetCallerMethod(), ss);
            Console.WriteLine(logLine);
            Console.ResetColor();
            System.Diagnostics.Trace.WriteLine(logLine);
            WriteToLog(logLine);
        }

        private static void WriteToLog(string ss)
        {
            try
            {
                LogStream?.WriteLine(ss);
            }
            catch { };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetCallerMethod()
        {
            int level = 0;
            string currentCaller = new StackFrame(level++, true).GetMethod().DeclaringType.FullName;
            string caller;
            try
            {
                do
                {
                    var callerMethod = new StackFrame(level++, true).GetMethod();
                    caller = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", callerMethod.DeclaringType.FullName, callerMethod.Name);
                    level++;
                } while (caller.StartsWith(currentCaller, StringComparison.InvariantCultureIgnoreCase));
            }
            catch
            {
                caller = "Unkwown function";
            }
            return caller;
        }

        internal static void FlushAndClose()
        {
            Verbose("FlushAndClose called. Finished processing ICM");
            LogStream?.Flush();
        }
    }
}
