using LivesiteAutomation.Json2Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    public partial class GenevaActions
    {
        // TODO : make sovereign cloud available
        public static Task<ZipArchiveEntry> GetNodeDiagnostics(int icm, ShortRDFERoleInstance instance)
        {
            SALsA.GetInstance(icm)?.Log.Information("Calling GenevaAction GetNodeDiagnostics with params {0}", instance);
            var param = new GenevaOperations.GetNodeDiagnostics
            {
                smefabrichostparam = instance.Fabric,
                smenodediagnosticstagparam = Constants.GetNodeDiagnosticsParam,
                smedeploymentidordeploymentparam = instance.DeploymentName,
                smevmnameparam = instance.InstanceName
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(icm, Constants.GetNodeDiagnosticsExtensionName, Constants.GetNodeDiagnosticsOperatorName, actionParam).GetOperationFileOutputAsync(icm);

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                    task.Result != null ? Utility.ExtractZip(task.Result).Entries.Where(x => x.Name != "").First() : null
                ));
        }
    }
}
