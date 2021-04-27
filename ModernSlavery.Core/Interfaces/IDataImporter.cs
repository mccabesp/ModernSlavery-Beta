using System.Threading.Tasks;

namespace ModernSlavery.Core.Interfaces
{
    public interface IDataImporter
    {
        Task EnsureSystemUserExistsAsync();
        Task<int> ImportPrivateOrganisationsAsync(long userId, int maxRecords = 0, bool importWhenAny = false, bool throwWhenExists = false);
        Task<int> ImportPublicOrganisationsAsync(long userId, int maxRecords = 0, bool importWhenAny = false, bool throwWhenExists = false);
        Task<int> ImportSICCodesAsync(bool importWhenAny = false);
        Task<int> ImportSICSectionsAsync(bool importWhenAny = false);
        Task<int> ImportStatementSectorTypesAsync(bool importWhenAny = false);
    }
}