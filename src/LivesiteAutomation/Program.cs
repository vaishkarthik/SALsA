using LivesiteAutomation.Kusto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LivesiteAutomation
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
                //throw ex;
            }
            finally
            {
                Utility.UploadLog(icm);

                // How do you feel about memory leak ?
                // We keep all instances so they can be accessed online if need be...
                // SALsA.RemoveInstance(icm);
            }
        }
    }
}
