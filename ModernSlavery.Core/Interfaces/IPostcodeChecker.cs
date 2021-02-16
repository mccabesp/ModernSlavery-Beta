using System.Threading.Tasks;

namespace ModernSlavery.Core.Interfaces
{
    public interface IPostcodeChecker
    {
        Task<bool> CheckPostcodeAsync(string postcode);
    }
}