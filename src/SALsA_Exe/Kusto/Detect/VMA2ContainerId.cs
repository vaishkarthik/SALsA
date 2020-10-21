using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation.Kusto
{
    public class VMA2ContainerId : KustoBase<VMA2ContainerId.MessageLine>
    {
        public class MessageLine
        {
            public DateTime PreciseTimeStamp { get; set; }
            public string Cluster { get; set; }
            public string RoleInstanceName { get; set; }
            public string ContainerId { get; set; }
            public string NodeId { get; set; }
            public string Usage_ResourceGroupName { get; set; }
            public string Usage_Region { get; set; }
            public string GA_GAVersion { get; set; }
            public string AvailabilityState { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }

        override protected string Cluster { get { return Constants.KustoExecutionGraphCluster; } }
        override protected string DataBase { get { return Constants.KustoVMADatabase; } }
        override protected string Table { get { return Constants.KustoVMATable; } }

        private string _subscriptions;
        private string _resourceGroupName;
        private string _virtualMachinesName;

        public VMA2ContainerId(int icm, string subscriptions, string resourceGroupName, string virtualMachinesName, bool send = false) : base(icm, send)
        {
            _subscriptions = subscriptions;
            _resourceGroupName = resourceGroupName;
            _virtualMachinesName = virtualMachinesName;
            Init();
        }

        protected override void GenerateKustoQuery()
        {
            KustoQuery = String.Format("where ContainerId =~ \"{2}\" or ( LastKnownSubscriptionId =~  \"{0}\" and (Usage_ResourceGroupName =~ \"{1}\" or isempty(Usage_ResourceGroupName)) and RoleInstanceName has \"{2}\") | sort by PreciseTimeStamp desc | summarize arg_max(PreciseTimeStamp, *) by RoleInstanceName  | project PreciseTimeStamp, Cluster, RoleInstanceName, ContainerId, NodeId, Usage_ResourceGroupName, Usage_Region, GA_GAVersion, AvailabilityState ",
                                                _subscriptions, _resourceGroupName, _virtualMachinesName);
        }
    }
}
