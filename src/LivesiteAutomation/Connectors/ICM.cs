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
                var body = new Incident.DescriptionPost() { Id = Convert.ToInt32(this.Id), Description = entry };
                var response = TryGetResponse(this.Id, Utility.ObjectToJson(body), "PATCH", "", Constants.ICMAddDiscussionURL);
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

        internal static WebResponse TryGetResponse(int id, string body, string method, string suffix = "", string prefix = Constants.ICMGetIncidentURL)
        {
            Exception ex = new Exception();
            for (int i = 0; i < Constants.ICMHttpRetryLimit; ++i)
            {
                try
                {
                    var req = BuildRequestWithBody(id, body, "PATCH", suffix, prefix);
                    return req.GetResponse();
                }
                catch (Exception e)
                {
                    ex = e;
                    Log.Instance.Error("Failed http request, retrying in {1}sec... Reason : {0}", e.Message, (int)Math.Pow(2, i));
                    Thread.Sleep((int)Math.Pow(2, i) * 1000);
                }
            }
            throw ex;
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
                var req = BuildGetRequest(this.Id);
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
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
                var req = BuildRequestWithBody(this.Id, Utility.ObjectToJson(body), "POST", Constants.ICMTrnasferIncidentSuffix);
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
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
                var req = BuildGetRequest(this.Id, Constants.ICMDescriptionEntriesSuffix);
                var response = (HttpWebResponse)req.GetResponse();
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

        private static HttpWebRequest BuildGetRequest(int id, string suffix = "", string prefix = Constants.ICMGetIncidentURL)
        {
            var fullurl = String.Format("{0}({1}){2}", prefix, id, suffix);
            var request = (HttpWebRequest)WebRequest.Create(fullurl);
            request.ClientCertificates.Add(Authentication.Instance.Cert);
            request.Proxy = new WebProxy("127.0.0.1", 8080);
            return request;
        }

        private static HttpWebRequest BuildRequestWithBody(int id, string body, string method, string suffix = "", string prefix = Constants.ICMGetIncidentURL)
        {
            var request = BuildGetRequest(id, suffix, prefix);
            request.Method = method;
            var bArr = Encoding.Default.GetBytes(body);
            request.ContentLength = bArr.Length;
            request.ContentType = "application/json";
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(bArr, 0, bArr.Length);
            dataStream.Close();
            return request;
        }

        private string ReadResponseBody(HttpWebResponse response)
        {
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }
    }
}
