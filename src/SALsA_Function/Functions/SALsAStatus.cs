using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using SALsA.General;
using System.Net.Http.Headers;

namespace SALsA.Functions
{
    public static class SALsAStatus
    {
        [FunctionName("SALsAStatus")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "status")] HttpRequestMessage req,
            ILogger log)
        {
            // TODO remvoe this from the API and have it in the MVC part
            var icms = SALsA.LivesiteAutomation.TableStorage.GetRecentEntity();
            var lst = new List<string[]>();
            lst.Add(new string[] { "ICM", "Status", "Log" });
            foreach (var icm in icms)
            {
                if (icm.SALsAState == SALsA.General.SALsAState.Ignore.ToString()) continue;
                var icmLink = String.Format("https://portal.microsofticm.com/imp/v3/incidents/details/{0}/home", icm.PartitionKey);
                icmLink = Utility.UrlToHml(icm.PartitionKey.ToString(), icmLink, 20);

                var status = icm.SALsAState;

                var logPath = icm.Log;
                logPath = icm.SALsAState == SALsAState.Running.ToString() || icm.SALsAState == SALsAState.Queued.ToString() ? "Wait..." : "Unavailable";
                if (icm.Log.StartsWith("http"))
                {
                    status = Utility.UrlToHml(icm.SALsAState, icm.SALsALog, 20);
                    logPath = Utility.UrlToHml("HTML", logPath, 20);
                }

                lst.Add(new string[] { icmLink, status, logPath });
            }
            string result = Utility.List2DToHTML(lst, true);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true,
                MustRevalidate = true
            };
            response.Content = new StringContent(result, System.Text.Encoding.UTF8, "text/html");

            return response;
        }
    }
}