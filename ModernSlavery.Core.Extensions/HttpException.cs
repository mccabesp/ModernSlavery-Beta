using System;
using System.Net;

namespace ModernSlavery.Core.Extensions
{
    public class HttpException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public object Value { get; set; }
        public HttpException(HttpStatusCode statusCode, string message = null, object value = null) : base(message)
        {
            StatusCode = statusCode;
            Value = value;
        }

        public HttpException(int statusCode, string message = null, object value=null) : this((HttpStatusCode)statusCode, message, value)
        {
        }
    }
}