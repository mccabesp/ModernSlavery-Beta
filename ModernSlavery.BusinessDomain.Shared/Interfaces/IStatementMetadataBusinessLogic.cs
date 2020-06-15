using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IStatementMetadataBusinessLogic
    {
        /// <summary>
        /// Get the statement metadata from the specified year for the specified organisation.
        /// </summary>
        Task<StatementMetadata> GetStatementMetadataByOrganisationAndYear(Organisation organisation, int reportingYear);

        /// <summary>
        /// Check if the user can access the statement of the provided organisation and year.
        /// </summary>
        Task<StatementActionResult> CanAccessStatementMetadata(User user, Organisation organisation, int reportingYear);

        /// <summary>
        /// Save the statement metadata.
        /// </summary>
        Task<StatementActionResult> SaveStatementMetadata(User user, StatementMetadata statementMetadata);
    }

    public enum StatementActionResult : byte
    {
        Unknown = 0,
        Success = 1,
        InvalidPermissions = 2,
        Unauthorised = 3,
        Uneditable = 4,
        Locked = 5
    }
}
