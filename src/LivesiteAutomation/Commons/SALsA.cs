using LivesiteAutomation.Json2Class.RDFESubscriptionWrapper;
using System.Collections.Generic;
using System.Data;
using static LivesiteAutomation.Utility;

namespace LivesiteAutomation
{
    public class SALsA
    {
        public class SALsAInstance
        {
            public Log Log;
            public ICM ICM;
            public TaskManager TaskManager;
            public SALsAInstance(int icm)
            {
                this.Log = new Log(icm);
                this.ICM = new ICM(icm);
                this.TaskManager = new TaskManager(icm);
            }
        }

        public static Log GlobalLog;
        private static Dictionary<int, SALsAInstance> instances;

        public static void AddInstance(int icm)
        {
            SALsA.instances.Add(icm, new SALsA.SALsAInstance(icm));
        }
        public static void RemoveInstance(int icm)
        {
            SALsA.instances.Remove(icm);
        }
        public static SALsAInstance GetInstance(int icm)
        {
            SALsA.instances.TryGetValue(icm, out SALsAInstance value);
            return value;
        }

        static SALsA()
        {

            GlobalLog = new Log();
            instances = new Dictionary<int, SALsAInstance>();
            // Initialise singletons;
            _ = Authentication.Instance;
            _ = Authentication.Instance.Cert;
            _ = Authentication.Instance.StorageCredentials;

        }
    }
}