using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation.Json2Class
{
    class ARMDeployment
    {
        public string subscriptions { get; set; }
        public string resourceGroups { get; set; }
        public string location { get; set; }
        public string name { get; set; }
        public List<string> extensions { get; set; } = new List<string>();

        // Pseudo unique class attribute so we can search it later
        public override int GetHashCode()
        {
            return (subscriptions + resourceGroups + location).GetHashCode();
        }
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
        }
        public List<Value> value { get; set; }
    }

    class ARMSubscription
    {
        public List<ARMDeployment> deployments { get; set; } = new List<ARMDeployment>();
    }
}