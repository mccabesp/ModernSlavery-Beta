using Autofac;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.Core
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
            //Register some singletons
            builder.Autofac.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance()
                .WithParameter("seed", _sharedOptions.ObfuscationSeed);

            builder.Autofac.RegisterType<EncryptionHandler>().As<IEncryptionHandler>().SingleInstance();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            
        }
    }
}