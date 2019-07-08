using LivesiteAutomation.Json2Class;
using Microsoft.IdentityModel.Protocols.WSFederation.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

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
                Log.Instance.Error("Unable to get or analyse the RDFE subscription {0} for ICM {1}", this.SubscriptionId, ICM.Instance.Id);
                Log.Instance.Exception(ex);
                return null;
            }
        }

        private RDFESubscription AnalyzeRDFESubscriptionResult(string xml)
        {
            XmlDocument doc = new XmlDocument();
            xml = xml.Replace("=== <", "<").Replace("> ===", ">").Replace("&", "&amp;").Trim();
            xml = xml.Replace("xmlns:xsd", "xmlns_xsd").Replace("xmlns:xsi", "xmlns_xsi");

            var serializer = new XmlSerializer(typeof(Json2Class.RDFESubscriptionWrapper.Subscription)); using (var stream = new StringReader(xml))
            using (var reader = XmlReader.Create(stream))
            {
                var result = (Json2Class.RDFESubscriptionWrapper.Subscription)serializer.Deserialize(reader);
                
                var multiDeployments = result.HostedService;
                var rdfeSubscription = new RDFESubscription();
                foreach (var element in multiDeployments)
                {
                    var deployments = BuildDeployment(element);
                    if (deployments != null)
                    {
                        rdfeSubscription.deployments = rdfeSubscription.deployments.Concat(deployments).ToList();
                    }
                }
                return rdfeSubscription;
            }

        }
        
        private List<RDFEDeployment> BuildDeployment(Json2Class.RDFESubscriptionWrapper.SubscriptionHostedService element)
        {
            try
            {
                List<RDFEDeployment> deployments = new List<RDFEDeployment>();
                // TODO : add element.Deployment.ServiceConfiguration.osFamily
                string deploymentInfo = String.Join(Environment.NewLine, element.Text);
                deploymentInfo += Environment.NewLine + String.Join(Environment.NewLine, element.Deployment.Text);
                deploymentInfo += Environment.NewLine + element.LastCreateDeploymentInProductionSlotTracking;
                deploymentInfo += Environment.NewLine + String.Join(Environment.NewLine, element.ExtendedProperties);
                var deploymentInfoTmp = deploymentInfo.Split(Environment.NewLine.ToArray()).Select(x => x.Trim()).ToArray();
                var deploymentInfoClass = PrepClassFromFakeXML(deploymentInfoTmp);
                string deploymentJson = String.Format("{{{0}}}", String.Join(",", deploymentInfoClass));
                var rdfeDeployment = Utility.JsonToObject<RDFEDeployment>(deploymentJson);
                rdfeDeployment.RoleInstances = new List<RDFERoleInstance>();
                foreach (var r in element.Deployment.Role)
                {
                    string roleInfo = String.Join(Environment.NewLine, r.Text);
                    foreach (var i in r.RoleInstance)
                    {
                        var instanceInfo = String.Join(Environment.NewLine, i.Text);
                        instanceInfo += Environment.NewLine + String.Join(Environment.NewLine, i.VM);
                        instanceInfo += Environment.NewLine + String.Join(Environment.NewLine, roleInfo);
                        var tmp = instanceInfo.Split(Environment.NewLine.ToArray()).Select(x => x.Trim()).ToArray();
                        var dep = PrepClassFromFakeXML(tmp);
                        string instance = String.Format("{{{0}}}", String.Join(",", dep));
                        var roleInstance = Utility.JsonToObject<RDFERoleInstance>(instance);
                        rdfeDeployment.RoleInstances.Add(roleInstance);
                    }
                    deployments.Add(rdfeDeployment);
                }
                return deployments;
            }
            catch (Exception ex)
            {
                Log.Instance.Warning("No deployment found for : {0}", element.Text[0]);
                Log.Instance.Warning(ex);
                return null;
            }
        }
        
        private string[] PrepClassFromFakeXML(string[] tmp)
        {
            var dep = String.Join("\n", tmp).Trim().Replace("\r\n", "\n").Split('\n').Select(e => e.Trim()).ToArray();
            // Sometimes, we have multiple elements in same line, so we look itup with a regex and divide them.
            var dep2d = dep.Select(e => Regex.Split(e, @"(,\s)(?=\w+:)")).ToArray();
            dep = dep2d.SelectMany(x => x).ToArray();
            // Remvoe everytrhing that is not *:*
            dep = dep.Where(f => Regex.Match(f, @".*:.*").Success).ToArray();

            for (int i = 0; i < dep.Length; ++i)
            {
                if (!dep[i].Contains(":"))
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
            return dep;
        }
    }
}
