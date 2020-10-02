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

namespace SALsA_Function
{
    public static class ManualRunIIDGet
    {
        [FunctionName("ManualRunIID_Get")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "manual/ManualRunIID")] HttpRequest req, ExecutionContext context,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function {0} processed a request.", context.FunctionName);

            return Utility.ReturnTemplate(context);
        }
    }
    public static class ManualRunIIDPost
    {
        [FunctionName("ManualRunIID_Post")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "manual/ManualRunIID")] HttpRequest req, ExecutionContext context,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function {0} processed a request.", context.FunctionName);

            return Utility.ManualRun<LivesiteAutomation.ManualRun.ManualRun_ICM>(req);
        }
    }
}
