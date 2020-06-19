using CommandLine;
using Microsoft.Azure.Management.Fluent;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Infrastructure.Azure.AppService;
using ModernSlavery.Infrastructure.Azure.DevOps;
using ModernSlavery.Infrastructure.Azure.KeyVault;
using ModernSlavery.Infrastructure.Azure.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModernSlavery.Infrastructure.Azure
{
    public class CommandLineParser
    {
        private static AzureManager AzureManager = new AzureManager();
        private static IAzure _azure = null;
        private static AzureOptions _azureOptions;

        private static Type[] LoadCommandLineVerbTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();
        }

        public static void ExecuteCommandLine(string[] commandLineArgs)
        {
            //Type[] types = { typeof(AddOptions), typeof(CommitOptions), typeof(CloneOptions) };
            //or collect types using reflection /plugins /Ioc container
            var commandLineVerbTypes = LoadCommandLineVerbTypes();

            Parser.Default.ParseArguments(commandLineArgs, commandLineVerbTypes)
                  .WithParsed(ExecuteVerbs)
                  .WithNotParsed(HandleErrors);
        }

        private static void HandleErrors(IEnumerable<Error> errors)
        {
            errors.ForEach(error => Console.WriteLine(error.ToString()));
        }

        private static void ExecuteVerbs(object verbType)
        {
            switch (verbType)
            {
                case KeyVaultOptions keyVaultOptions:
                    Authenticate(verbType as AzureOptions);

                    var keyVaultManager = new KeyVaultManager(_azure, keyVaultOptions.VaultName);

                    switch (verbType)
                    {
                        case KeyVaultSetOptions options:
                            keyVaultManager.SetValue(options.key, options.value);
                            break;
                        case KeyVaultImportOptions options:
                            keyVaultManager.ImportValues(options.filepath, options.section);
                            break;
                        case KeyVaultListOptions options:
                            keyVaultManager.GetValues(options.section).ForEach(kv => Console.WriteLine($"[{kv.Key}]={kv.Value}"));
                            break;
                        case KeyVaultExportOptions options:
                            keyVaultManager.ExportValues(options.VaultName, options.filepath, options.section);
                            break;
                    }
                    break;
                case AppServiceOptions appServiceOptions:
                    Authenticate(verbType as AzureOptions);
                    var appServiceManager = new AppServiceManager(_azure);

                    switch (verbType)
                    {
                        case AppServiceSetPricingTierOptions options:
                            appServiceManager.SetAppServicePricingTier(options.AppService, options.PricingTier);
                            break;
                    }
                    break;
                case SqlDatabaseOptions sqlDatabaseOptions:
                    Authenticate(verbType as AzureOptions);
                    var sqlManager = new SqlManager(_azure);

                    switch (verbType)
                    {
                        case SqlDatabasetSetEditionOptions options:
                            sqlManager.SetDatabaseEdition(options.Database, options.DatabaseEdition);
                            break;
                    }
                    break;
                case DevOpsOptions devOpsOptions:
                    var devOpsManager = new DevOpsManager(devOpsOptions.Organisation, devOpsOptions.PersonalAccessToken);

                    switch (verbType)
                    {
                        default:
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Sign in to the azure tenant
        /// </summary>
        /// <param name="azureOptions"></param>
        private static void Authenticate(AzureOptions azureOptions)
        {
            if (azureOptions == null) throw new ArgumentNullException(nameof(azureOptions));
            if (_azure != null && _azureOptions.ClientId == azureOptions.ClientId && _azureOptions.ClientSecret == azureOptions.ClientSecret && _azureOptions.TenantId == azureOptions.TenantId && _azureOptions.SubscriptionId == azureOptions.SubscriptionId) return;
            _azure = AzureManager.Authenticate(azureOptions);
            _azureOptions = azureOptions;
        }
    }
}
