using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyREST.Models
{
    public class ParkTicket
    {
        public int TicketID { get; set; }
        public string DriverName { get; set; }
        public string License { get; set; }
        public string ParkName { get; set; }
        public DateTime Date { get; set; }

    }
}