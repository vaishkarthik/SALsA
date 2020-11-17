using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation
{
    public partial class Analyzer
    {
        private (Nullable<Guid> subscriptionId, string resourceGroupName, string VMName, DateTime startTime) AnalyzeICM()
        {
            var currentICM = SALsA.GetInstance(Id).ICM;
            var subscriptionId = GetSubscriptionId(currentICM);
            var resourceGroupName = currentICM.GetCustomField(Constants.AnalyzerResourceGroupField);
            var VMName = currentICM.GetCustomField(Constants.AnalyzerVMNameField);
            DateTime startTime;
            if (!DateTime.TryParse(currentICM.GetCustomField(Constants.AnalyzerStartTimeField), out startTime))
            {
                Log.Warning("Failed to parse DateTime");
            }
            return (subscriptionId, resourceGroupName, VMName, startTime);
        }

        private Nullable<Guid> GetSubscriptionId(ICM icm)
        {
            try
            {
                // Look for the custom field "subscription"
                String subscriptionId = SALsA.GetInstance(Id).ICM.GetCustomField(Constants.AnalyzerSubscriptionIdField);

                // Look in the ICM field for the subscriptionId
                if (!CheckIfSubscriptionIdIsValid(subscriptionId))
                {
                    Log.Verbose("Failed to get SubscriptionId from CustomField");
                    subscriptionId = icm.CurrentICM.SubscriptionId;
                    // Look in the ICM description for the subscriptionId
                    if (!CheckIfSubscriptionIdIsValid(subscriptionId))
                    {
                        Log.Verbose("Failed to get SubscriptionId from SubscriptionId ICM Field");
                        var regex = new Regex("[0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12}", RegexOptions.IgnoreCase);
                        Match m = regex.Match(icm.CurrentICM.Summary);
                        subscriptionId = m.Value;
                        // If we coudnt find it, fail it.
                        if (!CheckIfSubscriptionIdIsValid(subscriptionId))
                        {
                            Log.Error("Failed to find any SubscriptionId in the ICM");
                            throw new Exception("Failed to find valid SubscriptionId");
                        }
                    }
                }
                return Guid.Parse(Utility.DecodeHtml(subscriptionId));
            }
            catch (Exception ex)
            {
                Log.Error("Failed to find a valid subscription id for ICM : {0}", icm.CurrentICM.Id);
                Log.Exception(ex);
                return null;
            }
        }

        private bool CheckIfSubscriptionIdIsValid(string s)
        {
            Log.Verbose("Parsing strign for valid SubscriptionID : {0}", s);
            try
            {

                Guid tmp = new Guid();
                return Guid.TryParse(Utility.DecodeHtml(s), out tmp);
            }
            catch
            {
                return false;
            }
        }
    }
}
