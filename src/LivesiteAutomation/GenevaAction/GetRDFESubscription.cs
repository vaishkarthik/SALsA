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
        public static Task<String> GetRDFESubscription(int icm, Guid guid)
        {
            SALsA.GetInstance(icm)?.Log.Information("Calling GenevaAction GetRDFESubscription with guid:{0}", guid);
            var param = new GenevaOperations.GetRDFESubscriptionResources
            {
                wellknownsubscriptionid = guid.ToString(),
                detaillevel = Constants.GetRDFESubscriptionDetailLevel
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            return new GenevaAction(icm, Constants.GetRDFESubscriptionExtensionName, Constants.GetRDFESubscriptionOperationName, actionParam).GetOperationResultOutputAsync(icm);
        }
    }
}
