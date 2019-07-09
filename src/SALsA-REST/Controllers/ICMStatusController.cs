using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SALsA_REST.Controllers
{
    public class ICMStatusController : ApiController
    {
        
        // GET: api/ICMStatus
        public IEnumerable<string> Get()
        {
            return LivesiteAutomation.SALsA.ListInstances().Select(x => Convert.ToString(x)).ToArray();
        }

        // GET: api/ICMStatus/5
        public HttpResponseMessage Get(int id)
        {
            string text;
            try
            {
                var fileName = LivesiteAutomation.SALsA.GetInstance(id)?.Log?.LogFullPath;
                text = System.IO.File.ReadAllText(fileName);
            }
            catch (Exception ex)
            {
                text = ex.ToString();
            }
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(text, System.Text.Encoding.UTF8, "text/plain");
            return response;
        }
        /*
        // POST: api/ICMStatus
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/ICMStatus/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ICMStatus/5
        public void Delete(int id)
        {
        }
        */
    }
}
