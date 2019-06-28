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

        public string Icm { get; set; } = "N/A";
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
        private enum LogLevel { Online = 0x0020, Verbose = 0x0010, Information = 0x0008, Warning = 0x0004, Error = 0x002, Critical = 0x001};

        public void Send(object obj)
        {
            Send("{0}", (string)(obj.ToString()));
        }

        public void Send(string ss, params object[] arg)
        {
            string toSend = String.Format(CultureInfo.InvariantCulture, ss, arg);
            InternalLog(toSend, LogLevel.Online);
            SendOnline(toSend);
        }

        // SendForce will force write the log entry, even if it already exists.
        public void SendForce(object obj)
        {
            SendForce("{0}", (string)(obj.ToString()));
        }

        public void SendForce(string ss, params object[] arg)
        {
            string toSend = String.Format(CultureInfo.InvariantCulture, ss, arg);
            InternalLog(toSend, LogLevel.Online);
            SendOnline(toSend, true);
        }

        // Forcing toString on simple objects
        public void Verbose(object obj)
        {
            Verbose("{0}", (string)(obj.ToString()));
        }
        public void Information(object obj)
        {
            Information("{0}", (string)(obj.ToString()));
        }
        public void Warning(object obj)
        {
            Warning("{0}", (string)(obj.ToString()));
        }
        public void Error(object obj)
        {
            Error("{0}", (string)(obj.ToString()));
        }
        public void Critical(object obj)
        {
            Critical("{0}", (string)(obj.ToString()));
        }


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
            string logLine = String.Format(CultureInfo.InvariantCulture, "{0} [ICM:{1}] {2} |> {3} <{4}>: {5}", UID, Icm, currentTime, lvl, GetCallerMethod(), ss);
            Console.WriteLine(logLine);
            System.Diagnostics.Trace.WriteLine(logLine);
            WriteToLog(logLine);
        }

        private void WriteToLog(string ss)
        {
            sw?.WriteLine(ss);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private string GetCallerMethod()
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


        private void SendOnline(string ss, bool force = false)
        {
            ICM.IncidentMapping[Icm].AddICMDiscussion(ss, force);
        }

    }
}
