using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation.Kusto
{
    class CAD : KustoBase
    {
        override protected string Cluster { get { return Constants.KustoExecutionGraphCluster; } }
        override protected string DataBase { get { return Constants.KustoVMADatabase; } }
        override protected string Table { get { return Constants.KustoCADTable; } }
        public CAD(int icm) : base(icm) { }

        // This request is to get ContainerId, Cluster, Node, etc.
        public String BuildAndSendRequest(string subscriptions, string tenantName, string virtualMachinesName)
        {
            return ParseResult(BuildAndSendRequestRaw(subscriptions, tenantName, virtualMachinesName));
        }

        public List<object[]> BuildAndSendRequestRaw(string subscriptions, string tenantName, string virtualMachinesName)
        {
            var query = String.Format("where LastKnownSubscriptionId =~  \"{0}\" | where TenantName =~  \"{1}\" | where RoleInstanceName has  \"{2}\" | sort by PreciseTimeStamp desc | project PreciseTimeStamp, Cluster, RoleInstanceName, ContainerId, NodeId, TenantName, TenantId, AvailabilityState | summarize arg_max(PreciseTimeStamp, *) by RoleInstanceName",
                                                            subscriptions, tenantName, virtualMachinesName);
            return kustoClient.Query(Table, query, this.Icm, null);
        }
    }
}
