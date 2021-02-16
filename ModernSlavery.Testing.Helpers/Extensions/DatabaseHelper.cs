using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Extensions;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using ModernSlavery.Testing.Helpers.Classes;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class DatabaseHelper
    {
        private const string LastFullDatabaseResetKey = "LastFullDatabaseReset";

        /// <summary>
        /// Deletes all tables in the database (except _EFMigrationsHistory) and reimports all seed data
        /// </summary>
        /// <param name="host"></param>
        public static async Task ResetDatabaseAsync(this IHost host, bool force = true)
        {
            var config = host.Services.GetRequiredService<IConfiguration>();
            var vaultName = config["Vault"];

            var testBusinessLogic = host.Services.GetTestBusinessLogic();

            if (!force)
            {
                var lastFullDatabaseReset = string.IsNullOrWhiteSpace(vaultName) ? RegistryHelper.GetRegistryKey(LastFullDatabaseResetKey).ToDateTime() : host.GetKeyVaultSecret(vaultName, LastFullDatabaseResetKey).ToDateTime();
                if (lastFullDatabaseReset > DateTime.MinValue && lastFullDatabaseReset.Date == DateTime.Now.Date)
                {
                    await testBusinessLogic.ClearDatabaseAsync(lastFullDatabaseReset).ConfigureAwait(false);
                    return;
                }
            }

            await testBusinessLogic.ClearDatabaseAsync().ConfigureAwait(false);
            await testBusinessLogic.SeedDatabaseAsync().ConfigureAwait(false);

            //Reset the database after import
            await testBusinessLogic.InitialiseDatabaseWebjobsAsync().ConfigureAwait(false);

            //Set the full reset time to now (+15 seconds grace)
            Thread.Sleep(15);
            var importTime = DateTime.Now;

            //Set the last database reset
            if (string.IsNullOrWhiteSpace(vaultName))
                RegistryHelper.SetRegistryKey(LastFullDatabaseResetKey, importTime.ToString());
            else
                host.SetKeyVaultSecret(vaultName, LastFullDatabaseResetKey, importTime.ToString());
        }

        /// <summary>
        /// Shim for saving when needed, should prob refactor this out.
        /// </summary>
        /// <returns></returns>
        public static async Task SaveDatabaseAsync(this BaseUITest uiTest)
        {
            await uiTest.ServiceScope.GetDataRepository().SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
