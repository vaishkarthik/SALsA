using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation.Json2Class
{
    public class GuestAgentKustoStruct
    {
        public string Cluster;
        public string NodeId;
        public string ContainerId;
        public string RoleInstanceName;
        public override string ToString() { return Utility.ObjectToJson(this, true); }
    }
}
