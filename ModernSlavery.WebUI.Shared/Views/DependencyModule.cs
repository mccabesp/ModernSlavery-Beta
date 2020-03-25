using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.WebUI.Account
{
    public class DependencyModule: IDependencyModule
    {
        public DependencyModule()
        {
            //Any IOptions constructor parameters are automatically resolved
        }

        public bool AutoSetup { get; } = false;

        public void Register(DependencyBuilder builder)
        {
            //TODO: Register dependencies here
        }

        public void Configure(IContainer container)
        {
            //TODO: Configure dependencies here


        }
    }
}
