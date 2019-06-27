using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private class Transfer
        {
            public class TransferParameters
            {
                public class Description
                {
                    public string Text = "AzureRT Automatically transfering this incident to the writeful; owners.";
                    public string RenderType = "Plaintext";
                    public string ChangeBy = "azGaExt";
                }

                public string OwningTenantPublicId = null;
                public string OwningTeamPublicId = null;
                public Description description = new Description();
            }
            public TransferParameters transferParameters = new TransferParameters();
            public Transfer(string owningTeamPublicId)
            {
                transferParameters.OwningTeamPublicId = owningTeamPublicId;
                // TODO replace by Kusto function
                transferParameters.OwningTenantPublicId = Constants.ICMTeamToTenantLookupTable[transferParameters.OwningTeamPublicId].ToString();
            }
        }
        private readonly string id;
        public JObject icm { get; private set; }

        public ICM( string icmId )
        {
            id = icmId;
            Log.Instance.ICM = this.id;
        }

        public ICM GetICM()
        {
            try
            { 
                var req = BuildGetRequest(Constants.ICMGetIncidentURL);
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                Log.Instance.Verbose("Got response for IMC {0}", this.id);
                icm = JObject.Parse(ReadResponseBody(response));
                Log.Instance.Verbose(icm);
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to get ICM {0}", this.id);
                Log.Instance.Exception(ex);
            }
            return this;
        }

        public bool TransferICM(string owningTeam)
        {
            try
            {
                var body = new Transfer(owningTeam);
                var req = BuildPostRequest(Constants.ICMGetIncidentURL, JsonConvert.SerializeObject(body), Constants.ICMTrnasferIncidentSuffix);
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                if(response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    Log.Instance.Error("ICM <{0}> transfer action returned status code {1}", icm, response.StatusCode);
                    Log.Instance.Error(ReadResponseBody(response));
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to transfer ICM {0}", icm);
                Log.Instance.Exception(ex);
                return false;
            }
        }

        private HttpWebRequest BuildGetRequest(string url, string suffix = "")
        {
            var fullurl = String.Format("{0}({1}){2}", url, this.id, suffix);
            var request = (HttpWebRequest)HttpWebRequest.Create(fullurl);
            request.ClientCertificates.Add(Authentication.Instance.Cert);
            return request;
        }

        private HttpWebRequest BuildPostRequest(string url, string body, string suffix = "")
        {
            var request = BuildGetRequest(url, suffix);
            request.Method = "POST";
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
