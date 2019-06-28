using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation.Json2Class
{
    public class GenevaOperations
    {
        public class GetARMSubscriptionResources
        {
            public string wellknownsubscriptionid { get; set; }
        }
        public class GetRDFESubscriptionResources
        {
            public string wellknownsubscriptionid { get; set; }
            public string detaillevel { get; set; }
        }
    }
}
