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
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    class ICM
    {
        private readonly string ID;
        public Incident CurrentICM { get; private set; }
        public List<DescriptionEntry> DescriptionEntries { get; private set; }

        public bool AddICMDiscussion(string entry)
        {
            try
            {
                var body = String.Format(CultureInfo.InvariantCulture, "{{\"Description\":\"{0}\",\"CustomFields\":[],\"Id\":{1}}}", entry, this.ID);
                var req = BuildRequestWithBody(body, "PATCH", "", Constants.ICMAddDiscussionURL);
                                
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                Log.Instance.Verbose("Got response for IMC {0}", this.ID);
                return true;
 
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to add discussion element to ICM {0}", this.ID);
                Log.Instance.Exception(ex);
                return false;
            }
        }

        public ICM(string icmId)
        {
            this.ID = icmId;
            Log.Instance.ICM = this.ID;
        }

        public ICM GetICM()
        {
            try
            {
                var req = BuildGetRequest();
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                Log.Instance.Verbose("Got response for IMC {0}", this.ID);

                CurrentICM = Utility.JsonToObject<Incident>(ReadResponseBody(response));
                Log.Instance.Verbose(CurrentICM);
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to get ICM {0}", this.ID);
                Log.Instance.Exception(ex);
            }
            return this;
        }

        public bool TransferICM(string owningTeam)
        {
            try
            {
                var body = new Transfer(owningTeam);
                var req = BuildRequestWithBody(Utility.ObjectToJson(body), "POST", Constants.ICMTrnasferIncidentSuffix);
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    Log.Instance.Error("ICM <{0}> transfer action returned status code {1}", ID, response.StatusCode);
                    Log.Instance.Error(ReadResponseBody(response));
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to transfer ICM {0}", ID);
                Log.Instance.Exception(ex);
                return false;
            }
        }

        public void GetICMDescrition()
        {
            try
            {
                var req = BuildGetRequest(Constants.ICMDescriptionEntriesSuffix);
                var response = (HttpWebResponse)req.GetResponse();
                Dictionary<string, object> de = Utility.JsonToObject<Dictionary<string, object>>(ReadResponseBody(response));

                DescriptionEntries = ((JArray)de["value"]).Select(x => new DescriptionEntry
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
                Log.Instance.Error("Failed to get discussion entries for ICM {0}", ID);
                Log.Instance.Exception(ex);
            }
        }

        private HttpWebRequest BuildGetRequest(string suffix = "", string prefix = Constants.ICMGetIncidentURL)
        {
            var fullurl = String.Format("{0}({1}){2}", prefix, ID, suffix);
            var request = (HttpWebRequest)HttpWebRequest.Create(fullurl);
            request.ClientCertificates.Add(Authentication.Instance.Cert);
            return request;
        }

        private HttpWebRequest BuildRequestWithBody(string body, string method, string suffix = "", string prefix = Constants.ICMGetIncidentURL)
        {
            var request = BuildGetRequest(suffix, prefix);
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
