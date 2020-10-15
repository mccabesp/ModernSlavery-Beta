namespace ModernSlavery.WebUI.Shared.Models
{
    public class CookieSettingsViewModel
    {
        public string GoogleAnalyticsMSU { get; set; }
        public string GoogleAnalyticsGovUk { get; set; }
        public string ApplicationInsights { get; set; }
        public string RememberSettings { get; set; }

        public bool ChangesHaveBeenSaved { get; set; }
    }
}