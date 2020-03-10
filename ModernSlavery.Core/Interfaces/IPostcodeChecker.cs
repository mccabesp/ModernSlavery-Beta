using System.Threading.Tasks;

namespace ModernSlavery.Infrastructure
{
    public interface IPostcodeChecker
    {
        Task<bool> IsValidPostcode(string postcode);
    }
}