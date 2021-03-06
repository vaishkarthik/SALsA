﻿using SALsA.LivesiteAutomation.Json2Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation
{
    public partial class GenevaActions
    {
        // TODO : make sovereign cloud available
        public static Task<Stream> InspectIaaSDiskForARMVM(int icm, ARMDeployment deployment)
        {
            var param = new GenevaOperations.InspectIaaSDiskForARMVM
            {
                smecrpregion = deployment.Location,
                smeresourcegroupname = deployment.ResourceGroups,
                smevmname = deployment.Name,
                wellknownscopedsubscriptionid = deployment.Subscriptions,
                smelogextractmode = Constants.InspectIaaSDiskForARMVMMode,
                smeskiptostep = Constants.InspectIaaSDiskForARMVMStep,
                smetimeoutinmins = Constants.InspectIaaSDiskForARMVMTimeout
            };
            Log.Information("Calling InspectIaaSDiskForARMVM with params {0}", param);
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(icm, Constants.InspectIaaSDiskForARMVMExtensionName, Constants.InspectIaaSDiskForARMVMOperationName, actionParam).GetOperationFileOutputAsync(icm);

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                task.Result
                ));
        }
        // TODO : make sovereign cloud available
        public static Task<Stream> InspectIaaSDiskForARMVM(int icm, ARMDeployment deployment, int id)
        {
            var param = new GenevaOperations.InspectIaaSDiskForARMVMVMSS
            {
                smecrpregion = deployment.Location,
                smeresourcegroupname = deployment.ResourceGroups,
                smevmssname = deployment.Name,
                wellknownscopedsubscriptionid = deployment.Subscriptions,
                smelogextractmode = Constants.InspectIaaSDiskForARMVMMode,
                smeskiptostep = Constants.InspectIaaSDiskForARMVMStep,
                smetimeoutinmins = Constants.InspectIaaSDiskForARMVMTimeout,
                smevmssinstanceid = id
            };
            Log.Information("Calling InspectIaaSDiskForARMVM of id:{0} with params {1}", id, param);
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(icm, Constants.InspectIaaSDiskForARMVMExtensionName, Constants.InspectIaaSDiskForARMVMSSOperationName, actionParam).GetOperationFileOutputAsync(icm);

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                task.Result
                ));
        }
    }
}
