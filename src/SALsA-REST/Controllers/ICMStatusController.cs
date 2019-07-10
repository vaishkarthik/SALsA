using SALsA_REST.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Http;
using System.Web.UI;

namespace SALsA_REST.Controllers
{
    public class ICMStatusController : ApiController
    {
        
        // GET: api/ICMStatus
        public HttpResponseMessage Get()
        {
            // TODO remvoe this from the API and have it in the MVC part
            string template = "<!DOCTYPE html><html><body><link rel=icon href=/favicon.ico><h2 align=\"center\">SALsA Status</h2><table style=\"width:50%\" align=\"center\" >  <tr style=\"padding-left:30px;\" align=\"left\">    <th>ICM</th>    <th>Status</th>   </tr>  __INSERT__</table></body></html>";
            var icms = LivesiteAutomation.SALsA.ListInstances();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < icms.Count; i++)
            {
                var status = ICMModel.Instance.IsRunning(icms[i]) ? "Running" : "Done";
                sb.Append(String.Format("<tr><td><a href=\"/api/icm/status/{0}\">{0}</a></td><td>{1}</td></tr>", icms[i], status));
            }
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(template.Replace("__INSERT__", sb.ToString()), System.Text.Encoding.UTF8, "text/html");
            
            return response;
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
