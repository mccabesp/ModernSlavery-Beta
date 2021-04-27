using System;
using System.Threading.Tasks;
using IdentityServer4.Events;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.WebUI.Identity.Classes
{
    public class AuditEventSink : IEventSink
    {
        private readonly ILogger Logger;

        public AuditEventSink(ILogger<AuditEventSink> logger)
        {
            Logger = logger;
        }

        public Task PersistAsync(Event evt)
        {
            if (evt is UserLoginSuccessEvent loginSuccessEvent)
                Logger.LogInformation(
                    $"{loginSuccessEvent.Name}:{loginSuccessEvent.Message}: Name:{loginSuccessEvent.DisplayName}; Username:{loginSuccessEvent.Username}; IPAddress:{loginSuccessEvent.RemoteIpAddress};");
            else if (evt is UserLoginFailureEvent loginFailureEvent)
                Logger.LogWarning(
                    $"{loginFailureEvent.Name}:{loginFailureEvent.Message}: Username:{loginFailureEvent.Username}; IPAddress:{loginFailureEvent.RemoteIpAddress};");
            else if (evt is UserLogoutSuccessEvent logoutSuccessEvent)
                Logger.LogInformation(
                    $"{logoutSuccessEvent.Name}:{logoutSuccessEvent.Message}: Username:{logoutSuccessEvent.DisplayName}; IPAddress:{logoutSuccessEvent.RemoteIpAddress};");
            else
                switch (evt.EventType)
                {
                    case EventTypes.Failure:
                        Logger.LogCritical(new Exception(evt.SerializeError()),evt.Name);
                        break;
                    case EventTypes.Error:
                        Logger.LogError(new Exception(evt.SerializeError()), evt.Name);
                        break;
                    case EventTypes.Information:
                    case EventTypes.Success:
                        Logger.LogInformation(evt.SerializeError());
                        break;
                }

            return Task.CompletedTask;
        }
    }
}