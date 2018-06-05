using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace MyREST.Models
{
    public class Citizen
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender Sex{ get; set; }
        public string NIN { get; set; }
        public string FingerPrint { get; set; }

    }

    public enum Gender
    {
        Male,Female
    }
}