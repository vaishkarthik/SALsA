using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace SALsA_Queue
{
    public static class QueueICM
    {
        [FunctionName("QueueICM")]
        public static void Run([QueueTrigger("salsaqueue")]string myQueueItem, TraceWriter log)
        {
            SALsA.LivesiteAutomation.Program.Run(int.Parse(myQueueItem));
        }
    }
}
