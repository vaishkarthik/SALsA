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
                _icm = _IcmId;
                var icmLink = String.Format("https://portal.microsofticm.com/imp/v3/incidents/details/{0}/home", _IcmId);
                IcmId = Utility.UrlToHml(_IcmId, icmLink, 20);
                IcmStatus = _IcmStatus;
                IcmCreation = _IcmCreation;
            }
            public void Update(string _IcmId, string _SalsaStatus, string _SalsALog, Nullable<DateTime> _SalsaLogIngestion = null)
            {
                _icm = _IcmId;
                var icmLink = String.Format("https://portal.microsofticm.com/imp/v3/incidents/details/{0}/home", _IcmId);
                IcmId = Utility.UrlToHml(_IcmId, icmLink, 20);

                SalsaStatus = _SalsaStatus;

                _SalsaInternalIngestion = _SalsaLogIngestion.Value;
                if (_SalsALog.StartsWith("http"))
                {
                    SalsaLogIngestion = Utility.UrlToHml(_SalsaLogIngestion.HasValue ? _SalsaLogIngestion.Value.ToUniversalTime().ToString("s") + "Z" : "HTML", _SalsALog, 20);
                }
                else
                {
                    SalsaLogIngestion = _SalsaLogIngestion.HasValue ? _SalsaLogIngestion.Value.ToString("s") + "Z" : _SalsALog;
                }
            }
            public string[] ToArray()
            {
                return new string[] { IcmId, SalsaStatus, SalsaLogIngestion, FunctionUtility.ReRunButton(int.Parse(_icm)), 
                    IcmStatus, IcmCreation.HasValue ? IcmCreation.Value.ToUniversalTime().ToString("s") + "Z" : "N/A" };
            }
            public string _icm;
            public string IcmId;
            public string SalsaStatus = "N/A";
            public string SalsaLogIngestion = "N/A";
            public string IcmStatus = "Transferred out";
            public Nullable<DateTime> IcmCreation = null;
            public Nullable<DateTime> _SalsaInternalIngestion = null;
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
            lst.Add(new string[] { "ICM", "Status", "SALsA Ingested", "Rerun SALsA", "ICM State", "ICM Creation (UTC)" });
            if(icms != null)
            {
                foreach (var run in icms)
                {
                    if (run.SALsAState == SALsA.General.SALsAState.Ignore.ToString()) continue;

                    StatusLine tuple;
                    if (icmsDic.ContainsKey(int.Parse(run.PartitionKey)))
                    {
                        tuple = icmsDic[int.Parse(run.PartitionKey)];
                    }
                    else
                    {
                        try
                        {
                            var currentIcm = ICM.PopulateICMInfo(int.Parse(run.PartitionKey));
                            tuple = new StatusLine(currentIcm.Id, currentIcm.Status, currentIcm.CreateDate);
                            if (currentIcm.Status != "Resolved") // We do not care about the Owning team if it is closed / resolved
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
                    tuple.Update(run.PartitionKey, status, logPath, run.Timestamp.UtcDateTime);
                    icmsDic[int.Parse(run.PartitionKey)] = tuple;
                }
            }
            var values = icmsDic.Values.ToList();
            values.Sort((y, x) => {
                int ret = DateTime.Compare(x._SalsaInternalIngestion.HasValue ? x._SalsaInternalIngestion.Value : new DateTime(0),
                                           y._SalsaInternalIngestion.HasValue ? y._SalsaInternalIngestion.Value : new DateTime(0));
                return ret != 0 ? ret : DateTime.Compare(x.IcmCreation.HasValue ? x.IcmCreation.Value : new DateTime(0),
                                                         y.IcmCreation.HasValue ? y.IcmCreation.Value : new DateTime(0));
            });
            foreach(var value in values)
            {
                if(value.SalsaLogIngestion == "N/A")
                {
                    try
                    {
                        FunctionUtility.AddRunToSALsA(int.Parse(value._icm));
                    }
                    catch { };
                }
                else if(value.SalsaStatus == "Queued" && value.IcmStatus == "Transferred out" && value.IcmCreation.HasValue == false)
                {
                    value.SalsaStatus = SALsAState.ICMNotAccessible.ToString();
                    TableStorage.AppendEntity(value.IcmId, SALsAState.ICMNotAccessible);
                }    
            }
            lst.AddRange(values.Select(x => x.ToArray()).ToList());

            string result = Utility.List2DToHTML(lst, true) + String.Format("<p style=\"text - align: right\">Page generated at : {0}Z</p>", DateTime.UtcNow.ToString("s"));
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