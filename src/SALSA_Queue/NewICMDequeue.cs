using System;
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
        public static void Run([QueueTrigger("salsaqueue", Connection = "")]string myQueueItem, TraceWriter log)
        {
            var currentDir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process.Start(Path.Combine(currentDir, @"..\SALsA_Exe.exe"), myQueueItem);
        }
    }
}
