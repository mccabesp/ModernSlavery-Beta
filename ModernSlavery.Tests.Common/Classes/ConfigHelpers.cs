using System;
using Microsoft.Extensions.Configuration;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.Infrastructure.Configuration;
using ModernSlavery.Infrastructure.Messaging;
using ModernSlavery.Infrastructure.Search;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.WebUI.Submission.Classes;

namespace ModernSlavery.Tests.Common.Classes
{
    public static class ConfigHelpers
    {
        public static IConfiguration Config = new ConfigBuilder().Build();

        public static SharedOptions SharedOptions = Bind<SharedOptions>();
        public static SearchOptions SearchOptions = Bind<SearchOptions>();
        public static StorageOptions StorageOptions = Bind<StorageOptions>();
        public static EmailOptions EmailOptions = Bind<EmailOptions>();
        public static SubmissionOptions SubmissionOptions = Bind<SubmissionOptions>();
        public static GovNotifyOptions GovNotifyOptions = Bind<GovNotifyOptions>();

        private static T Bind<T>(string configSection = null)
        {
            var instance = (T) Activator.CreateInstance(typeof(T));

            if (string.IsNullOrWhiteSpace(configSection))
            {
                var configSettingAttribute = instance.GetAttribute<OptionsAttribute>();
                configSection = configSettingAttribute?.Key;
            }

            if (string.IsNullOrWhiteSpace(configSection))
                Config.Bind(instance);
            else
                Config.Bind(configSection, instance);

            return instance;
        }
    }
}