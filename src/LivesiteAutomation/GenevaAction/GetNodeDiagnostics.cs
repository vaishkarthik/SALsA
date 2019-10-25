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
        public static Task<ZipArchiveEntry> GetNodeDiagnosticsFilesByDeploymentIdorVMName(int icm, ShortRDFERoleInstance instance)
        {
            SALsA.GetInstance(icm)?.Log.Information("Calling GenevaAction GetNodeDiagnostics with params {0}", instance);
            var param = new GenevaOperations.GetNodeDiagnosticsDeployment
            {
                smefabrichostparam = instance.Fabric,
                smenodediagnosticstagparam = Constants.GetNodeDiagnosticsParam,
                smedeploymentidordeploymentparam = instance.DeploymentName,
                smevmnameparam = instance.InstanceName
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(icm, Constants.GetNodeDiagnosticsExtensionName, Constants.GetNodeDiagnosticsOperatorNameDeployment, actionParam).GetOperationFileOutputAsync(icm);

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                    task.Result != null ? Utility.ExtractZip(task.Result).Entries.Where(x => x.Name != "").First() : null
                ));
        }

        public static Task<Stream> GetNodeDiagnosticsFiles(int icm, string cluster, string nodeid, string startTime, string endTime)
        {
            SALsA.GetInstance(icm)?.Log.Information("Calling GenevaAction GetNodeDiagnostics (for host) with params cluster:{0} and nodeid:", cluster, nodeid);
            var param = new GenevaOperations.GetNodeDiagnosticsFiles
            {
                smefabrichostparam = cluster,
                smenodeidparam = nodeid,
                smenodediagnosticstypeparam = "",
                smenodediagnosticstagparam = Constants.GetNodeDiagnosticsFilesTagsParam,
                smestarttimeparam = startTime,
                smeendtimeparam = endTime
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(icm, Constants.GetNodeDiagnosticsExtensionName, Constants.GetNodeDiagnosticsOperatorNameFiles, actionParam).GetOperationFileOutputAsync(icm);

            return Task.Run(() => (
                    task.Result
                ));
        }
    }
}
