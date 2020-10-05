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

namespace SALsA.Functions
{
    public static class SALsAStatus
    {
        [FunctionName("SALsAStatus")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "status")] HttpRequest req,
            ILogger log)
        {
            // TODO remvoe this from the API and have it in the MVC part
            var icms = SALsA.LivesiteAutomation.TableStorage.GetRecentEntity();
            var lst = new List<string[]>();
            lst.Add(new string[] { "ICM", "Status", "Log" });
            foreach (var icm in icms)
            {
                if (icm.RowKey == SALsA.LivesiteAutomation.SALsA.State.Ignore.ToString()) continue;
                var icmLink = String.Format("https://portal.microsofticm.com/imp/v3/incidents/details/{0}/home", icm);
                icmLink = Utility.UrlToHml(icm.ToString(), icmLink, 20);

                var status = String.Format("/api/icm/status/{0}", icm);
                status = Utility.UrlToHml(icm.RowKey.ToString(), status, 20);

                var logPath = icm.Log;
                logPath = icm.RowKey == SALsA.LivesiteAutomation.SALsA.State.Running.ToString() ? "Wait..." : Utility.UrlToHml("HTML", logPath, 20);

                lst.Add(new string[] { icmLink, status, logPath });
            }
            string result = Utility.List2DToHTML(lst, true);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(result, System.Text.Encoding.UTF8, "text/html");

            return response;
        }
    }
    public static class SALsAStatusIcm
    {
        [FunctionName("SALsAStatusIcm")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "status/{id:int}")] HttpRequest req, int id,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request for SALsAStatusIcm with id: {id}");
            string content;
            try
            {
                var filePath = SALsA.LivesiteAutomation.SALsA.GetInstance(id)?.Log?.LogFullPath;
                log.LogInformation($"SALsAStatusIcm LogFullPath: {filePath}");
                using (FileStream fileStream = new FileStream(
                        filePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite))
                {
                    using (StreamReader streamReader = new StreamReader(fileStream))
                    {
                        content = streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                content = ex.ToString();
                log.LogInformation($"SALsAStatusIcm Exception: {ex}");
            }
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(content, System.Text.Encoding.UTF8, "text/plain");
            return response;
        }
    }
}