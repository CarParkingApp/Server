using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;


namespace MyREST.Controllers
{
    public class NiraController : ApiController
    {

        NIRA nira = new NIRA();
      
        [HttpGet]
        public bool CheckPerson([FromUri]string nin)
        {
            return nira.CheckPerson(nin);
        }

        [HttpGet]
        public int GetPerson([FromUri]string nin)
        {
            return nira.GetPerson(nin);
        }

        [HttpPost]
        public bool CheckPersonf([FromBody] string fingerprint)
        {
            return nira.CheckPerson(new ImageConverter().ToBitmap(fingerprint));
        }

        [HttpPost]
        public int GetPersonf([FromBody] string fingerprint)
        {
            return nira.GetPerson(new ImageConverter().ToBitmap(fingerprint));
        }
    }
}