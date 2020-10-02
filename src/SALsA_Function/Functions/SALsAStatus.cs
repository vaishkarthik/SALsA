using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SALsA_Function
{
    public static class SALsAStatus
    {
        [FunctionName("SALsAStatus")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "status")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            return new OkObjectResult(null);
        }
    }
}


/*


        // GET: api/ICMStatus
        public HttpResponseMessage Get()
        {
            // TODO remvoe this from the API and have it in the MVC part
            var icms = LivesiteAutomation.SALsA.ListInstances();
            var lst = new List<string[]>();
            lst.Add(new string[] { "ICM", "Status", "Log" });
            foreach (var icm in icms)
            {
                if (SALsA.GetInstance(icm).State == SALsA.State.Ignore) continue;
                var icmLink = String.Format("https://portal.microsofticm.com/imp/v3/incidents/details/{0}/home", icm);
                icmLink = Utility.UrlToHml(icm.ToString(), icmLink, 20);

                var status = String.Format("/api/icm/status/{0}", icm);
                status = Utility.UrlToHml(SALsA.GetInstance(icm).State.ToString(), status, 20);

                var log = SALsA.GetInstance(icm).ICM.SAS;
                log = SALsA.GetInstance(icm).State == SALsA.State.Running ? "Wait..." : LivesiteAutomation.Utility.UrlToHml("HTML", log, 20);

                lst.Add(new string[] { icmLink, status, log });
            }
            string result = LivesiteAutomation.Utility.List2DToHTML(lst, true);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(result, System.Text.Encoding.UTF8, "text/html");

            return response;
        }

        // GET: api/ICMStatus/5
        public HttpResponseMessage Get(int id)
        {
            string content;
            try
            {
                var filePath = LivesiteAutomation.SALsA.GetInstance(id)?.Log?.LogFullPath;
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
            }
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(content, System.Text.Encoding.UTF8, "text/plain");
            return response;
        }

 */