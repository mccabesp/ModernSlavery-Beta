using Autofac;

namespace ModernSlavery.SharedKernel.Interfaces
{
    public interface IDependencyModule
    {
        void Bind(ContainerBuilder builder);
    }
}
