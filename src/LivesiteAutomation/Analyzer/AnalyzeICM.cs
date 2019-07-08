using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    public partial class Analyzer
    {
        private (Guid subscriptionId, string resourceGroupName, string VMName, DateTime startTime) AnalyzeICM()
        {
            var currentICM = ICM.Instance;
            var subscriptionId = GetSubscriptionId(currentICM); ;
            var resourceGroupName = GetCustomField(currentICM, Constants.AnalyzerResourceGroupField);
            var VMName = GetCustomField(currentICM, Constants.AnalyzerVMNameField);
            DateTime startTime;
            if (!DateTime.TryParse(GetCustomField(currentICM, Constants.AnalyzerStartTimeField), out startTime))
            {
                Log.Instance.Warning("Failed to parse DateTime");
            }
            return (subscriptionId, resourceGroupName, VMName, startTime);
        }

        private Guid GetSubscriptionId(ICM icm)
        {
            try
            {
                // Look for the custom field "subscription"
                String subscriptionId = GetCustomField(icm, Constants.AnalyzerSubscriptionIdField);

                // Look in the ICM field for the subscriptionId
                if (!CheckIfSubscriptionIdIsValid(subscriptionId))
                {
                    Log.Instance.Verbose("Failed to get SubscriptionId from CustomField");
                    subscriptionId = icm.CurrentICM.SubscriptionId;
                    // Look in the ICM description for the subscriptionId
                    if (!CheckIfSubscriptionIdIsValid(subscriptionId))
                    {
                        Log.Instance.Verbose("Failed to get SubscriptionId from SubscriptionId ICM Field");
                        var regex = new Regex("[0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12}", RegexOptions.IgnoreCase);
                        Match m = regex.Match(icm.CurrentICM.Summary);
                        subscriptionId = m.Value;
                        // If we coudnt find it, fail it.
                        if (!CheckIfSubscriptionIdIsValid(subscriptionId))
                        {
                            Log.Instance.Error("Failed to find any SubscriptionId in the ICM");
                            throw new Exception("Failed to find valid SubscriptionId");
                        }
                    }
                }
                return Guid.Parse(Utility.DecodeHtml(subscriptionId));
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to find a valid subscription id for ICM : {0}", icm.CurrentICM.Id);
                Log.Instance.Exception(ex);
                throw ex;
            }
        }

        private string GetCustomField(ICM icm, string lookup)
        {
            try
            {
                foreach (var fields in icm.CurrentICM.CustomFieldGroups)
                {
                    var sid = fields.CustomFields.Find(x => x.Name == lookup);
                    if (sid != null)
                    {
                        return sid.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to find a valid value for <{0}> in ICM : {1}", lookup, icm.CurrentICM.Id);
                Log.Instance.Exception(ex);
            }
            return null;

        }

        private bool CheckIfSubscriptionIdIsValid(string s)
        {
            Log.Instance.Verbose("Parsing strign for valid SubscriptionID : {0}", s);
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
