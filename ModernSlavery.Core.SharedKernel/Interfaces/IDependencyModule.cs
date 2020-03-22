using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace ModernSlavery.Core.SharedKernel.Interfaces
{
    public interface IDependencyModule
    {
        void Bind(ContainerBuilder builder, IServiceCollection services);
    }
}
