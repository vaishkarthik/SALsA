using SALsA.General;
using SALsA.LivesiteAutomation.ManualRun;
using System;
using System.ComponentModel;

namespace SALsA.LivesiteAutomation
{
    public class Program
    {
        // For when compiling as an exe
        static void Main(string[] args)
        {
            // Test if input arguments were supplied:
            if (args.Length == 1 && int.TryParse(args[0], out int num))
            {
                Run(num);
            }
            else if (args.Length == 3 && int.TryParse(args[0], out int num2))
            {
                string context = Utility.Base64Decode(args[2]);
                object obj;
                if (string.Equals(args[1], typeof(ManualRun_RDFE_Fabric).FullName, StringComparison.OrdinalIgnoreCase))
                {
                    obj = Utility.JsonToObject<ManualRun_RDFE_Fabric>(context);
                }
                else if (string.Equals(args[1], typeof(ManualRun_RDFE_Tenant).FullName, StringComparison.OrdinalIgnoreCase))
                {
                    obj = Utility.JsonToObject<ManualRun_RDFE_Tenant>(context);
                }
                else if (string.Equals(args[1], typeof(ManualRun_IID).FullName, StringComparison.OrdinalIgnoreCase))
                {
                    obj = Utility.JsonToObject<ManualRun_IID>(context);
                }
                else if (string.Equals(args[1], typeof(ManualRun_ICM).FullName, StringComparison.OrdinalIgnoreCase))
                {
                    obj = Utility.JsonToObject<ManualRun_ICM>(context);
                }
                else
                {
                    obj = null;
                }
                Run(num2, obj);
            }
            else
            {
                throw new ArgumentException("Please enter a valid numeric argument for the ICM. Usage : SALsA.exe <icmId> [<manualType> <manualParamsBase64>]");
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
                Log.Critical("Main failed !");
                Log.Exception(ex);

                SALsA.GetInstance(icm)?.TaskManager.WaitAllTasks();

                try
                {
                    Log.Information("ICM State : {0}", Utility.ObjectToJson(SALsA.GetInstance(icm).ICM));
                }
                catch { }
                try
                {
                    Log.Information("ICM SALsA : {0}", Utility.ObjectToJson(SALsA.GetInstance(icm)));
                }
                catch { }
                //throw ex;
            }
            finally
            {
                SALsA.GetInstance(icm)?.ICM.EmptyMessageQueue();
                if(SALsA.GetInstance(icm)?.State == SALsAState.Running)
                {
                    if(SALsA.GetInstance(icm)?.ICM?.SAS != null)
                    { 
                        SALsA.GetInstance(icm).State = SALsAState.Done;
                    }
                    else
                    {
                        SALsA.GetInstance(icm).State = SALsAState.UnknownException;
                    }
                }
                BlobStorageUtility.UploadLog(icm);
                SALsA.GetInstance(icm)?.RefreshTable();
            }
        }

    }
}
