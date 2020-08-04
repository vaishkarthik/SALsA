using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation.Kusto
{
    public class VMA : KustoBase<VMA.MessageLine>
    {
        public class MessageLine
        {
            public string DowntimeStartInUtc { get; set; }
            public string DowntimeEndInUtc { get; set; }
            public string DowntimeDurationInSec { get; set; }
            public string ResourceId { get; set; }
            public string IncarnationId { get; set; }
            public string DowntimeCategory { get; set; }
            public string FailureCategory { get; set; }
            public string FailureSignature { get; set; }
            public string FailureDetails { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }

        override protected string Cluster { get { return Constants.KustoExecutionGraphCluster; } }
        override protected string DataBase { get { return Constants.KustoVMADatabase; } }
        override protected string Table { get { return Constants.KustoVMATable; } }

        private string _containerId;

        public VMA(int icm, string containerId) : base(icm) 
        {
            _containerId = containerId;
        }

        protected override void GenerateKustoQuery()
        {
            KustoQuery = String.Format("where * == \"{0}\" | sort by PreciseTimeStamp desc | project DowntimeStartInUtc, DowntimeEndInUtc, DowntimeDurationInSec, ResourceId, IncarnationId, DowntimeCategory, FailureCategory, FailureSignature, FailureDetails"
                , _containerId);
        }
    }
}
