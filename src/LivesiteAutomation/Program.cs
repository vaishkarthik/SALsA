using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    class Program
    {
        static void Main(string[] args)
        {
            Authentication abcdef = new Authentication();

            Log.Instance.Verbose("Test");
            Log.Instance.Verbose("Test", "2");
            Log.Instance.Verbose("Test {0}", "2");
        }
    }
}
