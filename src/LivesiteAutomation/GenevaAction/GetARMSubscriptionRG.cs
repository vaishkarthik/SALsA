using SALsA.LivesiteAutomation.Json2Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation
{
    public partial class GenevaActions
    {
        // TODO : make sovereign cloud available
        public static async Task<String> GetARMSubscriptionRG(int icm, Guid guid, string ressourceGroup)
        {
            SALsA.GetInstance(icm)?.Log.Information("Calling GenevaAction GetARMSubscriptionRG with param Guid:{0}", guid);
            var param = new GenevaOperations.GetARMSubscriptionResources { wellknownsubscriptionid = guid.ToString(), resourcegroupname = ressourceGroup };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            return await new GenevaAction(icm, Constants.GetARMSubscriptionRGExtensionName, Constants.GetARMSubscriptionRGOperationName, actionParam).GetOperationResultOutputAsync(icm);
        }
    }
}
