using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using SALsA.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Azure.Storage.Queues;
using SALsA.LivesiteAutomation;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using System.Drawing;

namespace SALsA.General
{
    public class SALsAReport
    {
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

            public string[] Arrayify()
            {
                try
                {
                    List<string> e = new List<string>();
                    e.Add(FunctionUtility.UrlToHml(String.Format("https://portal.microsofticm.com/imp/v3/incidents/details/{0}/home", IcmId), IcmId.ToString()));
                    e.Add(SalsaInternalLog != null ? FunctionUtility.UrlToHml(SalsaInternalLog, SalsaStatus) : SalsaStatus);
                    var stringSalsaIngestionTime = string.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", SalsaIngestionTime.Value);
                    e.Add(SalsaLogLink != null ? FunctionUtility.UrlToHml(SalsaLogLink, stringSalsaIngestionTime) : stringSalsaIngestionTime);
                    e.Add(FunctionUtility.ReRunButton(IcmId));
                    e.Add(FunctionUtility.ColorICMStatus(IcmOwningTeam, IcmStatus));
                    e.Add(IcmCreation != null ? string.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", IcmCreation) : "N/A");
                    return e.ToArray();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(String.Format("StatusLine.ToArray) of ICM {0} failed due to ex : {1}", IcmId, ex));
                    // Fallback to plaintext
                    List<string> e = new List<string>();
                    e.Add(IcmId.ToString());
                    e.Add(IcmStatus ?? "");
                    e.Add(IcmOwningTeam ?? "");
                    e.Add(IcmCreation?.ToString() ?? "");
                    e.Add(SalsaStatus ?? "");
                    e.Add(SalsaInternalLog ?? "");
                    e.Add(SalsaLogLink ?? "");
                    e.Add(SalsaIngestionTime?.ToString() ?? "");
                    return e.ToArray();
                }

            }
        }
        public string[][] Arrayify()
        {
            List<string[]> e = new List<string[]>();
            var values = Rows.ToList();
            values.Sort((y, x) => {
                int ret = DateTime.Compare(x.SalsaIngestionTime.HasValue ? x.SalsaIngestionTime.Value : new DateTime(0),
                                           y.SalsaIngestionTime.HasValue ? y.SalsaIngestionTime.Value : new DateTime(0));
                return ret != 0 ? ret : DateTime.Compare(x.IcmCreation.HasValue ? x.IcmCreation.Value : new DateTime(0),
                                                         y.IcmCreation.HasValue ? y.IcmCreation.Value : new DateTime(0));
            });
            foreach (var r in values)
            {
                e.Add(r.Arrayify());
            }
            return e.ToArray();
        }
    }

    static class FunctionUtility
    {
        public static HttpResponseMessage ReturnTemplate(HttpRequestMessage req, ExecutionContext context)
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
            fileName = fileName.Replace("_", "");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(GetFileContent(String.Format("{0}.html", fileName)),
                    System.Text.Encoding.UTF8, "text/html");
            return response;
        }

        public static string GetFileContent(string fileName, string subPath = "HTMLTemplate")
        {
            var currentDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(Directory.GetParent(currentDir).FullName, subPath);
            filePath = Path.Combine(filePath, String.Format("{0}", fileName));
            return System.IO.File.ReadAllText(filePath);
        }

        internal static Dictionary<string, string> RequestStreamToDic(HttpRequestMessage req)
        {
            string ret = req.Content.ReadAsStringAsync().Result;
            var parsed = HttpUtility.ParseQueryString(ret);
            var dic = parsed.AllKeys.ToDictionary(k => k, k => parsed[k]);
            return dic;
        }

        internal static HttpResponseMessage ManualRun<T>(HttpRequestMessage req)
        {
            var dic = RequestStreamToDic(req);
            var icmId = int.Parse(dic["icmid"]);
            var obj = Utility.JsonToObject<T>(
                Utility.ObjectToJson(dic));
            return RunIfReadySALsA(req, icmId, obj);
        }

        // TODO : improve on this and do something about the messy one in Utility.List2DToHTML
        internal static string List2DToHTMLWithFilter(string[][] lst, KeyValuePair<string, bool>[] headers)
        {
            using (var sw = new StringWriter())
            {
                sw.WriteLine(@"<table id=""table"">");
                sw.WriteLine(@"<thead>");
                sw.WriteLine(@"<tr>");
                foreach (var e in headers)
                {
                    sw.WriteLine(String.Format("<th {0}>{1}</th>",
                        (e.Value ? "" : @" class="".disable-filter"""), e.Key));
                }
                sw.WriteLine(@"<tr>");
                sw.WriteLine(@"</tr>");
                sw.WriteLine(@"</thead>");
                sw.WriteLine(@"<tbody>");
                foreach (var line in lst)
                {
                    sw.WriteLine(@"<tr>");
                    foreach (var e in line)
                    {
                        sw.WriteLine(String.Format("<td>{0}</td>", e));
                    }
                    sw.WriteLine(@"</tr>");
                }
                sw.WriteLine(@"</tbody");
                sw.WriteLine(@"</table>");
                return sw.ToString();
            }
        }

        internal static HttpResponseMessage RunIfReadySALsA(HttpRequestMessage req, int icm, object manual = null)
        {
            var entity = SALsA.LivesiteAutomation.TableStorage.GetEntity(icm);
            if (entity != null && entity.RowKey == SALsAState.Running.ToString())
            {

                var response = req.CreateResponse(HttpStatusCode.Conflict);
                response.Content = new StringContent($"ICM#{icm} is already running. Please wait for the run to finish then try again.",
                    System.Text.Encoding.UTF8, "text/html");
                return response;
            }
            else
            {
                AddRunToSALsA(icm, manual);
                var response = req.CreateResponse(HttpStatusCode.TemporaryRedirect);
                response.Headers.Location = new Uri("/api/status", UriKind.Relative);
                return response;
            }
        }

        internal static void AddRunToSALsA(int icm, object manual = null)
        {
            var client = new QueueClient(Authentication.Instance.GenevaAutomationConnectionString, Constants.QueueName);
            string queueMessage;
            if (manual != null)
            {
                queueMessage = String.Format("{0} {1} {2}", icm, manual.GetType(), Utility.Base64Encode(Utility.ObjectToJson(manual)));
            }
            else
            {
                queueMessage = icm.ToString();
            }
            TableStorage.AppendEntity(icm, SALsAState.Queued);
            client.SendMessage(Utility.Base64Encode(queueMessage));
        }

        public static string ReRunButton(int icm)
        {
            return String.Format("<form action=\"/api/icm/{0}\"><input type=\"submit\" value=\"Run again\"/></form>", icm);
        }

        internal static void HealthProbe()
        {
            var salsaTableEntity = TableStorage.CleanRecentEntity();
            var client = new QueueClient(Authentication.Instance.GenevaAutomationConnectionString, Constants.QueueName);

            var messagesInQueue = -1;
            try
            { 
                var peek = client.PeekMessages();
                messagesInQueue = peek.Value.Length;
            } catch { }

            if (messagesInQueue == 0)
            {
                foreach (var entity in salsaTableEntity)
                {
                    if(entity.SALsAState == SALsAState.Queued.ToString())
                    {
                        Console.WriteLine("ICM:{0} stuck in Queued state. Will remove and reset.", entity.PartitionKey);
                        TableStorage.RemoveEntity(entity);
                    }
                }
            }

            var allExistingIcms = ICM.GetAllWithTeamsICM(Constants.ICMTeamToTenantLookupTable.Keys.ToList()).value.Select(x => x.Id.ToString()).ToList();
            var allOurIcms = TableStorage.ListAllEntity().Select(x => x.RowKey).ToList();

            foreach (var icm in allExistingIcms)
            {
                if (!allOurIcms.Contains(icm))
                {
                    FunctionUtility.AddRunToSALsA(int.Parse(icm));
                }
            }
        }

        internal static string ColorICMStatus(string owningTeamId, string status)
        {
            if (status == null) status = String.Empty;
            if (string.IsNullOrWhiteSpace(owningTeamId)) owningTeamId = "Unknown - No access";
            Color c;
            switch (status.ToLowerInvariant())
            {
                case "holding":
                case "active":
                case "new":
                case "correlating":
                    c = Color.Red;
                    break;
                case "mitigated":
                case "mitigating":
                    c = Color.Orange;
                    break;
                case "resolved":
                    c = Color.Green;
                    break;
                default:
                    c = Color.Gray;
                    break;
            }
            return ColorICMStatus(owningTeamId, c);
        }

        public static string ColorICMStatus(string owningTeamId, Color c)
        {
            return String.Format(
                "<span style=\"color: {0}\">{1}</span>",
                    System.Drawing.ColorTranslator.ToHtml(c),
                    owningTeamId
             );

        }

        internal static string UrlToHml(string url, string name)
        {
            return String.Format("<a href=\"{0}\">{1}</a>", url, name);
        }
    }

    public static class Auth
    {
        private static HashSet<string> _users = null;
        private static HashSet<string> Users
        {
            get
            {
                if(_users == null)
                {
                    _users = new HashSet<string>();
                    var currentDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    var accessPath = Path.Combine(Directory.GetParent(currentDir).FullName, @"access.txt");
                    foreach (var user in File.ReadAllLines(accessPath))
                    {
                        var u = user.Trim().ToLowerInvariant();
                        if (u.StartsWith(" ") ||
                            u.StartsWith(";") ||
                            u.StartsWith("#") ||
                            u.StartsWith("/") ||
                            u.StartsWith("\\") ||
                            u == "")
                        {
                            continue;
                        }
                        else
                        {
                            _users.Add(u);
                        }
                    }
                }
                return _users;
            }
        }

        public static bool CheckUser(string username)
        {
            return Users.Contains(username.Split('@').First().Trim().ToLowerInvariant());
        }

        public static HttpResponseMessage GenerateErrorForbidden(HttpRequestMessage req, string username)
        {
            var response = req.CreateResponse(HttpStatusCode.Forbidden);
            response.Content = new StringContent(String.Format("<b>Access denied</b><br>You do not have access to this page.<br>Please add your alias (<i> {0} </i>) here : <b>{1}</b> and send a PR.",
            username.Split('@').First().Trim(), Utility.UrlToHml("access.txt", "https://github.com/Azure/SALsA/edit/master/access.txt", 14)), System.Text.Encoding.UTF8, "text/html");

            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true,
                MustRevalidate = true
            };

            return response;
        }

        public static bool CheckIdentity(HttpRequestMessage req, ILogger log, System.Security.Claims.ClaimsPrincipal claimsPrincipal, out HttpResponseMessage err)
        {
            log.LogInformation("Connected Identity : {0}", claimsPrincipal.Identity.Name);
            if(claimsPrincipal.Identity.Name == null && req.RequestUri.Host.ToLowerInvariant() == "localhost")
            {
                // Test mode, running in localhost
                err = null;
                return true;
            }    
            if (!Auth.CheckUser(claimsPrincipal.Identity.Name))
            {
                log.LogWarning("Access Denied for {0}", claimsPrincipal.Identity.Name);
                err = Auth.GenerateErrorForbidden(req, claimsPrincipal.Identity.Name);
                return false;
            }
            else
            { 
                log.LogWarning("Access Granted for {0}", claimsPrincipal.Identity.Name);
                err = null;
                return true;
            }
        }
    }
}