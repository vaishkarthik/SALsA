using SALsA_REST.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SALsA_REST.Controllers
{
    public class ManualRunIIDController : ApiController
    {
        static HttpResponseMessage response = null;
        // GET: manualrun/iid
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get()
        {   
            if (response == null)
            {
                response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(System.IO.File.ReadAllText(
                    System.Web.Hosting.HostingEnvironment.MapPath("~/HTMLTemplate/ManualIID.html")),
                    System.Text.Encoding.UTF8, "text/html");
            }

            return response;
        }

        // POST: manualrun/iid
        [System.Web.Http.HttpPost]
        public string Post([FromBody]LivesiteAutomation.ManualRun.ManualRun_ICM iid) 
        {
            return iid.VMName;
            /*
            LivesiteAutomation.ManualRun.ManualRun_IID iid = new LivesiteAutomation.ManualRun.ManualRun_IID
            {
                SubscriptionID = new Guid(subid),
                ResourceGroupName = rgroup,
                VMName = rgroup
                //Region = region
            };
            //int.TryParse(vmssid, out iid.Instance);
            */
        }

        /*
        // GET: api/ICM
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // PUT: api/ICM/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ICM/5
        public void Delete(int id)
        {
        }
        */
    }
}
