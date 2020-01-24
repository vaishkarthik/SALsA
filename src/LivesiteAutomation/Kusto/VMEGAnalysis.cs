using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation.Kusto
{
    class VMEGAnalysis : KustoBase
    {
        public class MessageLine
        {
            public string TIMESTAMP { get; set; }
            public string DowntimeCategory { get; set; }
            public string DowntimeStartInUtc { get; set; }
            public string DowntimeEndInUtc { get; set; }
            public string FailureSignature { get; set; }
            public string Followup { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }
        override protected string Cluster { get { return Constants.KustoExecutionGraphCluster; } }
        override protected string DataBase { get { return Constants.KustoExecutionGraphDatabase; } }
        override protected string Table { get { return Constants.KustoExecutionGraphTable; } }
        private string ContainerId;
        public VMEGAnalysis(int icm) : base(icm) { }
        public MessageLine BuildAndSendRequest(string containerId)
        {
            this.ContainerId = containerId;
            var query = String.Format("where virtualMachineUniqueId == \"{0}\" | sort by TIMESTAMP desc | project TIMESTAMP, RoleInstanceName, DowntimeCategory, DowntimeStartInUtc, DowntimeEndInUtc, FailureSignature, Followup", ContainerId);
            List<object[]> table = kustoClient.Query(Table, query, this.Icm);
            return ParseResult(table);
        }

        public MessageLine ParseResult(List<object[]> table)
        {
            MessageLine message = new MessageLine()
            {
                TIMESTAMP = (string)table[1][0],
                DowntimeCategory = (string)table[1][1],
                DowntimeStartInUtc = (string)table[1][2],
                DowntimeEndInUtc = (string)table[1][3],
                FailureSignature = (string)table[1][4],
                Followup = (string)table[1][5]
            };
            return message;
        }
    }
}
