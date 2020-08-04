using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation.Kusto
{
    public class LogContainerHealthSnapshot : KustoBase<LogContainerHealthSnapshot.MessageLine>
    {
        public class MessageLine
        {
            public string Timestamp { get; set; }
            public string containerState { get; set; }
            public string actualOperationalState { get; set; }
            public string containerOsState { get; set; }
            public string vmExpectedHealthState { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }

        }
        protected override string Cluster { get { return Constants.KustoLogContainerSnapshotCluster; } }
        protected override string DataBase { get { return Constants.KustoLogContainerSnapshotDatabase; } }
        protected override string Table { get { return Constants.KustoLogContainerSnapshotTable; } }

        string _containerId;
        string _startTime;

        public LogContainerHealthSnapshot(int icm, string containerId, string dateTime = null) : base(icm)
        {
            _startTime = dateTime != null ? dateTime : ICM.GetCustomField(Icm, Constants.AnalyzerStartTimeField);
            _containerId = containerId;
        }

        protected override void GenerateKustoQuery()
        {
            KustoQuery = String.Format("where TIMESTAMP >= \"{0}\" | where containerId =~ \"{1}\" | project Timestamp=TIMESTAMP, containerState, actualOperationalState, containerLifecycleState, containerOsState, vmExpectedHealthState",
                _startTime, _containerId);
        }
    }
}
