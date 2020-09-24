using LivesiteAutomation.Commons;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    public class ICM
    {
        public int Id { get; private set; }
        public string SAS { get; private set; }
        public Incident CurrentICM { get; private set; }
        public List<Incident.DescriptionEntry> DescriptionEntries { get; private set; }
        private static HttpClient client = null;
        private ConcurrentBag<string> MessageQueue = new ConcurrentBag<string>();

        public bool AddICMDiscussion(string entry, bool htmlfy = true)
        {
            SALsA.GetInstance(Id)?.Log.Verbose("Adding to ICM String {0}", entry);
            if (htmlfy)
            {
                try
                {
                    entry = Utility.EncodeHtml(entry);
                }
                catch (Exception ex)
                {
                    SALsA.GetInstance(Id)?.Log.Warning("Failed to html encode {0}, will use raw input", entry);
                    SALsA.GetInstance(Id)?.Log.Exception(ex);
                }
            }
            /*
            // This is not used anymore since we uplaod to an HTML page instead
            if (repeat == false)
            {
                if (DescriptionEntries == null)
                {
                    GetICMDiscussion();
                }
                foreach (var de in DescriptionEntries)
                {
                    if (de.SubmittedBy == Constants.ICMIdentityName && Utility.DecodeHtml(de.Text).CompareTo(Utility.DecodeHtml(entry)) == 0)
                    {
                        SALsA.GetInstance(Id)?.Log.Verbose("Did not add entry to ICM since already sent", this.Id);
                        return false;
                    }
                }
            }
            */
            try
            {
                MessageQueue.Add(entry);
                return true;
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(Id)?.Log.Error("Failed to add discussion element to ICM {0}", this.Id);
                SALsA.GetInstance(Id)?.Log.Exception(ex);
                return false;
            }
        }

        public void EmptyMessageQueue()
        {
            StringBuilder entry = new StringBuilder("");
            string reason = null;
            try
            {
                var message = Utility.GenerateICMHTMLPage(Id, MessageQueue.ToArray());
                MessageQueue = new ConcurrentBag<string>(); // Dispose of our current one
                SAS = Utility.UploadICMRun(Id, message);
                message = Utility.UrlToHml(String.Format("SALsA Logs {0}",
                    DateTime.ParseExact(SALsA.GetInstance(Id)?.Log.StartTime, "yyMMddTHHmmss", null)
                        .ToString("yyyy-MM-ddTHH:mm:ssZ")), SAS);
                if (message == null) throw new ArgumentNullException("Message is null, please verify run log");
                var body = new StringContent(Utility.ObjectToJson(new Incident.DescriptionPost(message)));
                body.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = Client.PatchAsync(BuildUri(this.Id), body).Result;
                reason = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
                SALsA.GetInstance(Id)?.Log.Verbose("Got response for ICM {0}", this.Id);
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(Id).State = SALsA.State.UnknownException;
                SALsA.GetInstance(Id)?.Log.Error("Failed to add discussion element to ICM {0}. Reason : ", this.Id, reason);
                SALsA.GetInstance(Id)?.Log.Exception(ex);
            }
        }

        public ICM(int icmId)
        {
            this.Id = icmId;
            PopulateICMInfo();
        }

        private void PopulateICMInfo()
        {
            try
            {
                var response = Client.GetAsync(BuildUri(this.Id)).Result;
                response.EnsureSuccessStatusCode();
                SALsA.GetInstance(Id)?.Log.Verbose("Got response for IMC {0}", this.Id);

                CurrentICM = Utility.JsonToObject<Incident>(ReadResponseBody(response));
                SALsA.GetInstance(Id)?.Log.Verbose(CurrentICM);
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(Id)?.Log.Error("Failed to get ICM {0}", this.Id);
                SALsA.GetInstance(Id)?.Log.Exception(ex);
            }
        }

        public void TransferICM(string owningTeam)
        {
            try
            {
                var body = new StringContent(Utility.ObjectToJson(new Incident.DescriptionPost(owningTeam)));
                body.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = Client.PostAsync(BuildUri(this.Id, Constants.ICMTrnasferIncidentSuffix), body).Result;
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(Id)?.Log.Error("Failed to transfer ICM {0}", Id);
                SALsA.GetInstance(Id)?.Log.Exception(ex);
            }
        }

        private void GetICMDiscussion()
        {
            try
            {
                var response = Client.GetAsync(BuildUri(this.Id, Constants.ICMDescriptionEntriesSuffix)).Result;
                response.EnsureSuccessStatusCode();
                Dictionary<string, object> de = Utility.JsonToObject<Dictionary<string, object>>(ReadResponseBody(response));

                DescriptionEntries = ((JArray)de["value"]).Select(x => new Incident.DescriptionEntry
                {
                    DescriptionEntryId = (string)x["DescriptionEntryId"],
                    SubmittedBy = (string)x["SubmittedBy"],
                    Cause = (string)x["Cause"],
                    SubmitDate = Convert.ToDateTime((string)x["SubmitDate"]),
                    Text = (string)x["Text"]
                }).ToList();

            }
            catch (Exception ex)
            {
                SALsA.GetInstance(Id)?.Log.Error("Failed to get discussion entries for ICM {0}", Id);
                SALsA.GetInstance(Id)?.Log.Exception(ex);
            }
        }

        private static Uri BuildUri(int id, string suffix = "")
        {
            return new Uri(String.Format("{0}({1}){2}", Constants.ICMRelativeBaseAPIUri, id, suffix), UriKind.Relative);
        }

        static internal HttpClient Client
        {
            get
            {
                if (client == null)
                {
                    var handler = new HttpClientHandler();
                    handler.ClientCertificates.Add(Authentication.Instance.Cert);
                    handler.PreAuthenticate = true;
                    handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

                    client = new HttpClient(handler);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.BaseAddress = new Uri(Constants.ICMBaseUri);
                }
                return client;
            }
        }

        private static string ReadResponseBody(HttpResponseMessage response)
        {
            return response.Content.ReadAsStringAsync().Result;
        }

        public static string GetCustomField(int icm, string lookup)
        {
            try
            {
                foreach (var fields in SALsA.GetInstance(icm).ICM.CurrentICM.CustomFieldGroups)
                {
                    var sid = fields.CustomFields.Find(x => x.Name == lookup);
                    if (sid != null && sid.Value != "")
                    {
                        return sid.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(icm)?.Log.Error("Failed to find a valid value for <{0}> in ICM : {1}", lookup, icm);
                SALsA.GetInstance(icm)?.Log.Exception(ex);
            }
            return null;
        }

        public static bool CheckIfICMExists(int icm)
        {
            try
            {
                var response = Client.GetAsync(BuildUri(icm)).Result;
                response.EnsureSuccessStatusCode();
                SALsA.GetInstance(icm)?.Log.Verbose("Got response for IMC {0}", icm);

                var currentICM = Utility.JsonToObject<Incident>(ReadResponseBody(response));
                SALsA.GetInstance(icm)?.Log.Verbose(currentICM);

                return true;
            } 
            catch (Exception ex)
            {
                SALsA.GetInstance(icm)?.Log.Error("Failed to get ICM {0}", icm);
                SALsA.GetInstance(icm)?.Log.Exception(ex);
                return false;
            }
        }
    }
}
