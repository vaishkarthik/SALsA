using SALsA.LivesiteAutomation.Json2Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation
{
    public partial class GenevaActions
    {
        // TODO : make sovereign cloud available
        public static Task<String> GetContainerSettings(int icm, ShortRDFERoleInstance dep)
        {
            Log.Information("Calling GenevaAction GetContainerSettings with ContainerId: {0}", dep.ContainerID);
            var param = new GenevaOperations.GetContainerSettings
            {
                smefabrichostparam = dep.Fabric,
                smecontaineridparam = dep.ContainerID.ToString()
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            return new GenevaAction(icm, Constants.GetContainerSettingsExtensionName, Constants.GetContainerSettingsOperatorName, actionParam).GetOperationResultOutputAsync(icm);
        }
    }
}
