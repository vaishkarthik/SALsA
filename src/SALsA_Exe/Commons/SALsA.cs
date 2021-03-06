﻿using SALsA.LivesiteAutomation.Json2Class.RDFESubscriptionWrapper;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using SALsA.General;
using static SALsA.General.Utility;

namespace SALsA.LivesiteAutomation
{
    public class SALsA
    {
        public class SALsAInstance
        {
            public ICM ICM;
            public TaskManager TaskManager;
            private SALsAState _state;
            public SALsAState State { get { return _state; } set { _state = value; TableStorage.AppendEntity(ICM.Id, value, Log.SAS, ICM.SAS); } }
            public SALsAInstance(int icm)
            {
                Log.ResetLog();
                Log.Id = icm;
                this.ICM = new ICM(icm);
                this.TaskManager = new TaskManager(icm);
                this.State = SALsAState.Running;
            }

            public void RefreshTable()
            {
                TableStorage.AppendEntity(ICM.Id, State, Log.SAS, ICM.SAS);
            }
        }

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
            instances = new Dictionary<int, SALsAInstance>();
            // Initialise singletons;
            _ = Authentication.Instance;
            _ = Authentication.Instance.Cert;
            _ = Authentication.Instance.BlobStorageCredentials;
            _ = Authentication.Instance.TableStorageClient;
        }
    }
}