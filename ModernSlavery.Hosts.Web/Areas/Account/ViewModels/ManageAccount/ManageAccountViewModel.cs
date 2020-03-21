using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.WebUI.Areas.Account.Resources;
using ModernSlavery.WebUI.Shared.Classes;

namespace ModernSlavery.WebUI.Areas.Account.ViewModels
{

    [Serializable]
    [DefaultResource(typeof(AccountResources))]
    public class ManageAccountViewModel
    {

        [Display(Name = nameof(EmailAddress))]
        public string EmailAddress { get; set; }

        [Display(Name = nameof(FirstName))]
        public string FirstName { get; set; }

        [Display(Name = nameof(LastName))]
        public string LastName { get; set; }

        [Display(Name = nameof(JobTitle))]
        public string JobTitle { get; set; }

        [Display(Name = nameof(ContactPhoneNumber))]
        public string ContactPhoneNumber { get; set; }

        [Display(Name = nameof(SendUpdates))]
        public bool SendUpdates { get; set; }

        [Display(Name = nameof(AllowContact))]
        public bool AllowContact { get; set; }

    }

}
