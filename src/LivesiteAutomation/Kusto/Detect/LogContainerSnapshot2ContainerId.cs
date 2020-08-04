using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation.Kusto
{
    public class LogContainerSnapshot2ContainerId : KustoBase<LogContainerSnapshot2ContainerId.MessageLine>
    {
        public class MessageLine
        {
            public string Cluster { get; set; }
            public string NodeId { get; set; }
            public string ContainerId { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }

        }
        override protected string Cluster { get { return Constants.KustoLogContainerSnapshotCluster; } }
        override protected string DataBase { get { return Constants.KustoLogContainerSnapshotDatabase; } }
        override protected string Table { get { return Constants.KustoLogContainerSnapshotTable; } }

        private string _vmId;

        public LogContainerSnapshot2ContainerId(int icm, string vmId) : base(icm) 
        {
            _vmId = vmId;
        }
        protected override void GenerateKustoQuery()
        {
            KustoQuery = String.Format("where virtualMachineUniqueId == \"{0}\" | sort by TIMESTAMP desc | extend Cluster=Tenant, NodeId=nodeId, ContainerId=containerId | take 1 | project Cluster, NodeId, ContainerId", _vmId);
        }
    }
}
