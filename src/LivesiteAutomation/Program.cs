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
            // Test if input arguments were supplied:
            bool test = int.TryParse(args[0], out int num);
            if (args.Length >= 0 && test == false)
            {
                System.Console.WriteLine("Please enter a valid numeric argument for the ICM.");
                System.Console.WriteLine("Usage: SALsA <num>");
                System.Environment.Exit(-1);
            }

            // Initialise singletons;
            _ = Log.Instance;
            _ = Authentication.Instance;
            _ = Authentication.Instance.StorageCredentials;

            ICM.CreateInstance(num);
            // We do not need to keep the analyzer in memory, for now.
            _ =  new Analyzer();

            Utility.TaskManager.Instance.WaitAllTasks();
            Utility.UploadLog();
        }
    }
}
