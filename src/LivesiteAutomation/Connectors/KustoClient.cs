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

            var authority = Authentication.Instance.ServicePrincipal.tenant;
            var applicationClientId = Authentication.Instance.ServicePrincipal.appId;
            var applicationKey = Authentication.Instance.ServicePrincipal.password;
            var kcsb = new KustoConnectionStringBuilder(String.Format("https://{0}.kusto.windows.net", clusterName), database)
                .WithAadApplicationKeyAuthentication(applicationClientId, applicationKey, authority);
            client = KustoClientFactory.CreateCslQueryProvider(kcsb);

            SALsA.GetInstance(icm)?.Log.Information("Finished creating Kusto connector : {0}", kcsb);
        }

        public List<object[]> Query(string table, ref string query, int icm, string timestampField = "TIMESTAMP")
        {
            if (timestampField != null)
            {
                // TODO : If ICM AnalyzerStartTimeField was changed, it might be newer than the ICM creation date
                DateTime startTime;
                if (!DateTime.TryParse(ICM.GetCustomField(icm, Constants.AnalyzerStartTimeField), out startTime))
                {
                    startTime = SALsA.GetInstance(icm).ICM.CurrentICM.CreateDate.AddDays(-1);
                }
                string start = startTime.ToString("u");
                //string end = SALsA.GetInstance(icm).ICM.CurrentICM.CreateDate.ToString("u");
                query = String.Format("{0} | where {1} > datetime({2}) | {3} | limit {4}", table, timestampField, start, /*end,*/ query, Constants.KustoClientQueryLimit);
            }
            else
            {
                query = String.Format("{0} | {1} | limit {2}", table, query, Constants.KustoClientQueryLimit);
            }
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