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
        private (Guid subscriptionId, string resourceGroupName, string VMName, DateTime startTime) AnalyzeICM(Incident currentICM)
        {
            var subscriptionId = GetSubscriptionId(currentICM); ;
            var resourceGroupName = GetCustomField(currentICM, Constants.AnalyzerResourceGroupField);
            var VMName = GetCustomField(currentICM, Constants.AnalyzerVMNameField);
            DateTime startTime;
            _ = DateTime.TryParse(GetCustomField(currentICM, Constants.AnalyzerStartTimeField), out startTime);
            return (subscriptionId, resourceGroupName, VMName, startTime);
        }

        private Guid GetSubscriptionId(Incident incident)
        {
            try
            {
                // Look for the custom field "subscription"
                String subscriptionId = GetCustomField(incident, Constants.AnalyzerSubscriptionIdField);

                // Look in the ICM field for the subscriptionId
                if (!CheckIfSubscriptionIdIsValid(subscriptionId))
                {
                    Log.Instance.Verbose("Failed to get SubscriptionId from CustomField");
                    subscriptionId = incident.SubscriptionId;
                    // Look in the ICM description for the subscriptionId
                    if (!CheckIfSubscriptionIdIsValid(subscriptionId))
                    {
                        Log.Instance.Verbose("Failed to get SubscriptionId from SubscriptionId ICM Field");
                        var regex = new Regex("[0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12}", RegexOptions.IgnoreCase);
                        Match m = regex.Match(incident.Summary);
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
                Log.Instance.Error("Failed to find a valid subscription id for ICM : {0}", incident.Id);
                Log.Instance.Exception(ex);
                throw ex;
            }
        }

        private string GetCustomField(Incident incident, string lookup)
        {
            try
            {
                foreach (var fields in incident.CustomFieldGroups)
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
                Log.Instance.Error("Failed to find a valid value for <{0}> in ICM : {1}", lookup, incident.Id);
                Log.Instance.Exception(ex);
            }
            return null;

        }

        private bool CheckIfSubscriptionIdIsValid(string s)
        {
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
