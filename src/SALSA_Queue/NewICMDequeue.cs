using System;
using System.Diagnostics;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace SALSA_Queue
{
    public static class NewICMDequeue
    {
        [FunctionName("NewICMDequeue")]
        public static void Run([QueueTrigger("salsaqueue", Connection = "")]string myQueueItem, TraceWriter log)
        {
            Process.Start("SALSA_Function.exe", myQueueItem);
        }
    }
}
