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

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Modern Slavery Statement API",
                    Description = "Web API for returning organisation data and information on their modern slavery statements",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Stephen McCabe",
                        Email = "stephen.mccabe@cadenceinnova.com",
                        Url = new Uri("https://twitter.com/mccabesp"),
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
            app.UseSwagger(options=> { options.RouteTemplate = "Api/{documentName}/OpenApi.json"; });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/Api/v1/OpenApi.json", "Modern Slavery Statement Service API");
                c.RoutePrefix = "Api";
            });
        }

        public void RegisterModules(IList<Type> modules)
        {
            //Register references dependency modules
        }
    }
}
