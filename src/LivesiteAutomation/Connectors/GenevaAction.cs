using Microsoft.Azure.Geneva.Actions.Client;
using Microsoft.Azure.Geneva.Actions.Client.Credentials;
using Microsoft.Azure.Geneva.Actions.Client.Models;
using Microsoft.Azure.Geneva.Actions.Client.Extensions;
using Microsoft.WindowsAzure.Security.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LivesiteAutomation
{
    public partial class GenevaActions
    { 
        private class GenevaAction
        {
            private ClientHomeSts sts;
            private ConnectionParameters cp;
            private GenevaActionsRestAPIClient client;
            private OperationRequest operationRequest;
            private OperationResult operationResult;
            string operationName;
            string extensionName;
            Dictionary<string, string> actionParam;
            ActionsEnvironments actionsEnvironments;

            internal GenevaAction(int icm, string extensionName, string operationName, Dictionary<string, string> actionParam, ActionsEnvironments actionsEnvironments = ActionsEnvironments.Public)
            {
                try
                {
                    this.extensionName = extensionName;
                    this.operationName = operationName;
                    this.actionParam = actionParam;
                    this.actionsEnvironments = actionsEnvironments;

                    SALsA.GetInstance(icm)?.Log.Verbose("Creating GenevaAction for {0}: {1}, with parameters : ", extensionName, operationName,
                        actionParam.Select(kvp => kvp.Key + ": " + kvp.Value.ToString()));

                    sts = new ClientHomeSts(new Uri("https://ch1-dsts.dsts.core.windows.net"));
                    cp = ConnectionParameters.Create(actionsEnvironments, Authentication.Instance.Cert, sts, X509CertCredentialType.SubjectNameCredential);
                    client = new GenevaActionsRestAPIClient(cp);
                    SALsA.GetInstance(icm)?.Log.Verbose("Client created for {0}: {1}", extensionName, operationName);

                    var operationDetails = client.Extension.GetOperationDetails(extensionName, operationName);
                    SALsA.GetInstance(icm)?.Log.Verbose("operationDetails id : ", operationDetails.Id);

                    operationRequest = new OperationRequest
                    {
                        // TODO : a smarter way than takign first endpoint by default
                        Endpoint = operationDetails.AllowedEndpoints[0],
                        Extension = extensionName,
                        Id = operationDetails.Id,
                        Parameters = actionParam
                    };
                    SALsA.GetInstance(icm)?.Log.Verbose("operationRequest populated. Extension : ", operationRequest.Extension);
                }
                catch (Exception ex)
                {
                    SALsA.GetInstance(icm)?.Log.Error("Failed GenevaAction {0}: {1}", extensionName, operationName);
                    SALsA.GetInstance(icm)?.Log.Exception(ex);
                }
            }

            private async Task RunOperationManualPollAsync(int icm)
            {
                try
                {
                    ExecuteOperationResponse operationRunning = await client.Operations.RunOperationAsync(operationRequest);
                    while (true)
                    {
                        OperationResult status = await client.Operations.GetOperationStatusAsync(operationRunning.Id);
                        if (status.Status.IsStateComplete())
                        {
                            // Operation reached a final state, get the result.
                            operationResult = await client.Operations.GetOperationResultsAsync(operationRunning.Id);
                            SALsA.GetInstance(icm)?.Log.Information("Operation has completed execution for {0}: {1}. Operation Result is:{2}{3}", extensionName, operationName, System.Environment.NewLine, operationResult.ResultMessage);
                            // We upload all results of all operations
                            SALsA.GetInstance(icm)?.TaskManager.AddTask(
                                BlobStorage.UploadText(icm, String.Format("action/{1}-{0}_{2}.txt", extensionName, operationName, SALsA.GetInstance(icm)?.Log.UID),
                                operationResult.ResultMessage));
                            return;
                        }
                        // Warning: Setting too short a delay could result in requests being throttled
                        SALsA.GetInstance(icm)?.Log.Verbose("Operation <{0}: {1}> is still in process, polling status again in 5 seconds", extensionName, operationName);
                        await Task.Delay(5000);
                    }
                }

                catch (Exception ex)
                {
                    SALsA.GetInstance(icm)?.Log.Error("Operation <{0}: {1}> execution failed.", extensionName, operationName);
                    SALsA.GetInstance(icm)?.Log.Exception(ex);
                }
            }

            public async Task<Stream> GetOperationFileOutputAsync(int icm)
            {
                try
                {
                    await RunOperationManualPollAsync(icm);
                    var fileOutputStream = await client.Operations.GetOperationFileOutputAsync(operationResult?.ExecutionId);
                    SALsA.GetInstance(icm)?.Log.Information("Operation <{0}: {1}> get stream Success", extensionName, operationName);
                    return fileOutputStream;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Operation <{0}: {1}> output to file failed");
                    SALsA.GetInstance(icm)?.Log.Exception(ex);
                    return null;
                }
            }

            public async Task<String> GetOperationResultOutputAsync(int icm)
            {
                try
                {
                    await RunOperationManualPollAsync(icm);
                    SALsA.GetInstance(icm)?.Log.Information("Operation <{0}: {1}> get result Success", extensionName, operationName);
                    return operationResult.ResultMessage;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Operation <{0}: {1}> output to file failed");
                    SALsA.GetInstance(icm)?.Log.Exception(ex);
                    return null;
                }
            }
        }
    }
}
