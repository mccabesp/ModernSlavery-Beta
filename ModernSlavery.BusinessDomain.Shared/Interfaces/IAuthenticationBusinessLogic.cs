using System;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IAuthenticationBusinessLogic
    {
        TimeSpan GetUserLoginLockRemaining(User user);
    }
}