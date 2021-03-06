﻿using SALsA.LivesiteAutomation.Connectors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation.Kusto
{
    public class GuestAgentExtensionEvents : KustoBase<GuestAgentExtensionEvents.MessageLine>
    {
        public class MessageLine
        {
            public DateTime PreciseTimeStamp { get; set; }
            public string ExtensionName { get; set; }
            public string ExtensionVersion { get; set; }
            public string ExtensionOperation { get; set; }
            public bool OperationSuccess { get; set; }
            public string Message { get; set; }
            public string TaskName { get; set; }
            public string Duration { get; set; }
            public string IsInternal { get; set; }
            public string ExtensionType { get; set; }
            public string Cluster { get; set; }
            public string NodeId { get; set; }
            public string ContainerId { get; set; }
            public string RoleInstanceName { get; set; }
            public string TenantName { get; set; }
            public string ResourceGroupName { get; set; }
            public string SubscriptionId { get; set; }
            public string GAVersion { get; set; }
            public string Region { get; set; }
            public string OSVersion { get; set; }
            public string ExecutionMode { get; set; }
            public long RAM { get; set; }
            public long Processors { get; set; }

            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }

        override protected string Cluster { get { return Constants.KustoGuestAgentGenericLogsCluster; } }
        override protected string DataBase { get { return Constants.KustoGuestAgentGenericLogsDataBase; } }
        override protected string Table { get { return Constants.KustoGuestAgentExtensionEventsTable; } }

        string _containerId;
        string _startTime;

        public GuestAgentExtensionEvents(int icm, string containerId, string dateTime = null, bool send = false) : base(icm, send)
        {
            _startTime = InitStartTime(dateTime);
            _containerId = containerId;
            Init();
        }
        override protected void GenerateKustoQuery()
        {
            KustoQuery = String.Format("where TIMESTAMP > datetime({0}) | where ContainerId =~ \"{1}\" | extend OperationSuccess = MapOperationSuccessField(OperationSuccess) | project  PreciseTimeStamp, ExtensionName = Name, ExtensionVersion = Version, ExtensionOperation = Operation, OperationSuccess, Message, TaskName, Duration, IsInternal, ExtensionType, Cluster, NodeId, ContainerId, RoleInstanceName, TenantName, GAVersion, Region, OSVersion, ExecutionMode, RAM, Processors | sort by PreciseTimeStamp desc",
                                     _startTime, _containerId);
        }
    }
}
