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
            // Initialise singletons;
            _ = Log.Instance;
            _ = Authentication.Instance;
            _ = Authentication.Instance.StorageCredentials;

            var myIcm = new ICM("130631034");
            myIcm.GetICM();
            var analyzer =  new Analyzer(ref myIcm);
            myIcm.AddICMDiscussion("Random Guid : " + Guid.NewGuid());
            analyzer.Wait();
        }
    }
}
