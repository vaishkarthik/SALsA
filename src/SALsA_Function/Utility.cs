using LivesiteAutomation.ManualRun;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;

namespace SALsA_Function
{
    static class Utility
    {
        public static HttpResponseMessage ReturnTemplate(ExecutionContext context)
        {
            // Filename is expected to be ex: ManualRunICM.html, but function name is ManualRunICM_Get,
            // so we need to remove those.
            var fileName = context.FunctionName;
            if(fileName.ToLowerInvariant().EndsWith("_get"))
            {
                fileName = fileName.Substring(0, fileName.Length - 4);
            }
            else if(fileName.ToLowerInvariant().EndsWith("_post"))
            {
                fileName = fileName.Substring(0, fileName.Length - 5);
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(System.IO.File.ReadAllText(
                        System.IO.Path.Combine(context.FunctionDirectory,
                        String.Format("../HTMLTemplate/{0}.html", fileName))),
                    System.Text.Encoding.UTF8, "text/html");
            return response;
        }

        internal static Dictionary<string, string> RequestStreamToDic(HttpRequest req)
        {
            string ret = req.ReadAsStringAsync().Result;
            var parsed = HttpUtility.ParseQueryString(ret);
            var dic = parsed.AllKeys.ToDictionary(k => k, k => parsed[k]);
            return dic;
        }

        internal static IActionResult ManualRun<T>(HttpRequest req)
        {
            var dic = RequestStreamToDic(req);
            var icmId = dic["icmid"];
            var obj = LivesiteAutomation.Utility.JsonToObject<T>(
                LivesiteAutomation.Utility.ObjectToJson(dic));

            // TODO

            return new ConflictObjectResult($"ICM#{icmId} is already running. Please wait for the run to finish then try again.");
        }
    }
}