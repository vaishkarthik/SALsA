using LivesiteAutomation;
using LivesiteAutomation.ManualRun;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SALsA_REST.Models
{
    public class ICMModel
    {
        private ConcurrentDictionary<int, Task> RunningPool;
        private static ICMModel instance = null;
        public static ICMModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ICMModel();
                }
                return instance;
            }
        }

        private ICMModel()
        {
            RunningPool = new ConcurrentDictionary<int, Task>();
        }

        public bool RunAutomation(int id, object obj = null)
        {
            if(IsRunning(id))
            {
                return false;
            }
            else
            {
                CreateAndRunInBackground(id, obj);
                return true;
            }
        }

        public bool IsRunning(int id)
        {
            return RunningPool.ContainsKey(id);
        }

        private void CreateAndRunInBackground(int id, object obj)
        {
            Task task = new Task(() => RunTask(id, obj));
            task.Start();
            RunningPool.TryAdd(id, task);
        }

        private void RunTask(int id, object obj)
        {
            LivesiteAutomation.Program.Run(id, obj);
            RunningPool.TryRemove(id, out _);
        }

    }
}