using System;
using System.Net;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Classes.ErrorMessages
{
    [Serializable]
    public class CustomError
    {
        public CustomError(int code, string description)
        {
            Code = code;
            Description = description;
        }

        public CustomError(HttpStatusCode httpStatusCode, string description)
            : this(httpStatusCode.ToInt32(), description)
        {
        }

        public int Code { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return Description;
        }

        public HttpException ToHttpException()
        {
            var codeConvertedToStatus = Enum.IsDefined(typeof(HttpStatusCode), Code)
                ? (HttpStatusCode) Code
                : HttpStatusCode.NotFound;

            return new HttpException(codeConvertedToStatus, Description);
        }
    }
}