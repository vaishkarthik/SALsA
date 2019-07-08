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
        public static Task<String> GetARMSubscription(int icm, Guid guid)
        {
            SALsA.GetInstance(icm)?.Log.Information("Calling GenevaAction GetARMSubscription with param Guid:{0}", guid);

            var param = new GenevaOperations.GetARMSubscriptionResources { wellknownsubscriptionid = guid.ToString() };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            return new GenevaAction(icm, Constants.GetARMSubscriptionExtensionName, Constants.GetARMSubscriptionOperationName, actionParam).GetOperationResultOutputAsync(icm);
        }
    }
}
