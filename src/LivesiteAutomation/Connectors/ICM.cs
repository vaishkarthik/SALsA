using LivesiteAutomation.Commons;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public Incident CurrentICM { get; private set; }
        public List<Incident.DescriptionEntry> DescriptionEntries { get; private set; }
        private HttpClient client = null;

        public bool AddICMDiscussion(string entry, bool repeat = false, bool htmlfy = true)
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
            try
            {
                var body = new StringContent(Utility.ObjectToJson(new Incident.DescriptionPost(entry)));
                body.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = Client.PatchAsync(BuildUri(this.Id), body).Result;
                var reason = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
                SALsA.GetInstance(Id)?.Log.Verbose("Got response for IMC {0}", this.Id);
                return true;

            }
            catch (Exception ex)
            {
                SALsA.GetInstance(Id)?.Log.Error("Failed to add discussion element to ICM {0}", this.Id);
                SALsA.GetInstance(Id)?.Log.Exception(ex);
                return false;
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

        internal HttpClient Client
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

        private string ReadResponseBody(HttpResponseMessage response)
        {
            return response.Content.ReadAsStringAsync().Result;
        }
    }
}
