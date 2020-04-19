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
    public class ToolsController : ApiController
    {
        static StringContent Content = null;
        // GET: tools
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            if (Content == null)
            {
                response.Content = new StringContent(System.IO.File.ReadAllText(
                    System.Web.Hosting.HostingEnvironment.MapPath("~/HTMLTemplate/tools.html")),
                    System.Text.Encoding.UTF8, "text/html");
            }
            else
            {
                response.Content = Content;
            }

            return response;
        }
    }
}
