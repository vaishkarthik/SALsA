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
using SALsA.LivesiteAutomation;
using System.Linq;

namespace SALsA.Functions
{
    public static class ICMStatus
    {
        [FunctionName("Public_ICMStatus")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status/{id:int}")] HttpRequestMessage req, int id,
            ILogger log, System.Security.Claims.ClaimsPrincipal claimsPrincipal)
        {
            log.LogInformation("Connected Identity : {0}", claimsPrincipal.Identity.Name);
            if(!Auth.CheckUser(claimsPrincipal.Identity.Name))
            {
                log.LogWarning("Access denied");
                return Auth.GenerateErrorForbidden(req, claimsPrincipal.Identity.Name);
            }
            log.LogWarning("Access Granted");

            var icm = TableStorage.ListAllEntity().Where(x => x.RowKey == id.ToString()).ToList();
            HttpResponseMessage response;
            if (icm.Count == 0)
            {
                log.LogInformation("No ICM found, will redirect to /api/status");
                FunctionUtility.AddRunToSALsA(id);
                response = req.CreateResponse(HttpStatusCode.TemporaryRedirect);
                response.Headers.Location = new Uri("/api/status", UriKind.Relative);
            }
            else if (icm.FirstOrDefault().Log == null)
            {
                log.LogInformation("ICM found, but no logs. Will return 404");
                response = req.CreateResponse(HttpStatusCode.NotFound);
                response.Content = new StringContent(String.Format("<b>SALsA Logs not available.</b><br>Status : <b>{0}</b>",
                    Utility.UrlToHml(icm.FirstOrDefault().SALsAState, icm.FirstOrDefault().SALsALog, 14)), System.Text.Encoding.UTF8, "text/html");
            }
            else
            {
                log.LogInformation("ICM found, logs found. Redirecting to logs");
                response = req.CreateResponse(HttpStatusCode.TemporaryRedirect);
                response.Headers.Location = new Uri(icm.FirstOrDefault().Log, UriKind.Absolute);
            }

            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true,
                MustRevalidate = true
            };
            return response;
        }
    }
}
