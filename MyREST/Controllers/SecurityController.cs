using MyREST.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyREST.Controllers
{
    public class SecurityController : ApiController
    {

       
        [HttpPost]
        public async Task<string> Authenticate([FromUri] int UserType,[FromUri]int ParkID,[FromBody] User user)
        {
            UserRepository UserRepo = new UserRepository();

            if (await UserRepo.Login(user, UserType, ParkID))
            {
                return "Authenticated";
            }
            else
            {
                return "Not Authenticated";
            }
        }

       

    }
}
