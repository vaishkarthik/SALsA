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
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string AvailabilityState { get; set; }
            public string ContainerType { get; set; }
            public string RCAEngineCategory { get; set; }
            public string RCA { get; set; }
            public string Detail { get; set; }
            public double DurationInMin { get; set; }
            public int DaysFromLastRootHE { get; set; }
            public string DowntimeReasonHint { get; set; }
            public string EG_DowntimeCategory { get; set; }
            public string EG_FailureCategory { get; set; }
            public string EG_FailureSignature { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }

        override protected string Cluster { get { return Constants.KustoExecutionGraphCluster; } }
        override protected string DataBase { get { return Constants.KustoVMADatabase; } }
        override protected string Table { get { return Constants.KustoVMATable; } }

        private string _containerId;

        public VMA(int icm, string containerId, bool send = false) : base(icm, send)
        {
            _containerId = containerId;
            Init();
        }

        protected override void GenerateKustoQuery()
        {
            KustoQuery = String.Format("where ContainerId =~ \"{0}\" | project StartTime, EndTime, AvailabilityState, ContainerType, RCAEngineCategory, RCA, Detail, DurationInMin, DaysFromLastRootHE, DowntimeReasonHint, EG_DowntimeCategory, EG_FailureCategory, EG_FailureSignature"
                , _containerId);
        }
    }
}
