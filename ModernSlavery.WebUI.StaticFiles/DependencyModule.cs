using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.WebUI.StaticFiles
{
    public class DependencyModule : IDependencyModule
    {
        private readonly ILogger _logger;
        private readonly SharedOptions _sharedOptions;
        private readonly ResponseCachingOptions _responseCachingOptions;

        public DependencyModule(
            ILogger<DependencyModule> logger,
            SharedOptions sharedOptions,
            ResponseCachingOptions responseCachingOptions)
        {
            _logger = logger;
            _sharedOptions = sharedOptions;
            _responseCachingOptions = responseCachingOptions;
        }

        public void Register(IDependencyBuilder builder)
        {
            //TODO: Register dependencies here
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            var app = lifetimeScope.Resolve<IApplicationBuilder>();

            var fileRepository = lifetimeScope.Resolve<IFileRepository>();
            
            //Set the static file options
            var staticFileOptions=new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    //Caching static files is required to reduce connections since the default behavior of checking if a static file has changed and returning a 304 still requires a connection.
                    if (_responseCachingOptions.StaticCacheSeconds > 0)ctx.Context.SetResponseCache(_responseCachingOptions.StaticCacheSeconds);
                }
            };

            // Include un-bundled js + css folders to serve the source files in dev environment
            if (_sharedOptions.IsLocal())
            {
                var wwwroot = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, this.GetType().Namespace, "wwwroot");
                staticFileOptions.FileProvider = new PhysicalFileProvider(wwwroot);
            }

            app.UseStaticFiles(staticFileOptions);

            //Ensure ShortCodes, SicCodes and SicSections exist on remote 
            Task.WaitAll(
                fileRepository.PushRemoteFileAsync(Filenames.ShortCodes, _sharedOptions.DataPath),
                fileRepository.PushRemoteFileAsync(Filenames.SicCodes, _sharedOptions.DataPath),
                fileRepository.PushRemoteFileAsync(Filenames.SicSections, _sharedOptions.DataPath)
            );
        }
    }
}