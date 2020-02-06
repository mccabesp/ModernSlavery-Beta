using System.Threading.Tasks;

namespace ModernSlavery.Core.Interfaces
{
    public interface IQueue
    {

        string Name { get; }

        Task AddMessageAsync<TInstance>(TInstance instance);

        Task AddMessageAsync(string message);

    }
}
