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

        // Base class to inherit from
        public class GetIaaSVMBase
        {
            public string smecrpregion { get; set; }
            public string wellknownsubscriptionid { get; set; }
            public string smeresourcegroupnameparameter { get; set; }
            public string smevmnameparameter { get; set; }
        }
        public class GetVMConsoleSerialLogs : GetIaaSVMBase { }
        public class GetVMConsoleScreenshot : GetIaaSVMBase { }

        public class GetVMModelAndInstanceView : GetIaaSVMBase
        {
            public string smegetvmoptionparameter { get; set; }
        }
        public class InspectIaaSDiskForARMVM
        {
            public string smecrpregion { get; set; }
            public string wellknownsubscriptionid { get; set; }
            public string smeresourcegroupname { get; set; }
            public string smevmname { get; set; }
            public string smelogextractmode { get; set; }
            public string smeskiptostep { get; set; }
            public int smetimeoutinmins { get; set; }
        }
        
    }
}
