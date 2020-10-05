using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation.Json2Class
{
    public class ARMDeployment
    {
        public string Subscriptions { get; set; }
        public string ResourceGroups { get; set; }
        public string Location { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public List<string> Extensions { get; set; } = new List<string>();

        public override string ToString() { return Utility.ObjectToJson(this, true); }
    }
    class ARMSubscriptionRaw
    {
        public class Sku
        {
            public string name { get; set; }
            public string tier { get; set; }
            public int? capacity { get; set; }
            public string family { get; set; }
            public string size { get; set; }
        }

        public class Value
        {
            public DateTime changedTime { get; set; }
            public DateTime createdTime { get; set; }
            public string id { get; set; }
            public string location { get; set; }
            public string name { get; set; }
            public Sku sku { get; set; }
            public dynamic tags { get; set; }
            public string type { get; set; }
            public string kind { get; set; }

            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }
        public List<Value> value { get; set; }
    }

    class ARMSubscription
    {
        public List<ARMDeployment> deployments { get; set; } = new List<ARMDeployment>();
    }
}