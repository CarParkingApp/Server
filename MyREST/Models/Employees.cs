using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyREST.Models
{
    public class Employees
    {
        public int EmpId { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public string Position { get; set; }

        public string Gender { get; set; }
    }
}