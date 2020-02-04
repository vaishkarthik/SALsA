using Microsoft.Azure.KeyVault.Models;
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
        public string UID { get; private set; }
        private StreamWriter sw = null;
        private int Id;
        public string LogFileName { get; private set; }
        public string LogFullPath { get; private set; }
        public string LogFolderPath { get; private set; }
        public Log(int Id = 0)
        {
            this.Id = Id;
            UID = Utility.ShortRandom;
            LogFolderPath = System.IO.Path.Combine(System.IO.Path.GetPathRoot(Environment.SystemDirectory), "Log");
            LogFileName = String.Format("{0}-{1}{2}", Constants.LogFileNamePrefix, UID, Constants.LogFileNameExtension);
            LogFullPath = System.IO.Path.Combine(LogFolderPath, LogFileName);
            if (!File.Exists(LogFullPath))
            {
                try
                {
                    new System.IO.FileInfo(LogFolderPath).Directory.Create();
                    sw = File.AppendText(LogFullPath);
                }
                catch
                {
                    // TODO : log this later
                    LogFolderPath = System.IO.Path.GetTempPath();
                    LogFullPath = Path.Combine(LogFolderPath, LogFileName);
                    new System.IO.FileInfo(LogFolderPath).Directory.Create();
                    sw = File.AppendText(LogFullPath);
                }
            }
            else
            {
                sw = File.AppendText(LogFullPath);
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
            string toSend = arg.Length > 0 ?  String.Format(CultureInfo.InvariantCulture, ss, arg) : ss;
            InternalLog(toSend, LogLevel.Online);
            SendOnline(toSend);
        }
        public void Send(string ss, bool htmlfy, params object[] arg)
        {
            string toSend = arg.Length > 0 ? String.Format(CultureInfo.InvariantCulture, ss, arg) : ss;
            InternalLog(toSend, LogLevel.Online);
            SendOnline(toSend, htmlfy: false);
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
            Console.ForegroundColor = ConsoleColor.DarkGray;
            InternalLog(String.Format(CultureInfo.InvariantCulture, ss, arg), LogLevel.Verbose);
        }
        public void Information(string ss, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.White;
            InternalLog(String.Format(CultureInfo.InvariantCulture, ss, arg), LogLevel.Information);
        }
        public void Warning(string ss, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            InternalLog(String.Format(CultureInfo.InvariantCulture, ss, arg), LogLevel.Warning);
        }
        public void Error(string ss, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            InternalLog(String.Format(CultureInfo.InvariantCulture, ss, arg), LogLevel.Error);
        }
        public void Critical(string ss, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            InternalLog(String.Format(CultureInfo.InvariantCulture, ss, arg), LogLevel.Critical);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Exception(Exception ex)
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
        private void InternalLog(string ss, LogLevel lvl)
        {

            var currentTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture);
            string logLine = String.Format(CultureInfo.InvariantCulture, "{0} [ICM:{1}] {2} |> {3} <{4}>: {5}", UID, Id, currentTime, lvl, GetCallerMethod(), ss);
            Console.WriteLine(logLine);
            Console.ResetColor();
            System.Diagnostics.Trace.WriteLine(logLine);
            WriteToLog(logLine);
        }

        private void WriteToLog(string ss)
        {
            try
            {
                sw?.WriteLine(ss);
            }
            catch { };
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


        private void SendOnline(string ss, bool force = false, bool htmlfy = true)
        {
            SALsA.GetInstance(Id)?.ICM.AddICMDiscussion(ss, force, htmlfy);
        }

        internal void FlushAndClose()
        {
            this.Verbose("FlushAndClose called. Finished processing ICM");
            sw?.Flush();
            sw?.Close();
            sw = null;
        }
    }
}
