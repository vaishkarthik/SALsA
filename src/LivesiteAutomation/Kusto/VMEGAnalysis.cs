using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation.Kusto
{
    class VMEGAnalysis : KustoBase
    {
        override protected string Cluster { get { return Constants.KustoExecutionGraphCluster; } }
        override protected string DataBase { get { return Constants.KustoExecutionGraphDatabase; } }
        override protected string Table { get { return Constants.KustoExecutionGraphTable; } }
        private string ContainerId;
        public VMEGAnalysis(int icm) : base(icm) { }
        public String BuildAndSendRequest(string containerId)
        {
            this.ContainerId = containerId;
            var query = String.Format("where * =~ \"{0}\" | sort by TIMESTAMP desc | project ResourceId, DowntimeStartInUtc, DowntimeEndInUtc, DowntimeDurationInSec, IncarnationId, DowntimeCategory, FailureCategory, FailureSignature, FailureDetails, Followup", ContainerId);
            List<object[]> table = kustoClient.Query(Table, query, this.Icm);
            return ParseResult(table);
        }
    }
}
