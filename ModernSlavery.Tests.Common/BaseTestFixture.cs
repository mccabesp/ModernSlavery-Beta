using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Tests.Common
{
    public class BaseTestFixture
    {
        public static IContainer DependencyContainer { get; private set; }

        public IContainer RegisterModules(params Module[] modules)
        {
            var builder = new ContainerBuilder();

            foreach (var module in modules)
                builder.RegisterModule(module);

            IContainer container = builder.Build();
            DependencyContainer = container;
            return container;
        }
    }

    public class BaseTestFixture<TModule> : BaseTestFixture where TModule : Module
    {
        public BaseTestFixture()
        {
            RegisterModules(Activator.CreateInstance<TModule>());
        }
    }
}
