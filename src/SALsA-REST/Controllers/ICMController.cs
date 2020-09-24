using SALsA_REST.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SALsA_REST.Controllers
{
    public class ICMController : ApiController
    {
        // GET: api/ICM/5
        public bool Get(int id)
        {
            return ICMModel.RunAutomation(id);
        }

        /*
        // GET: api/ICM
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // POST: api/ICM
        public void Post([FromBody]string value)
        {
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
