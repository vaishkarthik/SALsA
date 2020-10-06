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

namespace SALsA.Functions
{
    public static class NewICMGet
    {
        [FunctionName("NewICM")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "icm/{id:int}")] HttpRequest req, int id,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed for NewICM Get: {0}", id);

            return FunctionUtility.RunIfReadySALsA(id);
        }
    }
}