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
            int num;
            // Test if input arguments were supplied:
            bool test = int.TryParse(args[0], out num);
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

            var myIcm = new ICM(num);
            myIcm.GetICM();
            var analyzer =  new Analyzer(ref myIcm);
            //myIcm.AddICMDiscussion("SALSAid: " + Log.Instance.UID);
            analyzer.Wait();
            Log.Instance.UploadLog();
        }
    }
}
