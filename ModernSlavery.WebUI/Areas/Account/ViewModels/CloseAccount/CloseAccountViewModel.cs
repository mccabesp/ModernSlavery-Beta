using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.WebUI.Areas.Account.Resources;
using ModernSlavery.WebUI.Classes;

namespace ModernSlavery.WebUI.Areas.Account.ViewModels
{

    [Serializable]
    [DefaultResource(typeof(AccountResources))]
    public class CloseAccountViewModel
    {

        public bool IsSoleUserOfOneOrMoreOrganisations { get; set; }

        [DataType(DataType.Password)]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(AccountResources.CloseAccountEnterPasswordRequired))]
        [Display(Name = nameof(AccountResources.CloseAccountEnterPassword))]
        public string EnterPassword { get; set; }

    }

}
