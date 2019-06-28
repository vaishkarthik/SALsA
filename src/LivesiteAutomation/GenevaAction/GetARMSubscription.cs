using LivesiteAutomation.Json2Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    public partial class GenevaActions
    {
        // TODO : make sovereign cloud available
        public static Task<String> GetARMSubscription(Guid guid)
        {
            var param = new GenevaOperations.GetSubscriptionResources { wellknownsubscriptionid = guid.ToString() };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            return new GenevaAction(Constants.GetARMSubscriptionExtensionName, Constants.GetARMSubscriptionOperationName, actionParam).GetOperationResultOutputAsync();
        }
    }
}
