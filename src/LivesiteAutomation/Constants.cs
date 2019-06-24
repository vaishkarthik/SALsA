using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    static class Constants
    {
        public static string LogDefaultPath = System.IO.Path.Combine(System.IO.Path.GetPathRoot(Environment.SystemDirectory), "Log", LogFileName);
        public const string LogFileName = "LivesiteAUtomation.log";
    }
}
