using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SHS.Sage.Services.Controllers
{
    public class HL7Controller : ApiController
    {
        // GET: api/HL7
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/HL7/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/HL7
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/HL7/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/HL7/5
        public void Delete(int id)
        {
        }
    }
}
