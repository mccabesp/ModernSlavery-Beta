using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Extensions.AspNetCore;
using System;
using System.Collections.Generic;
using System.Text;

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
