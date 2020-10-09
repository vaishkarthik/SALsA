using SALsA.LivesiteAutomation.Commons;
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
using SALsA.General;

namespace SALsA.LivesiteAutomation
{
    public partial class ICM
    {

        public List<Incident.DescriptionEntry> DescriptionEntries { get; private set; }
        private static HttpClient client = null;

        public static AllIncidents GetAllICM()
        {
            String tenantQuery = String.Join(" or ",
                Constants.ICMTeamToTenantLookupTable.Keys.ToList().ConvertAll(
                    e => String.Format("OwningTeamId eq '{0}'", e.ToUpperInvariant())));
            try
            {
                var query = new Uri(String.Format("{0}?$filter=({1}) and (Status eq 'ACTIVE' or Status eq 'MITIGATED')", Constants.ICMRelativeBaseAPIUri, tenantQuery), UriKind.Relative);

                var response = Client.GetAsync(query).Result;
                response.EnsureSuccessStatusCode();
                var result = ReadResponseBody(response);
                var allIncidents = Utility.JsonToObject<AllIncidents>(result);
                return allIncidents;

            }
            catch { return null; }
        }

        public static Incident PopulateICMInfo(int icmId)
        {
                var response = Client.GetAsync(BuildUri(icmId)).Result;
                response.EnsureSuccessStatusCode();

                return Utility.JsonToObject<Incident>(ReadResponseBody(response));
        }

        public static void TransferICM(string owningTeam, int icmId)
        {
            var body = new StringContent(Utility.ObjectToJson(new Incident.DescriptionPost(owningTeam)));
            body.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = Client.PostAsync(BuildUri(icmId, Constants.ICMTrnasferIncidentSuffix), body).Result;
            response.EnsureSuccessStatusCode();
        }

        private void GetICMDiscussion(int icmId)
        {
            var response = Client.GetAsync(BuildUri(icmId, Constants.ICMDescriptionEntriesSuffix)).Result;
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

        public static string GetCustomFieldInternal(List<Incident.CustomFieldGroup> allFields, string lookup)
        {
            foreach (var fields in allFields)
            {
                var sid = fields.CustomFields.Find(x => x.Name == lookup);
                if (sid != null && sid.Value != "")
                {
                    return sid.Value;
                }
            }
            return null;
        }

        public static bool CheckIfICMAccessible(int icm)
        {
            try
            {
                var response = Client.GetAsync(BuildUri(icm)).Result;
                response.EnsureSuccessStatusCode();
                return true;
            } 
            catch
            {
                return false;
            }
        }
    }
}
