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
using Autofac.Features.AttributeFilters;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Hosts.Webjob.Classes;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class PurgeStatementsWebJob : WebJob
    {
        #region Dependencies
        private readonly IAuditLogger _manualChangeLog;
        private readonly ISmtpMessenger _messenger;
        private readonly SharedOptions _sharedOptions;
        private readonly IDataRepository _dataRepository;
        #endregion

        public PurgeStatementsWebJob(
            [KeyFilter(Filenames.ManualChangeLog)] IAuditLogger manualChangeLog,
            ISmtpMessenger messenger,
            SharedOptions sharedOptions,
            IDataRepository dataRepository)
        {
            _manualChangeLog = manualChangeLog;
            _messenger = messenger;
            _sharedOptions = sharedOptions;
            _dataRepository = dataRepository;
        }

        //Remove retired copies of MSU data
        [Disable(typeof(DisableWebJobProvider))]
        public async Task PurgeStatementsAsync([TimerTrigger("%PurgeStatements%")] TimerInfo timer, ILogger log)
        {
            if (RunningJobs.Contains(nameof(PurgeStatementsAsync))) return;
            RunningJobs.Add(nameof(PurgeStatementsAsync));
            try
            {
                var deletedCount = 0;

                var deadline =
                    VirtualDateTime.Now.AddDays(0 - _sharedOptions.PurgeRetiredReturnDays);
                var statements = await _dataRepository.GetAll<Statement>()
                    .Where(r => r.StatusDate < deadline &&
                                (r.Status == StatementStatuses.Retired || r.Status == StatementStatuses.Deleted))
                    .ToListAsync().ConfigureAwait(false);

                foreach (var statement in statements)
                {
                    var logItem = new ManualChangeLogModel(
                        nameof(PurgeStatementsAsync),
                        ManualActions.Delete,
                        $"TABLE {nameof(Statement)}",
                        nameof(statement.StatementId),
                        statement.StatementId.ToString(),
                        null,
                        Json.SerializeObject(DownloadModel.Create(statement)),
                        null);
                    _dataRepository.Delete(statement);
                    await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
                    deletedCount++;
                    await _manualChangeLog.WriteAsync(logItem).ConfigureAwait(false);
                }

                log.LogDebug($"Executed WebJob {nameof(PurgeStatementsAsync)} successfully. Deleted: {deletedCount}.");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(PurgeStatementsAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                RunningJobs.Remove(nameof(PurgeStatementsAsync));
            }
        }
    }
}