using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SALsA.General;
using System.Net.Http.Headers;
using System.Net.Http;

namespace SALsA.Functions
{
    public static class NewICMGet
    {
        [FunctionName("NewICMQueue")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "icm/{id:int}")] HttpRequestMessage req, int id,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed for NewICM Get: {0}", id);

            var response = FunctionUtility.RunIfReadySALsA(req, id);
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