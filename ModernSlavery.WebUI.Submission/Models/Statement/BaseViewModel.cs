using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class BaseViewModel
    {
        [BindNever]
        public string BackUrl { get; set; }
        [BindNever]
        public string CancelUrl { get; set; }
        [BindRequired]
        public string ContinueUrl { get; set; }
    }
}
