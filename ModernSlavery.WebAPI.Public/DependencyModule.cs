using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.WebAPI.Public
{
    public class DependencyModule : IDependencyModule
    {
        private readonly ILogger _logger;
        public DependencyModule(
            ILogger<DependencyModule> logger
        //TODO Add any required IOptions here
        )
        {
            _logger = logger;
            //TODO set any required local IOptions here
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    // Filter out other controllers
                    var assemblyName = ((ControllerActionDescriptor)apiDesc.ActionDescriptor).ControllerTypeInfo.Assembly.GetName().Name;
                    var currentAssemblyName = GetType().Assembly.GetName().Name;
                    return currentAssemblyName == assemblyName;
                });

                c.SwaggerDoc("Version 1", new OpenApiInfo
                {
                    Version = "Version 1",
                    Title = "Modern Slavery Statement Summary API",
                    Description = "Public Web API for returning summary data for published modern slavery statements",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Modern Slavery Unit",
                        Email = "modernslaveryunit@homeoffice.com",
                        Url = new Uri("https://twitter.com/modernslavery"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under LICX",
                        Url = new Uri("https://example.com/license"),
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //Register dependencies here
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            var app = lifetimeScope.Resolve<IApplicationBuilder>();

            
            //Configure dependencies here
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(options=> { options.RouteTemplate = "Api/{documentName}/ModernSlaverySummaryApi.json"; });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(options => 
            {
                options.SwaggerEndpoint("/Api/Version 1/ModernSlaverySummaryApi.json", "Modern Slavery Statement Summary API");
                options.RoutePrefix = "Api";
                options.InjectStylesheet("/Api/ModernSlaverySummaryApi.css");
                //options.InjectJavascript("/assets/javascripts/jquery-1.11.3.min.js");
                //options.InjectJavascript("/Api/ModernSlaverySummaryApi.js");
                options.DocumentTitle = "Modern Slavery Statement Summary API";
            });
        }

        public void RegisterModules(IList<Type> modules)
        {
            //Register references dependency modules
        }
    }
}
