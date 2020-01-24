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
        private string VMID;
        public AzureCMVMIdToContainerID(int icm) : base(icm) { }
        public MessageLine BuildAndSendRequest(string vmId)
        {
            this.VMID = vmId;
            var query = String.Format("where virtualMachineUniqueId == \"{0}\" | sort by TIMESTAMP desc | extend Cluster=Tenant, NodeId=nodeId, ContainerId=containerId | take 1 | project Cluster, NodeId, ContainerId", VMID);
            List<object[]> table = kustoClient.Query(Table, query, this.Icm);
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
