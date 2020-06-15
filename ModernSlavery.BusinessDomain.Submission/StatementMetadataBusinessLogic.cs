using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.BusinessDomain.Submission
{
    partial class StatementMetadataBusinessLogic : IStatementMetadataBusinessLogic
    {
        readonly ISharedBusinessLogic SharedBusinessLogic;

        public StatementMetadataBusinessLogic(ISharedBusinessLogic sharedBusinessLogic)
        {
            SharedBusinessLogic = sharedBusinessLogic;
        }

        public async Task<StatementMetadata> GetStatementMetadataById(long statementMetadataId)
        {
            // example data access
            return await SharedBusinessLogic.DataRepository
                .FirstOrDefaultAsync<StatementMetadata>(s => s.StatementMetadataId == statementMetadataId);
        }

        public async Task<StatementMetadata> GetStatementMetadataByOrganisationAndYear(Organisation organisation, int year)
        {
            throw new NotImplementedException();
        }

        public async Task<StatementActionResult> CanAccessStatementMetadata(User user, Organisation organisation, int reportingYear)
        {
            var data = await GetStatementMetadataByOrganisationAndYear(organisation, reportingYear);

            // is the year even valid?

            #region Failure 1: can the user view statements

            if (!organisation.UserOrganisations.Any(uo => uo.User == user))
                return StatementActionResult.Unauthorised;

            #endregion

            #region Failure 2: can the user edit the current statement for this organisation

            #endregion

            #region Failure 3: is the statement in a state that can be edited, eg not submitted

            var statement = await GetStatementMetadataByOrganisationAndYear(organisation, reportingYear);
            if (!statement.CanBeEdited)
                return StatementActionResult.Uneditable;

            #endregion

            #region Failure 4: Another person has this statement metadata locked

            //if ()
            //    return StatementActionResult.Locked;

            #endregion

            #region Failure 5: should the user be redirected

            // what should happen here?

            #endregion

            return StatementActionResult.Success;
        }

        public async Task<StatementActionResult> SaveStatementMetadata(User user, StatementMetadata statementMetadata)
        {
            if (!statementMetadata.CanBeEdited)
            {
                return StatementActionResult.Uneditable;
            }

            // Is this check enough?
            if (statementMetadata.StatementMetadataId == 0)
            {
                SharedBusinessLogic.DataRepository.Insert(statementMetadata);
            }
            else
            {
                // edit?
            }

            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            throw new NotImplementedException();
        }
    }
}
