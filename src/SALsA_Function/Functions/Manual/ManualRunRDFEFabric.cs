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
    public static class ManualRunRdfeFabricGet
    {
        [FunctionName("Manual_RunRDFEFabric_Get")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manual/ManualRunRdfeFabric")] HttpRequestMessage req, ExecutionContext context,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function {0} processed a request.", context.FunctionName);

            return FunctionUtility.ReturnTemplate(req, context);
        }
    }
    public static class ManualRunRdfeFabricPost
    {
        [FunctionName("Manual_RunRDFEFabric_Post")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manual/ManualRunRdfeFabric")] HttpRequestMessage req, ExecutionContext context,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function {0} processed a request.", context.FunctionName);

            return FunctionUtility.ManualRun<SALsA.LivesiteAutomation.ManualRun.ManualRun_ICM>(req);
        }
    }
}
