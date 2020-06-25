using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.BusinessDomain.Submission
{
    partial class StatementBusinessLogic : IStatementBusinessLogic
    {
        readonly ISharedBusinessLogic SharedBusinessLogic;

        public StatementBusinessLogic(ISharedBusinessLogic sharedBusinessLogic)
        {
            SharedBusinessLogic = sharedBusinessLogic;
        }

        public async Task<Statement> GetStatementByOrganisationAndYear(Organisation organisation, int year)
        {
            return await SharedBusinessLogic.DataRepository
                // Is the accounting year the correct year?
                .FirstOrDefaultAsync<Statement>(s => s.OrganisationId == organisation.OrganisationId && s.SubmissionDeadline.Year == year);
        }

        public async Task<StatementActionResult> CanAccessStatement(User user, Organisation organisation, int reportingYear)
        {
            // only assigned users
            var assignment = organisation.UserOrganisations.FirstOrDefault(uo => uo.User == user);
            if (assignment == null)
                return StatementActionResult.Unauthorised;

            // only active/pending/new organisations
            if (!organisation.Status.IsAny(OrganisationStatuses.Active, OrganisationStatuses.Pending, OrganisationStatuses.New))
                return StatementActionResult.Unauthorised;

            var statement = await GetStatementByOrganisationAndYear(organisation, reportingYear);
            if (statement != null && !statement.CanBeEdited)
                return StatementActionResult.Uneditable;

            return StatementActionResult.Success;
        }

        public async Task<StatementActionResult> SaveStatement(User user, Organisation organisation, Statement statement)
        {
            if (!statement.CanBeEdited)
            {
                return StatementActionResult.Uneditable;
            }

            // Is this check enough?
            if (statement.StatementId == 0)
            {
                statement.OrganisationId = organisation.OrganisationId;
                statement.SubmissionDeadline = SharedBusinessLogic.GetAccountingStartDate(organisation.SectorType, VirtualDateTime.Now.Year);
            }

            SharedBusinessLogic.DataRepository.Update(statement);

            await SharedBusinessLogic.DataRepository.SaveChangesAsync();
            SharedBusinessLogic.DataRepository.CommitTransaction();

            return StatementActionResult.Success;
        }
    }
}
