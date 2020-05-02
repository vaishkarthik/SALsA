using SALsA_REST.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace SALsA_REST.Controllers
{
    public class ManualRunRdfeTenantController : ApiController
    {
        static string Content = null;
        // GET: manualrun/rdfe/tenant
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            if (Content == null)
            {
                Content = System.IO.File.ReadAllText(
                    System.Web.Hosting.HostingEnvironment.MapPath("~/HTMLTemplate/ManualRdfeTenant.html"));
            }
            response.Content = new StringContent(Content,
                    System.Text.Encoding.UTF8, "text/html");

            return response;
        }

        [System.Web.Http.HttpPost]
        public bool Post(HttpRequestMessage request)
        {
            string ret = request.Content.ReadAsStringAsync().Result;
            var parsed = HttpUtility.ParseQueryString(ret);
            var dic = parsed.AllKeys.ToDictionary(k => k, k => parsed[k]);
            var obj = LivesiteAutomation.Utility.JsonToObject<LivesiteAutomation.ManualRun.ManualRun_RDFE_Tenant>(
                LivesiteAutomation.Utility.ObjectToJson(dic));
            int icm = int.Parse(dic["icmid"]);
            if (obj != null && LivesiteAutomation.ICM.CheckIfICMExists(icm))
            {
                ICMModel.Instance.RunAutomation(icm, obj);
                return true;
            }
            else
            {
                return false;
            }
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
