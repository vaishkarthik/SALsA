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
    class GenevaAction
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

        GenevaAction(string extensionName, string operationName, Dictionary<string, string> actionParam, ActionsEnvironments actionsEnvironments = ActionsEnvironments.Public)
        {
            try
            {
                this.extensionName = extensionName;
                this.operationName = operationName;
                this.actionParam = actionParam;
                this.actionsEnvironments = actionsEnvironments;

                Log.Instance.Verbose("Creating GenevaAction for {0}: {1}, with parameters : ", extensionName, operationName,
                    actionParam.Select(kvp => kvp.Key + ": " + kvp.Value.ToString()));

                sts = new ClientHomeSts(new Uri("https://ch1-dsts.dsts.core.windows.net"));
                cp = ConnectionParameters.Create(actionsEnvironments, Authentication.Instance.Cert, sts, X509CertCredentialType.SubjectNameCredential);
                client = new GenevaActionsRestAPIClient(cp);
                Log.Instance.Verbose("Client created for {0}: {1}", extensionName, operationName);

                var operationDetails = client.Extension.GetOperationDetails(extensionName, operationName);
                Log.Instance.Verbose("operationDetails id : ", operationDetails.Id);

                operationRequest = new OperationRequest
                {
                    // TODO : a smarter way than takign first endpoint by default
                    Endpoint = operationDetails.AllowedEndpoints[0],
                    Extension = extensionName,
                    Id = operationDetails.Id,
                    Parameters = actionParam
                };
                Log.Instance.Verbose("operationRequest populated. Endpoint : ", operationRequest.Endpoint);
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed GenevaAction {0}: {1}", extensionName, operationName);
                Log.Instance.Exception(ex);
            }
        }

        private async Task RunOperationManualPollAsync()
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
                        Log.Instance.Information("Operation has completed execution for {0}: {1}. Operation Result is:{2}{3}", extensionName, operationName, System.Environment.NewLine, operationResult.ResultMessage);
                        return;
                    }
                    // Warning: Setting too short a delay could result in requests being throttled
                    Log.Instance.Verbose("Operation <{0}: {1}> is still in process, polling status again in 5 seconds", extensionName, operationName);
                    await Task.Delay(5000);
                }
            }

            catch (Exception ex)
            {
                Log.Instance.Error("Operation <{0}: {1}> execution failed.", extensionName, operationName);
                Log.Instance.Exception(ex);
            }
        }

        private async Task<Stream> GetOperationFileOutputAsync()
        {
            try
            {
                var fileOutputStream = await client.Operations.GetOperationFileOutputAsync(operationResult?.ExecutionId);
                Log.Instance.Information("Operation <{0}: {1}> get stream Success", extensionName, operationName);
                return fileOutputStream;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Operation <{0}: {1}> output to file failed");
                Log.Instance.Exception(ex);
                return null;
            }
        }

        public async Task<String> GetOperationResultOutputAsync()
        {
            try
            {
                var result = await client.Operations.GetBatchOperationResultsWithHttpMessagesAsync(operationResult?.ExecutionId);
                var output = await result.Response.Content.ReadAsStringAsync();
                Log.Instance.Information("Operation <{0}: {1}> get result Success", extensionName, operationName);
                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Operation <{0}: {1}> output to file failed");
                Log.Instance.Exception(ex);
                return null;
            }
        }
    }
}
