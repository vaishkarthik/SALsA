using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SALsA.General;

namespace SALsA.General
{
    static class FunctionUtility
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
            var icmId = int.Parse(dic["icmid"]);
            var obj = Utility.JsonToObject<T>(
                Utility.ObjectToJson(dic));
            return RunIfReadySALsA(icmId, obj);
        }

        internal static IActionResult RunIfReadySALsA(int icm, object manual = null)
        {
            var entity = SALsA.LivesiteAutomation.TableStorage.GetEntity(icm);
            if (entity != null && entity.RowKey == SALsA.LivesiteAutomation.SALsA.State.Running.ToString())
                return new ConflictObjectResult($"ICM#{icm} is already running. Please wait for the run to finish then try again.");
            else
            {
                AddRunToSALsA(icm, manual);
                return new RedirectResult("/api/status", true);
            }
        }

        internal static void AddRunToSALsA(int icm, object manual = null)
        {
            // TODO use queue instead of thread ?
            new Task(() => SALsA.LivesiteAutomation.Program.Run(icm, manual)).Start();
        }
    }
}