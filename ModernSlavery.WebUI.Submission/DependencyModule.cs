using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Submission.Classes;
using ModernSlavery.WebUI.Submission.Models;
using ModernSlavery.WebUI.Submission.Models.Statement;
using ModernSlavery.WebUI.Submission.Presenters;

namespace ModernSlavery.WebUI.Submission
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
            //Register service dependencies here
            services.AddSingleton<SectorTypeIndex>();
            services.AddScoped<OrganisationPageViewModel>();

            services.AddSingleton<PolicyTypeIndex>();
            services.AddScoped<PoliciesPageViewModel>();

            services.AddSingleton<RiskTypeIndex>();
            services.AddScoped<RisksPageViewModel>();
            
            services.AddSingleton<DiligenceTypeIndex>();
            services.AddScoped<DueDiligencePageViewModel>();

            services.AddSingleton<TrainingTypeIndex>();
            services.AddScoped<TrainingPageViewModel>();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //Register dependencies here
            builder.RegisterType<StatementPresenter>().As<IStatementPresenter>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ScopePresenter>().As<IScopePresenter>().InstancePerLifetimeScope();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Configure dependencies here
        }

        public void RegisterModules(IList<Type> modules)
        {
            //Register references dependency modules
            modules.AddDependency<ModernSlavery.BusinessDomain.Submission.DependencyModule>();
            modules.AddDependency<ModernSlavery.BusinessDomain.Shared.DependencyModule>();
            modules.AddDependency<ModernSlavery.WebUI.Shared.DependencyModule>();
        }

    }
}