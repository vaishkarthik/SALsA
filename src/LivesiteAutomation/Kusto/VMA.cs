using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation.Kusto
{
    class VMA : KustoBase
    {
        override protected string Cluster { get { return Constants.KustoExecutionGraphCluster; } }
        override protected string DataBase { get { return Constants.KustoExecutionGraphDatabase; } }
        override protected string Table { get { return Constants.KustoExecutionGraphTable; } }
        private string ContainerId;
        public VMA(int icm) : base(icm) { }
        public String BuildAndSendRequest(string containerId)
        {
            this.ContainerId = containerId;
            var query = String.Format("where ContainerId == \"{0}\" | sort by PreciseTimeStamp desc | project DowntimeStartInUtc, DowntimeEndInUtc, DowntimeDurationInSec, ResourceId, IncarnationId, DowntimeCategory, FailureCategory, FailureSignature, FailureDetails", ContainerId);
            List<object[]> table = kustoClient.Query(Table, query, this.Icm, "PreciseTimeStamp");
            return ParseResult(table);
        }

        private String ParseResult(List<object[]> table)
        {
            return Utility.KustoToHTML(table);
        }
    }
}
