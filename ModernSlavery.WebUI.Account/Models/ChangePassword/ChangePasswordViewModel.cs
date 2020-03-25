using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Resources;

namespace ModernSlavery.WebUI.Account.Models.ChangePassword
{

    [Serializable]
    [DefaultResource(typeof(AccountResources))]
    public class ChangePasswordViewModel
    {

        [DataType(DataType.Password)]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(AccountResources.CurrentPasswordRequired))]
        [Display(Name = nameof(CurrentPassword))]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(AccountResources.NewPasswordRequired))]
        [StringLength(100, MinimumLength = 8, ErrorMessageResourceName = nameof(AccountResources.PasswordLength))]
        [Display(Name = nameof(NewPassword))]
        [Password]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(AccountResources.ConfirmNewPasswordRequired))]
        [Display(Name = nameof(ConfirmNewPassword))]
        [Compare(nameof(NewPassword), ErrorMessageResourceName = nameof(AccountResources.ConfirmPasswordCompare))]
        public string ConfirmNewPassword { get; set; }

    }

}
