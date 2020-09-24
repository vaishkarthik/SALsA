using LivesiteAutomation.Json2Class.RDFESubscriptionWrapper;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using static LivesiteAutomation.Utility;

namespace LivesiteAutomation
{
    public class SALsA
    {
        public enum State
        {
            Running,
            Done,
            NotFound,
            UnknownException,
            MissingSubscriptionId,
        }
        public class SALsAInstance
        {
            public Log Log;
            public ICM ICM;
            public TaskManager TaskManager;
            public State State;
            public SALsAInstance(int icm)
            {
                this.Log = new Log(icm);
                this.ICM = new ICM(icm);
                this.TaskManager = new TaskManager(icm);
                this.State = State.Running;
            }
        }

        public static Log GlobalLog;
        private static Dictionary<int, SALsAInstance> instances;

        public static void AddInstance(int icm)
        {
            SALsA.instances[icm] = new SALsA.SALsAInstance(icm);
        }
        public static void RemoveInstance(int icm)
        {
            try
            {
                SALsA.instances.Remove(icm);
            }
            catch { /* Best effor. It is okay to fail. */ };
        }
        public static SALsAInstance GetInstance(int icm)
        {
            SALsA.instances.TryGetValue(icm, out SALsAInstance value);
            return value;
        }
        public static List<int> ListInstances()
        {
            return new List<int>(instances.Keys);
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