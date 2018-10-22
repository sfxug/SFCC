using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace SFCC.Common.Models
{
    public class ServiceResponse<T>
    {
        public HttpResponseMessage Response { get; set; }
        public bool Authorized { get; set; }
        public string Message { get; set; }
        public T Result { get; set; }
    }
}