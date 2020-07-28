using System.Threading.Tasks;

namespace ModernSlavery.Core.Interfaces
{
    public interface IDataImporter
    {
        Task ImportOrganisationsAsync(bool force = false);
        Task ImportSICCodesAsync(bool force = false);
        Task ImportSICSectionsAsync(bool force = false);
        Task ImportStatementDiligenceTypesAsync(bool force = false);
        Task ImportStatementPolicyTypesAsync(bool force = false);
        Task ImportStatementRiskTypesAsync(bool force = false);
        Task ImportStatementSectorTypesAsync(bool force = false);
        Task ImportStatementTrainingTypesAsync(bool force = false);
    }
}