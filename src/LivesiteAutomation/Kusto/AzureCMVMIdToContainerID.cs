using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation.Kusto
{
    class AzureCMVMIdToContainerID : KustoBase
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
        private readonly string VMID;
        public AzureCMVMIdToContainerID(int icm, string vmId) : base(icm) { this.VMID = vmId; }
        public MessageLine BuildAndSendRequest()
        {
            // TODO : Should be ICM datetime - 1day
            var query = String.Format("{0} | where virtualMachineUniqueId == \"{1}\" | sort by TIMESTAMP desc | extend Cluster=Tenant, NodeId=nodeId, ContainerId=containerId | take 1 | project Cluster, NodeId, ContainerId", Table, VMID);
            List<object[]> table = kustoClient.Query(query);
            return ParseResult(table);
        }

        public MessageLine ParseResult(List<object[]> table)
        {
            MessageLine message = new MessageLine()
            {
                Cluster = (string)table[1][0],
                NodeId = (string)table[1][1],
                ContainerId = (string)table[1][2]
            };
            return message;
        }
    }
}
