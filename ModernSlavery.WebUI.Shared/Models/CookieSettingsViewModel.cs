using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Shared.Models
{
    public class CookieSettingsViewModel
    {
        [Text("onfONF")]
        public string GoogleAnalyticsMSU { get; set; }
        [Text("onfONF")] 
        public string GoogleAnalyticsGovUk { get; set; }
        [Text("onfONF")] 
        public string ApplicationInsights { get; set; }
        [Text("onfONF")] 
        public string RememberSettings { get; set; }

        public bool ChangesHaveBeenSaved { get; set; }
    }
}