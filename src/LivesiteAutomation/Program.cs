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
        public static void Run(int icm)
        {
            _ = Log.Instance;
            try
            {
                // Initialise singletons;
                _ = Authentication.Instance;
                _ = Authentication.Instance.StorageCredentials;

                _ = ICM.CreateInstance(icm);
                // We do not need to keep the analyzer in memory, for now.
                _ = new Analyzer();

                Utility.TaskManager.Instance.WaitAllTasks();
            }
            catch (Exception ex)
            {
                Log.Instance.Critical("Main failed !");
                Log.Instance.Exception(ex);
                throw ex;
            }
            finally
            {
                Utility.UploadLog();
                Log.Instance.Reload();
            }
        }
    }
}
