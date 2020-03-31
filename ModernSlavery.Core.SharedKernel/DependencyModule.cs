using Autofac;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.Core.SharedKernel
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

        public void Register(IDependencyBuilder builder)
        {
            builder.Autofac.RegisterType<SnapshotDateHelper>().As<ISnapshotDateHelper>().SingleInstance();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            
        }
    }
}