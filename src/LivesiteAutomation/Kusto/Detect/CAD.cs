using LivesiteAutomation.Json2Class.RDFESubscriptionWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation.Kusto
{
    // For RDFE PaaS
    public class CAD : KustoBase<CAD.MessageLine>
    {
        public class MessageLine
        {
            public string Cluster { get; set; }
            public string NodeId { get; set; }
            public string ContainerId { get; set; }
            public string RoleInstanceName { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }

        override protected string Cluster { get { return Constants.KustoExecutionGraphCluster; } }
        override protected string DataBase { get { return Constants.KustoVMADatabase; } }
        override protected string Table { get { return Constants.KustoCADTable; } }

        private string _subscriptions;
        private string _tenantName;
        private string _virtualMachinesName;

        public CAD(int icm, string subscriptions, string tenantName, string virtualMachinesName) : base(icm)
        {
            _subscriptions = subscriptions;
            _tenantName = tenantName;
            _virtualMachinesName = virtualMachinesName;
        }

        override protected void GenerateKustoQuery()
        {
            KustoQuery = String.Format("where LastKnownSubscriptionId =~  \"{0}\" | where TenantName =~  \"{1}\" | where RoleInstanceName has  \"{2}\" | sort by PreciseTimeStamp desc | project PreciseTimeStamp, Cluster, RoleInstanceName, ContainerId, NodeId, TenantName, TenantId, AvailabilityState | summarize arg_max(PreciseTimeStamp, *) by RoleInstanceName",
                                                            _subscriptions, _tenantName, _virtualMachinesName);
        }
    }
}
