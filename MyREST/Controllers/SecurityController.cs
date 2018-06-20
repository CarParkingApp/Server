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

        UserRepository UserRepo = new UserRepository();

        [HttpPost]
        public async Task<string> Authenticate([FromUri] int UserType,[FromUri]int ParkID,[FromBody] User user)
        {
            

            return (await UserRepo.Login(user, UserType, ParkID)).ToString();
          
        }

       [HttpGet]
       public async Task<User> Login([FromUri]string Email,[FromUri]string Password)
        {
            return await UserRepo.Login(Email, Password);
        }
        
    }
}
