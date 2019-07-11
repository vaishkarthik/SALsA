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
        public static async Task<String> GetARMSubscription(int icm, Guid guid)
        {
            SALsA.GetInstance(icm)?.Log.Information("Calling GenevaAction GetARMSubscription with param Guid:{0}", guid);

            var param = new GenevaOperations.GetARMSubscriptionResources { wellknownsubscriptionid = guid.ToString() };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var result = await new GenevaAction(icm, Constants.GetARMSubscriptionExtensionName, Constants.GetARMSubscriptionOperationName, actionParam).GetOperationResultOutputAsync(icm);
            return await FindNextIfAvailable(icm, guid, result);
        }

        private static async Task<String> GetARMSubscription(int icm, Guid guid, string token)
        {
            SALsA.GetInstance(icm)?.Log.Information("Calling GenevaAction GetARMSubscription with param Guid:{0}", guid);

            var param = new GenevaOperations.GetARMSubscriptionResources { wellknownsubscriptionid = guid.ToString(), skiptoken = token };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            return await new GenevaAction(icm, Constants.GetARMSubscriptionExtensionName, Constants.GetARMSubscriptionOperationNameWithToken, actionParam).GetOperationResultOutputAsync(icm);
        }

        private async static Task<string> FindNextIfAvailable(int icm, Guid guid, string resultMessage)
        {
            try
            {
                dynamic resultJson = Utility.JsonToObject<dynamic>(resultMessage);
                string nextLink = resultJson.nextLink;
                string uri = Uri.UnescapeDataString(nextLink);
                string token = uri.Split(new[] { '?' }, 1).Last().Split('&').Where(x => x.Split('=')[0].Contains("skiptoken")).FirstOrDefault().Split('=').Last();
                SALsA.GetInstance(icm)?.Log.Information("<{0}: {1}> nextLink detected Token : {2}", Constants.GetARMSubscriptionExtensionName, Constants.GetARMSubscriptionOperationName, token);
                string nextResult = await GetARMSubscription(icm, guid, token);

                dynamic nextResultJson = Utility.JsonToObject<dynamic>(nextResult);
                nextResultJson.value = new Newtonsoft.Json.Linq.JArray(new[] { (Newtonsoft.Json.Linq.JArray)resultJson.value,
                                                                               (Newtonsoft.Json.Linq.JArray)nextResultJson.value }.SelectMany(arr => arr));
                return await FindNextIfAvailable(icm, guid, Utility.ObjectToJson(nextResultJson));
            }
            catch
            {
                return resultMessage;
            }
        }
    }


}
