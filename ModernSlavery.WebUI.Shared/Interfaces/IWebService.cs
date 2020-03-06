using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Extensions.AspNetCore;

namespace ModernSlavery.WebUI.Shared.Abstractions
{
    public interface IWebService
    {
        IMapper AutoMapper { get; }
        IHttpCache Cache { get; }
        IHttpSession Session { get; }
        IWebHostEnvironment HostingEnvironment { get; }
        IShortCodesRepository ShortCodesRepository { get; }
        IWebTracker WebTracker { get; }
    }
}
