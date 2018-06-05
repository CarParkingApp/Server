using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Web;

namespace MyREST.Models
{
    public class ParkingYard
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Reference { get; set; }
        public string Place_ID { get; set; }
        public String Image { get; set; }
        public YardLocation Location { get; set; }
        public YardSpaces Spaces { get; set; }
        public List<Activity> ParkActivities { get; set; }

        public class YardLocation
        {
            public int id { get; set; }
            public string Name { get; set; }
            public double Longitude { get; set; }
            public double Latitude { get; set; }
        }

        public class YardSpaces
        {
            public int Space_ID { get; set; }
            public int Count { get; set; }

            ///<summary>
            ///Displays the Number Of Spaces Used in the Parking Yard.
            ///</summary>
            public int UsedSpaces { get; set; }
        }

        public class Activity
        {
            public DateTime Date { get; set; }
            public String ActivityName { get; set; }
        }
        public class Activities
        {
            public string License { get; set; }
            public string Owner { get; set; }
            public int ParkID { get; set; }
            public DateTime IN { get; set; }
            public DateTime OUT { get; set; }
        }

    }
}