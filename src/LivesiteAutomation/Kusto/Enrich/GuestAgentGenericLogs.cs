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
    public class GuestAgentGenericLogs : KustoBase<GuestAgentGenericLogs.MessageLine>
    {
        public class MessageLine
        {
            public DateTime PreciseTimeStamp { get; set; }
            public string EventName { get; set; }
            public string Level { get; set; }
            public string Event { get; set; }
            public string Message { get; set; }
            public string Cluster { get; set; }
            public string NodeId { get; set; }
            public string ContainerId { get; set; }
            public string RoleInstanceName { get; set; }
            public string TenantName { get; set; }
            public string GAVersion { get; set; }
            public string Region { get; set; }
            public string OSVersion { get; set; }
            public string ExecutionMode { get; set; }
            public string RAM { get; set; }
            public string Processors { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }

        override protected string Cluster { get { return Constants.KustoGuestAgentGenericLogsCluster; } }
        override protected string DataBase { get { return Constants.KustoGuestAgentGenericLogsDataBase; } }
        override protected string Table { get { return Constants.KustoGuestAgentGenericLogsTable; } }

        string _containerId;
        string _startTime;

        public GuestAgentGenericLogs(int icm, string containerId, string dateTime = null, bool send = false) : base(icm, send)
        {
            _startTime = dateTime != null ? dateTime : ICM.GetCustomField(Icm, Constants.AnalyzerStartTimeField);
            if (_startTime == null) { _startTime = DefaultStartTime; }

            _containerId = containerId;
            Init();
        }
        override protected void GenerateKustoQuery()
        {
            KustoQuery = String.Format("where TIMESTAMP > datetime({0}) | where ContainerId =~ \"{1}\" | project PreciseTimeStamp=todatetime(Context2), EventName, Level=CapabilityUsed, Event=Context3, Message=Context1, Cluster, NodeId, ContainerId, RoleInstanceName, TenantName, GAVersion, Region, OSVersion, ExecutionMode, RAM, Processors | sort by PreciseTimeStamp desc", _startTime, _containerId);
        }
    }
}
