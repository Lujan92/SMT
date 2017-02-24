using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMT.Models
{
    public class ResultViewModel
    {
        public bool result { get; set; }
        public string message { get; set; }
        public dynamic data { get; set; }

        public ResultViewModel() { }

        public ResultViewModel(bool result, string message, dynamic data)
        {
            this.result = result;
            this.message = message;
            this.data = data;
        }

        public ResultViewModel(Exception e)
        {
            result = false;
            message = e.InnerException != null ? e.InnerException.Message : e.Message;
        }


    }

    public class ResultPaginado<T>
    {
        public List<T> data { get; set; }
        public int total { get; set; }
    }
}