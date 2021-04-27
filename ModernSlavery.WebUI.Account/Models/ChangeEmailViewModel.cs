using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Resources;

namespace ModernSlavery.WebUI.Account.Models
{
    [Serializable]
    [DefaultResource(typeof(AccountResources))]
    public class ChangeEmailViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(AccountResources.EmailAddressRequired))]
        [EmailAddress]
        [MaxLength(254, ErrorMessageResourceName = nameof(AccountResources.EmailAddressLength))]
        [Display(Name = nameof(EmailAddress))]
        public string EmailAddress { get; set; }

        [Required(AllowEmptyStrings = false,ErrorMessageResourceName = nameof(AccountResources.ConfirmEmailAddressRequired))]
        [Compare(nameof(EmailAddress), ErrorMessageResourceName = nameof(AccountResources.ConfirmEmailAddressCompare))]
        [Display(Name = nameof(ConfirmEmailAddress))]
        [EmailAddress]
        public string ConfirmEmailAddress { get; set; }
    }
}