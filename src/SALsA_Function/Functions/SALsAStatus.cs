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
using Microsoft.Spatial;
using System.Drawing;
using System.Diagnostics;

namespace SALsA.Functions
{
    public static class SALsAStatus
    {
        // Kvp of Header name, and should it be filter(able)
        public static KeyValuePair<string, bool>[] StatusHeaders = new KeyValuePair<string, bool>[]
            {
            new KeyValuePair<string, bool>("ICM", false),
            new KeyValuePair<string, bool>("Status", true),
            new KeyValuePair<string, bool>("SALsA Ingested", false),
            new KeyValuePair<string, bool>("Rerun SALsA", false),
            new KeyValuePair<string, bool>("ICM State", true),
            new KeyValuePair<string, bool>("ICM Creation (UTC)", false)
            };
    
        [FunctionName("Public_SALsAStatus")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequestMessage req,
            ILogger log, System.Security.Claims.ClaimsPrincipal claimsPrincipal)
        {
            if (Auth.CheckIdentity(req, log, claimsPrincipal, out HttpResponseMessage err) == false) { return err; };
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            var report = SALsAStatus_internal.GenerateStatusReport();

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true,
                MustRevalidate = true
            };
            string table   = FunctionUtility.List2DToHTMLWithFilter(report.Arrayify(), StatusHeaders);
            var table_js   = FunctionUtility.GetFileContent("excel-bootstrap-table-filter-bundle.js", Path.Join("HTMLTemplate", "excel-bootstrap-table-filter-bundle"));
            var table_css  = FunctionUtility.GetFileContent("excel-bootstrap-table-filter-style.css", Path.Join("HTMLTemplate", "excel-bootstrap-table-filter-bundle"));
            var table_html = FunctionUtility.GetFileContent("SALsAStatus.html");

            var ret = table_html.Replace("__TABLE_INPUT_PLACEHOLDER__", table)
                      .Replace("__excel-bootstrap-table-filter-bundle.js__", table_js)
                      .Replace("__excel-bootstrap-table-filter-style.css__", table_css)
                      .Replace("__GENERATED_TIMESTAMP__", string.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", DateTime.UtcNow));
            watch.Stop();
            response.Content = new StringContent(ret.Replace("__LOADDING_TIME__", watch.Elapsed.TotalSeconds.ToString()), System.Text.Encoding.UTF8, "text/html");

            return response;
        }
    }
}