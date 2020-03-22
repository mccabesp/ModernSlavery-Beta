using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.WebUI.Shared.Models.HttpResultModels;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllowOnlyTrustedDomainsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var globalOptions = (GlobalOptions)context.HttpContext.RequestServices.GetService(typeof(GlobalOptions));
            var trustedIPDomains= globalOptions?.TrustedIPDomains.SplitI();
            if (trustedIPDomains != null && trustedIPDomains.Any())
            {
                string userHostAddress = context.HttpContext.GetUserHostAddress();
                if (string.IsNullOrWhiteSpace(userHostAddress) || !userHostAddress.IsTrustedAddress(trustedIPDomains))
                {
                    LogAttempt(context, globalOptions.TrustedIPDomains, userHostAddress);
                    context.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }

        private void LogAttempt(ActionExecutingContext context, string trustedIPDomains, string userHostAddress)
        {
            try
            {
                ILogger logger = context.HttpContext.RequestServices?.GetService<ILogger<AllowOnlyTrustedDomainsAttribute>>();
                logger = context.HttpContext.RequestServices?.GetService<ILogger>();
                if (logger == null)return;

                string controllerMessagePart = context.Controller == null || string.IsNullOrWhiteSpace(context.Controller.ToString())
                    ? "an unknown controller"
                    : $"controller {context.Controller}";

                string forbiddingReasonMessagePart = string.IsNullOrWhiteSpace(userHostAddress)
                    ? "since it was not possible to read its host address information"
                    : $"for address {userHostAddress} as it is not part of the trusted ips '{trustedIPDomains}'";

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
