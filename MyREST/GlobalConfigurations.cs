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
        private string PortName;
        private string Title;

        public string SystemTitle
        {
            get
            {
                Title = ConfigurationManager.AppSettings["SystemTitle"];
                return Title;
            }
            set
            {
                Title = value;
            }

        }

        public string SerialPortName
        {
            get
            {
                PortName = ConfigurationManager.AppSettings["PortName"];
                return PortName;
            }
            set
            {
                PortName = value;
            }
        }

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