using LivesiteAutomation.Json2Class;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace LivesiteAutomation
{
    public partial class Analyzer
    {
        RDFESubscription AnalyzeRDFESubscription(Guid subscriptionId)
        {
            try
            {
                var rdfe = GenevaActions.GetRDFESubscription(subscriptionId).Result;
                var rdfeSubscription = AnalyzeRDFESubscriptionResult(rdfe);

                return rdfeSubscription;
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Unable to get or analyse the RDFE subscription {0} for ICM {1}", this.SubscriptionId, Log.Instance.Icm);
                Log.Instance.Exception(ex);
                return null;
            }
        }

        private RDFESubscription AnalyzeRDFESubscriptionResult(string xml)
        {
            XmlDocument doc = new XmlDocument();
            xml = xml.Replace("=== <", "<").Replace("> ===", ">").Trim();
            doc.LoadXml(xml);
            var json = JsonConvert.SerializeXmlNode(doc);
            var obj = Utility.JsonToObject<dynamic>(json.Replace("\"#text\"", "text"));
            var deployments = obj.Subscription.HostedService;
            var rdfeSubscription = new RDFESubscription();
            foreach (var element in deployments)
            {
                List<string> tmp = null;
                try
                {
                    tmp = Utility.JsonToObject<List<string>>(Utility.ObjectToJson(element.Deployment.text));
                }
                catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
                {
                    Log.Instance.Warning("No deployment found for : {0}", element.ToString());
                    continue;
                }
                    var dep = String.Join("", tmp).Trim().Replace("\r\n", "\n").Split('\n').Select(e => e.Trim()).ToArray();
                // Sometimes, we have multiple elements in same line, so we look itup with a regex and divide them.
                var dep2d = dep.Select(e => Regex.Split(e, @"(,\s)(?=\w+:)")).ToArray();
                dep = dep2d.SelectMany(x => x).ToArray();
                // Remvoe everytrhing that is not *:*
                dep = dep.Where(f => Regex.Match(f, @".*:.*").Success).ToArray();

                for (int i = 0; i < dep.Length; ++i)
                {
                    if(!dep[i].Contains(":"))
                    {
                        continue;
                    }
                    var split = dep[i].Split(new[] { ':' }, 2);
                    if (split.Length == 1)
                    {
                        split.Append("");
                    }
                    dep[i] = String.Format("\"{0}\":\"{1}\"", split[0].Trim(), split[1].Trim());
                }
                string newJson = String.Format("{{{0}}}", String.Join(",", dep));
                rdfeSubscription.deployments.Add(Utility.JsonToObject<RDFEDeployment>(newJson));
            }
            return rdfeSubscription;
        }
    }
}
