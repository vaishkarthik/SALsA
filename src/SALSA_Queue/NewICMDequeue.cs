using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace SALSA_Queue
{
    public static class NewICMDequeue
    {
        [FunctionName("NewICMDequeue")]
        public static void Run([QueueTrigger("salsaqueue", Connection = "AzureWebJobsStorage")]string myQueueItem, TraceWriter log)
        {
            var currentDir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var salsaPath = Path.Combine(Directory.GetParent(currentDir).FullName, @"SALsA_Exe\SALsA_Exe.exe");

            var logManager = new LogManager(log);

            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += logManager.p_OutputDataReceived;
            process.ErrorDataReceived += logManager.p_ErrorDataReceived;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = salsaPath;
            process.StartInfo.Arguments = myQueueItem;
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }
        public class LogManager
        { 
            public LogManager(TraceWriter _log)
            {
                this.Log = _log;
            }
            TraceWriter Log;
            public void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                Log.Info(e.Data);
            }
            public void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
            {
                Log.Error(e.Data);
            }
        }
    }
}
