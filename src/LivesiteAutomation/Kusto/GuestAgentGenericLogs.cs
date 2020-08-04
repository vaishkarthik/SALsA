using Kusto.Cloud.Platform.IO;
using Kusto.Cloud.Platform.Utils;
using Kusto.Data.Linq;
using LivesiteAutomation.Connectors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LivesiteAutomation.Kusto
{
    public class GuestAgentGenericLogs : KustoBase
    {
        public class MessageLine : IComparable<MessageLine>
        {
            public DateTime TIMESTAMP { get; set; }
            public DateTime PreciseTimeStamp { get; set; }
            public string Region { get; set; }
            public string ActivityId { get; set; }
            public string ContainerId { get; set; }
            public string GAVersion { get; set; }
            public string EventName { get; set; }
            public string CapabilityUsed { get; set; }
            public string Context1 { get; set; }
            public string Context2 { get; set; }
            public string Context3 { get; set; }
            public string OSVersion { get; set; }
            public string Cluster { get; set; }
            public string DataCenter { get; set; }
            public string NodeId { get; set; }
            public string NodeIdentity { get; set; }
            public string RoleName { get; set; }
            public string RoleInstanceName { get; set; }
            public string TenantName { get; set; }
            public string ExecutionMode { get; set; }
            public long RAM { get; set; }
            public long Processors { get; set; }
            public string OpcodeName { get; set; }
            public string Environment { get; set; }
            public string SourceNamespace { get; set; }
            public string SourceMoniker { get; set; }
            public string SourceVersion { get; set; }
            public string DeviceId { get; set; }
            public int CompareTo(MessageLine other)
            {
                return PreciseTimeStamp.CompareTo(other.PreciseTimeStamp);
            }
        }
        override protected string Cluster { get { return Constants.KustoGuestAgentGenericLogsCluster; } }
        override protected string DataBase { get { return Constants.KustoGuestAgentGenericLogsDataBase; } }
        override protected string Table { get { return Constants.KustoGuestAgentGenericLogsTable; } }
        private string ContainerID;
        public GuestAgentGenericLogs(int icm) : base(icm) { }
        public MessageLine[] BuildAndSendRequest(string containerId)
        {
            this.ContainerID = containerId;
            var query = String.Format("where TIMESTAMP > datetime({0})", ICM.GetCustomField(Icm, Constants.AnalyzerStartTimeField));
            List<object[]> table = kustoClient.Query(Table, query, this.Icm);
            return ParseResult(table);
        }

        private new MessageLine[] ParseResult(List<object[]> table)
        {
            MessageLine[] messages = new MessageLine[table.Count - 1];
            for (int i = 1; i < table.Count; ++i)
            {
                MessageLine line = new MessageLine();
                for (int j = 0; j < table[i].Length; ++j)
                {
                    System.Reflection.PropertyInfo pinfo = typeof(MessageLine).GetProperty(((DataColumn)table[0][j]).Caption);
                    pinfo.SetValue(line, table[i][j]);
                }
                messages[i - 1] = line;
            }
            Array.Sort(messages);
            return messages;
        }
    }
}
