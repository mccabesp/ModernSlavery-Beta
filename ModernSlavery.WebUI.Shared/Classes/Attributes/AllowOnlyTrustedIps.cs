﻿using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllowOnlyTrustedDomainsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sharedOptions = (SharedOptions) context.HttpContext.RequestServices.GetService(typeof(SharedOptions));
            var userHostAddress = context.HttpContext.GetUserHostAddress();
            if (string.IsNullOrWhiteSpace(userHostAddress) || !sharedOptions.IsTrustedAddress(userHostAddress))
            {
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
                ILogger logger = context.HttpContext.RequestServices
                    ?.GetService<ILogger<AllowOnlyTrustedDomainsAttribute>>();
                logger = context.HttpContext.RequestServices?.GetService<ILogger>();
                if (logger == null) return;

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