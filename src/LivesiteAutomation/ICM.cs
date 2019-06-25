using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    class ICM
    {
        private readonly string id;
        public JObject icm { get; private set; }

        public ICM( string icmId )
        {
            id = icmId;
        }

        public ICM getICM()
        {
            try
            { 
                var req = buildRequest(Constants.ICMGetIncidentURL);
                var response = req.GetResponse();
                Log.Instance.Verbose("Got response for IMC {0}", this.id);
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string result = reader.ReadToEnd();
                    icm = JObject.Parse(result);
                    Console.WriteLine(icm.ToString(Newtonsoft.Json.Formatting.None));
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to get ICM {0}", icm);
                Log.Instance.Exception(ex);
            }
            return this;
        }

        private HttpWebRequest buildRequest(string url)
        {
            var fullurl = String.Format("{0}({1})", url, this.id);
            var request = (HttpWebRequest)WebRequest.Create(fullurl);
            request.ClientCertificates.Add(Authentication.Instance.cert);
            return request;
        }
    }
}
