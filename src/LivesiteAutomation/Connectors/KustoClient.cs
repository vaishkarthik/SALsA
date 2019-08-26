using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace LivesiteAutomation.Connectors
{
    public class KustoClient
    {
        ICslQueryProvider client;
        public KustoClient(string clusterName, string database, int icm)
        {
            SALsA.GetInstance(icm)?.Log.Information("Creating Kusto connector for Cluster : {0} in database : {1}", clusterName, database);
            // Create Auth Context for AAD (common or tenant-specific endpoint):
            AuthenticationContext authContext = new AuthenticationContext(String.Format("https://login.microsoftonline.com/{0}",
                Authentication.Instance.ServicePrincipal.tenant));

            var authority = Authentication.Instance.ServicePrincipal.tenant;
            var applicationClientId = Authentication.Instance.ServicePrincipal.appId;
            var applicationKey = Authentication.Instance.ServicePrincipal.password;
            var kcsb = new KustoConnectionStringBuilder(String.Format("https://{0}.kusto.windows.net", clusterName), database)
                .WithAadApplicationKeyAuthentication(applicationClientId, applicationKey, authority);
            client = KustoClientFactory.CreateCslQueryProvider(kcsb);

            SALsA.GetInstance(icm)?.Log.Information("Finished creating Kusto connector : {0}", kcsb);
        }

        public List<object[]> Query(string query)
        {
            query += Constants.KustoClientQueryLimit;
            var clientRequestProperties = new ClientRequestProperties() { ClientRequestId = Guid.NewGuid().ToString() };
            using (var reader = client.ExecuteQuery(query, clientRequestProperties))
            {
                DataTable dt = new DataTable();
                dt.Load(reader);
                List<object[]> data = new List<object[]>();
                data.Add(new object[dt.Columns.Count]);
                dt.Columns.CopyTo(data[0], 0);
                foreach(DataRow line in dt.Rows)
                {
                    data.Add(new object[dt.Columns.Count]);
                    line.ItemArray.CopyTo(data[data.Count-1], 0);
                }
                return data;
            }
        }
    }
}