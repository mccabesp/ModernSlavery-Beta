using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Extensions.AspNetCore;
using ModernSlavery.WebUI.Shared.Abstractions;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public class WebService : IWebService
    {
        public WebService(IMapper autoMapper, IHttpCache cache, IHttpSession session, IWebHostEnvironment hostingEnvironment, IShortCodesRepository shortCodesRepository, IWebTracker webTracker)
        {
            AutoMapper = autoMapper;
            Cache = cache;
            Session = session;
            HostingEnvironment = hostingEnvironment;
            ShortCodesRepository = shortCodesRepository;
            WebTracker = webTracker;
        }

        public IMapper AutoMapper { get; }
        public IHttpCache Cache { get; }
        public IHttpSession Session { get; }
        public IWebHostEnvironment HostingEnvironment { get; }
        public IShortCodesRepository ShortCodesRepository { get; }
        public IWebTracker WebTracker { get; }

    }
}
