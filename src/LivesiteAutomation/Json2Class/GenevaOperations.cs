using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation.Json2Class
{
    public class GenevaOperations
    {
        public class GetARMSubscriptionResources
        {
            public string wellknownsubscriptionid { get; set; }
            public string wellknownscopedsubscriptionid { get { return wellknownsubscriptionid; } }
            public string resourcegroupname { get; set; }
            public string skiptoken { get; set; } = null;
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }
        public class GetRDFESubscriptionResources
        {
            public string wellknownsubscriptionid { get; set; }
            public string wellknownscopedsubscriptionid { get { return wellknownsubscriptionid; } }
            public string detaillevel { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }

        // Base class to inherit from
        public class GetIaaSVMBase
        {
            public string smecrpregion { get; set; }
            public string wellknownsubscriptionid { get; set; }
            public string wellknownscopedsubscriptionid { get { return wellknownsubscriptionid; } }
            public string smeresourcegroupnameparameter { get; set; }
            public string smevmnameparameter { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }
        public class GetVMConsoleSerialLogs : GetIaaSVMBase { }
        public class GetVMConsoleScreenshot : GetIaaSVMBase { }

        public class GetVMModelAndInstanceView : GetIaaSVMBase
        {
            public string smegetvmoptionparameter { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }
        public class InspectIaaSDiskForARMVM
        {
            public string smecrpregion { get; set; }
            public string wellknownscopedsubscriptionid { get; set; }
            public string smeblobsasurl { get { return ""; } }
            public string smeresourcegroupname { get; set; }
            public string smevmname { get; set; }
            public string smelogextractmode { get; set; }
            public string smeskiptostep { get; set; }
            public int smetimeoutinmins { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }

        public class GetIaaSVMBaseVMSS
        {
            public string smecrpregion { get; set; }
            public string smeresourcegroupnameparameter { get; set; }
            public string smevirtualmachinescalesetnameparameter { get; set; }
            public string wellknownsubscriptionid { get; set; }
            public string wellknownscopedsubscriptionid { get { return wellknownsubscriptionid; } }
            public int smevirtualmachinescalesetvminstanceidparameter { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }
        public class GetVMConsoleSerialLogsVMSS : GetIaaSVMBaseVMSS { }
        public class GetVMConsoleScreenshotVMSS : GetIaaSVMBaseVMSS { }

        public class InspectIaaSDiskForARMVMVMSS
        {
            public string smecrpregion { get; set; }
            public string smeresourcegroupname { get; set; }
            public string smevmssname { get; set; }
            public string smeblobsasurl { get { return ""; } }
            public string wellknownscopedsubscriptionid { get; set; }
            
            public string smelogextractmode { get; set; }
            public string smeskiptostep { get; set; }
            public int smetimeoutinmins { get; set; }
            public int smevmssinstanceid { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }

        public class GetVMModelAndInstanceViewVMSS
        {
            public string smecrpregion { get; set; }
            public string smeresourcegroupnameparameter { get; set; }
            public string smevirtualmachinescalesetnameparameter { get; set; }
            public string wellknownsubscriptionid { get; set; }
            public string wellknownscopedsubscriptionid { get { return wellknownsubscriptionid; } }
            public string smegetvmscalesetvmoptionparameter { get; set; }
            public int smevirtualmachinescalesetvminstanceidparameter { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }

        public class GetNodeDiagnostics
        {
            public string smevmnameparam { get; set; }
            public string smedeploymentidordeploymentparam { get; set; }
            public string smenodediagnosticstagparam { get; set; }
            public string smefabrichostparam { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }

        public class GetClassicVMScreenshot
        {
            public string smefabrichostparam { get; set; }
            public string smetenantnameparam { get; set; }
            public string smevmnameparam { get; set; }
            public string smescreenshotsizeparam { get; set; }
            public override string ToString() { return Utility.ObjectToJson(this, true); }
        }
    }
}
