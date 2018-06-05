using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace MyREST.Models
{
    public class User
    {
        public int ID { get; set; }       
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }  
        public Citizen NationalRegInfo { get; set; }
        public List<UserActivity> UserActivity { get; set; }
        public string FingerPrint { get; set; }
    }


    public class UserActivity
    {
        public DateTime Date { get; set; }
        public string Activity { get; set; }
    }
}