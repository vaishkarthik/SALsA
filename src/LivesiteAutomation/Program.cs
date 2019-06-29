﻿using System;
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
            // Initialise singletons;
            _ = Log.Instance;
            _ = Authentication.Instance;

            var myIcm = new ICM("129901901");
            myIcm.GetICM();
            _ =  new Analyzer(ref myIcm);
            myIcm.GetICMDiscussion();
            myIcm.AddICMDiscussion("Jedi : Hello from SALsA");
        }
    }
}