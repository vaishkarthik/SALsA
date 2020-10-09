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
    public static class SALsAStatus
    {
        private class StatusLine
        {
            public StatusLine(){}
            public StatusLine(string _IcmId, string _IcmStatus, Nullable<DateTime> _IcmCreation = null)
            {
                IcmId = _IcmId;
                IcmStatus = _IcmStatus;
                IcmCreation = _IcmCreation;
            }
            public void Update(string _IcmId, string _SalsaStatus, string _SalsALog, Nullable<DateTime> _SalsaProcessed = null)
            {
                IcmId = _IcmId;
                SalsaStatus = _SalsaStatus;


                if (_SalsALog.StartsWith("http"))
                {
                    SalsaLog = Utility.UrlToHml(_SalsaProcessed.HasValue ? _SalsaProcessed.Value.ToUniversalTime().ToString("s") : "HTML", _SalsALog, 20);
                }
                else
                {
                    SalsaLog = _SalsaProcessed.HasValue ? _SalsaProcessed.Value.ToString("s") : _SalsALog;
                }
            }
            public string[] ToArray()
            {
                return new string[] { IcmId, SalsaStatus, SalsaLog, IcmStatus, IcmCreation.HasValue ? IcmCreation.Value.ToUniversalTime().ToString("s") : "N/A" };
            }
            public string IcmId;
            public string SalsaStatus = "N/A";
            public string SalsaLog = "N/A";
            public string IcmStatus = "Transferred out";
            public Nullable<DateTime> IcmCreation = null;
        }

        [FunctionName("SALsAStatus")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequestMessage req,
            ILogger log)
        {
            var icmsDic = new Dictionary<int, StatusLine>();
            var allExistingIcms = ICM.GetAllICM();
            foreach(var icm in allExistingIcms.value)
            {
                icmsDic[int.Parse(icm.Id)] = new StatusLine(icm.Id, icm.Status, icm.CreateDate);
            }

            var icms = SALsA.LivesiteAutomation.TableStorage.GetRecentEntity(allExistingIcms.value.Select(x => x.Id).ToArray());
            var lst = new List<string[]>();
            lst.Add(new string[] { "ICM", "Status", "SALsA Ingested", "ICM State", "ICM Creation" });
            foreach (var run in icms)
            {
                if (run.SALsAState == SALsA.General.SALsAState.Ignore.ToString()) continue;
                var icmLink = String.Format("https://portal.microsofticm.com/imp/v3/incidents/details/{0}/home", run.PartitionKey);
                icmLink = Utility.UrlToHml(run.PartitionKey.ToString(), icmLink, 20);

                StatusLine tuple;
                if(icmsDic.ContainsKey(int.Parse(run.PartitionKey)))
                {
                    tuple = icmsDic[int.Parse(run.PartitionKey)];
                }
                else
                {
                    try
                    {
                        var currentIcm = ICM.PopulateICMInfo(int.Parse(run.PartitionKey));
                        tuple = new StatusLine(currentIcm.Id, currentIcm.Status, currentIcm.CreateDate);
                        if(currentIcm.Status != "Resolved") // We do not care about the Owning team if it is closed / resolved
                        { 
                            tuple.IcmStatus = currentIcm.OwningTeamId;
                        }
                    }
                    catch
                    {
                        tuple = new StatusLine();
                    }
                }

                var status = run.SALsAState;
                var logPath = run.Log;
                if (run.Log != null && run.Log.StartsWith("http"))
                {
                    status = Utility.UrlToHml(run.SALsAState, run.SALsALog, 20);
                }
                else
                {
                    logPath = run.SALsAState == SALsAState.Running.ToString() || run.SALsAState == SALsAState.Queued.ToString() ? "Wait..." : "Unavailable";
                }
                tuple.Update(icmLink, status, logPath, run.Timestamp.UtcDateTime);
                icmsDic[int.Parse(run.PartitionKey)] = tuple;
            }
            foreach(var tuple in icmsDic.Values.ToList())
            {
                lst.Add(tuple.ToArray());
            }
            string result = Utility.List2DToHTML(lst, true);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true,
                MustRevalidate = true
            };
            response.Content = new StringContent(result, System.Text.Encoding.UTF8, "text/html");

            return response;
        }
    }
}