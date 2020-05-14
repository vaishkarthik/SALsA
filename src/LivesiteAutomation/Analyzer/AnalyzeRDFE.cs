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
                var rdfe = GenevaActions.GetRDFESubscription(Id, subscriptionId).Result;
                var rdfeSubscription = AnalyzeRDFESubscriptionResult(rdfe);

                return rdfeSubscription;
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(Id)?.Log.Error("Unable to get or analyse the RDFE subscription {0}", this.SubscriptionId);
                SALsA.GetInstance(Id)?.Log.Exception(ex);
                return null;
            }
        }

        private RDFESubscription AnalyzeRDFESubscriptionResult(string xml)
        {
            var openB = Guid.NewGuid().ToString();
            var closeB = Guid.NewGuid().ToString();
            xml = xml.Replace("=== <", openB).Replace("> ===", closeB).Replace("&", "&amp;").Trim();
            xml = xml.Replace("<", "&lt;").Replace(" > ", "&gt;").Trim();
            xml = xml.Replace(openB, "<").Replace(closeB, ">");
            xml = xml.Replace("xmlns:xsd", "xmlns_xsd").Replace("xmlns:xsi", "xmlns_xsi");
            var xmlArray = xml.Split( new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None );
            var comp = new Regex(@"^\s*\w*(Label|LastRefreshTime):.*", RegexOptions.Singleline | RegexOptions.Compiled);
            for (int i = 0; i < xmlArray.Length; ++i)
            {
                if(comp.IsMatch(xmlArray[i]))
                {
                    var pieces = xmlArray[i].Split(new[] { ':' }, 2);
                    pieces[1] = WebUtility.UrlEncode(pieces[1]);
                    xmlArray[i] = String.Format("{0}:{1}", pieces[0], pieces[1]);
                }
            }
            xml = string.Join(Environment.NewLine, xmlArray);
            var serializer = new XmlSerializer(typeof(Json2Class.RDFESubscriptionWrapper.Subscription));
            using (var stream = new StringReader(xml))
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
                SALsA.GetInstance(Id)?.Log.Warning("No deployment found for : {0}", element.Text[0]);
                SALsA.GetInstance(Id)?.Log.Warning(ex);
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
