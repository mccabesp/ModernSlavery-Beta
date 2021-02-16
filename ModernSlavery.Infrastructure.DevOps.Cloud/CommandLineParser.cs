using CommandLine;
using Microsoft.Azure.Management.Fluent;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Infrastructure.Azure.AppService;
using ModernSlavery.Infrastructure.Azure.Cache;
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
        private static AzureManager AzureManager;
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
            IAzure _Azure = null;
            switch (verbType)
            {
                case KeyVaultOptions keyVaultOptions:
                    Authenticate(verbType as AzureOptions);

                    var keyVaultManager = new KeyVaultManager(_Azure, keyVaultOptions.VaultName);

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
                case RedisCacheOptions cacheOptions:
                    Authenticate(verbType as AzureOptions);

                    var cacheManager = new DistributedCacheManager(AzureManager, null,null);

                    switch (verbType)
                    {
                        case RedisCacheRebootOptions options:
                            cacheManager.ClearCacheAsync().Wait();
                            break;
                    }
                    break;
                case AppServiceOptions appServiceOptions:
                    Authenticate(verbType as AzureOptions);
                    var appServiceManager = new AppServiceManager(_Azure);

                    switch (verbType)
                    {
                        case AppServiceSetPricingTierOptions options:
                            appServiceManager.SetAppServicePricingTier(options.AppService, options.PricingTier);
                            break;
                    }
                    break;
                case SqlServerOptions sqlServerOptions:
                case SqlDatabaseOptions sqlDatabaseOptions:
                    Authenticate(verbType as AzureOptions);
                    var sqlManager = new SqlManager(_Azure);

                    switch (verbType)
                    {
                        case SqlDatabasetSetEditionOptions options:
                            sqlManager.SetDatabaseEdition(options.Database, options.DatabaseEdition);
                            break;
                        case SqlServerOpenFirewallOptions options:
                            sqlManager.OpenFirewall(options.Server, options.RuleName, options.StartIP, options.EndIP);
                            break;
                        case SqlServerDeleteFirewallOptions options:
                            sqlManager.DeleteFirewall(options.Server, options.RuleName);
                            break;
                    }
                    break;
                case DevOps.DevOpsOptions devOpsOptions:
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
            AzureManager = AzureManager ?? new AzureManager(azureOptions);
            if (azureOptions == null) throw new ArgumentNullException(nameof(azureOptions));
            AzureManager.Authenticate();
            _azureOptions = azureOptions;
        }
    }
}
