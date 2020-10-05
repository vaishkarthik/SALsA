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
using SALsA.General;

namespace SALsA.Functions
{
    public static class ManualRunRDFEFabricGet
    {
        [FunctionName("ManualRunRDFEFabric)Get")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "manual/ManualRunRDFEFabric")] HttpRequest req, ExecutionContext context,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function {0} processed a request.", context.FunctionName);

            return FunctionUtility.ReturnTemplate(context);
        }
    }
    public static class ManualRunRDFEFabricPost
    {
        [FunctionName("ManualRunRDFEFabric_Post")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "manual/ManualRunRDFEFabric")] HttpRequest req, ExecutionContext context,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function {0} processed a request.", context.FunctionName);

            return FunctionUtility.ManualRun<SALsA.LivesiteAutomation.ManualRun.ManualRun_ICM>(req);
        }
    }
}
