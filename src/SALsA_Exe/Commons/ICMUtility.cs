using SALsA.LivesiteAutomation.Commons;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation
{
    public partial class ICM
    {
        public int Id { get; private set; }
        public string SAS { get; private set; }
        public Incident CurrentICM { get; private set; }
        private ConcurrentBag<string> MessageQueue = new ConcurrentBag<string>();

        public ICM(int icmId)
        {
            this.Id = icmId;

            this.CurrentICM = PopulateICMInfo(icmId);
            SALsA.GetInstance(Id)?.Log.Verbose("Got response for IMC {0}", icmId);
            SALsA.GetInstance(Id)?.Log.Verbose(CurrentICM);
        }

        public string GetCustomField(string lookup)
        {
            try
            {
                return GetCustomFieldInternal(CurrentICM.CustomFieldGroups, lookup);
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(Id)?.Log.Error("Failed to find a valid value for <{0}> in ICM : {1}", lookup, Id);
                SALsA.GetInstance(Id)?.Log.Exception(ex);
            }
            return null;
        }

        public bool PostICMHeader(string head = Constants.ICMInfoHeaderHtml)
        {
            string entry = head + Utility.UrlToHml(Constants.ICMInfoReportName, Constants.ICMInfoReportEndpoint + this.Id.ToString(), 24);
            SALsA.GetInstance(Id)?.Log.Verbose("Adding to ICM String {0}", entry);
            var discussion = ICM.GetICMDiscussion(this.Id);
            foreach (var de in discussion)
            {
                if (de.SubmittedBy == Constants.ICMIdentityName && Utility.DecodeHtml(de.Text).CompareTo(Utility.DecodeHtml(entry)) == 0)
                {
                    SALsA.GetInstance(Id)?.Log.Verbose("Did not add entry to ICM since already sent", this.Id);
                    return false;
                }
            }
            try
            {
                return ICM.PostDiscussion(this.Id, entry);
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(Id)?.Log.Error("Failed to add discussion element to ICM {0}", Id);
                SALsA.GetInstance(Id)?.Log.Exception(ex);
                return false;
            }
        }

        public bool QueueICMDiscussion(string entry, bool htmlfy = true)
        {
            if (entry == null)
            {
                return false;
            }
            SALsA.GetInstance(Id)?.Log.Verbose("Adding to ICM String {0}", entry);
            if (htmlfy)
            {
                try
                {
                    entry = Utility.EncodeHtml(entry);
                }
                catch (Exception ex)
                {
                    SALsA.GetInstance(Id)?.Log.Warning("Failed to html encode {0}, will use raw input", entry);
                    SALsA.GetInstance(Id)?.Log.Exception(ex);
                }
            }

            try
            {
                MessageQueue.Add(entry.ToString());
                return true;
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(Id)?.Log.Error("Failed to add discussion element to ICM {0}", this.Id);
                SALsA.GetInstance(Id)?.Log.Exception(ex);
                return false;
            }
        }

        public void EmptyMessageQueue()
        {
            SALsA.GetInstance(Id)?.Log.Verbose("Empty Message Queue with {0} elements", MessageQueue.Count);
            if (MessageQueue.IsEmpty || SALsA.GetInstance(Id).State == SALsAState.Ignore || SALsA.GetInstance(Id).State == SALsAState.MissingInfo) return; // Ignore the ICM
            string reason = null;
            try
            {
                var message = Utility.GenerateICMHTMLPage(Id, MessageQueue.ToArray(), SALsA.GetInstance(Id)?.Log.StartTime);
                SAS = BlobStorageUtility.UploadICMRun(Id, message);
                if (Constants.ShouldPostToICM)
                {
                    message = Utility.UrlToHml(String.Format("SALsA Logs {0}",
                        DateTime.ParseExact(SALsA.GetInstance(Id)?.Log.StartTime, "yyMMddTHHmmss", null)
                            .ToString("yyyy-MM-ddTHH:mm:ssZ")), SAS);
                    if (message == null) throw new ArgumentNullException("Message is null, please verify run log");
                    var body = new StringContent(Utility.ObjectToJson(new Incident.DescriptionPost(message)));
                    body.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = Client.PatchAsync(BuildUri(this.Id), body).Result;
                    reason = response.Content.ReadAsStringAsync().Result;
                    response.EnsureSuccessStatusCode();
                    SALsA.GetInstance(Id)?.Log.Verbose("Got response for ICM {0}", this.Id);
                }
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(Id).State = SALsAState.UnknownException;
                SALsA.GetInstance(Id)?.Log.Error("Failed to add message element to ICM {0}. Reason : {1}", this.Id, reason);
                SALsA.GetInstance(Id)?.Log.Exception(ex);
            }
            finally
            {
                MessageQueue = new ConcurrentBag<string>(); // Dispose of our current one
            }
        }

        // Tools
        public DateTime ICMImpactStartTime()
        {
            DateTime date;
            DateTime.TryParse(GetCustomField(Constants.AnalyzerStartTimeField), out date);
            if (date == null)
            {
                date = this.CurrentICM.ImpactStartDate;
            }
            if (date == null)
            {
                date = DateTime.Today.AddDays(-7).ToUniversalTime();
            }
            return date;
        }
    }
}
