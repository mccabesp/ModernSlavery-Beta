﻿using ModernSlavery.BusinessDomain.Shared;
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
    partial class StatementMetadataBusinessLogic : IStatementMetadataBusinessLogic
    {
        readonly ISharedBusinessLogic SharedBusinessLogic;

        public StatementMetadataBusinessLogic(ISharedBusinessLogic sharedBusinessLogic)
        {
            SharedBusinessLogic = sharedBusinessLogic;
        }

        public async Task<StatementMetadata> GetStatementMetadataByOrganisationAndYear(Organisation organisation, int year)
        {
            return await SharedBusinessLogic.DataRepository
                // Is the accounting year the correct year?
                .FirstOrDefaultAsync<StatementMetadata>(s => s.OrganisationId == organisation.OrganisationId && s.AccountingDate.Year == year);
        }

        public async Task<StatementActionResult> CanAccessStatementMetadata(User user, Organisation organisation, int reportingYear)
        {
            // only assigned users
            var assignment = organisation.UserOrganisations.FirstOrDefault(uo => uo.User == user);
            if (assignment == null)
                return StatementActionResult.Unauthorised;

            // only active/pending/new organisations
            if (!organisation.Status.IsAny(OrganisationStatuses.Active, OrganisationStatuses.Pending, OrganisationStatuses.New))
                return StatementActionResult.Unauthorised;

            var statement = await GetStatementMetadataByOrganisationAndYear(organisation, reportingYear);
            if (statement != null && statement.CanBeEdited)
                return StatementActionResult.Uneditable;

            return StatementActionResult.Success;
        }

        public async Task<StatementActionResult> SaveStatementMetadata(User user, Organisation organisation, StatementMetadata statement)
        {
            if (!statement.CanBeEdited)
            {
                return StatementActionResult.Uneditable;
            }

            // Is this check enough?
            if (statement.StatementMetadataId == 0)
            {
                statement.OrganisationId = organisation.OrganisationId;
                statement.Created = VirtualDateTime.Now;
                statement.AccountingDate = SharedBusinessLogic.GetAccountingStartDate(organisation.SectorType, VirtualDateTime.Now.Year);

                SharedBusinessLogic.DataRepository.Insert(statement);

                // Add the statement to the org
                //statementMetadata.Organisation.Statements.Add(statementMetadata);
            }

            // status, status history

            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            return StatementActionResult.Success;
        }
    }
}
