using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation.Kusto
{
    public class VMEGAnalysis : KustoBase<VMEGAnalysis.MessageLine>
    {
        public class MessageLine
        {
            public string ResourceId { get; set; }
            public DateTime DowntimeStartInUtc { get; set; }
            public DateTime DowntimeEndInUtc { get; set; }
            public int DowntimeDurationInSec { get; set; }
            public string IncarnationId { get; set; }
            public string DowntimeCategory { get; set; }
            public string FailureCategory { get; set; }
            public string FailureSignature { get; set; }
            public string FailureDetails { get; set; }
            public string Followup { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }

        override protected string Cluster { get { return Constants.KustoExecutionGraphCluster; } }
        override protected string DataBase { get { return Constants.KustoExecutionGraphDatabase; } }
        override protected string Table { get { return Constants.KustoExecutionGraphTable; } }

        private string _containerId;

        public VMEGAnalysis(int icm, string containerId, bool send = false) : base(icm, send)
        {
            _containerId = containerId;
            Init();
        }

        protected override void GenerateKustoQuery()
        {
            KustoQuery = String.Format("where ContainerId =~ \"{0}\" | sort by TIMESTAMP desc | project ResourceId, DowntimeStartInUtc, DowntimeEndInUtc, DowntimeDurationInSec, IncarnationId, DowntimeCategory, FailureCategory, FailureSignature, FailureDetails, Followup",
                _containerId);
        }
    }
}
