using SALsA.LivesiteAutomation.Commons;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation.Kusto
{
    class GuestAgentPerformanceCounterEvents : KustoBase<GuestAgentPerformanceCounterEvents.MessageLine>
    {
        public class MessageLine
        {
            public DateTime PreciseTimeStamp { get; set; }
            public string Category { get; set; }
            public string Instance { get; set; }
            public double Value { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }

        private List<Color> colorsToUse = new List<Color> { Color.Red, Color.Blue, Color.AliceBlue, Color.CadetBlue, Color.BlueViolet, Color.DarkBlue, Color.DarkViolet, Color.PaleVioletRed };

        override protected string Cluster { get { return Constants.KustoGuestAgentGenericLogsCluster; } }
        override protected string DataBase { get { return Constants.KustoGuestAgentGenericLogsDataBase; } }
        override protected string Table { get { return Constants.KustoGuestAgentPerformanceCounterEventsTable; } }
        override protected int KustoBaseLimit { get { return 50000; } }

        string _containerId;
        string _startTime;

        public GuestAgentPerformanceCounterEvents(int icm, string containerId, string dateTime = null, bool send = false) : base(icm, send)
        {
            _startTime = InitStartTime(dateTime);
            _containerId = containerId;
            Init();
        }
        override protected void GenerateKustoQuery()
        {
            KustoQuery = String.Format("where TIMESTAMP > datetime({0}) | where ContainerId =~ \"{1}\" | where Category == \"Memory\" or Category == \"Processor\" | where Counter == \"% Processor Time\" or Counter == \"% Committed Bytes in Use\" | project PreciseTimeStamp, Category, Instance, Value | sort by PreciseTimeStamp desc", _startTime, _containerId);
        }

        override protected void GenerateHTMLResult()
        {
            if (_Results.Length == 0)
            {
                SALsA.GetInstance(this.Icm)?.Log.Information("Kusto query for {0}.{1}.{2} yielded empty results. Will skip.", Cluster, DataBase, Table);
                _HTMLResults = null;
                return;
            }
            var query = KustoQuery.Replace("|", Environment.NewLine + "|");
            query = String.Format("cluster('{0}').database('{1}').{2}{3}", Cluster, DataBase, query, Environment.NewLine);
            var htmlOut = Utility.BitMapToHTML(GenerateBitmap());
            var header = String.Format("<p>Execute: [<a href=\"https://dataexplorer.azure.com/clusters/{0}.kusto.windows.net/databases/{1}?query={2}\">Web</a>] [<a href=\"https://{0}.kusto.windows.net/{1}?query={2}&amp;web=0\">Desktop</a>] [<a href=\"https://lens.msftcloudes.com/v2/#/discover/query//results?datasource=(cluster:{0}.kusto.windows.net,database:{1},type:Kusto)&amp;query={2}&amp;runquery=1\">Web (Lens)</a>] [<a href=\"https://{0}.kusto.windows.net/{1}?query={2}&amp;saw=1\">Desktop (SAW)</a>] <a href=\"https://{0}.kusto.windows.net/{1}\">https://{0}.kusto.windows.net/{1}</a></p><pre><code>{3}</code></pre>",
                this.Cluster, this.DataBase, Utility.Base64Encode(Utility.CompressString(query)), Utility.EncodeHtml(query));
            _HTMLResults = String.Format("{0}{1}", header, htmlOut);
            if (WriteToIcm == true)
            {
                SALsA.GetInstance(this.Icm)?.ICM.QueueICMDiscussion(_HTMLResults, htmlfy: false);
            }
        }

        public Bitmap GenerateBitmap()
        {
            var instances = new SortedDictionary<string, List<KeyValuePair<DateTime, double>>>();
            foreach(var line in _Results)
            {
                var name = String.Format("{0}:{1}", line.Category, string.IsNullOrWhiteSpace(line.Instance) ? "_Total" : line.Instance);
                if(instances.ContainsKey(name) == false)
                {
                    instances[name] = new List<KeyValuePair<DateTime, double>>();
                }
                instances[name].Add(new KeyValuePair<DateTime, double>(line.PreciseTimeStamp, line.Value));
            }
            var ru = new ResourceUsage();
            foreach (KeyValuePair<string, List<KeyValuePair<DateTime, double>>> entry in instances)
            {
                var myColor = Color.Black;
                if (colorsToUse.Count > 0)
                {
                    myColor = colorsToUse[0];
                    colorsToUse.RemoveAt(0);
                }
                ru.AddArea(entry.Key, entry.Value, myColor);
            }
            return ru.GenerateGraph();

        }
    }
}
