using ModernSlavery.Extensions;
using ModernSlavery.Extensions.AspNetCore;

namespace ModernSlavery.Core
{
    public static class FeatureFlagHelper
    {
        public static bool IsFeatureEnabled(FeatureFlag featureFlag)
        {
            bool? flagValue = GetFeatureFlagValue(featureFlag);

            return flagValue.HasValue && flagValue.Value;
        }

        public static bool? GetFeatureFlagValue(FeatureFlag featureFlag)
        {
            string appSettingName = $"FeatureFlag_{featureFlag}";

            string appSettingValue = Config.GetAppSetting(appSettingName);

            if (string.IsNullOrEmpty(appSettingValue))
            {
                return null;
            }

            return appSettingValue.ToBoolean();
        }
    }

    public enum FeatureFlag
    {
        ReportingStepByStep
    }
}
