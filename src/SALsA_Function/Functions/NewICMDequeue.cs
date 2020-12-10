using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SALsA.General;
using SALsA.LivesiteAutomation;

namespace SALsA_Function.Functions
{
    public static class NewICMDequeue
    {
        [FunctionName("Internal_NewICMDequeue")]
        public static void Run([QueueTrigger("salsaqueue", Connection = "AzureWebJobsStorage")] string myQueueItem, ILogger log)
        {
            try
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
                if (process.ExitCode != 0)
                {
                    throw new Exception("SALSA_Exe exit with error code : " + process.ExitCode.ToString());
                }
            }
            catch (Exception ex)
            {
                TableStorage.AppendEntity(myQueueItem.Split(" ").First(), SALsA.General.SALsAState.UnknownException, "Not available", ex.Message);
                throw ex;
            }
        }
        public class LogManager
        {
            public LogManager(ILogger _log)
            {
                this.Log = _log;
            }
            ILogger Log;
            public void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                Log.LogInformation(e.Data);
            }
            public void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
            {
                Log.LogError(e.Data);
            }
        }
    }
}
