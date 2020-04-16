using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public class WebService : IWebService
    {
        public WebService(IErrorViewModelFactory errorViewModelFactory, FeatureSwitchOptions featureSwitchOptions,
            IMapper autoMapper, IHttpCache cache, IHttpSession session, IWebHostEnvironment hostingEnvironment,
            IShortCodesRepository shortCodesRepository, IWebTracker webTracker,
            IEventLogger eventLogger)
        {
            ErrorViewModelFactory = errorViewModelFactory;
            FeatureSwitchOptions = featureSwitchOptions;
            AutoMapper = autoMapper;
            Cache = cache;
            Session = session;
            HostingEnvironment = hostingEnvironment;
            ShortCodesRepository = shortCodesRepository;
            WebTracker = webTracker;
            CustomLogger = eventLogger;
        }

        public IErrorViewModelFactory ErrorViewModelFactory { get; }
        public FeatureSwitchOptions FeatureSwitchOptions { get; }
        public IMapper AutoMapper { get; }
        public IHttpCache Cache { get; }
        public IHttpSession Session { get; }
        public IWebHostEnvironment HostingEnvironment { get; }
        public IShortCodesRepository ShortCodesRepository { get; }
        public IWebTracker WebTracker { get; }
        public IEventLogger CustomLogger { get; }
    }
}