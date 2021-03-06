﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation.Kusto
{
    public class LogContainerHealthSnapshot : KustoBase<LogContainerHealthSnapshot.MessageLine>
    {
        public class MessageLine
        {
            public DateTime Timestamp { get; set; }
            public string containerState { get; set; }
            public string actualOperationalState { get; set; }
            public string containerOsState { get; set; }
            public string vmExpectedHealthState { get; set; }
            public string faultInfo { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }

        }
        protected override string Cluster { get { return Constants.KustoLogContainerSnapshotCluster; } }
        protected override string DataBase { get { return Constants.KustoLogContainerSnapshotDatabase; } }
        protected override string Table { get { return Constants.KustoLogContainerHealthSnapshotTable; } }

        string _containerId;
        string _startTime;

        public LogContainerHealthSnapshot(int icm, string containerId, string dateTime = null, bool send = false) : base(icm, send)
        {
            _startTime = InitStartTime(dateTime);
            _containerId = containerId;
            Init();
        }

        protected override void GenerateKustoQuery()
        {
            KustoQuery = String.Format("where TIMESTAMP >= datetime({0}) | where containerId =~ \"{1}\" | project Timestamp=TIMESTAMP, containerState, actualOperationalState, containerLifecycleState, containerOsState, vmExpectedHealthState, faultInfo | sort by Timestamp desc",
                _startTime, _containerId);
        }
    }
}
