using SALsA.LivesiteAutomation.Json2Class.RDFESubscriptionWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation.Kusto
{
    // For RDFE PaaS
    public class CAD : KustoBase<CAD.MessageLine>
    {
        public class MessageLine
        {
            public DateTime PreciseTimeStamp { get; set; }
            public string Cluster { get; set; }
            public string RoleInstanceName { get; set; }
            public string ContainerId { get; set; }
            public string NodeId { get; set; }
            public string TenantName { get; set; }
            public string TenantId { get; set; }
            public string AvailabilityState { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }

        override protected string Cluster { get { return Constants.KustoExecutionGraphCluster; } }
        override protected string DataBase { get { return Constants.KustoVMADatabase; } }
        override protected string Table { get { return Constants.KustoCADTable; } }

        private string _subscriptions;
        private string _tenantName;
        private string _virtualMachinesName;

        public CAD(int icm, string subscriptions, string tenantName, string virtualMachinesName, bool send = false) : base(icm, send)
        {
            _subscriptions = subscriptions;
            _tenantName = tenantName;
            _virtualMachinesName = virtualMachinesName;
            Init();
        }

        override protected void GenerateKustoQuery()
        {
            KustoQuery = String.Format("where LastKnownSubscriptionId =~  \"{0}\" | where TenantName =~  \"{1}\" | where RoleInstanceName has  \"{2}\" | sort by PreciseTimeStamp desc | summarize arg_max(PreciseTimeStamp, *) by RoleInstanceName | project PreciseTimeStamp, Cluster, RoleInstanceName, ContainerId, NodeId, TenantName, TenantId, AvailabilityState",
                                                            _subscriptions, _tenantName, _virtualMachinesName);
        }
    }
}
