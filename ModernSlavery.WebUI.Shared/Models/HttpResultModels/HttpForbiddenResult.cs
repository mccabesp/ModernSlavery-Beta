using System.Net;
using Microsoft.Extensions.Logging;

namespace ModernSlavery.WebUI.Shared.Models.HttpResultModels
{
    public class HttpForbiddenResult : HttpStatusViewResult
    {

        public HttpForbiddenResult(LogLevel logLevel = LogLevel.Warning) : this(null, logLevel) { }

        public HttpForbiddenResult(string statusDescription, LogLevel logLevel = LogLevel.Warning) : base(
            HttpStatusCode.Forbidden,
            statusDescription,
            logLevel) { }

    }
}
