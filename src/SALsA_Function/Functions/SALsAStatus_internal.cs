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

namespace SALsA.Functions
{
    public static class SALsAStatus_internal
    {
        internal class SALsAReport
        {
            public DateTime StartTime = DateTime.UtcNow;
            public TimeSpan Ellapsed;
            public List<StatusLine> Rows = new List<StatusLine>();

            public class StatusLine
            {
                public int IcmId;
                public string IcmStatus = null;
                public string IcmOwningTeam = null;
                public Nullable<DateTime> IcmCreation = null;
                public string SalsaStatus = null;
                public string SalsaInternalLog = null;
                public string SalsaLogLink = null;
                public Nullable<DateTime> SalsaIngestionTime = null;
            }
        }
        [FunctionName("Internal_SALsAStatus")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "internal/status")] HttpRequestMessage req,
            ILogger log, System.Security.Claims.ClaimsPrincipal claimsPrincipal)
        {
            if (Auth.CheckIdentity(req, log, claimsPrincipal, out HttpResponseMessage err) == false) { return err; };
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            var icmsDic = new List<SALsAReport.StatusLine>();

            var salsaIncident = SALsA.LivesiteAutomation.TableStorage.ListAllEntity();
            var incidents = ICM.GetIncidentsWithId(salsaIncident.Select(x => x.PartitionKey).ToList());
            var report = new SALsAReport();
            if (salsaIncident != null)
            {
                foreach (var run in salsaIncident)
                {
                    if (run.SALsAState == SALsA.General.SALsAState.Ignore.ToString()) continue;

                    SALsAReport.StatusLine line = new SALsAReport.StatusLine
                    {
                        IcmId = int.Parse(run.PartitionKey),
                        SalsaIngestionTime = run.Timestamp.UtcDateTime,
                        SalsaInternalLog = run.Log,
                        SalsaLogLink = run.SALsALog,
                        SalsaStatus = run.SALsAState
                    };

                    if(incidents.ContainsKey(run.PartitionKey))
                    {
                        line.IcmCreation = DateTime.Parse(incidents[run.PartitionKey].CreateDate);
                        line.IcmOwningTeam = incidents[run.PartitionKey].OwningTeamId;
                        line.IcmStatus = incidents[run.PartitionKey].Status;
                    }
                    report.Rows.Add(line);
                }
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true,
                MustRevalidate = true
            };
            watch.Stop();
            report.Ellapsed = watch.Elapsed;
            response.Content = new StringContent(Utility.ObjectToJson(report, true), System.Text.Encoding.UTF8, "application/json");

            return response;
        }
    }
}