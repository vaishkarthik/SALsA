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
using SALsA.General;

namespace SALsA.Functions
{
    public static class Tools
    {

        [FunctionName("Tools")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "manual/Tools")] HttpRequestMessage req, ExecutionContext context,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function {0} processed a request.", context.FunctionName);

            return FunctionUtility.ReturnTemplate(req, context);
        }
    }
}
