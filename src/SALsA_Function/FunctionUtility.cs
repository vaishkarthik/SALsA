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
    internal class StatusLine
    {
        public StatusLine() { }
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
        public string IcmStatus = "Unknown";
        public Nullable<DateTime> IcmCreation = null;
        public Nullable<DateTime> _SalsaInternalIngestion = null;

        public static string[] Headers = new string[] { "ICM", "Status", "SALsA Ingested", "Rerun SALsA", "ICM State", "ICM Creation (UTC)" };
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

            var currentDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(Directory.GetParent(currentDir).FullName, "HTMLTemplate");
            filePath = Path.Combine(filePath, String.Format("{0}.html", fileName));
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(System.IO.File.ReadAllText(filePath),
                    System.Text.Encoding.UTF8, "text/html");
            return response;
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
            Color c;
            switch (status.ToLowerInvariant())
            {
                case "holding":
                case "active":
                case "new":
                case "correlating":
                    c = Color.DarkRed;
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
            return String.Format("<span style=\"color: {0}\">{1}</span>", System.Drawing.ColorTranslator.ToHtml(c), owningTeamId);
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
            username.Split('@').First().Trim(), Utility.UrlToHml("access.txt", "https://msazure.visualstudio.com/One/_git/Compute-CPlat-SALsA?path=%2Faccess.txt&version=GBmaster&_a=contents", 14)), System.Text.Encoding.UTF8, "text/html");

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