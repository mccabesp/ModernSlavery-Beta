﻿using System;
using Autofac;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.BusinessDomain.DevOps
{
    [AutoRegister]
    public class DependencyModule : IDependencyModule
    {
        public DependencyModule()
        {
            //TODO: Add IOptions parameters
        }

        public void Register(IDependencyBuilder builder)
        {
            //TODO: Add registrationsd here
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //TODO: Add configuration here
        }
    }
}