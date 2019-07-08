using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    class Program
    {
        static void Main(string[] args)
        {
            _ = Log.Instance;
            try {
                int num = -1;
                // Test if input arguments were supplied:
                if (args.Length <= 0 || !int.TryParse(args[0], out num))
                {
                    throw new ArgumentException("Please enter a valid numeric argument for the ICM. Usage : SALsA.exe <icmId>");
                }

                // Initialise singletons;
                _ = Authentication.Instance;
                _ = Authentication.Instance.StorageCredentials;

                _ = ICM.CreateInstance(num);
                // We do not need to keep the analyzer in memory, for now.
                _ =  new Analyzer();

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
