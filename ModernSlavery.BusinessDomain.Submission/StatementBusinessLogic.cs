using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
            var dataResult = await SharedBusinessLogic.DataRepository
                // Is the accounting year the correct year?
                .FirstOrDefaultAsync<Statement>(s => s.OrganisationId == organisation.OrganisationId && s.SubmissionDeadline.Year == year);

            if (dataResult != null)
                return dataResult;

            var path = GetFileName(organisation.OrganisationId, year);
            if (!await SharedBusinessLogic.FileRepository.GetFileExistsAsync(path))
                return null;

            var content = await SharedBusinessLogic.FileRepository.ReadAsync(path);

            var fileResult = JsonConvert.DeserializeObject<Statement>(content);

            return fileResult;
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

        private async Task SaveToFile(Organisation organisation, Statement statement)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(statement, Formatting.Indented, settings));
            var fileName = GetFileName(organisation.OrganisationId, statement.SubmissionDeadline.Year);
            await SharedBusinessLogic.FileRepository.WriteAsync(fileName, data);
        }

        private string GetFileName(long organisationId, int reportingYear)
            => $"{organisationId}_{reportingYear}.json";

        public async Task<StatementActionResult> SaveStatement(User user, Organisation organisation, Statement statement)
        {
            if (!statement.CanBeEdited)
            {
                return StatementActionResult.Uneditable;
            }

            if (statement.Status == ReturnStatuses.Draft)
            {
                await SaveToFile(organisation, statement);

                return StatementActionResult.Success;
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
