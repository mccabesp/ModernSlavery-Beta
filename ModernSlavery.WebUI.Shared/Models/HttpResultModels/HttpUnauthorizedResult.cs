using System.Net;
using Microsoft.Extensions.Logging;

namespace ModernSlavery.WebUI.Shared.Models.HttpResultModels
{
    public class HttpUnauthorizedResult : HttpStatusViewResult
    {

        public HttpUnauthorizedResult(LogLevel logLevel = LogLevel.Warning) : this(null, logLevel) { }

        public HttpUnauthorizedResult(string statusDescription, LogLevel logLevel = LogLevel.Warning) : base(
            HttpStatusCode.Unauthorized,
            statusDescription,
            logLevel) { }

    }

}
