﻿using SALsA.LivesiteAutomation.Json2Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation
{
    public partial class GenevaActions
    {
        // TODO : make sovereign cloud available
        public static Task<ZipArchiveEntry> GetNodeDiagnosticsFilesByDeploymentIdorVMName(int icm, ShortRDFERoleInstance instance)
        {
            Log.Information("Calling GenevaAction GetNodeDiagnostics with params {0}", instance);
            var param = new GenevaOperations.GetNodeDiagnosticsDeployment
            {
                smefabrichostparam = instance.Fabric,
                smenodediagnosticstagparam = Constants.GetNodeDiagnosticsParam,
                smedeploymentidordeploymentparam = instance.DeploymentId,
                smevmnameparam = instance.InstanceName
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(icm, Constants.GetNodeDiagnosticsExtensionName, Constants.GetNodeDiagnosticsOperatorNameDeployment, actionParam).GetOperationFileOutputAsync(icm);

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                    task.Result != null ? Utility.ExtractZip(task.Result).Entries.Where(x => x.Name != "").First() : null
                ));
        }

        public static Task<Stream> GetNodeDiagnosticsFiles(int icm, string cluster, string nodeid, string logType, string startTime, string endTime)
        {
            Log.Information("Calling GenevaAction GetNodeDiagnostics (for host type: {0}) with params cluster:{1} and nodeid:{2}", logType, cluster, nodeid);
            var param = new GenevaOperations.GetNodeDiagnosticsFiles
            {
                smefabrichostparam = cluster,
                smenodeidparam = nodeid,
                smenodediagnosticstypeparam = "",
                smenodediagnosticstagparam = logType,
                smestarttimeparam = startTime,
                smeendtimeparam = endTime
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(icm, Constants.GetNodeDiagnosticsExtensionName, Constants.GetNodeDiagnosticsOperatorNameFiles, actionParam).GetOperationFileOutputAsync(icm);

            return Task.Run(() => (
                    task.Result
                ));
        }
        public static Task<ZipArchiveEntry> GetNodeDiagnosticsFilesByContainerId(int icm, ShortRDFERoleInstance instance)
        {
            Log.Information("Calling GenevaAction GetNodeDiagnostics with params {0}", instance);
            var param = new GenevaOperations.GetNodeDiagnosticsFiles
            {
                smefabrichostparam = instance.Fabric,
                smenodeidparam = instance.NodeId.ToString(),
                smenodediagnosticstypeparam = "",
                smenodediagnosticstagparam = Constants.GetNodeDiagnosticsParam,
                smecontaineriddiagnosticsfileparam = instance.ContainerID.ToString()
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(icm, Constants.GetNodeDiagnosticsExtensionName, Constants.GetNodeDiagnosticsOperatorNameFiles, actionParam).GetOperationFileOutputAsync(icm);

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                    task.Result != null ? Utility.ExtractZip(task.Result).Entries.Where(x => x.Name != "").First() : null
                ));
        }

    }
}
