using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Autofac;
using AutoFixture;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using Swashbuckle.AspNetCore.Filters;

namespace ModernSlavery.WebAPI.Public
{
    public class DependencyModule : IDependencyModule
    {
        private readonly ILogger _logger;
        private readonly SharedOptions _sharedOptions;

        public DependencyModule(
            ILogger<DependencyModule> logger,
            SharedOptions sharedOptions)
        {
            _logger = logger;
            _sharedOptions = sharedOptions;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFixture,Fixture>();

            //Add the swagger examples from this assembly
            services.AddSwaggerExamplesFromAssemblyOf<DependencyModule>();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(options =>
            {
                options.ExampleFilters();

                // DataContractAttribute.Name is not honored for some reason, so we have to override it
                options.CustomSchemaIds(type =>
                {
                    var dataContractAttribute = type.GetCustomAttribute<DataContractAttribute>();
                    return dataContractAttribute != null && dataContractAttribute.Name != null ? dataContractAttribute.Name : type.Name;
                });

                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    // Filter out other controllers
                    var assemblyName = ((ControllerActionDescriptor)apiDesc.ActionDescriptor).ControllerTypeInfo.Assembly.GetName().Name;
                    var currentAssemblyName = GetType().Assembly.GetName().Name;
                    return currentAssemblyName == assemblyName;
                });

                options.SwaggerDoc("V1", new OpenApiInfo
                {
                    Version = "V1",
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
                options.IncludeXmlComments(xmlPath);
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //Register dependencies here
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            var app = lifetimeScope.Resolve<IApplicationBuilder>();

            string documentName = "V1";

            if (_sharedOptions.UseDeveloperExceptions)
                app.UseDeveloperExceptionPage();

            app.UseDeveloperExceptionPage();

            //Configure dependencies here
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(options=> 
            { 
                options.RouteTemplate = "/Api/{documentName}/ModernSlaverySummaryApi.json";
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(options => 
            {
                options.SwaggerEndpoint($"{documentName}/ModernSlaverySummaryApi.json", "Modern Slavery Statement Summary API");
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
