using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class IPAddressFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var authorisationBusinessLogic = context.HttpContext.RequestServices.GetRequiredService<IAuthorisationBusinessLogic>();
            var userHostAddress = context.HttpContext.GetUserHostAddress();
            if (!authorisationBusinessLogic.IsTrustedAddress(userHostAddress))
            {
                var sharedOptions= context.HttpContext.RequestServices.GetRequiredService<SharedOptions>();
                LogAttempt(context, sharedOptions.TrustedDomainsOrIPs, userHostAddress);
                context.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                return;
            }

            base.OnActionExecuting(context);
        }

        private void LogAttempt(ActionExecutingContext context, string trustedDomainsOrIPs, string userHostAddress)
        {
            try
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<IPAddressFilterAttribute>>();

                var controllerMessagePart =
                    context.Controller == null || string.IsNullOrWhiteSpace(context.Controller.ToString())
                        ? "an unknown controller"
                        : $"controller {context.Controller}";

                var forbiddingReasonMessagePart = string.IsNullOrWhiteSpace(userHostAddress)
                    ? "since it was not possible to read its host address information"
                    : $"for address {userHostAddress} as it is not part of the trusted ips '{trustedDomainsOrIPs}'";

                logger.LogWarning($"Access to {controllerMessagePart} was forbidden {forbiddingReasonMessagePart}");
            }
            catch (Exception ex)
            {
                // Don't care if there was an error during logging
                // It's more important that the code continues
            }
        }
    }
}