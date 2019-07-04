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
        private HttpClient client = null;

        public bool AddICMDiscussion(string entry, bool repeat = false, bool htmlfy = true)
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
                var body = new StringContent(Utility.ObjectToJson(new Incident.DescriptionPost(entry)));
                body.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = Client.PatchAsync(BuildUri(this.Id), body).Result;
                // TODO : Handle non 200 and display error
                var reason = response.Content.ReadAsStringAsync().Result;
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
                var response = Client.GetAsync(BuildUri(this.Id)).Result;
                Utility.CheckStatusCode(response);
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
                var body = new StringContent(Utility.ObjectToJson(new Incident.DescriptionPost(owningTeam)));
                body.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = Client.PostAsync(BuildUri(this.Id, Constants.ICMTrnasferIncidentSuffix), body).Result;
                if (response.IsSuccessStatusCode)
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
                var response = Client.GetAsync(BuildUri(this.Id, Constants.ICMDescriptionEntriesSuffix)).Result;
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

        private static Uri BuildUri(int id, string suffix = "")
        {
            return new Uri(String.Format("{0}({1}){2}", Constants.ICMRelativeBaseAPIUri, id, suffix), UriKind.Relative);
        }

        internal HttpClient Client
        {
            get { 
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
