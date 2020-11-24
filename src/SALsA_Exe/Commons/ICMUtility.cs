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
        private readonly object __lockObj = new object();

        public ICM(int icmId)
        {
            this.Id = icmId;

            this.CurrentICM = PopulateICMInfo(icmId);
            Log.Verbose("Got response for IMC {0}", icmId);
            Log.Verbose(CurrentICM);
        }

        public string GetCustomField(string lookup)
        {
            try
            {
                return GetCustomFieldInternal(CurrentICM.CustomFieldGroups, lookup);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to find a valid value for <{0}> in ICM : {1}", lookup, Id);
                Log.Exception(ex);
            }
            return null;
        }
        public string GetCustomField(string[] lookups)
        {
            foreach (var lookup in lookups)
            {
                var value = GetCustomFieldInternal(CurrentICM.CustomFieldGroups, lookup);
                if (String.IsNullOrWhiteSpace(value) == false)
                { return value; }

            }
            return null;
        }

        public bool PostICMHeader(string head = Constants.ICMInfoHeaderHtml)
        {
            string entry = head + "<br>" +  Utility.UrlToHml(Constants.ICMInfoReportName, Constants.ICMInfoReportEndpoint + this.Id.ToString(), 24);
            Log.Verbose("Adding to ICM String {0}", entry);
            var discussion = ICM.GetICMDiscussion(this.Id);
            var cur = SALsA.GetInstance(this.Id).ICM.CurrentICM;
            if (Constants.ICMTeamsAlwaysPostHeader.Contains(
                SALsA.GetInstance(this.Id).ICM.CurrentICM.OwningTeamId.Split('\\').First(),
                StringComparer.InvariantCultureIgnoreCase) == false)
            {
                foreach (var de in discussion)
                {
                    if (de.SubmittedBy == Constants.ICMIdentityName || de.Text.Contains(Constants.ICMInfoHeaderHtml))
                    {
                        Log.Verbose("Did not add entry to ICM since already sent", this.Id);
                        return false;
                    }
                }
            }
            else 
            {
                var currentTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                entry = String.Format("[{0}] {1}", currentTime, entry);
            }
            try
            {
                return ICM.PostDiscussion(this.Id, entry);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to add discussion element to ICM {0}", Id);
                Log.Exception(ex);
                return false;
            }
        }

        public bool QueueICMDiscussion(string entry, bool htmlfy = true)
        {
            if (entry == null)
            {
                return false;
            }
            Log.Verbose("Adding to ICM String {0}", entry);
            if (htmlfy)
            {
                try
                {
                    entry = Utility.EncodeHtml(entry);
                }
                catch (Exception ex)
                {
                    Log.Warning("Failed to html encode {0}, will use raw input", entry);
                    Log.Exception(ex);
                }
            }

            try
            {
                MessageQueue.Add(entry.ToString());
                RefreshTempPage(true);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to add discussion element to ICM {0}", this.Id);
                Log.Exception(ex);
                return false;
            }
        }

        public void EmptyMessageQueue()
        {
            Log.Verbose("Empty Message Queue with {0} elements", MessageQueue.Count);
            if (MessageQueue.IsEmpty || SALsA.GetInstance(Id).State == SALsAState.Ignore || SALsA.GetInstance(Id).State == SALsAState.MissingInfo) return; // Ignore the ICM
            string reason = null;
            try
            {
                RefreshTempPage();
                if (Constants.ShouldPostToICM)
                {
                    var message = Utility.UrlToHml(String.Format("SALsA Logs {0}",
                        DateTime.ParseExact(Log.StartTime, "yyMMddTHHmmss", null)
                            .ToString("yyyy-MM-ddTHH:mm:ssZ")), SAS);
                    if (message == null) throw new ArgumentNullException("Message is null, please verify run log");
                    var body = new StringContent(Utility.ObjectToJson(new Incident.DescriptionPost(message)));
                    body.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = Client.PatchAsync(BuildUri(this.Id), body).Result;
                    reason = response.Content.ReadAsStringAsync().Result;
                    response.EnsureSuccessStatusCode();
                    Log.Verbose("Got response for ICM {0}", this.Id);
                }
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(Id).State = SALsAState.UnknownException;
                Log.Error("Failed to add message element to ICM {0}. Reason : {1}", this.Id, reason);
                Log.Exception(ex);
            }
            finally
            {
                MessageQueue = new ConcurrentBag<string>(); // Dispose of our current one
            }
        }

        private void RefreshTempPage(bool isTemp = false)
        {
            lock (__lockObj)
            {
                var message = Utility.GenerateICMHTMLPage(Id, MessageQueue.ToArray(), Log.StartTime, isTemp);
                SAS = BlobStorageUtility.UploadICMRun(Id, message);
                SALsA.GetInstance(Id)?.RefreshTable();
            }
        }

        // Tools
        public DateTime ICMImpactStartTime()
        {
            DateTime date;
            DateTime.TryParse(GetCustomField(Constants.AnalyzerStartTimeField), out date);
            if (date == null || date.Ticks == 0)
            {
                date = this.CurrentICM.ImpactStartDate;
            }
            if (date == null || date.Ticks == 0)
            {
                date = DateTime.Today.AddDays(-7).ToUniversalTime();
            }
            return date;
        }
    }
}
