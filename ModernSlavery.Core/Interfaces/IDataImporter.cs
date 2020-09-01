using System.Threading.Tasks;

namespace ModernSlavery.Core.Interfaces
{
    public interface IDataImporter
    {
        Task ImportPrivateOrganisationsAsync(long userId, int maxRecords = 0, bool force = false);
        Task ImportPublicOrganisationsAsync(long userId, int maxRecords = 0, bool force = false);
        Task ImportSICCodesAsync(bool force = false);
        Task ImportSICSectionsAsync(bool force = false);
        Task ImportStatementDiligenceTypesAsync(bool force = false);
        Task ImportStatementPolicyTypesAsync(bool force = false);
        Task ImportStatementRiskTypesAsync(bool force = false);
        Task ImportStatementSectorTypesAsync(bool force = false);
        Task ImportStatementTrainingTypesAsync(bool force = false);
    }
}