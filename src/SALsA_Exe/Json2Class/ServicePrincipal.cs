﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation.Json2Class
{
    public class ServicePrincipal
    {
        public string appId { get; set; }
        public string displayName { get; set; }
        public string name { get; set; }
        public string password { get; set; }
        public string tenant { get; set; }
        public override string ToString() { return Utility.ObjectToJson(this, true); }
    }
}
