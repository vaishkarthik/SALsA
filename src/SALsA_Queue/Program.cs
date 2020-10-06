using SALsA.General;
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace SALsA.LivesiteAutomation
{
    public class Program
    {
        // For when compiling as an exe
        static void Main(string[] args)
        {
            // Test if input arguments were supplied:
            if (args.Length >= 1 && int.TryParse(args[0], out int num))
            {
                Run(num);
            }
            else
            {
                throw new ArgumentException("Please enter a valid numeric argument for the ICM. Usage : SALsA.exe <icmId>");
            }
        }
        public static void Run(int icm, object manualRun = null)
        {
            try
            {
                SALsA.AddInstance(icm);
                // We do not need to keep the analyzer in memory, for now.
                _ = manualRun == null ? new Analyzer(icm) : new Analyzer(icm, manualRun);

                SALsA.GetInstance(icm)?.TaskManager.WaitAllTasks();
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(icm)?.Log.Critical("Main failed !");
                SALsA.GetInstance(icm)?.Log.Exception(ex);

                SALsA.GetInstance(icm)?.TaskManager.WaitAllTasks();
                //throw ex;
            }
            finally
            {
                SALsA.GetInstance(icm)?.ICM.EmptyMessageQueue();
                if(SALsA.GetInstance(icm)?.State == SALsAState.Running)
                {
                    SALsA.GetInstance(icm).State = SALsAState.Done;
                }
                BlobStorageUtility.UploadLog(icm);
                SALsA.GetInstance(icm)?.RefreshTable();
            }
        }

    }
    public static class QueueICM
    {
        [FunctionName("QueueICM")]
        public static void Run([QueueTrigger("salsaqueue")] string myQueueItem, TraceWriter log)
        {
            Program.Run(int.Parse(myQueueItem));
        }
    }
}
