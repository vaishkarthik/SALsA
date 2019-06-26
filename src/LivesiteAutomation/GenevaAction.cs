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

        GenevaAction(string extensionName, string operationName, Dictionary<string, string> actionParam, ActionsEnvironments aEnv = ActionsEnvironments.Public)
        {
            try
            {
                Log.Instance.Verbose("Creating GenevaAction for {0}: {1}, with parameters : ", extensionName, operationName,
                    actionParam.Select(kvp => kvp.Key + ": " + kvp.Value.ToString()));

                sts = new ClientHomeSts(new Uri("https://ch1-dsts.dsts.core.windows.net"));
                cp = ConnectionParameters.Create(aEnv, Authentication.Instance.cert, sts, X509CertCredentialType.SubjectNameCredential);
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

        private async Task RunOperationAndPollAsync()
        {
            try
            {
                OperationResult operationResult = await client.Operations.RunOperationAndPollForResultsAsync(operationRequest);

                Console.WriteLine("\nOperation Result using PollForResultsAsync method: \n" + operationResult.ResultMessage);
            }

            catch (Exception e)
            {
                Console.WriteLine("Operation execution failed. \nError Message: " + e.Message);
            }
        }

        private async Task RunOperationManualPollAsync(GenevaActionsRestAPIClient client, OperationRequest operationRequest)
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
                        OperationResult operationResult = await client.Operations.GetOperationResultsAsync(operationRunning.Id);
                        Console.WriteLine("\nOperation has completed execution. Operation Result is: \n" + operationResult.ResultMessage);
                        return;
                    }
                    // Warning: Setting too short a delay could result in requests being throttled
                    Console.WriteLine("Operation is still in process, polling status again in 5 seconds");
                    await Task.Delay(5000);
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Operation execution failed. \nError Message: " + e.Message);
            }
        }

        private async Task GetOperationFileOutputAsync(GenevaActionsRestAPIClient client, OperationRequest operationRequest, string filePath)
        {
            try
            {
                OperationResult operationResult = await client.Operations.RunOperationAndPollForResultsAsync(operationRequest);

                Console.WriteLine("\nOperation Result using PollForResultsAsync method: \n" + operationResult.ResultMessage);

                var fileOutputStream = await client.Operations.GetOperationFileOutputWithHttpMessagesAsync(operationResult.ExecutionId);
                var fileData = await fileOutputStream.Response.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes(filePath, fileData);

                Console.WriteLine("File written at path: " + filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Operation execution failed. \nError Message: " + e.Message);
            }
        }
    }
}
