using Kusto.Cloud.Platform.Utils;
using LivesiteAutomation.Connectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Drawing;

namespace LivesiteAutomation.Kusto
{
    public abstract class KustoBase<MessageLine> where MessageLine : new()
    {
        protected abstract string Cluster { get; }
        protected abstract string DataBase { get; }
        protected abstract string Table { get; }

        protected int Icm;
        protected KustoClient kustoClient;

        protected List<object[]> RawResults { get; set; }
        public MessageLine[] Results { get; protected set; }
        public string HTMLResults { get; protected set; }
        protected string KustoQuery;

        protected abstract void GenerateKustoQuery();

        public KustoBase(int icm)
        {
            this.Icm = icm;
            kustoClient = new KustoClient(Cluster, DataBase, Icm);
            GenerateKustoQuery();
            SendKustoRequest();
        }
        private String GenerateHTMLResult()
        {
            var query = KustoQuery.Replace("|", Environment.NewLine + "|");
            query = String.Format("cluster('{0}').database('{1}').{2}{3}", Cluster, DataBase, query, Environment.NewLine);
            var htmlOut = Utility.List2DToHTML(RawResults);
            var header = String.Format("<p>Execute: [<a href=\"https://dataexplorer.azure.com/clusters/{0}.kusto.windows.net/databases/{1}?query={2}\">Web</a>] [<a href=\"https://{0}.kusto.windows.net/{1}?query={2}&amp;web=0\">Desktop</a>] [<a href=\"https://lens.msftcloudes.com/v2/#/discover/query//results?datasource=(cluster:{0}.kusto.windows.net,database:{1},type:Kusto)&amp;query={2}&amp;runquery=1\">Web (Lens)</a>] [<a href=\"https://{0}.kusto.windows.net/{1}?query={2}&amp;saw=1\">Desktop (SAW)</a>] <a href=\"https://{0}.kusto.windows.net/{1}\">https://{0}.kusto.windows.net/{1}</a></p><pre><code>{3}</code></pre>",
                this.Cluster, this.DataBase, Utility.Base64Encode(Utility.CompressString(query)), Utility.EncodeHtml(query));
            return String.Format("{0}<br><br>{1}", header, htmlOut);
        }

        protected void SendKustoRequest()
        {
            GenerateKustoQuery();
            RawResults = kustoClient.Query(Table, ref KustoQuery, this.Icm, null);
            BuildResult();
        }

        protected virtual void BuildResult()
        {
            MessageLine[] messages = new MessageLine[RawResults.Count - 1];
            for (int i = 1; i < RawResults.Count; ++i)
            {
                MessageLine line = new MessageLine();
                for (int j = 0; j < RawResults[i].Length; ++j)
                {
                    System.Reflection.PropertyInfo pinfo = typeof(MessageLine).GetProperty(((DataColumn)RawResults[0][j]).Caption);
                    pinfo.SetValue(line, RawResults[i][j]);
                }
                messages[i - 1] = line;
            }
            Results = messages;
        }
    }
}
