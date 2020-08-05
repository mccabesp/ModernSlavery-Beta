using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Infrastructure.Database;
using Newtonsoft.Json;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        //Remove any unverified users their addresses, UserOrgs, Org and addresses and archive to zip
        public async Task PurgeUsers([TimerTrigger("01:00:00:00")] TimerInfo timer, ILogger log)
        {
            try
            {
                var deadline =
                    VirtualDateTime.Now.AddDays(0 - _SharedBusinessLogic.SharedOptions.PurgeUnverifiedUserDays);
                var users = await _SharedBusinessLogic.DataRepository.GetAll<User>()
                    .Where(u => u.EmailVerifiedDate == null &&
                                (u.EmailVerifySendDate == null || u.EmailVerifySendDate.Value < deadline))
                    .ToListAsync().ConfigureAwait(false);
                var pinExpireyDate =
                    VirtualDateTime.Now.AddDays(0 - _SharedBusinessLogic.SharedOptions.PinInPostExpiryDays);
                foreach (var user in users)
                {
                    //Ignore if they have verified PIN
                    if (user.UserOrganisations.Any(
                        uo => uo.PINConfirmedDate != null || uo.PINSentDate != null && uo.PINSentDate < pinExpireyDate))
                        continue;

                    var logItem = new ManualChangeLogModel(
                        nameof(PurgeUsers),
                        ManualActions.Delete,
                        AppDomain.CurrentDomain.FriendlyName,
                        nameof(user.UserId),
                        user.UserId.ToString(),
                        null,
                        JsonConvert.SerializeObject(new {user.UserId, user.EmailAddress, user.JobTitle, user.Fullname}),
                        null);
                    _SharedBusinessLogic.DataRepository.Delete(user);
                    await _SharedBusinessLogic.DataRepository.SaveChangesAsync().ConfigureAwait(false);
                    await _ManualChangeLog.WriteAsync(logItem).ConfigureAwait(false);
                }

                log.LogDebug($"Executed {nameof(PurgeUsers)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(PurgeUsers)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }

        //Remove any incomplete registrations
        public async Task PurgeRegistrations([TimerTrigger("01:00:00:00")] TimerInfo timer, ILogger log)
        {
            try
            {
                var deadline =
                    VirtualDateTime.Now.AddDays(0 - _SharedBusinessLogic.SharedOptions.PurgeUnconfirmedPinDays);
                var registrations = await _SharedBusinessLogic.DataRepository.GetAll<UserOrganisation>()
                    .Where(u => u.PINConfirmedDate == null && u.PINSentDate != null && u.PINSentDate.Value < deadline)
                    .ToListAsync().ConfigureAwait(false);
                foreach (var registration in registrations)
                {
                    var logItem = new ManualChangeLogModel(
                        nameof(PurgeRegistrations),
                        ManualActions.Delete,
                        AppDomain.CurrentDomain.FriendlyName,
                        $"{nameof(UserOrganisation.OrganisationId)}:{nameof(UserOrganisation.UserId)}",
                        $"{registration.OrganisationId}:{registration.UserId}",
                        null,
                        JsonConvert.SerializeObject(
                            new
                            {
                                registration.UserId,
                                registration.User.EmailAddress,
                                registration.OrganisationId,
                                registration.Organisation.EmployerReference,
                                registration.Organisation.OrganisationName,
                                registration.Method,
                                registration.PINSentDate,
                                registration.PINConfirmedDate
                            }),
                        null);
                    _SharedBusinessLogic.DataRepository.Delete(registration);
                    await _SharedBusinessLogic.DataRepository.SaveChangesAsync().ConfigureAwait(false);
                    await _ManualChangeLog.WriteAsync(logItem).ConfigureAwait(false);
                }

                log.LogDebug($"Executed {nameof(PurgeRegistrations)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(PurgeRegistrations)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }

        //Remove any unverified users their addresses, UserOrgs, Org and addresses and archive to zip
        public async Task PurgeOrganisations([TimerTrigger("01:00:00:00", RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                var deadline =
                    VirtualDateTime.Now.AddDays(0 - _SharedBusinessLogic.SharedOptions.PurgeUnusedOrganisationDays);
                var orgs = await _SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .Where(
                        o => o.Created < deadline
                             && !o.Statements.Any()
                             && !o.OrganisationScopes.Any(
                                 sc => sc.ScopeStatus == ScopeStatuses.InScope ||
                                       sc.ScopeStatus == ScopeStatuses.OutOfScope)
                             && !o.UserOrganisations.Any(
                                 uo => uo.Method == RegistrationMethods.Manual || uo.PINConfirmedDate != null ||
                                       uo.PINSentDate > deadline)
                             && !o.OrganisationAddresses.Any(a => a.CreatedByUserId == -1 || a.Source == "D&B"))
                    .ToListAsync().ConfigureAwait(false);

                if (orgs.Any())
                {
                    var count = 0;
                    foreach (var org in orgs)
                    {
                        var logItem = new ManualChangeLogModel(
                            nameof(PurgeOrganisations),
                            ManualActions.Delete,
                            AppDomain.CurrentDomain.FriendlyName,
                            nameof(org.OrganisationId),
                            org.OrganisationId.ToString(),
                            null,
                            JsonConvert.SerializeObject(
                                new
                                {
                                    org.OrganisationId,
                                    Address = org.GetLatestAddress()?.GetAddressString(),
                                    org.EmployerReference,
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
                        var searchRecord = _OrganisationBusinessLogic.CreateEmployerSearchModel(org, true);

                        await _SharedBusinessLogic.DataRepository.BeginTransactionAsync(
                            async () =>
                            {
                                try
                                {
                                    org.LatestAddress = null;
                                    org.LatestRegistration = null;
                                    org.LatestStatement = null;
                                    org.LatestScope = null;
                                    org.UserOrganisations.ForEach(uo => _SharedBusinessLogic.DataRepository.Delete(uo));
                                    await _SharedBusinessLogic.DataRepository.SaveChangesAsync().ConfigureAwait(false);

                                    _SharedBusinessLogic.DataRepository.Delete(org);
                                    await _SharedBusinessLogic.DataRepository.SaveChangesAsync().ConfigureAwait(false);

                                    _SharedBusinessLogic.DataRepository.CommitTransaction();
                                }
                                catch (Exception ex)
                                {
                                    _SharedBusinessLogic.DataRepository.RollbackTransaction();
                                    log.LogError(
                                        ex,
                                        $"{nameof(PurgeOrganisations)}: Failed to purge organisation {org.OrganisationId} '{org.OrganisationName}' ERROR: {ex.Message}:{ex.GetDetailsText()}");
                                }
                            }).ConfigureAwait(false);
                        //Remove this organisation from the search index
                        await _EmployerSearchRepository.RemoveFromIndexAsync(new[] {searchRecord}).ConfigureAwait(false);

                        await _ManualChangeLog.WriteAsync(logItem).ConfigureAwait(false);
                        count++;
                    }

                    log.LogDebug($"Executed {nameof(PurgeOrganisations)} successfully: {count} deleted");
                }
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(PurgeOrganisations)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }

        //Remove retired copies of GPG data
        public async Task PurgeGPGData([TimerTrigger("01:00:00:00")] TimerInfo timer, ILogger log)
        {
            try
            {
                var deadline =
                    VirtualDateTime.Now.AddDays(0 - _SharedBusinessLogic.SharedOptions.PurgeRetiredReturnDays);
                var returns = await _SharedBusinessLogic.DataRepository.GetAll<Return>()
                    .Where(r => r.StatusDate < deadline &&
                                (r.Status == StatementStatuses.Retired || r.Status == StatementStatuses.Deleted))
                    .ToListAsync().ConfigureAwait(false);

                foreach (var @return in returns)
                {
                    var logItem = new ManualChangeLogModel(
                        nameof(PurgeGPGData),
                        ManualActions.Delete,
                        AppDomain.CurrentDomain.FriendlyName,
                        nameof(@return.ReturnId),
                        @return.ReturnId.ToString(),
                        null,
                        JsonConvert.SerializeObject(DownloadResult.Create(@return)),
                        null);
                    _SharedBusinessLogic.DataRepository.Delete(@return);
                    await _SharedBusinessLogic.DataRepository.SaveChangesAsync().ConfigureAwait(false);
                    await _ManualChangeLog.WriteAsync(logItem).ConfigureAwait(false);
                }

                log.LogDebug($"Executed {nameof(PurgeGPGData)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(PurgeGPGData)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }

        //Remove test users and organisations
        [Disable(typeof(DisableWebjobProvider))]
        public async Task PurgeTestDataAsync([TimerTrigger("00:12:00:00", RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            if (_SharedBusinessLogic.SharedOptions.IsProduction()) return;

            try
            {
                var databaseContext = new DatabaseContext(default);
                databaseContext.DeleteAllTestRecords(VirtualDateTime.Now.AddDays(-1));

                log.LogDebug($"Executed {nameof(PurgeTestDataAsync)} successfully");
            }
            catch (Exception ex)
            {
                //Send Email to GEO reporting errors
                var message = $"Failed webjob ({nameof(PurgeTestDataAsync)}):{ex.Message}:{ex.GetDetailsText()}";
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);

                //Rethrow the error
                throw;
            }
        }
    }
}