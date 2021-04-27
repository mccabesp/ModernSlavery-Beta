using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models.LogModels;
using Autofac.Features.AttributeFilters;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Hosts.Webjob.Classes;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class PurgeOrganisationsWebJob : WebJob
    {
        #region Dependencies
        private readonly IAuditLogger _manualChangeLog;
        private readonly ISmtpMessenger _messenger;
        private readonly SharedOptions _sharedOptions;
        private readonly IDataRepository _dataRepository;
        private readonly ISearchBusinessLogic _searchBusinessLogic;
        private readonly IStatementBusinessLogic _statementBusinessLogic;
        #endregion

        public PurgeOrganisationsWebJob(
            [KeyFilter(Filenames.ManualChangeLog)] IAuditLogger manualChangeLog,
            ISmtpMessenger messenger,
            SharedOptions sharedOptions,
            IDataRepository dataRepository,
            ISearchBusinessLogic searchBusinessLogic,
            IStatementBusinessLogic statementBusinessLogic)
        {
            _manualChangeLog = manualChangeLog;
            _messenger = messenger;
            _sharedOptions = sharedOptions;
            _dataRepository = dataRepository;
            _searchBusinessLogic = searchBusinessLogic;
            _statementBusinessLogic = statementBusinessLogic;
        }

        //Remove any unverified users their addresses, UserOrgs, Org and addresses and archive to zip
        [Disable(typeof(DisableWebJobProvider))]
        public async Task PurgeOrganisationsAsync([TimerTrigger("%PurgeOrganisations%")]
            TimerInfo timer,
                ILogger log)
        {
            if (RunningJobs.Contains(nameof(PurgeOrganisationsAsync))) return;
            RunningJobs.Add(nameof(PurgeOrganisationsAsync));
            try
            {
                var deletedCount = 0;

                var deadline = VirtualDateTime.Now.AddDays(0 - _sharedOptions.PurgeUnusedOrganisationDays);
                var orgs = await _dataRepository.GetAll<Organisation>()
                    .Where(
                        o => o.Created < deadline
                             && !o.Statements.Any()
                             && !o.StatementOrganisations.Any()
                             && !o.OrganisationScopes.Any(sc => sc.ScopeStatus == ScopeStatuses.InScope || sc.ScopeStatus == ScopeStatuses.OutOfScope)
                             && !o.OrganisationNames.Any(a => a.Source == "External")
                             && !o.UserOrganisations.Any(uo => uo.Method == RegistrationMethods.Manual || uo.PINConfirmedDate != null || uo.PINSentDate > deadline))
                    .ToListAsync().ConfigureAwait(false);

                foreach (var org in orgs)
                {
                    var logItem = new ManualChangeLogModel(
                        nameof(PurgeOrganisationsAsync),
                        ManualActions.Delete,
                        $"TABLE {nameof(Organisation)}",
                        nameof(org.OrganisationId),
                        org.OrganisationId.ToString(),
                        null,
                        Json.SerializeObject(
                            new
                            {
                                org.OrganisationId,
                                Address = (org.LatestAddress ?? org.GetLatestAddress())?.GetAddressString(),
                                org.OrganisationReference,
                                org.DUNSNumber,
                                org.CompanyNumber,
                                org.OrganisationName,
                                org.SectorType,
                                org.Status,
                                SicCodes = org.GetLatestSicCodeIdsString(),
                                SicSource = org.GetLatestSicSource(),
                                org.DateOfCessation
                            }),
                        null);

                    await _dataRepository.ExecuteTransactionAsync(
                        async () => {
                            try
                            {
                                org.LatestAddress = null;
                                org.LatestRegistration = null;
                                org.LatestStatement = null;
                                org.LatestScope = null;
                                org.UserOrganisations.ForEach(uo => _dataRepository.Delete(uo));
                                await _dataRepository.SaveChangesAsync().ConfigureAwait(false);

                                _dataRepository.Delete(org);
                                await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
                                _dataRepository.CommitTransaction();
                                deletedCount++;
                            }
                            catch (Exception ex)
                            {
                                _dataRepository.RollbackTransaction();
                                log.LogError(
                                    ex,
                                    $"{nameof(PurgeOrganisationsAsync)}: Failed to purge organisation {org.OrganisationId} '{org.OrganisationName}' ERROR: {ex.Message}:{ex.GetDetailsText()}");
                            }
                        }).ConfigureAwait(false);

                    await _manualChangeLog.WriteAsync(logItem).ConfigureAwait(false);

                    //Delete all the draft statements for this organisation
                    await _statementBusinessLogic.DeleteDraftStatementsAsync(org.OrganisationId);

                    //Remove this organisation from the search index
                    await _searchBusinessLogic.RemoveSearchDocumentsAsync(org);

                }

                log.LogDebug($"Executed WebJob {nameof(PurgeOrganisationsAsync)} successfully. Deleted: {deletedCount}.");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(PurgeOrganisationsAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                RunningJobs.Remove(nameof(PurgeOrganisationsAsync));
            }

        }
    }
}