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
        private readonly int Id;
        public Incident CurrentICM { get; private set; }
        public List<Incident.DescriptionEntry> DescriptionEntries { get; private set; }
        public static Dictionary<int, ICM> IncidentMapping { get; private set; }

        public bool AddICMDiscussion(string entry, bool repeat = true, bool htmlfy = true)
        {
            if (htmlfy)
            { 
                entry = Utility.EncodeHtml(entry);
            }
            if (repeat == false)
            {
                if (DescriptionEntries == null)
                {
                    GetICMDiscussion();
                }
                foreach (var de in DescriptionEntries)
                {
                    if(de.SubmittedBy == Constants.ICMIdentityName && Utility.DecodeHtml(de.Text).CompareTo(Utility.DecodeHtml(entry)) == 0)
                    {
                        Log.Instance.Verbose("Did not add entry to ICM since already sent", this.Id);
                        return false;
                    }
                }
            }
            try
            {
                var body = new Incident.DescriptionPost() { Id = Convert.ToInt32(this.Id), Description = entry };

                var req = BuildHttpClient(this.Id, "", Constants.ICMAddDiscussionURL);
                var response = req.PatchAsync("", new StringContent(Utility.ObjectToJson(body))).Result;

                Log.Instance.Verbose("Got response for IMC {0}", this.Id);
                return true;
 
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to add discussion element to ICM {0}", this.Id);
                Log.Instance.Exception(ex);
                return false;
            }
        }

        public ICM(string icmId)
        {
            this.Id = Convert.ToInt32(icmId);
            Log.Instance.Icm = this.Id;
            IncidentMapping = new Dictionary<int, ICM>() { { this.Id, this } };
        }

        public ICM(int icmId)
        {
            this.Id = icmId;
            Log.Instance.Icm = this.Id;
            IncidentMapping = new Dictionary<int, ICM>() { { this.Id, this } };
        }

        public ICM GetICM()
        {
            try
            {
                var req = BuildHttpClient(this.Id);
                var response = req.GetAsync("").Result;
                Log.Instance.Verbose("Got response for IMC {0}", this.Id);

                CurrentICM = Utility.JsonToObject<Incident>(ReadResponseBody(response));
                Log.Instance.Verbose(CurrentICM);
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to get ICM {0}", this.Id);
                Log.Instance.Exception(ex);
            }
            return this;
        }

        public bool TransferICM(string owningTeam)
        {
            try
            {
                var body = new Incident.Transfer(owningTeam);
                var req = BuildHttpClient(this.Id, Constants.ICMTrnasferIncidentSuffix);
                var response = req.PostAsync("", new StringContent(Utility.ObjectToJson(body))).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    Log.Instance.Error("ICM <{0}> transfer action returned status code {1}", Id, response.StatusCode);
                    Log.Instance.Error(ReadResponseBody(response));
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to transfer ICM {0}", Id);
                Log.Instance.Exception(ex);
                return false;
            }
        }

        public void GetICMDiscussion()
        {
            try
            {
                var req = BuildHttpClient(this.Id, Constants.ICMDescriptionEntriesSuffix);
                var response = req.GetAsync("").Result;
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
                Log.Instance.Error("Failed to get discussion entries for ICM {0}", Id);
                Log.Instance.Exception(ex);
            }
        }

        private static Uri BuildUri(int id, string suffix = "", string prefix = Constants.ICMGetIncidentURL)
        {
            return new Uri(String.Format("{0}({1}){2}", prefix, id, suffix));
        }

        internal static HttpClient BuildHttpClient(int id, string suffix = "", string prefix = Constants.ICMGetIncidentURL)
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(Authentication.Instance.Cert);
            handler.PreAuthenticate = true;
            handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = BuildUri(id, suffix, prefix);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        private string ReadResponseBody(HttpResponseMessage response)
        {
            return response.Content.ReadAsStringAsync().Result;
        }
    }
}
