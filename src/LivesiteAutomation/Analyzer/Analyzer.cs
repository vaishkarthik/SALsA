using LivesiteAutomation.Json2Class;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LivesiteAutomation
{
    public partial class Analyzer
    {
        public Guid SubscriptionId { get; private set; }
        public string ResourceGroupName { get; private set; }
        public string VMName { get; private set; }
        public DateTime StartTime { get; private set; }
        public Analyzer(ref ICM icm)
        {
            (SubscriptionId, ResourceGroupName, VMName, StartTime) = AnalyzeICM(icm.CurrentICM);
            Log.Instance.Send("{0}", Utility.ObjectToJson(this, true));

            var arm  = AnalyzeARMSubscription(SubscriptionId);
            var rdfe = AnalyzeRDFESubscription(SubscriptionId);
        }
    }
}
