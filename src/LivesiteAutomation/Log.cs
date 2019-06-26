using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Activation;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{

    public sealed class Log
    {

        public string ICM { get; set; } = "N/A";
        private string UID;
        private StreamWriter sw = null;
        private Log()
        {
            UID = Guid.NewGuid().ToString("n").Substring(0, 8);
            if (!File.Exists(Constants.LogDefaultPath))
            {
                try
                {
                    new System.IO.FileInfo(Constants.LogDefaultPath).Directory.Create();
                    sw = File.AppendText(Constants.LogDefaultPath);
                }
                catch
                {
                    // TODO : log this later
                    Constants.LogDefaultPath = Path.Combine(System.IO.Path.GetTempPath(), Constants.LogFileName);
                    new System.IO.FileInfo(Constants.LogDefaultPath).Directory.Create();
                    sw = File.AppendText(Constants.LogDefaultPath);
                }
            }
            else
            {
                sw = File.AppendText(Constants.LogDefaultPath);
            }
        }
        private static Log instance = null;
        public static Log Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Log();
                }
                return instance;
            }
        }
        // https://msdn.microsoft.com/en-us/magazine/ff714589.aspx
        private enum LogLevel { Verbose = 0x0010, Information = 0x0008, Warning = 0x0004, Error = 0x002, Critical = 0x001};

        public void Verbose(string ss, params object[] arg)
        {
            InternalLog(String.Format(CultureInfo.InvariantCulture, ss, arg), LogLevel.Verbose);
        }
        public void Information(string ss, params object[] arg)
        {
            InternalLog(String.Format(CultureInfo.InvariantCulture, ss, arg), LogLevel.Information);
        }
        public void Warning(string ss, params object[] arg)
        {
            InternalLog(String.Format(CultureInfo.InvariantCulture, ss, arg), LogLevel.Warning);
        }
        public void Error(string ss, params object[] arg)
        {
            InternalLog(String.Format(CultureInfo.InvariantCulture, ss, arg), LogLevel.Error);
        }
        public void Critical(string ss, params object[] arg)
        {
            InternalLog(String.Format(CultureInfo.InvariantCulture, ss, arg), LogLevel.Critical);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Exception(Exception ex)
        {
            InternalLog(String.Format(CultureInfo.InvariantCulture, "EXCEPTION :\n{0}", ex.ToString()), LogLevel.Error);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InternalLog(string ss, LogLevel lvl)
        {

            var currentTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm: ss.fffffffZ", CultureInfo.InvariantCulture);
            string logLine = String.Format(CultureInfo.InvariantCulture, "{0} [ICM:{1}] {2} |> {3} <{4}>: {5}", UID, ICM, currentTime, lvl, GetCallerMethod(3), ss);
            Console.WriteLine(logLine);
            System.Diagnostics.Trace.WriteLine(logLine);
            WriteToLog(logLine);
        }

        private void WriteToLog(string ss)
        {
            sw?.WriteLine(ss);
        }

        private string GetCallerMethod(int level = 2)
        {
            var caller = new StackFrame(level, true).GetMethod();
            string callerPath = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", caller.DeclaringType.FullName, caller.Name);
            return callerPath;
        }
    }
}
