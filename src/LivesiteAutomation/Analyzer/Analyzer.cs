using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LivesiteAutomation
{
    class Analyzer
    {
        public Guid SubscriptionId { get; private set; }
        public Analyzer(ref ICM icm)
        {
            SubscriptionId = GetSubscriptionId(icm.CurrentICM);
        }

        private Guid GetSubscriptionId(Incident incident)
        {
            try
            { 
                String subscriptionId = null;

                // Look for the custom field "subscription"
                foreach (var fields in incident.CustomFieldGroups)
                {
                    var sid = fields.CustomFields.Find(x => x.Name == Constants.AnalyzerSubscriptionIdField);
                    if (sid != null)
                    {
                        subscriptionId = sid.Value;
                        break;
                    }
                }
                // Look in the ICM field for the subscriptionId
                if (ChecIfSubscriptionIdIsValid(subscriptionId))
                {
                    Log.Instance.Verbose("Failed to get SubscriptionId from CustomField");
                    subscriptionId = incident.SubscriptionId;
                    // Look in the ICM description for the subscriptionId
                    if (ChecIfSubscriptionIdIsValid(subscriptionId))
                    {
                        Log.Instance.Verbose("Failed to get SubscriptionId from SubscriptionId ICM Field");
                        var regex = new Regex("[0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12}", RegexOptions.IgnoreCase);
                        Match m = regex.Match(incident.Summary);
                        subscriptionId = m.Value;
                        // If we coudnt find it, fail it.
                        if (ChecIfSubscriptionIdIsValid(subscriptionId))
                        {
                            Log.Instance.Error("Failed to find any SubscriptionId in the ICM");
                            throw new Exception("Failed to find valid SubscriptionId");
                        }
                    }
                }
                return Guid.Parse(Utility.StripHTML(subscriptionId));
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to find a valid subscription id for ICM : {0}", incident.Id);
                Log.Instance.Exception(ex);
                throw ex;
            }
        }

        private bool ChecIfSubscriptionIdIsValid(string s)
        {
            Guid tmp = new Guid();
            return Guid.TryParse(Utility.StripHTML(s), out tmp);
        }
    }
}
