using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialise singletons;
            _ = Log.Instance;
            _ = Authentication.Instance;
            _ = Authentication.Instance.StorageCredentials;

            var myIcm = new ICM("129901901");
            myIcm.GetICM();
            var analyzer =  new Analyzer(ref myIcm);
            myIcm.GetICMDiscussion();
            myIcm.AddICMDiscussion("Jedi : Hello from SALsA");
            analyzer.task.Wait();
        }
    }
}
