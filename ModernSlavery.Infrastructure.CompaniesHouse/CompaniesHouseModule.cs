using System;
using System.Net.Http;
using Autofac;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.SharedKernel.Interfaces;

namespace ModernSlavery.Infrastructure.CompaniesHouse
{
    public class CompaniesHouseModule: IDependencyModule
    {
        private readonly CompaniesHouseOptions _options;
        public CompaniesHouseModule(CompaniesHouseOptions options)
        {
            _options= options;
        }

        public void Bind(ContainerBuilder builder) 
        {
            builder.RegisterType<CompaniesHouseAPI>()
                .As<ICompaniesHouseAPI>()
                .SingleInstance()
                .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(ICompaniesHouseAPI)));

        }
    }
}
