using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation.Kusto
{
    public class NodeId2Cluster : KustoBase<NodeId2Cluster.MessageLine>
    {
        public class MessageLine
        {
            public string Cluster { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }

        }
        override protected string Cluster { get { return Constants.KustoGuestAgentGenericLogsCluster; } }
        override protected string DataBase { get { return Constants.KustoGuestAgentGenericLogsDataBase; } }
        override protected string Table { get { return Constants.KustoHostAgentTable; } }

        private string _nodeId;

        public NodeId2Cluster(int icm, string nodeId, bool send = false) : base(icm, send)
        {
            _nodeId = nodeId;
            Init();
        }
        protected override void GenerateKustoQuery()
        {
            KustoQuery = String.Format("where NodeId == \"{0}\" | project Cluster | where Cluster != \"\" | take 1", _nodeId);
        }
    }
}
