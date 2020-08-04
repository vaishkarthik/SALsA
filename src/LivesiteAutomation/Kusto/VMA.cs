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
        override protected string DataBase { get { return Constants.KustoVMADatabase; } }
        override protected string Table { get { return Constants.KustoVMATable; } }
        private string ContainerId;
        public VMA(int icm) : base(icm) { }
        // This request is to get failure details
        public List<object[]> BuildAndSendRequestRaw(string containerId)
        {
            this.ContainerId = containerId;
            var query = String.Format("where * == \"{0}\" | sort by PreciseTimeStamp desc | project DowntimeStartInUtc, DowntimeEndInUtc, DowntimeDurationInSec, ResourceId, IncarnationId, DowntimeCategory, FailureCategory, FailureSignature, FailureDetails", ContainerId);
            return kustoClient.Query(Table, query, this.Icm, "PreciseTimeStamp");
        }
        public String BuildAndSendRequest(string containerId)
        {
            return ParseResult(BuildAndSendRequestRaw(containerId));
        }

        // This request is to get ContainerId, Cluster, Node, etc.
        public String BuildAndSendRequest(string subscriptions, string resourceGroups, string virtualMachinesName)
        {
            return ParseResult(BuildAndSendRequestRaw(subscriptions, resourceGroups, virtualMachinesName));
        }

        public List<object[]> BuildAndSendRequestRaw(string subscriptions, string resourceGroups, string virtualMachinesName)
        {
            var query = String.Format("where LastKnownSubscriptionId =~  \"{0}\" | where Usage_ResourceGroupName =~  \"{1}\" | where RoleInstanceName has  \"{2}\" | sort by PreciseTimeStamp desc | project PreciseTimeStamp, Cluster, RoleInstanceName, ContainerId, NodeId, Usage_ResourceGroupName, Usage_Region, GA_GAVersion, AvailabilityState | summarize arg_max(PreciseTimeStamp, *) by RoleInstanceName",
                                                subscriptions, resourceGroups, virtualMachinesName);
            return kustoClient.Query(Table, query, this.Icm, null);
        }
    }
}
