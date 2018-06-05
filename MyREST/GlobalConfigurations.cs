using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace MyREST
{
    public class GlobalConfigurations
    {
        private string conn;
        public string ConnectionString
        {
            get
            {
                conn = ConfigurationManager.ConnectionStrings["SQLDataConnection"].ConnectionString;
                return conn;
            }

            set
            {
                conn = value;
            }
        }
        
        public string NIRAConnectionString
        {
            get
            {
                conn = ConfigurationManager.ConnectionStrings["NIRADataConnection"].ConnectionString;
                return conn;
            }
            set
            {
                conn = value;
            }
        }
    }
}