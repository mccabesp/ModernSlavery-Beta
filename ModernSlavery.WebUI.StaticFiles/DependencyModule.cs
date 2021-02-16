using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
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
        private readonly StaticHeaderOptions _staticHeaderOptions;
        private readonly SharedOptions _sharedOptions;
        private readonly ResponseCachingOptions _responseCachingOptions;

        public DependencyModule(
            ILogger<DependencyModule> logger,
            StaticHeaderOptions staticHeaderOptions,
            SharedOptions sharedOptions,
            ResponseCachingOptions responseCachingOptions)
        {
            _logger = logger;
            _staticHeaderOptions = staticHeaderOptions;
            _sharedOptions = sharedOptions;
            _responseCachingOptions = responseCachingOptions;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //TODO: Register service dependencies here
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //TODO: Configure autofac dependencies here
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            var app = lifetimeScope.Resolve<IApplicationBuilder>();

            //Set the static file options
            var staticFileOptions = new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    //Caching static files is required to reduce connections since the default behavior of checking if a static file has changed and returning a 304 still requires a connection.
                    if (_responseCachingOptions.Enabled && _responseCachingOptions.StaticCacheSeconds > 0)
                    {
                        var headers = context.Context.Response.GetTypedHeaders();

                        headers.CacheControl = new CacheControlHeaderValue
                        {                             
                            Public = true,
                            MaxAge = TimeSpan.FromSeconds(_responseCachingOptions.StaticCacheSeconds)
                        };
                    }

                    foreach (var key in _staticHeaderOptions.Keys)
                    {
                        //Get the header value
                        var value = _staticHeaderOptions[key];
                        
                        //Add the header
                        context.Context.SetResponseHeader(key, value);
                    }
                }
            };


            //When in Development mode StaticWebAssetBasePath should be is obtained from "**\obj\**\staticwebassets\ModernSlavery.Hosts.Web.StaticWebAssets.xml" but its not working
            // Include un-bundled js + css folders to serve the source files in dev environment
            if (_sharedOptions.IsDevelopment() && !string.IsNullOrWhiteSpace(_sharedOptions.DevelopmentWebroot))
            {
                staticFileOptions.FileProvider = new PhysicalFileProvider(_sharedOptions.DevelopmentWebroot);
            }


            app.UseStaticFiles(staticFileOptions);            
        }

        public void RegisterModules(IList<Type> modules)
        {
            //TODO: Add any linked dependency modules here
        }


        
    }
}