using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
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
        [Disable(typeof(DisableWebjobProvider))]
        public async Task PurgeUsers([TimerTrigger("%PurgeUsers%")] TimerInfo timer, ILogger log)
        {
            if (RunningJobs.Contains(nameof(PurgeUsers))) return;
            RunningJobs.Add(nameof(PurgeUsers));
            try
            {
                var deadline =
                    VirtualDateTime.Now.AddDays(0 - _sharedOptions.PurgeUnverifiedUserDays);
                var users = await _dataRepository.GetAll<User>()
                    .Where(u => u.EmailVerifiedDate == null &&
                                (u.EmailVerifySendDate == null || u.EmailVerifySendDate.Value < deadline))
                    .ToListAsync().ConfigureAwait(false);
                var pinExpireyDate =
                    VirtualDateTime.Now.AddDays(0 - _sharedOptions.PinInPostExpiryDays);
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
                        JsonConvert.SerializeObject(new { user.UserId, user.EmailAddress, user.JobTitle, user.Fullname }),
                        null);
                    _dataRepository.Delete(user);
                    await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
                    await _manualChangeLog.WriteAsync(logItem).ConfigureAwait(false);
                }

                log.LogDebug($"Executed {nameof(PurgeUsers)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(PurgeUsers)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                RunningJobs.Remove(nameof(PurgeUsers));
            }
            
        }

        //Remove any incomplete registrations
        [Disable(typeof(DisableWebjobProvider))]
        public async Task PurgeRegistrations([TimerTrigger("%PurgeRegistrations%")] TimerInfo timer, ILogger log)
        {
            if (RunningJobs.Contains(nameof(PurgeRegistrations))) return;
            RunningJobs.Add(nameof(PurgeRegistrations));
            try
            {
                var deadline =
                    VirtualDateTime.Now.AddDays(0 - _sharedOptions.PurgeUnconfirmedPinDays);
                var registrations = await _dataRepository.GetAll<UserOrganisation>()
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
                                registration.Organisation.OrganisationReference,
                                registration.Organisation.OrganisationName,
                                registration.Method,
                                registration.PINSentDate,
                                registration.PINConfirmedDate
                            }),
                        null);
                    _dataRepository.Delete(registration);
                    await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
                    await _manualChangeLog.WriteAsync(logItem).ConfigureAwait(false);
                }

                log.LogDebug($"Executed {nameof(PurgeRegistrations)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(PurgeRegistrations)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                RunningJobs.Remove(nameof(PurgeRegistrations));
            }
            
        }

        //Remove any unverified users their addresses, UserOrgs, Org and addresses and archive to zip
        [Disable(typeof(DisableWebjobProvider))]
        public async Task PurgeOrganisations([TimerTrigger("%PurgeOrganisations%")]
            TimerInfo timer,
            ILogger log)
        {
            if (RunningJobs.Contains(nameof(PurgeOrganisations))) return;
            RunningJobs.Add(nameof(PurgeOrganisations));
            try
            {
                var deadline =
                    VirtualDateTime.Now.AddDays(0 - _sharedOptions.PurgeUnusedOrganisationDays);
                var orgs = await _dataRepository.GetAll<Organisation>()
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

                        var searchRecords = await _searchBusinessLogic.GetOrganisationSearchIndexesAsync(org);

                        await _dataRepository.BeginTransactionAsync(
                            async () =>
                            {
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
                                }
                                catch (Exception ex)
                                {
                                    _dataRepository.RollbackTransaction();
                                    log.LogError(
                                        ex,
                                        $"{nameof(PurgeOrganisations)}: Failed to purge organisation {org.OrganisationId} '{org.OrganisationName}' ERROR: {ex.Message}:{ex.GetDetailsText()}");
                                }
                            }).ConfigureAwait(false);
                        //Remove this organisation from the search index
                        await _organisationSearchRepository.RemoveFromIndexAsync(searchRecords).ConfigureAwait(false);

                        await _manualChangeLog.WriteAsync(logItem).ConfigureAwait(false);
                        count++;
                    }

                    log.LogDebug($"Executed {nameof(PurgeOrganisations)} successfully: {count} deleted");
                }
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(PurgeOrganisations)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                RunningJobs.Remove(nameof(PurgeOrganisations));
            }
            
        }

        //Remove retired copies of GPG data
        [Disable(typeof(DisableWebjobProvider))]
        public async Task PurgeStatementData([TimerTrigger("%PurgeStatementData%")] TimerInfo timer, ILogger log)
        {
            if (RunningJobs.Contains(nameof(PurgeStatementData))) return;
            RunningJobs.Add(nameof(PurgeStatementData));
            try
            {
                var deadline =
                    VirtualDateTime.Now.AddDays(0 - _sharedOptions.PurgeRetiredReturnDays);
                var statements = await _dataRepository.GetAll<Statement>()
                    .Where(r => r.StatusDate < deadline &&
                                (r.Status == StatementStatuses.Retired || r.Status == StatementStatuses.Deleted))
                    .ToListAsync().ConfigureAwait(false);

                foreach (var statement in statements)
                {
                    var logItem = new ManualChangeLogModel(
                        nameof(PurgeStatementData),
                        ManualActions.Delete,
                        AppDomain.CurrentDomain.FriendlyName,
                        nameof(statement.StatementId),
                        statement.StatementId.ToString(),
                        null,
                        JsonConvert.SerializeObject(DownloadModel.Create(statement)),
                        null);
                    _dataRepository.Delete(statement);
                    await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
                    await _manualChangeLog.WriteAsync(logItem).ConfigureAwait(false);
                }

                log.LogDebug($"Executed {nameof(PurgeStatementData)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(PurgeStatementData)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                RunningJobs.Remove(nameof(PurgeStatementData));
            }
            
        }

        //Remove test users and organisations
        [Disable(typeof(DisableWebjobProvider))]
        public async Task PurgeTestDataAsync([TimerTrigger("%PurgeTestDataAsync%")]
            TimerInfo timer,
            ILogger log)
        {
            if (RunningJobs.Contains(nameof(PurgeTestDataAsync))) return;
            RunningJobs.Add(nameof(PurgeTestDataAsync));
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
                await _messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);

                //Rethrow the error
                throw;
            }
            finally
            {
                RunningJobs.Remove(nameof(PurgeTestDataAsync));
            }
            if (_sharedOptions.IsProduction()) return;

            
        }
    }
}