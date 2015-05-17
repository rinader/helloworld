using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Crossover.Builder.Web.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/<controller>
        [Authorize]
        public string Get()
        {
            var userName = this.RequestContext.Principal.Identity.Name;
            return string.Format("Hello, {0}.", userName);
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}