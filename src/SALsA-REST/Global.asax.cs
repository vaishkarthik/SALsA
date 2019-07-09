using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace SALsA_REST
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        // Dont remove this ligne, it keeps the DLL active, so singletons dont get reset...
        static LivesiteAutomation.Log Logger = LivesiteAutomation.SALsA.GlobalLog;
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
