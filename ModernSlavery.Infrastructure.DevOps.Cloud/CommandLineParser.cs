using CommandLine;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Infrastructure.DevOps.Cloud.AzureManagerOptions;
using ModernSlavery.Infrastructure.DevOps.Cloud.AzureManagerOptions.Interfaces;
using ModernSlavery.Infrastructure.DevOps.Cloud.AzureResourceManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModernSlavery.Infrastructure.DevOps.Cloud
{
    public class CommandLineParser
    {
        private static AuthenticationManager AuthenticationManager = new AuthenticationManager();
        private static IAzure _azure=null;
        private static IAzureOptions _azureOptions;

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
            Authenticate(verbType as IAzureOptions);

            switch (verbType)
            {
                case IKeyVaultOptions keyVaultOptions:
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
                                keyVaultManager.GetValues(options.section).ForEach(kv=>Console.WriteLine($"[{kv.Key}]={kv.Value}"));
                                break;
                            case KeyVaultExportOptions options:
                                keyVaultManager.ExportValues(options.VaultName, options.filepath, options.section);
                                break;
                        }
                        break;
            }
        }

        /// <summary>
        /// Sign in to the azure tenant
        /// </summary>
        /// <param name="azureOptions"></param>
        private static void Authenticate(IAzureOptions azureOptions)
        {
            if (azureOptions == null) throw new ArgumentNullException(nameof(azureOptions));
            if (_azure != null && _azureOptions.ClientId == azureOptions.ClientId && _azureOptions.ClientSecret == azureOptions.ClientSecret && _azureOptions.TenantId == azureOptions.TenantId && _azureOptions.SubscriptionId == azureOptions.SubscriptionId) return;
            _azure=AuthenticationManager.Authenticate(azureOptions);
            _azureOptions = azureOptions;
        }
    }
}
