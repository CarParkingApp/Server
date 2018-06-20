using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyREST
{
    [Flags]
    public enum ResponseCode { NO_CONNECTION = 404,SUCCESS=200 };

    public class Response
    {
        public bool Valid { get; set; }
        public int Code { get; set; }
        public string Data { get; set; }
        public string Message { get; set; }
    }

   
}