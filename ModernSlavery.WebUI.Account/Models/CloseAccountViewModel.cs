using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Resources;

namespace ModernSlavery.WebUI.Account.Models
{
    [Serializable]
    [DefaultResource(typeof(AccountResources))]
    public class CloseAccountViewModel
    {
        public bool IsSoleUserOfOneOrMoreOrganisations { get; set; }

        [DataType(DataType.Password)]
        [Required(AllowEmptyStrings = false,
            ErrorMessageResourceName = nameof(AccountResources.CloseAccountEnterPasswordRequired))]
        [Display(Name = nameof(AccountResources.CloseAccountEnterPassword))]
        public string EnterPassword { get; set; }
    }
}