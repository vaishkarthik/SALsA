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
    public static class ICMModel
    {
        public static bool RunAutomation(int id, object obj = null)
        {
            if (IsRunning(id))
            {
                return false;
            }
            else
            {
                CreateAndRunInBackground(id, obj);
                return true;
            }
        }

        public static bool IsRunning(int id)
        {
            return SALsA.GetInstance(id)?.State == SALsA.State.Running;
        }

        private static void CreateAndRunInBackground(int id, object obj)
        {
            Task task = new Task(() => RunTask(id, obj));
            task.Start();
        }

        private static void RunTask(int id, object obj)
        {
            LivesiteAutomation.Program.Run(id, obj);
        }

    }
}