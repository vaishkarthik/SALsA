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
    public static class HealthProbe
    {
        [FunctionName("Internal_HealthProbe")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "healthprobe")] HttpRequestMessage req,
            ILogger log, System.Security.Claims.ClaimsPrincipal claimsPrincipal)
        {
            var allExistingIcms = ICM.GetAllICM().value.Select(x => x.Id.ToString() ).ToList();
            var allOurIcms = TableStorage.ListAllEntity().Select(x => x.RowKey).ToList();

            foreach (var icm in allExistingIcms)
            {
                if(!allOurIcms.Contains(icm))
                {
                    FunctionUtility.AddRunToSALsA(int.Parse(icm));
                }
            }
            var response = req.CreateResponse(HttpStatusCode.OK);
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
