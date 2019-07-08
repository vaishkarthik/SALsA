using LivesiteAutomation;
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

        public bool RunAutomation(int id)
        {
            if(IsRunning(id))
            {
                return false;
            }
            else
            {
                CreateAndRunInBackground(id);
                return true;
            }
        }

        public bool IsRunning(int id)
        {
            return RunningPool.ContainsKey(id);
        }

        private void CreateAndRunInBackground(int id)
        {
            Task task = new Task(() => RunTask(id));
            task.Start();
            RunningPool.TryAdd(id, task);
        }

        private void RunTask(int id)
        {
            LivesiteAutomation.Program.Run(id);
            RunningPool.TryRemove(id, out _);
        }
            
    }
}