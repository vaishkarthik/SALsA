using System;
using System.Collections.Generic;
using System.IO;
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
            string content;
            try
            {
                var filePath= LivesiteAutomation.SALsA.GetInstance(id)?.Log?.LogFullPath;
                using (FileStream fileStream = new FileStream(
                        filePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite))
                {
                    using (StreamReader streamReader = new StreamReader(fileStream))
                    {
                        content = streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                content = ex.ToString();
            }
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(content, System.Text.Encoding.UTF8, "text/plain");
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
