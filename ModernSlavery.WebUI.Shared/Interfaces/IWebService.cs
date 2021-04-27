using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Options;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Models;

namespace ModernSlavery.WebUI.Shared.Interfaces
{
    public interface IWebService
    {
        IErrorViewModelFactory ErrorViewModelFactory { get; }
        FeatureSwitchOptions FeatureSwitchOptions { get; }

        IWebHostEnvironment HostingEnvironment { get; }
        IMapper AutoMapper { get; }
        IHttpCache Cache { get; }
        IHttpSession Session { get; }
        IShortCodesRepository ShortCodesRepository { get; }
        IWebTracker WebTracker { get; }
        IEventLogger CustomLogger { get; }
    }
}